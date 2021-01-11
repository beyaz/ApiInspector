using System;
using System.Windows;
using System.Windows.Forms.Integration;
using FastColoredTextBoxNS;

namespace ApiInspector.Components
{
    /// <summary>
    ///     The special text editor
    /// </summary>
    class SpecialTextEditor : WindowsFormsHost
    {
        #region Fields
        /// <summary>
        ///     The editor
        /// </summary>
        protected readonly FastColoredTextBox editor = new FastColoredTextBox();
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="JsonTextEditor" /> class.
        /// </summary>
        public SpecialTextEditor()
        {
            Child               = editor;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment   = VerticalAlignment.Stretch;
            MinHeight           = 100;
        }
        #endregion

        #region Public Events
        /// <summary>
        ///     Occurs when [text changed].
        /// </summary>
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

    /// <summary>
    ///     The json text editor
    /// </summary>
    class JsonTextEditor : SpecialTextEditor
    {
        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="JsonTextEditor" /> class.
        /// </summary>
        public JsonTextEditor()
        {
            editor.Language = FastColoredTextBoxNS.Language.JSON;
        }
        #endregion
    }

    /// <summary>
    ///     The SQL text editor
    /// </summary>
    class SQLTextEditor : SpecialTextEditor
    {
        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="SQLTextEditor" /> class.
        /// </summary>
        public SQLTextEditor()
        {
            editor.Language = FastColoredTextBoxNS.Language.SQL;
        }
        #endregion
    }
}