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
       


        #region Constructors
        public MainWindow()
        {
            InitializeComponent();

            var builder = new InvocationInfoEditorContextBuilder();

            currentInvocationInfo.Context = builder.Build();


        }
        #endregion



    }
}