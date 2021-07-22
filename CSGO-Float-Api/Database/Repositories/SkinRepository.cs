using CSGO_Float_Api.Models;
using System.Collections.Generic;
using System.Linq;

namespace CSGO_Float_Api.Database.Repositories
{
    public interface ISkinRepository
    {
        //CRUD
        void Add(Skin skin);
        void AddRange(List<Skin> SkinsList);
        void Update(Skin skin);
        void Delete(ulong Id);
        void Delete(Skin skin);
        Skin Get(ulong Id);

        int GetCount();
    }

    public class SkinRepository : ISkinRepository
    {
        private DatabaseContext _context;
        public SkinRepository(DatabaseContext context)
        {
            _context = context;
        }

        public void Add(Skin skin)
        {
            _context.Add(skin);
            _context.SaveChanges();
        }

        public void AddRange(List<Skin> SkinsList)
        {
            _context.AddRange(SkinsList);
            _context.SaveChanges();
        }

        public void Delete(ulong Id)
        {
            Skin item = Get(Id);
            _context.Skins.Remove(item);
            _context.SaveChanges();
        }

        public void Delete(Skin skin)
        {
            _context.Skins.Remove(skin);
            _context.SaveChanges();
        }

        public Skin Get(ulong Id)
        {
            return _context.Skins.Where(a=>a.param_a == Id).FirstOrDefault();
        }

        public int GetCount()
        {
            return _context.Skins.Count();
        }

        public void Update(Skin skin)
        {
            _context.Update(skin);
            _context.SaveChanges();
        }
    }
}
