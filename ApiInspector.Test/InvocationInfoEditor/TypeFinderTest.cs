using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static ApiInspector.InvocationInfoEditor.TypeFinder;

namespace ApiInspector.InvocationInfoEditor
{
    [TestClass]
    public class TypeFinderTest
    {
        #region Public Methods
        [TestMethod]
        public void Find()
        {
            FindType("BOA.Types.InternetBanking.WebCreditCardRequest").Should().NotBeNull();
            FindType("BOA.Card.Contracts.CreditCard.Limit.ChangeCorporateCardLimitRequest").Should().NotBeNull();
        }
        #endregion
    }
}