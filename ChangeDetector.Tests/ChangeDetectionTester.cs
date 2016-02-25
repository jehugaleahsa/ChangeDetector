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

            Assert.IsTrue(detector.HasChange(e => e.BooleanValue, original, updated), "Boolean value should be changed.");
            Assert.IsTrue(detector.HasChange(e => e.DateTimeValue, original, updated), "DateTime value should be changed.");
            Assert.IsTrue(detector.HasChange(e => e.GuidValue, original, updated), "Guid value should be changed.");
            Assert.IsTrue(detector.HasChange(e => e.IntValue, original, updated), "Int value should be changed.");
            Assert.IsTrue(detector.HasChange(e => e.MoneyValue, original, updated), "Money value should be changed.");
            Assert.IsTrue(detector.HasChange(e => e.PercentValue, original, updated), "Percent value should be changed.");
            Assert.IsTrue(detector.HasChange(e => e.StringValue, original, updated), "String value should be changed.");

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

            Assert.IsTrue(detector.HasChange(a => a.StringValue, original, updated), "No change detected for the field.");

            var changes = detector.GetChanges(original, updated);

            Assert.AreEqual(1, changes.Count(), "The wrong number of changes were detected.");
            IFieldChange change = changes.Single();
            Assert.AreEqual(TestEntityChangeDetector.StringDescription, change.FieldName, "The wrong field was recorded.");
            Assert.AreEqual(null, change.FormatOriginalValue(), "The old value was not recorded.");
            Assert.AreEqual(Formatters.FormatString(updated.StringValue), change.FormatUpdatedValue(), "The new value was not recorded.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldDetectChange_UpdatedNull()
        {
            var detector = new TestEntityChangeDetector();
            TestEntity original = new TestEntity() { StringValue = "After" };
            TestEntity updated = null;

            Assert.IsTrue(detector.HasChange(a => a.StringValue, original, updated), "No change detected for the field.");

            var changes = detector.GetChanges(original, updated);

            Assert.AreEqual(1, changes.Count(), "The wrong number of changes were detected.");
            IFieldChange change = changes.Single();
            Assert.AreEqual(TestEntityChangeDetector.StringDescription, change.FieldName, "The wrong field was recorded.");
            Assert.AreEqual(Formatters.FormatString(original.StringValue), change.FormatOriginalValue(), "The old value was not recorded.");
            Assert.AreEqual(null, change.FormatUpdatedValue(), "The new value was not recorded.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldNotDetectChange_OriginalAndUpdatedNull()
        {
            var detector = new TestEntityChangeDetector();
            TestEntity original = null;
            TestEntity updated = null;

            Assert.IsFalse(detector.HasChange(a => a.StringValue, original, updated), "Change detected for the field.");

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
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionIfAccessorNull()
        {
            new BadChangeDetector("Description", null, Formatters.FormatString);
        }

        public class BadChangeDetector : EntityConfiguration<TestEntity>
        {
            public BadChangeDetector(string description, Expression<Func<TestEntity, string>> accessor, Func<string, string> formatter)
            {
                Add(accessor, description, formatter);
            }
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldReturnFalseIfCheckingIfUnconfiguredPropertyChanged()
        {
            var detector = new TestEntityChangeDetector();
            TestEntity original = new TestEntity();
            TestEntity updated = new TestEntity();

            bool hasChange = detector.HasChange(a => a.NotConfigured, original, updated);
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
            detector.HasChange(accessor, original, updated);
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        [ExpectedException(typeof(ArgumentException))]
        public void ShouldThrowExceptionIfAccessorRefersToNonMember_HasChanges()
        {
            var detector = new TestEntityChangeDetector();
            TestEntity original = new TestEntity();
            TestEntity updated = new TestEntity();

            detector.HasChange(e => 123, original, updated);
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        [ExpectedException(typeof(ArgumentException))]
        public void ShouldThrowExceptionIfAccessorRefersToNonProperty_HasChanges()
        {
            var detector = new TestEntityChangeDetector();
            TestEntity original = new TestEntity();
            TestEntity updated = new TestEntity();

            detector.HasChange(e => e.Field, original, updated);
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
            IFieldChange change = changes.Single();
            Assert.AreEqual(DerivedChangeDetector.DerivedDescription, change.FieldName, "The wrong field was recorded.");
            Assert.AreEqual(Formatters.FormatInt32(original.DerivedValue), change.FormatOriginalValue(), "The old value was not recorded.");
            Assert.AreEqual(Formatters.FormatInt32(updated.DerivedValue), change.FormatUpdatedValue(), "The new value was not recorded.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldSeeChangeToDerivedProperty()
        {
            var detector = new DerivedChangeDetector();
            DerivedEntity original = new DerivedEntity() { DerivedValue = 123 };
            DerivedEntity updated = new DerivedEntity() { DerivedValue = 234 };

            bool hasChange = detector.As<DerivedEntity>().HasChange(x => x.DerivedValue, original, updated);

            Assert.IsTrue(hasChange, "The change was not detected.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldSeeChangeToBasePropertyWhenBaseObject()
        {
            var detector = new DerivedChangeDetector();
            TestEntity original = new DerivedEntity() { IntValue = 123 };
            TestEntity updated = new DerivedEntity() { IntValue = 234 };

            bool hasChange = detector.As<DerivedEntity>().HasChange(x => x.IntValue, original, updated);

            Assert.IsTrue(hasChange, "The change was not detected.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldSeeChangeToDerivedPropertyWhenBaseObject()
        {
            var detector = new DerivedChangeDetector();
            TestEntity original = new DerivedEntity() { DerivedValue = 123 };
            TestEntity updated = new DerivedEntity() { DerivedValue = 234 };

            bool hasChange = detector.As<DerivedEntity>().HasChange(x => x.DerivedValue, original, updated);

            Assert.IsTrue(hasChange, "The change was not detected.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldSeeChangeToNonDerivedProperty()
        {
            var detector = new DerivedChangeDetector();
            DerivedEntity original = new DerivedEntity() { IntValue = 123 };
            DerivedEntity updated = new DerivedEntity() { IntValue = 234 };

            bool hasChange = detector.As<DerivedEntity>().HasChange(x => x.IntValue, original, updated);

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
            IFieldChange change = changes.Single();
            Assert.AreEqual(DerivedChangeDetector.DerivedDescription, change.FieldName, "The wrong field was recorded.");
            Assert.AreEqual(null, change.FormatOriginalValue(), "The old value was not recorded.");
            Assert.AreEqual(Formatters.FormatInt32(updated.DerivedValue), change.FormatUpdatedValue(), "The new value was not recorded.");
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
            DoubleDerivedEntity original = new DoubleDerivedEntity() { DoubleDerivedValue = "John" };
            DoubleDerivedEntity updated = new DoubleDerivedEntity() { DoubleDerivedValue = "Tom" };

            var changes = detector.GetChanges(original, updated);

            Assert.AreEqual(1, changes.Count(), "The wrong number of changes were detected.");
            IFieldChange change = changes.Single();
            Assert.AreEqual(DoubleDerivedChangeDetector.DoubleDerivedDescription, change.FieldName, "The wrong field was recorded.");
            Assert.AreEqual(Formatters.FormatString(original.DoubleDerivedValue), change.FormatOriginalValue(), "The old value was not recorded.");
            Assert.AreEqual(Formatters.FormatString(updated.DoubleDerivedValue), change.FormatUpdatedValue(), "The new value was not recorded.");
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
