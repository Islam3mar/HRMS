using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Application.DTOs
{
    public class AttendanceRecordInput
    {
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan CheckInTime { get; set; }
        public TimeSpan CheckOutTime { get; set; }
    }
}
