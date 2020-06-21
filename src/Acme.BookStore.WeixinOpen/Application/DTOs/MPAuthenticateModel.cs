using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Acme.BookStore.WeixinOpen.Application.DTOs
{
    public class MpAuthenticateModel
    {
        /// <summary>
        /// 应用程序名称
        /// </summary>
        public string appName { get; set; }

        /// <summary>
        /// 用于换取微信的session_key的Code
        /// code由小程序端代码wx.login方法返回
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// 解密Userinfo使用
        /// </summary>
        public string encryptedData { get; set; }

        public string iv { get; set; }
    }
}
