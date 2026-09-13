using AutoMapper;
using FluentValidation;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces;

namespace HRMS.Application.Services
{
    public class GeneralSettingsService : IGeneralSettingsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<GeneralSettingsInput> _validator;

        public GeneralSettingsService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<GeneralSettingsInput> validator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<GeneralSettings?> GetSettingsAsync()
            => await _unitOfWork.GeneralSettings.GetSingleAsync();

        public async Task<GeneralSettingsResult> SaveSettingsAsync(GeneralSettingsInput input)
        {
            var result = await ValidateAsync(input);
            if (result.HasErrors) return result;

            var settings = await _unitOfWork.GeneralSettings.GetSingleAsync();

            if (settings == null)
            {
                settings = _mapper.Map<GeneralSettings>(input);
                await _unitOfWork.GeneralSettings.AddAsync(settings);
            }
            else
            {
                _mapper.Map(input, settings);
                _unitOfWork.GeneralSettings.Update(settings);
            }

            await _unitOfWork.SaveChangesAsync();

            result.Success = true;
            result.Settings = settings;
            return result;
        }

        // ---------- Validation (FluentValidation) ----------
        private async Task<GeneralSettingsResult> ValidateAsync(GeneralSettingsInput input)
        {
            var result = new GeneralSettingsResult();

            var validation = await _validator.ValidateAsync(input);
            if (validation.IsValid) return result;

            foreach (var failure in validation.Errors)
            {
                switch (failure.PropertyName)
                {
                    case nameof(GeneralSettingsInput.AdditionRatePercentage): result.AdditionRateError = failure.ErrorMessage; break;
                    case nameof(GeneralSettingsInput.DeductionRatePercentage): result.DeductionRateError = failure.ErrorMessage; break;
                    case nameof(GeneralSettingsInput.WeeklyHoliday1): result.WeeklyHoliday1Error = failure.ErrorMessage; break;
                    case nameof(GeneralSettingsInput.WeeklyHoliday2): result.WeeklyHoliday2Error = failure.ErrorMessage; break;
                }
            }

            return result;
        }
    }
}