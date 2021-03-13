namespace atlantis.Features {
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using atlantis.Persistence;
    using MediatR;

    public record UpdateUserRoles(long UserId, string[] Add, string[] Remove) : IRequest<DbUser> {
    }

    public class UpdateUserRolesHandler : IRequestHandler<UpdateUserRoles, DbUser> {
        public UpdateUserRolesHandler(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public async Task<DbUser> Handle(UpdateUserRoles request, CancellationToken cancellationToken) {
            var user = await db.Users.FindAsync(request.UserId);
            if (user == null) return null;

            var roles = user.Roles.ToDictionary(x => x.Role);
            foreach (var add in request.Add ?? Enumerable.Empty<string>()) {
                if (!roles.ContainsKey(add)) {
                    var role = new DbUserRole { Role = add };
                    user.Roles.Add(role);
                    roles.Add(add, role);
                }
            }

            foreach (var remove in request.Remove ?? Enumerable.Empty<string>()) {
                if (roles.ContainsKey(remove)) {
                    roles.Remove(remove);

                    var i = user.Roles.FindIndex(x => x.Role == remove);
                    user.Roles.RemoveAt(i);
                }
            }

            await db.SaveChangesAsync();

            return user;
        }
    }
}
