using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WpfControls;

namespace ApiInspector.Components
{
    /// <summary>
    ///     The intellisense text box
    /// </summary>
    public sealed class IntellisenseTextBox : AutoCompleteTextBox, ISuggestionProvider
    {
        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="IntellisenseTextBox" /> class.
        /// </summary>
        public IntellisenseTextBox()
        {
            Provider = this;
        }
        #endregion

        #region Public Properties
        /// <summary>
        ///     Gets or sets the context.
        /// </summary>
        public IReadOnlyList<string> Suggestions { get; set; }
        #endregion

        #region Public Methods
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            
            Editor.Style = WPFExtensions.SearchInMergedDictionaries<Style>("TextBoxStyle");

            BorderThickness = new Thickness(0);

            // Popup boder radius
            {
                var border = (Border) Popup.Child;

                border.CornerRadius = new CornerRadius(1);
            }

            if (IsTextAlignmentCenter)
            {
                Editor.TextAlignment = TextAlignment.Center;
            }
        }

        public bool IsTextAlignmentCenter { get; set; }

        /// <summary>
        ///     Sets the value.
        /// </summary>
        public void SetValue(string value)
        {
            Editor.Text  = value;
            Popup.IsOpen = false;
        }
        #endregion

        #region Explicit Interface Methods
        /// <summary>
        ///     Gets the suggestions.
        /// </summary>
        IEnumerable ISuggestionProvider.GetSuggestions(string filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return null;
            }

            return Suggestions?.Where(x => x.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).Take(10);
        }
        #endregion
    }
}