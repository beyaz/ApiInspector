using System.Windows.Controls;
using ApiInspector.DataAccess;
using ApiInspector.MainWindow;
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

            var invocationInfo = context.Get(Data.InvocationInfo);
            var itemSourceList = context.Get(Data.ItemSourceList);

            // On Search Directory Changed
            {
                itemSourceList.AssemblyNameList.Should().BeNull();
                
                ViewController.OnAssemblySearchDirectoryChanged(context);

                itemSourceList.AssemblyNameList.Count.Should().BeGreaterThan(0);
            }


            // On Assembly Name Changed
            {
                itemSourceList.ClassNameList.Should().BeNull();

                invocationInfo.AssemblyName = "BOA.Process.Kernel.Card.dll";


                ViewController.OnAssemblyNameChanged(context);

                itemSourceList.ClassNameList.Count.Should().BeGreaterThan(0);
            }


            // On Class Name Changed
            {
                itemSourceList.MethodNameList.Should().BeNull();

                invocationInfo.ClassName = "BOA.Process.Kernel.Card.CallCenter.IVR.CreditCard.GetCardDetail";


                ViewController.OnClassNameChanged(context);

                itemSourceList.MethodNameList.Count.Should().BeGreaterThan(0);
            }


            // On Method Name Changed
            {
                context.Update(Data.ParametersPanel,new StackPanel());

                context.Contains(Data.MethodDefinition).Should().BeFalse();

                invocationInfo.MethodName = "ExecuteInOldCardSystem";


                ViewController.OnMethodNameSelected(context);

                context.Contains(Data.MethodDefinition).Should().BeTrue();
            }

        }
    }
}