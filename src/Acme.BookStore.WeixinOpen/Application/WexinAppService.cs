using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Acme.BookStore.WeixinOpen.Application.DTOs;
using Acme.BookStore.WeixinOpen.Application.Interfaces;
using Acme.BookStore.WeixinOpen.Domain;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Serilog;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.SettingManagement;
using Volo.Abp.Settings;
using Volo.Abp.Uow;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace Acme.BookStore.WeixinOpen.Application
{
    public class WexinAppService : ApplicationService, IWeixinAppService
    {
        private readonly IGuidGenerator _guidGenerator;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IPasswordHasher<IdentityUser> _passwordHasher;
        private readonly ICurrentTenant _currentTenant;
        private readonly ISettingProvider _setting;
        private readonly WeixinManager _weixinManager;
        private readonly IdentityUserStore _identityUserStore;
        //private readonly ICapPublisher _capBus;
        private readonly IUserClaimsPrincipalFactory<IdentityUser> _principalFactory;
        //private readonly IdentityServerOptions _options;
        private readonly IHttpContextAccessor _httpContextAccessor;
        //private readonly ITokenService _ts;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        //private readonly IAppProvider _appProvider;
        private WexinOpenOptions _options;
        private ISettingManager _settingManager;

        public WexinAppService(
            IGuidGenerator guidGenerator,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            IPasswordHasher<IdentityUser> passwordHasher,
            ICurrentTenant currentTenant,
            ISettingProvider setting,
            WeixinManager weixinManager,
            IdentityUserStore identityUserStore,
            //ICapPublisher capBus,
            IUserClaimsPrincipalFactory<IdentityUser> principalFactory,
            //IdentityServerOptions options,
            IHttpContextAccessor httpContextAccessor,
            //ITokenService TS,
            IUnitOfWorkManager unitOfWorkManager,
            //IAppProvider appProvider
            IOptions<WexinOpenOptions> optionsAccessor,
            ISettingManager settingManager
        )
        {
            ObjectMapperContext = typeof(WeixinModule);
            _guidGenerator = guidGenerator;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _passwordHasher = passwordHasher;
            _currentTenant = currentTenant;
            _setting = setting;
            _weixinManager = weixinManager;
            _identityUserStore = identityUserStore;
            //_capBus = capBus;
            _principalFactory = principalFactory;
            _httpContextAccessor = httpContextAccessor;
            _unitOfWorkManager = unitOfWorkManager;
            _options = optionsAccessor.Value;
            _settingManager = settingManager;
        }


        [HttpPost]
        [UnitOfWork(IsDisabled = false)]
        public async Task<MpAuthenticateResultModel> MiniAuth(MpAuthenticateModel loginModel)
        {
            try
            {


                //var app = await _appProvider.GetOrNullAsync(appName);
                //var appid = app["appid"] ?? throw new AbpException($"App:{appName} appid未设置");
                //var appSec = app["appsec"] ?? throw new AbpException($"App:{appName} appsec未设置");
                if (_options == null)
                {
                    throw new AbpException("小程序未配置");
                }
                var appid = _options.AppId ?? throw new AbpException($"App:{loginModel.appName} appid未设置");
                var appSec = _options.AppSecret ?? throw new AbpException($"App:{loginModel.appName} appsec未设置");
                var session = await _weixinManager.Mini_Code2Session(loginModel.code, _options.AppId, _options.AppSecret);

                // 解密用户信息
                var miniUserInfo =
                    await _weixinManager.Mini_GetUserInfo(appid, loginModel.encryptedData, session.session_key,
                        loginModel.iv);

                //miniUserInfo.AppName = appName;

                // 更新数据库
                //await _capBus.PublishAsync("weixin.services.mini.getuserinfo", miniUserInfo);

                // todo: 如果对应主体下只有一个微信小程序账号，那是获取不到unionId的，只能获得openId
                // 所以，下面的代码可能需要修正一下
                // 当unionid为空的时候， 第一个参数loginProvider可以考虑设置为appid_openid
                // 对应的 providerKey的值的格式为 {appid}_{OpenId}

                IdentityUser user = null;
                if(!String.IsNullOrEmpty(miniUserInfo.unionId))
                {
                    user = await _identityUserStore.FindByLoginAsync($"unionid", miniUserInfo.unionId);
                }
                if (user == null && !String.IsNullOrEmpty(miniUserInfo.openId))
                {
                    user = await _identityUserStore.FindByLoginAsync($"appid_openid", $"{appid}_{miniUserInfo.openId}");
                }

                if (user == null)
                {
                    var userId = _guidGenerator.Create();
                    var userName = string.Empty;
                    if (!string.IsNullOrWhiteSpace(miniUserInfo.unionId))
                    {
                        userName = miniUserInfo.unionId;
                    }
                    else
                    {
                        userName = $"{appid}_{miniUserInfo.openId}";
                    }

                    user = new IdentityUser(userId, userName, $"{userName}@somall.top",
                        _currentTenant.Id)
                    {
                        Name = miniUserInfo.nickName
                    };

                    using (var uow = _unitOfWorkManager.Begin(requiresNew: true))
                    {
                        var passHash = _passwordHasher.HashPassword(user, "1q2w3E*");
                        await _identityUserStore.CreateAsync(user);
                        await _identityUserStore.SetPasswordHashAsync(user, passHash);

                        if (!string.IsNullOrWhiteSpace(miniUserInfo.unionId))
                        {
                            await _identityUserStore.AddLoginAsync(user,
                                new UserLoginInfo($"unionid", miniUserInfo.unionId, "unionid"));
                        }


                        await _identityUserStore.AddLoginAsync(user,
                            new UserLoginInfo("appid_openid", $"{appid}_{miniUserInfo.openId}", "openid"));

                        await _unitOfWorkManager.Current.SaveChangesAsync();
                        await uow.CompleteAsync();
                    }
                }

                var serverClient = _httpClientFactory.CreateClient();
                var authServerUrl = _configuration["AuthServer:Authority"];

                //var disco = await serverClient.GetDiscoveryDocumentAsync(_configuration["AuthServer:Authority"]);


                var disco = await serverClient.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
                {
                    Address = authServerUrl,
                    Policy =
                    {
                        ValidateIssuerName = false,
                        ValidateEndpoints = false
                    }
                });

                if (disco.IsError)
                {
                    throw new AbpException(disco.Error);
                }


                var result = await serverClient.RequestTokenAsync(
                    new TokenRequest
                    {
                        Address = disco.TokenEndpoint,
                        GrantType = "UserWithTenant",

                        ClientId = _configuration["AuthServer:ClientId"],
                        ClientSecret = _configuration["AuthServer:ClientSecret"],
                        Parameters =
                        {
                        {"user_id", $"{user.Id}"},
                        {"tenantid", $"{user.TenantId}"},
                        {
                            "scope", "BookStore"
                        }
                        }
                    });

                var token = result.AccessToken;

                if (string.IsNullOrEmpty(token) || string.IsNullOrWhiteSpace(token))
                {
                    throw new AbpException("从IdentityServer获取Token失败。");
                }

                return await Task.FromResult(new MpAuthenticateResultModel
                {
                    AccessToken = token,
                    ExternalUser = miniUserInfo,
                    SessionKey = session.session_key
                });
            }
            catch(Exception ex)
            {
                Log.Logger.Error(ex, "小程序登录验证失败。");
                throw;
            }
        }

        [Authorize(WeixinPermisssions.ChangeSettings.Update)]
        public async Task<bool> UpdateSettings(IList<MPUpdateSettingItem> settings)
        {
            foreach (var item in settings)
            {
                await _settingManager.SetGlobalAsync(item.SettingKey,item.SettingValue);
            }

            return true;
        }


        //    [HttpGet]
        //    [Authorize]
        //    public async Task<object> CheckLogin(bool? dbCheck = false)
        //    {
        //        if (!dbCheck.HasValue)
        //        {
        //            return await Task.FromResult("ok");
        //        }

        //        return await Task.FromResult(CurrentUser);
        //    }
    }
}
