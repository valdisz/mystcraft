namespace atlantis
{
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    public class Mutation {
        public Mutation(Database db, AccessControl accessControl) {
            this.db = db;
            this.accessControl = accessControl;
        }

        private readonly Database db;
        private readonly AccessControl accessControl;

        public async Task<DbUser> CreateUser(string email, string password) {
            var salt = accessControl.GetSalt();
            var digest = accessControl.ComputeDigest(salt, password);

            var user = await db.Users.AddAsync(new DbUser {
                Email = email,
                Algorithm = DigestAlgorithm.SHA256,
                Salt = salt,
                Digest = digest
            });
            await db.SaveChangesAsync();

            return user.Entity;
        }

        public async Task<DbGame> CreateGame(string name) {
            var newGame = new DbGame  {
                Name = name
            };

            await db.Games.AddAsync(newGame);
            await db.SaveChangesAsync();

            return newGame;
        }

        public async Task<string> DeleteGame(long id)  {
            var game = await db.Games.FirstOrDefaultAsync(x => x.Id == id);
            if (game != null) {
                db.Games.Remove(game);
                await db.SaveChangesAsync();

                return "deleted";
            }

            return "not found";
        }
    }
}
