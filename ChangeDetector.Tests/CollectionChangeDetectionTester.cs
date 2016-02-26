using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChangeDetector.Tests
{
    [TestClass]
    public class CollectionChangeDetectionTester
    {
        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldDetectItemsAdded()
        {
            CollectionChangeDetector<int> detector = new CollectionChangeDetector<int>();
            var changes = detector.GetChanges(new int[0], new int[] { 1 });
            Assert.AreEqual(1, changes.Count, "There should have only been one change.");
            var change = changes.Single();
            Assert.AreEqual(1, change.Item, "The wrong item was returned.");
            Assert.AreEqual(ElementState.Added, change.State, "The item should have been added.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldDetectItemsRemoved()
        {
            CollectionChangeDetector<int> detector = new CollectionChangeDetector<int>();
            var changes = detector.GetChanges(new int[] { 1 }, new int[0]);
            Assert.AreEqual(1, changes.Count, "There should have only been one change.");
            var change = changes.Single();
            Assert.AreEqual(1, change.Item, "The wrong item was returned.");
            Assert.AreEqual(ElementState.Removed, change.State, "The item should have been removed.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldDetectItemsUnmodified()
        {
            CollectionChangeDetector<int> detector = new CollectionChangeDetector<int>();
            var changes = detector.GetChanges(new int[] { 1 }, new int[] { 1 }, ElementState.Unmodified);
            Assert.AreEqual(1, changes.Count, "There should have only been one change.");
            var change = changes.Single();
            Assert.AreEqual(1, change.Item, "The wrong item was returned.");
            Assert.AreEqual(ElementState.Unmodified, change.State, "The item should have been unmodified.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldReturnNothingIfSearchingForDetached()
        {
            CollectionChangeDetector<int> detector = new CollectionChangeDetector<int>();
            var changes = detector.GetChanges(new int[] { 1 }, new int[0], ElementState.Detached);
            Assert.AreEqual(0, changes.Count, "There should have only been one change.");
        }
    }
}
