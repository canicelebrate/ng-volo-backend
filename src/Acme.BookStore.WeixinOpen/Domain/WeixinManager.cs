using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.WxOpen.AdvancedAPIs.Sns;
using ServiceStack.Redis;
using Volo.Abp;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;
using Volo.Abp.Uow;
using Senparc.Weixin.WxOpen.Entities;
using Senparc.Weixin.WxOpen.Helpers;
using Serilog;

namespace Acme.BookStore.WeixinOpen.Domain
{
    public class WeixinManager : ITransientDependency
    {
        private readonly IDistributedCache<AccessTokeCacheItem> _tokenCache;
        private readonly IRedisClient _redisClient;
        private readonly IRepository<WechatUserinfo> _wechatUserRepository;
        private readonly Volo.Abp.ObjectMapping.IObjectMapper _mapper;
        private readonly ICurrentTenant _currentTenant;
        private readonly ISettingProvider _setting;


        public WeixinManager(
            IDistributedCache<AccessTokeCacheItem> tokenCache,
            IRedisClient redisClient,
            IRepository<WechatUserinfo> wechatUserRepository,
            Volo.Abp.ObjectMapping.IObjectMapper mapper,
            ICurrentTenant currentTenant,
            ISettingProvider setting)
        {
            _tokenCache = tokenCache;
            _redisClient = redisClient;
            _wechatUserRepository = wechatUserRepository;
            _mapper = mapper;
            _currentTenant = currentTenant;
            _setting = setting;
            //_weixinApi = weixinApi;
        }


        [UnitOfWork]
        public virtual async Task CreateOrUpdate(DecodedUserInfo userInfo, string appName)
        {
            var find = await _wechatUserRepository.FirstOrDefaultAsync(x => x.appid == userInfo.watermark.appid &&
                                                                            x.openid == userInfo.openId);
            if (find == null)
            {
                await _wechatUserRepository.InsertAsync(
                    new WechatUserinfo(userInfo.watermark.appid, userInfo.openId, userInfo.unionId, 
                        userInfo.nickName, userInfo.avatarUrl, appName: appName)
                    {
                        city = userInfo.city,
                        province = userInfo.province,
                        sex = userInfo.gender,
                        country = userInfo.country
                    });
            }
            else
            {
                _mapper.Map(userInfo, find);
            }
        }


        public async Task<JsCode2JsonResult> Mini_Code2Session(string code, string appid, string appsecret)
        {
            var session = await SnsApi.JsCode2JsonAsync(appid, appsecret, code);

            if (session == null)
            {
                throw new UserFriendlyException("解密失败");
            }

            return session;
        }

        public async Task<string> GetAccessTokenAsync(string appid, string appSeret)

        {
            var cache = await _tokenCache.GetOrAddAsync(
                appid, //Cache key
                async () => await GetAccessToken(appid, appSeret),
                () => new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddHours(1)
                }
            );
            if (cache != null)
            {
                if (cache.TimeExpired <= DateTimeOffset.Now)
                {
                    await _tokenCache.RemoveAsync(appid);
                    cache = await _tokenCache.GetOrAddAsync(
                        appid, //Cache key
                        async () => await GetAccessToken(appid, appSeret),
                        () => new DistributedCacheEntryOptions
                        {
                            AbsoluteExpiration = DateTimeOffset.Now.AddHours(1)
                        }
                    );
                }

                return cache.AccessToken;
            }

            throw new UserFriendlyException("token 获取失败");
        }

        public async Task<DecodedUserInfo> Mini_GetUserInfo(string appid, string encryptedDataStr, string sessionKey, string iv)
        {
            var json = EncryptHelper.DecodeEncryptedData(sessionKey,encryptedDataStr, iv);

            var userInfo = JsonConvert.DeserializeObject<DecodedUserInfo>(json);

            userInfo.watermark.appid = appid;

            return await Task.FromResult(userInfo);
        }

        private async Task<AccessTokeCacheItem> GetAccessToken(string appid, string appSeret)
        {
            var token = await CommonApi.GetTokenAsync(appid, appSeret);
            if (token.errcode == 0)
            {
                Log.Logger.Information(JsonConvert.SerializeObject(token));
                return new AccessTokeCacheItem()
                {
                    Appid = appid,
                    AppSecret = appSeret,
                    AccessToken = token.access_token,
                    TimeCreated = DateTimeOffset.Now,
                    TimeExpired = DateTimeOffset.Now.AddSeconds(token.expires_in)
                };
            }

            Log.Logger.Error($"token 获取失败: {token.errmsg}");
            Log.Logger.Error(JsonConvert.SerializeObject(token));
            return null;
        }

        // 获取微信小程序码图片的方法，因为SoMall中图片资源是上传到又拍平台的，
        // 但是我们希望使用七牛作为我们的OSS，所以在我们实现七牛OSS管理模块前，我们先注掉下面的代码
        // todo: 以来七牛云存储实现以下方法：Getwxacodeunlimit

        //public virtual async Task<string> Getwxacodeunlimit(string appId, string appSec, string scene, string page = "pages/index/index")
        //{
        //    if (page.IsNullOrEmptyOrWhiteSpace())
        //    {
        //        page = "pages/index/index";
        //    }

        //    var key = $"SoMall:QR:{appId}";
        //    var cache = await _redisClient.Database.HashGetAsync(key, scene);

        //    if (cache.HasValue)
        //    {
        //        return cache.ToString();
        //    }

        //    var token = await GetAccessTokenAsync(appId, appSec);

        //    var img = await _weixinApi.WxacodeGetUnlimit(token, scene, page);

        //    var upyun = await GetUploader();

        //    var result = upyun.writeFile($"/somall/mini_qr/{scene}.jpg", img, true);

        //    if (result)
        //    {
        //        var path = $"{upyun.Domain}/somall/mini_qr/{scene}.jpg";
        //        await _redisClient.Database.HashSetAsync(key, scene,
        //            path);
        //        return path;
        //    }

        //    throw new UserFriendlyException("生成小程序二维码失败");
        //}

        //private async Task<UpYun> GetUploader()
        //{
        //    var bucketName = await _setting.GetOrNullAsync(OssManagementSettings.BucketName);
        //    var domain = await _setting.GetOrNullAsync(OssManagementSettings.DomainHost);
        //    var pwd = await _setting.GetOrNullAsync(OssManagementSettings.AccessKey);
        //    var username = await _setting.GetOrNullAsync(OssManagementSettings.AccessId);
        //    return new UpYun(bucketName, username,
        //        pwd, domain);
        //}
    }
}
