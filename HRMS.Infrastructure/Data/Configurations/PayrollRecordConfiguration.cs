using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations
{
    public class PayrollRecordConfiguration : IEntityTypeConfiguration<PayrollRecord>
    {
        public void Configure(EntityTypeBuilder<PayrollRecord> builder)
        {
            builder.Property(p => p.BaseSalary).HasColumnType("decimal(18,2)");
            builder.Property(p => p.OvertimeHours).HasColumnType("decimal(18,2)");
            builder.Property(p => p.DeductionHours).HasColumnType("decimal(18,2)");
            builder.Property(p => p.TotalOvertimeAmount).HasColumnType("decimal(18,2)");
            builder.Property(p => p.TotalDeductionAmount).HasColumnType("decimal(18,2)");
            builder.Property(p => p.NetSalary).HasColumnType("decimal(18,2)");

            // مينفعش يتعمل اكتر من سجل معتمد لنفس الموظف عن نفس الشهر/السنة
            builder.HasIndex(p => new { p.EmployeeId, p.Month, p.Year }).IsUnique();

            builder.HasOne(p => p.Employee)
                   .WithMany()
                   .HasForeignKey(p => p.EmployeeId)
                   .OnDelete(DeleteBehavior.Restrict); // سجل الراتب المعتمد يفضل موجود حتى لو الموظف اتمسح
        }
    }
}
