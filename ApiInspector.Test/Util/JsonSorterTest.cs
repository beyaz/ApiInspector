using System.Collections.Generic;
using ApiInspector.Application;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiInspector.Util
{
    [TestClass]
    public class JsonSorterTest
    {
        #region Public Methods
        [TestInitialize]
        public void Initialize()
        {
            BoaAssemblyResolver.AttachToCurrentDomain();
        }

        [TestMethod]
        public void SortJson()
        {
            var input = new JsonSorterSortInput
            {
                JsonFilePath  = @"D:\temp\mobile\CreditAndDebitCardList\Old.json",
                ClassFullName = "BOA.Integration.Model.MobileBranch.CreditAndDebitCardListResponse",
                SortByPropertyMaps = new Dictionary<string, string>
                {
                    {"ClosedCardList", "CreditCardNumber"}
                }
            };
            JsonSorter.Sort(input);
        }
        #endregion
    }
}