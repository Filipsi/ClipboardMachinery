using Caliburn.Micro;
using ClipboardMachinery.Common.Events;
using ClipboardMachinery.Components.ActionButton;
using ClipboardMachinery.Components.Tag;
using ClipboardMachinery.Core.Repository;
using ClipboardMachinery.Popup.Manager.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using static ClipboardMachinery.Common.Events.TagEvent;

namespace ClipboardMachinery.Popup.TagEditor {

    public class TagEditorViewModel : Screen, IExtensionControlsProvider {

        #region IExtensionControlsProvider

        public BindableCollection<ActionButtonViewModel> ExtensionControls { get; }

        #endregion

        #region Fields

        private TagModel tagModel;
        private readonly IEventAggregator eventAggregator;
        private readonly IDataRepository dataRepository;

        #endregion

        public TagEditorViewModel(
            TagModel tagModel, Func<ActionButtonViewModel> actionButtonFactory,
            IEventAggregator eventAggregator, IDataRepository dataRepository) {

            this.tagModel = tagModel;
            this.eventAggregator = eventAggregator;
            this.dataRepository = dataRepository;
            ExtensionControls = new BindableCollection<ActionButtonViewModel>();

            // Create extension control buttons
            ActionButtonViewModel removeButton = actionButtonFactory.Invoke();
            removeButton.ToolTip = "Remove";
            removeButton.Icon = (Geometry)Application.Current.FindResource("IconRemove");
            removeButton.HoverColor = (SolidColorBrush)Application.Current.FindResource("DangerousActionBrush");
            removeButton.ClickAction = HandleRemoveClick;
            ExtensionControls.Add(removeButton);
        }

        #region Handlers

        private void HandleRemoveClick(ActionButtonViewModel button) {
            dataRepository.DeleteTag(tagModel.Id);
            eventAggregator.PublishOnCurrentThreadAsync(new TagEvent(tagModel, TagEventType.Remove));
            eventAggregator.PublishOnCurrentThreadAsync(PopupEvent.Close());
        }

        #endregion

    }

}
