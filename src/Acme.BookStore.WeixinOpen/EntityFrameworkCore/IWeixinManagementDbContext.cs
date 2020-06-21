using Acme.BookStore.WeixinOpen.Domain;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Acme.BookStore.WeixinOpen.EntityFrameworkCore
{
    public interface IWeixinManagementDbContext : IEfCoreDbContext
    {
        [ConnectionStringName("Weixin")]
        DbSet<WechatUserinfo> WechatUserinfos { get; set; }
    }
}