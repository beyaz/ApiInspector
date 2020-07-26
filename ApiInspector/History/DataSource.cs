using System;
using System.Collections.Generic;
using System.Linq;
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
        readonly BoaDevDataSource boaDevDataSource = new BoaDevDataSource();

        /// <summary>
        ///     The file data source
        /// </summary>
        readonly FileDataSource fileDataSource = new FileDataSource();
        #endregion

        #region Public Methods
        /// <summary>
        ///     Gets the history.
        /// </summary>
        public IReadOnlyList<InvocationInfo> GetHistory()
        {
            IReadOnlyList<InvocationInfo> records = null;
            if (Utility.IsSuccess(() => boaDevDataSource.GetHistory(), ref records))
            {
                Array.ForEach(records.ToArray(), fileDataSource.SaveToHistory);
            }

            return fileDataSource.GetHistory();
        }

        /// <summary>
        ///     Removes the specified information.
        /// </summary>
        public void Remove(InvocationInfo info)
        {
            Utility.TryRun(() => boaDevDataSource.Remove(info));

            fileDataSource.Remove(info);
        }

        /// <summary>
        ///     Saves to history.
        /// </summary>
        public void SaveToHistory(InvocationInfo info)
        {
            Utility.TryRun(() => boaDevDataSource.SaveToHistory(info));

            fileDataSource.SaveToHistory(info);
        }
        #endregion
    }
}