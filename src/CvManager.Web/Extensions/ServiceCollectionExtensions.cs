using CvManager.Application.Common;
using CvManager.Domain;
using CvManager.Infrastructure.Data;
using CvManager.Infrastructure.Salesforce;
using CvManager.Infrastructure.Services;
using CvManager.Web.Options;
using CvManager.Web.Security;
using CvManager.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CvManager.Web.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddAppOptions(IConfiguration configuration)
        {
            services.Configure<UploadcareOptions>(configuration.GetSection(UploadcareOptions.SectionName));
            services.Configure<SendGridOptions>(configuration.GetSection(SendGridOptions.SectionName));
            services.Configure<SalesforceOptions>(configuration.GetSection(SalesforceOptions.SectionName));
            return services;
        }

        public IServiceCollection AddDatabase(IConfiguration configuration) =>
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("SupabaseConnection")));

        public IServiceCollection AddBusinessServices()
        {
            services.AddScoped<AccountService>();
            services.AddScoped<AdminService>();
            services.AddScoped<AttributeService>();
            services.AddScoped<ProfileService>();
            services.AddScoped<PositionService>();
            services.AddScoped<PositionExportService>();
            services.AddScoped<CvService>();
            services.AddScoped<DiscussionService>();
            services.AddScoped<SearchService>();
            services.AddScoped<EmailService>();
            services.AddScoped<SalesforceService>();
            return services;
        }

        public IServiceCollection ConfigureIdentity()
        {
            services.AddIdentity<IdentityUser, IdentityRole>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = true;
                    options.SignIn.RequireConfirmedEmail = true;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequiredLength = FieldLengths.PasswordMin;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                    options.Lockout.AllowedForNewUsers = true;
                })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddErrorDescriber<LocalizedIdentityErrorDescriber>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.Events.OnValidatePrincipal = CookiePrincipalValidator.ValidateAsync;
            });

            return services;
        }

        public IServiceCollection AddExternalLogins(IConfiguration configuration)
        {
            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = configuration["Authentication:Google:ClientId"] ?? string.Empty;
                    options.ClientSecret = configuration["Authentication:Google:ClientSecret"] ?? string.Empty;
                    options.Events.OnRemoteFailure = RedirectOnRemoteFailure;
                })
                .AddFacebook(options =>
                {
                    options.AppId = configuration["Authentication:Facebook:AppId"] ?? string.Empty;
                    options.AppSecret = configuration["Authentication:Facebook:AppSecret"] ?? string.Empty;
                    options.Events.OnRemoteFailure = RedirectOnRemoteFailure;
                });

            return services;
        }

        public IServiceCollection ConfigureAuthorization()
        {
            services.AddAuthorizationBuilder()
                .AddPolicy(AuthorizationPolicies.AdminOnly, policy => policy.RequireRole(RoleNames.Administrator))
                .AddPolicy(
                    AuthorizationPolicies.RecruiterOrAdmin,
                    policy => policy.RequireRole(RoleNames.Administrator, RoleNames.Recruiter));
            return services;
        }

        public IServiceCollection ConfigureForwardedHeaders() =>
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();
            });

        public IServiceCollection AddWebUi()
        {
            services.AddControllersWithViews()
                .AddDataAnnotationsLocalization();
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.AddSignalR();
            return services;
        }
    }

    private static Task RedirectOnRemoteFailure(RemoteFailureContext context)
    {
        context.Response.Redirect("/Account/Login");
        context.HandleResponse();
        return Task.CompletedTask;
    }
}
