namespace atlantis
{
    using HotChocolate.Types;
    using HotChocolate.Types.Relay;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    public class UserType : ObjectType<DbUser> {
        protected override void Configure(IObjectTypeDescriptor<DbUser> descriptor) {
            descriptor.AsNode()
                .IdField(x => x.Id)
                .NodeResolver((ctx, id) => {
                    var db = ctx.Service<Database>();
                    return db.Users
                        .SingleOrDefaultAsync(x => x.Id == id);
                });
        }
    }
}
