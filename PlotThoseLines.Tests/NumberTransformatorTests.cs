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
            double value = 2_500_000_000_000; // 2.5 T

            // Act
            string result = NumberFormatter.Format(value);

            // Assert (vérifie le suffixe et la valeur numérique)
            Assert.IsTrue(result.EndsWith(" T"));
            StringAssert.StartsWith(result, "2");
        }
    }
}