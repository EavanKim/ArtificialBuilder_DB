using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using ArtificialBuilder.Models;
using ArtificialBuilder_EDP;
using ArtificialBuilder_EDP.Core.UI.Window;

namespace ArtificialBuilder
{
    /// <summary>
    /// AB_Window 의 DB 표현(AB_Response_Window_Model + AB_Window_Component_Model 묶음)을
    /// Circuit 정의(Resources/Circuits/{circuitType}.json) 에서 생성하는 헬퍼.
    /// Circuit 정의는 ResourceBuilder 가 resources.abr 의 "circuit" 카테고리로 패킹합니다.
    /// </summary>
    public static class AB_Window_Component_Seeding
    {
        /// <summary>ApplyCircuitDefAsync 반환 — 생성된 윈도우 + canvas envelope + primary chat id.</summary>
        public class Apply_Result
        {
            public List<AB_Response_Window_Model> Windows { get; set; } = new();
            public string EnvelopeJson { get; set; } = "";
            public string? PrimaryChatWindowId { get; set; }
        }

        /// <summary>
        /// window_components 가 비어 있는 윈도우에 Frame/Layout/Depth 공통 3 만 채웁니다.
        /// 성격 컴포넌트 (message/image/3d) 는 사용자가 속성 패널에서 별도 장전 — 빈 윈도우 원칙.
        /// 멱등 — 이미 컴포넌트가 있는 윈도우는 건드리지 않습니다.
        /// </summary>
        public static async Task EnsureDefaultComponentsAsync()
        {
            List<AB_Response_Window_Model> windows = await AB_Circuit_Db_Proxy.I.GetAllWindowsAsync();
            if (windows.Count == 0) return;

            List<AB_Window_Component_Model> allComps = await AB_Circuit_Db_Proxy.I.GetAllWindowComponentsAsync();
            var hasComps = new HashSet<string>();
            foreach (var c in allComps) hasComps.Add(c.WindowId_);

            int seeded = 0;
            foreach (var w in windows)
            {
                if (hasComps.Contains(w.Id_)) continue;
                await AddFrameLayoutDepthAsync(w.Id_, w.Name_, w.Position_ ?? "center", w.SortOrder_);
                seeded++;
            }

            if (seeded > 0)
                AB_Log.Info("WinSeed", $"Frame/Layout/Depth 공통 3 시드: {seeded} 개 윈도우 (빈 윈도우)");
        }

