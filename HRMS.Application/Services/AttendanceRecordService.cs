using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using ClosedXML.Excel;
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

        public AttendanceRecordService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
            record.CreatedAt = DateTime.Now;

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
            record.UpdatedAt = DateTime.Now;

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

        // ---------- Validation (قاعدة رقم 1 لـ 5 المذكورة فى صورة الـ Validation Rules) ----------
        private async Task<AttendanceRecordResult> ValidateAsync(AttendanceRecordInput input, int? excludeId)
        {
            var result = new AttendanceRecordResult();

            // قاعدة 3: لازم يبقى فيه موظف متحدد
            if (input.EmployeeId <= 0 || await _unitOfWork.Employees.GetByIdAsync(input.EmployeeId) == null)
            {
                result.EmployeeIdError = "من فضلك ادخل اسم موظف صالح";
                return result;
            }

            if (input.Date == default)
            {
                result.DateError = "من فضلك ادخل تاريخ صحيح";
                return result;
            }

            if (input.CheckInTime == default)
                result.CheckInTimeError = "من فضلك ادخل وقت الحضور";

            if (input.CheckOutTime == default)
                result.CheckOutTimeError = "من فضلك ادخل وقت الانصراف";
            else if (input.CheckInTime != default && input.CheckOutTime <= input.CheckInTime)
                result.CheckOutTimeError = "وقت الانصراف يجب ان يكون بعد وقت الحضور";

            if (result.HasErrors) return result;

            // قاعدة 6 (رد العميل): مفيش أكتر من سجل لنفس الموظف فى نفس اليوم
            if (await _unitOfWork.AttendanceRecords.RecordExistsAsync(input.EmployeeId, input.Date, excludeId))
                result.DateError = "يوجد سجل حضور وانصراف مسجل بالفعل لهذا الموظف فى نفس هذا اليوم";

            return result;
        }

        // ==================== استيراد من Excel ====================
        // الأعمدة المتوقعة بالترتيب: الرقم القومي | التاريخ | وقت الحضور | وقت الانصراف
        public async Task<AttendanceImportResult> ImportFromExcelAsync(Stream fileStream)
        {
            var importResult = new AttendanceImportResult();

            // بنجيب كل الموظفين مرة واحدة عشان نقلل عدد الاستعلامات على قاعدة البيانات
            var employeesByNationalId = (await _unitOfWork.Employees.GetAllAsync())
            .ToDictionary(e => e.NationalId);

            using var workbook = new XLWorkbook(fileStream);
            var sheet = workbook.Worksheets.First();
            var rows = sheet.RowsUsed().Skip(1);

            foreach (var row in rows)
            {
                var rowNumber = row.RowNumber();
                try
                {
                    var nationalId = row.Cell(1).GetString().Trim();
                    var dateCell = row.Cell(2);
                    var checkInCell = row.Cell(3);
                    var checkOutCell = row.Cell(4);

                    if (string.IsNullOrWhiteSpace(nationalId))
                    {
                        importResult.FailedCount++;
                        importResult.Errors.Add($"صف {rowNumber}: الرقم القومي فارغ");
                        continue;
                    }


                    if (!employeesByNationalId.TryGetValue(nationalId, out var employee))
                    {
                        importResult.FailedCount++;
                        importResult.Errors.Add($"صف {rowNumber}: لا يوجد موظف بهذا الرقم القومي ({nationalId})");
                        continue;
                    }

                    if (employee == null)
                    {
                        importResult.FailedCount++;
                        importResult.Errors.Add($"صف {rowNumber}: لا يوجد موظف بهذا الرقم القومي ({nationalId})");
                        continue;
                    }

                    if (!dateCell.TryGetValue(out DateTime date))
                    {
                        importResult.FailedCount++;
                        importResult.Errors.Add($"صف {rowNumber}: تاريخ غير صالح");
                        continue;
                    }

                    var checkIn = ParseTime(checkInCell);
                    var checkOut = ParseTime(checkOutCell);

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
