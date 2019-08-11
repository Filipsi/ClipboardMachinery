using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Nito.Mvvm;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;
using Action = System.Action;

namespace ClipboardMachinery.Common {

    public abstract class ValidationScreenBase : Screen, INotifyDataErrorInfo {

        #region Properties

        public bool HasErrors {
            get {
                lock (validationLock) {
                    return errors?.Any(propErrors => propErrors.Value?.Count > 0) == true;
                }
            }
        }

        public bool IsValid
            => ValidationProcess?.IsCompleted == true && !HasErrors;

        public NotifyTask ValidationProcess {
            get => validationProcess;
            set {
                if (validationProcess == value) {
                    return;
                }

                validationProcess = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Events

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        #endregion

        #region Fields

        private readonly Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>();
        private readonly List<string> disabledProperties = new List<string>();
        private readonly object validationLock = new object();

        private NotifyTask validationProcess;

        #endregion

        protected ValidationScreenBase() {
        }

        #region Exposed logic

        public IEnumerable GetErrors(string propertyName) {
            lock (validationLock) {
                if (string.IsNullOrEmpty(propertyName)) {
                    return errors.SelectMany(err => err.Value.ToList());
                }

                if (errors.ContainsKey(propertyName) && errors[propertyName]?.Count > 0) {
                    return errors[propertyName].ToList();
                }

                return null;
            }
        }

        public void DisablePropertyValidation(params string[] propertyNames) {
            foreach (string propertyName in propertyNames) {
                if (!disabledProperties.Contains(propertyName)) {
                    disabledProperties.Add(propertyName);
                }
            }
        }

        public void EnablePropertyValidation(params string[] propertyNames) {
            foreach (string propertyName in propertyNames) {
                if (disabledProperties.Contains(propertyName)) {
                    disabledProperties.Remove(propertyName);
                }
            }
        }

        public void ValidateProperty(object value, [CallerMemberName] string propertyName = null) {
            StartValidationProcess(() => HandlePropertyValidation(value, propertyName), CancellationToken.None);
        }

        public void Validate() {
            StartValidationProcess(HandleValidation, CancellationToken.None);
        }

        #endregion

        #region Logic

        private void HandlePropertyValidation(object value, string propertyName) {
            lock (validationLock) {
                // Skip property if it has disabled validation
                if (disabledProperties.Contains(propertyName)) {
                    return;
                }

                ValidationContext validationContext = new ValidationContext(this, null, null) {
                    MemberName = propertyName
                };

                List<ValidationResult> validationResults = new List<ValidationResult>();
                Validator.TryValidateProperty(value, validationContext, validationResults);

                // Clear previous errors from tested property
                if (errors.ContainsKey(propertyName)) {
                    errors.Remove(propertyName);
                }

                NotifyOfErrorsChange(propertyName);
                HandleValidationResults(validationResults);
            }
        }

        private void HandleValidation() {
            lock (validationLock) {
                List<ValidationResult> validationResults = new List<ValidationResult>();

                Validator.TryValidateObject(
                    instance: this,
                    validationContext: new ValidationContext(this, null, null),
                    validationResults: validationResults,
                    validateAllProperties: true
                );

                // Clear all previous errors
                errors.Clear();

                List<string> propertyNames = errors.Keys.ToList();
                foreach (string propertyName in propertyNames.Where(prop => !disabledProperties.Contains(prop))) {
                    NotifyOfErrorsChange(propertyName);
                }

                HandleValidationResults(validationResults);
            }
        }

        private void HandleValidationResults(IEnumerable<ValidationResult> validationResults) {
            // Group validation results by property names
            IEnumerable<IGrouping<string, ValidationResult>> resultsByPropNames = validationResults
                .SelectMany(res => res.MemberNames, (result, memberName) => new { memberName, result })
                .GroupBy(t => t.memberName, t => t.result);

            // Add errors to dictionary and inform binding engine about errors
            foreach (IGrouping<string, ValidationResult> prop in resultsByPropNames) {
                List<string> messages = prop.Select(r => r.ErrorMessage).ToList();
                string propertyName = prop.Key;

                if (disabledProperties.Contains(propertyName)) {
                    continue;
                }

                errors.Add(propertyName, messages);
                NotifyOfErrorsChange(prop.Key);
            }
        }

        #endregion

        #region Handlers

        private void OnValidationProcessPropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(NotifyTask.IsCompleted):
                    NotifyOfPropertyChange(() => HasErrors);
                    NotifyOfPropertyChange(() => IsValid);
                    OnValidationProcessCompleted();
                    break;
            }
        }

        internal virtual void OnValidationProcessCompleted() {
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken) {
            if (!close) {
                return base.OnDeactivateAsync(false, cancellationToken);
            }

            if (ValidationProcess != null) {
                ValidationProcess.PropertyChanged -= OnValidationProcessPropertyChanged;
            }

            return base.OnDeactivateAsync(true, cancellationToken);
        }

        #endregion

        #region Helpers

        private void StartValidationProcess(Action validationAction, CancellationToken cancellationToken) {
            if (ValidationProcess?.IsNotCompleted == true) {
                return;
            }

            if (ValidationProcess != null) {
                ValidationProcess.PropertyChanged -= OnValidationProcessPropertyChanged;
            }

            NotifyTask validationTask = NotifyTask.Create(Task.Run(validationAction, cancellationToken));
            validationTask.PropertyChanged += OnValidationProcessPropertyChanged;
            ValidationProcess = validationTask;
        }

        protected void NotifyOfErrorsChange([CallerMemberName] string propertyName = null) {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        #endregion

    }

}
