using System;
using ApiInspector.Application;
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
        }
        #endregion
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