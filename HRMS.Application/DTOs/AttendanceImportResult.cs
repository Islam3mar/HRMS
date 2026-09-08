using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Application.DTOs
{
    public class AttendanceImportResult
    {
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> Errors { get; set; } = new();   // رسالة لكل صف فشل، بتتضمن رقم الصف
    }
}
