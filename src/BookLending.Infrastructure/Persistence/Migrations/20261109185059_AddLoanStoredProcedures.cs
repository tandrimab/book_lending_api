using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookLending.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLoanStoredProcedures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE dbo.usp_GetLoansByMember
    @MemberId NVARCHAR(36)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, MemberId, BookId, LoanDate, DueDate, ReturnedOn FROM Loans WHERE MemberId = @MemberId ORDER BY LoanDate DESC
END");

            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE dbo.usp_CreateLoan
    @Id       NVARCHAR(36),
    @MemberId NVARCHAR(36),
    @BookId   NVARCHAR(36),
    @LoanDate DATETIME2,
    @DueDate  DATETIME2,
    @ReturnedOn DATETIME2
    AS
    BEGIN
        SET NOCOUNT ON;
        INSERT INTO Loans (Id, MemberId, BookId, LoanDate, DueDate, ReturnedOn) VALUES (@Id, @MemberId, @BookId, @LoanDate, @DueDate, NULL)
END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.usp_GetLoansByMember");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.usp_CreateLoan");
        }

    }
}
