using System.Collections.Generic;
using ApiInspector.Models;
using BOA.Card.Contracts.CreditCard.Limit;
using BOA.Common.Types;
using BOA.UnitTestHelper;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiInspector.Invoking
{
    [TestClass]
    public class CardServiceInvocationTests
    {
        #region Public Methods
        [TestMethod]
        public void Should_invoke_any_service_method()
        {
            BOAAssemblyResolver.AttachToCurrentDomain();

            var invocationInfo = new InvocationInfo
            {
                AssemblyName            = "BOA.Card.Services.CreditCard.Limit.dll",
                AssemblySearchDirectory = "d:\\boa\\server\\bin\\",
                ClassName               = "BOA.Card.Services.CreditCard.Limit.CRDLimitService",

                Environment = "dev",
                MethodName  = "GetCardAvailableLimit",
                Parameters = new List<InvocationMethodParameterInfo>
                {
                    new InvocationMethodParameterInfo
                    {
                        Value = "{  CardRefNumber:'1000'}"
                    }
                }
            };

            var targetType = Invoker.InitializeTargetType(invocationInfo);

            var invocationParameters = new List<object>
            {
                new GetCardAvailableLimitRequest
                {
                    CardRefNumber = "1"
                }
            };
            var input = new CardServiceMethodInvokerInput(targetType, invocationInfo.MethodName, invocationParameters, message => { }, new BOAContext("dev"));

            var cardServiceMethodInvoker = new CardServiceMethodInvoker();

            var response = (GenericResponse<GetCardAvailableLimitResponse>) cardServiceMethodInvoker.Invoke(input);

            response.Success.Should().BeTrue();

            response.Value.Should().NotBeNull();
        }
        #endregion
    }
}