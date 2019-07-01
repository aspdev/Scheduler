using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Client.Torun.DataService.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Client_Torun");

            migrationBuilder.CreateTable(
                name: "tbl_AdminSettings",
                schema: "Client_Torun",
                columns: table => new
                {
                    AdminSettingsId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    NumberOfDoctorsOnDuty = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_AdminSettings", x => x.AdminSettingsId);
                });

            migrationBuilder.CreateTable(
                name: "tbl_DayOffRequestStatus",
                schema: "Client_Torun",
                columns: table => new
                {
                    RequestStatusId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RequestStatus = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_DayOffRequestStatus", x => x.RequestStatusId);
                });

            migrationBuilder.CreateTable(
                name: "tbl_SchedulerRole",
                schema: "Client_Torun",
                columns: table => new
                {
                    RoleId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RoleName = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_SchedulerRole", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "tbl_User",
                schema: "Client_Torun",
                columns: table => new
                {
                    UserId = table.Column<Guid>(nullable: false),
                    FirstName = table.Column<string>(maxLength: 50, nullable: false),
                    LastName = table.Column<string>(maxLength: 50, nullable: false),
                    Email = table.Column<string>(maxLength: 50, nullable: false),
                    Password = table.Column<string>(maxLength: 500, nullable: false),
                    TokenToResetPassword = table.Column<string>(nullable: true),
                    TokenToResetPasswordValidFrom = table.Column<DateTime>(nullable: true),
                    RoleId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_User", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_tbl_User_tbl_SchedulerRole_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "Client_Torun",
                        principalTable: "tbl_SchedulerRole",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_DayOff",
                schema: "Client_Torun",
                columns: table => new
                {
                    DayOffId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_DayOff", x => x.DayOffId);
                    table.ForeignKey(
                        name: "FK_tbl_DayOff_tbl_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "Client_Torun",
                        principalTable: "tbl_User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_DayOffRequest",
                schema: "Client_Torun",
                columns: table => new
                {
                    DayOffRequestId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    RequestStatus = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_DayOffRequest", x => x.DayOffRequestId);
                    table.ForeignKey(
                        name: "FK_tbl_DayOffRequest_tbl_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "Client_Torun",
                        principalTable: "tbl_User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Duty",
                schema: "Client_Torun",
                columns: table => new
                {
                    DutyId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Duty", x => x.DutyId);
                    table.ForeignKey(
                        name: "FK_tbl_Duty_tbl_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "Client_Torun",
                        principalTable: "tbl_User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_DutyRequirement",
                schema: "Client_Torun",
                columns: table => new
                {
                    DutyRequirementId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    RequiredTotalDutiesInMonth = table.Column<int>(nullable: false),
                    TotalHolidayDuties = table.Column<int>(nullable: false, defaultValue: 0),
                    TotalWeekdayDuties = table.Column<int>(nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_DutyRequirement", x => x.DutyRequirementId);
                    table.ForeignKey(
                        name: "FK_tbl_DutyRequirement_tbl_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "Client_Torun",
                        principalTable: "tbl_User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "Client_Torun",
                table: "tbl_AdminSettings",
                columns: new[] { "AdminSettingsId", "NumberOfDoctorsOnDuty" },
                values: new object[] { 1, 2 });

            migrationBuilder.InsertData(
                schema: "Client_Torun",
                table: "tbl_SchedulerRole",
                columns: new[] { "RoleId", "RoleName" },
                values: new object[] { 1, 1 });

            migrationBuilder.InsertData(
                schema: "Client_Torun",
                table: "tbl_User",
                columns: new[] { "UserId", "Email", "FirstName", "LastName", "Password", "RoleId", "TokenToResetPassword", "TokenToResetPasswordValidFrom" },
                values: new object[,]
                {
                    { new Guid("855eb0f6-6725-4deb-be76-03a82bcc9168"), "adam@gmail.com", "Adam", "Jones", "123ABCabc", 1, null, null },
                    { new Guid("028b3ca3-e4af-4af6-8b9e-00e1f3267acf"), "fiona@gmail.com", "Fiona", "Woods", "123ABCabc", 1, null, null },
                    { new Guid("4504fc59-4eb3-4f80-bfb7-a7bd3675e467"), "george@gmail.com", "George", "Clooney", "123ABCbac", 1, null, null },
                    { new Guid("5feb44a1-98c6-48b7-a1d2-cb369ca94b96"), "james@gmail.com", "James", "Lincoln", "123ABCabc", 1, null, null },
                    { new Guid("539aa771-2c56-452b-9748-bb7f5bb24afa"), "martha@gmail.com", "Martha", "Argerich", "123ABCabc", 1, null, null },
                    { new Guid("b51185d9-9bf7-4fff-ac43-4ac1dbed7a58"), "derek@gmail.com", "Derek", "Banas", "123ABCabc", 1, null, null },
                    { new Guid("24cc6ba3-f0e4-4a9d-95ae-cec96500f298"), "edward@gmail.com", "Edward", "Snowden", "123ABCabc", 1, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_DayOff_UserId",
                schema: "Client_Torun",
                table: "tbl_DayOff",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_DayOffRequest_UserId",
                schema: "Client_Torun",
                table: "tbl_DayOffRequest",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Duty_UserId",
                schema: "Client_Torun",
                table: "tbl_Duty",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_DutyRequirement_UserId",
                schema: "Client_Torun",
                table: "tbl_DutyRequirement",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_User_RoleId",
                schema: "Client_Torun",
                table: "tbl_User",
                column: "RoleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tbl_AdminSettings",
                schema: "Client_Torun");

            migrationBuilder.DropTable(
                name: "tbl_DayOff",
                schema: "Client_Torun");

            migrationBuilder.DropTable(
                name: "tbl_DayOffRequest",
                schema: "Client_Torun");

            migrationBuilder.DropTable(
                name: "tbl_DayOffRequestStatus",
                schema: "Client_Torun");

            migrationBuilder.DropTable(
                name: "tbl_Duty",
                schema: "Client_Torun");

            migrationBuilder.DropTable(
                name: "tbl_DutyRequirement",
                schema: "Client_Torun");

            migrationBuilder.DropTable(
                name: "tbl_User",
                schema: "Client_Torun");

            migrationBuilder.DropTable(
                name: "tbl_SchedulerRole",
                schema: "Client_Torun");
        }
    }
}
