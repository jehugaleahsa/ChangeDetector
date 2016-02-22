# ChangeDetector

Easily determine what's changed on an object.

Download using NuGet: [ChangeDetector](http://nuget.org/packages/ChangeDetector)

## Overview
ChangeDetector will take two objects of the same type and determine if they differ. You can define an entity configuration for any class by simply creating a subclass of `EntityConfiguration<TEntity>`:

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
    
Within the constructor, you can call `Add` for each property you wish to detect changes. The first argument to `Add` is the human-friendly name you want to give the column. The next argument is a lambda to get at the property you want to use. Finally, the third argument is a delegate to convert the value to a human-friendly string. The `Formatters` static class provides formatters for common values.

## Detecting Changes
Once you have defined your change detector, you can get the list of changes for two objects by calling `GetChanges`.

    TestEntityChangeDetector detector = new TestEntityChangeDetector();
    TestEntity entity1 = new TestEntity() { StringValue = "ABC" };
    TestEntity entity2 = new TestEntity() { StringValue = "DEF" };
    
    IEnumerable<FieldChange> changes = detector.GetChanges(entity1, entity2);
    // String: ABC -> DEF
    
Or, if you just need to know if a value changed:

    bool hasChanged = detector.HasChange(entity1, entity2, x => x.StringValue);

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
