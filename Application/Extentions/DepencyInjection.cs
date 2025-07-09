using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Services;
using AutoMapper;
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
        public static void AddApplicationServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(DepencyInjection).Assembly);
            });

            services.AddScoped<ICurrentFamily, CurrentFamily>();

            //services.AddAutoMapper(typeof(FamilyMappingProfile));
        }
    }
}
