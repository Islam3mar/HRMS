using AutoMapper;
using ClosedXML.Excel;
using FluentValidation;
using HRMS.Application.DTOs;
using HRMS.Application.Interfaces;
using HRMS.Domain.Common;
using HRMS.Domain.Entities;
using HRMS.Domain.Interfaces;

namespace HRMS.Application.Services
{
    public class AttendanceRecordService : IAttendanceRecordService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<AttendanceRecordInput> _validator;

        public AttendanceRecordService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<AttendanceRecordInput> validator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<PagedResult<AttendanceRecord>> SearchAsync(AttendanceSearchFilter filter)
            => await _unitOfWork.AttendanceRecords.SearchAsync(filter);

        public async Task<IEnumerable<AttendanceRecord>> SearchAllAsync(AttendanceSearchFilter filter)
            => await _unitOfWork.AttendanceRecords.SearchAllAsync(filter);

        public async Task<AttendanceRecord?> GetByIdAsync(int id)
            => await _unitOfWork.AttendanceRecords.GetByIdWithDetailsAsync(id);

        public async Task<AttendanceRecordResult> CreateAsync(AttendanceRecordInput input)
        {
            var result = await ValidateAsync(input, excludeId: null);
            if (result.HasErrors) return result;

            var record = _mapper.Map<AttendanceRecord>(input);

            await _unitOfWork.AttendanceRecords.AddAsync(record);
            await _unitOfWork.SaveChangesAsync();

            result.Success = true;
            result.Record = record;
            return result;
        }

