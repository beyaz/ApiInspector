using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using FastColoredTextBoxNS;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using TextChangedEventArgs = FastColoredTextBoxNS.TextChangedEventArgs;

namespace ApiInspector.Components
{
    /// <summary>
    ///     The special text editor
    /// </summary>
    class SpecialTextEditor : Border
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


            BorderThickness = new Thickness(1);
            BorderBrush     = System.Windows.Media.Brushes.Aqua;

            Loaded += (s, e) =>
            {
                Child = new WindowsFormsHost
                {
                    Child = editor
                };
            };

            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment   = VerticalAlignment.Stretch;
            MinHeight           = 200;
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


        
        public void SetAutoComplete(ICollection<string> suggestions)
        {
            var popupMenu = new AutocompleteMenu(this.editor)
            {
                MinFragmentLength = 2,
                AppearInterval = 5000
            };
            popupMenu.Items.SetAutocompleteItems(suggestions);
            popupMenu.Items.MaximumSize = new System.Drawing.Size(200, 300);
            popupMenu.Items.Width       = 200;

            editor.KeyPressed += (s, e) =>
            {
                
                var isSpaceKey = e.KeyChar == '@';
                if (isSpaceKey)
                {
                    popupMenu.Show(true);
                    e.Handled = true;
                }
            };
        }

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