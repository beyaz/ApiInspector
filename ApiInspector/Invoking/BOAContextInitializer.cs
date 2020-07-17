using System;
using BOA.Base;
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

        public static string BOATransactionShouldCommit = nameof(BOATransactionShouldCommit);

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

            var objectHelper = CreateObjectHelper(targetEnvironment);

            objectHelper.Context.DBLayer.BeginTransaction();

            CardService.UseLocalProxy = true;

            context.Update(BOAExecutionContext, objectHelper);

            context.SubscribeEvent(BOATransactionShouldCommit, () => { Dispose(context); });
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

        static void Dispose(DataContext context)
        {

            ObjectHelper objectHelper = context.TryGet(BOAExecutionContext);
            if (objectHelper == null)
            {
                return;
            }



            objectHelper.Context.DBLayer.CommitTransaction();

            context.Remove(BOAExecutionContext);
        }
        #endregion
    }
}