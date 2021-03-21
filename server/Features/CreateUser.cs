namespace advisor.Features {
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
            if (user != null) return null;

            var salt = accessControl.GetSalt();
            var digest = accessControl.ComputeDigest(salt, request.Password);

            user = new DbUser {
                Email = request.Email,
                Algorithm = DigestAlgorithm.SHA256,
                Salt = salt,
                Digest = digest
            };

            foreach (var role in request.Roles ?? Enumerable.Empty<string>()) {
                user.Roles.Add(new DbUserRole { Role = role });
            }

            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();

            return user;
        }
    }
}
