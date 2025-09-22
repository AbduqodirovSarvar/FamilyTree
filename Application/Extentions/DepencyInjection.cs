using Application.Common.Interfaces;
using Application.Common.Interfaces.EntityServices;
using Application.Common.Interfaces.EntityServices.Auths;
using Application.Common.Interfaces.EntityServices.Common;
using Application.Common.Interfaces.Repositories.Common;
using Application.Common.Mappings;
using Application.Common.Models.ViewModels;
using Application.Services;
using Application.Services.EntityServices;
using Application.Services.EntityServices.Auths;
using Application.Services.EntityServices.Common;
using AutoMapper;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Application.Extentions
{
    public static class DepencyInjection
    {
        public static void AddApplicationDepencies(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(DepencyInjection).Assembly);
            });

            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped(typeof(IGenericEntityService<,,,>), typeof(GenericEntityService<,,,>));
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IFamilyService, FamilyService>();
            services.AddScoped<IMemberService, MemberService>();
            services.AddScoped<IUploadedFileService, UploadedFileService>();
            services.AddScoped<IUserRolePermissionService, UserRolePermissionService>();
            services.AddScoped<IUserRoleService, UserRoleService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddSingleton<IFileService, FileService>();

            services.AddAutoMapper(x =>
            {
                x.CreateMap<User, UserViewModel>().ReverseMap();
                x.CreateMap<Family, FamilyViewModel>().ReverseMap();
            });
        }
    }
}
