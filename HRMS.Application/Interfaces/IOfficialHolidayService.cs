using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Application.DTOs;
using HRMS.Domain.Entities;

namespace HRMS.Application.Interfaces
{
    public interface IOfficialHolidayService
    {
        Task<IEnumerable<OfficialHoliday>> GetAllAsync();
        Task<OfficialHoliday?> GetByIdAsync(int id);
        Task<OfficialHolidayResult> CreateAsync(OfficialHolidayInput input);
        Task<OfficialHolidayResult> UpdateAsync(int id, OfficialHolidayInput input);
        Task<bool> DeleteAsync(int id);
    }
}
