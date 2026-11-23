using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shop_API.Migrations
{
    /// <inheritdoc />
    public partial class AddVariantImageUniqueMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ProductVariantImages_ProductVariantId_MediaId",
                table: "ProductVariantImages",
                columns: new[] { "ProductVariantId", "MediaId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductVariantImages_ProductVariantId_MediaId",
                table: "ProductVariantImages");
        }
    }
}
