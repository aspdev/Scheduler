using Client.Torun.DataService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Client.Torun.DataService.EntityConfigurations
{
    public class SchedulerRoleConfiguration : IEntityTypeConfiguration<SchedulerRole>
    {
        public void Configure(EntityTypeBuilder<SchedulerRole> builder)
        {
            builder.ToTable("tbl_SchedulerRole");

            builder.HasKey(sr => sr.RoleId);

            builder.Property(sr => sr.RoleId).ValueGeneratedOnAdd();
            builder.Property(sr => sr.RoleName).IsRequired();
        }
    }
}