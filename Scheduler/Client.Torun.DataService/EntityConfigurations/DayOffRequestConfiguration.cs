using Client.Torun.DataService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Client.Torun.DataService.EntityConfigurations
{
    public class DayOffRequestConfiguration : IEntityTypeConfiguration<DayOffRequest>
    {
        public void Configure(EntityTypeBuilder<DayOffRequest> builder)
        {
            builder.ToTable("tbl_DayOffRequest");

            builder.HasKey(d => d.DayOffRequestId);

            builder.Property(d => d.DayOffRequestId).ValueGeneratedOnAdd();
            builder.Property(d => d.Date).IsRequired();
            builder.Property(d => d.UserId).IsRequired();
            builder.Property(d => d.RequestStatus).IsRequired();
            
        }
    }
}