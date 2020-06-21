﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Acme.BookStore.WeixinOpen.Domain
{
    public class AccessTokeCacheItem
    {
        public string AccessToken { get; set; }

        public string Appid { get; set; }

        public string AppSecret { get; set; }

        public DateTimeOffset TimeCreated { get; set; }

        public DateTimeOffset TimeExpired { get; set; }
    }
}
