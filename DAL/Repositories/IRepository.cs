using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DAL.Repositories
{

    public interface IRepository<T>  where T : class
    {
        void Insert(T entity);
        void Insert(ICollection<T> entities);
        void Update(T entity);
        void Update(ICollection<T> entities);
        void Delete(T entity);
        void Delete(IEnumerable<T> entities);
        IQueryable<T> GetAll(Expression<Func<T, bool>> expression = null);
        T Get(object key);
        T Get(Expression<Func<T, bool>> query);
        void Unchanged(T entity);
    }
}
