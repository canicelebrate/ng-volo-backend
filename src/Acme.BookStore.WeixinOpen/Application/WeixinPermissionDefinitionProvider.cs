using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acme.BookStore.WeixinOpen.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Acme.BookStore.WeixinOpen.Application
{
    public class WeixinPermissionDefinitionProvider : PermissionDefinitionProvider
    {
        public override void Define(IPermissionDefinitionContext context)
        {
            var productManagementGroup = context.AddGroup(WeixinPermisssions.GroupName, L("Permission:WeixinManagement"));

            var products = productManagementGroup.AddPermission(WeixinPermisssions.ChangeSettings.Default, L("Permission:ChangeSettings"));
            products.AddChild(WeixinPermisssions.ChangeSettings.Update, L("Permission:ChangeSettings:Edit"));
        }

        private static LocalizableString L(string name)
        {
            return LocalizableString.Create<WeixinOpenResource>(name);
        }
    }
}
