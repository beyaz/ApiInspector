using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using static System.Windows.DependencyProperty;

namespace ApiInspector.Components
{
    /// <summary>
    ///     The automatic filtered ComboBox
    /// </summary>
    public class AutoFilteredComboBox : ComboBox
    {
        #region Fields
        /// <summary>
        ///     The coll view
        /// </summary>
        ICollectionView _collView;

        /// <summary>
        ///     The keyboard selection guard
        /// </summary>
        bool _keyboardSelectionGuard;

        /// <summary>
        ///     The length
        /// </summary>
        int _length;

        /// <summary>
        ///     The saved text
        /// </summary>
        string _savedText;

        /// <summary>
        ///     The silence events
        /// </summary>
        int _silenceEvents;

        /// <summary>
        ///     The start
        /// </summary>
        int _start;

        /// <summary>
        ///     The text saved
        /// </summary>
        bool _textSaved;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="AutoFilteredComboBox" /> class.
        /// </summary>
        public AutoFilteredComboBox()
        {
            var textProperty = DependencyPropertyDescriptor.FromProperty(
                                                                         TextProperty,
                                                                         typeof(AutoFilteredComboBox));
            textProperty.AddValueChanged(this, OnTextChanged);
            RegisterIsCaseSensitiveChangeNotification();
        }
        #endregion

        #region Properties
        /// <summary>
        ///     Gets the editable text box.
        /// </summary>
        TextBox EditableTextBox
        {
            get { return (TextBox) GetTemplateChild("PART_EditableTextBox"); }
        }

        /// <summary>
        ///     Gets the items popup.
        /// </summary>
        Popup ItemsPopup
        {
            get { return (Popup) GetTemplateChild("PART_Popup"); }
        }

        /// <summary>
        ///     Gets the items scroll viewer.
        /// </summary>
        ScrollViewer ItemsScrollViewer
        {
            get
            {
                var border = ItemsPopup.FindName("DropDownBorder") as Border;
                if (border == null)
                {
                    return null;
                }

                return border.Child as ScrollViewer;
            }
        }
        #endregion

        #region Public Methods
        // Handle selection

        /// <summary>
        ///     Called when <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" /> is called.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            EditableTextBox.SelectionChanged += EditableTextBox_SelectionChanged;
            ItemsPopup.Focusable             =  true;
        }
        #endregion

        #region Methods
        // Handle focus

        /// <summary>
        ///     Invoked whenever an unhandled <see cref="E:System.Windows.UIElement.GotFocus" /> event reaches this element in its
        ///     route.
        /// </summary>
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            if (ItemsSource == null)
            {
                return;
            }

