using System;
using ApiInspector.Application;
using ApiInspector.Invoking.BoaSystem;
using BOA.Common.Types;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static ApiInspector._;

namespace ApiInspector.Invoking
{
    [TestClass]
    public class BoaContextTests
    {
        #region Public Methods
        [TestMethod]
        public void Authenticate()
        {
            AttachBoaSystemAssemblyResolverToCurrentDomain();

            CallDev();
            CallTest();
            CallDev();
            CallTest();
        }
        #endregion

        #region Methods
        static BOAContext CreateBOAContext(EnvironmentInfo environmentInfo)
        {
            return new BOAContext(environmentInfo, Console.WriteLine);
        }

        void CallDev()
        {
            using (var boaContext = CreateBOAContext(EnvironmentInfo.Dev))
            {
                boaContext.Context.DBLayer.GetDBCommand(Databases.Boa, string.Empty);

                boaContext.Context.DBLayer.ConnectionList[0].Value.ConnectionString.IndexOf(@"srvdev\atlas", StringComparison.OrdinalIgnoreCase).Should().BeGreaterThan(0);
            }
        }

        void CallTest()
        {
            using (var boaContext = CreateBOAContext(EnvironmentInfo.Test))
            {
                boaContext.Context.DBLayer.GetDBCommand(Databases.Boa, string.Empty);

                boaContext.Context.DBLayer.ConnectionList[0].Value.ConnectionString.IndexOf(@"srvtest\atlas", StringComparison.OrdinalIgnoreCase).Should().BeGreaterThan(0);
            }
        }
        #endregion
    }
}