using Client.Torun.DataService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Client.Torun.DataService.EntityConfigurations
{
    public class DutyRequirementConfiguration : IEntityTypeConfiguration<DutyRequirement>
    {
        public void Configure(EntityTypeBuilder<DutyRequirement> builder)
        {
            builder.ToTable("tbl_DutyRequirement");

            builder.HasKey(dr => dr.DutyRequirementId);

            builder.Property(dr => dr.DutyRequirementId).ValueGeneratedOnAdd();
            builder.Property(dr => dr.UserId).IsRequired();
            builder.Property(dr => dr.Date).IsRequired();
            builder.Property(dr => dr.RequiredTotalDutiesInMonth).IsRequired();
            builder.Property(dr => dr.TotalWeekdayDuties).HasDefaultValue(0);
            builder.Property(dr => dr.TotalHolidayDuties).HasDefaultValue(0);

        }
    }
}