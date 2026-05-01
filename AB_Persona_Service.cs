using ArtificialBuilder_EDP;
using ArtificialBuilder.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ArtificialBuilder
{
    /// <summary>
    /// 페르소나 비즈니스 로직 서비스.
    /// DB 접근만 수행하며 UI 의존성 없음.
    /// </summary>
    public class AB_Persona_Service : AB_Service
    {
        // --- 조회 ---

        /// <summary>모든 페르소나 이름 조회.</summary>
        public List<string> GetNames()
        {
            return AB_Board.Persona.GetPersonaNames();
        }

        /// <summary>현재 활성 페르소나 이름.</summary>
        public string? ActiveName => AB_Board.Persona.ActiveName;

        /// <summary>현재 페르소나 설정 로드.</summary>
        public async Task<AB_Db_Result<AB_Persona_Settings_Model>> GetSettingsAsync()
        {
            return await AB_Persona_Db_Proxy.I.GetSettingsAsync();
        }

        // --- 전환 ---

        /// <summary>페르소나 전환. Persona_Switched 이벤트 발행.</summary>
        public async Task SwitchAsync(string _name)
        {
            await AB_Board.Persona.CloseAsync();
            await AB_Board.Persona.OpenAsync(_name);
            Emit(new Persona_Switched { Name_ = _name });
        }

        // --- 생성 ---

        /// <summary>새 페르소나 생성 + 전환. Persona_List_Changed, Persona_Switched 발행.</summary>
        public async Task CreateAsync(string _name)
        {
            await AB_Board.Persona.OpenAsync(_name);
            AB_Persona_Settings_Model settings = new AB_Persona_Settings_Model();
            await AB_Persona_Db_Proxy.I.AddSettingsAsync(settings);

            Emit(new Persona_Switched { Name_ = _name });
            Emit(new Persona_List_Changed
            {
                Names_ = GetNames(),
                ActiveName_ = _name
            });
        }

        // --- 설정 저장 (+ 이름 변경) ---

        /// <summary>
        /// 페르소나 설정 저장. 이름 변경 시 파일 리네임.
        /// 이름이 변경되면 Persona_Switched 발행.
        /// </summary>
        public async Task SaveSettingsAsync(string _oldName, string _newName, string? _prompt)
        {
            var sR = await AB_Persona_Db_Proxy.I.GetSettingsAsync();
            AB_Persona_Settings_Model s;
            if (!sR.IsOk)
            {
                s = new AB_Persona_Settings_Model();
                s.Prompt_ = _prompt;
                s.UpdatedAt_ = DateTime.UtcNow;
                await AB_Persona_Db_Proxy.I.AddSettingsAsync(s);
            }
            else
            {
                s = sR.Data;
                s.Prompt_ = _prompt;
                s.UpdatedAt_ = DateTime.UtcNow;
                await AB_Persona_Db_Proxy.I.SaveSettingsAsync(s);
            }

            // 이름 변경 시 .psna 파일 리네임
            if (_oldName != _newName)
            {
                await AB_Board.Persona.CloseAsync();

                if (File.Exists($"persona/{_oldName}.psna"))
                {
                    File.Move($"persona/{_oldName}.psna", $"persona/{_newName}.psna", overwrite: false);
                }

                await AB_Board.Persona.OpenAsync(_newName);

                Emit(new Persona_Switched { Name_ = _newName });
                Emit(new Persona_List_Changed
                {
                    Names_ = GetNames(),
                    ActiveName_ = _newName
                });
            }
        }

        // --- 이름 변경 ---

        /// <summary>페르소나 파일명 변경 — DB 레이어의 RenameAsync 를 경유. UI 는 이 메서드만 호출.</summary>
        public async Task RenameAsync(string _oldName, string _newName)
        {
            await AB_Persona_Db_Proxy.I.RenameAsync(_oldName, _newName);
        }

        // --- 삭제 ---

        /// <summary>
        /// 페르소나 삭제. 마지막 한 개도 삭제 가능 — 0 으로 떨어지면 UI 가 첫 입력 화면으로 복귀.
        /// 삭제 후 남은 페르소나가 있으면 첫 번째로 자동 전환.
        /// 실제 파일/디렉터리 정리는 AB_Persona_Db_Gateway 의 Delete_Persona_Request 가 담당 (플랫/디렉터리 양쪽 처리).
        /// </summary>
        public async Task<bool> DeleteAsync(string _name)
        {
            var deleteR = await AB_Persona_Db_Proxy.I.DeleteAsync(_name);
            if (!deleteR.Success) return false;

            List<string> remaining = GetNames();
            if (remaining.Count > 0)
                Emit(new Persona_Switched { Name_ = AB_Board.Persona.ActiveName ?? remaining[0] });
            else
                Emit(new Persona_Switched { Name_ = "" });  // 빈 문자열 = 활성 페르소나 없음

            Emit(new Persona_List_Changed
            {
                Names_ = remaining,
                ActiveName_ = AB_Board.Persona.ActiveName
            });

            return true;
        }

        // --- 초기 설정 (첫 실행) ---

        /// <summary>첫 실행 시 페르소나 생성.</summary>
        public async Task SetupFirstPersonaAsync(string _name)
        {
            await CreateAsync(_name);
        }
    }
}
