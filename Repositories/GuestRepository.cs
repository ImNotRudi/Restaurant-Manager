using System.Collections.Generic;
using System.Linq;
using WpfApp1.Model;

namespace WpfApp1.Repositories
{
    public class GuestRepository : IGuestRepository
    {
        private readonly GuestContext _context;

        public GuestRepository()
        {
            _context = new GuestContext();
            _context.Database.EnsureCreated();
        }

        public void Create(Guest entity)
        {
            _context.Guests.Add(entity);
            _context.SaveChanges();
        }

        public List<Guest> ReadAll()
        {
            return _context.Guests.ToList();
        }

        public Guest Read(int id)
        {
            return _context.Guests.Find(id);
        }

        public void Update(Guest entity)
        {
            _context.Guests.Update(entity);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var guest = _context.Guests.Find(id);
            if (guest != null)
            {
                _context.Guests.Remove(guest);
                _context.SaveChanges();
            }
        }
    }
}