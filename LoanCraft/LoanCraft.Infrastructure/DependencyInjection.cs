using LoanCraft.Application.Common.Interfaces;
using LoanCraft.Infrastructure.Authentication;
using LoanCraft.Infrastructure.Persistence;
using LoanCraft.Infrastructure.RealTime;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LoanCraft.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("LoanCraftDb"));

            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            services.AddSignalR();
            services.AddSingleton<IWorkflowHubNotifier, WorkflowHubNotifier>();
            services.AddHttpClient("ExternalApi");

            return services;
        }
    }
}