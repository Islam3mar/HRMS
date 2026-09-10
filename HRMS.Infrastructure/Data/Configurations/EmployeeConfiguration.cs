using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations
{
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.Property(e => e.FullName).IsRequired().HasMaxLength(150);
            builder.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(11);
            builder.Property(e => e.NationalId).IsRequired().HasMaxLength(100);
            builder.Property(e => e.Salary).HasColumnType("decimal(18,2)");
            builder.Property(e => e.Gender).HasConversion<string>().HasMaxLength(20);

            builder.HasIndex(e => e.NationalId).IsUnique();

            builder.HasOne(e => e.Department)
                   .WithMany(d => d.Employees)
                   .HasForeignKey(e => e.DepartmentId)
                   .OnDelete(DeleteBehavior.SetNull);   // لو القسم اتمسح، الموظف يفضل موجود بس بلا قسم
        }
    }
    }
