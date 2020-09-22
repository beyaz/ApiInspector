using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

using WpfControls;

namespace ApiInspector.Components.KeyValueEditor
{
  public  class SuggestionHandler
    {
        public IReadOnlyList<CodeWithDescriptionContract> Handle(string filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return null;
            }

            return new List<CodeWithDescriptionContract>();
        }
    }
    /// <summary>
    ///     Interaction logic for CodeWithDescriptionEditor.xaml
    /// </summary>
    public partial class CodeWithDescriptionEditor : ISuggestionProvider
    {
        #region string Label

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(nameof(Label), typeof(string), typeof(CodeWithDescriptionEditor), new PropertyMetadata(default(string)));

        public string Label
        {
            get { return (string) GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }
        #endregion

       

        public SuggestionHandler SuggestionHandler { get; set; }

        #region Constructors
        public CodeWithDescriptionEditor()
        {
            InitializeComponent();
        }
        #endregion

        #region Explicit Interface Methods
        IEnumerable ISuggestionProvider.GetSuggestions(string filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return null;
            }

            if (SuggestionHandler == null)
            {
                return null;
            }

            return SuggestionHandler.Handle(filter).Where(x => x.Code.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 || x.Description.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
        }
        #endregion

        #region Text
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), 
                                                                                             typeof(string),
                                                                                             typeof(CodeWithDescriptionEditor),
                                                                                             new PropertyMetadata(default(string), OnTextChanged));

        static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = ((CodeWithDescriptionEditor) d);

            editor.Value = editor.SuggestionHandler.Handle((string)e.NewValue).FirstOrDefault(x => x.ToString() == (string) e.NewValue)?.Code;
        }

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        #endregion

        #region SelectedItem
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value),
                                                                                              typeof(string),
                                                                                              typeof(CodeWithDescriptionEditor),
                                                                                              new PropertyMetadata(default(string), OnValueChanged));

        static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = ((CodeWithDescriptionEditor) d);

            editor.Text = editor.SuggestionHandler.Handle(null).FirstOrDefault(x => x.Code == (string) e.NewValue)?.ToString();
        }

        public string Value
        {
            get { return (string) GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        #endregion
    }
}