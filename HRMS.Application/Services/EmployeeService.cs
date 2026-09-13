using AutoMapper;
using FluentValidation;
using HRMS.Application.Common;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Common;
using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces;
using HRMS.Domain.Specifications.Employees;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IEncryptionService _encryptionService;
        private readonly IValidator<EmployeeInput> _validator;

        public EmployeeService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IEncryptionService encryptionService,
            IValidator<EmployeeInput> validator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _encryptionService = encryptionService;
            _validator = validator;
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            var employees = (await _unitOfWork.Employees.GetAllAsync()).ToList();
            foreach (var e in employees) e.NationalId = _encryptionService.Decrypt(e.NationalId);
            return employees;
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(id);
            if (employee != null) employee.NationalId = _encryptionService.Decrypt(employee.NationalId);
            return employee;
        }

        public async Task<EmployeeResult> CreateEmployeeAsync(EmployeeInput input)
        {
            var result = await ValidateAsync(input, excludeEmployeeId: null);
            if (result.HasErrors) return result;

            var employee = _mapper.Map<Employee>(input);
            employee.NationalId = _encryptionService.Encrypt(input.NationalId);

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

            result = await ValidateAsync(input, excludeEmployeeId: id);
            if (result.HasErrors) return result;

            _mapper.Map(input, employee);
            employee.NationalId = _encryptionService.Encrypt(input.NationalId);

            _unitOfWork.Employees.Update(employee);
            await _unitOfWork.SaveChangesAsync();

            result.Success = true;
            result.Employee = employee;
            return result;
        }

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
                return (false, "لا يمكن حذف هذا الموظف لأن له رواتب معتمدة أو سجلات مرتبطة به");
            }
        }

        public async Task<PagedResult<Employee>> GetPagedEmployeesAsync(int page, int pageSize)
        {
            var spec = new EmployeesPagedSpecification(page, pageSize);
            var items = (await _unitOfWork.Employees.ListAsync(spec)).ToList();
            var total = await _unitOfWork.Employees.CountAsync(spec);

            foreach (var e in items) e.NationalId = _encryptionService.Decrypt(e.NationalId);

            return new PagedResult<Employee> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
        }

        // ---------- Validation (FluentValidation) ----------
        private async Task<EmployeeResult> ValidateAsync(EmployeeInput input, int? excludeEmployeeId)
        {
            var result = new EmployeeResult();

            var context = new ValidationContext<EmployeeInput>(input);
            if (excludeEmployeeId.HasValue)
                context.RootContextData["ExcludeEmployeeId"] = excludeEmployeeId.Value;

            var validation = await _validator.ValidateAsync(context);
            if (validation.IsValid) return result;

            foreach (var failure in validation.Errors)
            {
                switch (failure.PropertyName)
                {
                    case nameof(EmployeeInput.FullName): result.FullNameError = failure.ErrorMessage; break;
                    case nameof(EmployeeInput.Address): result.AddressError = failure.ErrorMessage; break;
                    case nameof(EmployeeInput.PhoneNumber): result.PhoneNumberError = failure.ErrorMessage; break;
                    case nameof(EmployeeInput.Nationality): result.NationalityError = failure.ErrorMessage; break;
                    case nameof(EmployeeInput.NationalId): result.NationalIdError = failure.ErrorMessage; break;
                    case nameof(EmployeeInput.BirthDate): result.BirthDateError = failure.ErrorMessage; break;
                    case nameof(EmployeeInput.ContractDate): result.ContractDateError = failure.ErrorMessage; break;
                    case nameof(EmployeeInput.Salary): result.SalaryError = failure.ErrorMessage; break;
                    case nameof(EmployeeInput.AttendanceTime): result.AttendanceTimeError = failure.ErrorMessage; break;
                    case nameof(EmployeeInput.DepartureTime): result.DepartureTimeError = failure.ErrorMessage; break;
                }
            }

            return result;
        }
    }
}