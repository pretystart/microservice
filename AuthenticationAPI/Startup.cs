using Autofac;
using Autofac.Extensions.DependencyInjection;
using eShopOnContainers.Identity;
using Identity.API.Certificate;
using Identity.API.Configuration;
using Identity.API.Data;
using Identity.API.Models;
using Identity.API.Services;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Ocelot.JWTAuthorizePolicy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<AppSettings>(Configuration);
            var audienceConfig = Configuration.GetSection("Audience");
            //注入OcelotJwtBearer
            services.AddJTokenBuild(audienceConfig["Issuer"], audienceConfig["Audience"], audienceConfig["Secret"], "/api/denied");
            services.AddMvc();

            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddTransient<ILoginService<ApplicationUser>, EFLoginService>();
            services.AddTransient<IRedirectService, RedirectService>();

            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            var cred = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(audienceConfig["Secret"])), SecurityAlgorithms.HmacSha256);
            // Adds IdentityServer
            services.AddIdentityServer(x => x.IssuerUri = "null")
                .AddSigningCredential(Certificate.Get())
                //.AddValidationKey(cred)
                .AddAspNetIdentity<ApplicationUser>()
                //.AddConfigurationStore(builder =>builder.UseSqlServer(connectionString, options =>options.MigrationsAssembly(migrationsAssembly)))
                //.AddConfigurationStore(options =>
                //{
                //    options.DefaultSchema = "dbo";
                //    options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString);
                //})
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseSqlServer(connectionString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                //.AddOperationalStore(builder =>builder.UseSqlServer(connectionString, options =>options.MigrationsAssembly(migrationsAssembly)))
                //.AddOperationalStore(options =>
                //{
                //    options.DefaultSchema = "dbo";
                //    options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString);
                //})
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseSqlServer(connectionString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));

                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = true;
                    options.TokenCleanupInterval = 30;
                })
                .Services.AddTransient<IProfileService, ProfileService>();

            var container = new ContainerBuilder();
            container.Populate(services);
            return new AutofacServiceProvider(container.Build());
        }
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();


            // Make work identity server redirections in Edge and lastest versions of browers. WARN: Not valid in a production environment.
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("Content-Security-Policy", "script-src 'unsafe-inline'");
                await next();
            });

            app.UseIdentity();

            // Adds IdentityServer
            app.UseIdentityServer();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            // Store idsrv grant config into db
            InitializeGrantStoreAndConfiguration(app).Wait();

            //Seed Data
            var hasher = new PasswordHasher<ApplicationUser>();
            new Microsoft.eShopOnContainers.Services.Catalog.API.Infrastructure.ApplicationContextSeed(hasher).SeedAsync(app, env, loggerFactory).Wait();
        }

        private async Task InitializeGrantStoreAndConfiguration(IApplicationBuilder app)
        {
            //callbacks urls from config:
            Dictionary<string, string> clientUrls = new Dictionary<string, string>();
            clientUrls.Add("Mvc", Configuration.GetValue<string>("MvcClient"));
            clientUrls.Add("my", Configuration.GetValue<string>("myClient"));
            clientUrls.Add("Spa", Configuration.GetValue<string>("SpaClient"));
            clientUrls.Add("Xamarin", Configuration.GetValue<string>("XamarinCallback"));
            clientUrls.Add("LocationsApi", Configuration.GetValue<string>("LocationApiClient"));
            clientUrls.Add("MarketingApi", Configuration.GetValue<string>("MarketingApiClient"));
            clientUrls.Add("BasketApi", Configuration.GetValue<string>("BasketApiClient"));
            clientUrls.Add("OrderingApi", Configuration.GetValue<string>("OrderingApiClient"));

            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();

                if (!context.Clients.Any())
                {
                    foreach (var client in Config.GetClients(clientUrls))
                    {
                        await context.Clients.AddAsync(client.ToEntity());
                    }
                    await context.SaveChangesAsync();
                }

                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in Config.GetResources())
                    {
                        await context.IdentityResources.AddAsync(resource.ToEntity());
                    }
                    await context.SaveChangesAsync();
                }

                if (!context.ApiResources.Any())
                {
                    foreach (var api in Config.GetApis())
                    {
                        await context.ApiResources.AddAsync(api.ToEntity());
                    }
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
