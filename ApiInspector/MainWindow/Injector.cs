using ApiInspector.Invoking.BoaSystem;
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


        }
        #endregion

        #region Public Methods
        public override void Dispose(bool disposing)
        {
            new BoaDirectInvokeDisposer().Dispose();

            base.Dispose(disposing);
        }

        public T Get<T>()
        {
            return ResolutionExtensions.Get<T>(this);
        }
        #endregion
    }
}