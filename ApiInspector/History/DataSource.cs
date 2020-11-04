using System;
using System.Collections.Generic;
using ApiInspector.Models;
using static ApiInspector.Utility;

namespace ApiInspector.History
{
    /// <summary>
    ///     The data source
    /// </summary>
    class DataSource
    {
        #region Fields
        /// <summary>
        ///     The boa dev data source
        /// </summary>
        readonly BoaDevDataSource boaDevDataSource;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="DataSource" /> class.
        /// </summary>
        public DataSource(BoaDevDataSource boaDevDataSource)
        {
            this.boaDevDataSource = boaDevDataSource ?? throw new ArgumentNullException(nameof(boaDevDataSource));
        }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Gets the history.
        /// </summary>
        public IReadOnlyList<InvocationInfo> GetHistory()
        {
            return TryRun(()=>BoaDevDataSource.GetHistory(new Scope())) ?? new List<InvocationInfo>();
        }
        
        #endregion
    }
}