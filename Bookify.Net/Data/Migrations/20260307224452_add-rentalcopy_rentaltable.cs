using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookify.Net.Data.Migrations
{
    /// <inheritdoc />
    public partial class addrentalcopy_rentaltable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Rentals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubsecriberId = table.Column<int>(type: "int", nullable: false),
                    StratDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PanaltyPaid = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LastUpdateedById = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rentals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rentals_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Rentals_AspNetUsers_LastUpdateedById",
                        column: x => x.LastUpdateedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Rentals_Subsecribers_SubsecriberId",
                        column: x => x.SubsecriberId,
                        principalTable: "Subsecribers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RentalCopies",
                columns: table => new
                {
                    RentalId = table.Column<int>(type: "int", nullable: false),
                    bookCopyId = table.Column<int>(type: "int", nullable: false),
                    RentalDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtendedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentalCopies", x => new { x.RentalId, x.bookCopyId });
                    table.ForeignKey(
                        name: "FK_RentalCopies_BookCopies_bookCopyId",
                        column: x => x.bookCopyId,
                        principalTable: "BookCopies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RentalCopies_Rentals_RentalId",
                        column: x => x.RentalId,
                        principalTable: "Rentals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RentalCopies_bookCopyId",
                table: "RentalCopies",
                column: "bookCopyId");

            migrationBuilder.CreateIndex(
                name: "IX_Rentals_CreatedById",
                table: "Rentals",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Rentals_LastUpdateedById",
                table: "Rentals",
                column: "LastUpdateedById");

            migrationBuilder.CreateIndex(
                name: "IX_Rentals_SubsecriberId",
                table: "Rentals",
                column: "SubsecriberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RentalCopies");

            migrationBuilder.DropTable(
                name: "Rentals");
        }
    }
}
