using FishingMap.Domain.DTO.Permits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingMap.Domain.Interfaces
{
    public interface IPermitsService
    {
        Task<PermitDTO> AddPermit(PermitDTO permit);
        Task DeletePermit(int id);
        Task<PermitDTO?> GetPermit(int id);
        Task<IEnumerable<PermitDTO>> GetPermits(string search);
        Task<PermitDTO?> UpdatePermit(int id, PermitDTO permit);
    }
}