            if (DropDownOnFocus)
            {
                IsDropDownOpen = true;
            }
        }

        /// <summary>
        ///     Called when [is case sensitive changed].
        /// </summary>
        protected virtual void OnIsCaseSensitiveChanged(object    sender,
                                                        EventArgs e)
        {
            if (IsCaseSensitive)
            {
                IsTextSearchEnabled = false;
            }

            RefreshFilter();
        }

        /// <summary>
        ///     Called when the source of an item in a selector changes.
        /// </summary>
        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            if (newValue != null)
            {
                _collView        =  CollectionViewSource.GetDefaultView(newValue);
                _collView.Filter += FilterPredicate;
            }

            if (oldValue != null)
            {
                _collView        =  CollectionViewSource.GetDefaultView(oldValue);
                _collView.Filter -= FilterPredicate;
            }

            base.OnItemsSourceChanged(oldValue, newValue);
        }

        /// <summary>
        ///     Invoked when a <see cref="E:System.Windows.Input.Keyboard.KeyDown" /> attached routed event occurs.
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    // Escape removes current filter
                    _keyboardSelectionGuard = false;
                    UnSilenceEvents();
                    ClearFilter();
                    IsDropDownOpen = true;
                    return;
            }

            base.OnKeyDown(e);
        }

        /// <summary>
        ///     Invoked when a <see cref="E:System.Windows.Input.Keyboard.PreviewKeyDown" /> attached routed event occurs.
        /// </summary>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                case Key.Tab:
                    IsDropDownOpen = false;
                    break;
                case Key.Escape:
                    // Escape removes current filter
                    _keyboardSelectionGuard = false;
                    UnSilenceEvents();
                    ClearFilter();
                    IsDropDownOpen = true;
                    return;
                case Key.Down:
                case Key.Up:
                    // Open dropdown
                    IsDropDownOpen = true;
                    if (!_keyboardSelectionGuard)
                    {
                        _keyboardSelectionGuard = true;
                        SilenceEvents();
                    }

                    break;
            }

            base.OnPreviewKeyDown(e);
        }

        /// <summary>
        ///     Invoked when an unhandled <see cref="E:System.Windows.Input.Keyboard.PreviewKeyDown" /> attached event reaches an
        ///     element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// </summary>
        protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            if (Text.Length == 0)
            {
                RestoreSavedText();
            }
            else if (SelectedItem != null)
            {
                _savedText = SelectedItem.ToString();
            }

            base.OnPreviewLostKeyboardFocus(e);
        }

        /// <summary>
        ///     Clears the filter.
        /// </summary>
        void ClearFilter()
        {
            _length = 0;
            _start  = 0;
            RefreshFilter();
            Text = "";
            ScrollItemsToTop();
        }

        /// <summary>
        ///     Handles the SelectionChanged event of the EditableTextBox control.
        /// </summary>
        void EditableTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var origTextBox = (TextBox) e.OriginalSource;
            var origStart   = origTextBox.SelectionStart;
            var origLength  = origTextBox.SelectionLength;

            if (_silenceEvents > 0)
            {
                return;
            }

            _start  = origStart;
            _length = origLength;
            RefreshFilter();
            ScrollItemsToTop();
        }

        /// <summary>
        ///     Filters the predicate.
        /// </summary>
        bool FilterPredicate(object value)
        {
            // We don't like nulls.
            if (value == null)
            {
                return false;
            }

            // If there is no text, there's no reason to filter.
            if (Text.Length == 0)
            {
                return true;
            }

            var prefix = Text;

            // If the end of the text is selected, do not mind it.
            if (_length > 0 && _start + _length == Text.Length)
            {
                prefix = prefix.Substring(0, _start);
            }

            if (IsCaseSensitive)
            {
                return (value + string.Empty).IndexOf(prefix, StringComparison.Ordinal) >= 0;
            }

            return (value + string.Empty).IndexOf(prefix, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        ///     Called when [text changed].
        /// </summary>
        void OnTextChanged(object sender, EventArgs e)
        {
            if (!_textSaved)
            {
                _savedText = Text;
                _textSaved = true;
            }

            if (IsTextSearchEnabled || _silenceEvents != 0)
            {
                return;
            }

            RefreshFilter();

            // Manually simulate the automatic selection that would have been
            // available if the IsTextSearchEnabled dependency property was set.
            if (Text.Length <= 0)
            {
                return;
            }

            var prefix = Text.Length;
            _collView = CollectionViewSource.GetDefaultView(ItemsSource);
            foreach (var item in _collView)
            {
                var text = item.ToString().Length;
                SelectedItem = item;

                SilenceEvents();
                EditableTextBox.Text = item.ToString();
                EditableTextBox.Select(prefix, text - prefix);
                UnSilenceEvents();
                break;
            }
        }

        /// <summary>
        ///     Refreshes the filter.
        /// </summary>
        void RefreshFilter()
        {
            if (ItemsSource == null)
            {
                return;
            }

            _collView = CollectionViewSource.GetDefaultView(ItemsSource);
            _collView.Refresh();
            IsDropDownOpen = true;
        }

        /// <summary>
        ///     Registers the is case sensitive change notification.
        /// </summary>
        void RegisterIsCaseSensitiveChangeNotification()
        {
            DependencyPropertyDescriptor.FromProperty(IsCaseSensitiveProperty, typeof(AutoFilteredComboBox)).AddValueChanged(this, OnIsCaseSensitiveChanged);
        }

        /// <summary>
        ///     Restores the saved text.
        /// </summary>
        void RestoreSavedText()
        {
            Text = _textSaved ? _savedText : "";
            EditableTextBox.SelectAll();
        }

        // Handle filtering

        /// <summary>
        ///     Scrolls the items to top.
        /// </summary>
        void ScrollItemsToTop()
        {
            // need to find the scroll viewer containing list items and scroll
            // it to the top whenever filter is updated; otherwise user won't
            // see the top part of the filtered list of choices
            // See http://social.msdn.microsoft.com/forums/en-US/wpf/thread/5b788897-669c-4d1f-8744-9ace6e5c4b38
            var scrollViewer = ItemsScrollViewer;
            if (scrollViewer == null)
            {
                return;
            }

            scrollViewer.ScrollToTop();
        }

        /// <summary>
        ///     Silences the events.
        /// </summary>
        void SilenceEvents()
        {
            ++_silenceEvents;
        }

        /// <summary>
        ///     Uns the silence events.
        /// </summary>
        void UnSilenceEvents()
        {
            if (_silenceEvents > 0)
            {
                --_silenceEvents;
            }
        }
        #endregion

        #region bool IsCaseSensitive
        /// <summary>
        ///     The is case sensitive property
        /// </summary>
        public static readonly DependencyProperty IsCaseSensitiveProperty = Register("IsCaseSensitive", typeof(bool), typeof(AutoFilteredComboBox), new UIPropertyMetadata(false));

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is case sensitive.
        /// </summary>
        [Description("The way the combo box treats the case sensitivity of typed text")]
        [Category("AutoFiltered ComboBox")]
        [DefaultValue(true)]
        public bool IsCaseSensitive
        {
            [DebuggerStepThrough] get { return (bool) GetValue(IsCaseSensitiveProperty); }
            [DebuggerStepThrough] set { SetValue(IsCaseSensitiveProperty, value); }
        }
        #endregion

        #region bool DropDownOnFocus
        /// <summary>
        ///     The drop down on focus property
        /// </summary>
        public static readonly DependencyProperty DropDownOnFocusProperty = Register("DropDownOnFocus", typeof(bool), typeof(AutoFilteredComboBox), new UIPropertyMetadata(true));

        /// <summary>
        ///     Gets or sets a value indicating whether [drop down on focus].
        /// </summary>
        [Description("The way the combo box behaves when it receives focus")]
        [Category("AutoFiltered ComboBox")]
        [DefaultValue(true)]
        public bool DropDownOnFocus
        {
            [DebuggerStepThrough] get { return (bool) GetValue(DropDownOnFocusProperty); }
            [DebuggerStepThrough] set { SetValue(DropDownOnFocusProperty, value); }
        }
        #endregion
    }
}