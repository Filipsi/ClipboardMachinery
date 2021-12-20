using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Onova.Exceptions;
using Onova.Services;

namespace ClipboardMachinery.Plumbing.Customization {

    public class AppVeyorPackageResolver : IPackageResolver {

        #region Fields

        private const string API_BASE_ADDRESS = "https://ci.appveyor.com";

        private readonly string accountName;
        private readonly string projectSlug;
        private readonly string artifactPath;
        private readonly string branchName;

        #endregion

        public AppVeyorPackageResolver(string accountName, string projectSlug, string artifactPath, string branchName) {
            this.accountName = accountName;
            this.projectSlug = projectSlug;
            this.artifactPath = artifactPath;
            this.branchName = branchName;
        }

        #region New region

        private IReadOnlyDictionary<Version, string> ParsePackageVersionUrlMap(JsonElement projectInfo) {
            Dictionary<Version, string> map = new Dictionary<Version, string>();

            foreach (JsonElement buildJson in projectInfo.GetProperty("builds").EnumerateArray()) {
                // Get release name
                string buildVersion = buildJson.GetProperty("version").GetString();

                if (string.IsNullOrWhiteSpace(buildVersion)) {
                    continue;
                }

                // Try to parse version
                string versionText = Regex.Match(buildVersion, "(\\d+\\.\\d+(?:\\.\\d+)?(?:\\.\\d+)?)").Groups[1].Value;
                if (!Version.TryParse(versionText, out Version version)) {
                    continue;
                }

                // Find asset
                JsonElement jobsJson = buildJson.GetProperty("jobs");
                foreach (JsonElement assetJson in jobsJson.EnumerateArray()) {
                    int artifactsCount = assetJson.GetProperty("artifactsCount").GetInt32();
                    if (artifactsCount == 0) {
                        continue;
                    }

                    string jobId = assetJson.GetProperty("jobId").GetString();
                    if (string.IsNullOrWhiteSpace(jobId)) {
                        continue;
                    }

                    // Add to dictionary
                    map[version] = $"{API_BASE_ADDRESS}/api/buildjobs/{jobId}/artifacts/{string.Format(artifactPath, buildVersion)}";
                }
            }

            return map;
        }

        private async Task<IReadOnlyDictionary<Version, string>> GetPackageVersionUrlMapAsync(CancellationToken cancellationToken) {
            string url = $"{API_BASE_ADDRESS}/api/projects/{accountName}/{projectSlug}/history?recordsNumber=5&includeJobs=true";

            if (!string.IsNullOrWhiteSpace(branchName)) {
                url += $"&branch={branchName}";
            }

            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url))
            using (HttpClient client = new HttpClient()) {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using (HttpResponseMessage response = await client.GetAsync(url, cancellationToken)) {
                    response.EnsureSuccessStatusCode();

                    JsonElement jsonResponse = await ReadAsJsonAsync(response.Content, cancellationToken);
                    return ParsePackageVersionUrlMap(jsonResponse);
                }
            }
        }

        public async Task<IReadOnlyList<Version>> GetPackageVersionsAsync(CancellationToken cancellationToken = default) {
            IReadOnlyDictionary<Version, string> versions = await GetPackageVersionUrlMapAsync(cancellationToken);
            return versions.Keys.ToArray();
        }

        public async Task DownloadPackageAsync(Version version, string destFilePath, IProgress<double> progress = null, CancellationToken cancellationToken = default) {
            // Get map
            IReadOnlyDictionary<Version, string> map = await GetPackageVersionUrlMapAsync(cancellationToken);

            // Try to get package URL
            if (!map.TryGetValue(version, out string packageUrl)) {
                return;
            }

            if (string.IsNullOrWhiteSpace(packageUrl)) {
                throw new PackageNotFoundException(version);
            }

            // Download
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, packageUrl)) {
                HttpClientHandler handler = new HttpClientHandler();

                if (handler.SupportsAutomaticDecompression) {
                    handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                }

                handler.UseCookies = false;

                using (HttpClient httpClient = new HttpClient(handler, true)) {
                    request.Headers.Add("Accept", "application/octet-stream"); // required

                    using (HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)) {
                        response.EnsureSuccessStatusCode();

                        using (FileStream output = File.Create(destFilePath)) {
                            await CopyToStreamAsync(response.Content, output, progress, cancellationToken);
                        }
                    }
                }
            }
        }

        #endregion

        #region Helpers

        private async Task CopyToStreamAsync(HttpContent content, Stream destination, IProgress<double> progress = null, CancellationToken cancellationToken = default) {
            long? length = content.Headers.ContentLength;
            using Stream source = await content.ReadAsStreamAsync();

            using (PooledBuffer<byte> buffer = new PooledBuffer<byte>(81920)) {
                long totalBytesCopied = 0L;
                int bytesCopied;

                do {
                    bytesCopied = await CopyBufferedToAsync(source, destination, buffer.Array, cancellationToken);
                    totalBytesCopied += bytesCopied;

                    if (length != null)
                        progress?.Report(1.0 * totalBytesCopied / length.Value);
                } while (bytesCopied > 0);
            }
        }

        private async Task<int> CopyBufferedToAsync(Stream source, Stream destination, byte[] buffer, CancellationToken cancellationToken = default) {
            int bytesCopied = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
            await destination.WriteAsync(buffer, 0, bytesCopied, cancellationToken);

            return bytesCopied;
        }

        public static async Task<JsonElement> ReadAsJsonAsync(HttpContent content, CancellationToken cancellationToken = default) {
            using (Stream stream = await content.ReadAsStreamAsync())
            using (JsonDocument document = await JsonDocument.ParseAsync(stream, default, cancellationToken)) {
                return document.RootElement.Clone();
            }
        }

        #endregion

        #region Http

        private readonly struct PooledBuffer<T> : IDisposable {

            public T[] Array { get; }

            public PooledBuffer(int minimumLength) =>
                Array = ArrayPool<T>.Shared.Rent(minimumLength);

            public void Dispose() {
                ArrayPool<T>.Shared.Return(Array);
            }

        }

        #endregion

    }

}