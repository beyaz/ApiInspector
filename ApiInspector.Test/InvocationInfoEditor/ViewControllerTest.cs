using ApiInspector.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiInspector.InvocationInfoEditor
{
    /// <summary>
    ///     The view controller test
    /// </summary>
    [TestClass]
    public class ViewControllerTest
    {
        #region Public Methods
        /// <summary>
        ///     Shoulds the load data according to component interaction.
        /// </summary>
        [TestMethod]
        public void Should_load_data_according_to_component_interaction()
        {
            var scope = new Scope();

            var invocationInfo = new InvocationInfo
            {
                AssemblySearchDirectory = @"d:\boa\server\bin\"
            };

            var itemSourceList = new ItemSourceList();

          
           

            // On Search Directory Changed
            {
                itemSourceList.AssemblyNameList.Should().BeNull();

                ViewController.OnAssemblySearchDirectoryChanged(scope);

                itemSourceList.AssemblyNameList.Count.Should().BeGreaterThan(0);
            }

            // On Assembly Name Changed
            {
                itemSourceList.ClassNameList.Should().BeNull();

                invocationInfo.AssemblyName = "BOA.Process.Kernel.Card.dll";

                ViewController.OnAssemblyNameChanged(scope);

                itemSourceList.ClassNameList.Count.Should().BeGreaterThan(0);
            }

            // On Class Name Changed
            {
                itemSourceList.MethodNameList.Should().BeNull();

                invocationInfo.ClassName = "BOA.Process.Kernel.Card.CallCenter.IVR.CreditCard.GetCardDetail";

                ViewController.OnClassNameChanged(scope);

                itemSourceList.MethodNameList.Count.Should().BeGreaterThan(0);
            }

            // On Method Name Changed
            {
                scope.TryGet(Keys.MethodDefinition).Should().BeNull();

                invocationInfo.MethodName = "ExecuteInOldCardSystem";

                ViewController.OnMethodNameSelected(scope);
                
                scope.Get(Keys.MethodDefinition).Should().NotBeNull();
            }
        }
        #endregion
    }
}