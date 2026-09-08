using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Application.DTOs;
using HRMS.Domain.Common;
using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces
{
    public interface IAttendanceRecordService
    {
        Task<PagedResult<AttendanceRecord>> SearchAsync(AttendanceSearchFilter filter);
        Task<IEnumerable<AttendanceRecord>> SearchAllAsync(AttendanceSearchFilter filter);   // للطباعة و التصدير
        Task<AttendanceRecord?> GetByIdAsync(int id);

        Task<AttendanceRecordResult> CreateAsync(AttendanceRecordInput input);
        Task<AttendanceRecordResult> UpdateAsync(int id, AttendanceRecordInput input);
        Task<bool> DeleteAsync(int id);

        Task<AttendanceImportResult> ImportFromExcelAsync(Stream fileStream);
        Task<byte[]> ExportToExcelAsync(AttendanceSearchFilter filter);
    }
}
