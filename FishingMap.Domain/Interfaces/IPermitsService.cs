using FishingMap.Domain.Data.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingMap.Domain.Interfaces
{
    public interface IPermitsService
    {
        Task<Permit> AddPermit(Permit permit);
        Task DeletePermit(int id);
        Task<Permit> GetPermit(int id);
        Task<IEnumerable<Permit>> GetPermits(string search);
        Task<Permit> UpdatePermit(int id, Permit permit);
    }
}
