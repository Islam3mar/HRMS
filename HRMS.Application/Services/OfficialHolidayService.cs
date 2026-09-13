using AutoMapper;
using FluentValidation;
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
        private readonly IValidator<OfficialHolidayInput> _validator;

        public OfficialHolidayService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<OfficialHolidayInput> validator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<IEnumerable<OfficialHoliday>> GetAllAsync()
            => await _unitOfWork.OfficialHolidays.GetAllOrderedByDateAsync();

        public async Task<OfficialHoliday?> GetByIdAsync(int id)
            => await _unitOfWork.OfficialHolidays.GetByIdAsync(id);

        public async Task<OfficialHolidayResult> CreateAsync(OfficialHolidayInput input)
        {
            var result = await ValidateAsync(input, excludeHolidayId: null);
            if (result.HasErrors) return result;

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

            var result = await ValidateAsync(input, excludeHolidayId: id);
            if (result.HasErrors) return result;

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

        // ---------- Validation (FluentValidation) ----------
        // ملحوظة: فحص تكرار التاريخ (DateExistsAsync) بقى جوه الـ Validator نفسه
        // (OfficialHolidayInputValidator.ValidateUniqueDateAsync) فمش محتاجين نكرره هنا تانى.
        private async Task<OfficialHolidayResult> ValidateAsync(OfficialHolidayInput input, int? excludeHolidayId)
        {
            var result = new OfficialHolidayResult();

            var context = new ValidationContext<OfficialHolidayInput>(input);
            if (excludeHolidayId.HasValue)
                context.RootContextData["ExcludeHolidayId"] = excludeHolidayId.Value;

            var validation = await _validator.ValidateAsync(context);
            if (validation.IsValid) return result;

            foreach (var failure in validation.Errors)
            {
                switch (failure.PropertyName)
                {
                    case nameof(OfficialHolidayInput.Name): result.NameError = failure.ErrorMessage; break;
                    case nameof(OfficialHolidayInput.Date): result.DateError = failure.ErrorMessage; break;
                }
            }

            return result;
        }
    }
}