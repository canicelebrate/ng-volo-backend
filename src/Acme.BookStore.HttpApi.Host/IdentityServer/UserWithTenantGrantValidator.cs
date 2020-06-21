﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Validation;

namespace Acme.BookStore.IdentityServer
{
    public class UserWithTenantGrantValidator : IExtensionGrantValidator
    {
        public string GrantType => ExtensionGrantTypes.UserWithTenantGrantType;

        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            var userid = context.Request.Raw.Get("user_id");
            var tenantid = context.Request.Raw.Get("tenantid");

            if (string.IsNullOrEmpty(userid))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant);
                await Task.CompletedTask;
            }

            var claimList = new List<Claim> { new Claim("tenantid", tenantid) };

            context.Result = new GrantValidationResult(
                subject: userid,
                authenticationMethod: ExtensionGrantTypes.UserWithTenantGrantType,
                claims: claimList
            );

            //下一步还会在 AbpProfileService中赋值tenantId后调用
            //usermananger.finduser验证调用实际数据库 IsActiveAsync

            await Task.CompletedTask;
        }

        static class ExtensionGrantTypes
        {
            public const string UserWithTenantGrantType = "UserWithTenant";
        }
    }
}
