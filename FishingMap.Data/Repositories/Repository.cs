using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using FishingMap.Data.Context;
using FishingMap.Data.Interfaces;

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
            return entity;
        }

        // find an entity in the database
        public virtual async Task<TEntity?> Find(
                        Expression<Func<TEntity, bool>> filter,
                        Expression<Func<TEntity, object>>[]? includeProperties = null,
                        bool noTracking = false)
        {
            var query = _context.Set<TEntity>().AsQueryable();

            if (noTracking)
            {
                query = query.AsNoTracking();
            }

            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }
            }

            return await query.FirstOrDefaultAsync(filter);
        }

        public virtual async Task<IEnumerable<TEntity>> GetAll(
            Expression<Func<TEntity, bool>>? filter = null,
            Expression<Func<TEntity, object>>[]? includeProperties = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            bool noTracking = false)
        {
            var query = _context.Set<TEntity>().AsQueryable();

            if (noTracking)
            {
                query = query.AsNoTracking();
            }

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
                return await orderBy(query).ToListAsync();
            }
            else
            {
                return await query.ToListAsync();
            }
        }

        public virtual async Task<TEntity?> GetById(
            int id,
            Expression<Func<TEntity, object>>[]? includeProperties = null,
            bool noTracking = false)
        {
            var query = _context.Set<TEntity>().AsQueryable();

            if (noTracking)
            {
                query = query.AsNoTracking();
            }

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
            _context.Set<TEntity>().Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;

            return entity;
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
