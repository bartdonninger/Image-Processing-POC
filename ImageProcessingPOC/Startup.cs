using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web.Caching;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.DependencyInjection;
using SixLabors.ImageSharp.Web.Memory;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Processors;
using SixLabors.ImageSharp.Web.Resolvers;

namespace ImageProcessingPOC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.TryAddSingleton(new AzureBlobContainerSettings
            {
                AccountName = "",
                AccessKey = "",
                ContainerName = ""
            });



            //services.AddImageSharpCore()
            //    .SetRequestParser<QueryCollectionRequestParser>()
            //    .SetBufferManager<PooledBufferManager>()
            //    .SetCache(provider => new PhysicalFileSystemCache(
            //        provider.GetRequiredService<IHostingEnvironment>(),
            //        provider.GetRequiredService<IBufferManager>(),
            //        provider.GetRequiredService<IOptions<ImageSharpMiddlewareOptions>>())
            //    {
            //        Settings =
            //        {
            //            [PhysicalFileSystemCache.Folder] = PhysicalFileSystemCache.DefaultCacheFolder,
            //            [PhysicalFileSystemCache.CheckSourceChanged] = "true"
            //        }
            //    })
            //    .SetCacheHash<CacheHash>()
            //    .SetAsyncKeyLock<AsyncKeyLock>()
            //    .AddResolver<PhysicalFileSystemResolver>()
            //    .AddProcessor<ResizeWebProcessor>()
            //    .AddProcessor<FormatWebProcessor>()
            //    .AddProcessor<BackgroundColorWebProcessor>();

            //// Add the default service and options.
            services.AddImageSharpCore()
                .SetRequestParser<QueryCollectionRequestParser>()
                .SetBufferManager<PooledBufferManager>()
                .SetCache<PhysicalFileSystemCache>()
                .SetCacheHash<CacheHash>()
                .SetAsyncKeyLock<AsyncKeyLock>()
                //.AddResolver<PhysicalFileSystemResolver>()
                .AddResolver<AzureBlobImageResolver>()
                .AddProcessor<ResizeWebProcessor>();

            //// Or add the default service and custom options.
            //services.AddImageSharp(
            //    options =>
            //        {
            //            options.Configuration = Configuration.Default;
            //            options.MaxBrowserCacheDays = 7;
            //            options.MaxCacheDays = 365;
            //            options.CachedNameLength = 8;
            //            options.OnValidate = _ => { };
            //            options.OnBeforeSave = _ => { };
            //            options.OnProcessed = _ => { };
            //            options.OnPrepareResponse = _ => { };
            //        });

            //// Or we can fine-grain control adding the default options and configure all other services.
            //services.AddImageSharpCore()
            //        .SetRequestParser<QueryCollectionRequestParser>()
            //        .SetBufferManager<PooledBufferManager>()
            //        .SetCache<PhysicalFileSystemCache>()
            //        .SetCacheHash<CacheHash>()
            //        .SetAsyncKeyLock<AsyncKeyLock>()
            //        .AddResolver<PhysicalFileSystemResolver>()
            //        .AddProcessor<ResizeWebProcessor>()
            //        .AddProcessor<FormatWebProcessor>()
            //        .AddProcessor<BackgroundColorWebProcessor>();


            //// Or we can fine-grain control adding custom options and configure all other services
            //// There are also factory methods for each builder that will allow building from configuration files.
            //services.AddImageSharpCore(
            //    options =>
            //        {
            //            options.Configuration = Configuration.Default;
            //            options.MaxBrowserCacheDays = 7;
            //            options.MaxCacheDays = 365;
            //            options.CachedNameLength = 8;
            //            options.OnValidate = _ => { };
            //            options.OnBeforeSave = _ => { };
            //            options.OnProcessed = _ => { };
            //            options.OnPrepareResponse = _ => { };
            //        })
            //    .SetRequestParser<QueryCollectionRequestParser>()
            //    .SetBufferManager<PooledBufferManager>()
            //    .SetCache(provider =>
            //      {
            //          var p = new PhysicalFileSystemCache(
            //              provider.GetRequiredService<IHostingEnvironment>(),
            //              provider.GetRequiredService<IBufferManager>(),
            //              provider.GetRequiredService<IOptions<ImageSharpMiddlewareOptions>>());

            //          p.Settings[PhysicalFileSystemCache.Folder] = PhysicalFileSystemCache.DefaultCacheFolder;
            //          p.Settings[PhysicalFileSystemCache.CheckSourceChanged] = "true";

            //          return p;
            //      })
            //    .SetCacheHash<CacheHash>()
            //    .SetAsyncKeyLock<AsyncKeyLock>()
            //    .AddResolver<PhysicalFileSystemResolver>()
            //    .AddProcessor<ResizeWebProcessor>()
            //    .AddProcessor<FormatWebProcessor>()
            //    .AddProcessor<BackgroundColorWebProcessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            //app.UseImageSharp();

            app.Map("/media", HandleMapTest1);

            app.UseMvc();
            //app.UseDefaultFiles();

            //app.UseStaticFiles();
        }

        private static void HandleMapTest1(IApplicationBuilder app)
        {
            app.UseImageSharp();
        }
    }
}
