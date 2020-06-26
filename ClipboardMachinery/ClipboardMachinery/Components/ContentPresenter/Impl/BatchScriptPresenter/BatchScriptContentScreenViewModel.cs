using Caliburn.Micro;
using ClipboardMachinery.Components.Buttons.ActionButton;
using ClipboardMachinery.Components.Buttons.ToggleButton;
using ClipboardMachinery.Components.Clip;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml;

namespace ClipboardMachinery.Components.ContentPresenter.Impl.BatchScriptPresenter {

    public class BatchScriptContentScreenViewModel : ContentScreen {

        #region Properties

        public string ProcessOutput {
            get => processOutput;
            private set {
                if (processOutput == value) {
                    return;
                }

                processOutput = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Fields

        private readonly ToggleButtonViewModel runScriptButton;
        private CancellationTokenSource cts;
        private string processOutput;

        #endregion

        static BatchScriptContentScreenViewModel() {
            using (Stream stream = Assembly.GetEntryAssembly().GetManifestResourceStream("ClipboardMachinery.Resources.Syntax.Batch.xshd")) {
                if (stream == null) {
                    throw new InvalidOperationException("Could not find embedded resource with Batch syntax highlighting!");
                }

                using (XmlReader reader = new XmlTextReader(stream)) {
                    IHighlightingDefinition batchHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                    HighlightingManager.Instance.RegisterHighlighting("Batch", new string[] { ".cmd", ".bat", ".nt", ".btm" }, batchHighlighting);
                }
            }
        }

        public BatchScriptContentScreenViewModel(IContentPresenter creator, ClipViewModel owner, Action<ContentScreen> releaseFn,
            ToggleButtonViewModel toggleButton) : base(creator, owner, releaseFn) {

            // Create run script button
            runScriptButton = toggleButton;
            runScriptButton.ToolTip = "Execute script";
            runScriptButton.Icon = (Geometry)Application.Current.FindResource("IconPlay");
            runScriptButton.ToggledIcon = (Geometry)Application.Current.FindResource("IconStop");
            runScriptButton.HoverColor = (SolidColorBrush)Application.Current.FindResource("PositiveActionBrush");
            runScriptButton.ToggleColor = (SolidColorBrush)Application.Current.FindResource("DangerousActionBrush");
            runScriptButton.ClickAction = RunScript;
            runScriptButton.ConductWith(this);

            // Add buttons to side controls of the clip
            Clip.SideControls.Add(runScriptButton);
        }

        #region Handlers

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken) {
            if (close) {
                cts?.Cancel();
                cts?.Dispose();
            }

            return base.OnDeactivateAsync(close, cancellationToken);
        }

        #endregion

        #region Logic

        public override string GetClipboardString() {
            return Clip.Model.Content;
        }

        private Task RunScript(ActionButtonViewModel button) {
            if (!(button is ToggleButtonViewModel toggleButton)) {
                return Task.CompletedTask;
            }

            if (!toggleButton.IsToggled) {
                cts?.Cancel();
                toggleButton.IsToggled = false;
               
                return Task.CompletedTask;
            }

            ProcessOutput = null;
            cts?.Dispose();
            cts = new CancellationTokenSource();
            
            Task.Run(async () => {
                PrintProcessOutput("Starting command line process...");
                int exitCode = await RunCommandAsync(Clip.Model.Content, cts.Token);
                PrintProcessOutput($"Process exited with code {exitCode}.");
                toggleButton.IsToggled = false;
                cts.Dispose();
                cts = null;
            }, cts.Token);

            return Task.CompletedTask;
        }

        private Task<int> RunCommandAsync(string command, CancellationToken cancellationToken = default) {
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();

            Process process = new Process {
                StartInfo = {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
                EnableRaisingEvents = true
            };

            cancellationToken.Register(() => {
                try {
                    if (!process.HasExited) {
                        process.Kill();
                        process.Dispose();
                    }
                } catch { }

                tcs.TrySetCanceled();
            });

            process.OutputDataReceived += (sender, e) => PrintProcessOutput(e.Data);
            process.ErrorDataReceived += (sender, e) => PrintProcessOutput(e.Data);
            

            process.Exited += (sender, args) => {
                tcs.TrySetResult(process.ExitCode);
                process.Dispose();
            };

            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            return tcs.Task;
        }

        private void PrintProcessOutput(string line) {
            if (string.IsNullOrWhiteSpace(line)) {
                return;
            }

            ProcessOutput += $"{Environment.NewLine}[{DateTime.Now:HH:mm:ss}] {line}";
        }

        #endregion

    }

}
