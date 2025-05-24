using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddUserStatisticsDto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"
    CREATE PROCEDURE GetUserStatistics
    AS
    BEGIN
        SELECT 
            u.id AS UserId,
            u.name AS Name,
            u.email AS Email,
            COUNT(DISTINCT t.id) AS TutorialCount,
            COUNT(DISTINCT c.id) AS CommentCount,
            AVG(r.score) AS AverageRating
        FROM user u
        LEFT JOIN tutorial t ON u.id = t.author_id
        LEFT JOIN comment c ON u.id = c.user_id
        LEFT JOIN rating r ON u.id = r.user_id
        GROUP BY u.id, u.name, u.email
    END";

            migrationBuilder.Sql(sp);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS GetUserStatistics");
        }
    }
}
