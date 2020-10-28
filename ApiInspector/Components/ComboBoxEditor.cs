using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using static ApiInspector.Components.EditorProp;

namespace ApiInspector.Components
{
    /// <summary>
    ///     The ComboBox editor
    /// </summary>
    static class ComboBoxEditor
    {
        #region Public Methods
        /// <summary>
        ///     Creates the ComboBox.
        /// </summary>
        public static ComboBox CreateComboBox(Scope scope)
        {
            return InitializeComboBox(scope, new ComboBox());
        }

        /// <summary>
        ///     Creates the ComboBox with label.
        /// </summary>
        public static Panel CreateComboBoxWithLabel(Scope scope)
        {
            var label = LabelCreator.CreateLabel(scope);

            var comboBox = CreateComboBox(scope);

            var sp = new StackPanel();

            sp.Children.Add(label);
            sp.Children.Add(comboBox);

            return sp;
        }

        /// <summary>
        ///     Initializes the ComboBox.
        /// </summary>
        public static ComboBox InitializeComboBox(Scope scope, ComboBox comboBox)
        {
            var dataContext = scope.Get(DataContext);
            var bindingPath = scope.Get(BindingPath);
            var itemsSource = scope.Get(ItemsSource);

            var displayMemberPath = scope.TryGet(DisplayMemberPath);
            var valueMemberPath   = scope.TryGet(ValueMemberPath);

            comboBox.DataContext       = dataContext;
            comboBox.ItemsSource       = itemsSource;
            comboBox.IsEditable        = true;
            comboBox.DisplayMemberPath = displayMemberPath;
            comboBox.SelectedValuePath = valueMemberPath;

            comboBox.Loaded += delegate
            {
                var textBox = (TextBox) comboBox.Template.FindName("PART_EditableTextBox", comboBox);
                var popup   = (Popup) comboBox.Template.FindName("PART_Popup", comboBox);

                textBox.TextChanged += delegate
                {
                    comboBox.Items.Filter += item =>
                    {
                        if (item == null)
                        {
                            return true;
                        }

                        var text = string.Empty;

                        if (displayMemberPath != null)
                        {
                            text = item.GetType().GetProperty(displayMemberPath)?.GetValue(item) + string.Empty;
                        }
                        else
                        {
                            text = item + string.Empty;
                        }

                        if (text.IndexOf(textBox.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            popup.IsOpen = true;
                            return true;
                        }

                        return false;
                    };
                };
            };

            comboBox.KeyDown += (sender, e) =>
            {
                if (e.Key == Key.Down)
                {
                    e.Handled = true;
                    comboBox.Items.MoveCurrentToNext();
                }
            };

            BindingOperations.SetBinding(comboBox, Selector.SelectedValueProperty, new Binding
            {
                Source              = dataContext,
                Path                = new PropertyPath(bindingPath),
                Mode                = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            return comboBox;
        }
        #endregion
    }
}