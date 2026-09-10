using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces;

namespace HRMS.Application.Services
{
    public class OfficialHolidayService : IOfficialHolidayService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OfficialHolidayService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<OfficialHoliday>> GetAllAsync()
            => await _unitOfWork.OfficialHolidays.GetAllOrderedByDateAsync();

        public async Task<OfficialHoliday?> GetByIdAsync(int id)
            => await _unitOfWork.OfficialHolidays.GetByIdAsync(id);

        public async Task<OfficialHolidayResult> CreateAsync(OfficialHolidayInput input)
        {
            var result = Validate(input);
            if (result.HasErrors) return result;

            if (await _unitOfWork.OfficialHolidays.DateExistsAsync(input.Date))
            {
                result.DateError = "يوجد اجازة رسمية مسجلة بنفس هذا التاريخ من قبل";
                return result;
            }

            var holiday = _mapper.Map<OfficialHoliday>(input);
            await _unitOfWork.OfficialHolidays.AddAsync(holiday);
            await _unitOfWork.SaveChangesAsync();

            result.Success = true;
            result.Holiday = holiday;
            return result;
        }

        public async Task<OfficialHolidayResult> UpdateAsync(int id, OfficialHolidayInput input)
        {
            var holiday = await _unitOfWork.OfficialHolidays.GetByIdAsync(id);
            if (holiday == null)
                return new OfficialHolidayResult { NameError = "الاجازة غير موجودة" };

            var result = Validate(input);
            if (result.HasErrors) return result;

            if (await _unitOfWork.OfficialHolidays.DateExistsAsync(input.Date, id))
            {
                result.DateError = "يوجد اجازة رسمية مسجلة بنفس هذا التاريخ من قبل";
                return result;
            }

            _mapper.Map(input, holiday);
            _unitOfWork.OfficialHolidays.Update(holiday);
            await _unitOfWork.SaveChangesAsync();

            result.Success = true;
            result.Holiday = holiday;
            return result;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var holiday = await _unitOfWork.OfficialHolidays.GetByIdAsync(id);
            if (holiday == null) return false;

            _unitOfWork.OfficialHolidays.Delete(holiday);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private static OfficialHolidayResult Validate(OfficialHolidayInput input)
        {
            var result = new OfficialHolidayResult();

            if (string.IsNullOrWhiteSpace(input.Name))
                result.NameError = "من فضلك ادخل اسم الاجازة";

            if (input.Date == default)
                result.DateError = "من فضلك ادخل تاريخ الاجازة";

            return result;
        }
    }
}
