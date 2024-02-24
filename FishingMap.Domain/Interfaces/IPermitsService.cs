using FishingMap.Domain.DTO.Permits;

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
