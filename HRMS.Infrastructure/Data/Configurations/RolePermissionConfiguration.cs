using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations
{
    public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.Property(p => p.SystemPage)
                   .HasConversion<string>()   // بيتخزن "Employees" مش "1" — أوضح لو حد فتح الداتابيز يدوي
                   .HasMaxLength(50);

            builder.HasOne(p => p.Role)
                   .WithMany(r => r.Permissions)
                   .HasForeignKey(p => p.RoleId)
                   .OnDelete(DeleteBehavior.Cascade);   // لو المجموعة اتمسحت، صلاحياتها تتمسح معاها

            // منع تكرار نفس الصفحة مرتين لنفس المجموعة
            builder.HasIndex(p => new { p.RoleId, p.SystemPage }).IsUnique();
        }
    }
}
