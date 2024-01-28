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
            string[]? includeProperties = null);

        Task<IEnumerable<TEntity>> GetAll(
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            string[]? includeProperties = null);
        
        Task<TEntity?> GetById(
            int id,
            string[]? includeProperties = null);

        Task<TEntity?> GetById(
            int id,
            params Expression<Func<TEntity, object>>[] includeProperties);

        TEntity Update(TEntity entity);
        Task Delete(int id);
        void Delete(TEntity entity);
        Task Save();
    }
}
