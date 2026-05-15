namespace ArtificialBuilder.DB.Component
{
    // 1 대상 = EF Core Find / Query 1 단위. 1 외부 종속 = DbContext 1.
    // Tick 안 = 칠판 read DataId → DbContext.Find/Query → 칠판 write 결과 → 메시지 큐 enqueue.
    // 외부 종속 본체 (DbContext holder) = Sub 3 attach 시점에 채움.
    public class AB_Component_Db_Read : AB_Component_Db
    {
        public AB_Component_Db_Read()
        {
        }

        public override void Tick(double _delta_sec)
        {
        }

        public override void Dispose()
        {
        }
    }
}
