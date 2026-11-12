using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finaco.Api.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable("accounts", table => new
        {
            Id = table.Column<Guid>(type: "uuid", nullable: false), Name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false), Type = table.Column<int>(type: "integer", nullable: false), CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "TRY"), OpeningBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true), OpeningBalanceDate = table.Column<DateOnly>(type: "date", nullable: true), CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
        }, constraints: table => table.PrimaryKey("PK_accounts", x => x.Id));
        migrationBuilder.CreateTable("categories", table => new
        {
            Id = table.Column<Guid>(type: "uuid", nullable: false), Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false), Kind = table.Column<int>(type: "integer", nullable: false), IsArchived = table.Column<bool>(type: "boolean", nullable: false), CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
        }, constraints: table => table.PrimaryKey("PK_categories", x => x.Id));
        migrationBuilder.CreateTable("import_jobs", table => new
        {
            Id = table.Column<Guid>(type: "uuid", nullable: false), FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false), Status = table.Column<int>(type: "integer", nullable: false), CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
        }, constraints: table => table.PrimaryKey("PK_import_jobs", x => x.Id));
        migrationBuilder.CreateTable("transactions", table => new
        {
            Id = table.Column<Guid>(type: "uuid", nullable: false), AccountId = table.Column<Guid>(type: "uuid", nullable: false), TransactionDate = table.Column<DateOnly>(type: "date", nullable: false), DescriptionOriginal = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false), DescriptionNormalized = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true), Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false), CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "TRY"), Type = table.Column<int>(type: "integer", nullable: false), CategoryId = table.Column<Guid>(type: "uuid", nullable: true), CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false), UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_transactions", x => x.Id);
            table.ForeignKey("FK_transactions_accounts_AccountId", x => x.AccountId, "accounts", "Id", onDelete: ReferentialAction.Cascade);
            table.ForeignKey("FK_transactions_categories_CategoryId", x => x.CategoryId, "categories", "Id", onDelete: ReferentialAction.SetNull);
            table.CheckConstraint("ck_transactions_amount_positive", "amount > 0");
        });
        migrationBuilder.CreateTable("import_rows", table => new
        {
            Id = table.Column<Guid>(type: "uuid", nullable: false), ImportJobId = table.Column<Guid>(type: "uuid", nullable: false), RowNumber = table.Column<int>(type: "integer", nullable: false), ParsedValuesJson = table.Column<string>(type: "jsonb", nullable: false), ValidationErrorsJson = table.Column<string>(type: "jsonb", nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_import_rows", x => x.Id);
            table.ForeignKey("FK_import_rows_import_jobs_ImportJobId", x => x.ImportJobId, "import_jobs", "Id", onDelete: ReferentialAction.Cascade);
        });
        migrationBuilder.CreateIndex("IX_accounts_Name", "accounts", "Name");
        migrationBuilder.CreateIndex("IX_categories_Name_Kind", "categories", new[] { "Name", "Kind" }, unique: true);
        migrationBuilder.CreateIndex("IX_transactions_AccountId_TransactionDate", "transactions", new[] { "AccountId", "TransactionDate" });
        migrationBuilder.CreateIndex("IX_transactions_CategoryId", "transactions", "CategoryId");
        migrationBuilder.CreateIndex("IX_transactions_Type", "transactions", "Type");
        migrationBuilder.CreateIndex("IX_import_jobs_FileName_CreatedAt", "import_jobs", new[] { "FileName", "CreatedAt" });
        migrationBuilder.CreateIndex("IX_import_rows_ImportJobId_RowNumber", "import_rows", new[] { "ImportJobId", "RowNumber" }, unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("import_rows"); migrationBuilder.DropTable("transactions"); migrationBuilder.DropTable("accounts"); migrationBuilder.DropTable("categories"); migrationBuilder.DropTable("import_jobs");
    }
}

