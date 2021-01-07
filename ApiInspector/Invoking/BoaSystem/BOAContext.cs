using System;
using System.Security.Authentication;
using BOA.Base;
using BOA.Base.Data;
using BOA.Business.Kernel.General;
using BOA.Common.Configuration;
using BOA.Common.Helpers;
using BOA.Common.Types;
using BOA.Process.Kernel.Card.Internal;
using FunctionalPrograming;
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
        public AuthenticationResponse authenticationResponse { get; private set; }

        public ExecutionDataContext context { get; private set; }
        public EnvironmentInfo EnvironmentInfo { get; }
        public bool IsBoaConfigurationFileLoaded { get; private set; }

        public ObjectHelper objectHelper { get; private set; }
        #endregion

        #region Public Methods
        public BOAContextData MarkAsConfigurationFileLoaded()
        {
            IsBoaConfigurationFileLoaded = true;

            return this;
        }

        public BOAContextData WithAuthenticationResponse(AuthenticationResponse authenticationResponse)
        {
            this.authenticationResponse = authenticationResponse;

            return this;
        }

        public BOAContextData WithExecutionDataContext(ExecutionDataContext context)
        {
            this.context = context;

            return this;
        }

        public BOAContextData WithObjectHelper(ObjectHelper objectHelper)
        {
            this.objectHelper = objectHelper;

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
        readonly Action<string> trace;

        BOAContextData data;
        #endregion

        #region Constructors
        

        public BOAContext(EnvironmentInfo environmentInfo, Action<string> trace)
        {
            data = new BOAContextData(environmentInfo);

            this.trace = trace;
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
                data = GetOrCreateContext(data, trace);

                return data.context;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Authenticates the specified channel contract.
        /// </summary>
        public void Authenticate(ChannelContract channelContract)
        {
            data = Authenticate(data, trace, channelContract);
        }

        /// <summary>
        ///     Authenticates this instance.
        /// </summary>
        public void Authenticate()
        {
            data = LoadBOAConfigurationFile(data, trace);

            Authenticate(ConfigurationManager.ChannelSection.Channel.DefaultChannel);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (data.objectHelper == null)
            {
                new BoaDirectInvokeDisposer().Dispose();
                return;
            }

            data.objectHelper.Context.DBLayer.CommitTransaction();
            new BoaDirectInvokeDisposer().Dispose();
        }

        /// <summary>
        ///     Gets the object helper.
        /// </summary>
        public ObjectHelper GetObjectHelper()
        {
            data = GetOrCreateObjectHelper(data, trace);

            return data.objectHelper;
        }
        #endregion

        #region Methods
        static BOAContextData Authenticate(BOAContextData data, Action<string> trace, ChannelContract channelContract)
        {
            if (data.authenticationResponse != null)
            {
                return data;
            }

            data = LoadBOAConfigurationFile(data, trace);

            var request = new AuthenticationRequest
            {
                AuthenticationContext = new AuthenticationContext
                {
                    Channel  = channelContract,
                    UserName = EnvironmentVariables.GetUserName()
                }
            };
            trace("Authenticate response waiting...");

            var response = new UserManager().Authenticate(request);
            if (!response.Success)
            {
                throw new AuthenticationException("LoginFailed." + StringHelper.ResultToDetailedString(response.Results));
            }

            trace("Authenticate is success.");

            return data.WithAuthenticationResponse(response);
        }

        static BOAContextData GetOrCreateContext(BOAContextData data, Action<string> trace)
        {
            if (data.context != null)
            {
                return data;
            }

            data = LoadBOAConfigurationFile(data, trace);

            var context = new ExecutionDataContext
            {
                EngineContext = new EngineContext()
            };

            data = LoadBOAConfigurationFile(data, trace);

            data = Authenticate(data, trace, ConfigurationManager.ChannelSection.Channel.DefaultChannel);

            context.ApplicationContext = data.authenticationResponse.ApplicationContext;

            var createNewBusinessKey = FPExtensions.fun(() =>
            {
                const string ResourceCode = "ODSTATMTCP";

                return new BusinessKey(context).CreateBusinessKey(ResourceCode, context.ApplicationContext.User.BranchId, DateTime.Now.Date).Value;
            });

            context.EngineContext.MainBusinessKey = createNewBusinessKey();

            return data.WithExecutionDataContext(context);
        }

        static BOAContextData GetOrCreateObjectHelper(BOAContextData data, Action<string> trace)
        {
            if (data.objectHelper != null)
            {
                return data;
            }

            data = GetOrCreateContext(data, trace);

            var objectHelper = new ObjectHelper
            {
                Context = data.context
            };

            objectHelper.Context.DBLayer.BeginTransaction();

            if (EnvironmentVariables.UseLocalProxyForCardServices())
            {
                CardService.UseLocalProxy = true;
            }

            return data.WithObjectHelper(objectHelper);
        }

        static BOAContextData LoadBOAConfigurationFile(BOAContextData data, Action<string> trace)
        {
            if (data.IsBoaConfigurationFileLoaded)
            {
                return data;
            }

            BoaConfigurationFile.Load(data.EnvironmentInfo.ToString, trace);

            return data.MarkAsConfigurationFileLoaded();
        }
        #endregion
    }
}