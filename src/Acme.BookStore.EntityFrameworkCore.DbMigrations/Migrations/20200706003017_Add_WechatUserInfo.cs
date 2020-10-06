using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Acme.BookStore.Migrations
{
    public partial class Add_WechatUserInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Weixin_WechatUserinfos",
                columns: table => new
                {
                    appid = table.Column<string>(maxLength: 32, nullable: false),
                    openid = table.Column<string>(maxLength: 32, nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorId = table.Column<Guid>(nullable: true),
                    unionid = table.Column<string>(maxLength: 32, nullable: true),
                    nickname = table.Column<string>(maxLength: 32, nullable: true),
                    headimgurl = table.Column<string>(maxLength: 255, nullable: true),
                    city = table.Column<string>(maxLength: 255, nullable: true),
                    province = table.Column<string>(maxLength: 255, nullable: true),
                    country = table.Column<string>(maxLength: 255, nullable: true),
                    sex = table.Column<int>(nullable: false),
                    FromClient = table.Column<int>(nullable: false),
                    AppName = table.Column<string>(maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Weixin_WechatUserinfos", x => new { x.openid, x.appid });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Weixin_WechatUserinfos");
        }
    }
}
