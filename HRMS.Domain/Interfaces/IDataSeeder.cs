using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Domain.Interfaces
{
    public interface IDataSeeder
    {
        Task SeedAsync();
    }
}
