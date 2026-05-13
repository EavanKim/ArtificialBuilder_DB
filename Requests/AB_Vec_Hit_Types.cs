namespace ArtificialBuilder.Requests
{
    /// <summary>Vec 검색 결과 — 로어/CData 공통 (id + 거리).</summary>
    public class AB_Vec_Hit : ArtificialBuilder_EDP.Core.AB_Object
    {
        /// <summary>대상 ID.</summary>
        public string Id = "";
        /// <summary>거리 (작을수록 가까움).</summary>
        public double Distance;

        public override void Reset()
        {
            base.Reset();
            Id = "";
            Distance = 0.0;
        }
    }

    /// <summary>채팅 임베딩 검색 결과. 키 튜플: (session_id, node_id, turn_index, refresh_index, emission_order). 2026-05-11 — node_id string → long.</summary>
    public class AB_Vec_Chat_Hit : ArtificialBuilder_EDP.Core.AB_Object
    {
        /// <summary>세션 ID.</summary>
        public long SessionId;
        /// <summary>소스 노드 ID.</summary>
        public long NodeId;
        /// <summary>턴 인덱스.</summary>
        public int TurnIndex;
        /// <summary>refresh 레이어 인덱스.</summary>
        public int RefreshIndex;
        /// <summary>같은 (turn, refresh, node) 내 발행 순서.</summary>
        public int EmissionOrder;
        /// <summary>거리.</summary>
        public double Distance;

        public override void Reset()
        {
            base.Reset();
            SessionId = 0L;
            NodeId = 0L;
            TurnIndex = 0;
            RefreshIndex = 0;
            EmissionOrder = 0;
            Distance = 0.0;
        }
    }

    /// <summary>세션 임베딩 메타 (컨텍스트 키 + 차원수). 2026-05-11 — node_id string → long.</summary>
    public class AB_Chat_Embedding_Info : ArtificialBuilder_EDP.Core.AB_Object
    {
        /// <summary>소스 노드 ID.</summary>
        public long NodeId;
        /// <summary>턴 인덱스.</summary>
        public int TurnIndex;
        /// <summary>refresh 레이어 인덱스.</summary>
        public int RefreshIndex;
        /// <summary>발행 순서.</summary>
        public int EmissionOrder;
        /// <summary>차원 수.</summary>
        public int Dimensions;

        public override void Reset()
        {
            base.Reset();
            NodeId = 0L;
            TurnIndex = 0;
            RefreshIndex = 0;
            EmissionOrder = 0;
            Dimensions = 0;
        }
    }
}
