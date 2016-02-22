using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChangeDetector.Tests
{
    [TestClass]
    public class ChangeDetectionTester
    {
        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldDetectAllChanges()
        {
            var detector = new TestEntityChangeDetector();
            TestEntity original = new TestEntity()
            {
                BooleanValue = false,
                DateTimeValue = new DateTime(2015, 07, 29),
                GuidValue = Guid.NewGuid(),
                IntValue = 123,
                MoneyValue = 12.34m,
                PercentValue = 12.5m,
                StringValue = "Hello"
            };
            TestEntity updated = new TestEntity()
            {
                BooleanValue = true,
                DateTimeValue = new DateTime(2015, 07, 30),
                GuidValue = Guid.NewGuid(),
                IntValue = 234,
                MoneyValue = 10m,
                PercentValue = 15m,
                StringValue = "Goodbye"
            };

            Assert.IsTrue(detector.HasChange(original, updated, e => e.BooleanValue), "Boolean value should be changed.");
            Assert.IsTrue(detector.HasChange(original, updated, e => e.DateTimeValue), "DateTime value should be changed.");
            Assert.IsTrue(detector.HasChange(original, updated, e => e.GuidValue), "Guid value should be changed.");
            Assert.IsTrue(detector.HasChange(original, updated, e => e.IntValue), "Int value should be changed.");
            Assert.IsTrue(detector.HasChange(original, updated, e => e.MoneyValue), "Money value should be changed.");
            Assert.IsTrue(detector.HasChange(original, updated, e => e.PercentValue), "Percent value should be changed.");
            Assert.IsTrue(detector.HasChange(original, updated, e => e.StringValue), "String value should be changed.");

            var changes = detector.GetChanges(original, updated);
            Assert.AreEqual(7, changes.Count(), "The wrong number of changes were detected.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldDetectChange_OriginalNull()
        {
            var detector = new TestEntityChangeDetector();
            TestEntity original = null;
            TestEntity updated = new TestEntity() { StringValue = "After" };

            Assert.IsTrue(detector.HasChange(original, updated, a => a.StringValue), "No change detected for the field.");

            var changes = detector.GetChanges(original, updated);

            Assert.AreEqual(1, changes.Count(), "The wrong number of changes were detected.");
            FieldChange change = changes.Single();
            Assert.AreEqual(TestEntityChangeDetector.StringDescription, change.FieldName, "The wrong field was recorded.");
            Assert.AreEqual(null, change.OldValue, "The old value was not recorded.");
            Assert.AreEqual(Formatters.FormatString(updated.StringValue), change.NewValue, "The new value was not recorded.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldDetectChange_UpdatedNull()
        {
            var detector = new TestEntityChangeDetector();
            TestEntity original = new TestEntity() { StringValue = "After" };
            TestEntity updated = null;

            Assert.IsTrue(detector.HasChange(original, updated, a => a.StringValue), "No change detected for the field.");

            var changes = detector.GetChanges(original, updated);

            Assert.AreEqual(1, changes.Count(), "The wrong number of changes were detected.");
            FieldChange change = changes.Single();
            Assert.AreEqual(TestEntityChangeDetector.StringDescription, change.FieldName, "The wrong field was recorded.");
            Assert.AreEqual(Formatters.FormatString(original.StringValue), change.OldValue, "The old value was not recorded.");
            Assert.AreEqual(null, change.NewValue, "The new value was not recorded.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldNotDetectChange_OriginalAndUpdatedNull()
        {
            var detector = new TestEntityChangeDetector();
            TestEntity original = null;
            TestEntity updated = null;

            Assert.IsFalse(detector.HasChange(original, updated, a => a.StringValue), "Change detected for the field.");

            var changes = detector.GetChanges(original, updated);

            Assert.AreEqual(0, changes.Count(), "The wrong number of changes were detected.");
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

        [TestMethod]
        [TestCategory("Unit Test")]
        [ExpectedException(typeof(ArgumentException))]
        public void ShouldThrowExceptionIfPropertyMissingDescription()
        {
            new BadChangeDetector("    ", e => e.StringValue, Formatters.FormatString);
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionIfAccessorNull()
        {
            new BadChangeDetector("Description", null, Formatters.FormatString);
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionIfFormatterNull()
        {
            new BadChangeDetector("Description", e => e.StringValue, null);
        }

        public class BadChangeDetector : EntityConfiguration<TestEntity>
        {
            public BadChangeDetector(string description, Expression<Func<TestEntity, string>> accessor, Func<string, string> formatter)
            {
                Add(description, accessor, formatter);
            }
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldReturnFalseIfCheckingIfUnconfiguredPropertyChanged()
        {
            var detector = new TestEntityChangeDetector();
            TestEntity original = new TestEntity();
            TestEntity updated = new TestEntity();

            bool hasChange = detector.HasChange(original, updated, a => a.NotConfigured);
            Assert.IsFalse(hasChange, "A change should not be detected if the property is not recognized.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionIfAccessorNull_HasChanges()
        {
            var detector = new TestEntityChangeDetector();
            TestEntity original = new TestEntity();
            TestEntity updated = new TestEntity();

            Expression<Func<TestEntity, int>> accessor = null;
            detector.HasChange(original, updated, accessor);
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        [ExpectedException(typeof(ArgumentException))]
        public void ShouldThrowExceptionIfAccessorRefersToNonMember_HasChanges()
        {
            var detector = new TestEntityChangeDetector();
            TestEntity original = new TestEntity();
            TestEntity updated = new TestEntity();

            detector.HasChange(original, updated, e => 123);
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        [ExpectedException(typeof(ArgumentException))]
        public void ShouldThrowExceptionIfAccessorRefersToNonProperty_HasChanges()
        {
            var detector = new TestEntityChangeDetector();
            TestEntity original = new TestEntity();
            TestEntity updated = new TestEntity();

            detector.HasChange(original, updated, e => e.Field);
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
                    .Add(DerivedDescription, e => e.DerivedValue, Formatters.FormatInt32);
            }
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldIncludeChangeToDerivedProperty()
        {
            var detector = new DerivedChangeDetector();
            DerivedEntity original = new DerivedEntity() { DerivedValue = 123 };
            DerivedEntity updated = new DerivedEntity() { DerivedValue = 234 };

            var changes = detector.GetChanges(original, updated);

            Assert.AreEqual(1, changes.Count(), "The wrong number of changes were detected.");
            FieldChange change = changes.Single();
            Assert.AreEqual(DerivedChangeDetector.DerivedDescription, change.FieldName, "The wrong field was recorded.");
            Assert.AreEqual(Formatters.FormatInt32(original.DerivedValue), change.OldValue, "The old value was not recorded.");
            Assert.AreEqual(Formatters.FormatInt32(updated.DerivedValue), change.NewValue, "The new value was not recorded.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldSeeChangeToDerivedProperty()
        {
            var detector = new DerivedChangeDetector();
            DerivedEntity original = new DerivedEntity() { DerivedValue = 123 };
            DerivedEntity updated = new DerivedEntity() { DerivedValue = 234 };

            bool hasChange = detector.As<DerivedEntity>().HasChange(original, updated, x => x.DerivedValue);

            Assert.IsTrue(hasChange, "The change was not detected.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldSeeChangeToNonDerivedProperty()
        {
            var detector = new DerivedChangeDetector();
            DerivedEntity original = new DerivedEntity() { IntValue = 123 };
            DerivedEntity updated = new DerivedEntity() { IntValue = 234 };

            bool hasChange = detector.As<DerivedEntity>().HasChange(original, updated, x => x.IntValue);

            Assert.IsTrue(hasChange, "The change was not detected.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldDetectChangeToDerivedProperty_OriginalNull()
        {
            var detector = new DerivedChangeDetector();
            DerivedEntity original = null;
            DerivedEntity updated = new DerivedEntity() { DerivedValue = 234 };

            var changes = detector.GetChanges(original, updated);

            Assert.AreEqual(1, changes.Count(), "The wrong number of changes were detected.");
            FieldChange change = changes.Single();
            Assert.AreEqual(DerivedChangeDetector.DerivedDescription, change.FieldName, "The wrong field was recorded.");
            Assert.AreEqual(null, change.OldValue, "The old value was not recorded.");
            Assert.AreEqual(Formatters.FormatInt32(updated.DerivedValue), change.NewValue, "The new value was not recorded.");
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
                    .Add(DoubleDerivedDescription, x => x.DoubleDerivedValue, Formatters.FormatString);
            }
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldIncludeChangeToDoubleDerivedProperty()
        {
            var detector = new DoubleDerivedChangeDetector();
            DoubleDerivedEntity original = new DoubleDerivedEntity() { DoubleDerivedValue = "John" };
            DoubleDerivedEntity updated = new DoubleDerivedEntity() { DoubleDerivedValue = "Tom" };

            var changes = detector.GetChanges(original, updated);

            Assert.AreEqual(1, changes.Count(), "The wrong number of changes were detected.");
            FieldChange change = changes.Single();
            Assert.AreEqual(DoubleDerivedChangeDetector.DoubleDerivedDescription, change.FieldName, "The wrong field was recorded.");
            Assert.AreEqual(Formatters.FormatString(original.DoubleDerivedValue), change.OldValue, "The old value was not recorded.");
            Assert.AreEqual(Formatters.FormatString(updated.DoubleDerivedValue), change.NewValue, "The new value was not recorded.");
        }
    }
}
