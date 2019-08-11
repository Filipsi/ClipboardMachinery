using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Threading.Tasks;

namespace ClipboardMachinery.Core.DataStorage.Validation {

    public class DataRepositoryCheckAttribute : ValidationAttribute {

        #region Properties

        public string DataRepositoryProperty { get; set; }

        public bool InvertResult { get; set; }

        public override bool RequiresValidationContext { get; } = true;

        #endregion

        #region Fields

        private static readonly Type dataRepositoryType = typeof(IDataRepository);
        private readonly string validationMethodName;

        private IDataRepository cachedDataRepository;
        private MethodInfo cachedValidationMethod;

        #endregion

        public DataRepositoryCheckAttribute(string validationMethodName) {
            this.validationMethodName = validationMethodName;
        }

        #region Logic

        protected override ValidationResult IsValid(object value, ValidationContext ctx) {
            IDataRepository dataRepository = GetDataRepository(ctx);
            if (dataRepository == null) {
                return new ValidationResult($"Unable to validate property {ctx.MemberName}, data repository instance not found.", new []{ ctx.MemberName });
            }

            MethodInfo validationMethod = GetValidationMethod();
            if (validationMethod == null) {
                return new ValidationResult($"Unable to validate property {ctx.MemberName}, validation method {validationMethodName} not found in data repository implementation.", new[] { ctx.MemberName });
            }

            // Execute validation method
            object result = validationMethod.Invoke(dataRepository, new []{ value });
            bool isResultValid = InvertResult ? !IsResultValid(result) : IsResultValid(result);

            // Resolve result
            return isResultValid
                ? ValidationResult.Success
                : new ValidationResult(ErrorMessage, new[] {ctx.MemberName});
        }

        #endregion

        #region Helpers

        private IDataRepository GetDataRepository(ValidationContext ctx) {
            // Use cached value, if it already was computed
            if (cachedDataRepository != null) {
                return cachedDataRepository;
            }

            // Fall-back for when repository property name is not specified
            // This is expected default behavior
            if (string.IsNullOrEmpty(DataRepositoryProperty)) {
                FieldInfo internalRepositoryField = ctx.ObjectType.GetField(
                    name: "dataRepository",
                    bindingAttr: BindingFlags.Instance | BindingFlags.NonPublic
                );

                cachedDataRepository = (IDataRepository) internalRepositoryField?.GetValue(ctx.ObjectInstance);
            } else {
                // Try to find data repository using a property name
                PropertyInfo repositoryProperty = ctx.ObjectType.GetProperty(
                    name: DataRepositoryProperty,
                    bindingAttr: BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty
                );

                cachedDataRepository = (IDataRepository) repositoryProperty?.GetValue(ctx.ObjectInstance);
            }

            return cachedDataRepository;
        }

        private MethodInfo GetValidationMethod() {
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (cachedValidationMethod == null) {
                cachedValidationMethod = dataRepositoryType.GetMethod(
                    name: validationMethodName,
                    bindingAttr: BindingFlags.Instance | BindingFlags.Public
                );
            }

            return cachedValidationMethod;
        }

        private static bool IsResultValid(object result) {
            switch (result) {
                case bool boolResult:
                    return boolResult;

                case Task<bool> resultTask:
                    return resultTask.Result;
            }

            return false;
        }

        #endregion

    }

}
