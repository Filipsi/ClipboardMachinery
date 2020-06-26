using System;

namespace ClipboardMachinery.Core.TagKind.Impl.Schemas {

    public class UriTagKindSchema : ITagKindSchema {

        #region Properties

        public Type Kind { get; } = typeof(Uri);

        public string Name { get; } = "Uniform Resource Identifier";

        public string Description { get; } = "Absolute resource identifier such as URL address, SSH connection or file system path.";

        public string Icon { get; } = "IconCompass";

        #endregion

        #region Logic

        public bool TryParse(string value, out object result) {
            if (value.Length > 3 && value.Substring(1,1) == ":") {
                value = "file:///" + value;
            }

            value = value.Replace('\\', '/');

            if (Uri.IsWellFormedUriString(value, UriKind.Absolute)) {
                result = new Uri(value, UriKind.Absolute);
                return true;
            }

            result = null;
            return false;
        }

        public string ToPersistentValue(object value) {
            if (value is Uri resource) {
                return resource.AbsoluteUri;
            }

            return value.ToString();
        }

        #endregion

    }

}
