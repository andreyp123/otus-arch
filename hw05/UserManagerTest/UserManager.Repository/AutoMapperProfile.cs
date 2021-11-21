using AutoMapper;
using UserManager.Common;
using UserManager.Repository.Model;

namespace UserManager.Repository
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<UserEntity, User>();
            CreateMap<User, UserEntity>();
        }
    }
}
