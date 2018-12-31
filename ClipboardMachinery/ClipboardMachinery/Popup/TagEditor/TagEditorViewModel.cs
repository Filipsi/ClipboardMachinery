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
using System.Windows.Media.Animation;
using static ClipboardMachinery.Common.Events.TagEvent;

namespace ClipboardMachinery.Popup.TagEditor {

    public class TagEditorViewModel : Screen, IExtensionControlsProvider {

        #region IExtensionControlsProvider

        public BindableCollection<ActionButtonViewModel> ExtensionControls { get; }

        #endregion

        #region Properties

        public TagModel Model {
            get;
        }

        public object Value {
            get => val;
            set {
                if (val == value) {
                    return;
                }

                val = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Fields

        private readonly IEventAggregator eventAggregator;
        private readonly IDataRepository dataRepository;

        private object val;

        #endregion

        public TagEditorViewModel(
            TagModel tagModel, Func<ActionButtonViewModel> actionButtonFactory,
            IEventAggregator eventAggregator, IDataRepository dataRepository) {

            Model = tagModel;
            Value = Model.Value;
            ExtensionControls = new BindableCollection<ActionButtonViewModel>();
            this.eventAggregator = eventAggregator;
            this.dataRepository = dataRepository;

            // Create extension control buttons
            ActionButtonViewModel removeButton = actionButtonFactory.Invoke();
            removeButton.ToolTip = "Remove";
            removeButton.Icon = (Geometry)Application.Current.FindResource("IconRemove");
            removeButton.HoverColor = (SolidColorBrush)Application.Current.FindResource("DangerousActionBrush");
            removeButton.ClickAction = HandleRemoveClick;
            ExtensionControls.Add(removeButton);

            ActionButtonViewModel saveButton = actionButtonFactory.Invoke();
            saveButton.ToolTip = "Save";
            saveButton.Icon = (Geometry)Application.Current.FindResource("IconSave");
            saveButton.HoverColor = (SolidColorBrush)Application.Current.FindResource("ElementSelectBrush");
            saveButton.ClickAction = HandleSaveClick;
            ExtensionControls.Add(saveButton);
        }

        #region Handlers

        private void HandleRemoveClick(ActionButtonViewModel button) {
            dataRepository.DeleteTag(Model.Id);
            eventAggregator.PublishOnCurrentThreadAsync(new TagEvent(Model, TagEventType.Remove));
            eventAggregator.PublishOnCurrentThreadAsync(PopupEvent.Close());
        }

        private void HandleSaveClick(ActionButtonViewModel button) {
            Model.Value = Value;
            dataRepository.UpdateTag(Model.Id, Model.Value);
        }

        #endregion

    }

}
