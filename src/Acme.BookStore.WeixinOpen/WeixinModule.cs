﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acme.BookStore.WeixinOpen.Application;
using Acme.BookStore.WeixinOpen.Domain;
using Acme.BookStore.WeixinOpen.EntityFrameworkCore;
using Acme.BookStore.WeixinOpen.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Senparc.CO2NET;
using Senparc.CO2NET.RegisterServices;
using Senparc.Weixin.Entities;
using Senparc.Weixin.RegisterServices;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AutoMapper;
using Volo.Abp.Caching;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.Settings;
using Volo.Abp.Validation.Localization;
using Volo.Abp.VirtualFileSystem;

namespace Acme.BookStore.WeixinOpen
{
    [DependsOn(
        typeof(AbpLocalizationModule),
        typeof(AbpCachingModule),
        // typeof(AbpHttpClientModule),
        // typeof(AbpAspNetCoreMvcModule),
        typeof(AbpAutoMapperModule)
    )]
    public class WeixinModule : AbpModule
    {

        public IConfiguration Configuration { get; private set; }


        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddAbpDbContext<WeixinManagementDbContext>(options =>
            {
                options.AddDefaultRepositories(true);
            });

            context.Services.AddAutoMapperObjectMapper<WeixinModule>();
            Configure<AbpAutoMapperOptions>(options =>
            {
                options.AddProfile<WeixinApplicationAutoMapperProfile>(validate: true);
            });


            // 为Wexin模块指定RootPath，这个RootPath将被用作module名称，
            // 这样就可以通过命令 abp generate-proxy -m weixin
            // 只生成本模块相关的代理脚本

            Configure<AbpAspNetCoreMvcOptions>(options =>
            {
                options.MinifyGeneratedScript = true;
                options.ConventionalControllers
                    .Create(typeof(WeixinModule).Assembly, opts =>
                    {
                        opts.RootPath = "weixin";
                    });
            });


            // 微信支付功能暂未实现
            //context.Services.AddHttpClient<IPayApi, PayApi>(
            //    cfg => { cfg.BaseAddress = new Uri("https://api.mch.weixin.qq.com/"); });

            //context.Services.AddSingleton<ISignatureGenerator, SignatureGenerator>();

            // CAP
            //context.Services.AddTransient<WexinCapSubscriberService>();


            context.Services.AddOptions<WexinOpenOptions>().
                Configure<ISettingProvider>( async (o, s) =>
                {
                    o.AppId = await s.GetOrNullAsync(WeixinManagementSetting.MiniAppId);
                    o.AppSecret = await s.GetOrNullAsync(WeixinManagementSetting.MiniAppSecret);
  
                });



            // Localization
            Configure<AbpVirtualFileSystemOptions>(options =>  
            {
                //为BookStoreDomainSharedModule进行内嵌文件系统配置
                options.FileSets.AddEmbedded<WeixinModule>("Acme.BookStore.WeixinOpen");
            }); // line 8

            Configure<AbpLocalizationOptions>(options =>  
            {
                // 1、将BookStoreResource添加到abp资源集合
                // 2、BookStoreResource相关的资源可以在虚拟json文件/Localization/BookStore中找到
                options.Resources
                    .Add<WeixinOpenResource>("en")//在AbpLocalizationOptions的Resources中加入新的BookStoreResource，默认语言为en
                    .AddVirtualJson("/Localization/WeixinOpen");//所有定义在/Localization/BookStore目录下的json文件数据被用来作为返回的本地化数据
            });

            Configuration = context.Services.GetConfiguration();

            context.Services.AddSenparcGlobalServices(Configuration)//Senparc.CO2NET 全局注册
                .AddSenparcWeixinServices(Configuration);//Senparc.Weixin 注册

        }



    }
}
