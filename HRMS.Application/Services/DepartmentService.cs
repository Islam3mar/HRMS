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
    public class DepartmentService : IDepartmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DepartmentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Department>> GetAllAsync()
            => await _unitOfWork.Departments.GetAllAsync();

        public async Task<Department?> GetByIdAsync(int id)
            => await _unitOfWork.Departments.GetByIdAsync(id);

        public async Task<DepartmentResult> CreateAsync(DepartmentInput input)
        {
            var result = await ValidateAsync(input, excludeId: null);
            if (result.HasErrors) return result;

            var department = _mapper.Map<Department>(input);

            await _unitOfWork.Departments.AddAsync(department);
            await _unitOfWork.SaveChangesAsync();

            result.Success = true;
            result.Department = department;
            return result;
        }

        public async Task<DepartmentResult> UpdateAsync(int id, DepartmentInput input)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(id);
            if (department == null)
                return new DepartmentResult { NameError = "القسم غير موجود" };

            var result = await ValidateAsync(input, excludeId: id);
            if (result.HasErrors) return result;

            _mapper.Map(input, department);

            _unitOfWork.Departments.Update(department);
            await _unitOfWork.SaveChangesAsync();

            result.Success = true;
            result.Department = department;
            return result;
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(id);
            if (department == null) return (false, "القسم غير موجود");

            if (await _unitOfWork.Departments.HasEmployeesAsync(id))
                return (false, "لا يمكن حذف هذا القسم لأنه مرتبط بموظفين بالفعل");

            _unitOfWork.Departments.Delete(department);
            await _unitOfWork.SaveChangesAsync();
            return (true, null);
        }

        private async Task<DepartmentResult> ValidateAsync(DepartmentInput input, int? excludeId)
        {
            var result = new DepartmentResult();

            if (string.IsNullOrWhiteSpace(input.Name))
            {
                result.NameError = "من فضلك ادخل اسم القسم";
                return result;
            }

            if (await _unitOfWork.Departments.NameExistsAsync(input.Name.Trim(), excludeId))
                result.NameError = "يوجد قسم بنفس هذا الاسم من قبل";

            return result;
        }
    }
}
