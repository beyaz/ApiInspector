using System.Collections.Generic;
using ApiInspector.Application;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiInspector.Util
{
    [TestClass]
    public class JsonSorterTest
    {
        #region Public Methods
        [TestMethod]
        public void SortJson()
        {
            JsonSorter.Sort(@"D:\temp\mobile\CreditAndDebitCardList\Old.json", "BOA.Integration.Model.MobileBranch.CreditAndDebitCardListResponse", new Dictionary<string, string>
            {
                {"ClosedCardList", "CreditCardNumber"}
            });
        }
        #endregion

        #region Methods
        [TestInitialize]
      public  void Initialize()
        {
            BoaAssemblyResolver.AttachToCurrentDomain();
        }
        #endregion
    }
}