// FIXME
namespace advisor.Features;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record UserCreate(string Email, string Password, params string[] Roles) : IRequest<DbUser>;

// public class UserCreateHandler : IRequestHandler<UserCreate, DbUser> {
//     public UserCreateHandler(Database db, IAccessControl accessControl) {
//         this.db = db;
//         this.accessControl = accessControl;
//     }

//     private readonly Database db;
//     private readonly IAccessControl accessControl;

//     public async Task<DbUser> Handle(UserCreate request, CancellationToken cancellationToken) {
//         var user = await db.Users.SingleOrDefaultAsync(x => x.Email == request.Email);
//         if (user != null) {
//             return user;
//         }

//         var now = DateTimeOffset.UtcNow;

//         user = new DbUser {
//             Email = request.Email,
//             CreatedAt = now,
//             LastLoginAt = now
//         };

//         if (!string.IsNullOrWhiteSpace(request.Password)) {
//             user.Algorithm = DigestAlgorithm.SHA256;
//             user.Salt = accessControl.GetSalt();
//             user.Digest = accessControl.ComputeDigest(user.Salt, request.Password);
//         }

//         var resultingRoles = new HashSet<string>(user.Roles);
//         resultingRoles.UnionWith(request.Roles);

//         user.Roles.Clear();
//         user.Roles.AddRange(resultingRoles);

//         await db.Users.AddAsync(user);
//         await db.SaveChangesAsync();

//         return user;
//     }
// }
