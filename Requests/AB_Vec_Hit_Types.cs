namespace ArtificialBuilder.Requests
{
    /// <summary>Vec 검색 결과 — 로어/CData 공통 (id + 거리).</summary>
    public class AB_Vec_Hit
    {
        /// <summary>대상 ID.</summary>
        public string Id = "";
        /// <summary>거리 (작을수록 가까움).</summary>
        public double Distance;
    }

    /// <summary>채팅 임베딩 검색 결과. 키 튜플: (session_id, node_id, turn_index, refresh_index, emission_order).</summary>
    public class AB_Vec_Chat_Hit
    {
        /// <summary>세션 ID.</summary>
        public long SessionId;
        /// <summary>소스 노드 ID.</summary>
        public string NodeId = "";
        /// <summary>턴 인덱스.</summary>
        public int TurnIndex;
        /// <summary>refresh 레이어 인덱스.</summary>
        public int RefreshIndex;
        /// <summary>같은 (turn, refresh, node) 내 발행 순서.</summary>
        public int EmissionOrder;
        /// <summary>거리.</summary>
        public double Distance;
    }

    /// <summary>세션 임베딩 메타 (컨텍스트 키 + 차원수).</summary>
    public class AB_Chat_Embedding_Info
    {
        /// <summary>소스 노드 ID.</summary>
        public string NodeId = "";
        /// <summary>턴 인덱스.</summary>
        public int TurnIndex;
        /// <summary>refresh 레이어 인덱스.</summary>
        public int RefreshIndex;
        /// <summary>발행 순서.</summary>
        public int EmissionOrder;
        /// <summary>차원 수.</summary>
        public int Dimensions;
    }
}
