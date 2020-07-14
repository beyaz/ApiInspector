using ApiInspector.DataAccess;
using ApiInspector.DataFlow;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiInspector.InvocationInfoEditor
{
    [TestClass]
    public class ControllerTest
    {
        [TestMethod]
        public void Should_load_data_according_to_component_interaction()
        {
            var builder = new ContextBuilder();

            var context = builder.Build();

            context.Contains(Data.AssemblyNames).Should().BeFalse();
                
            Controller.OnAssemblySearchDirectoryChanged(context);

            context.Get(Data.AssemblyNames).Count.Should().BeGreaterThan(0);
        }
    }
}