using HRMS.Domain.Common;
using HRMS.Domain.Interfaces;
using HRMS.Infrastructure.Data;

namespace HRMS.Infrastructure.Repositories
{
    public class GenericRepository<T> : BaseRepository<T>, IGenericRepository<T> where T : BaseEntity
    {
        public GenericRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
