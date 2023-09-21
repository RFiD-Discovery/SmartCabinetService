using SmartCabinetService.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartCabinetService.Repository
{
    public class DiscoveryRepository : IRepository
    {
        private readonly DiscoveryEntityContext _context;

        public DiscoveryRepository()
        {
            _context = new DiscoveryEntityContext();

        }
        public int Add<T>(T t) where T : class
        {
            _context.Set<T>().Add(t);
            return _context.SaveChanges();
        }
        public int Remove<T>(T t) where T : class
        {
            _context.Entry(t).State = EntityState.Deleted;
            return _context.SaveChanges();
        }
        public List<T> GetAll<T>() where T : class
        {
            return _context.Set<T>().ToList();
        }
        public int Update<T>(T t) where T : class
        {
            _context.Entry(t).State = EntityState.Modified;
            return _context.SaveChanges();
        }
        public int Count<T>() where T : class
        {
            return _context.Set<T>().Count();
        }
        public T Find<T>(Expression<Func<T, bool>> match) where T : class
        {
            return _context.Set<T>().SingleOrDefault(match);
        }
        public List<T> FindAll<T>(Expression<Func<T, bool>> match) where T : class
        {
            return _context.Set<T>().Where(match).ToList();

        }
        public List<T> AllIncluding<T>(params Expression<Func<T, object>>[] include) where T : class
        {
            IQueryable<T> retVal = _context.Set<T>();

            retVal = include.Aggregate(retVal, (current, item) => current.Include(item));

            return retVal.ToList();
        }
        public int ExecuteSql(string sql, params object[] parameters)
        {
            return _context.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, sql, parameters);

        }
        public List<T> SqlQuery<T>(string sql, params object[] parameters) where T : class
        {
            return _context.Database.SqlQuery<T>(sql, parameters).ToList();
        }
    }
}
