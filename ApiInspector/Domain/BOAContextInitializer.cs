using System;
using System.Collections.Generic;
using BOA.Base.Data;
using BOA.Common.Types;
using BOA.DataFlow;
using BOA.UnitTestHelper;

namespace ApiInspector.Domain
{
    static class BOAContextInitializer
    {
        #region Public Methods
        public static void Initialize(DataContext context)
        {
            var targetEnvironment = context.Get(Data.TargetEnvironment);

            ExecutionDataContext executionDataContext;
            if (targetEnvironment.IndexOf("dev", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                executionDataContext = new BOATestContextDev().objectHelper.Context;

                executionDataContext.DBLayer.ConnectionMock = new Dictionary<Databases, string>
                {
                    {Databases.BanksoftCC, @"Data Source=srvxdev\zumrut;Initial Catalog=KrediKuveyt;Min Pool Size=10; Max Pool Size=100;Application Name=BOAApp;Integrated Security=true;"}
                };
            }
            else if (targetEnvironment.IndexOf("test", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                executionDataContext = new BOATestContextTest().objectHelper.Context;

                //executionDataContext.DBLayer.ConnectionMock = new Dictionary<Databases, string>
                //{
                //    {Databases.BanksoftCC, @"Data Source=srvxtest\zumrut;Initial Catalog=KrediKuveyt;Min Pool Size=10; Max Pool Size=100;Application Name=BOAApp;Integrated Security=true;"}
                //};
            }
            else
            {
                throw new NotImplementedException(nameof(targetEnvironment));
            }

            context.Update(Data.ExecutionDataContext, executionDataContext);
        }
        #endregion
    }
}