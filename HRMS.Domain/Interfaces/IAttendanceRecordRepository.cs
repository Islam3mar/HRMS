using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Common;
using HRMS.Domain.Entities;

namespace HRMS.Domain.Interfaces
{
    public interface IAttendanceRecordRepository : IGenericRepository<AttendanceRecord>
    {
        // بحث بصفحات (Server-side Pagination) بالاسم / القسم / الفترة الزمنية
        Task<PagedResult<AttendanceRecord>> SearchAsync(AttendanceSearchFilter filter);

        // نفس البحث بس من غير Pagination (مستخدم فى التصدير و الطباعة، بيطلع كل النتائج المطابقة للفلتر)
        Task<IEnumerable<AttendanceRecord>> SearchAllAsync(AttendanceSearchFilter filter);

        Task<AttendanceRecord?> GetByIdWithDetailsAsync(int id);

        // قاعدة رقم 6: مفيش أكتر من سجل لنفس الموظف فى نفس اليوم
        Task<bool> RecordExistsAsync(int employeeId, DateTime date, int? excludeId = null);
    }
}
