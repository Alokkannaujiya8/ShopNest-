using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopNest.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryStockManagementModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Products_ProductId",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_ProductId_ProductVariantId",
                table: "Inventories");

            migrationBuilder.RenameColumn(
                name: "StockQuantity",
                table: "Inventories",
                newName: "ReservedStock");

            migrationBuilder.RenameColumn(
                name: "Location",
                table: "Inventories",
                newName: "Sku");

            migrationBuilder.AddColumn<string>(
                name: "PerformedBy",
                table: "InventoryTransactions",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PreviousStock",
                table: "InventoryTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "InventoryTransactions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionNumber",
                table: "InventoryTransactions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UpdatedStock",
                table: "InventoryTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AvailableStock",
                table: "Inventories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentStock",
                table: "Inventories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "LastPurchasePrice",
                table: "Inventories",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MaximumStockLevel",
                table: "Inventories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinimumStockLevel",
                table: "Inventories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReorderLevel",
                table: "Inventories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "SellingPrice",
                table: "Inventories",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitCost",
                table: "Inventories",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "WarehouseId",
                table: "Inventories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Warehouses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouses", x => x.Id);
                });

            // Seed default warehouse
            migrationBuilder.Sql("INSERT INTO Warehouses (Id, Name, Code, CreatedAtUtc, IsDeleted) VALUES ('E5B8E8DF-A5E8-4EF5-86A8-D0DDE5357AE0', 'Main Warehouse', 'WH-MAIN', GETUTCDATE(), 0)");

            // Bind existing inventories to default warehouse
            migrationBuilder.Sql("UPDATE Inventories SET WarehouseId = 'E5B8E8DF-A5E8-4EF5-86A8-D0DDE5357AE0'");

            // Transfer StockQuantity (renamed to ReservedStock) to CurrentStock & AvailableStock, and reset ReservedStock to 0
            migrationBuilder.Sql("UPDATE Inventories SET CurrentStock = ReservedStock, AvailableStock = ReservedStock, ReservedStock = 0");

            // Update Sku and pricing info from Products
            migrationBuilder.Sql("UPDATE i SET i.Sku = p.Sku, i.MinimumStockLevel = p.MinimumStock, i.MaximumStockLevel = p.MaximumStock, i.ReorderLevel = p.MinimumStock + 5, i.SellingPrice = p.Price, i.UnitCost = CASE WHEN p.CostPrice > 0 THEN p.CostPrice ELSE p.Price * 0.6 END, i.LastPurchasePrice = CASE WHEN p.CostPrice > 0 THEN p.CostPrice ELSE p.Price * 0.6 END FROM Inventories i JOIN Products p ON i.ProductId = p.Id");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_TransactionNumber",
                table: "InventoryTransactions",
                column: "TransactionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_ProductId_ProductVariantId_WarehouseId",
                table: "Inventories",
                columns: new[] { "ProductId", "ProductVariantId", "WarehouseId" },
                unique: true,
                filter: "[ProductVariantId] IS NOT NULL AND [WarehouseId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_WarehouseId",
                table: "Inventories",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_Code",
                table: "Warehouses",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Products_ProductId",
                table: "Inventories",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Warehouses_WarehouseId",
                table: "Inventories",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Products_ProductId",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Warehouses_WarehouseId",
                table: "Inventories");

            migrationBuilder.DropTable(
                name: "Warehouses");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransactions_TransactionNumber",
                table: "InventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_ProductId_ProductVariantId_WarehouseId",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_WarehouseId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "PerformedBy",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "PreviousStock",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "TransactionNumber",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "UpdatedStock",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "AvailableStock",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "CurrentStock",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "LastPurchasePrice",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "MaximumStockLevel",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "MinimumStockLevel",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "ReorderLevel",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "SellingPrice",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "UnitCost",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "Inventories");

            migrationBuilder.RenameColumn(
                name: "Sku",
                table: "Inventories",
                newName: "Location");

            migrationBuilder.RenameColumn(
                name: "ReservedStock",
                table: "Inventories",
                newName: "StockQuantity");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_ProductId_ProductVariantId",
                table: "Inventories",
                columns: new[] { "ProductId", "ProductVariantId" },
                unique: true,
                filter: "[ProductVariantId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Products_ProductId",
                table: "Inventories",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
