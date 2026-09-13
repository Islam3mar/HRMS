using System;
using System.Collections.Generic;
using System.Text;
using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HRMS.Infrastructure.Data.Configurations
{
    public class GeneralSettingsConfiguration : IEntityTypeConfiguration<GeneralSettings>
    {
        public void Configure(EntityTypeBuilder<GeneralSettings> builder)
        {
            builder.Property(s => s.AdditionRatePercentage).HasColumnType("decimal(18,2)");
            builder.Property(s => s.DeductionRatePercentage).HasColumnType("decimal(18,2)");
            builder.Property(s => s.WeeklyHoliday1).HasConversion<string>().HasMaxLength(20);
            builder.Property(s => s.WeeklyHoliday2).HasConversion<string>().HasMaxLength(20);
        }
    }
}
