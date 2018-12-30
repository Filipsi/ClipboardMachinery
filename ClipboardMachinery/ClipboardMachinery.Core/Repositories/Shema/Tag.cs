using ServiceStack.DataAnnotations;

namespace ClipboardMachinery.Core.Repositories.Shema {

    public class Tag {

        [Reference]
        public TagType Type { get; set; }

        [Required]
        public object Value { get; set; }

    }

}
