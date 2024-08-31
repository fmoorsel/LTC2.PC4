using LTC2.Services.Calculator.ServiceTasks;
using LTC2.Shared.BaseMessages.Interfaces;
using LTC2.Shared.BaseMessages.Services;
using LTC2.Shared.Messaging.Implementations.FileBasedBroker.Extensions;
using LTC2.Shared.Models.Settings;
using LTC2.Shared.Repositories.Interfaces;
using LTC2.Shared.Repositories.Repositories;
using LTC2.Shared.Secrets.Interfaces;
using LTC2.Shared.Secrets.Vaults;
using LTC2.Shared.SpatiaLiteRepository.Repositories;
using LTC2.Shared.StravaConnector.Bootstrap.Extensions;
using LTC2.Shared.Utils.Bootstrap.Extensions;
using LTC2.Shared.Utils.Bootstrap.Interfaces;
using LTC2.Webapps.MainApp.Models;
using LTC2.Webapps.MainApp.Services;
using LTC2.Webapps.MainApp.ServiceTasks;
using LTC2.Webapps.MainApp.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace LTC2.Webapps.MainApp
{
    public class Startup
    {
        private string _allowSpecificOrigins = "_allowSpecificOrigins";

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var appSettings = GetAppSettings();

            var settingsService = new MainAppSettingsService();

            services.AddHostedService<Worker>();

            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(GetSeriLogger(), true));

            services.AddSettings(settingsService);

            services.AddStravaConnector();
            services.AddFileBasedBroker();

            services.AddControllers().AddNewtonsoftJson();

            services.AddControllersWithViews();

            services.AddSingleton<TokenUtils>();
            services.AddSingleton<ProfileManager>();
            services.AddSingleton(appSettings);

            //services.AddSingleton<IScoresRepository, ScoreRepository>();
            services.AddSingleton<IScoresRepository, SqliteScoreRepository>();
            services.AddSingleton<IIntermediateResultsRepository, IntermediateResultsRepository>();
            services.AddSingleton<IInternalProfileRepository, InternalProfileRepository>();
            services.AddSingleton<IBaseTranslationService, BaseTranslationService>();

            services.AddSingleton<TilesRepository>();

            services.AddSingleton<ISecretsVault, WindowsSecretsVault>();
            services.AddSingleton<IDesktopProfileRepository, DesktopProfileRepository>();

            services.AddSingleton<IServiceTask, InitScoreRepositoryTask>();
            services.AddSingleton<IServiceTask, InitStatusPublisherTask>();
            services.AddSingleton<IServiceTask, InitIntermediateResultRepository>();
            services.AddSingleton<IServiceTask, InitStravaPropertiesTask>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                var authorizationSettings = settingsService.GetSettings<AuthorizationSettings>();

                var key = GetSigningKey(authorizationSettings);

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = authorizationSettings.ValidIssuers,
                    ValidAudience = authorizationSettings.ValidAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authorizationSettings.Key))
                };
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "LTC2.API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme.
                            Enter 'Bearer' [space] and then your token in the text input below.
                            Example: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });
            });

            var allowedOrigins = appSettings.AllowedOrigins.Split(',').ToArray();

            services.AddCors(options =>
                                options.AddPolicy(_allowSpecificOrigins,
                                                    policy =>
                                                        policy.WithOrigins(allowedOrigins)
                                                            .AllowCredentials()
                                                            .AllowAnyMethod()
                                                            .AllowAnyHeader()
                                                )
                                );
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCookiePolicy();

            app.UseExceptionHandler("/Home/Error");

            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".pbf"] = "application/x-protobuf";

            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = provider
            });

            app.UseRouting();

            app.UseCors(_allowSpecificOrigins);

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "LTC2 WebApi");
            });

            app.UseEndpoints(endpoints => endpoints.MapDefaultControllerRoute());
        }

        public static AppSettings GetAppSettings()
        {
            var processModule = Process.GetCurrentProcess().MainModule;
            var appSettingsFolder = Path.GetDirectoryName(processModule?.FileName);

            var configuration = new ConfigurationBuilder().SetBasePath(appSettingsFolder)
                        .AddJsonFile("appsettings.json", true, true)
                        .Build();

            return configuration.GetSection("AppSettings").Get<AppSettings>();
        }

        private static Serilog.ILogger GetSeriLogger()
        {
            var configuration = new ConfigurationBuilder()
                                        .SetBasePath(Directory.GetCurrentDirectory())
                                        .AddJsonFile("serlilogsettings.json")
                                        .Build();

            Log.Logger = new LoggerConfiguration()
                                .ReadFrom.Configuration(configuration)
                                .CreateLogger();

            return Log.Logger;
        }

        private string GetSigningKey(AuthorizationSettings authorizationSettings)
        {
            if (authorizationSettings.Key == null || authorizationSettings.Key == string.Empty)
            {
                var key = $"{Guid.NewGuid()}-{Guid.NewGuid()}";

                authorizationSettings.Key = key;
            }

            return authorizationSettings.Key;
        }

    }
}
