using Client.Torun.DataService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Client.Torun.DataService.EntityConfigurations
{
    public class AdminSettingsConfiguration : IEntityTypeConfiguration<AdminSettings>
    {
        public void Configure(EntityTypeBuilder<AdminSettings> builder)
        {
            builder.ToTable("tbl_AdminSettings");

            builder.HasKey(s => s.AdminSettingsId);

            builder.Property(s => s.AdminSettingsId).ValueGeneratedOnAdd();
            builder.Property(s => s.NumberOfDoctorsOnDuty).IsRequired();

        }
    }
}