using System;
using System.Collections.Generic;
using BOA.Base;
using BOA.Common.Types;
using BOA.DataFlow;
using BOA.Process.Kernel.Card;
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
        public static DataKey<ObjectHelper> BOAExecutionContext = new DataKey<ObjectHelper>(nameof(BOAExecutionContext));

        /// <summary>
        ///     The target environment
        /// </summary>
        public static DataKey<string> TargetEnvironment = new DataKey<string>(nameof(TargetEnvironment));
        #endregion

        public static string BOATransactionShouldCommit = nameof(BOATransactionShouldCommit);

        #region Public Methods
        /// <summary>
        ///     Initializes the specified context.
        /// </summary>
        public static void Initialize(DataContext context)
        {
            var targetEnvironment = context.Get(TargetEnvironment);

            var objectHelper = CreateObjectHelper(targetEnvironment);

            objectHelper.Context.DBLayer.BeginTransaction();

            CardService.UseLocalProxy = true;

            context.Update(BOAExecutionContext, objectHelper);

            context.SubscribeEvent(BOATransactionShouldCommit,()=> { Dispose(context); });
        }

        static void Dispose(DataContext context)
        {
            var objectHelper =  context.Get(BOAExecutionContext);

            objectHelper.Context.DBLayer.CommitTransaction();
        }
        
        #endregion

        #region Methods
        /// <summary>
        ///     Creates the object helper.
        /// </summary>
        static ObjectHelper CreateObjectHelper(string targetEnvironment)
        {
            ObjectHelper objectHelper = null;
            if (targetEnvironment.IndexOf("dev", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                objectHelper = new BOATestContextDev().objectHelper;

                //objectHelper.Context.DBLayer.ConnectionMock = new Dictionary<Databases, string>
                //{
                //    {Databases.BanksoftCC, @"Data Source=srvxdev\zumrut;Initial Catalog=KrediKuveyt;Min Pool Size=10; Max Pool Size=100;Application Name=BOAApp;Integrated Security=true;"}
                //};
            }
            else if (targetEnvironment.IndexOf("test", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                objectHelper = new BOATestContextTest().objectHelper;

                //objectHelper.Context.DBLayer.ConnectionMock = new Dictionary<Databases, string>
                //{
                //    {Databases.BOACard, @"Data Source=srvxtest\zumrut;Initial Catalog=BOACard2;Min Pool Size=10; Max Pool Size=100;Application Name=BOAApp;Integrated Security=true;"},
                //    {Databases.BanksoftCC, @"Data Source=srvxtest\zumrut;Initial Catalog=KrediKuveyt;Min Pool Size=10; Max Pool Size=100;Application Name=BOAApp;Integrated Security=true;"}
                //};
            }
            else
            {
                throw new NotImplementedException(nameof(targetEnvironment));
            }

            return objectHelper;
        }
        #endregion
    }
}