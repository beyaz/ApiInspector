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
            var invocationInfo = new InvocationInfo
            {
                AssemblySearchDirectory = @"d:\boa\server\bin\"
            };

            var itemSourceList = new ItemSourceList();

            var model = new ViewModel
            {
                InvocationInfo = invocationInfo,
                Trace          = message => { },
                ItemSourceList = itemSourceList
            };
            // TODO: initialize scope
            var viewController = new ViewController(model);

            // On Search Directory Changed
            {
                itemSourceList.AssemblyNameList.Should().BeNull();

                viewController.OnAssemblySearchDirectoryChanged();

                itemSourceList.AssemblyNameList.Count.Should().BeGreaterThan(0);
            }

            // On Assembly Name Changed
            {
                itemSourceList.ClassNameList.Should().BeNull();

                invocationInfo.AssemblyName = "BOA.Process.Kernel.Card.dll";

                viewController.OnAssemblyNameChanged();

                itemSourceList.ClassNameList.Count.Should().BeGreaterThan(0);
            }

            // On Class Name Changed
            {
                itemSourceList.MethodNameList.Should().BeNull();

                invocationInfo.ClassName = "BOA.Process.Kernel.Card.CallCenter.IVR.CreditCard.GetCardDetail";

                viewController.OnClassNameChanged();

                itemSourceList.MethodNameList.Count.Should().BeGreaterThan(0);
            }

            // On Method Name Changed
            {
                model.MethodDefinition.Should().BeNull();

                invocationInfo.MethodName = "ExecuteInOldCardSystem";

                viewController.OnMethodNameSelected();

                model.MethodDefinition.Should().NotBeNull();
            }
        }
        #endregion
    }
}