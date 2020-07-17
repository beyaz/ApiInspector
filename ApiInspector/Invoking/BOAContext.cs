using System;
using System.Collections.Generic;
using BOA.Base;
using BOA.Common.Types;
using BOA.Process.Kernel.Card;
using BOA.UnitTestHelper;

namespace ApiInspector.Invoking
{
    /// <summary>
    ///     The boa context
    /// </summary>
    class BOAContext : IDisposable
    {
        #region Fields
        /// <summary>
        ///     The target environment
        /// </summary>
        readonly string targetEnvironment;

        /// <summary>
        ///     The object helper
        /// </summary>
        ObjectHelper objectHelper;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="BOAContext" /> class.
        /// </summary>
        public BOAContext(string targetEnvironment)
        {
            this.targetEnvironment = targetEnvironment;
        }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (objectHelper == null)
            {
                return;
            }

            objectHelper.Context.DBLayer.CommitTransaction();
        }

        /// <summary>
        ///     Gets the object helper.
        /// </summary>
        public ObjectHelper GetObjectHelper()
        {
            if (objectHelper == null)
            {
                Initialize();
            }

            return objectHelper;
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Creates the object helper.
        /// </summary>
        static ObjectHelper CreateObjectHelper(string targetEnvironment)
        {
            ObjectHelper objectHelper = null;
            if (targetEnvironment.IndexOf("dev", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                objectHelper = new BOATestContextDev().objectHelper;

                //objectHelper.Context.DBLayer.ConnectionMock = new Dictionary<Databases, string>
                //{
                //    {Databases.BanksoftCC, @"Data Source=srvxdev\zumrut;Initial Catalog=KrediKuveyt;Min Pool Size=10; Max Pool Size=100;Application Name=BOAApp;Integrated Security=true;"}
                //};
            }
            else if (targetEnvironment.IndexOf("test", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                objectHelper = new BOATestContextTest().objectHelper;

                objectHelper.Context.DBLayer.ConnectionMock = new Dictionary<Databases, string>
                {
                    {Databases.BOACard, @"Data Source=srvxtest\zumrut;Initial Catalog=BOACard2;Min Pool Size=10; Max Pool Size=100;Application Name=BOAApp;Integrated Security=true;"},
                    //{Databases.BanksoftCC, @"Data Source=srvxtest\zumrut;Initial Catalog=KrediKuveyt;Min Pool Size=10; Max Pool Size=100;Application Name=BOAApp;Integrated Security=true;"}
                };
            }
            else
            {
                throw new NotImplementedException(nameof(targetEnvironment));
            }

            return objectHelper;
        }

        /// <summary>
        ///     Initializes this instance.
        /// </summary>
        void Initialize()
        {
            objectHelper = CreateObjectHelper(targetEnvironment);

            objectHelper.Context.DBLayer.BeginTransaction();

            CardService.UseLocalProxy = true;
        }
        #endregion
    }
}