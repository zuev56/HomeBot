using Home.Data.Models.Vk;
using Home.Services.Vk.Models;
using Home.Web.API.Areas.Vk.Models;
using Zs.Common.Extensions;

namespace Home.Web.API
{
    public static class MapperConfiguration
    {
        public static AutoMapper.MapperConfiguration CreateMapperConfiguration()
        {
            return new AutoMapper.MapperConfiguration(config =>
            {
                config.CreateMap<User, ListUserDto>()
                    .ForMember(destination => destination.Name, member => member.MapFrom(i => $"{i.FirstName} {i.LastName}"))
                    .ForMember(destination => destination.IsOnline, member => member.MapFrom(i => false))
                    .ForMember(destination => destination.ActivitySec, member => member.MapFrom(i => -1));

                config.CreateMap<UserWithActivity, ListUserDto>()
                    .ForMember(destination => destination.Id, member => member.MapFrom(i => i.User.Id))
                    .ForMember(destination => destination.Name, member => member.MapFrom(i => $"{i.User.FirstName} {i.User.LastName}"))
                    .ForMember(destination => destination.IsOnline, member => member.MapFrom(i => i.isOnline))
                    .ForMember(destination => destination.ActivitySec, member => member.MapFrom(i => i.ActivitySec));

                config.CreateMap<SimpleUserActivity, PeriodInfoDto>()
                    .ForMember(destination => destination.TimeInSite, member => member.MapFrom(i => i.TimeInSite.ToDayHHmmss()))
                    .ForMember(destination => destination.TimeInApp, member => member.MapFrom(i => i.TimeInApp.ToDayHHmmss()))
                    .ForMember(destination => destination.FullTime, member => member.MapFrom(i => (i.TimeInSite + i.TimeInApp).ToDayHHmmss()));

                config.CreateMap<DetailedUserActivity, FullTimeInfoDto>()
                    .ForMember(destination => destination.TimeInSite, member => member.MapFrom(i => i.TimeInSite.ToDayHHmmss()))
                    .ForMember(destination => destination.TimeInApp, member => member.MapFrom(i => i.TimeInApp.ToDayHHmmss()))
                    .ForMember(destination => destination.FullTime, member => member.MapFrom(i => i.FullTime.ToDayHHmmss()))
                    .ForMember(destination => destination.AvgDailyTime, member => member.MapFrom(i => i.AvgDailyTime.ToDayHHmmss()));


            });
        }
    }
}
