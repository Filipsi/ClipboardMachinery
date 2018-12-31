using ServiceStack.DataAnnotations;

namespace ClipboardMachinery.Core.Repository.Schema {

    public class Tag {

        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Clip))]
        public int? ClipId { get; set; }

        [References(typeof(TagType))]
        public string TypeId { get; set; }

        [Reference]
        public TagType Type { get; set; }

        [Required]
        public object Value { get; set; }

    }

}
