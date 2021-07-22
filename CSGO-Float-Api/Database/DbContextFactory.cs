using Microsoft.EntityFrameworkCore;

namespace CSGO_Float_Api.Database
{
    public class DbContextFactory
    {
        public DatabaseContext Create()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlite(Program.connectionString)
                .Options;

            return new DatabaseContext(options);
        }
    }
}
