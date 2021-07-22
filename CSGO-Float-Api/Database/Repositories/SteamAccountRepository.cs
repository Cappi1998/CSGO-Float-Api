using CSGO_Float_Api.Models;
using System.Collections.Generic;
using System.Linq;
using X.PagedList;

namespace CSGO_Float_Api.Database.Repositories
{
    public interface ISteamAccountRepository
    {
        //CRUD
        void Add(SteamAccount acc);
        void AddRange(List<SteamAccount> accsList);
        void Update(SteamAccount acc);
        void Delete(string Username);
        void Delete(SteamAccount acc);
        SteamAccount Get(string Username);
        List<SteamAccount> GetAllAccounts();
        IPagedList<SteamAccount> GetAllAccounts(int? page);
        int GetCount();
    }
    public class SteamAccountRepository : ISteamAccountRepository
    {
        public readonly DbContextFactory _dbContextFactory;
        public SteamAccountRepository(DbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }
        
        public void Add(SteamAccount acc)
        {
            using (var _context = _dbContextFactory.Create())
            {
                _context.Add(acc);
                _context.SaveChanges();
            }
        }

        public void AddRange(List<SteamAccount> accsList)
        {
            using (var _context = _dbContextFactory.Create())
            {
                _context.AddRange(accsList);
                _context.SaveChanges();
            }
        }

        public void Delete(string Username)
        {
            using (var _context = _dbContextFactory.Create())
            {
                SteamAccount acc = Get(Username);
                _context.SteamAccounts.Remove(acc);
                _context.SaveChanges();
            }
        }

        public void Delete(SteamAccount acc)
        {
            using (var _context = _dbContextFactory.Create())
            {
                _context.SteamAccounts.Remove(acc);
                _context.SaveChanges();
            }
        }

        public SteamAccount Get(string Username)
        {
            using (var _context = _dbContextFactory.Create())
            {
                return _context.SteamAccounts.Where(a => a.Username == Username).FirstOrDefault();
            }
        }

        public List<SteamAccount> GetAllAccounts()
        {
            using (var _context = _dbContextFactory.Create())
            {
                return _context.SteamAccounts.ToList();
            }
        }

        public IPagedList<SteamAccount> GetAllAccounts(int? page)
        {
            using (var _context = _dbContextFactory.Create())
            {
                int RegistroPorPagina = 15;

                int NumeroPagina = page ?? 1;
                return _context.SteamAccounts.ToPagedList<SteamAccount>(NumeroPagina, RegistroPorPagina);
            }
        }

        public int GetCount()
        {
            using (var _context = _dbContextFactory.Create())
            {
              return _context.SteamAccounts.Count();
            }
        }

        public void Update(SteamAccount acc)
        {
            using (var _context = _dbContextFactory.Create())
            {
                _context.Update(acc);
                _context.SaveChanges();
            }
        }
    }
}
