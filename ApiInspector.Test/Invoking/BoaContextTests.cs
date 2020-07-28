using ApiInspector.MainWindow;
using ApiInspector.Tracing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;

namespace ApiInspector.Invoking
{
    [TestClass]
    public class BoaContextTests
    {
        [TestMethod]
        public void Authenticate()
        {
            using (var injector = new Injector(new TraceQueue(),"Dev"))
            {
                var boaContext = injector.Get<BOAContext>();
                boaContext.Context.ToString();

            }
        }
    }
}