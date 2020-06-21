using Acme.BookStore.WeixinOpen.Domain;
using AutoMapper;
using Senparc.Weixin.WxOpen.Entities;

namespace Acme.BookStore.WeixinOpen
{
    public class WeixinApplicationAutoMapperProfile : Profile
    {
        public WeixinApplicationAutoMapperProfile()
        {
            CreateMap<DecodedUserInfo, WechatUserinfo>()
                .ForMember(x => x.appid, opt => opt.MapFrom((x => x.watermark.appid)))
                .ForMember(x => x.openid, opt => opt.MapFrom( x => x.openId))
                .ForMember(x => x.unionid, opt => opt.MapFrom(x => x.unionId))
                .ForMember(x => x.nickname, opt => opt.MapFrom(x => x.nickName))
                .ForMember(x => x.headimgurl, opt => opt.MapFrom(x => x.avatarUrl))
                .ForMember(x => x.country, opt => opt.MapFrom(x => x.country))
                .ForMember(x => x.province, opt => opt.MapFrom(x => x.province))
                .ForMember(x => x.city, opt => opt.MapFrom(x => x.city))
                .ForMember(x => x.sex, opt => opt.MapFrom(x => x.gender))
                .ForMember(x => x.FromClient, opt => opt.Ignore())
                .ForMember(x => x.CreationTime, opt => opt.Ignore())
                .ForMember(x => x.CreatorId, opt => opt.Ignore())
                .ForMember(x => x.AppName, opt => opt.Ignore())
                ;
        }
    }
}
