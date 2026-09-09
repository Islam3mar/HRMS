using System.Collections.Generic;
using System.Threading.Tasks;
using HRMS.Domain.Common;
using HRMS.Domain.Specifications;

namespace HRMS.Domain.Interfaces
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);

        // ---------- Specification Pattern (إضافة جديدة، مفيش أي method قديم اتشال) ----------
        Task<IEnumerable<T>> ListAsync(ISpecification<T> spec);
        Task<int> CountAsync(ISpecification<T> spec);
        Task<T?> FirstOrDefaultAsync(ISpecification<T> spec);
    }
}
