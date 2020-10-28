using System.Collections;

namespace ApiInspector.Components
{
    /// <summary>
    ///     The editor property
    /// </summary>
    static class EditorProp
    {
        #region Static Fields
        /// <summary>
        ///     The binding path
        /// </summary>
        public static readonly DataKey<string> BindingPath = new DataKey<string>(typeof(EditorProp), nameof(BindingPath));

        /// <summary>
        ///     The data context
        /// </summary>
        public static readonly DataKey<object> DataContext = new DataKey<object>(typeof(EditorProp), nameof(DataContext));

        /// <summary>
        ///     The display member path
        /// </summary>
        public static readonly DataKey<string> DisplayMemberPath = new DataKey<string>(typeof(EditorProp), nameof(DisplayMemberPath));

        /// <summary>
        ///     The items source
        /// </summary>
        public static readonly DataKey<IEnumerable> ItemsSource = new DataKey<IEnumerable>(typeof(EditorProp), nameof(ItemsSource));

        /// <summary>
        ///     The label
        /// </summary>
        public static readonly DataKey<string> LabelText = new DataKey<string>(typeof(EditorProp), nameof(LabelText));

        /// <summary>
        ///     The value member path
        /// </summary>
        public static readonly DataKey<string> ValueMemberPath = new DataKey<string>(typeof(EditorProp), nameof(ValueMemberPath));
        #endregion
    }
}