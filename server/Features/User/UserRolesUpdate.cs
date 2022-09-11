namespace advisor.Features {
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using MediatR;

    public record UserRolesUpdate(long UserId, string[] Add, string[] Remove) : IRequest<DbUser> {
    }

    public class UserRolesUpdateHandler : IRequestHandler<UserRolesUpdate, DbUser> {
        public UserRolesUpdateHandler(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public async Task<DbUser> Handle(UserRolesUpdate request, CancellationToken cancellationToken) {
            var user = await db.Users.FindAsync(request.UserId);
            if (user == null) return null;

            user.Roles = user.Roles
                .Union(request.Add)
                .Distinct()
                .Except(request.Remove)
                .ToList();

            await db.SaveChangesAsync();

            return user;
        }
    }
}
