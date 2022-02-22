namespace advisor.Features {
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public record CreateUser(string Email, string Password, params string[] Roles) : IRequest<DbUser> {
    }

    public class CreateUserHandler : IRequestHandler<CreateUser, DbUser> {
        public CreateUserHandler(Database db, AccessControl accessControl) {
            this.db = db;
            this.accessControl = accessControl;
        }

        private readonly Database db;
        private readonly AccessControl accessControl;

        public async Task<DbUser> Handle(CreateUser request, CancellationToken cancellationToken) {
            var user = await db.Users.SingleOrDefaultAsync(x => x.Email == request.Email);
            if (user != null) {
                return user;
            }

            user = new DbUser {
                Email = request.Email
            };

            if (!string.IsNullOrWhiteSpace(request.Password)) {
                user.Algorithm = DigestAlgorithm.SHA256;
                user.Salt = accessControl.GetSalt();
                user.Digest = accessControl.ComputeDigest(user.Salt, request.Password);
            }

            var resultingRoles = new HashSet<string>(user.Roles);
            resultingRoles.UnionWith(request.Roles);

            user.Roles.Clear();
            user.Roles.AddRange(resultingRoles);

            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();

            return user;
        }
    }
}
