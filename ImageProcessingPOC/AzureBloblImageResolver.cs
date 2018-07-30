using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Web.Helpers;
using SixLabors.ImageSharp.Web.Memory;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Resolvers;
using IBufferManager = SixLabors.ImageSharp.Web.Memory.IBufferManager;

namespace ImageProcessingPOC
{
    /// <summary>
    ///     Returns images stored in an azure blob container
    /// </summary>
    public class AzureBlobImageResolver : IImageResolver
    {
        private readonly IBufferManager _bufferManager;
        private readonly AzureBlobContainerSettings _containerSettings;
        private readonly ImageSharpMiddlewareOptions _options;

        public AzureBlobImageResolver(AzureBlobContainerSettings containerSettings,
            IBufferManager bufferManager,
            IOptions<ImageSharpMiddlewareOptions> options)
        {
            _containerSettings = containerSettings;
            _bufferManager = bufferManager;
            _options = options.Value;
        }

        public async Task<IByteBuffer> ResolveImageAsync(HttpContext context, ILogger logger)
        {
            //logger.LogInformation($"[AzureBlobImageResolver] Reading {filename} from Azure");

            var filename = context.Request.Path.Value.TrimStart('/');

            var imageBlob = GetBlobContainer().GetBlockBlobReference(filename);

            if (await imageBlob.ExistsAsync() == false)
                return null;

            IByteBuffer buffer;

            using (var stream = await imageBlob.OpenReadAsync())
            {
                buffer = _bufferManager.Allocate((int)stream.Length);
                await stream.ReadAsync(buffer.Array, 0, (int)stream.Length);
            }

            return buffer;
        }

        public Task<bool> IsValidRequestAsync(HttpContext context, ILogger logger)
        {
            //return Task.FromResult(FormatHelpers.GetExtension(_options.Configuration, context.Request.Path) != null);
            return Task.FromResult(CheckFileName(context.Request.Path));
        }

        public Func<HttpContext, bool> Match { get; set; } = _ => true;

        public IDictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();

        protected CloudBlobContainer GetBlobContainer()
        {
            var storageCredentials = new StorageCredentials(
                _containerSettings.AccountName,
                _containerSettings.AccessKey);
            var cloudStorageAccount = new CloudStorageAccount(storageCredentials, true);
            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            return cloudBlobClient.GetContainerReference(_containerSettings.ContainerName);
        }

        /// <summary>Gets the file extension for the given image uri</summary>
        /// <param name="configuration">The library configuration</param>
        /// <param name="uri">The full request uri</param>
        /// <returns>The <see cref="T:System.String" /></returns>
        public static string GetExtension(Configuration configuration, string uri)
        {
            string[] strArray = uri.Split('?');
            StringValues stringValues;
            if (strArray.Length > 1 && QueryHelpers.ParseQuery(strArray[1]).TryGetValue("format", out stringValues))
                return (string)stringValues;
            string str1 = strArray[0];
            string str2 = (string)null;
            int num1 = 0;
            foreach (IImageFormat imageFormat in configuration.ImageFormats)
            {
                foreach (string fileExtension in imageFormat.FileExtensions)
                {
                    int num2 = str1.LastIndexOf(string.Format(".{0}", (object)fileExtension), StringComparison.OrdinalIgnoreCase);
                    if (num2 >= num1)
                    {
                        num1 = num2;
                        str2 = fileExtension;
                    }
                }
            }
            return str2;
        }

        public static bool CheckFileName(string uri)
        {
            string[] strArray = uri.Split('?');
            if (Guid.Parse(strArray[0].TrimStart('/')) != Guid.Empty)
                return true;

            return false;
        }


    }

    /// <summary>
    ///     Configuration settings for the AzureBlobImageResolver
    /// </summary>
    public class AzureBlobContainerSettings
    {
        public string AccountName { get; set; }
        public string AccessKey { get; set; }
        public string ContainerName { get; set; }
    }
}