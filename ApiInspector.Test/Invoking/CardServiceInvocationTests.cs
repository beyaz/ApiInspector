using System.Collections.Generic;
using ApiInspector.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiInspector.Invoking
{
    [TestClass]
    public class CardServiceInvocationTests
    {
        [TestMethod]
        public void Should_invoke_any_service_method()
        {
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

            var invoker = new Invoker(message => { });

            invoker.Invoke(invocationInfo);
            
        }
    }
}