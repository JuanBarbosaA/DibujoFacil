using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovedTutorialTrigger : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TRIGGER trg_AfterTutorialApproved
                ON Tutorial
                AFTER UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    UPDATE [User]
                    SET Points = Points + 200
                    FROM [User]
                    INNER JOIN (
                        SELECT i.AuthorId
                        FROM inserted i
                        INNER JOIN deleted d ON i.Id = d.Id
                        WHERE d.Status <> 'approved' 
                        AND i.Status = 'approved'
                    ) AS ApprovedTutorials 
                    ON [User].Id = ApprovedTutorials.AuthorId;
                END;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER trg_AfterTutorialApproved");
        }
    }
}
