using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations
{
    public class OfficialHolidayConfiguration : IEntityTypeConfiguration<OfficialHoliday>
    {
        public void Configure(EntityTypeBuilder<OfficialHoliday> builder)
        {
            builder.Property(h => h.Name).IsRequired().HasMaxLength(150);
            builder.Property(h => h.Date).IsRequired();
            builder.HasIndex(h => h.Date).IsUnique();   // منع تكرار نفس التاريخ على مستوى الداتابيز كمان
        }
    }
}
