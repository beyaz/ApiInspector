using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiInspector.InvocationInfoEditor
{
    [TestClass]
    public class TypeFinderTest
    {
        #region Public Methods
        [TestMethod]
        public void Find()
        {
            var typeFinder = new TypeFinder();

            typeFinder.Find("BOA.Types.InternetBanking.WebCreditCardRequest").Should().NotBeNull();
        }
        #endregion
    }
}