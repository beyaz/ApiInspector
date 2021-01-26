using System;
using ApiInspector.Serialization;
using BOA.DataFlow;
using static ApiInspector.Keys;

namespace ApiInspector
{
    /// <summary>
    ///     The scope
    /// </summary>
    class Scope : DataContext
    {
        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="Scope" /> class.
        /// </summary>
        public Scope()
        {
            SetupGet(DbConnection, context => ConnectionInfo.GetDbConnection());
            SetupGet(UserName, context => _.AuthenticationUserName);
            SetupGet(SerializeHistoryForDatabaseInsert, context => new Serializer().SerializeToJsonIgnoreDefaultValuesHandleObjectTypeNames);
            
            Add(Trace, m => { });
        }
        #endregion
    }

    /// <summary>
    ///     The history event
    /// </summary>
    enum HistoryEvent
    {
        /// <summary>
        ///     The remove selected invocation information
        /// </summary>
        RemoveSelectedInvocationInfo,

        /// <summary>
        ///     The save to history
        /// </summary>
        SaveToHistory
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