        public async Task<AttendanceRecordResult> UpdateAsync(int id, AttendanceRecordInput input)
        {
            var record = await _unitOfWork.AttendanceRecords.GetByIdAsync(id);
            if (record == null)
                return new AttendanceRecordResult { EmployeeIdError = "السجل غير موجود" };

            var result = await ValidateAsync(input, excludeId: id);
            if (result.HasErrors) return result;

            _mapper.Map(input, record);

            _unitOfWork.AttendanceRecords.Update(record);
            await _unitOfWork.SaveChangesAsync();

            result.Success = true;
            result.Record = record;
            return result;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var record = await _unitOfWork.AttendanceRecords.GetByIdAsync(id);
            if (record == null) return false;

            _unitOfWork.AttendanceRecords.Delete(record);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // ---------- Validation (FluentValidation) ----------
        private async Task<AttendanceRecordResult> ValidateAsync(AttendanceRecordInput input, int? excludeId)
        {
            var result = new AttendanceRecordResult();

            var context = new ValidationContext<AttendanceRecordInput>(input);
            if (excludeId.HasValue)
                context.RootContextData["ExcludeRecordId"] = excludeId.Value;

            var validation = await _validator.ValidateAsync(context);
            if (validation.IsValid) return result;

            foreach (var failure in validation.Errors)
            {
                switch (failure.PropertyName)
                {
                    case nameof(AttendanceRecordInput.EmployeeId): result.EmployeeIdError = failure.ErrorMessage; break;
                    case nameof(AttendanceRecordInput.Date): result.DateError = failure.ErrorMessage; break;
                    case nameof(AttendanceRecordInput.CheckInTime): result.CheckInTimeError = failure.ErrorMessage; break;
                    case nameof(AttendanceRecordInput.CheckOutTime): result.CheckOutTimeError = failure.ErrorMessage; break;
                }
            }

            return result;
        }

        // ==================== استيراد من Excel ====================
        // الأعمدة المتوقعة: كود الموظف | اسم الموظف (للمراجعة فقط) | التاريخ | وقت الحضور | وقت الانصراف
        public async Task<AttendanceImportResult> ImportFromExcelAsync(Stream fileStream)
        {
            var importResult = new AttendanceImportResult();

            var employeesById = (await _unitOfWork.Employees.GetAllAsync()).ToDictionary(e => e.Id);

            using var workbook = new XLWorkbook(fileStream);
            var sheet = workbook.Worksheets.First();
            var rows = sheet.RowsUsed().Skip(1);

            foreach (var row in rows)
            {
                var rowNumber = row.RowNumber();
                try
                {
                    if (!row.Cell(1).TryGetValue(out int employeeId))
                    {
                        importResult.FailedCount++;
                        importResult.Errors.Add($"صف {rowNumber}: كود الموظف غير صالح");
                        continue;
                    }

                    if (!employeesById.TryGetValue(employeeId, out var employee))
                    {
                        importResult.FailedCount++;
                        importResult.Errors.Add($"صف {rowNumber}: لا يوجد موظف بكود ({employeeId})");
                        continue;
                    }

                    if (!row.Cell(3).TryGetValue(out DateTime date))
                    {
                        importResult.FailedCount++;
                        importResult.Errors.Add($"صف {rowNumber}: تاريخ غير صالح");
                        continue;
                    }

                    var checkIn = ParseTime(row.Cell(4));
                    var checkOut = ParseTime(row.Cell(5));

                    if (checkIn == null || checkOut == null)
                    {
                        importResult.FailedCount++;
                        importResult.Errors.Add($"صف {rowNumber}: وقت الحضور أو الانصراف غير صالح");
                        continue;
                    }

                    var input = new AttendanceRecordInput
                    {
                        EmployeeId = employee.Id,
                        Date = date.Date,
                        CheckInTime = checkIn.Value,
                        CheckOutTime = checkOut.Value
                    };

                    var result = await CreateAsync(input);
                    if (!result.Success)
                    {
                        importResult.FailedCount++;
                        var error = result.EmployeeIdError ?? result.DateError ?? result.CheckInTimeError ?? result.CheckOutTimeError;
                        importResult.Errors.Add($"صف {rowNumber}: {error}");
                        continue;
                    }

                    importResult.SuccessCount++;
                }
                catch (Exception ex)
                {
                    importResult.FailedCount++;
                    importResult.Errors.Add($"صف {rowNumber}: حدث خطأ اثناء قراءة الصف ({ex.Message})");
                }
            }

            return importResult;
        }

        // ==================== قالب الاستيراد ====================
        public async Task<byte[]> GenerateImportTemplateAsync()
        {
            var employees = (await _unitOfWork.Employees.GetAllAsync()).OrderBy(e => e.FullName);

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("قالب الاستيراد");
            sheet.RightToLeft = true;

            sheet.Cell(1, 1).Value = "كود الموظف";
            sheet.Cell(1, 2).Value = "اسم الموظف (للمراجعة فقط - متتعدلش)";
            sheet.Cell(1, 3).Value = "التاريخ";
            sheet.Cell(1, 4).Value = "وقت الحضور";
            sheet.Cell(1, 5).Value = "وقت الانصراف";
            sheet.Row(1).Style.Font.Bold = true;

            var rowIndex = 2;
            foreach (var employee in employees)
            {
                sheet.Cell(rowIndex, 1).Value = employee.Id;
                sheet.Cell(rowIndex, 2).Value = employee.FullName;
                rowIndex++;
            }

            sheet.Column(3).Style.DateFormat.Format = "yyyy-mm-dd";
            sheet.Column(4).Style.DateFormat.Format = "hh:mm";
            sheet.Column(5).Style.DateFormat.Format = "hh:mm";
            sheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private static TimeSpan? ParseTime(IXLCell cell)
        {
            if (cell.TryGetValue(out DateTime dt)) return dt.TimeOfDay;
            if (cell.TryGetValue(out TimeSpan ts)) return ts;

            var text = cell.GetString().Trim();
            if (TimeSpan.TryParse(text, out var parsed)) return parsed;

            return null;
        }

        // ==================== تصدير Excel ====================
        public async Task<byte[]> ExportToExcelAsync(AttendanceSearchFilter filter)
        {
            var records = await _unitOfWork.AttendanceRecords.SearchAllAsync(filter);

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("تقرير الحضور والانصراف");
            sheet.RightToLeft = true;

            sheet.Cell(1, 1).Value = "م";
            sheet.Cell(1, 2).Value = "اسم الموظف";
            sheet.Cell(1, 3).Value = "القسم";
            sheet.Cell(1, 4).Value = "التاريخ";
            sheet.Cell(1, 5).Value = "وقت الحضور";
            sheet.Cell(1, 6).Value = "وقت الانصراف";
            sheet.Row(1).Style.Font.Bold = true;

            var rowIndex = 2;
            foreach (var record in records)
            {
                sheet.Cell(rowIndex, 1).Value = rowIndex - 1;
                sheet.Cell(rowIndex, 2).Value = record.Employee.FullName;
                sheet.Cell(rowIndex, 3).Value = record.Employee.Department?.Name ?? "-";
                sheet.Cell(rowIndex, 4).Value = record.Date.ToString("yyyy/MM/dd");
                sheet.Cell(rowIndex, 5).Value = record.CheckInTime.ToString(@"hh\:mm");
                sheet.Cell(rowIndex, 6).Value = record.CheckOutTime.ToString(@"hh\:mm");
                rowIndex++;
            }

            sheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}