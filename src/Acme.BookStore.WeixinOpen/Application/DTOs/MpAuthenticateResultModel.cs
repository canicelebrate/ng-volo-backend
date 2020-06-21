using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Senparc.Weixin.WxOpen.Entities;

namespace Acme.BookStore.WeixinOpen.Application.DTOs
{
    public class MpAuthenticateResultModel
    {
        /// <summary>
        /// Access Token
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// 用户详细信息
        /// </summary>
        public DecodedUserInfo ExternalUser { get; set; }

        public string SessionKey { get; set; }
    }
}
