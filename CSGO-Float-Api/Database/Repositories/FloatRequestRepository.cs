using CSGO_Float_Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CSGO_Float_Api.Database.Repositories
{
    public interface IFloatRequestRepository
    {
        //CRUD
        void Add(FloatRequest request);
        void Update(FloatRequest request);
        void Delete(int Id);
        void Delete(FloatRequest request);
        FloatRequest Get(int Id);
        int GetCount();
    }

    public class FloatRequestRepository : IFloatRequestRepository
    {
        private DatabaseContext _context;
        public FloatRequestRepository(DatabaseContext context)
        {
            _context = context;
        }

        public void Add(FloatRequest request)
        {
            _context.Add(request);
            _context.SaveChanges();
        }

        public void Delete(int Id)
        {
            FloatRequest item = Get(Id);
            _context.FloatRequests.Remove(item);
            _context.SaveChanges();
        }

        public void Delete(FloatRequest request)
        {
            _context.FloatRequests.Remove(request);
            _context.SaveChanges();
        }

        public FloatRequest Get(int Id)
        {
            return _context.FloatRequests.Where(a=>a.ID == Id).Include(a => a.Skins).FirstOrDefault();
        }

        public int GetCount()
        {
            return _context.FloatRequests.Count();
        }

        public void Update(FloatRequest request)
        {
            _context.Update(request);
            _context.SaveChanges();
        }
    }
}
