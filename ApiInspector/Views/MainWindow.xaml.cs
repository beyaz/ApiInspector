using System.Collections.Generic;
using System.Windows;
using ApiInspector.Application;
using ApiInspector.Components;
using ApiInspector.DataAccess;

namespace ApiInspector.Views
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        BOA.DataFlow.DataContext context;

        void InitializeContext()
        {
            var builder = new InvocationInfoEditorContextBuilder();

            context = builder.Build();
        }

        #region Constructors
        public MainWindow()
        {
            InitializeComponent();

            InitializeContext();

            currentInvocationInfo.Context = context;

            Loaded += OnLoad;

        }
        #endregion
        void OnLoad(object sender, RoutedEventArgs routedEventArgs)
        {
            environmentIntellisenseTextBox.Editor.TextChanged += (s, e) =>
            {
                context.Update(DataKeys.TargetEnvironment, environmentIntellisenseTextBox.Editor.Text);
            };
            
        }


    }
}