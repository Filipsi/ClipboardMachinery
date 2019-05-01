using ServiceStack.DataAnnotations;

namespace ClipboardMachinery.Core.Data.Schema {

    public class Tag {

        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Clip))]
        public int? ClipId { get; set; }

        [References(typeof(TagType))]
        public string TypeName { get; set; }

        [Reference]
        public TagType Type { get; set; }

        [Required]
        public object Value { get; set; }

    }

}
