using Client.Torun.DataService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Client.Torun.DataService.EntityConfigurations
{
    public class DutyConfiguration : IEntityTypeConfiguration<Duty>
    {
        public void Configure(EntityTypeBuilder<Duty> builder)
        {
            builder.ToTable("tbl_Duty");

            builder.HasKey(d => d.DutyId);

            builder.Property(d => d.DutyId).ValueGeneratedOnAdd();
            builder.Property(d => d.Date).IsRequired();
            builder.Property(d => d.UserId).IsRequired();
        }
    }
}