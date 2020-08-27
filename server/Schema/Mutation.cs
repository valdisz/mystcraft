namespace atlantis {
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    public class Mutation {
        public Mutation(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public async Task<DbGame> NewGame(string name) {
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
