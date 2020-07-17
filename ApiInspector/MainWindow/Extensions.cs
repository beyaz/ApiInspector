using System.Windows.Controls;
using System.Windows.Documents;

namespace ApiInspector.MainWindow
{
    /// <summary>
    ///     The extensions
    /// </summary>
    static class Extensions
    {
        #region Public Methods
        /// <summary>
        ///     Gets the text.
        /// </summary>
        public static string GetText(this RichTextBox richTextBox)
        {
            return new TextRange(richTextBox.Document.ContentStart,
                                 richTextBox.Document.ContentEnd).Text;
        }

        /// <summary>
        ///     Sets the text.
        /// </summary>
        public static void SetText(this RichTextBox richTextBox, string text)
        {
            richTextBox.Document.Blocks.Clear();
            richTextBox.Document.Blocks.Add(new Paragraph(new Run(text)));
        }
        #endregion
    }
}