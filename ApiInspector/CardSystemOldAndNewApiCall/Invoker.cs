using System;
using System.Collections.Generic;
using System.IO;
using BOA.Base.Data;
using BOA.Common.Types;
using BOA.DataFlow;
using BOA.Process.Kernel.Card;
using BOA.UnitTestHelper;

namespace ApiInspector.CardSystemOldAndNewApiCall
{
    
    static class Invoker
    {
        #region Public Methods

        public static DataKey<bool> IsDevEnvironment = new DataKey<bool>(nameof(IsDevEnvironment));
        public static DataKey<bool> IsTestEnvironment = new DataKey<bool>(nameof(IsDevEnvironment));
        public static DataKey<object> ClassInstanceUnderTest = new DataKey<object>(nameof(ClassInstanceUnderTest));

        /// <summary>
        ///     Runs this instance.
        /// </summary>
        public static void Run(DataContext context)
        {


            var testInstance = context.Get(ClassInstanceUnderTest);


            ExecutionDataContext executionDataContext;

            
            if (context.Get(IsDevEnvironment))
            {
                executionDataContext = new BOATestContextDev().objectHelper.Context;

                executionDataContext.DBLayer.ConnectionMock = new Dictionary<Databases, string>
                {
                    {Databases.BanksoftCC, @"Data Source=srvxdev\zumrut;Initial Catalog=KrediKuveyt;Min Pool Size=10; Max Pool Size=100;Application Name=BOAApp;Integrated Security=true;"}
                };
            }
            else if (context.Get(IsTestEnvironment))
            {
                executionDataContext = new BOATestContextTest().objectHelper.Context;

                //executionDataContext.DBLayer.ConnectionMock = new Dictionary<Databases, string>
                //{
                //    {Databases.BanksoftCC, @"Data Source=srvxtest\zumrut;Initial Catalog=KrediKuveyt;Min Pool Size=10; Max Pool Size=100;Application Name=BOAApp;Integrated Security=true;"}
                //};
            }
            else
            {
                throw new NotImplementedException("environmentType");
            }

            executionDataContext.DBLayer.BeginTransaction();

            CardService.UseLocalProxy = true;
            
            Invoking.Invoker.Invoke(context);

            // TODO: invoke old and new api

            executionDataContext.DBLayer.CommitTransaction();

            ResultSerializer.Serialize(context);
        }
        #endregion

    }
}