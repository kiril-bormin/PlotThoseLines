using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlotThoseLines.Services;
using System.IO;

namespace PlotThoseLines.Tests
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void Parse_ValidInput_ReturnsPoints()
        {
            var lines = new[] { "date,market_cap", "2024-01-01,100" };

            var result = Parser.Parse(lines);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(100.0, result[0].Value);
        }

        [TestMethod]
        public void Parse_NotEnoughLines_ThrowsException()
        {
            var lines = new[] { "date,market_cap" };

            Assert.ThrowsException<InvalidDataException>(() => Parser.Parse(lines));
        }

        [TestMethod]
        public void Parse_MissingPriceColumn_ThrowsException()
        {
            var lines = new[] { "date,other", "2024-01-01,100" };

            Assert.ThrowsException<InvalidDataException>(() => Parser.Parse(lines));
        }
    }
}