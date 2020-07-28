using ApiInspector.Invoking;
using ApiInspector.Tracing;
using Ninject;

namespace ApiInspector.MainWindow
{
    public class Injector : StandardKernel
    {
        #region Constructors
        public Injector(ITracer tracer, string environment)
        {
            var environmentInfo = EnvironmentInfo.Parse(environment);

            Bind<ITracer>().ToMethod(c => tracer);

            Bind<EnvironmentInfo>().ToMethod(c => environmentInfo);

            Bind<BOAContext>().ToSelf().InSingletonScope();
            Bind<BoaConfigurationFile>().ToSelf().InSingletonScope();
            
        }

        public override void Dispose(bool disposing)
        {
            new BoaDirectInvokeDisposer().Dispose();
            base.Dispose(disposing);
        }
        #endregion
    }
}