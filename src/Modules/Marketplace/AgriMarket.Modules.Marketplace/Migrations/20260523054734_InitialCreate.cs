using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AgriMarket.Modules.Marketplace.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "marketplace");

            migrationBuilder.CreateTable(
                name: "Counties",
                schema: "marketplace",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    EhakCode = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Counties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Equipments",
                schema: "marketplace",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Make = table.Column<string>(type: "text", nullable: false),
                    Model = table.Column<string>(type: "text", nullable: true),
                    ManufactureYear = table.Column<int>(type: "integer", nullable: true),
                    HorsePower = table.Column<int>(type: "integer", nullable: true),
                    Condition = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceCategories",
                schema: "marketplace",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Municipalities",
                schema: "marketplace",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    EhakCode = table.Column<string>(type: "text", nullable: false),
                    CountyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Municipalities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Municipalities_Counties_CountyId",
                        column: x => x.CountyId,
                        principalSchema: "marketplace",
                        principalTable: "Counties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                schema: "marketplace",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MunicipalityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Locations_Municipalities_MunicipalityId",
                        column: x => x.MunicipalityId,
                        principalSchema: "marketplace",
                        principalTable: "Municipalities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServiceListings",
                schema: "marketplace",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    PricePerHectare = table.Column<decimal>(type: "numeric", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    UserProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceListings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceListings_Locations_LocationId",
                        column: x => x.LocationId,
                        principalSchema: "marketplace",
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceListings_ServiceCategories_ServiceCategoryId",
                        column: x => x.ServiceCategoryId,
                        principalSchema: "marketplace",
                        principalTable: "ServiceCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Availabilities",
                schema: "marketplace",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceListingId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsBooked = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Availabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Availabilities_ServiceListings_ServiceListingId",
                        column: x => x.ServiceListingId,
                        principalSchema: "marketplace",
                        principalTable: "ServiceListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceListingEquipments",
                schema: "marketplace",
                columns: table => new
                {
                    ServiceListingId = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceListingEquipments", x => new { x.ServiceListingId, x.EquipmentId });
                    table.ForeignKey(
                        name: "FK_ServiceListingEquipments_Equipments_EquipmentId",
                        column: x => x.EquipmentId,
                        principalSchema: "marketplace",
                        principalTable: "Equipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceListingEquipments_ServiceListings_ServiceListingId",
                        column: x => x.ServiceListingId,
                        principalSchema: "marketplace",
                        principalTable: "ServiceListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "marketplace",
                table: "Counties",
                columns: new[] { "Id", "EhakCode", "Name" },
                values: new object[,]
                {
                    { new Guid("a0000000-0000-0000-0000-000000000001"), "0037", "Harju maakond" },
                    { new Guid("a0000000-0000-0000-0000-000000000002"), "0039", "Hiiu maakond" },
                    { new Guid("a0000000-0000-0000-0000-000000000003"), "0044", "Ida-Viru maakond" },
                    { new Guid("a0000000-0000-0000-0000-000000000004"), "0049", "Jõgeva maakond" },
                    { new Guid("a0000000-0000-0000-0000-000000000005"), "0051", "Järva maakond" },
                    { new Guid("a0000000-0000-0000-0000-000000000006"), "0056", "Lääne maakond" },
                    { new Guid("a0000000-0000-0000-0000-000000000007"), "0059", "Lääne-Viru maakond" },
                    { new Guid("a0000000-0000-0000-0000-000000000008"), "0063", "Põlva maakond" },
                    { new Guid("a0000000-0000-0000-0000-000000000009"), "0067", "Pärnu maakond" },
                    { new Guid("a0000000-0000-0000-0000-00000000000a"), "0070", "Rapla maakond" },
                    { new Guid("a0000000-0000-0000-0000-00000000000b"), "0074", "Saare maakond" },
                    { new Guid("a0000000-0000-0000-0000-00000000000c"), "0078", "Tartu maakond" },
                    { new Guid("a0000000-0000-0000-0000-00000000000d"), "0082", "Valga maakond" },
                    { new Guid("a0000000-0000-0000-0000-00000000000e"), "0084", "Viljandi maakond" },
                    { new Guid("a0000000-0000-0000-0000-00000000000f"), "0086", "Võru maakond" }
                });

            migrationBuilder.InsertData(
                schema: "marketplace",
                table: "ServiceCategories",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { new Guid("a1b2c3d4-0001-0000-0000-000000000001"), "Round and square baling services", "Hay Baling" },
                    { new Guid("a1b2c3d4-0002-0000-0000-000000000002"), "Grain and cereal harvesting", "Combine Harvesting" },
                    { new Guid("a1b2c3d4-0003-0000-0000-000000000003"), "Crop protection and fertilizer spraying", "Spraying" },
                    { new Guid("a1b2c3d4-0004-0000-0000-000000000004"), "Ploughing, discing, and cultivating", "Soil Preparation" },
                    { new Guid("a1b2c3d4-0005-0000-0000-000000000005"), "Precision and broadcast seeding", "Seeding" },
                    { new Guid("a1b2c3d4-0006-0000-0000-000000000006"), "Grass and hay mowing services", "Mowing" },
                    { new Guid("a1b2c3d4-0007-0000-0000-000000000007"), "Agricultural cargo transport", "Transport" }
                });

            migrationBuilder.InsertData(
                schema: "marketplace",
                table: "Municipalities",
                columns: new[] { "Id", "CountyId", "EhakCode", "Name" },
                values: new object[,]
                {
                    { new Guid("b0000000-0000-0000-0000-000000000001"), new Guid("a0000000-0000-0000-0000-000000000001"), "0784", "Tallinn" },
                    { new Guid("b0000000-0000-0000-0000-000000000002"), new Guid("a0000000-0000-0000-0000-000000000001"), "0141", "Anija vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000003"), new Guid("a0000000-0000-0000-0000-000000000001"), "0198", "Harku vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000004"), new Guid("a0000000-0000-0000-0000-000000000001"), "0245", "Jõelähtme vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000005"), new Guid("a0000000-0000-0000-0000-000000000001"), "0296", "Keila linn" },
                    { new Guid("b0000000-0000-0000-0000-000000000006"), new Guid("a0000000-0000-0000-0000-000000000001"), "0303", "Kiili vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000007"), new Guid("a0000000-0000-0000-0000-000000000001"), "0338", "Kose vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000008"), new Guid("a0000000-0000-0000-0000-000000000001"), "0353", "Kuusalu vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000009"), new Guid("a0000000-0000-0000-0000-000000000001"), "0424", "Loksa linn" },
                    { new Guid("b0000000-0000-0000-0000-00000000000a"), new Guid("a0000000-0000-0000-0000-000000000001"), "0431", "Lääne-Harju vald" },
                    { new Guid("b0000000-0000-0000-0000-00000000000b"), new Guid("a0000000-0000-0000-0000-000000000001"), "0446", "Maardu linn" },
                    { new Guid("b0000000-0000-0000-0000-00000000000c"), new Guid("a0000000-0000-0000-0000-000000000001"), "0651", "Raasiku vald" },
                    { new Guid("b0000000-0000-0000-0000-00000000000d"), new Guid("a0000000-0000-0000-0000-000000000001"), "0653", "Rae vald" },
                    { new Guid("b0000000-0000-0000-0000-00000000000e"), new Guid("a0000000-0000-0000-0000-000000000001"), "0718", "Saku vald" },
                    { new Guid("b0000000-0000-0000-0000-00000000000f"), new Guid("a0000000-0000-0000-0000-000000000001"), "0726", "Saue vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000010"), new Guid("a0000000-0000-0000-0000-000000000001"), "0890", "Viimsi vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000011"), new Guid("a0000000-0000-0000-0000-000000000002"), "0205", "Hiiumaa vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000012"), new Guid("a0000000-0000-0000-0000-000000000003"), "0130", "Alutaguse vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000013"), new Guid("a0000000-0000-0000-0000-000000000003"), "0251", "Jõhvi vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000014"), new Guid("a0000000-0000-0000-0000-000000000003"), "0321", "Kohtla-Järve linn" },
                    { new Guid("b0000000-0000-0000-0000-000000000015"), new Guid("a0000000-0000-0000-0000-000000000003"), "0442", "Lüganuse vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000016"), new Guid("a0000000-0000-0000-0000-000000000003"), "0511", "Narva linn" },
                    { new Guid("b0000000-0000-0000-0000-000000000017"), new Guid("a0000000-0000-0000-0000-000000000003"), "0514", "Narva-Jõesuu linn" },
                    { new Guid("b0000000-0000-0000-0000-000000000018"), new Guid("a0000000-0000-0000-0000-000000000003"), "0735", "Sillamäe linn" },
                    { new Guid("b0000000-0000-0000-0000-000000000019"), new Guid("a0000000-0000-0000-0000-000000000003"), "0803", "Toila vald" },
                    { new Guid("b0000000-0000-0000-0000-00000000001a"), new Guid("a0000000-0000-0000-0000-000000000004"), "0247", "Jõgeva vald" },
                    { new Guid("b0000000-0000-0000-0000-00000000001b"), new Guid("a0000000-0000-0000-0000-000000000004"), "0486", "Mustvee vald" },
                    { new Guid("b0000000-0000-0000-0000-00000000001c"), new Guid("a0000000-0000-0000-0000-000000000004"), "0618", "Põltsamaa vald" },
                    { new Guid("b0000000-0000-0000-0000-00000000001d"), new Guid("a0000000-0000-0000-0000-000000000005"), "0255", "Järva vald" },
                    { new Guid("b0000000-0000-0000-0000-00000000001e"), new Guid("a0000000-0000-0000-0000-000000000005"), "0567", "Paide linn" },
                    { new Guid("b0000000-0000-0000-0000-00000000001f"), new Guid("a0000000-0000-0000-0000-000000000005"), "0834", "Türi vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000020"), new Guid("a0000000-0000-0000-0000-000000000006"), "0184", "Haapsalu linn" },
                    { new Guid("b0000000-0000-0000-0000-000000000021"), new Guid("a0000000-0000-0000-0000-000000000006"), "0441", "Lääne-Nigula vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000022"), new Guid("a0000000-0000-0000-0000-000000000007"), "0191", "Haljala vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000023"), new Guid("a0000000-0000-0000-0000-000000000007"), "0272", "Kadrina vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000024"), new Guid("a0000000-0000-0000-0000-000000000007"), "0663", "Rakvere linn" },
                    { new Guid("b0000000-0000-0000-0000-000000000025"), new Guid("a0000000-0000-0000-0000-000000000007"), "0661", "Rakvere vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000026"), new Guid("a0000000-0000-0000-0000-000000000007"), "0792", "Tapa vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000027"), new Guid("a0000000-0000-0000-0000-000000000007"), "0897", "Vinni vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000028"), new Guid("a0000000-0000-0000-0000-000000000007"), "0903", "Viru-Nigula vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000029"), new Guid("a0000000-0000-0000-0000-000000000007"), "0928", "Väike-Maarja vald" },
                    { new Guid("b0000000-0000-0000-0000-00000000002a"), new Guid("a0000000-0000-0000-0000-000000000008"), "0284", "Kanepi vald" },
                    { new Guid("b0000000-0000-0000-0000-00000000002b"), new Guid("a0000000-0000-0000-0000-000000000008"), "0622", "Põlva vald" },
                    { new Guid("b0000000-0000-0000-0000-00000000002c"), new Guid("a0000000-0000-0000-0000-000000000008"), "0708", "Räpina vald" },
                    { new Guid("b0000000-0000-0000-0000-00000000002d"), new Guid("a0000000-0000-0000-0000-000000000009"), "0214", "Häädemeeste vald" },
                    { new Guid("b0000000-0000-0000-0000-00000000002e"), new Guid("a0000000-0000-0000-0000-000000000009"), "0305", "Kihnu vald" },
                    { new Guid("b0000000-0000-0000-0000-00000000002f"), new Guid("a0000000-0000-0000-0000-000000000009"), "0430", "Lääneranna vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000030"), new Guid("a0000000-0000-0000-0000-000000000009"), "0638", "Põhja-Pärnumaa vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000031"), new Guid("a0000000-0000-0000-0000-000000000009"), "0624", "Pärnu linn" },
                    { new Guid("b0000000-0000-0000-0000-000000000032"), new Guid("a0000000-0000-0000-0000-000000000009"), "0712", "Saarde vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000033"), new Guid("a0000000-0000-0000-0000-000000000009"), "0809", "Tori vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000034"), new Guid("a0000000-0000-0000-0000-00000000000a"), "0293", "Kehtna vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000035"), new Guid("a0000000-0000-0000-0000-00000000000a"), "0317", "Kohila vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000036"), new Guid("a0000000-0000-0000-0000-00000000000a"), "0503", "Märjamaa vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000037"), new Guid("a0000000-0000-0000-0000-00000000000a"), "0669", "Rapla vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000038"), new Guid("a0000000-0000-0000-0000-00000000000b"), "0478", "Muhu vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000039"), new Guid("a0000000-0000-0000-0000-00000000000b"), "0714", "Saaremaa vald" },
                    { new Guid("b0000000-0000-0000-0000-00000000003a"), new Guid("a0000000-0000-0000-0000-00000000000c"), "0171", "Elva vald" },
                    { new Guid("b0000000-0000-0000-0000-00000000003b"), new Guid("a0000000-0000-0000-0000-00000000000c"), "0283", "Kambja vald" },
                    { new Guid("b0000000-0000-0000-0000-00000000003c"), new Guid("a0000000-0000-0000-0000-00000000000c"), "0291", "Kastre vald" },
                    { new Guid("b0000000-0000-0000-0000-00000000003d"), new Guid("a0000000-0000-0000-0000-00000000000c"), "0432", "Luunja vald" },
                    { new Guid("b0000000-0000-0000-0000-00000000003e"), new Guid("a0000000-0000-0000-0000-00000000000c"), "0528", "Nõo vald" },
                    { new Guid("b0000000-0000-0000-0000-00000000003f"), new Guid("a0000000-0000-0000-0000-00000000000c"), "0586", "Peipsiääre vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000040"), new Guid("a0000000-0000-0000-0000-00000000000c"), "0793", "Tartu linn" },
                    { new Guid("b0000000-0000-0000-0000-000000000041"), new Guid("a0000000-0000-0000-0000-00000000000c"), "0796", "Tartu vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000042"), new Guid("a0000000-0000-0000-0000-00000000000d"), "0557", "Otepää vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000043"), new Guid("a0000000-0000-0000-0000-00000000000d"), "0824", "Tõrva vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000044"), new Guid("a0000000-0000-0000-0000-00000000000d"), "0855", "Valga vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000045"), new Guid("a0000000-0000-0000-0000-00000000000e"), "0480", "Mulgi vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000046"), new Guid("a0000000-0000-0000-0000-00000000000e"), "0615", "Põhja-Sakala vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000047"), new Guid("a0000000-0000-0000-0000-00000000000e"), "0899", "Viljandi linn" },
                    { new Guid("b0000000-0000-0000-0000-000000000048"), new Guid("a0000000-0000-0000-0000-00000000000e"), "0901", "Viljandi vald" },
                    { new Guid("b0000000-0000-0000-0000-000000000049"), new Guid("a0000000-0000-0000-0000-00000000000f"), "0142", "Antsla vald" },
                    { new Guid("b0000000-0000-0000-0000-00000000004a"), new Guid("a0000000-0000-0000-0000-00000000000f"), "0698", "Rõuge vald" },
                    { new Guid("b0000000-0000-0000-0000-00000000004b"), new Guid("a0000000-0000-0000-0000-00000000000f"), "0732", "Setomaa vald" },
                    { new Guid("b0000000-0000-0000-0000-00000000004c"), new Guid("a0000000-0000-0000-0000-00000000000f"), "0919", "Võru linn" },
                    { new Guid("b0000000-0000-0000-0000-00000000004d"), new Guid("a0000000-0000-0000-0000-00000000000f"), "0917", "Võru vald" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Availabilities_IsBooked",
                schema: "marketplace",
                table: "Availabilities",
                column: "IsBooked");

            migrationBuilder.CreateIndex(
                name: "IX_Availabilities_ServiceListingId",
                schema: "marketplace",
                table: "Availabilities",
                column: "ServiceListingId");

            migrationBuilder.CreateIndex(
                name: "IX_Counties_EhakCode",
                schema: "marketplace",
                table: "Counties",
                column: "EhakCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Locations_MunicipalityId",
                schema: "marketplace",
                table: "Locations",
                column: "MunicipalityId");

            migrationBuilder.CreateIndex(
                name: "IX_Municipalities_CountyId",
                schema: "marketplace",
                table: "Municipalities",
                column: "CountyId");

            migrationBuilder.CreateIndex(
                name: "IX_Municipalities_EhakCode",
                schema: "marketplace",
                table: "Municipalities",
                column: "EhakCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCategories_Name",
                schema: "marketplace",
                table: "ServiceCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceListingEquipments_EquipmentId",
                schema: "marketplace",
                table: "ServiceListingEquipments",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceListings_IsActive",
                schema: "marketplace",
                table: "ServiceListings",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceListings_LocationId",
                schema: "marketplace",
                table: "ServiceListings",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceListings_ServiceCategoryId",
                schema: "marketplace",
                table: "ServiceListings",
                column: "ServiceCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Availabilities",
                schema: "marketplace");

            migrationBuilder.DropTable(
                name: "ServiceListingEquipments",
                schema: "marketplace");

            migrationBuilder.DropTable(
                name: "Equipments",
                schema: "marketplace");

            migrationBuilder.DropTable(
                name: "ServiceListings",
                schema: "marketplace");

            migrationBuilder.DropTable(
                name: "Locations",
                schema: "marketplace");

            migrationBuilder.DropTable(
                name: "ServiceCategories",
                schema: "marketplace");

            migrationBuilder.DropTable(
                name: "Municipalities",
                schema: "marketplace");

            migrationBuilder.DropTable(
                name: "Counties",
                schema: "marketplace");
        }
    }
}
