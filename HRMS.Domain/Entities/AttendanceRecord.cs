using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Common;

namespace HRMS.Domain.Entities
{
    // سجل حضور و انصراف يومي لموظف واحد (المطلوب السابع)
    public class AttendanceRecord : BaseEntity
    {
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public DateTime Date { get; set; }          // تاريخ اليوم المسجل عليه الحضور

        public TimeSpan CheckInTime { get; set; }    // وقت الحضور
        public TimeSpan CheckOutTime { get; set; }   // وقت الانصراف
    }
}
