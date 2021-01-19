using System.Windows;
using System.Windows.Media;

namespace ApiInspector.MainWindow
{
    /// <summary>
    ///     Interaction logic for ActionButton.xaml
    /// </summary>
    public partial class ActionButton
    {

        public void HideIcon()
        {
            IconVisibility = Visibility.Collapsed;
        }

        public void ShowSuccessIcon()
        {
            IconGeometry   = Icons.Success;
            IconVisibility = Visibility.Visible;
        }

        public void ShowExecuteIcon2()
        {
            IconGeometry   = Icons.Execute;
            IconVisibility = Visibility.Visible;
        }


        #region Visibility IconVisibility

        public static readonly DependencyProperty IconVisibilityProperty = DependencyProperty.Register(
                                                                                                       "IconVisibility", typeof(Visibility), typeof(ActionButton), new PropertyMetadata(Visibility.Collapsed));

        public Visibility IconVisibility
        {
            get { return (Visibility) GetValue(IconVisibilityProperty); }
            set { SetValue(IconVisibilityProperty, value); }
        }
        #endregion

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
                                                                                                        new PropertyMetadata(default(bool), OnShowExecuteIconChanged));

        static void OnShowExecuteIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var actionButton = (ActionButton) d;
            if ((bool) e.NewValue)
            {
                actionButton.IconGeometry   = Icons.Execute;
                actionButton.IconVisibility = Visibility.Visible;
            }
            else
            {
                actionButton.IconVisibility = Visibility.Collapsed;
            }
        }

        public bool ShowExecuteIcon
        {
            get { return (bool) GetValue(ShowExecuteIconProperty); }
            set { SetValue(ShowExecuteIconProperty, value); }
        }
        #endregion

        #region bool ShowSettingsIcon
        public static readonly DependencyProperty ShowSettingsIconProperty = DependencyProperty.Register(
                                                                                                         "ShowSettingsIcon", typeof(bool), typeof(ActionButton),
                                                                                                         new PropertyMetadata(default(bool), OnShowSettingsIconChanged));

        static void OnShowSettingsIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var actionButton = (ActionButton) d;
            if ((bool) e.NewValue)
            {
                actionButton.IconGeometry   = Icons.Settings;
                actionButton.IconVisibility = Visibility.Visible;
            }
            else
            {
                actionButton.IconVisibility = Visibility.Collapsed;
            }
        }

        public bool ShowSettingsIcon
        {
            get { return (bool) GetValue(ShowSettingsIconProperty); }
            set { SetValue(ShowSettingsIconProperty, value); }
        }
        #endregion

        public ActionButton()
        {
            InitializeComponent();

            IsPressed = false;
        }

        static LinearGradientBrush DefaultBackground
        {
            get
            {
                var brush = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint   = new Point(1, 1)
                };

                brush.GradientStops.Add(new GradientStop(Colors.White, 0));
                brush.GradientStops.Add(new GradientStop(Colors.AliceBlue, 1));

                return brush;
            }
        }

        static SolidColorBrush PressedBackground
        {
            get
            {
                // ReSharper disable once PossibleNullReferenceException
                return new SolidColorBrush((Color) ColorConverter.ConvertFromString("#bee6fd"));
            }
        }

        public bool IsPressed
        {
            set
            {
                if (value)
                {
                    button.Background = PressedBackground;
                }
                else
                {
                    button.Background = DefaultBackground;
                }
            }
        }
    }
}