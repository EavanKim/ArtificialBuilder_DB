namespace ArtificialBuilder
{
    /// <summary>DB 조회 상태.</summary>
    public enum Db_Status
    {
        /// <summary>미설정.</summary>
        None,
        /// <summary>정상 조회됨.</summary>
        Ok,
        /// <summary>대상 없음.</summary>
        NotFound,
        /// <summary>오류 발생.</summary>
        Error
    }

    /// <summary>
    /// DB 조회 결과 래퍼. null 대신 빈 객체 + 상태 enum.
    /// Data는 항상 유효한 객체 (NotFound/Error 시 기본값).
    /// </summary>
    public struct AB_Db_Result<T> where T : new()
    {
        /// <summary>조회 상태.</summary>
        private Db_Status m_status;
        public Db_Status Status
        {
            get { return m_status; }
            set { m_status = value; }
        }
        /// <summary>결과 데이터 (NotFound/Error 시 기본값).</summary>
        private T m_data;
        public T Data
        {
            get { return m_data; }
            set { m_data = value; }
        }

        /// <summary>Status==Ok 단축 체크.</summary>
        public bool IsOk => Status == Db_Status.Ok;

        /// <summary>성공 결과 생성.</summary>
        public static AB_Db_Result<T> Ok(T _data) => new() { Status = Db_Status.Ok, Data = _data };
        /// <summary>NotFound 결과 생성.</summary>
        public static AB_Db_Result<T> NotFound() => new() { Status = Db_Status.NotFound, Data = new T() };
        /// <summary>Error 결과 생성.</summary>
        public static AB_Db_Result<T> Err() => new() { Status = Db_Status.Error, Data = new T() };
    }
}
