# ChangeDetector

Easily detect differences between objects.

Download using NuGet: [ChangeDetector](http://nuget.org/packages/ChangeDetector)

## Overview
ChangeDetector will take two objects of the same type and determine if they differ. It also lets you take snapshots of an object over its lifetime to see if and how it has changed. This is useful in scenarios where you need to track changes but do not want to complicate your business logic with recording every little update. Instead, you simply define which properties you want to track and register the entity with a tracker. After your business logic has run through, you can ask the tracker if any of those properties changed. You could use this to determine if an external system needs updated or even use it build up SQL commands inside of a homespun ORM -- there are many applications.

In order to track changes, you must create an `EntityConfiguration` for each entity you wish to track. This class will indicate which entity properties are of interest and how you wish to display them. You can define an entity configuration by simply creating a subclass of `EntityConfiguration<TEntity>`:

    public class TestEntityChangeDetector : EntityConfiguration<TestEntity>
    {
        public TestEntityChangeDetector()
        {
            Add(x => x.StringValue, "String", Formatters.FormatString);
            Add(x => x.DateTimeValue, "DateTime", Formatters.FormatDateTime);
            Add(x => x.MoneyValue, "Money", Formatters.FormatMoney);
            Add(x => x.IntValue, "Int", Formatters.FormatInt32);
            Add(x => x.BooleanValue, "Boolean", Formatters.FormatBoolean);
            Add(x => x.PercentValue, "Percent", Formatters.FormatPercent);
            Add(x => x.GuidValue, "Guid", Formatters.FormatGuid);
        }
    }
    
Within the constructor, you call `Add` for each property you wish to track. The first argument to `Add` is a lambda to get at the property you're tracking. The next argument is the human-friendly name you want to give the column. The third argument is a delegate to convert the value to a human-friendly string. The `Formatters` static class provides formatters for common values. There is also a fourth `IEqualityComparer` argument, in cases where you are not working with simple types. Only the first argument is required; by default the display name will default to the property name and the formatter will simply call `ToString()`. 

## Detecting Changes
If all you want to do is compare two objects, you can work with the `EntityConfiguration` directly. You can get the list of changes for two objects by calling `GetChanges`.

    TestEntityChangeDetector detector = new TestEntityChangeDetector();
    TestEntity entity1 = new TestEntity() { StringValue = "ABC" };
    TestEntity entity2 = new TestEntity() { StringValue = "DEF" };
    IEnumerable<IPropertyChange> changes = detector.GetChanges(entity1, entity2);
    // String: ABC -> DEF
    
Or, if you just need to know if a value changed:

    bool hasChanged = detector.HasChange(x => x.StringValue, entity1, entity2);
    
The `IPropertyChange` interface has the following members:
* Property - The `System.Reflection.PropertyInfo` object for the property that changed.
* DisplayName - The human-friendly name of the property passed to the `Add` method.
* OriginalValue - The value found in the first entity, as an `object`.
* UpdatedValue - The value found in the second entity, as an `object`.
* FormatOriginalValue - A method to get the formatted value of the first entity, using the supplied formatter.
* FormatUpdatedValue - A method to get the formatted value of the second entity, using the supplied formatter.

`OriginalValue` and `UpdatedValue` are `object`s. If the property could not be found in an entity, it is set to `null`, even for primitive properties. Be sure to check for `null`s if you plan to work directly with these properties. The `Format*` methods will account for `null`s on your behalf. 

## Change Tracker
Most of the time, you will want to track the state of a single object throughout it's lifetime. The `EntityChangeTracker` class provides methods to register an object and later ask what changed. The `EntityChangeTracker` constructor takes an `EntityConfiguration` -- this tells the tracker how to detect changes to the objects it manages. As soon as your entity comes into life, register it with the tracker using the `Attach` method. At the end of your code, you can simply ask the tracker for the status of the entity.

    TestEntityChangeDetector detector = new TestEntityChangeDetector();
    EntityChangeTracker tracker = new EntityChangeTracker(detector);
    TestEntity entity = new TestEntity() { IntValue = 123 };
    tracker.Attach(entity);
    // ... some business logic here ...
    EntityChange<TestEntity> changes = tracker.DetectChanges(entity);
    tracker.CommitChanges();
    
The `EntityChange` class gives a summary of what changed on the entity. It has the following members:
* Entity - The object that was being tracked
* State - Says whether the object is `Unmodified`, `Modified`, `Added`, `Removed` or `Detached`.
* GetChanges - Gets the individual properties that were changed.
* GetChange - Gets the `IPropertyChange` for a property, or `null` if the property isn't changed or tracked.
* HasChange - Determines whether there was a change for a property.
* As<TDerived> - Makes it easier to detect changes on sub-classes (see below)

There are three overloads of `DetectChanges`. The first takes no arguments and returns an `EntityChange` for each entity that was `Modified`, `Added` or `Removed`. If you only want to get back entities with certain states or include `Unmodified` entities, you can use the second overload that accepts an `EntityState` flag enum. Finally, you can retrieve the state of a single entity using the third overload. In the case that the entity is not being tracked, its state will be `Detached`.

Once you have finished processing the changes, you can commit the changes to the tracker, via `CommitChanges`. This will ensure the next time you call `DetectChanges` you will not get the same changes back again. `Modified` and `Added` entities will become `Unmodified`, and `Removed` entities will no longer be tracked.

## Inheritance
If you are dealing with a class hierarchy, you can specify how to detect changes whenever the entity is of a particular type, using the `When<TDerived>` method.

    public class TestEntityChangeDetector : EntityConfiguration<TestEntity>
    {
        public TestEntityChangeDetector()
        {
            Add(x => x.StringValue, "String", Formatters.FormatString);
        
            When<DerivedA>()
                .Add(x => x.A1, "A1", Formatters.FormatString)
                .Add(x => x.A2, "A2", Formatters.FormatString);
                
            When<DerivedB>()
                .Add(x => x.B1, "B1", Formatters.FormatString)
                .Add(x => x.B2, "B2", Formatters.FormatString);
        }
    }
    
You can chain as many properties after calling `When` as you need.

If you need to detect whether a property changed for a particular derived instance, you can use the `As<Derived>` method. This will expose two overloads of the `HasChange` method, either taking two base class objects or two derived class objects. In the overload accepting the base class, `HasChange` will return `false` if either object is not an instance of the derived class.

            var detector = new TestEntityChangeDetector();
            TestEntity original = new DerivedA() { A1 = "Hello" };
            TestEntity updated = new DerivedA() { A1 = "Goodbye" };
            
            bool hasChange = detector.As<DerivedA>().HasChange(x => x.A1, original, updated);

## Nulls
The change detector is smart enough to handle `null`s on your behalf. If one of the entities is `null`, the property values of the other entity are compared to `null`. If both entities are `null`, they are considered the equal. Be aware that `null`s will not be passed to the formatter, so you can't customize their format. However, you can simply check the result for `null` and replace it with a placeholder string.

# Collection Detection
There is also support for detecting changes between collections, via the `CollectionChangeDetector`. This class will take two collections (`ICollection<TElement>`) and see what elements are added or removed. You can pass an optional `IEqualityComparer<TElement>` to the constructor if the element type is not a simple type. `CollectionChangeDetector` has a  `GetChanges` method that returns an `ElementChangeCollection`, which allows you to see which elements were `Added` or `Removed`. There is an overload of the `GetChanges` method to explicitly search for `Added`, `Removed` or `Unmodified` elements.

    CollectionChangeDetector<int> detector = new CollectionChangeDetector<int>();
    var changes = detector.GetChanges(new int[] { 1, 2, 3 }, new int[] { 2, 4 });
    // Detects 1 and 3 are Removed and 4 is Added.

If you have an entity with a collection property, you can track how that collection changes throughout the lifetime of the entity. When initializing the `EntityConfiguration`, call `AddCollection` to register the collection. You can then pass the configuration to an `EntityChangeTracker` and it will automatically track the related collection with the entity. When you want to see what has changed, call `DetectCollectionChanges` on the tracker. `DetectCollectionChanges` accepts the entity and a delegate to specify which collection you're checking. You can optionally specify whether you are looking for `Added` or `Removed` elements. Just like with entities, calling `CommitChanges` will prevent the same changes from being returned for the collection.

## License
This is free and unencumbered software released into the public domain.

Anyone is free to copy, modify, publish, use, compile, sell, or
distribute this software, either in source code form or as a compiled
binary, for any purpose, commercial or non-commercial, and by any
means.

In jurisdictions that recognize copyright laws, the author or authors
of this software dedicate any and all copyright interest in the
software to the public domain. We make this dedication for the benefit
of the public at large and to the detriment of our heirs and
successors. We intend this dedication to be an overt act of
relinquishment in perpetuity of all present and future rights to this
software under copyright law.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.

For more information, please refer to <http://unlicense.org>
