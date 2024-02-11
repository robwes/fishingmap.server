using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FishingMap.Data.Interfaces
{
    public interface IRepository<TEntity> where TEntity : class, IEntity
    {
        Task<bool> Any(Expression<Func<TEntity, bool>>? filter);
        TEntity Add(TEntity entity);

        Task<TEntity?> Find(
            Expression<Func<TEntity, bool>> filter,
            Expression<Func<TEntity, object>>[]? includeProperties = null,
            bool noTracking = false);

        Task<IEnumerable<TEntity>> GetAll(
            Expression<Func<TEntity, bool>>? filter = null,
            Expression<Func<TEntity, object>>[]? includeProperties = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            bool noTracking = false);

        Task<TEntity?> GetById(
            int id,
            Expression<Func<TEntity, object>>[]? include = null,
            bool noTracking = false);

        TEntity Update(TEntity entity);
        Task Delete(int id);
        void Delete(TEntity entity);
        Task Save();
    }
}
