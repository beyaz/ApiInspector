using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using FastColoredTextBoxNS;
using HorizontalAlignment = System.Windows.HorizontalAlignment;

namespace ApiInspector.Components
{
    /// <summary>
    ///     The json text editor
    /// </summary>
    class JsonTextEditor : WindowsFormsHost
    {
        #region Static Fields
        /// <summary>
        ///     The text property
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(JsonTextEditor), new PropertyMetadata(default(string), OnTextChanged));
        #endregion

        #region Fields
        /// <summary>
        ///     The editor
        /// </summary>
        readonly FastColoredTextBox editor = new FastColoredTextBox
        {
            Language = FastColoredTextBoxNS.Language.JSON,
            Dock     = DockStyle.Fill
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

        #region Public Properties
        /// <summary>
        ///     Gets or sets the text.
        /// </summary>
        public string Text
        {
            get => (string) GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Called when [text changed].
        /// </summary>
        static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var jsonTextEditor = (JsonTextEditor) d;

            jsonTextEditor.editor.Text = (string) e.NewValue;
        }
        #endregion
    }
}