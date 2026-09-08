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
    public class GeneralSettingsService : IGeneralSettingsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GeneralSettingsService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<GeneralSettings?> GetSettingsAsync()
            => await _unitOfWork.GeneralSettings.GetSingleAsync();

        public async Task<GeneralSettingsResult> SaveSettingsAsync(GeneralSettingsInput input)
        {
            var result = new GeneralSettingsResult();

            // قاعدة 2: كل الحقول مطلوبة
            if (input.AdditionRatePerHour <= 0)
                result.AdditionRateError = "من فضلك ادخل بيانات الحقل";

            if (input.DeductionRatePerHour <= 0)
                result.DeductionRateError = "من فضلك ادخل بيانات الحقل";

            // قاعدة اضافية (منطقية): مينفعش نفس يوم الاجازة يتكرر
            if (input.WeeklyHoliday1 == input.WeeklyHoliday2)
                result.WeeklyHoliday2Error = "لا يمكن اختيار نفس يوم الاجازة مرتين";

            if (result.HasErrors) return result;

            var settings = await _unitOfWork.GeneralSettings.GetSingleAsync();

            if (settings == null)
            {
                settings = _mapper.Map<GeneralSettings>(input);
                settings.CreatedAt = DateTime.Now;
                await _unitOfWork.GeneralSettings.AddAsync(settings);
            }
            else
            {
                _mapper.Map(input, settings);
                settings.UpdatedAt = DateTime.Now;
                _unitOfWork.GeneralSettings.Update(settings);
            }

            await _unitOfWork.SaveChangesAsync();

            result.Success = true;
            result.Settings = settings;
            return result;
        }
    }
}
