# ChangeDetector

Easily determine what's changed on an object.

Download using NuGet: [ChangeDetector](http://nuget.org/packages/ChangeDetector)

## Overview
ChangeDetector will take two objects of the same type and determine if they differ. It also lets you take snapshots of an object over its lifetime to see if and how it has changed. This is useful in scenarios where you need to track changes but do not want to complicate your business logic with recording every little update. Instead, you simply define which properties you want to track and register the entity with a tracker. After your business logic has run through, you can ask the tracker if any of those properties changed. You could use this to determine if an external systems needs updated with the latest data or even use it build up SQL commnds inside of a homemade ORM.

In order to track changes, you must create an `EntityConfiguration` for each entity you wish to track. This class will indicate which entity properties are of interest and how you wish to display them. You can define an entity configuration by simply creating a subclass of `EntityConfiguration<TEntity>`:

    public class TestEntityChangeDetector : EntityConfiguration<TestEntity>
    {
        public TestEntityChangeDetector()
        {
            Add("String", x => x.StringValue, Formatters.FormatString);
            Add("DateTime", x => x.DateTimeValue, Formatters.FormatDateTime);
            Add("Money", x => x.MoneyValue, Formatters.FormatMoney);
            Add("Int", x => x.IntValue, Formatters.FormatInt32);
            Add("Boolean", x => x.BooleanValue, Formatters.FormatBoolean);
            Add("Percent", x => x.PercentValue, Formatters.FormatPercent);
            Add("Guid", x => x.GuidValue, Formatters.FormatGuid);
        }
    }
    
Within the constructor, you can call `Add` for each property you wish to track. The first argument to `Add` is the human-friendly name you want to give the column. The next argument is a lambda to get at the property you want to use. Finally, the third argument is a delegate to convert the value to a human-friendly string. The `Formatters` static class provides formatters for common values.

## Detecting Changes
If all you want to do is compare two objects, you can work with the `EntityConfiguration` directly.You can get the list of changes for two objects by calling `GetChanges`.

    TestEntityChangeDetector detector = new TestEntityChangeDetector();
    TestEntity entity1 = new TestEntity() { StringValue = "ABC" };
    TestEntity entity2 = new TestEntity() { StringValue = "DEF" };
    IEnumerable<FieldChange> changes = detector.GetChanges(entity1, entity2);
    // String: ABC -> DEF
    
Or, if you just need to know if a value changed:

    bool hasChanged = detector.HasChange(entity1, entity2, x => x.StringValue);
    
The `FieldChange` class has the following properties:
* Property - The `System.Reflection.PropertyInfo` object for the property that change.
* FieldName - The human-friendly name of the property passed to the `Add` method.
* OldValue - The formatted value of the original object.
* NewValue - The formatted value of the updated object.

If you need to get at the raw values, you can use the `PropertyInfo`'s `GetValue` method, passing in the original or updated objects. Just be sure to check the type of the object and for `null`s.

## Change Tracker

## Inheritance
If your entity can have multiple derived classes, you can specify how to detect changes whenever the entity is of that type, using the `When<TDerived>` method.

    public class TestEntityChangeDetector : EntityConfiguration<TestEntity>
    {
        public TestEntityChangeDetector()
        {
            Add("String", x => x.StringValue, Formatters.FormatString);
        
            When<DerivedA>()
                .Add("A1", x => x.A1, Formatters.FormatString)
                .Add("A2", x => x.A2, Formatters.FormatString);
                
            When<DerivedB>()
                .Add("B1", x => x.B1, Formatters.FormatString)
                .Add("B2", x => x.B2, Formatters.FormatString);
        }
    }
    
You can chain as many properties after calling `When` as you need.

If you need to detect whether a property changed for a particular derived class, you can use the `As<Derived>` method. This will expose two overloads of the `HasChange` method, either taking two base class objects or two derived class objects. In the overload accepting the base class, `HasChange` will return false if either object is not an instance of the derived class. 

## Nulls
The change detector is smart enough to handle `null`s on your behalf. If one of the entities are `null`, the value of the other entity is compared to `null`. If both entities are `null`, they are considered the same. Be aware that `null`s will not be passed to the formatter, so you can't customize their format. However, you can simply check the `OldValue` and `NewValue` for `null` and replace them with placeholder strings.

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
