using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Application.DTOs
{
    public class PayrollReportResult
    {
        public List<PayrollRowDto> Rows { get; set; } = new();

        // قاعدة 1: فى حالة البحث باسم موظف غير موجود
        public string? SearchError { get; set; }

        // قاعدة 2: فى حالة اختيار سنة خطأ (قبل تاريخ تأسيس الشركة)
        public string? YearError { get; set; }

        public bool HasErrors => SearchError != null || YearError != null;
    }
}
