using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acme.BookStore.WeixinOpen.Application.DTOs;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Acme.BookStore.WeixinOpen.Application.Interfaces
{
    public interface IWeixinAppService:IApplicationService
    {
        //Task<string> GetAccessToken(string appid);

        Task<MpAuthenticateResultModel> MiniAuth(MpAuthenticateModel loginModel);
    
    }
}
