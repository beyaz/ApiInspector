using System;
using System.Windows;
using System.Windows.Forms.Integration;
using FastColoredTextBoxNS;

namespace ApiInspector.Components
{
    /// <summary>
    ///     The json text editor
    /// </summary>
    class JsonTextEditor : WindowsFormsHost
    {
        #region Fields
        /// <summary>
        ///     The editor
        /// </summary>
        readonly FastColoredTextBox editor = new FastColoredTextBox
        {
            Language = FastColoredTextBoxNS.Language.JSON
        };
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="JsonTextEditor" /> class.
        /// </summary>
        public JsonTextEditor()
        {
            Child               = editor;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment   = VerticalAlignment.Stretch;
            MinHeight           = 100;
        }
        #endregion

        #region Public Events
        public event EventHandler<TextChangedEventArgs> TextChanged
        {
            add => editor.TextChanged += value;
            remove => editor.TextChanged -= value;
        }
        #endregion

        #region Public Properties
        /// <summary>
        ///     Gets or sets the text.
        /// </summary>
        public string Text
        {
            get => editor.Text;
            set => editor.Text = value;
        }
        #endregion
    }
}