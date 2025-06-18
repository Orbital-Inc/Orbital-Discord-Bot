using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MainBot.Migrations;

/// <inheritdoc />
public partial class InitialCreation : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Errors",
            columns: table => new
            {
                key = table.Column<Guid>(type: "uuid", nullable: false),
                errorTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                source = table.Column<string>(type: "text", nullable: true),
                message = table.Column<string>(type: "text", nullable: true),
                stackTrace = table.Column<string>(type: "text", nullable: true),
                extraInformation = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Errors", x => x.key);
            });

        migrationBuilder.CreateTable(
            name: "GuildSettings",
            columns: table => new
            {
                key = table.Column<Guid>(type: "uuid", nullable: false),
                verifyRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                muteRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                rainbowRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                administratorRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                moderatorRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                hiddenRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                userLogChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                messageLogChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                systemLogChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                commandLogChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                ticketCategoryId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                uglyColours = table.Column<long[]>(type: "bigint[]", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GuildSettings", x => x.key);
            });

        migrationBuilder.CreateTable(
            name: "MutedUsers",
            columns: table => new
            {
                key = table.Column<Guid>(type: "uuid", nullable: false),
                id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                muteExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                guildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                muteRoleId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MutedUsers", x => x.key);
            });

        migrationBuilder.CreateTable(
            name: "NukeChannels",
            columns: table => new
            {
                key = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "text", nullable: true),
                id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                guildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_NukeChannels", x => x.key);
            });

        migrationBuilder.CreateTable(
            name: "Guilds",
            columns: table => new
            {
                key = table.Column<Guid>(type: "uuid", nullable: false),
                id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                name = table.Column<string>(type: "text", nullable: true),
                guildSettingskey = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Guilds", x => x.key);
                table.ForeignKey(
                    name: "FK_Guilds_GuildSettings_guildSettingskey",
                    column: x => x.guildSettingskey,
                    principalTable: "GuildSettings",
                    principalColumn: "key",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Guilds_guildSettingskey",
            table: "Guilds",
            column: "guildSettingskey");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Errors");

        migrationBuilder.DropTable(
            name: "Guilds");

        migrationBuilder.DropTable(
            name: "MutedUsers");

        migrationBuilder.DropTable(
            name: "NukeChannels");

        migrationBuilder.DropTable(
            name: "GuildSettings");
    }
}
