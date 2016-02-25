using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChangeDetector.Tests
{
    [TestClass]
    public class ChangeTrackingTester
    {
        #region Attach

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
        public void ShouldLeaveUnmodifedAfterAttachingAddedEntity()
        {
            var detector = new TestEntityChangeDetector();
            EntityChangeTracker<TestEntity> tracker = new EntityChangeTracker<TestEntity>(detector);

            TestEntity entity = new TestEntity();
            tracker.Add(entity);
            tracker.Attach(entity);

            var change = tracker.DetectChanges(entity);
            Assert.AreEqual(EntityState.Unmodified, change.State, "Attaching an Added entity should make it Unmodified.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldLeaveUnmodifedAfterAttachingRemovedEntity()
        {
            var detector = new TestEntityChangeDetector();
            EntityChangeTracker<TestEntity> tracker = new EntityChangeTracker<TestEntity>(detector);

            TestEntity entity = new TestEntity();
            tracker.Remove(entity);
            tracker.Attach(entity);

            var change = tracker.DetectChanges(entity);
            Assert.AreEqual(EntityState.Unmodified, change.State, "Attaching a Removed entity should make it Unmodified.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldRetainModificationIfAttachedTwice()
        {
            var detector = new TestEntityChangeDetector();
            EntityChangeTracker<TestEntity> tracker = new EntityChangeTracker<TestEntity>(detector);

            TestEntity entity = new TestEntity() { IntValue = 123 };
            tracker.Attach(entity);
            var before = tracker.DetectChanges(entity);
            Assert.AreEqual(EntityState.Unmodified, before.State, "The entity should have been Unmodified to start with.");

            entity.IntValue = 234;

            tracker.Attach(entity); // Should not wipe out changes
            var after = tracker.DetectChanges(entity);
            Assert.AreEqual(EntityState.Modified, after.State, "The entity should have become Modified.");
        }

        #endregion

        #region Detach

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldDetachAddedEntity()
        {
            var detector = new TestEntityChangeDetector();
            EntityChangeTracker<TestEntity> tracker = new EntityChangeTracker<TestEntity>(detector);

            TestEntity entity = new TestEntity();
            tracker.Add(entity);
            tracker.Detach(entity);

            var change = tracker.DetectChanges(entity);
            Assert.AreEqual(EntityState.Detached, change.State, "Detaching an Added entity should make it Detached.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldDetachRemovedEntity()
        {
            var detector = new TestEntityChangeDetector();
            EntityChangeTracker<TestEntity> tracker = new EntityChangeTracker<TestEntity>(detector);

            TestEntity entity = new TestEntity();
            tracker.Attach(entity);
            tracker.Remove(entity);
            tracker.Detach(entity);

            var change = tracker.DetectChanges(entity);
            Assert.AreEqual(EntityState.Detached, change.State, "Detaching a Removed entity should make it Detached.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldDetachUnmodifiedEntity()
        {
            var detector = new TestEntityChangeDetector();
            EntityChangeTracker<TestEntity> tracker = new EntityChangeTracker<TestEntity>(detector);

            TestEntity entity = new TestEntity();
            tracker.Attach(entity);
            tracker.Detach(entity);

            var change = tracker.DetectChanges(entity);
            Assert.AreEqual(EntityState.Detached, change.State, "Detaching an Unmodified entity should make it Detached.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldLeaveDetachedEntityDetached()
        {
            var detector = new TestEntityChangeDetector();
            EntityChangeTracker<TestEntity> tracker = new EntityChangeTracker<TestEntity>(detector);

            TestEntity entity = new TestEntity();
            // Do not attach the entit, so Detach does nothing
            Assert.IsFalse(tracker.Detach(entity), "Should not be indicating the item was detached.");

            var change = tracker.DetectChanges(entity);
            Assert.AreEqual(EntityState.Detached, change.State, "Detaching a Detached entity should leave it Detached.");
        }

        #endregion

        #region Add

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
        public void ShouldDetectMostRecentChangesForAdded()
        {
            var detector = new TestEntityChangeDetector();
            EntityChangeTracker<TestEntity> tracker = new EntityChangeTracker<TestEntity>(detector);

            TestEntity entity = new TestEntity() { IntValue = 123 };
            tracker.Add(entity);

            entity.IntValue = 234;

            var after = tracker.DetectChanges(entity);
            Assert.AreEqual(EntityState.Added, after.State, "The entity should have remained Added.");
            Assert.IsTrue(after.HasChange(x => x.IntValue), "The IntValue should have changed.");
            var change = after.GetChange(x => x.IntValue);
            Assert.IsNotNull(change, "The change was null after detecting its prescence.");
            Assert.AreEqual(234, change.UpdatedValue, "The latest value was not returned.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldRemainAddedAfterAddingTwice()
        {
            var detector = new TestEntityChangeDetector();
            EntityChangeTracker<TestEntity> tracker = new EntityChangeTracker<TestEntity>(detector);

            TestEntity entity = new TestEntity();
            tracker.Add(entity);
            var before = tracker.DetectChanges(entity);
            Assert.AreEqual(EntityState.Added, before.State, "The entity should have been Added to start with.");

            tracker.Add(entity);
            var after = tracker.DetectChanges(entity);
            Assert.AreEqual(EntityState.Added, after.State, "The entity should have remained Added.");
        }

        /// <summary>
        /// The only way an entity would be Removed is if it was at one
        /// point Attached (Unmodified/Modified). If you Remove an Added
        /// entity, it becomes Detached instead. Thus, re-adding it
        /// is the same as un-removing it, or putting it back into
        /// an (Unmodified/Modified) state.
        /// </summary>
        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldLeaveUnmodifiedWhenAddingRemovedEntity()
        {
            var detector = new TestEntityChangeDetector();
            EntityChangeTracker<TestEntity> tracker = new EntityChangeTracker<TestEntity>(detector);

            TestEntity entity = new TestEntity();
            tracker.Attach(entity);
            tracker.Remove(entity);
            tracker.Add(entity);

            var change = tracker.DetectChanges(entity);
            Assert.AreEqual(EntityState.Unmodified, change.State, "Adding a Removed entity should make it Unmodified.");
        }

        /// <summary>
        /// The only way an entity would be Removed is if it was at one
        /// point Attached (Unmodified/Modified). If you Remove an Added
        /// entity, it becomes Detached instead. Thus, re-adding it
        /// is the same as un-removing it, or putting it back into
        /// an (Unmodified/Modified) state.
        /// </summary>
        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldLeaveModifiedWhenAddingRemovedEntity()
        {
            var detector = new TestEntityChangeDetector();
            EntityChangeTracker<TestEntity> tracker = new EntityChangeTracker<TestEntity>(detector);

            TestEntity entity = new TestEntity() { IntValue = 123 };
            tracker.Attach(entity);
            tracker.Remove(entity);
            tracker.Add(entity);

            entity.IntValue = 234;

            var change = tracker.DetectChanges(entity);
            Assert.AreEqual(EntityState.Modified, change.State, "Adding a Removed entity that was changed should make it Modified.");
            Assert.IsTrue(change.HasChange(x => x.IntValue), "The modified value does not reflect the latest value.");
        }

        #endregion

        #region Remove

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

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldDetectMostRecentChangesForRemoved()
        {
            var detector = new TestEntityChangeDetector();
            EntityChangeTracker<TestEntity> tracker = new EntityChangeTracker<TestEntity>(detector);

            TestEntity entity = new TestEntity() { IntValue = 123 };
            tracker.Attach(entity);
            tracker.Remove(entity);

            entity.IntValue = 234;

            var change = tracker.DetectChanges(entity);
            Assert.AreEqual(EntityState.Removed, change.State, "The entity should have remained Added.");
            Assert.IsTrue(change.HasChange(x => x.IntValue), "The IntValue should have changed.");
            
            var fieldChange = change.GetChange(x => x.IntValue);
            Assert.IsNotNull(fieldChange, "The change was null after detecting its prescence.");
            Assert.AreEqual(234, fieldChange.OriginalValue, "The latest value was not returned.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldDetatchAddedEntitiesThatAreRemoved()
        {
            var detector = new TestEntityChangeDetector();
            EntityChangeTracker<TestEntity> tracker = new EntityChangeTracker<TestEntity>(detector);

            TestEntity entity = new TestEntity();
            tracker.Add(entity);
            tracker.Remove(entity);

            var change = tracker.DetectChanges(entity);
            Assert.AreEqual(EntityState.Detached, change.State, "The entity should have been Detached.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldRemainRemovedAfterRemovingTwice()
        {
            var detector = new TestEntityChangeDetector();
            EntityChangeTracker<TestEntity> tracker = new EntityChangeTracker<TestEntity>(detector);

            TestEntity entity = new TestEntity();
            tracker.Attach(entity);
            tracker.Remove(entity);
            var before = tracker.DetectChanges(entity);
            Assert.AreEqual(EntityState.Removed, before.State, "The entity should have been Removed to start with.");

            tracker.Remove(entity);
            var after = tracker.DetectChanges(entity);
            Assert.AreEqual(EntityState.Removed, after.State, "The entity should have remained Removed.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldLeaveDetachedIfRemovingDetachedEntity()
        {
            var detector = new TestEntityChangeDetector();
            EntityChangeTracker<TestEntity> tracker = new EntityChangeTracker<TestEntity>(detector);

            TestEntity entity = new TestEntity();
            Assert.IsFalse(tracker.Remove(entity), "Should not have indicated the entity was removed when it was Detached.");
        }

        #endregion

        #region DetectChanges

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldDetectModifiedByDefault()
        {
            var detector = new TestEntityChangeDetector();
            EntityChangeTracker<TestEntity> tracker = new EntityChangeTracker<TestEntity>(detector);

            TestEntity entity = new TestEntity() { IntValue = 1234 };
            tracker.Attach(entity);
            entity.IntValue = 234;

            var changes = tracker.DetectChanges();
            Assert.AreEqual(1, changes.Count(), "The entity should have been seen as changed.");
            var change = changes.Single();
            Assert.AreSame(entity, change.Entity, "The wrong entity was returned.");
            Assert.AreEqual(EntityState.Modified, change.State, "The entity was not modified.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldDetectAddedByDefault()
        {
            var detector = new TestEntityChangeDetector();
            EntityChangeTracker<TestEntity> tracker = new EntityChangeTracker<TestEntity>(detector);

            TestEntity entity = new TestEntity();
            tracker.Add(entity);

            var changes = tracker.DetectChanges();
            Assert.AreEqual(1, changes.Count(), "The entity should have been seen as changed.");
            var change = changes.Single();
            Assert.AreSame(entity, change.Entity, "The wrong entity was returned.");
            Assert.AreEqual(EntityState.Added, change.State, "The entity was not added.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldDetectRemovedByDefault()
        {
            var detector = new TestEntityChangeDetector();
            EntityChangeTracker<TestEntity> tracker = new EntityChangeTracker<TestEntity>(detector);

            TestEntity entity = new TestEntity();
            tracker.Attach(entity);
            tracker.Remove(entity);

            var changes = tracker.DetectChanges();
            Assert.AreEqual(1, changes.Count(), "The entity should have been seen as changed.");
            var change = changes.Single();
            Assert.AreSame(entity, change.Entity, "The wrong entity was returned.");
            Assert.AreEqual(EntityState.Removed, change.State, "The entity was not removed.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldAllowRetrievingUnmodifiedEntities()
        {
            var detector = new TestEntityChangeDetector();
            EntityChangeTracker<TestEntity> tracker = new EntityChangeTracker<TestEntity>(detector);

            TestEntity entity = new TestEntity();
            tracker.Attach(entity);

            var changes = tracker.DetectChanges(EntityState.Unmodified);
            Assert.AreEqual(1, changes.Count(), "The entity should have been seen as changed.");
            var change = changes.Single();
            Assert.AreSame(entity, change.Entity, "The wrong entity was returned.");
            Assert.AreEqual(EntityState.Unmodified, change.State, "The entity was not unmodified.");
        }

        #endregion

        #region CommitChanges

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldNotDetectChangesAfterCommitting()
        {
            var detector = new TestEntityChangeDetector();
            EntityChangeTracker<TestEntity> tracker = new EntityChangeTracker<TestEntity>(detector);

            TestEntity entity = new TestEntity() { IntValue = 1234 };
            tracker.Attach(entity);
            entity.IntValue = 234;  // Should make the entity Modified

            tracker.CommitChanges();
            var change = tracker.DetectChanges(entity);
            Assert.AreEqual(EntityState.Unmodified, change.State, "The entity should be Unmodified after committing.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldNotDetectAddedAfterCommitting()
        {
            var detector = new TestEntityChangeDetector();
            EntityChangeTracker<TestEntity> tracker = new EntityChangeTracker<TestEntity>(detector);

            TestEntity entity = new TestEntity();
            tracker.Add(entity);

            tracker.CommitChanges();
            var change = tracker.DetectChanges(entity);
            Assert.AreEqual(EntityState.Unmodified, change.State, "The entity should be Unmodified after committing.");
        }

        [TestMethod]
        [TestCategory("Unit Test")]
        public void ShouldMarkRemovedAsDetachedAfterCommitting()
        {
            var detector = new TestEntityChangeDetector();
            EntityChangeTracker<TestEntity> tracker = new EntityChangeTracker<TestEntity>(detector);

            TestEntity entity = new TestEntity();
            tracker.Attach(entity);
            tracker.Remove(entity);

            tracker.CommitChanges();
            var change = tracker.DetectChanges(entity);
            Assert.AreEqual(EntityState.Detached, change.State, "The entity should be Detached after committing.");
        }

        #endregion

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
