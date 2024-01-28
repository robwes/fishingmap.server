using FishingMap.Data.Context;
using FishingMap.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FishingMap.Data.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity
    {
        protected readonly ApplicationDbContext _context;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
        }

        public virtual async Task<bool> Any(
            Expression<Func<TEntity, bool>>? filter)
        {
            if (filter != null)
            {
                return await _context.Set<TEntity>().AnyAsync(filter);
            }

            return await _context.Set<TEntity>().AnyAsync();
        }

        // add an entity to the database
        public virtual TEntity Add(TEntity entity)
        {
            // Add the entity to the context
            _context.Set<TEntity>().Add(entity);
            return entity; // This returned entity is not tracked
        }

        // find an entity in the database
        public virtual async Task<TEntity?> Find(
                       Expression<Func<TEntity, bool>> filter,
                       string[]? includeProperties = null)
        {
            var query = _context.Set<TEntity>().AsNoTracking();
            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }
            }

            return await query
                .AsNoTracking()
                .FirstOrDefaultAsync(filter);
        }

        // get all entities from the database
        public virtual async Task<IEnumerable<TEntity>> GetAll(
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            string[]? includeProperties = null
            )
        {
            IQueryable<TEntity> query = _context.Set<TEntity>().AsNoTracking();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }
            }

            if (orderBy != null)
            {
                return await orderBy(query).AsNoTracking().ToListAsync();
            }
            else
            {
                return await query.AsNoTracking().ToListAsync();
            }
        }

        // get an entity by id from the database
        public virtual async Task<TEntity?> GetById(
            int id,
            string[]? includeProperties = null)
        {
            var query = _context.Set<TEntity>().AsNoTracking();

            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }
            }

            return await query.FirstOrDefaultAsync(e => e.Id == id);
        }
        public virtual async Task<TEntity?> GetById(
            int id,
            params Expression<Func<TEntity, object>>[] includeProperties)
        {
            var query = _context.Set<TEntity>().AsNoTracking();

            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }
            }

            return await query.FirstOrDefaultAsync(e => e.Id == id);
        }

        // update an entity in the database
        public virtual TEntity Update(TEntity entity)
        {
            var entry = _context.Entry(entity);
            entry.CurrentValues.SetValues(entity);
            _context.Entry(entry.Entity).State = EntityState.Modified;

            return entry.Entity;
        }

        // delete an entity from the database
        public virtual async Task Delete(int id)
        {
            var entity = await _context.FindAsync<TEntity>(id);
            if (entity != null)
            {
                _context.Set<TEntity>().Remove(entity);
            }
        }

        public virtual void Delete(TEntity entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                _context.Set<TEntity>().Attach(entity);
            }

            _context.Set<TEntity>().Remove(entity);
        }

        // save changes to the database
        public virtual async Task Save()
        {
            await _context.SaveChangesAsync();
        }
    }
}
