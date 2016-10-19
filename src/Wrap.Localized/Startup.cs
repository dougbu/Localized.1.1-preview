using System.Globalization;
using System.Reflection;
using Localized._1._1_preview.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Wrap.Localized
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Setup a bit more localization.
            services.AddTransient<IConfigureOptions<MvcOptions>, MvcOptionsSetup>();

            // Add framework services.
            // Setting ResourcesPath only to confirm [ResourceLocation] wins. No resources in this directory.
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            // Embedded provider for views in the class library.
            var embeddedProvider = new EmbeddedFileProvider(typeof(Model).GetTypeInfo().Assembly);
            services
                .AddMvc()
                .AddDataAnnotationsLocalization()
                .AddRazorOptions(options => options.FileProviders.Add(embeddedProvider))
                .AddViewLocalization();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            var options = new RequestLocalizationOptions();
            foreach (var culture in new[]
            {
                new CultureInfo("fr-CA"),
                new CultureInfo("zh-CN"), // Chinese (simplified)
                new CultureInfo("zh-TW"), // Chinese (traditional)
            })
            {
                options.SupportedCultures.Add(culture);
                options.SupportedUICultures.Add(culture);
            }

            app.UseRequestLocalization(options);
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }

        private class MvcOptionsSetup : IConfigureOptions<MvcOptions>
        {
            private readonly IStringLocalizer<Model> _localizer;

            public MvcOptionsSetup(IStringLocalizer<Model> localizer)
            {
                _localizer = localizer;
            }

            public void Configure(MvcOptions options)
            {
                options.ModelBindingMessageProvider.ValueMustNotBeNullAccessor =
                    value => _localizer["Value '{0}' appears to be null and that's not valid.", value];
            }
        }
    }
}
