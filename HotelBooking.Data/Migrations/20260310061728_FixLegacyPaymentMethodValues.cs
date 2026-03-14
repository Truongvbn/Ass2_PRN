using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelBooking.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixLegacyPaymentMethodValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE [Payments]
                SET [Method] = CASE
                    WHEN [Method] IN (N'Card', N'Credit Card', N'CreditCard', N'Credit') THEN N'CreditCard'
                    WHEN [Method] IN (N'Debit Card', N'DebitCard', N'Debit') THEN N'DebitCard'
                    WHEN [Method] IN (N'Bank', N'Bank Transfer', N'BankTransfer', N'Transfer') THEN N'BankTransfer'
                    WHEN [Method] IN (N'Cash', N'CashPayment') THEN N'Cash'
                    ELSE [Method]
                END
                WHERE [Method] IS NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No down-migration for data normalization.
        }
    }
}
