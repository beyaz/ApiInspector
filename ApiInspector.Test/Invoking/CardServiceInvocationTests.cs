using System.Collections.Generic;
using ApiInspector.Models;
using BOA.Card.Contracts.CreditCard.Limit;
using BOA.Common.Types;
using BOA.DataFlow;
using BOA.UnitTestHelper;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiInspector.Invoking
{
    [TestClass]
    public class CardServiceInvocationTests
    {
        [TestMethod]
        public void Should_invoke_any_service_method()
        {
            BOAAssemblyResolver.AttachToCurrentDomain();

            var invocationInfo = new InvocationInfo
            {
                AssemblyName            ="BOA.Card.Services.CreditCard.Limit.dll",
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

            var context = new DataContext
            {
                {InvocationContextKeys.BOAContext, new BOAContext("dev")},
                {
                    InvocationContextKeys.InvocationParameters, new List<object>()
                    {
                        new GetCardAvailableLimitRequest
                        {
                            CardRefNumber = "1"
                        }
                    }
                },
                {InvocationContextKeys.InvocationInfo,invocationInfo},
                {InvocationContextKeys.Trace,(message)=>{}}
                
            };

            Invoker.InitializeTargetType(context);

            CardServiceMethodInvoker.Invoke(context);

            var response = (GenericResponse<GetCardAvailableLimitResponse>) InvocationContextKeys.Response[context];

            response.Success.Should().BeTrue();

            response.Value.Should().NotBeNull();
        }
    }
}