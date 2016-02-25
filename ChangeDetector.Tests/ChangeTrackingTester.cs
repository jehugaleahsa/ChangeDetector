using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChangeDetector.Tests
{
    [TestClass]
    public class ChangeTrackingTester
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
                Add(e => e.StringValue, StringDescription, Formatters.FormatString);
                Add(e => e.DateTimeValue, DateTimeDescription, Formatters.FormatDateTime);
                Add(e => e.MoneyValue, MoneyDescription, Formatters.FormatMoney);
                Add(e => e.IntValue, IntDescription, Formatters.FormatInt32);
                Add(e => e.BooleanValue, BooleanDescription, Formatters.FormatBoolean);
                Add(e => e.PercentValue, PercentDescription, Formatters.FormatPercent);
                Add(e => e.GuidValue, GuidDescription, Formatters.FormatGuid);
            }
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldIncludeChangeToDerivedProperty()
        {
            var detector = new DerivedChangeDetector();
            var tracker = new EntityChangeTracker<TestEntity>(detector);
            DerivedEntity entity = new DerivedEntity() { DerivedValue = 123 };
            tracker.Attach(entity);

            entity.DerivedValue = 234;

            var changes = tracker.DetectChanges();

            Assert.AreEqual(1, changes.Count(), "The wrong number of changes were detected.");
            EntityChange<TestEntity> change = changes.Single();
            Assert.AreSame(entity, change.Entity, "A change was detected on the wrong entity.");
            Assert.AreEqual(EntityState.Modified, change.State, "The entity should have been modified.");

            Assert.AreEqual(1, change.FieldChanges.Count(), "The wrong number of fields were seen as changed.");
            var fieldChange = change.FieldChanges.Single();
            Assert.AreEqual(DerivedChangeDetector.DerivedDescription, fieldChange.FieldName, "The wrong field was recorded.");

            bool hasChange = change.As<DerivedEntity>().HasChange(x => x.DerivedValue);
            Assert.IsTrue(hasChange, "The change was not detected.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldSeeChangeToBasePropertyWhenBaseObject()
        {
            var detector = new DerivedChangeDetector();
            var tracker = new EntityChangeTracker<TestEntity>(detector);
            DerivedEntity entity = new DerivedEntity() { IntValue = 123 };
            tracker.Attach(entity);

            entity.IntValue = 234;

            var changes = tracker.DetectChanges();

            Assert.AreEqual(1, changes.Count(), "The wrong number of changes were detected.");
            EntityChange<TestEntity> change = changes.Single();
            Assert.AreSame(entity, change.Entity, "A change was detected on the wrong entity.");
            Assert.AreEqual(EntityState.Modified, change.State, "The entity should have been modified.");

            Assert.AreEqual(1, change.FieldChanges.Count(), "The wrong number of fields were seen as changed.");
            var fieldChange = change.FieldChanges.Single();
            Assert.AreEqual(TestEntityChangeDetector.IntDescription, fieldChange.FieldName, "The wrong field was recorded.");

            bool hasBaseChange = change.HasChange(x => x.IntValue);
            Assert.IsTrue(hasBaseChange, "The change was not detected.");

            bool hasDerivedChange = change.As<DerivedEntity>().HasChange(x => x.IntValue);
            Assert.IsTrue(hasDerivedChange, "The change was not detected.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldDetectChangeToDerivedPropertyWhenAdding()
        {
            var detector = new DerivedChangeDetector();
            var tracker = new EntityChangeTracker<TestEntity>(detector);
            DerivedEntity entity = new DerivedEntity() { DerivedValue = 123 };
            tracker.Add(entity);

            var changes = tracker.DetectChanges();

            Assert.AreEqual(1, changes.Count(), "The wrong number of changes were detected.");
            EntityChange<TestEntity> change = changes.Single();
            Assert.AreSame(entity, change.Entity, "A change was detected on the wrong entity.");
            Assert.AreEqual(EntityState.Added, change.State, "The entity should have been modified.");
            Assert.AreEqual(1, change.FieldChanges.Count(), "The wrong number of fields were seen as changed.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldDetectChangeToDerivedPropertyWhenRemoving()
        {
            var detector = new DerivedChangeDetector();
            var tracker = new EntityChangeTracker<TestEntity>(detector);
            DerivedEntity entity = new DerivedEntity() { DerivedValue = 123 };
            tracker.Attach(entity);
            tracker.Remove(entity);

            var changes = tracker.DetectChanges();

            Assert.AreEqual(1, changes.Count(), "The wrong number of changes were detected.");
            EntityChange<TestEntity> change = changes.Single();
            Assert.AreSame(entity, change.Entity, "A change was detected on the wrong entity.");
            Assert.AreEqual(EntityState.Removed, change.State, "The entity should have been modified.");
            Assert.AreEqual(1, change.FieldChanges.Count(), "The wrong number of fields were seen as changed.");
        }

        public class DerivedEntity : TestEntity
        {
            public int? DerivedValue { get; set; }
        }

        public class DerivedChangeDetector : TestEntityChangeDetector
        {
            public const string DerivedDescription = "Derived";

            public DerivedChangeDetector()
            {
                When<DerivedEntity>()
                    .Add(e => e.DerivedValue, DerivedDescription, Formatters.FormatInt32);
            }
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldIncludeChangeToDoubleDerivedProperty()
        {
            var detector = new DoubleDerivedChangeDetector();
            var tracker = new EntityChangeTracker<TestEntity>(detector);
            DoubleDerivedEntity entity = new DoubleDerivedEntity() { DoubleDerivedValue = "John" };
            tracker.Attach(entity);

            entity.DoubleDerivedValue = "Tom";

            var changes = tracker.DetectChanges();

            Assert.AreEqual(1, changes.Count(), "The wrong number of changes were detected.");
            EntityChange<TestEntity> change = changes.Single();
            Assert.AreSame(entity, change.Entity, "A change was detected on the wrong entity.");
            Assert.AreEqual(EntityState.Modified, change.State, "The entity should have been modified.");
            Assert.AreEqual(1, change.FieldChanges.Count(), "The wrong number of fields were seen as changed.");
        }

        public class DoubleDerivedEntity : DerivedEntity
        {
            public string DoubleDerivedValue { get; set; }
        }

        public class DoubleDerivedChangeDetector : DerivedChangeDetector
        {
            public const string DoubleDerivedDescription = "DoubleDerived";

            public DoubleDerivedChangeDetector()
            {
                When<DoubleDerivedEntity>()
                    .Add(x => x.DoubleDerivedValue, DoubleDerivedDescription, Formatters.FormatString);
            }
        }
    }
}
