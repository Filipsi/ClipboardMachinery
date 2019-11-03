using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Markdig;
using Markdig.Extensions.AutoLinks;
using Markdig.Extensions.MediaLinks;
using Newtonsoft.Json;

namespace ClipboardMachinery.Windows.UpdateNotes {

    public class UpdateNotesViewModel : Screen {

        #region Properties

        public MarkdownPipeline MarkdownPipeline {
            get;
        }

        public Uri Address {
            get;
        }

        public string Title {
            get => title;
            private set {
                if (title == value) {
                    return;
                }

                title = value;
                NotifyOfPropertyChange();
            }
        }

        public string Content {
            get => content;
            private set {
                if (content == value) {
                    return;
                }

                content = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsLoading {
            get => isLoading;
            private set {
                if (isLoading == value) {
                    return;
                }

                isLoading = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => IsLoaded);
            }
        }

        public bool IsLoaded
            => !IsLoading;

        #endregion

        #region Fields

        private static readonly Uri githubApi = new Uri("https://api.github.com/", UriKind.Absolute);

        private readonly Version tagVersion;

        private string title;
        private string content;
        private bool isLoading;

        #endregion

        public UpdateNotesViewModel(string repositoryOwner, string repositoryName, Version tagVersion) {
            this.tagVersion = tagVersion;
            Address = new Uri(githubApi, $"repos/{repositoryOwner}/{repositoryName}/releases/tags/{tagVersion}");
            Title = $"Release notes for {tagVersion}";

            MarkdownPipeline = new MarkdownPipelineBuilder()
                .UseAutoLinks()
                .Build();
        }

        #region Actions

        public async Task LoadContent() {
            IsLoading = true;
            await Task.Delay(1500);

            try {
                await Task.Run(async () => {
                    string rawResponce = await MakeRequestAsync(Address);
                    dynamic jsonResponce = JsonConvert.DeserializeObject(rawResponce);
                    Content = (string) jsonResponce.body;
                });
            } catch (Exception) {
                Content = $"Unable to load release notes for version {tagVersion}.";
            }

            IsLoading = false;
        }

        public Task Confirm() {
            return IsLoading ? Task.CompletedTask : TryCloseAsync(true);
        }

        #endregion

        #region Logic

        private static async Task<string> MakeRequestAsync(Uri address) {
            // The downloaded resource ends up in the variable named content.
            MemoryStream content = new MemoryStream();

            // Initialize an HttpWebRequest for the current URL.
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(address);
            request.UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.106 Safari/537.36";

            // Send the request to the Internet resource and wait for the response.
            using (WebResponse response = await request.GetResponseAsync()) {
                // Get the data stream that is associated with the specified URL.
                using (Stream responseStream = response.GetResponseStream()) {
                    // Read the bytes in responseStream and copy them to content.
                    responseStream?.CopyTo(content);
                }
            }

            // Return the result as UTF-8 encoded string.
            return Encoding.UTF8.GetString(content.ToArray());
        }

        #endregion

    }

}
