using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Zs.Bot.Data.PostgreSQL;

#nullable disable

namespace Home.Data.Migrations
{
    public partial class InitialHomeContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "vk");

            migrationBuilder.EnsureSchema(
                name: "bot");

            migrationBuilder.CreateTable(
                name: "chat_types",
                schema: "bot",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    name = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "commands",
                schema: "bot",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    script = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    default_args = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    group = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_commands", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "message_types",
                schema: "bot",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_message_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "messengers",
                schema: "bot",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messengers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                schema: "bot",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    permissions = table.Column<string>(type: "json", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "vk",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    first_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    last_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    raw_data = table.Column<string>(type: "json", nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    insert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "chats",
                schema: "bot",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    chat_type_id = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    raw_data = table.Column<string>(type: "json", nullable: false),
                    raw_data_hash = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    raw_data_history = table.Column<string>(type: "json", nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    insert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chats", x => x.id);
                    table.ForeignKey(
                        name: "FK_chats_chat_types_chat_type_id",
                        column: x => x.chat_type_id,
                        principalSchema: "bot",
                        principalTable: "chat_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "bot",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    full_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    user_role_id = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    is_bot = table.Column<bool>(type: "bool", nullable: false),
                    raw_data = table.Column<string>(type: "json", nullable: false),
                    raw_data_hash = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    raw_data_history = table.Column<string>(type: "json", nullable: true),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    insert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_user_roles_user_role_id",
                        column: x => x.user_role_id,
                        principalSchema: "bot",
                        principalTable: "user_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "activity_log",
                schema: "vk",
                columns: table => new
                {
                    activity_log_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    is_online = table.Column<bool>(type: "boolean", nullable: true),
                    insert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    online_app = table.Column<int>(type: "integer", nullable: true),
                    is_online_mobile = table.Column<bool>(type: "boolean", nullable: false),
                    last_seen = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activity_log", x => x.activity_log_id);
                    table.ForeignKey(
                        name: "FK_activity_log_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "vk",
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                schema: "bot",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    reply_to_message_id = table.Column<int>(type: "integer", nullable: true),
                    messenger_id = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    message_type_id = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    chat_id = table.Column<int>(type: "integer", nullable: false),
                    text = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    raw_data = table.Column<string>(type: "json", nullable: false),
                    raw_data_hash = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    raw_data_history = table.Column<string>(type: "json", nullable: true),
                    is_succeed = table.Column<bool>(type: "bool", nullable: false),
                    fails_count = table.Column<int>(type: "integer", nullable: false),
                    fail_description = table.Column<string>(type: "json", nullable: true),
                    is_deleted = table.Column<bool>(type: "bool", nullable: false),
                    update_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    insert_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_messages_chats_chat_id",
                        column: x => x.chat_id,
                        principalSchema: "bot",
                        principalTable: "chats",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_messages_message_types_message_type_id",
                        column: x => x.message_type_id,
                        principalSchema: "bot",
                        principalTable: "message_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_messages_messages_reply_to_message_id",
                        column: x => x.reply_to_message_id,
                        principalSchema: "bot",
                        principalTable: "messages",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_messages_messengers_messenger_id",
                        column: x => x.messenger_id,
                        principalSchema: "bot",
                        principalTable: "messengers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_messages_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "bot",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "bot",
                table: "chat_types",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { "CHANNEL", "Channel" },
                    { "GROUP", "Group" },
                    { "PRIVATE", "Private" },
                    { "UNDEFINED", "Undefined" }
                });

            migrationBuilder.InsertData(
                schema: "bot",
                table: "commands",
                columns: new[] { "id", "default_args", "description", "group", "script" },
                values: new object[,]
                {
                    { "/help", "<UserRoleId>", "Получение справки по доступным функциям", "userCmdGroup", "SELECT bot.sf_cmd_get_help({0})" },
                    { "/nulltest", null, "Тестовый запрос к боту. Возвращает NULL", "moderatorCmdGroup", "SELECT null" },
                    { "/sqlquery", "select 'Pass your query as a parameter in double quotes'", "SQL-запрос", "adminCmdGroup", "select (with userQuery as ({0}) select json_agg(q) from userQuery q)" },
                    { "/test", null, "Тестовый запрос к боту. Возвращает ''Test''", "moderatorCmdGroup", "SELECT 'Test'" }
                });

            migrationBuilder.InsertData(
                schema: "bot",
                table: "message_types",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { "AUD", "Audio" },
                    { "CNT", "Contact" },
                    { "DOC", "Document" },
                    { "LOC", "Location" },
                    { "OTH", "Other" },
                    { "PHT", "Photo" },
                    { "SRV", "Service message" },
                    { "STK", "Sticker" },
                    { "TXT", "Text" },
                    { "UKN", "Unknown" },
                    { "VID", "Video" },
                    { "VOI", "Voice" }
                });

            migrationBuilder.InsertData(
                schema: "bot",
                table: "messengers",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { "DC", "Discord" },
                    { "FB", "Facebook" },
                    { "SK", "Skype" },
                    { "TG", "Telegram" },
                    { "VK", "Вконтакте" }
                });

            migrationBuilder.InsertData(
                schema: "bot",
                table: "user_roles",
                columns: new[] { "id", "name", "permissions" },
                values: new object[,]
                {
                    { "ADMIN", "Administrator", "[ \"adminCmdGroup\", \"moderatorCmdGroup\", \"userCmdGroup\" ]" },
                    { "MODERATOR", "Moderator", "[ \"moderatorCmdGroup\", \"userCmdGroup\" ]" },
                    { "OWNER", "Owner", "[ \"All\" ]" },
                    { "USER", "User", "[ \"userCmdGroup\" ]" }
                });

            migrationBuilder.InsertData(
                schema: "bot",
                table: "chats",
                columns: new[] { "id", "chat_type_id", "description", "insert_date", "name", "raw_data", "raw_data_hash", "raw_data_history" },
                values: new object[,]
                {
                    { -1, "PRIVATE", "IntegrationTestChat", new DateTime(2021, 11, 6, 12, 19, 45, 916, DateTimeKind.Utc).AddTicks(6758), "IntegrationTestChat", "{ \"test\": \"test\" }", "-1063294487", null },
                    { 1, "PRIVATE", null, new DateTime(2021, 11, 6, 12, 19, 45, 916, DateTimeKind.Utc).AddTicks(6763), "zuev56", "{ \"Id\": 210281448 }", "-1063294487", null }
                });

            migrationBuilder.InsertData(
                schema: "bot",
                table: "users",
                columns: new[] { "id", "full_name", "insert_date", "is_bot", "name", "raw_data", "raw_data_hash", "raw_data_history", "user_role_id" },
                values: new object[,]
                {
                    { -10, "for exported message reading", new DateTime(2021, 11, 6, 12, 19, 45, 916, DateTimeKind.Utc).AddTicks(6794), false, "Unknown", "{ \"test\": \"test\" }", "-1063294487", null, "USER" },
                    { -1, "IntegrationTest", new DateTime(2021, 11, 6, 12, 19, 45, 916, DateTimeKind.Utc).AddTicks(6798), false, "IntegrationTestUser", "{ \"test\": \"test\" }", "-1063294487", null, "USER" },
                    { 1, "Сергей Зуев", new DateTime(2021, 11, 6, 12, 19, 45, 916, DateTimeKind.Utc).AddTicks(6800), false, "zuev56", "{ \"Id\": 210281448 }", "-1063294487", null, "OWNER" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_activity_log_user_id",
                schema: "vk",
                table: "activity_log",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_chats_chat_type_id",
                schema: "bot",
                table: "chats",
                column: "chat_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_chat_id",
                schema: "bot",
                table: "messages",
                column: "chat_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_message_type_id",
                schema: "bot",
                table: "messages",
                column: "message_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_messenger_id",
                schema: "bot",
                table: "messages",
                column: "messenger_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_reply_to_message_id",
                schema: "bot",
                table: "messages",
                column: "reply_to_message_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_user_id",
                schema: "bot",
                table: "messages",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_user_role_id",
                schema: "bot",
                table: "users",
                column: "user_role_id");

            migrationBuilder.Sql(PostgreSqlBotContext.GetOtherSqlScripts(@"..\Home.Bot\appsettings.json"));
            migrationBuilder.Sql(Data.HomeContext.GetOtherSqlScripts(@"..\Home.Bot\appsettings.json"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activity_log",
                schema: "vk");

            migrationBuilder.DropTable(
                name: "commands",
                schema: "bot");

            migrationBuilder.DropTable(
                name: "messages",
                schema: "bot");

            migrationBuilder.DropTable(
                name: "users",
                schema: "vk");

            migrationBuilder.DropTable(
                name: "chats",
                schema: "bot");

            migrationBuilder.DropTable(
                name: "message_types",
                schema: "bot");

            migrationBuilder.DropTable(
                name: "messengers",
                schema: "bot");

            migrationBuilder.DropTable(
                name: "users",
                schema: "bot");

            migrationBuilder.DropTable(
                name: "chat_types",
                schema: "bot");

            migrationBuilder.DropTable(
                name: "user_roles",
                schema: "bot");
        }
    }
}
