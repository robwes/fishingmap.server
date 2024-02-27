using FishingMap.Domain.DTO.Permits;

namespace FishingMap.Domain.Interfaces
{
    public interface IPermitsService
    {
        Task<PermitDTO> AddPermit(PermitAdd permit);
        Task DeletePermit(int id);
        Task<PermitDTO?> GetPermit(int id);
        Task<IEnumerable<PermitDTO>> GetPermits(string search);
        Task<PermitDTO> UpdatePermit(int id, PermitUpdate permit);
    }
}
