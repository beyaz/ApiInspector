using ApiInspector.Invoking;
using ApiInspector.Tracing;
using Ninject;

namespace ApiInspector.MainWindow
{
    class Injector : StandardKernel
    {
        #region Constructors
        public Injector(ITracer tracer, EnvironmentInfo environmentInfo)
        {
            Bind<ITracer>().ToMethod(c => tracer);

            Bind<EnvironmentInfo>().ToMethod(c => environmentInfo);

            Bind<BOAContext>().ToSelf().InSingletonScope();

            Bind<BoaConfigurationFile>().ToSelf().InSingletonScope();
        }
        #endregion

        #region Public Methods
        public override void Dispose(bool disposing)
        {
            new BoaDirectInvokeDisposer().Dispose();

            base.Dispose(disposing);
        }
        #endregion
    }
}