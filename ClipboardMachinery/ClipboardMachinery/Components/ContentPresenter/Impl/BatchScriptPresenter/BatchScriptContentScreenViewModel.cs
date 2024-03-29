﻿using Caliburn.Micro;
using ClipboardMachinery.Components.Buttons.ActionButton;
using ClipboardMachinery.Components.Buttons.ToggleButton;
using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Core;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
                clearOutputButton.IsEnabled = value != null;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Fields

        private readonly ToggleButtonViewModel runScriptButton;
        private readonly ActionButtonViewModel clearOutputButton;
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
            ToggleButtonViewModel toggleButton, ActionButtonViewModel actionButton) : base(creator, owner, releaseFn) {

            // Create run script button
            runScriptButton = toggleButton;
            runScriptButton.ToolTip = "Execute script";
            runScriptButton.Icon = (Geometry)Application.Current.FindResource("IconPlay");
            runScriptButton.ToggledIcon = (Geometry)Application.Current.FindResource("IconStop");
            runScriptButton.HoverColor = (SolidColorBrush)Application.Current.FindResource("PositiveActionBrush");
            runScriptButton.ToggleColor = (SolidColorBrush)Application.Current.FindResource("DangerousActionBrush");
            runScriptButton.ClickAction = RunScript;
            runScriptButton.ConductWith(this);

            // Create clear ouput button
            clearOutputButton = actionButton;
            clearOutputButton.ToolTip = "Clear output";
            clearOutputButton.Icon = (Geometry)Application.Current.FindResource("IconFileRemove");
            clearOutputButton.ClickAction = ClearOutputLog;
            clearOutputButton.ConductWith(this);

            // Add buttons to side controls of the clip
            Clip.SideControls.Add(runScriptButton);
            Clip.SideControls.Add(clearOutputButton);
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

        private Task ClearOutputLog(ActionButtonViewModel button) {
            ProcessOutput = null;
            return Task.CompletedTask;
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

            return Task.Run(async () => {
                PrintProcessOutput("Starting command line process...");
                string workingDirectory = Clip.Model.Tags.FirstOrDefault(t => t.TypeName == SystemTagTypes.WorkspaceTagType.Name)?.Value ?? Directory.GetCurrentDirectory();
                int exitCode = await RunCommandAsync(Clip.Model.Content, workingDirectory, cts.Token).ConfigureAwait(false);
                PrintProcessOutput($"Process exited with code {exitCode}.");
                toggleButton.IsToggled = false;
                cts.Dispose();
                cts = null;
            }, cts.Token);
        }

        private Task<int> RunCommandAsync(string command, string workingDirectory, CancellationToken cancellationToken = default) {
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();

            Process process = new Process {
                StartInfo = {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    WorkingDirectory = workingDirectory,
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

            if (string.IsNullOrWhiteSpace(ProcessOutput)) {
                ProcessOutput += $"[{DateTime.Now:HH:mm:ss}] {line}";
            } else {
                ProcessOutput += $"{Environment.NewLine}[{DateTime.Now:HH:mm:ss}] {line}";
            }
        }

        #endregion

    }

}
