using Client.Torun.DataService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Client.Torun.DataService.EntityConfigurations
{
    public class DayOffRequestStatusConfiguration : IEntityTypeConfiguration<DayOffRequestStatus>
    {
        public void Configure(EntityTypeBuilder<DayOffRequestStatus> builder)
        {
            builder.ToTable("tbl_DayOffRequestStatus");

            builder.HasKey(sr => sr.RequestStatusId);

            builder.Property(sr => sr.RequestStatusId).ValueGeneratedOnAdd();
            builder.Property(sr => sr.RequestStatus).IsRequired();
        }
    }
}