using System;
using Autofac;
using FutureState.Services.Web.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Autofac.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using FutureState.Reflection;
using FutureState.Autofac;
using FutureState.Data.Providers;
using FutureState.Services.Web.Model;

namespace FutureState.Services.Web
{
    public class Startup : IStartup
    {
        public const string CorsPolicyName = "corsPolicy";

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // add mvc and default error handler
            services.AddMvc(m => m.Filters.Add(new FsActionFilterAttribute()));

            services.AddMvcCore().AddApiExplorer();
            
            //allow cors
            services.AddCors(x => x.AddPolicy(CorsPolicyName, builder =>
            {
                // todo: restrict to origins
                builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().AllowCredentials();
            }));

            var cb = new ContainerBuilder();

            // register services, units of work, specialized queries etc
            cb.RegisterModule<WebServicesModule>();

            services.AddSwaggerGen(c =>
            {
                c.IgnoreObsoleteActions();
                c.IgnoreObsoleteProperties();
                c.DescribeAllEnumsAsStrings();
                c.CustomSchemaIds((type) =>
                {
                    return type.FullName;
                });

                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "Future State Api",
                    Description = "FutureState Api",
                    TermsOfService = "",
                    Contact = new Contact { Name = "Aris Nikolaou" }
                });

                string appBasePath = Environment.CurrentDirectory;

                var filePath = Path.Combine(appBasePath, "FutureState.Services.Web.xml");
                if (File.Exists(filePath))
                    c.IncludeXmlComments(filePath);
            });


            // Populate the services.
            cb.Populate(services);

            var typeScanner = new AppTypeScanner();
           
            var appBuilder = new ApplicationContainerBuilder(cb, typeScanner);
            appBuilder.RegisterAll();

            // Build the container last - order is important
            this.ApplicationContainer = cb.Build();

            // temp add sample maybe enity

            var provider = this.ApplicationContainer.Resolve<ProviderLinq<MaybeEntity, Guid>>();
            provider.Add(new MaybeEntity()
            {
                Id = Guid.NewGuid(),
                FirstName = "Name",
                LastName = "Lastname",
                DateOfBirth = DateTime.UtcNow
            });

            // Create and return the service provider.
            return ApplicationContainer.Resolve<IServiceProvider>();
        }

        public IContainer ApplicationContainer { get; set; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc(routes =>
            {
                // SwaggerGen won't find controllers that are routed via this technique.
                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseCors(CorsPolicyName);

            app.UseStaticFiles();
            app.UseDeveloperExceptionPage();

            app.UseSwagger(setup =>
            {

            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(
                    "/swagger/v1/swagger.json",
                    "FutureState");
            });
        }
    }
}
