using System;
using System.Collections.Generic;
using BOA.Base.Data;
using BOA.Common.Types;
using BOA.DataFlow;
using BOA.UnitTestHelper;

namespace ApiInspector.Invoking
{
    /// <summary>
    ///     The boa context initializer
    /// </summary>
    static class BOAContextInitializer
    {
        #region Static Fields
        /// <summary>
        ///     The boa execution context
        /// </summary>
        public static DataKey<ExecutionDataContext> BOAExecutionContext = new DataKey<ExecutionDataContext>(nameof(BOAExecutionContext));

        /// <summary>
        ///     The target environment
        /// </summary>
        public static DataKey<string> TargetEnvironment = new DataKey<string>(nameof(TargetEnvironment));
        #endregion

        #region Public Methods
        /// <summary>
        ///     Initializes the specified context.
        /// </summary>
        public static void Initialize(DataContext context)
        {
            var targetEnvironment = context.Get(TargetEnvironment);

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

                executionDataContext.DBLayer.ConnectionMock = new Dictionary<Databases, string>
                {
                    {Databases.BanksoftCC, @"Data Source=srvxtest\zumrut;Initial Catalog=KrediKuveyt;Min Pool Size=10; Max Pool Size=100;Application Name=BOAApp;Integrated Security=true;"}
                };
            }
            else
            {
                throw new NotImplementedException(nameof(targetEnvironment));
            }

            context.Update(BOAExecutionContext, executionDataContext);
        }
        #endregion
    }
}