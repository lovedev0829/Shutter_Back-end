using AutoMapper;
using ShutterManagement.Core.SPModel.SP_Responses;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.DAL.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<LoginDetails, UserLoginDetails>();
            CreateMap<User, TblUser>();
        }
    }
}
