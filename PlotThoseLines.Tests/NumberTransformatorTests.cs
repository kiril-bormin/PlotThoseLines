using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlotThoseLines.Services;
using PlotThoseLines;

namespace PlotThoseLines.Tests
{
    [TestClass]
    public class NumberTransformatorTests
    {
        [TestMethod]
        public void Format_Trillions_ReturnsFormattedStringT()
        {
            // Arrange
            double value = 2_500_000_000_000;

            // Act
            string result = NumberFormatter.Format(value);

            // Assert (vérifie le suffixe et la valeur numérique)
            Assert.IsTrue(result.EndsWith(" T"));
            StringAssert.StartsWith(result, "2");
        }
        [TestMethod]
        public void Format_Milliards_ReturnsFormattedStringWithMd()
        {
            double value = 15_000_000_000; 
            string result = NumberFormatter.Format(value);

            Assert.AreEqual("15 Md", result);
        }
        [TestMethod]
        public void Format_SeuilExactUnMillion_Returns1M()
        {
            double value = 1_000_000;
            string result = NumberFormatter.Format(value);

            Assert.AreEqual("1 M", result);
        }
        [TestMethod]
        public void Format_PetiteValeur_NeContientAucunSuffixe()
        {
            double value = 500;
            string result = NumberFormatter.Format(value);

            Assert.AreEqual("500", result);
        }
    }
}