using System;
using ApiInspector.Application;
using ApiInspector.History;
using ApiInspector.Invoking.BoaSystem;
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
            SetupGet(UserName, context => EnvironmentVariable.GetUserName(null));
            SetupGet(SerializeHistoryForDatabaseInsert,context=>new Serialization.Serializer().SerializeToJsonIgnoreDefaultValuesHandleObjectTypeNames);
            SubscribeEvent(HistoryEvent.RemoveSelectedInvocationInfo,()=>BoaDevDataSource.Remove(this));
        }
        #endregion
    }

    enum HistoryEvent
    {
    RemoveSelectedInvocationInfo    
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