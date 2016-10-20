using System.Globalization;
using System.Reflection;
using Localized._1._1_preview.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Localized._1._1_preview
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
            //services.AddTransient<IConfigureOptions<MvcOptions>, MvcOptionsSetup2>();

            // Add framework services.
            // Embedded provider for views in the class library.
            // Setting ResourcesPath only to confirm [ResourceLocation] wins. No resources in this directory.
            var embeddedProvider = new EmbeddedFileProvider(typeof(Model).GetTypeInfo().Assembly);
            services
                .AddMvc()
                .AddDataAnnotationsLocalization()
                .AddRazorOptions(options => options.FileProviders.Add(embeddedProvider))
                .AddViewLocalization(options => options.ResourcesPath = "Resources");
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
                options.ModelMetadataDetailsProviders.Add(new BindingMetadataProvider(_localizer));
            }
        }

        private class BindingMetadataProvider : IBindingMetadataProvider
        {
            private readonly IStringLocalizer<Model> _localizer;

            public BindingMetadataProvider(IStringLocalizer<Model> localizer)
            {
                _localizer = localizer;
            }

            public void CreateBindingMetadata(BindingMetadataProviderContext context)
            {
                if (context.Key.MetadataKind != ModelMetadataKind.Property ||
                    !context.Key.ModelType.GetTypeInfo().IsValueType)
                {
                    return;
                }

                context.BindingMetadata.ModelBindingMessageProvider.ValueMustNotBeNullAccessor =
                    _ => _localizer["{0} is totally, completely required.", context.Key.Name];
            }
        }

        private class MvcOptionsSetup2 : IConfigureOptions<MvcOptions>
        {
            private readonly IStringLocalizer<Model> _localizer;

            public MvcOptionsSetup2(IStringLocalizer<Model> localizer)
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
