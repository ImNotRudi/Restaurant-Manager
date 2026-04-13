using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Model;

namespace WpfApp1.Repositories
{
    public interface IGuestRepository
    {
        void Create(Guest entity);
        List<Guest> ReadAll();
        Guest Read(int id);
        void Update(Guest entity);
        void Delete(int id);
    }
}
