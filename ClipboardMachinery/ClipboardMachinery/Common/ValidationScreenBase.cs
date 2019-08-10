using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Caliburn.Micro;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace ClipboardMachinery.Common {

    public abstract class ValidationScreenBase : Screen, INotifyDataErrorInfo {

        #region Properties

        public bool HasErrors {
            get {
                lock (validationLock) {
                    return errors != null && errors.Any(propErrors => propErrors.Value != null && propErrors.Value.Count > 0);
                }
            }
        }

        public bool IsValid
            => !HasErrors;

        #endregion

        #region Events

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        #endregion

        #region Fields

        private readonly Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>();
        private readonly object validationLock = new object();

        #endregion

        #region Logic

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

        public void NotifyOfErrorsChange([CallerMemberName] string propertyName = null) {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public void ValidateProperty(object value, [CallerMemberName] string propertyName = null) {
            lock (validationLock) {
                ValidationContext validationContext = new ValidationContext(this, null, null) {
                    MemberName = propertyName
                };

                List<ValidationResult> validationResults = new List<ValidationResult>();
                Validator.TryValidateProperty(value, validationContext, validationResults);

                // Clear previous errors from tested property
                if (errors.ContainsKey(propertyName ?? throw new ArgumentNullException(nameof(propertyName)))) {
                    errors.Remove(propertyName);
                }

                NotifyOfErrorsChange(propertyName);
                HandleValidationResults(validationResults);
            }
        }

        public void Validate() {
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
                propertyNames.ForEach(NotifyOfErrorsChange);
                HandleValidationResults(validationResults);
            }
        }

        private void HandleValidationResults(IEnumerable<ValidationResult> validationResults) {
            // Group validation results by property names
            IEnumerable<IGrouping<string, ValidationResult>> resultsByPropNames = validationResults
                .SelectMany(res => res.MemberNames, (result, memberName) => new {memberName, result})
                .GroupBy(t => t.memberName, t => t.result);

            // Add errors to dictionary and inform binding engine about errors
            foreach (IGrouping<string, ValidationResult> prop in resultsByPropNames) {
                List<string> messages = prop.Select(r => r.ErrorMessage).ToList();
                errors.Add(prop.Key, messages);
                NotifyOfErrorsChange(prop.Key);
            }
        }

        #endregion

    }

}
