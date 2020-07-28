using System;
using System.Collections.Generic;
using System.Security.Authentication;
using ApiInspector.Tracing;
using BOA.Base;
using BOA.Base.Data;
using BOA.Business.Kernel.General;
using BOA.Common.Helpers;
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
        ///     The environment information
        /// </summary>
        readonly EnvironmentInfo environmentInfo;

        /// <summary>
        ///     The tracer
        /// </summary>
        readonly ITracer tracer;

        /// <summary>
        ///     The object helper
        /// </summary>
        ObjectHelper objectHelper;
        #endregion

        readonly BoaConfigurationFile boaConfigurationFile;

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="BOAContext" /> class.
        /// </summary>
        public BOAContext(EnvironmentInfo environmentInfo, ITracer tracer, BoaConfigurationFile boaConfigurationFile)
        {
            this.environmentInfo      = environmentInfo ?? throw new ArgumentNullException(nameof(environmentInfo));
            this.tracer               = tracer ?? throw new ArgumentNullException(nameof(tracer));
            this.boaConfigurationFile = boaConfigurationFile ?? throw new ArgumentNullException(nameof(boaConfigurationFile));
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
        ObjectHelper CreateObjectHelper()
        {
            var testContext = CreateTestContext(environmentInfo);

            ObjectHelper instance = null;
            if (environmentInfo.IsDev)
            {
                instance = testContext.objectHelper;

                //objectHelper.Context.DBLayer.ConnectionMock = new Dictionary<Databases, string>
                //{
                //    {Databases.BanksoftCC, @"Data Source=srvxdev\zumrut;Initial Catalog=KrediKuveyt;Min Pool Size=10; Max Pool Size=100;Application Name=BOAApp;Integrated Security=true;"}
                //};
            }
            else if (environmentInfo.IsTest)
            {
                instance = testContext.objectHelper;

                instance.Context.DBLayer.ConnectionMock = new Dictionary<Databases, string>
                {
                    {Databases.BOACard, @"Data Source=srvxtest\zumrut;Initial Catalog=BOACard2;Min Pool Size=10; Max Pool Size=100;Application Name=BOAApp;Integrated Security=true;"}
                };

                tracer.Trace(@"BOACard cconnection is: srvxtest\zumrut;Initial Catalog=BOACard2");
            }
            else
            {
                throw new NotImplementedException(nameof(environmentInfo));
            }

            return instance;
        }

        /// <summary>
        ///     Creates the test context.
        /// </summary>
        public static BOATestContext CreateTestContext(EnvironmentInfo environmentInfo)
        {
            if (environmentInfo.IsDev)
            {
                return new BOATestContextDev();
            }

            if (environmentInfo.IsTest)
            {
                return new BOATestContextTest();
            }

            throw new NotImplementedException(nameof(environmentInfo));
        }

        /// <summary>
        ///     Initializes this instance.
        /// </summary>
        void Initialize()
        {
            objectHelper = CreateObjectHelper();

            objectHelper.Context.DBLayer.BeginTransaction();

            CardService.UseLocalProxy = true;
        }


        AuthenticationResponse authenticationResponse;

        public void Authenticate()
        {
            if (authenticationResponse != null)
            {
                tracer.Trace("Already authenticated.");
                return;
            }

            var request = boaConfigurationFile.GetAuthenticationRequest();

            var response = new BOA.Proxy.UserManager().Authenticate(request);
            if (!response.Success)
            {
                throw new AuthenticationException("LoginFailed." + StringHelper.ResultToDetailedString(response.Results));
            }

            authenticationResponse = response;


        }

        ExecutionDataContext _context;
        public ExecutionDataContext Context
        {
            get
            {
                if (_context == null)
                {
                    boaConfigurationFile.Load();

                    _context = new ExecutionDataContext
                    {
                        EngineContext = new EngineContext()
                    };

                    Authenticate();

                    _context.ApplicationContext            = authenticationResponse.ApplicationContext;
                    _context.EngineContext.MainBusinessKey = CreateNewBusinessKey();
                }

                return _context;
            }
        }

        decimal CreateNewBusinessKey()
        {
            const string ResourceCode = "ODSTATMTCP";

            return new BusinessKey(Context)
                   .CreateBusinessKey(ResourceCode, Context.ApplicationContext.User.BranchId, DateTime.Now.Date)
                   .AssertSuccess();
        }


        #endregion
    }
}