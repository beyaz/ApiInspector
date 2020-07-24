using System.Collections.Generic;
using System.Windows.Controls;
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
            var viewController = new ViewController();

            var defaultAssemblySearchDirectory = @"d:\boa\server\bin\";
            var invocationInfo                 = new InvocationInfo {AssemblySearchDirectory = defaultAssemblySearchDirectory};
            var itemSourceList = new ItemSourceList
            {
                AssemblySearchDirectoryList = new List<string> {defaultAssemblySearchDirectory},
                EnvironmentNameList         = new List<string> {"dev", "test"}
            };

            var viewData = new InvocationEditorViewModel
            {
                InvocationInfo = invocationInfo,
                ItemSourceList = itemSourceList,
                Logs = new List<string>()
            };

            // On Search Directory Changed
            {
                itemSourceList.AssemblyNameList.Should().BeNull();

                viewController.OnAssemblySearchDirectoryChanged(viewData);

                itemSourceList.AssemblyNameList.Count.Should().BeGreaterThan(0);
            }

            // On Assembly Name Changed
            {
                itemSourceList.ClassNameList.Should().BeNull();

                invocationInfo.AssemblyName = "BOA.Process.Kernel.Card.dll";

                viewController.OnAssemblyNameChanged(viewData);

                itemSourceList.ClassNameList.Count.Should().BeGreaterThan(0);
            }

            // On Class Name Changed
            {
                itemSourceList.MethodNameList.Should().BeNull();

                invocationInfo.ClassName = "BOA.Process.Kernel.Card.CallCenter.IVR.CreditCard.GetCardDetail";

                viewController.OnClassNameChanged(viewData);

                itemSourceList.MethodNameList.Count.Should().BeGreaterThan(0);
            }

            // On Method Name Changed
            {
                viewData.MethodDefinition.Should().BeNull();

                invocationInfo.MethodName = "ExecuteInOldCardSystem";

                viewController.OnMethodNameSelected(viewData);

                viewData.MethodDefinition.Should().NotBeNull();
            }
        }
        #endregion
    }
}