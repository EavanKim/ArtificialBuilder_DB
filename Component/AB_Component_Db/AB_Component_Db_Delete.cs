namespace ArtificialBuilder.DB.Component
{
    // 1 대상 = EF Core Remove 1 단위. 1 외부 종속 = DbContext 1.
    public class AB_Component_Db_Delete : AB_Component_Db
    {
        public AB_Component_Db_Delete()
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
