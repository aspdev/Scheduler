using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Client.Torun.DataService.Entities;
using Client.Torun.DataService.EntityConfigurations;
using Client.Torun.DataService.Enums;
using Microsoft.EntityFrameworkCore;

namespace Client.Torun.DataService
{
    public sealed class DataServiceContext : DbContext
    {
        public DataServiceContext(DbContextOptions<DataServiceContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<User> Users { get; set; }
        public DbSet<DayOff> DaysOff { get; set; }
        public DbSet<DayOffRequest> DayOffRequests { get; set; }
        public DbSet<Duty> Duties { get; set; }
        public DbSet<SchedulerRole> SchedulerRoles { get; set; }
        public DbSet<DayOffRequestStatus> DayOffRequestStatuses { get; set; }
        public DbSet<AdminSettings> AdminSettings { get; set; }
        public DbSet<DutyRequirement> DutyRequirements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("Client_Torun");

            modelBuilder.ApplyConfiguration(new DayOffConfiguration());
            modelBuilder.ApplyConfiguration(new DayOffRequestConfiguration());
            modelBuilder.ApplyConfiguration(new DayOffRequestStatusConfiguration());
            modelBuilder.ApplyConfiguration(new DutyConfiguration());
            modelBuilder.ApplyConfiguration(new SchedulerRoleConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new AdminSettingsConfiguration());
            modelBuilder.ApplyConfiguration(new DutyRequirementConfiguration());

            modelBuilder.Entity<AdminSettings>()
                .HasData(new AdminSettings { AdminSettingsId = 1, NumberOfDoctorsOnDuty = 2 });

            modelBuilder.Entity<SchedulerRole>()
                .HasData(new SchedulerRole { RoleId = 1, RoleName = Role.Doctor });

            modelBuilder.Entity<User>()
                .HasData(
                    new User
                    {
                        UserId = Guid.NewGuid(),
                        FirstName = "Adam",
                        LastName = "Jones",
                        Email = "adam@gmail.com",
                        Password = "123ABCabc",
                        RoleId = 1
                    },
                    new User
                    {
                        UserId = Guid.NewGuid(),
                        FirstName = "Fiona",
                        LastName = "Woods",
                        Email = "fiona@gmail.com",
                        Password = "123ABCabc",
                        RoleId = 1
                    },
                    new User
                    {
                        UserId = Guid.NewGuid(),
                        FirstName = "George",
                        LastName = "Clooney",
                        Email = "george@gmail.com",
                        Password = "123ABCbac",
                        RoleId = 1
                    },
                    new User
                    {
                        UserId = Guid.NewGuid(),
                        FirstName = "James",
                        LastName = "Lincoln",
                        Email = "james@gmail.com",
                        Password = "123ABCabc",
                        RoleId = 1
                    },
                    new User
                    {
                        UserId = Guid.NewGuid(),
                        FirstName = "Martha",
                        LastName = "Argerich",
                        Email = "martha@gmail.com",
                        Password = "123ABCabc",
                        RoleId = 1
                    },
                    new User
                    {
                        UserId = Guid.NewGuid(),
                        FirstName = "Derek",
                        LastName = "Banas",
                        Email = "derek@gmail.com",
                        Password = "123ABCabc",
                        RoleId = 1
                    },
                    new User
                    {
                        UserId = Guid.NewGuid(),
                        FirstName = "Edward",
                        LastName = "Snowden",
                        Email = "edward@gmail.com",
                        Password = "123ABCabc",
                        RoleId = 1
                    });


            

        }

    }

    
}
