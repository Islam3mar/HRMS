using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using AutoMapper;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Common;
using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces;
using HRMS.Domain.Specifications.Employees;

namespace HRMS.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        // تاريخ تأسيس الشركة (قاعدة رقم 6) - عدّله لو التاريخ مختلف
        private static readonly DateTime CompanyFoundationDate = new(2005, 6, 6);
        private const int MinimumAge = 20; // قاعدة رقم 4

        public EmployeeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
            => await _unitOfWork.Employees.GetAllAsync();

        public async Task<Employee?> GetEmployeeByIdAsync(int id)
            => await _unitOfWork.Employees.GetByIdAsync(id);

        public async Task<EmployeeResult> CreateEmployeeAsync(EmployeeInput input)
        {
            var result = new EmployeeResult();
            await ValidateAsync(result, input, excludeEmployeeId: null);

            if (result.HasErrors) return result;

            var employee = _mapper.Map<Employee>(input);

            await _unitOfWork.Employees.AddAsync(employee);
            await _unitOfWork.SaveChangesAsync();

            result.Success = true;
            result.Employee = employee;
            return result;
        }

        public async Task<EmployeeResult> UpdateEmployeeAsync(int id, EmployeeInput input)
        {
            var result = new EmployeeResult();

            var employee = await _unitOfWork.Employees.GetByIdAsync(id);
            if (employee == null)
            {
                result.FullNameError = "الموظف غير موجود";
                return result;
            }

            await ValidateAsync(result, input, excludeEmployeeId: id);
            if (result.HasErrors) return result;

            _mapper.Map(input, employee); // in-place mapping على الـ Entity الموجود

            _unitOfWork.Employees.Update(employee);
            await _unitOfWork.SaveChangesAsync();

            result.Success = true;
            result.Employee = employee;
            return result;
        }


        // DeleteEmployeeAsync method with error handling for foreign key constraints
        public async Task<(bool Success, string? Error)> DeleteEmployeeAsync(int id)
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(id);
            if (employee == null) return (false, "الموظف غير موجود");

            _unitOfWork.Employees.Delete(employee);

            try
            {
                await _unitOfWork.SaveChangesAsync();
                return (true, null);
            }
            catch (DbUpdateException)
            {
                // بيحصل لو الموظف مرتبط بسجلات رواتب معتمدة (DeleteBehavior.Restrict)
                return (false, "لا يمكن حذف هذا الموظف لأن له رواتب معتمدة أو سجلات مرتبطة به");
            }
        }


        public async Task<PagedResult<Employee>> GetPagedEmployeesAsync(int page, int pageSize)
        {
            var spec = new EmployeesPagedSpecification(page, pageSize);
            var items = await _unitOfWork.Employees.ListAsync(spec);
            var total = await _unitOfWork.Employees.CountAsync(spec);

            return new PagedResult<Employee> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
        }

        // ---------- Validation (القواعد 1 لـ 7) ----------
        private async Task ValidateAsync(EmployeeResult result, EmployeeInput input, int? excludeEmployeeId)
        {
            // قاعدة 2: الحقول المطلوبة
            if (string.IsNullOrWhiteSpace(input.FullName))
                result.FullNameError = "هذا الحقل مطلوب";

            if (string.IsNullOrWhiteSpace(input.Address))
                result.AddressError = "هذا الحقل مطلوب";

            if (string.IsNullOrWhiteSpace(input.Nationality))
                result.NationalityError = "هذا الحقل مطلوب";

            // قاعدة 3: رقم التليفون 11 رقم بالظبط
            if (string.IsNullOrWhiteSpace(input.PhoneNumber))
                result.PhoneNumberError = "هذا الحقل مطلوب";
            else if (!Regex.IsMatch(input.PhoneNumber, @"^\d{11}$"))
                result.PhoneNumberError = "رقم التليفون يجب ان يتكون من 11 رقم";

            // قاعدة 5: الرقم القومي 14 رقم بالظبط + عدم التكرار
            if (string.IsNullOrWhiteSpace(input.NationalId))
                result.NationalIdError = "هذا الحقل مطلوب";
            else if (!Regex.IsMatch(input.NationalId, @"^\d{14}$"))
                result.NationalIdError = "الرقم القومي يجب ان يتكون من 14 رقم";
            else if (await _unitOfWork.Employees.NationalIdExistsAsync(input.NationalId.Trim(), excludeEmployeeId))
                result.NationalIdError = "هذا الرقم القومي مستخدم بالفعل لموظف اخر";

            // قاعدة 4: تاريخ الميلاد منطقي + السن لا يقل عن 20 سنة
            if (input.BirthDate == default)
                result.BirthDateError = "هذا الحقل مطلوب";
            else if (input.BirthDate.Date > DateTime.Today)
                result.BirthDateError = "تاريخ الميلاد غير صحيح";
            else if (CalculateAge(input.BirthDate) < MinimumAge)
                result.BirthDateError = $"يجب ان يكون عمر الموظف {MinimumAge} سنة على الاقل";

            // قاعدة 6: تاريخ التعاقد لا يقل عن تاريخ تأسيس الشركة ولا يكون بالمستقبل
            if (input.ContractDate == default)
                result.ContractDateError = "هذا الحقل مطلوب";
            else if (input.ContractDate.Date < CompanyFoundationDate)
                result.ContractDateError = $"تاريخ التعاقد لا يمكن ان يكون قبل {CompanyFoundationDate:yyyy/MM/dd}";
            else if (input.ContractDate.Date > DateTime.Today)
                result.ContractDateError = "تاريخ التعاقد غير صحيح";

            // قاعدة 7: الراتب رقم صحيح اكبر من صفر (النوع decimal بيمنع الحروف والعلامات اصلاً)
            if (input.Salary <= 0)
                result.SalaryError = "الراتب يجب ان يكون رقم صحيح اكبر من صفر";

            if (input.AttendanceTime == default)
                result.AttendanceTimeError = "هذا الحقل مطلوب";

            if (input.DepartureTime == default)
                result.DepartureTimeError = "هذا الحقل مطلوب";
            else if (input.AttendanceTime != default && input.DepartureTime <= input.AttendanceTime)
                result.DepartureTimeError = "موعد الانصراف يجب ان يكون بعد موعد الحضور";
        }

        private static int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;
            return age;
        }
    }
}