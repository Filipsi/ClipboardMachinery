using ServiceStack.DataAnnotations;

namespace ClipboardMachinery.Core.DataStorage.Schema {

    [Alias("Tag")]
    public class TagEntity {

        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(ClipEntity))]
        public int? ClipId { get; set; }

        [References(typeof(TagTypeEntity))]
        public string TypeName { get; set; }

        [Reference]
        public TagTypeEntity Type { get; set; }

        [Required]
        public object Value { get; set; }

    }

}
