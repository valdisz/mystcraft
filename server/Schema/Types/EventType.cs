namespace advisor.Schema
{
    using HotChocolate.Types;
    using Persistence;

    public class EventType : ObjectType<DbEvent> {
        protected override void Configure(IObjectTypeDescriptor<DbEvent> descriptor) {
            descriptor.Field("unitNumber").Resolve(x => {
                var e = x.Parent<DbEvent>();
                return e.UnitNumber ?? e.MissingUnitNumber;
            });
        }
    }
}
