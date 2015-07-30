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

## Nulls
The change detector is smart enough to handle `null`s on your behalf. If one of the entities are `null`, the value of the other entity is compared to `null`. If both entities are `null`, they are considered the same. Be aware that `null`s will not be passed to the formatter, so you can't customize their format. However, you can simply check the `OldValue` and `NewValue` for `null` and replace them with placeholder strings.

## License
If you are looking for a license, you won't find one. The software in this project is free, as in "free as air". Feel free to use my software anyway you like. Use it to build up your evil war machine, swindle old people out of their social security or crush the souls of the innocent.

I love to hear how people are using my code, so drop me a line. Feel free to contribute any enhancements or documentation you may come up with, but don't feel obligated. I just hope this code makes someone's life just a little bit easier.
