using System.Collections.Generic;
using System.Linq;
using Dapper;
using static ApiInspector.Application.ConnectionInfo;

namespace ApiInspector.Application
{
    /// <summary>
    ///     The boa user data source
    /// </summary>
    class BoaUserDataSource
    {
        #region Public Methods
        /// <summary>
        ///     Gets the users.
        /// </summary>
        public IReadOnlyList<BoaUserModel> GetUsers(string search)
        {
            return GetDbConnection().Query<BoaUserModel>($"SELECT [UserCode], [Name]  FROM COR.BoaUser WITH(NOLOCK) WHERE [Name] LIKE '%' + @{nameof(search)} + '%'", new {search}).ToList();
        }
        #endregion
    }

    /// <summary>
    ///     The user model
    /// </summary>
    class BoaUserModel
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the user code.
        /// </summary>
        public string UserCode { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        public override string ToString()
        {
            return Name;
        }
        #endregion
    }
}