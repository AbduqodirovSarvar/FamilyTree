using Application.Common.Interfaces;
using Application.Common.Interfaces.EntityServices;
using Application.Common.Interfaces.EntityServices.Auths;
using Application.Common.Interfaces.EntityServices.Common;
using Application.Common.Mappings;
using Application.Services.EntityServices;
using Application.Services.EntityServices.Auths;
using Application.Services.EntityServices.Common;
using Microsoft.Extensions.DependencyInjection;

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
            services.AddScoped<IAuthService, AuthService>();
            services.AddAutoMapper(cfg =>
            {
                cfg.AddMaps(typeof(AssemblyMarker).Assembly);
            });
        }
    }
}
