using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ApiInspector.MainWindow
{
    /// <summary>
    /// Interaction logic for ActionButton.xaml
    /// </summary>
    public partial class ActionButton : UserControl
    {

        public static readonly DependencyProperty IconGeometryProperty = DependencyProperty.Register(
                                                        "IconGeometry", typeof(Geometry), typeof(ActionButton), new PropertyMetadata(default(Geometry)));

        public Geometry IconGeometry
        {
            get => (Geometry) GetValue(IconGeometryProperty);
            set => SetValue(IconGeometryProperty, value);
        }


        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(ActionButton), new PropertyMetadata(default(string)));

        public string Text
        {
            get => (string) GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public event RoutedEventHandler Click
        {
            add => button.Click += value;
            remove => button.Click += value;
        }


        #region bool ShowExecuteIcon
        public static readonly DependencyProperty ShowExecuteIconProperty = DependencyProperty.Register(
                                                        "ShowExecuteIcon", typeof(bool), typeof(ActionButton), 
                                                        new PropertyMetadata(default(bool),OnShowExecuteIconChanged));

        static void OnShowExecuteIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var actionButton = (ActionButton) d;
            if ((bool)e.NewValue == true)
            {
                actionButton.IconGeometry          = Geometry.Parse("M26.511,12.004L6.233,0.463c-2.151-1.228-4.344,0.115-4.344,2.53v24.093\r\n\t\tc0,2.046,1.332,2.979,2.57,2.979c0.583,0,1.177-0.184,1.767-0.543l20.369-12.468c1.024-0.629,1.599-1.56,1.581-2.555\r\n\t\tC28.159,13.503,27.553,12.593,26.511,12.004z M25.23,14.827L4.862,27.292c-0.137,0.084-0.245,0.126-0.319,0.147\r\n\t\tc-0.02-0.074-0.04-0.188-0.04-0.353V2.994c0-0.248,0.045-0.373,0.045-0.404c0.08,0.005,0.22,0.046,0.396,0.146l20.275,11.541\r\n\t\tc0.25,0.143,0.324,0.267,0.348,0.24C25.554,14.551,25.469,14.678,25.23,14.827z");
                actionButton.iconViewBox.Visibility = Visibility.Visible;
                
            }
            else
            {
                actionButton.iconViewBox.Visibility = Visibility.Collapsed;
            }
        }

        public bool ShowExecuteIcon
        {
            get { return (bool) GetValue(ShowExecuteIconProperty); }
            set { SetValue(ShowExecuteIconProperty, value); }
        }
        #endregion


        public ActionButton()
        {
            InitializeComponent();
            
            IsPressed =  false;

        }

        static LinearGradientBrush DefaultBackground
        {
            get
            {

                var brush = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0), 
                    EndPoint = new Point(1, 1)
                };

                brush.GradientStops.Add(new GradientStop(Colors.White,0));
                brush.GradientStops.Add(new GradientStop(Colors.AliceBlue,1));

                return brush;

            }
        }

        public bool IsPressed
        {
            set
            {
                if (value)
                {
                    button.Background = Brushes.LightSkyBlue;
                }
                else
                {
                    button.Background = DefaultBackground;
                }
            }
        }
    }
}
