using AutoMapper;
using FishingMap.Domain.Data.Context;
using FishingMap.Domain.Data.DTO.PermitObjects;
using FishingMap.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingMap.Domain.Services
{
    public class PermitsService : IPermitsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public PermitsService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Permit> AddPermit(Permit permit)
        {
            var now = DateTime.Now;
            var entity = new Data.Entities.Permit()
            {
                Name = permit.Name,
                Url = permit.Url,
                Created = now,
                Modified = now
            };

            entity = _context.Permits.Add(entity).Entity;
            await _context.SaveChangesAsync();

            return _mapper.Map<Permit>(entity);
        }

        public async Task DeletePermit(int id)
        {
            var permit = await _context.Permits.FindAsync(id);
            if (permit != null)
            {
                _context.Permits.Remove(permit);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Permit> GetPermit(int id)
        {
            var permit = await _context.Permits.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (permit != null)
            {
                return _mapper.Map<Permit>(permit);
            }

            return null;
        }

        public async Task<IEnumerable<Permit>> GetPermits(string search)
        {
            var query = _context.Permits.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search));
            }
            var permits = await query.OrderBy(p => p.Name).AsNoTracking().ToListAsync();
            return _mapper.Map<IEnumerable<Permit>>(permits);
        }

        public async Task<Permit> UpdatePermit(int id, Permit permit)
        {
            var entity = await _context.Permits.FirstOrDefaultAsync(p => p.Id == id);
            if (entity != null)
            {
                entity.Name = permit.Name;
                entity.Url = permit.Url;
                entity.Modified = DateTime.Now;

                await _context.SaveChangesAsync();

                return _mapper.Map<Permit>(entity);
            }

            return null;
        }
    }
}