        /// <summary>
        /// envelope 과 DB 윈도우 집합을 대조 — 적어도 한 개라도 매칭되면 true. 전부 mismatch 또는 envelope null/invalid 이면 false.
        /// false 반환 시 호출측은 envelope 재조립 (예: ApplyCircuitDefAsync 재호출) 을 해야 함.
        /// 순수 함수 — 테스트 가능, DB 접근 없음.
        /// </summary>
        public static bool EnvelopeMatchesDb(string? _envelopeJson, IEnumerable<string> _dbWindowIds)
        {
            if (string.IsNullOrEmpty(_envelopeJson)) return false;
            Window_Layout_Envelope? env;
            try { env = JsonSerializer.Deserialize<Window_Layout_Envelope>(_envelopeJson); }
            catch { return false; }
            if (env?.Windows == null || env.Windows.Count == 0) return false;
            var idSet = new HashSet<string>(_dbWindowIds);
            foreach (Window_Layout_Data wd in env.Windows)
            {
                if (!string.IsNullOrEmpty(wd.TemplateId) && idSet.Contains(wd.TemplateId))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Circuit 정의(AB_Circuit_Def) 를 읽어 windows + window_components + canvas envelope 를 한 번에 조립합니다.
        /// 같은 이름의 윈도우가 이미 있으면 재사용 (중복 생성 방지).
        /// _reuseIdByName 이 주어지면 DB 에서 이름 매칭 실패 시 해당 GUID 를 그대로 재사용 — envelope 에 남아있는
        /// 과거 GUID 를 유지해 context_records.DisplayTargets dead reference 방지. null 이면 기존 동작 (Guid.NewGuid()).
        /// </summary>
        public static async Task<Apply_Result> ApplyCircuitDefAsync(string _circuitType, IReadOnlyDictionary<string, string>? _reuseIdByName = null)
        {
            var result = new Apply_Result();

            AB_Circuit_Def? circuitDef = AB_Resource_Loader.I.LoadCircuitDef(_circuitType);
            if (circuitDef == null)
            {
                AB_Log.Warn("WinSeed", $"Circuit 정의 없음: {_circuitType} — 빈 결과 반환");
                return result;
            }

            var envelopeWindows = new List<Window_Layout_Data>();
            foreach (var entry in circuitDef.Windows)
            {
                AB_Response_Window_Model? window = await FindWindowByNameAsync(entry.Name);
                if (window == null)
                {
                    window = new AB_Response_Window_Model
                    {
                        Name_ = entry.Name,
                        Position_ = entry.Position,
                        Enabled_ = true,
                        SortOrder_ = entry.SortOrder
                    };
                    // envelope 에 과거 GUID 가 있으면 재사용 — response_windows 재시드여도 records 참조 유효.
                    if (_reuseIdByName != null && _reuseIdByName.TryGetValue(entry.Name, out string? reuseId) && !string.IsNullOrEmpty(reuseId))
                    {
                        window.Id_ = reuseId;
                        AB_Log.Info("WinSeed", $"ApplyCircuitDef: {entry.Name} 과거 GUID 재사용 {reuseId[..Math.Min(8, reuseId.Length)]}");
                    }
                    bool added = await AB_Circuit_Db_Proxy.I.AddWindowAsync(window);
                    if (!added)
                    {
                        AB_Log.Warn("WinSeed", $"ApplyCircuitDef: {entry.Name} 윈도우 생성 실패 (DB 미준비).");
                        continue;
                    }

                    await AddFrameLayoutDepthAsync(window.Id_, entry.Name, entry.Position, entry.SortOrder, entry.Layout);
                    int sortIdx = 0;
                    foreach (var compEntry in entry.Components)
                    {
                        string configJson = SerializeComponentConfig(compEntry);
                        await AB_Circuit_Db_Proxy.I.AddWindowComponentAsync(new AB_Window_Component_Model
                        {
                            WindowId_ = window.Id_,
                            ComponentType_ = compEntry.Type,
                            SortOrder_ = sortIdx++,
                            ConfigJson_ = configJson
                        });
                    }
                }

                result.Windows.Add(window);
                envelopeWindows.Add(new Window_Layout_Data
                {
                    TemplateId = window.Id_,
                    Name = window.Name_,
                    RatioX = entry.Layout.RatioX,
                    RatioY = entry.Layout.RatioY,
                    RatioW = entry.Layout.RatioW,
                    RatioH = entry.Layout.RatioH
                });

                if (result.PrimaryChatWindowId == null
                    && !string.IsNullOrEmpty(circuitDef.PrimaryChatWindowName)
                    && entry.Name == circuitDef.PrimaryChatWindowName)
                {
                    result.PrimaryChatWindowId = window.Id_;
                }
            }

            // primaryChatWindowName 미매칭 시 첫 message 컴포넌트 붙은 윈도우로 fallback
            if (result.PrimaryChatWindowId == null)
            {
                foreach (var entry in circuitDef.Windows)
                {
                    bool hasMessage = false;
                    foreach (var c in entry.Components) { if (c.Type == "message") { hasMessage = true; break; } }
                    if (!hasMessage) continue;
                    foreach (var w in result.Windows)
                    {
                        if (w.Name_ == entry.Name) { result.PrimaryChatWindowId = w.Id_; break; }
                    }
                    if (result.PrimaryChatWindowId != null) break;
                }
            }

            var envelope = new Window_Layout_Envelope
            {
                CanvasWidth = 1000,
                CanvasHeight = 700,
                Windows = envelopeWindows
            };
            result.EnvelopeJson = JsonSerializer.Serialize(envelope);
            return result;
        }

        /// <summary>
        /// 현재 DB 의 windows + window_components + canvas envelope + primary chat 를 AB_Circuit_Def JSON 으로 직렬화.
        /// Export 버튼용. 기본 시드 JSON 과 동일 스키마라 그대로 Import 가능.
        /// </summary>
        public static async Task<string> ExportCurrentToJsonAsync(string _circuitType)
        {
            var circuitDef = new AB_Circuit_Def { CircuitType = _circuitType };

            List<AB_Response_Window_Model> windows = await AB_Circuit_Db_Proxy.I.GetAllWindowsAsync();
            List<AB_Window_Component_Model> comps = await AB_Circuit_Db_Proxy.I.GetAllWindowComponentsAsync();

            // envelope (캔버스 비율 배치) 는 settings.WindowLayout_ 에 저장되어 있음
            Window_Layout_Envelope? envelope = null;
            var settingsR = await AB_Circuit_Db_Proxy.I.GetSettingsAsync();
            if (settingsR.IsOk && !string.IsNullOrEmpty(settingsR.Data.WindowLayout_))
            {
                try { envelope = JsonSerializer.Deserialize<Window_Layout_Envelope>(settingsR.Data.WindowLayout_); }
                catch (Exception ex)
                {
                    AB_Log.Warn("WinSeed", $"Export: envelope 파싱 실패 — {ex.Message}");
                    envelope = null;
                }
            }

            // primary chat 윈도우 이름 해석
            if (settingsR.IsOk && !string.IsNullOrEmpty(settingsR.Data.PrimaryChatWindowId_))
            {
                string pid = settingsR.Data.PrimaryChatWindowId_!;
                foreach (AB_Response_Window_Model w in windows)
                {
                    if (w.Id_ == pid) { circuitDef.PrimaryChatWindowName = w.Name_; break; }
                }
            }

            // windowId → components 그룹핑
            Dictionary<string, List<AB_Window_Component_Model>> compsByWindow = new();
            foreach (AB_Window_Component_Model c in comps)
            {
                if (!compsByWindow.TryGetValue(c.WindowId_, out var list))
                {
                    list = new List<AB_Window_Component_Model>();
                    compsByWindow[c.WindowId_] = list;
                }
                list.Add(c);
            }

            foreach (AB_Response_Window_Model w in windows)
            {
                AB_Circuit_Def_Window entry = new()
                {
                    Name = w.Name_,
                    Position = w.Position_ ?? "center",
                    SortOrder = w.SortOrder_,
                };

                if (envelope?.Windows != null)
                {
                    foreach (Window_Layout_Data wd in envelope.Windows)
                    {
                        if (wd.TemplateId == w.Id_)
                        {
                            entry.Layout = new AB_Circuit_Def_Layout
                            {
                                RatioX = wd.RatioX,
                                RatioY = wd.RatioY,
                                RatioW = wd.RatioW,
                                RatioH = wd.RatioH,
                            };
                            break;
                        }
                    }
                }

                // 성격 컴포넌트만 (frame/layout/depth 자동 부착이므로 제외)
                if (compsByWindow.TryGetValue(w.Id_, out var list))
                {
                    foreach (AB_Window_Component_Model c in list)
                    {
                        string type = c.ComponentType_;
                        if (type == "frame" || type == "layout" || type == "depth") continue;
                        JsonElement? configEl = null;
                        if (!string.IsNullOrEmpty(c.ConfigJson_))
                        {
                            try { configEl = JsonSerializer.Deserialize<JsonElement>(c.ConfigJson_); }
                            catch (Exception ex)
                            {
                                AB_Log.Warn("WinSeed", $"Export: component config 파싱 실패 — {ex.Message}");
                                configEl = null;
                            }
                        }
                        entry.Components.Add(new AB_Circuit_Def_Component { Type = type, Config = configEl });
                    }
                }

                circuitDef.Windows.Add(entry);
            }

            JsonSerializerOptions opts = new() { WriteIndented = true };
            return JsonSerializer.Serialize(circuitDef, opts);
        }

        /// <summary>
        /// 외부 AB_Circuit_Def 로 현재 DB 의 windows + window_components 를 완전 교체하고 envelope/primary chat 을 settings 에 반영.
        /// Import 버튼 및 Reset 확장용. ApplyCircuitDefAsync 의 "이름 중복 재사용" 멱등성과 달리, 본 메서드는 기존 윈도우를 먼저 삭제한다.
        /// </summary>
        public static async Task ReplaceWithCircuitDefAsync(AB_Circuit_Def _circuitDef)
        {
            List<AB_Response_Window_Model> existing = await AB_Circuit_Db_Proxy.I.GetAllWindowsAsync();
            foreach (AB_Response_Window_Model w in existing)
            {
                await AB_Circuit_Db_Proxy.I.DeleteWindowComponentsByWindowAsync(w.Id_);
                await AB_Circuit_Db_Proxy.I.DeleteWindowAsync(w.Id_);
            }
            AB_Log.Info("WinSeed", $"Replace: 기존 윈도우 {existing.Count} 개 삭제 완료");

            List<Window_Layout_Data> envelopeWindows = new();
            Dictionary<string, string> nameToId = new();

            foreach (AB_Circuit_Def_Window entry in _circuitDef.Windows)
            {
                AB_Response_Window_Model window = new()
                {
                    Name_ = entry.Name,
                    Position_ = entry.Position,
                    Enabled_ = true,
                    SortOrder_ = entry.SortOrder,
                };
                bool added = await AB_Circuit_Db_Proxy.I.AddWindowAsync(window);
                if (!added)
                {
                    AB_Log.Warn("WinSeed", $"Replace: {entry.Name} 윈도우 생성 실패 (DB 미준비)");
                    continue;
                }
                nameToId[entry.Name] = window.Id_;

                await AddFrameLayoutDepthAsync(window.Id_, entry.Name, entry.Position, entry.SortOrder, entry.Layout);
                int sortIdx = 0;
                foreach (AB_Circuit_Def_Component compEntry in entry.Components)
                {
                    string configJson = SerializeComponentConfig(compEntry);
                    await AB_Circuit_Db_Proxy.I.AddWindowComponentAsync(new AB_Window_Component_Model
                    {
                        WindowId_ = window.Id_,
                        ComponentType_ = compEntry.Type,
                        SortOrder_ = sortIdx++,
                        ConfigJson_ = configJson,
                    });
                }

                envelopeWindows.Add(new Window_Layout_Data
                {
                    TemplateId = window.Id_,
                    Name = window.Name_,
                    RatioX = entry.Layout.RatioX,
                    RatioY = entry.Layout.RatioY,
                    RatioW = entry.Layout.RatioW,
                    RatioH = entry.Layout.RatioH,
                });
            }

            // envelope + primary chat 을 settings 에 반영
            var settingsR = await AB_Circuit_Db_Proxy.I.GetSettingsAsync();
            if (!settingsR.IsOk)
            {
                AB_Log.Warn("WinSeed", "Replace: settings 없음 — envelope/primary 저장 스킵");
                return;
            }
            AB_Circuit_Settings_Model settings = settingsR.Data;
            Window_Layout_Envelope envelope = new()
            {
                CanvasWidth = 1000,
                CanvasHeight = 700,
                Windows = envelopeWindows,
            };
            settings.WindowLayout_ = JsonSerializer.Serialize(envelope);
            if (!string.IsNullOrEmpty(_circuitDef.PrimaryChatWindowName)
                && nameToId.TryGetValue(_circuitDef.PrimaryChatWindowName!, out var pid))
            {
                settings.PrimaryChatWindowId_ = pid;
            }
            settings.UpdatedAt_ = DateTime.UtcNow;
            await AB_Circuit_Db_Proxy.I.SaveSettingsAsync(settings);
            AB_Log.Info("WinSeed", $"Replace 완료 — 새 윈도우 {envelopeWindows.Count} 개, primaryChat={_circuitDef.PrimaryChatWindowName ?? "(미지정)"}");
        }

        /// <summary>
        /// Frame/Layout/Depth 공통 3 컴포넌트 DB 시드. _layout 이 있으면 Ratio 를 함께 저장 — circuit def 비율 좌표를
        /// Layout_Config 에 반영. null 이면 Position 기반 도킹 Circuit 기본 ratio 를 채운다 (레거시/단순 경로).
        /// </summary>
        private static async Task AddFrameLayoutDepthAsync(string _windowId, string _title, string _position, int _sortOrder, AB_Circuit_Def_Layout? _layout = null)
        {
            await AB_Circuit_Db_Proxy.I.AddWindowComponentAsync(new AB_Window_Component_Model
            {
                WindowId_ = _windowId, ComponentType_ = "frame", SortOrder_ = 0,
                ConfigJson_ = JsonSerializer.Serialize(new Frame_Config { Title = _title })
            });
            Layout_Config layoutCfg = new Layout_Config { Position = _position, SortOrder = _sortOrder };
            if (_layout != null && _layout.RatioW > 0.001)
            {
                layoutCfg.RatioX = _layout.RatioX;
                layoutCfg.RatioY = _layout.RatioY;
                layoutCfg.RatioW = _layout.RatioW;
                layoutCfg.RatioH = _layout.RatioH;
            }
            else
            {
                FillDefaultRatios(layoutCfg, _position);
            }
            await AB_Circuit_Db_Proxy.I.AddWindowComponentAsync(new AB_Window_Component_Model
            {
                WindowId_ = _windowId, ComponentType_ = "layout", SortOrder_ = 0,
                ConfigJson_ = JsonSerializer.Serialize(layoutCfg)
            });
            await AB_Circuit_Db_Proxy.I.AddWindowComponentAsync(new AB_Window_Component_Model
            {
                WindowId_ = _windowId, ComponentType_ = "depth", SortOrder_ = 0,
                ConfigJson_ = JsonSerializer.Serialize(new Depth_Config())
            });
        }

        /// <summary>Position 기반 도킹 Circuit 기본 ratio — circuit def 에 layout 미지정 시 fallback.</summary>
        public static void FillDefaultRatios(Layout_Config _cfg, string _position)
        {
            if (_position == "top")
            {
                _cfg.RatioX = 0.15; _cfg.RatioY = 0.02; _cfg.RatioW = 0.70; _cfg.RatioH = 0.06;
            }
            else if (_position == "center")
            {
                _cfg.RatioX = 0.15; _cfg.RatioY = 0.10; _cfg.RatioW = 0.70; _cfg.RatioH = 0.76;
            }
            else if (_position == "bottom")
            {
                _cfg.RatioX = 0.15; _cfg.RatioY = 0.86; _cfg.RatioW = 0.70; _cfg.RatioH = 0.12;
            }
            else if (_position == "image_main")
            {
                _cfg.RatioX = 0.02; _cfg.RatioY = 0.02; _cfg.RatioW = 0.65; _cfg.RatioH = 0.84;
            }
            else // overlay / unknown
            {
                _cfg.RatioX = 0.10; _cfg.RatioY = 0.10; _cfg.RatioW = 0.30; _cfg.RatioH = 0.20;
            }
        }

        /// <summary>Circuit 정의 JSON 의 config (JsonElement) → 타입에 맞는 POCO 로 재직렬화. 누락 필드는 기본값 유지.</summary>
        private static string SerializeComponentConfig(AB_Circuit_Def_Component _compEntry)
        {
            switch (_compEntry.Type)
            {
                case "message":
                {
                    var cfg = new Message_Config();
                    if (_compEntry.Config.HasValue) cfg = ParseOrDefault<Message_Config>(_compEntry.Config.Value, cfg);
                    return JsonSerializer.Serialize(cfg);
                }
                case "image2d":
                {
                    var cfg = new Image2DConfig();
                    if (_compEntry.Config.HasValue) cfg = ParseOrDefault<Image2DConfig>(_compEntry.Config.Value, cfg);
                    return JsonSerializer.Serialize(cfg);
                }
                case "layered2d":
                {
                    var cfg = new Layered2DConfig();
                    if (_compEntry.Config.HasValue) cfg = ParseOrDefault<Layered2DConfig>(_compEntry.Config.Value, cfg);
                    return JsonSerializer.Serialize(cfg);
                }
                case "3d":
                {
                    var cfg = new Three_DConfig();
                    if (_compEntry.Config.HasValue) cfg = ParseOrDefault<Three_DConfig>(_compEntry.Config.Value, cfg);
                    return JsonSerializer.Serialize(cfg);
                }
                case "back":
                {
                    var cfg = new Back_Config();
                    if (_compEntry.Config.HasValue) cfg = ParseOrDefault<Back_Config>(_compEntry.Config.Value, cfg);
                    return JsonSerializer.Serialize(cfg);
                }
                default:
                    return _compEntry.Config.HasValue ? _compEntry.Config.Value.GetRawText() : "{}";
            }
        }

        private static T ParseOrDefault<T>(JsonElement _el, T _fallback) where T : class
        {
            try
            {
                T? parsed = JsonSerializer.Deserialize<T>(_el.GetRawText());
                if (parsed != null) return parsed;
                AB_Log.Warn("WinSeed", $"ParseOrDefault<{typeof(T).Name}>: null 반환 — 기본값 사용");
                return _fallback;
            }
            catch (System.Exception ex)
            {
                AB_Log.Warn("WinSeed", $"ParseOrDefault<{typeof(T).Name}> 실패: {ex.Message} — 기본값 사용");
                return _fallback;
            }
        }

        private static async Task<AB_Response_Window_Model?> FindWindowByNameAsync(string _name)
        {
            List<AB_Response_Window_Model> all = await AB_Circuit_Db_Proxy.I.GetAllWindowsAsync();
            foreach (var w in all)
            {
                if (w.Name_ == _name) return w;
            }
            return null;
        }

        /// <summary>Kind → window_components.ComponentType_ 태그.</summary>
        public static string KindToTag(AB_Window_Component_Kind _kind)
        {
            switch (_kind)
            {
                case AB_Window_Component_Kind.Frame:   return "frame";
                case AB_Window_Component_Kind.Layout:  return "layout";
                case AB_Window_Component_Kind.Depth:   return "depth";
                case AB_Window_Component_Kind.Message: return "message";
                case AB_Window_Component_Kind.Image2D:   return "image2d";
                case AB_Window_Component_Kind.Layered2D: return "layered2d";
                case AB_Window_Component_Kind.ThreeD:  return "3d";
                case AB_Window_Component_Kind.Back:    return "back";
                default: return "";
            }
        }

        /// <summary>Frame 컴포넌트 ConfigJson.</summary>
        public class Frame_Config
        {
            public string Title { get; set; } = "";
            public string? Background { get; set; }
            public string? BorderColor { get; set; }
            public string? TitleBarColor { get; set; }
            public double CornerRadius { get; set; } = 4.0;
            public double BorderThickness { get; set; } = 1.0;
            public bool ShowTitleBar { get; set; } = true;
        }

        /// <summary>Layout 컴포넌트 ConfigJson.</summary>
        public class Layout_Config
        {
            public string Position { get; set; } = "center";
            public double RatioX { get; set; }
            public double RatioY { get; set; }
            public double RatioW { get; set; }
            public double RatioH { get; set; }
            public bool UseDockCircuit { get; set; } = true;
            public int SortOrder { get; set; }
        }

        /// <summary>Depth 컴포넌트 ConfigJson.</summary>
        public class Depth_Config
        {
            public int ZIndex { get; set; }
            public double Opacity { get; set; } = 1.0;
            public bool Visible { get; set; } = true;
        }

        /// <summary>Message 컴포넌트 ConfigJson.</summary>
        public class Message_Config
        {
            public bool IsInput { get; set; }
            public bool Markdown { get; set; } = true;
            public bool AutoScroll { get; set; } = true;
        }

        /// <summary>Image2D 컴포넌트 ConfigJson — 단일 2D 이미지, 스택 없음.</summary>
        public class Image2DConfig
        {
            public string Source { get; set; } = "asset";
            public string Ref { get; set; } = "";
            public double Opacity { get; set; } = 1.0;
            public string Stretch { get; set; } = "uniform";
        }

        /// <summary>
        /// Layered2D 컴포넌트 ConfigJson — **스택 컨테이너** (윈도우당 1 개). 내부에 N 레이어.
        /// 입력 payload 형식: `{"layers":[{"src":"...","opacity":1.0,"stretch":"uniform"}, ...]}`.
        /// Opacity / Stretch 는 각 레이어가 지정하지 않았을 때의 컨테이너 기본값.
        /// LayerOverrides — 편집 UI 에서 저장한 레이어별 오프셋 / z-순서 오버라이드 (인덱스 기반, 없는 인덱스는 기본값).
        /// </summary>
        public class Layered2DConfig
        {
            public string Source { get; set; } = "asset";
            public string Ref { get; set; } = "";
            public double Opacity { get; set; } = 1.0;
            public string Stretch { get; set; } = "uniform";
            public List<Layer_Override> LayerOverrides { get; set; } = new();
        }

        /// <summary>
        /// Layered2D 레이어 편집 오버라이드 — 인덱스 기반.
        /// Index 는 payload layers[] 의 원본 인덱스. 새 turn 에 레이어 수가 다르면 범위 내 인덱스만 적용.
        /// OffsetX/Y — RenderTransform.TranslateTransform 으로 이미지 평행이동 (픽셀).
        /// ZOrder — Avalonia Canvas ZIndex 값. null 이면 원본 인덱스 사용.
        /// </summary>
        public class Layer_Override
        {
            public int Index { get; set; }
            public double OffsetX { get; set; }
            public double OffsetY { get; set; }
            public int? ZOrder { get; set; }
        }

        /// <summary>3D 컴포넌트 ConfigJson — mesh 전용 (Spine/Spriter 는 별도 레이어).</summary>
        public class Three_DConfig
        {
            public string ModelKind { get; set; } = "mesh";
            public string ModelRef { get; set; } = "";
            public string Animation { get; set; } = "";
            public bool Loop { get; set; } = true;
            public bool Transparent { get; set; } = true;
        }

        /// <summary>Back (네비 뒤로가기) 컴포넌트 ConfigJson.</summary>
        public class Back_Config
        {
            public string Target { get; set; } = "home";
            public string Glyph { get; set; } = "←";
        }
    }
}
