namespace advisor
{
    using advisor.Model;
    using HotChocolate.Types;

    public class ItemType : ObjectType<AnItem> {
        protected override void Configure(IObjectTypeDescriptor<AnItem> descriptor) {
            descriptor.Name("Item");
        }
    }
}
