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
    /// <summary>
    ///     The boa context
    /// </summary>
    class BOAContext : IDisposable
    {
        #region Fields
        /// <summary>
        ///     The boa configuration file
        /// </summary>
        readonly BoaConfigurationFile boaConfigurationFile;

        

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

        /// <summary>
        ///     The object helper
        /// </summary>
        ObjectHelper objectHelper;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="BOAContext" /> class.
        /// </summary>
        public BOAContext(ITracer tracer, BoaConfigurationFile boaConfigurationFile)
        {
            this.tracer               = tracer ?? throw new ArgumentNullException(nameof(tracer));
            this.boaConfigurationFile = boaConfigurationFile ?? throw new ArgumentNullException(nameof(boaConfigurationFile));
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

                boaConfigurationFile.Load();

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

            boaConfigurationFile.Load();

            
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
            boaConfigurationFile.Load();

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
        #endregion
    }
}