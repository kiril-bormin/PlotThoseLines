using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlotThoseLines;
using PlotThoseLines.Services;
using System;
using System.Collections.Generic;

namespace PlotThoseLines.Tests
{
    [TestClass]
    public class SeriesMergeTests
    {
        [TestMethod]
        public void Merge_OverlappingDates_ReplacesOldPointsWithNewPoints()
        {
            var oldPoints = new List<SavedPoint>
            {
                new SavedPoint { Timestamp = new DateTime(2024, 1, 1), Value = 10 },
                new SavedPoint { Timestamp = new DateTime(2024, 1, 2), Value = 20 }
            };
            var newPoints = new List<SavedPoint>
            {
                new SavedPoint { Timestamp = new DateTime(2024, 1, 2), Value = 99 },
                new SavedPoint { Timestamp = new DateTime(2024, 1, 3), Value = 30 }
            };

            var result = SeriesMerge.Merge(oldPoints, newPoints);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(10, result[0].Value);
            Assert.AreEqual(99, result[1].Value);
            Assert.AreEqual(30, result[2].Value);
        }

        [TestMethod]
        public void Merge_EmptyOldPoints_ReturnsNewPoints()
        {
            var oldPoints = new List<SavedPoint>();
            var newPoints = new List<SavedPoint>
            {
                new SavedPoint { Timestamp = new DateTime(2024, 1, 1), Value = 50 }
            };

            var result = SeriesMerge.Merge(oldPoints, newPoints);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(50, result[0].Value);
        }

        [TestMethod]
        public void Merge_EmptyNewPoints_ReturnsOldPoints()
        {
            var oldPoints = new List<SavedPoint>
            {
                new SavedPoint { Timestamp = new DateTime(2024, 1, 1), Value = 50 }
            };
            var newPoints = new List<SavedPoint>();

            var result = SeriesMerge.Merge(oldPoints, newPoints);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(50, result[0].Value);
        }
    }
}