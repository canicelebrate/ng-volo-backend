using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Senparc.CO2NET;
using Senparc.Weixin.Entities;
using Senparc.Weixin.RegisterServices;
using Senparc.Weixin.WxOpen;
using Senparc.Weixin.MP;
using Senparc.CO2NET.RegisterServices;
using Senparc.CO2NET.AspNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Senparc.Weixin;

namespace Acme.BookStore.WeixinOpen.Extensions
{
    public static class ApplicationBuilderExt
    {
        public static void UseSenparc(this IApplicationBuilder app, IWebHostEnvironment env,
                SenparcSetting senparcSetting, SenparcWeixinSetting senparcWeixinSetting)
        {
            app.UseSenparcGlobal(env, senparcSetting, register =>
             {

             }, true).
             UseSenparcWeixin(senparcWeixinSetting, register =>
             {

                 register.RegisterWxOpenAccount(senparcWeixinSetting);

             });



            //var register = RegisterService.Start(senparcSetting.Value).UseSenparcGlobal(false);

            //register.UseSenparcWeixin(senparcWeixinSetting.Value, senparcSetting.Value);
            //register.RegisterWxOpenAccount(senparcWeixinSetting.Value);

        }
    }
}
