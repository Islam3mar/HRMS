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


        public async Task<PagedResult<EmployeeAttendanceGroup>> SearchGroupedByEmployeeAsync(AttendanceSearchFilter filter)
    => await _unitOfWork.AttendanceRecords.SearchGroupedByEmployeeAsync(filter);


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
            var rows = sheet.RowsUsed().Skip(1).ToList();

            var seenInFile = new HashSet<(int EmployeeId, DateTime Date)>();
            var recordsToInsert = new List<AttendanceRecord>();

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

                    if (!employeesById.ContainsKey(employeeId))
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
                    date = date.Date;

                    var checkIn = ParseTime(row.Cell(4));
                    var checkOut = ParseTime(row.Cell(5));

                    if (checkIn == null || checkOut == null)
                    {
                        importResult.FailedCount++;
                        importResult.Errors.Add($"صف {rowNumber}: وقت الحضور أو الانصراف غير صالح");
                        continue;
                    }

                    // تكرار داخل نفس الملف - الـ Validator بيفحص التكرار في الداتابيز بس،
                    // ومش هيشوف صفوف تانية في نفس الملف لسه ما اتحفظتش خالص
                    var key = (employeeId, date);
                    if (!seenInFile.Add(key))
                    {
                        importResult.FailedCount++;
                        importResult.Errors.Add($"صف {rowNumber}: هذا الصف مكرر لنفس الموظف ونفس التاريخ داخل نفس الملف");
                        continue;
                    }

                    var input = new AttendanceRecordInput
                    {
                        EmployeeId = employeeId,
                        Date = date,
                        CheckInTime = checkIn.Value,
                        CheckOutTime = checkOut.Value
                    };

                    // بننادي نفس الـ Validator المستخدم فى الفورم العادي (CreateAsync/UpdateAsync)
                    // عشان القواعد تفضل مصدرها مكان واحد بس، مش متكررة هنا بشكل مختلف
                    var validation = await _validator.ValidateAsync(input);
                    if (!validation.IsValid)
                    {
                        importResult.FailedCount++;
                        importResult.Errors.Add($"صف {rowNumber}: {validation.Errors.First().ErrorMessage}");
                        continue;
                    }

                    recordsToInsert.Add(new AttendanceRecord
                    {
                        EmployeeId = employeeId,
                        Date = date,
                        CheckInTime = checkIn.Value,
                        CheckOutTime = checkOut.Value
                    });

                    importResult.SuccessCount++;
                }
                catch (Exception ex)
                {
                    importResult.FailedCount++;
                    importResult.Errors.Add($"صف {rowNumber}: حدث خطأ اثناء قراءة الصف ({ex.Message})");
                }
            }

            // ---------- حفظ جماعي واحد بدل SaveChanges منفصل لكل صف ----------
            if (recordsToInsert.Count > 0)
            {
                await _unitOfWork.AttendanceRecords.AddRangeAsync(recordsToInsert);
                await _unitOfWork.SaveChangesAsync();
            }

            return importResult;
        }

        // ==================== قالب الاستيراد ====================
        public async Task<byte[]> GenerateImportTemplateAsync()
        {
            var employees = (await _unitOfWork.Employees.GetAllAsync()).OrderBy(e => e.FullName).ToList();

            using var workbook = new XLWorkbook();

            // شيت 1: منطقة إدخال البيانات - لازم تفضل فاضية تمامًا من أول صف بيانات (Row 2)
            var dataSheet = workbook.Worksheets.Add("قالب الاستيراد");
            dataSheet.RightToLeft = true;

            dataSheet.Cell(1, 1).Value = "كود الموظف";
            dataSheet.Cell(1, 2).Value = "اسم الموظف (للمراجعة فقط - متتعدلش)";
            dataSheet.Cell(1, 3).Value = "التاريخ";
            dataSheet.Cell(1, 4).Value = "وقت الحضور";
            dataSheet.Cell(1, 5).Value = "وقت الانصراف";
            dataSheet.Row(1).Style.Font.Bold = true;
            dataSheet.SheetView.FreezeRows(1);

            dataSheet.Column(3).Style.DateFormat.Format = "yyyy-mm-dd";
            dataSheet.Column(4).Style.DateFormat.Format = "hh:mm";
            dataSheet.Column(5).Style.DateFormat.Format = "hh:mm";
            dataSheet.Columns(1, 5).AdjustToContents();

            // شيت 2: مرجع أكواد الموظفين - منفصل تمامًا عن منطقة الإدخال عشان محدش يتلخبط
            var lookupSheet = workbook.Worksheets.Add("أكواد الموظفين");
            lookupSheet.RightToLeft = true;

            lookupSheet.Cell(1, 1).Value = "كود الموظف";
            lookupSheet.Cell(1, 2).Value = "اسم الموظف";
            lookupSheet.Row(1).Style.Font.Bold = true;
            lookupSheet.SheetView.FreezeRows(1);

            var rowIndex = 2;
            foreach (var employee in employees)
            {
                lookupSheet.Cell(rowIndex, 1).Value = employee.Id;
                lookupSheet.Cell(rowIndex, 2).Value = employee.FullName;
                rowIndex++;
            }

            lookupSheet.Columns().AdjustToContents();

            dataSheet.Workbook.Worksheets.Worksheet("قالب الاستيراد").SetTabActive();

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
            var records = (await _unitOfWork.AttendanceRecords.SearchAllAsync(filter)).ToList();

            var grouped = records
                .GroupBy(r => r.EmployeeId)
                .Select(g => new
                {
                    EmployeeName = g.First().Employee.FullName,
                    DepartmentName = g.First().Employee.Department?.Name ?? "-",
                    Records = g.OrderByDescending(r => r.Date).ToList()
                })
                .OrderBy(g => g.EmployeeName)
                .ToList();

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("تقرير الحضور والانصراف");
            sheet.RightToLeft = true;

            var currentRow = 1;

            foreach (var group in grouped)
            {
                // عنوان مجموعة الموظف - صف واحد ممتد عبر الأعمدة
                var headerRange = sheet.Range(currentRow, 1, currentRow, 4);
                headerRange.Merge();
                headerRange.Value = $"{group.EmployeeName} — {group.DepartmentName} ({group.Records.Count} سجل)";
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#E4F0F3");
                currentRow++;

                sheet.Cell(currentRow, 1).Value = "م";
                sheet.Cell(currentRow, 2).Value = "التاريخ";
                sheet.Cell(currentRow, 3).Value = "وقت الحضور";
                sheet.Cell(currentRow, 4).Value = "وقت الانصراف";
                sheet.Row(currentRow).Style.Font.Bold = true;
                currentRow++;

                var rowNumber = 1;
                foreach (var record in group.Records)
                {
                    sheet.Cell(currentRow, 1).Value = rowNumber++;
                    sheet.Cell(currentRow, 2).Value = record.Date.ToString("yyyy/MM/dd");
                    sheet.Cell(currentRow, 3).Value = record.CheckInTime.ToString(@"hh\:mm");
                    sheet.Cell(currentRow, 4).Value = record.CheckOutTime.ToString(@"hh\:mm");
                    currentRow++;
                }

                currentRow++; // سطر فاضي يفصل بين كل موظف والتاني
            }

            sheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}