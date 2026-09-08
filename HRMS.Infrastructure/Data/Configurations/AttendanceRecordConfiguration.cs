using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations
{
    public class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
    {
        public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
        {
            builder.Property(a => a.Date).IsRequired();
            builder.Property(a => a.CheckInTime).IsRequired();
            builder.Property(a => a.CheckOutTime).IsRequired();

            builder.HasOne(a => a.Employee)
                   .WithMany()
                   .HasForeignKey(a => a.EmployeeId)
                   .OnDelete(DeleteBehavior.Cascade);   // لو الموظف اتمسح، سجلات حضوره تتمسح معاه

            // قاعدة رقم 6: منع تكرار سجل لنفس الموظف فى نفس اليوم على مستوى الداتابيز كمان
            builder.HasIndex(a => new { a.EmployeeId, a.Date }).IsUnique();
        }
    }
}
