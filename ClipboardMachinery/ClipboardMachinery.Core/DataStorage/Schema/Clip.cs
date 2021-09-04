using ServiceStack.DataAnnotations;
using System.Collections.Generic;

namespace ClipboardMachinery.Core.DataStorage.Schema {

    public class Clip {

        [AutoIncrement]
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public string Presenter { get; set; }

        [Reference]
        public List<Tag> Tags { get; set; }

    }

}
