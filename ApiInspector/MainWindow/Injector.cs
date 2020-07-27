using ApiInspector.Tracing;
using Ninject;

namespace ApiInspector.MainWindow
{
    public class Injector : StandardKernel
    {
        #region Constructors
        public Injector(ITracer tracer)
        {
            Bind<ITracer>().ToMethod(c => tracer);
        }
        #endregion
    }
}