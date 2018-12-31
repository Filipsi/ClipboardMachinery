using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;

namespace ClipboardMachinery.Core.Repository.Schema {

    public class Clip {

        [AutoIncrement]
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public DateTime Created { get; set; }

        [Reference]
        public List<Tag> Tags { get; set; }

    }

}
