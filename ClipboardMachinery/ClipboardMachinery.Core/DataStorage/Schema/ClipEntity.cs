using ServiceStack.DataAnnotations;
using System.Collections.Generic;

namespace ClipboardMachinery.Core.DataStorage.Schema {

    [Alias("Clip")]
    public class ClipEntity {

        [AutoIncrement]
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public string Presenter { get; set; }

        [Reference]
        public List<TagEntity> Tags { get; set; }

    }

}
