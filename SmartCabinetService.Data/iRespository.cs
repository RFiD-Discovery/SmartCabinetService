using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SmartCabinetService.Data
{
    public interface IRepository
    {
        int Add<T>(T t) where T : class;
        int Remove<T>(T t) where T : class;
        List<T> GetAll<T>() where T : class;
        int Update<T>(T t) where T : class;
        int Count<T>() where T : class;
        T Find<T>(Expression<Func<T, bool>> match) where T : class;
        List<T> FindAll<T>(Expression<Func<T, bool>> match) where T : class;
        List<T> AllIncluding<T>(params Expression<Func<T, object>>[] include) where T : class;
        int ExecuteSql(string sql, params object[] parameters);
        List<T> SqlQuery<T>(string sql, params object[] parameters) where T : class;

    }
}
