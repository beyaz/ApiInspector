using ApiInspector.DataAccess;
using ApiInspector.DataFlow;
using ApiInspector.InvocationInfoEditor;
using BOA.DataFlow;

namespace ApiInspector.MainWindow
{
    public partial class View
    {
        #region Fields
        DataContext context;
        #endregion

        #region Constructors
        public View()
        {
            InitializeComponent();

            InitializeContext();

            currentInvocationInfo.Context = context;

        
        }
        #endregion

        #region Methods
        

        void InitializeContext()
        {
            var builder = new ContextBuilder();

            context = builder.Build();
        }
        #endregion
    }
}