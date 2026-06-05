using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kessler.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "RM558652");

            migrationBuilder.CreateTable(
                name: "ORBITAL_OBJECTS",
                schema: "RM558652",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NoradId = table.Column<string>(type: "VARCHAR2(20)", maxLength: 20, nullable: true),
                    Type = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Status = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    OrbitRegion = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    AltitudeKm = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    InclinationDeg = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    LaunchYear = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    EstimatedMassKg = table.Column<double>(type: "BINARY_DOUBLE", nullable: true),
                    EstimatedSizeM = table.Column<double>(type: "BINARY_DOUBLE", nullable: true),
                    Name = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    DataConfidence = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "TIMESTAMP", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "TIMESTAMP", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ORBITAL_OBJECTS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "USERS",
                schema: "RM558652",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Name = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "VARCHAR2(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "VARCHAR2(500)", nullable: false),
                    Role = table.Column<string>(type: "VARCHAR2(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "TIMESTAMP", nullable: true),
                    IsActive = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USERS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MISSION_SCENARIOS",
                schema: "RM558652",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    OrbitalObjectId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Objective = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    Type = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    RiskLevel = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    EstimatedDurationDays = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    EstimatedDeltaVMps = table.Column<double>(type: "BINARY_DOUBLE", nullable: true),
                    PlannedStartUtc = table.Column<DateTime>(type: "TIMESTAMP", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TIMESTAMP", nullable: true),
                    Name = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    DataConfidence = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "TIMESTAMP", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "TIMESTAMP", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MISSION_SCENARIOS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MISSION_SCENARIOS_ORBITAL_OBJECTS_OrbitalObjectId",
                        column: x => x.OrbitalObjectId,
                        principalSchema: "RM558652",
                        principalTable: "ORBITAL_OBJECTS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "REUSE_MATERIAL_ESTIMATES",
                schema: "RM558652",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    OrbitalObjectId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Material = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    RecoveryPotential = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    PreferredPath = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    EstimatedSharePct = table.Column<double>(type: "BINARY_DOUBLE", nullable: false),
                    Notes = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: true),
                    Name = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    DataConfidence = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "TIMESTAMP", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "TIMESTAMP", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REUSE_MATERIAL_ESTIMATES", x => x.Id);
                    table.ForeignKey(
                        name: "FK_REUSE_MATERIAL_ESTIMATES_ORBITAL_OBJECTS_OrbitalObjectId",
                        column: x => x.OrbitalObjectId,
                        principalSchema: "RM558652",
                        principalTable: "ORBITAL_OBJECTS",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MISSION_SCENARIOS_OrbitalObjectId",
                schema: "RM558652",
                table: "MISSION_SCENARIOS",
                column: "OrbitalObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_REUSE_MATERIAL_ESTIMATES_OrbitalObjectId",
                schema: "RM558652",
                table: "REUSE_MATERIAL_ESTIMATES",
                column: "OrbitalObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_USERS_Email",
                schema: "RM558652",
                table: "USERS",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MISSION_SCENARIOS",
                schema: "RM558652");

            migrationBuilder.DropTable(
                name: "REUSE_MATERIAL_ESTIMATES",
                schema: "RM558652");

            migrationBuilder.DropTable(
                name: "USERS",
                schema: "RM558652");

            migrationBuilder.DropTable(
                name: "ORBITAL_OBJECTS",
                schema: "RM558652");
        }
    }
}
