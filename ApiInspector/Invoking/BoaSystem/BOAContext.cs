using System;
using System.Security.Authentication;
using ApiInspector.Tracing;
using BOA.Base;
using BOA.Base.Data;
using BOA.Business.Kernel.General;
using BOA.Common.Configuration;
using BOA.Common.Helpers;
using BOA.Common.Types;
using BOA.Process.Kernel.Card.Internal;
using UserManager = BOA.Proxy.UserManager;

namespace ApiInspector.Invoking.BoaSystem
{
    class BOAContextData
    {
        #region Constructors
        public BOAContextData(EnvironmentInfo environmentInfo)
        {
            EnvironmentInfo = environmentInfo;
        }
        #endregion

        #region Public Properties
        public EnvironmentInfo EnvironmentInfo { get; }
        public bool IsBoaConfigurationFileLoaded { get; private set; }
        #endregion

        #region Public Methods
        public BOAContextData MarkAsConfigurationFileLoaded()
        {
            IsBoaConfigurationFileLoaded = true;

            return this;
        }
        #endregion
    }

    /// <summary>
    ///     The boa context
    /// </summary>
    class BOAContext : IDisposable
    {
        #region Fields
        readonly EnvironmentInfo environmentInfo;

        /// <summary>
        ///     The tracer
        /// </summary>
        readonly ITracer tracer;

        /// <summary>
        ///     The authentication response
        /// </summary>
        AuthenticationResponse authenticationResponse;

        /// <summary>
        ///     The context
        /// </summary>
        ExecutionDataContext context;

        BOAContextData data;

        /// <summary>
        ///     The object helper
        /// </summary>
        ObjectHelper objectHelper;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="BOAContext" /> class.
        /// </summary>
        public BOAContext(ITracer tracer, EnvironmentInfo environmentInfo)
        {
            this.tracer          = tracer ?? throw new ArgumentNullException(nameof(tracer));
            this.environmentInfo = environmentInfo ?? throw new ArgumentNullException(nameof(environmentInfo));

            data = new BOAContextData(environmentInfo);
        }
        #endregion

        #region Properties
        /// <summary>
        ///     Gets the context.
        /// </summary>
        internal ExecutionDataContext Context
        {
            get
            {
                if (context != null)
                {
                    return context;
                }

                LoadBOAConfigurationFile();

                context = new ExecutionDataContext
                {
                    EngineContext = new EngineContext()
                };

                Authenticate();

                context.ApplicationContext            = authenticationResponse.ApplicationContext;
                context.EngineContext.MainBusinessKey = CreateNewBusinessKey();

                return context;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Authenticates the specified channel contract.
        /// </summary>
        public void Authenticate(ChannelContract channelContract)
        {
            if (authenticationResponse != null)
            {
                return;
            }

            LoadBOAConfigurationFile();

            var request = new AuthenticationRequest
            {
                AuthenticationContext = new AuthenticationContext
                {
                    Channel  = channelContract,
                    UserName = EnvironmentVariables.GetUserName()
                }
            };
            tracer.Trace("Authenticate response waiting...");

            var response = new UserManager().Authenticate(request);
            if (!response.Success)
            {
                throw new AuthenticationException("LoginFailed." + StringHelper.ResultToDetailedString(response.Results));
            }

            tracer.Trace("Authenticate is success.");

            authenticationResponse = response;
        }

        /// <summary>
        ///     Authenticates this instance.
        /// </summary>
        public void Authenticate()
        {
            LoadBOAConfigurationFile();

            Authenticate(ConfigurationManager.ChannelSection.Channel.DefaultChannel);
        }

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
            if (objectHelper != null)
            {
                return objectHelper;
            }

            objectHelper = new ObjectHelper
            {
                Context = Context
            };

            objectHelper.Context.DBLayer.BeginTransaction();

            if (EnvironmentVariables.UseLocalProxyForCardServices())
            {
                CardService.UseLocalProxy = true;
            }

            return objectHelper;
        }
        #endregion

        #region Methods
        static BOAContextData LoadBOAConfigurationFile(BOAContextData data, Action<string> trace)
        {
            if (data.IsBoaConfigurationFileLoaded)
            {
                return data;
            }

            BoaConfigurationFile.Load(data.EnvironmentInfo.ToString, trace);

            return data.MarkAsConfigurationFileLoaded();
        }

        /// <summary>
        ///     Creates the new business key.
        /// </summary>
        decimal CreateNewBusinessKey()
        {
            const string ResourceCode = "ODSTATMTCP";

            return new BusinessKey(Context)
                   .CreateBusinessKey(ResourceCode, Context.ApplicationContext.User.BranchId, DateTime.Now.Date)
                   .Value;
        }

        void LoadBOAConfigurationFile()
        {
            data = LoadBOAConfigurationFile(data, tracer.Trace);
        }
        #endregion
    }
}