﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Settings;

namespace Acme.BookStore.WeixinOpen.Application
{
    public class WeixinSettingDefinitionProvider : SettingDefinitionProvider
    {
        public override void Define(ISettingDefinitionContext context)
        {
            context.Add(
                new SettingDefinition(WeixinManagementSetting.AppId),
                new SettingDefinition(WeixinManagementSetting.AppSecret),
                new SettingDefinition(WeixinManagementSetting.MiniAppId,isVisibleToClients:true),
                new SettingDefinition(WeixinManagementSetting.MiniAppSecret, isVisibleToClients: true),
                new SettingDefinition(WeixinManagementSetting.PayMchId),
                new SettingDefinition(WeixinManagementSetting.PayKey),
                new SettingDefinition(WeixinManagementSetting.PayNotify),
                new SettingDefinition(WeixinManagementSetting.CertPath),
                new SettingDefinition(WeixinManagementSetting.CertPassword)
            );
        }
    }

    public static class WeixinManagementSetting
    {
        private const string GroupName = "WeixinManagement";

        public const string AppId = GroupName + ".AppId";
        public const string AppSecret = GroupName + ".AppSecret";
        public const string MiniAppId = GroupName + ".MiniAppId";
        public const string MiniAppSecret = GroupName + ".MiniAppSecret";
        public const string PayMchId = GroupName + ".PayMchId";
        public const string PayKey = GroupName + ".PayKey";
        public const string PayNotify = GroupName + ".PayNotify";
        public const string CertPath = GroupName + ".CertPath";
        public const string CertPassword = GroupName + ".CertPassword";
    }
}
