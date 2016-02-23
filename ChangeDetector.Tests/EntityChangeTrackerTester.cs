using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChangeDetector.Tests
{
    [TestClass]
    public class EntityChangeTrackerTester
    {
        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldDetectChanges_Modified()
        {
            var detector = new TestEntityChangeDetector();
            EntityChangeTracker<TestEntity> tracker = new EntityChangeTracker<TestEntity>(detector);

            TestEntity entity = new TestEntity()
            {
                BooleanValue = false,
                DateTimeValue = new DateTime(2015, 07, 29),
                GuidValue = Guid.NewGuid(),
                IntValue = 123,
                MoneyValue = 12.34m,
                PercentValue = 12.5m,
                StringValue = "Hello"
            };
            tracker.Attach(entity);

            entity.BooleanValue = true;
            entity.DateTimeValue = new DateTime(2015, 07, 30);
            entity.GuidValue = Guid.NewGuid();
            entity.IntValue = 234;
            entity.MoneyValue = 10m;
            entity.PercentValue = 15m;
            entity.StringValue = "Goodbye";

            var changes = tracker.DetectChanges();
            Assert.AreEqual(1, changes.Count(), "The wrong number of changed entities were detected.");
            var change = changes.Single();
            Assert.AreSame(entity, change.Entity, "The wrong entity was returned.");
            Assert.AreEqual(EntityState.Modified, change.State, "The entity was not marked as modified.");
            Assert.AreEqual(7, change.FieldChanges.Count(), "The wrong number of changes were detected.");

            Assert.IsTrue(change.HasChange(e => e.BooleanValue), "Boolean value should be changed.");
            Assert.IsTrue(change.HasChange(e => e.DateTimeValue), "DateTime value should be changed.");
            Assert.IsTrue(change.HasChange(e => e.GuidValue), "Guid value should be changed.");
            Assert.IsTrue(change.HasChange(e => e.IntValue), "Int value should be changed.");
            Assert.IsTrue(change.HasChange(e => e.MoneyValue), "Money value should be changed.");
            Assert.IsTrue(change.HasChange(e => e.PercentValue), "Percent value should be changed.");
            Assert.IsTrue(change.HasChange(e => e.StringValue), "String value should be changed.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldDetectChanges_Added()
        {
            var detector = new TestEntityChangeDetector();
            EntityChangeTracker<TestEntity> tracker = new EntityChangeTracker<TestEntity>(detector);

            TestEntity entity = new TestEntity()
            {
                BooleanValue = false,
                DateTimeValue = new DateTime(2015, 07, 29),
                GuidValue = Guid.NewGuid(),
                IntValue = 123,
                MoneyValue = 12.34m,
                PercentValue = 12.5m,
                StringValue = "Hello"
            };
            tracker.Add(entity);

            var changes = tracker.DetectChanges();
            Assert.AreEqual(1, changes.Count(), "The wrong number of changed entities were detected.");
            var change = changes.Single();
            Assert.AreSame(entity, change.Entity, "The wrong entity was returned.");
            Assert.AreEqual(EntityState.Added, change.State, "The entity was not marked as modified.");
            Assert.AreEqual(7, change.FieldChanges.Count(), "The wrong number of changes were detected.");

            Assert.IsTrue(change.HasChange(e => e.BooleanValue), "Boolean value should be changed.");
            Assert.IsTrue(change.HasChange(e => e.DateTimeValue), "DateTime value should be changed.");
            Assert.IsTrue(change.HasChange(e => e.GuidValue), "Guid value should be changed.");
            Assert.IsTrue(change.HasChange(e => e.IntValue), "Int value should be changed.");
            Assert.IsTrue(change.HasChange(e => e.MoneyValue), "Money value should be changed.");
            Assert.IsTrue(change.HasChange(e => e.PercentValue), "Percent value should be changed.");
            Assert.IsTrue(change.HasChange(e => e.StringValue), "String value should be changed.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldDetectChanges_Removed()
        {
            var detector = new TestEntityChangeDetector();
            EntityChangeTracker<TestEntity> tracker = new EntityChangeTracker<TestEntity>(detector);

            TestEntity entity = new TestEntity()
            {
                BooleanValue = false,
                DateTimeValue = new DateTime(2015, 07, 29),
                GuidValue = Guid.NewGuid(),
                IntValue = 123,
                MoneyValue = 12.34m,
                PercentValue = 12.5m,
                StringValue = "Hello"
            };
            tracker.Attach(entity);
            tracker.Remove(entity);

            var changes = tracker.DetectChanges();
            Assert.AreEqual(1, changes.Count(), "The wrong number of changed entities were detected.");
            var change = changes.Single();
            Assert.AreSame(entity, change.Entity, "The wrong entity was returned.");
            Assert.AreEqual(EntityState.Removed, change.State, "The entity was not marked as modified.");
            Assert.AreEqual(7, change.FieldChanges.Count(), "The wrong number of changes were detected.");

            Assert.IsTrue(change.HasChange(e => e.BooleanValue), "Boolean value should be changed.");
            Assert.IsTrue(change.HasChange(e => e.DateTimeValue), "DateTime value should be changed.");
            Assert.IsTrue(change.HasChange(e => e.GuidValue), "Guid value should be changed.");
            Assert.IsTrue(change.HasChange(e => e.IntValue), "Int value should be changed.");
            Assert.IsTrue(change.HasChange(e => e.MoneyValue), "Money value should be changed.");
            Assert.IsTrue(change.HasChange(e => e.PercentValue), "Percent value should be changed.");
            Assert.IsTrue(change.HasChange(e => e.StringValue), "String value should be changed.");
        }

        public class TestEntity
        {
            public string StringValue { get; set; }

            public DateTime? DateTimeValue { get; set; }

            public decimal? MoneyValue { get; set; }

            public int? IntValue { get; set; }

            public bool? BooleanValue { get; set; }

            public decimal? PercentValue { get; set; }

            public Guid? GuidValue { get; set; }

            public int NotConfigured { get; set; }

            public int Field;
        }

        public class TestEntityChangeDetector : EntityConfiguration<TestEntity>
        {
            public const string StringDescription = "String";
            public const string DateTimeDescription = "DateTime";
            public const string MoneyDescription = "Money";
            public const string IntDescription = "Int";
            public const string BooleanDescription = "Boolean";
            public const string PercentDescription = "Percent";
            public const string GuidDescription = "Guid";

            public TestEntityChangeDetector()
            {
                Add(StringDescription, e => e.StringValue, Formatters.FormatString);
                Add(DateTimeDescription, e => e.DateTimeValue, Formatters.FormatDateTime);
                Add(MoneyDescription, e => e.MoneyValue, Formatters.FormatMoney);
                Add(IntDescription, e => e.IntValue, Formatters.FormatInt32);
                Add(BooleanDescription, e => e.BooleanValue, Formatters.FormatBoolean);
                Add(PercentDescription, e => e.PercentValue, Formatters.FormatPercent);
                Add(GuidDescription, e => e.GuidValue, Formatters.FormatGuid);
            }
        }
    }
}
