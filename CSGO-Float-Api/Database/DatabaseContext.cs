using CSGO_Float_Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CSGO_Float_Api.Database
{
    public class DatabaseContext : DbContext
    {

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
            
        }

        public DbSet<Skin> Skins { get; set; }
        public DbSet<FloatRequest> FloatRequests { get; set; }
        public DbSet<SteamAccount> SteamAccounts { get; set; }
    }
}
