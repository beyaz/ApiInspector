using System.Collections.Generic;
using ApiInspector.Models;
using BOA.Card.Contracts.CreditCard.Limit;
using BOA.DataFlow;
using BOA.UnitTestHelper;
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
                AssemblyName            ="BOA.Card.Services.CreditCard.Lifecycle.dll",
                AssemblySearchDirectory = "d:\\boa\\server\\bin\\",
                ClassName               = "BOA.Card.Services.CreditCard.Lifecycle.CRDLifecycleService",


                Environment = "dev",
                MethodName  = "SaveCustomerTemporaryAddress",
                Parameters = new List<InvocationMethodParameterInfo>
                {
                    new InvocationMethodParameterInfo
                    {
                        Value = "{  CustomerNumber:100,  CustomerTemporaryAddressText:''}"
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
                }
            };

            CardServiceMethodInvoker.Invoke(context);

            
        }
    }
}