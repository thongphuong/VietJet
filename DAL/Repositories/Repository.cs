using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    using System.Data.Entity;
    using System.Linq.Expressions;
    using DAL.Entities;

    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly EFDbContext _context;
        private const string EntityNotFound = "Data object is not found";

        public Repository(EFDbContext context)
        {
            _context = context;
        }
        //public void Dispose()
        //{
        //    _context.Dispose();
        //}

        public void Insert(T entity)
        {
            _context.Set<T>().Add(entity);
        }

        public void Insert(ICollection<T> entities)
        {
            _context.Set<T>().AddRange(entities);
        }

        public void Update(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void Update(ICollection<T> entities)
        {
            foreach (var entity in entities)
            {
                _context.Entry(entity).State = EntityState.Modified;
            }
        }

        public void Unchanged(T entity)
        {
            _context.Entry(entity).State = EntityState.Unchanged;
        }
        public void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        public void Delete(IEnumerable<T> entities)
        {
            _context.Set<T>().RemoveRange(entities);
        }

        public IQueryable<T> GetAll(Expression<Func<T, bool>> expression = null)
        {
            return expression == null ? _context.Set<T>() : _context.Set<T>().Where(expression);

        }

        public T Get(object key)
        {
            return _context.Set<T>().Find(key);
        }
        public T Get(Expression<Func<T, bool>> query)
        {
            return _context.Set<T>().FirstOrDefault(query);
        }

    }
}
