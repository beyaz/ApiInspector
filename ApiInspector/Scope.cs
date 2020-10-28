using System;
using BOA.DataFlow;

namespace ApiInspector
{
    /// <summary>
    ///     The scope
    /// </summary>
    class Scope : DataContext
    {
    }

    /// <summary>
    ///     The data key
    /// </summary>
    class DataKey<T> : BOA.DataFlow.DataKey<T>
    {
        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="DataKey{T}" /> class.
        /// </summary>
        public DataKey(Type locatedType, string fieldName) : base(locatedType, fieldName)
        {
        }
        #endregion
    }
}