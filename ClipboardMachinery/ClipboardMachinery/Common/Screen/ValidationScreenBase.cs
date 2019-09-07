using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Nito.Mvvm;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;
using Action = System.Action;

namespace ClipboardMachinery.Common.Screen {

    public abstract class ValidationScreenBase : Caliburn.Micro.Screen, INotifyDataErrorInfo {

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
            private set {
                if (validationProcess == value) {
                    return;
                }

                if (validationProcess != null) {
                    validationProcess.PropertyChanged -= OnValidationProcessPropertyChanged;
                }

                if (value != null) {
                    value.PropertyChanged += OnValidationProcessPropertyChanged;
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
        private IReadOnlyList<string> propsCache;

        #endregion

        protected ValidationScreenBase() {
        }

        #region Exposed logic

        public void ClearErrors() {
            lock (validationLock) {
                ValidationProcess = null;
                errors.Clear();
                NotifyOfPropertyChange(() => HasErrors);
                NotifyOfPropertyChange(() => IsValid);
                foreach (string propertyName in GetValidationProperties().Where(prop => !disabledProperties.Contains(prop))) {
                    NotifyOfErrorsChange(propertyName);
                }
            }
        }

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

        public Task ValidateProperty(object value, [CallerMemberName] string propertyName = null) {
            return StartValidationProcess(() => HandlePropertyValidation(value, propertyName), CancellationToken.None);
        }

        public Task Validate() {
            return StartValidationProcess(HandleValidation, CancellationToken.None);
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
                foreach (string propertyName in GetValidationProperties().Where(prop => !disabledProperties.Contains(prop))) {
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

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken) {
            if (!close) {
                return base.OnDeactivateAsync(false, cancellationToken);
            }

            if (ValidationProcess != null) {
                ValidationProcess.PropertyChanged -= OnValidationProcessPropertyChanged;
            }

            return base.OnDeactivateAsync(true, cancellationToken);
        }

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


        #endregion

        #region Helpers

        private Task StartValidationProcess(Action validationAction, CancellationToken cancellationToken) {
            if (ValidationProcess?.IsNotCompleted == true) {
                return ValidationProcess.Task;
            }

            ValidationProcess = NotifyTask.Create(Task.Run(validationAction, cancellationToken));
            return ValidationProcess.Task;
        }

        private IReadOnlyList<string> GetValidationProperties() {
            if (propsCache != null) {
                return propsCache;
            }

            PropertyInfo[] publicProperties = GetType().GetProperties(
                bindingAttr: BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty
            );

            IEnumerable<PropertyInfo> validationProperties = publicProperties
                .SelectMany(prop =>
                    prop.GetCustomAttributes(typeof(ValidationAttribute))
                        .Select(attr => new {Property = prop, Attribute = attr}))
                .Where(info => info.Attribute is ValidationAttribute)
                .Select(info => info.Property);

            propsCache = Array.AsReadOnly(
                validationProperties
                    .Select(prop => prop.Name)
                    .Distinct()
                    .ToArray()
            );

            return propsCache;
        }

        protected void NotifyOfErrorsChange([CallerMemberName] string propertyName = null) {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        #endregion

    }

}
