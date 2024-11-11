using System.Linq.Expressions;

namespace ShutterManagement.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        IQueryable<T> GetAllQuerable();
        Task<List<T>> GetAll();

        Task<List<T>> GetAll(Expression<Func<T, bool>> predicate);
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        T FirstOrDefault(Expression<Func<T, bool>> predicate);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        bool Any(Expression<Func<T, bool>> predicate);
        Task<T> GetById(int Id);
        Task<T> Add(T entity);
        Task Delete(T entity);
        Task<T> Update(T entity);
        Task Save();

    }

}
