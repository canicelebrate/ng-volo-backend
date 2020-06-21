using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Acme.BookStore.WeixinOpen.Application
{
    public static class WeixinPermisssions
    {
        public const string GroupName = "Weixin";

        public static class ChangeSettings
        {
            public const string Default = GroupName + ".ChangeSettings";
            public const string Update = Default + ".Update";
        }
    }
}
