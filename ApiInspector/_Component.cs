using System.Collections.Generic;
using System.Windows.Controls;
using ApiInspector.Models;

namespace ApiInspector
{
    static partial class _
    {
        public static Control NewDropDownEditor(object bindingSource, string propertyPath, IReadOnlyList<string> itemsSource)
        {
            var editor = new ComboBox
            {
                ItemsSource         = itemsSource,
                IsTextSearchEnabled = true,
                IsEditable          = true,
                StaysOpenOnEdit     = true
            };

            WPFExtensions.Bind(editor, ComboBox.TextProperty, bindingSource, propertyPath);

            return editor;
        }
    }
}