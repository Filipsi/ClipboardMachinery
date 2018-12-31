using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Caliburn.Micro;
using Castle.Windsor;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Components.Navigator;
using ClipboardMachinery.Components.Tag;
using ClipboardMachinery.Core.Repository;
using ClipboardMachinery.Core.Services.Clipboard;
using ClipboardMachinery.Core.Services.HotKeys;
using ClipboardMachinery.Plumbing;
using ClipboardMachinery.Plumbing.Factories;
using static ClipboardMachinery.Common.Events.ClipEvent;

namespace ClipboardMachinery.Windows.Shell {

    public class ShellViewModel : Conductor<IScreen>.Collection.OneActive, IShell, IHandle<ClipEvent> {

        #region Properties

    public bool IsVisible {
            get => isVisible;
            set {
                if (isVisible == value)
                    return;

                isVisible = value;
                NotifyOfPropertyChange();
            }
        }

        public NavigatorViewModel Navigator {
            get;
        }

        public string AppVersion
            => (Debugger.IsAttached ? "dev" : string.Empty) + Assembly.GetEntryAssembly().GetName().Version.ToString(3);

        public double AppWidth
            => SystemParameters.PrimaryScreenWidth / 3;

        public double MaxContentHeight
            => SystemParameters.PrimaryScreenHeight / 1.5;

        #endregion

        #region Fields

        private readonly IEventAggregator eventAggregator;
        private readonly IWindsorContainer windsorContainer;
        private readonly IDataRepository dataRepository;
        private readonly IClipboardService clipboardService;

        private bool isVisible = true;

        #endregion

        public ShellViewModel(
            IEventAggregator eventAggregator, NavigatorViewModel navigator,
            IWindsorContainer windsorContainer, IHotKeyService hotKeyService,
            IClipboardService clipboardService, IDataRepository dataRepository)  {

            this.eventAggregator = eventAggregator;
            this.windsorContainer = windsorContainer;
            this.dataRepository = dataRepository;
            this.clipboardService = clipboardService;

            // HotKeys
            hotKeyService.Register(System.Windows.Input.Key.H, KeyModifier.Ctrl, OnAppVisiblityToggle);

            // Clipboard
            clipboardService.ClipboardChanged += OnClipboardChanged;

            // Navigator
            Navigator = navigator;
            Navigator.ConductWith(this);
            Navigator.ExitButtonClicked += OnNavigatorExitButtonClicked;
            Navigator.PropertyChanged += OnNavigatorPropertyChanged;
        }

        #region Event Handlers

        private void OnAppVisiblityToggle(HotKey key) {
            IsVisible = !IsVisible;
        }

        public Task HandleAsync(ClipEvent message, CancellationToken cancellationToken) {
            if (message.EventType == ClipEventType.Select) {
                clipboardService.IgnoreNextChange(message.Source.Content);
                clipboardService.SetClipboardContent(message.Source.Content);
                IsVisible = false;
            }

            return Task.CompletedTask;
        }

        private void OnNavigatorExitButtonClicked(object sender, EventArgs e) {
            TryClose();
        }

        private void OnNavigatorPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName != nameof(Navigator.Selected)) {
                return;
            }

            NavigatorViewModel navigator = sender as NavigatorViewModel;

            if (navigator.Selected == null) {
                ActivateItem(null);

            } else {
                navigator.Selected.ConductWith(this);
                ActivateItem(navigator.Selected as IScreen);
            }
        }

        private async void OnClipboardChanged(object sender, ClipboardEventArgs e) {
            if (e.Payload == string.Empty) {
                return;
            }

            // Create clip model
            ClipModel model = new ClipModel {
                Created = DateTime.UtcNow,
                Content = e.Payload,
                // TODO: Add config option to disable this
                Tags = new BindableCollection<TagModel>(
                    new TagModel[] {
                        new TagModel {
                            Name = "source",
                            Value = e.Source
                        }
                    }
                )
            };

            // Save clip
            await dataRepository.InsertClip(
                content: model.Content,
                created: model.Created,
                tags: model.Tags.Select(tag => new KeyValuePair<string, object>(tag.Name, tag.Value)).ToArray()
            );

            // Dispatch information about new clip creation
            await eventAggregator.PublishOnCurrentThreadAsync(
                new ClipEvent(
                    model,
                    ClipEventType.Created
                )
            );
        }

        #endregion

    }

}
