using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MDRN_HATS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlignDropsAndPickupPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DropId",
                table: "Products",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProofUrl",
                table: "Payments",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAtUtc",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewedByUserId",
                table: "Payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Orders",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "CustomerEmail",
                table: "Orders",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "Orders",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerPhone",
                table: "Orders",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress",
                table: "Orders",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryInstructions",
                table: "Orders",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryMethod",
                table: "Orders",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "HomeDelivery");

            migrationBuilder.AddColumn<string>(
                name: "PickupPointAddressSnapshot",
                table: "Orders",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PickupPointId",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickupPointNameSnapshot",
                table: "Orders",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                table: "Orders",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "VariantDescription",
                table: "OrderItems",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Drops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Slug = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    CollaboratorName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    BannerUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    PrimaryColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    SecondaryColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    BackgroundColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    TextColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    ButtonColor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    StartsAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndsAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drops", x => x.Id);
                    table.CheckConstraint("CK_Drops_BackgroundColor_Format", "\"BackgroundColor\" IS NULL OR \"BackgroundColor\" ~ '^#[0-9A-Fa-f]{6}$'");
                    table.CheckConstraint("CK_Drops_ButtonColor_Format", "\"ButtonColor\" IS NULL OR \"ButtonColor\" ~ '^#[0-9A-Fa-f]{6}$'");
                    table.CheckConstraint("CK_Drops_PrimaryColor_Format", "\"PrimaryColor\" IS NULL OR \"PrimaryColor\" ~ '^#[0-9A-Fa-f]{6}$'");
                    table.CheckConstraint("CK_Drops_SecondaryColor_Format", "\"SecondaryColor\" IS NULL OR \"SecondaryColor\" ~ '^#[0-9A-Fa-f]{6}$'");
                    table.CheckConstraint("CK_Drops_Slug_Format", "\"Slug\" ~ '^[a-z0-9-]+$'");
                    table.CheckConstraint("CK_Drops_TextColor_Format", "\"TextColor\" IS NULL OR \"TextColor\" ~ '^#[0-9A-Fa-f]{6}$'");
                });

            migrationBuilder.CreateTable(
                name: "PickupPoints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Instructions = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PickupPoints", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_DropId",
                table: "Products",
                column: "DropId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ReviewedByUserId",
                table: "Payments",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PickupPointId",
                table: "Orders",
                column: "PickupPointId");

            migrationBuilder.CreateIndex(
                name: "IX_Drops_Active_StartsAtUtc_EndsAtUtc",
                table: "Drops",
                columns: new[] { "Active", "StartsAtUtc", "EndsAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Drops_Slug",
                table: "Drops",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PickupPoints_Active",
                table: "PickupPoints",
                column: "Active");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_PickupPoints_PickupPointId",
                table: "Orders",
                column: "PickupPointId",
                principalTable: "PickupPoints",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_AspNetUsers_ReviewedByUserId",
                table: "Payments",
                column: "ReviewedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Drops_DropId",
                table: "Products",
                column: "DropId",
                principalTable: "Drops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_PickupPoints_PickupPointId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_AspNetUsers_ReviewedByUserId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Drops_DropId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Drops");

            migrationBuilder.DropTable(
                name: "PickupPoints");

            migrationBuilder.DropIndex(
                name: "IX_Products_DropId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Payments_ReviewedByUserId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PickupPointId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DropId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProofUrl",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ReviewedAtUtc",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CustomerEmail",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CustomerPhone",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryInstructions",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryMethod",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PickupPointAddressSnapshot",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PickupPointId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PickupPointNameSnapshot",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "VariantDescription",
                table: "OrderItems");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Orders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
