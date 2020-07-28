using System;
using ApiInspector.MainWindow;
using ApiInspector.Tracing;
using BOA.Common.Types;
using BOA.UnitTestHelper;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;

namespace ApiInspector.Invoking
{
    [TestClass]
    public class BoaContextTests
    {
        #region Public Methods
        [TestMethod]
        public void Authenticate()
        {
            BOAAssemblyResolver.AttachToCurrentDomain();

            CallDev();
            CallTest();
            CallDev();
            CallTest();
        }
        #endregion

        #region Methods
        void CallDev()
        {
            using (var injector = new Injector(new TraceQueue(), EnvironmentInfo.Dev))
            {
                var boaContext = injector.Get<BOAContext>();

                boaContext.Context.DBLayer.GetDBCommand(Databases.Boa, string.Empty);

                boaContext.Context.DBLayer.ConnectionList[0].Value.ConnectionString.IndexOf(@"srvdev\atlas", StringComparison.OrdinalIgnoreCase).Should().BeGreaterThan(0);
            }
        }

        void CallTest()
        {
            using (var injector = new Injector(new TraceQueue(), EnvironmentInfo.Test))
            {
                var boaContext = injector.Get<BOAContext>();

                boaContext.Context.DBLayer.GetDBCommand(Databases.Boa, string.Empty);

                boaContext.Context.DBLayer.ConnectionList[0].Value.ConnectionString.IndexOf(@"srvtest\atlas", StringComparison.OrdinalIgnoreCase).Should().BeGreaterThan(0);
            }
        }
        #endregion
    }
}