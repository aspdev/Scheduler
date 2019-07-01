using Client.Torun.DataService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Client.Torun.DataService.EntityConfigurations
{
    public class DayOffConfiguration : IEntityTypeConfiguration<DayOff>
    {
        public void Configure(EntityTypeBuilder<DayOff> builder)
        {
            builder.ToTable("tbl_DayOff");

            builder.HasKey(d => d.DayOffId);

            builder.Property(d => d.DayOffId).ValueGeneratedOnAdd();
            builder.Property(d => d.DayOffId).IsRequired();
            builder.Property(d => d.UserId).IsRequired();
            builder.Property(d => d.Date).IsRequired();
        }
    }
}