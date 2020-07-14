using System;
using ApiInspector.Components;
using ApiInspector.DataAccess;
using ApiInspector.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiInspector.Test
{

    class AnyClass
    {
        public string AnyMethod_0()
        {
            return "0";
        }
    }


    [TestClass]
    public class InvocationTests
    {
        [TestMethod]
        public void Should_invoke_non_static_parameterless_method()
        {
            var builder = new InvocationInfoEditorContextBuilder();

            var context = builder.Build();

            var invocationInfo = new InvocationInfo()
            {
                AssemblyName = "ApiInspector.Test.dll",
                ClassName    = "ApiInspector.Test.AnyClass",
                MethodName   = "AnyMethod_0"
            };

            context.Update(DataKeys.InvocationInfo,invocationInfo);

            Logic.Execute(context);

            var response = context.Get(DataKeys.ExecutionResponse);

            response.Should().Be("0");
        }
    }
}
