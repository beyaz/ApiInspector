using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ApiInspector
{
    /// <summary>
    ///     The WPF extensions
    /// </summary>
    static class WPFExtensions
    {
        #region Public Methods
        /// <summary>
        ///     Binds the specified target.
        /// </summary>
        public static void Bind(DependencyObject target, DependencyProperty dependencyProperty, object source, string propertyPath)
        {
            BindingOperations.SetBinding(target, dependencyProperty, new Binding
            {
                Source              = source,
                Path                = new PropertyPath(propertyPath),
                Mode                = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
        }

        /// <summary>
        ///     News the stack panel.
        /// </summary>
        public static StackPanel NewStackPanel(params UIElement[] childElements)
        {
            var sp = new StackPanel();

            foreach (var element in childElements)
            {
                sp.Children.Add(element);
            }

            return sp;
        }

        public static TextBlock NewTextBlock(string text,FontWeight fontWeight)
        {
            return new TextBlock {FontWeight = fontWeight, Text = text};
        }

        public static GroupBox NewGroupBox(UIElement header, UIElement content)
        {
            return new GroupBox
            {
                Header = header,
                Content = content
            };
        }

        #endregion
    }
}