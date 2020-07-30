using System;
using System.Collections.Generic;
using ApiInspector.Models;

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
            return Utility.TryRun(() => boaDevDataSource.GetHistory()) ?? new InvocationInfo[0];
        }

        /// <summary>
        ///     Removes the specified information.
        /// </summary>
        public void Remove(InvocationInfo info)
        {
            boaDevDataSource.Remove(info);
        }

        /// <summary>
        ///     Saves to history.
        /// </summary>
        public void SaveToHistory(InvocationInfo info)
        {
            boaDevDataSource.SaveToHistory(info);
        }
        #endregion
    }
}