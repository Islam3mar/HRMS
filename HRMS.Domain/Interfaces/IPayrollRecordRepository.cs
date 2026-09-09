using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;

namespace HRMS.Domain.Interfaces
{
    public interface IPayrollRecordRepository : IGenericRepository<PayrollRecord>
    {
        // فيه سجل معتمد ومحفوظ فعلاً لهذا الموظف عن الشهر/السنة دول؟
        Task<PayrollRecord?> GetByEmployeeAndMonthAsync(int employeeId, int month, int year);

        // كل السجلات المعتمدة عن شهر/سنة معينة (لكل الموظفين)
        Task<IEnumerable<PayrollRecord>> GetByMonthAsync(int month, int year);
    }
}
