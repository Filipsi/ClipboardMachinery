using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;

namespace ClipboardMachinery.Core.Repositories.Shema {

    public class Clip {

        [AutoIncrement]
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public DateTime Created { get; set; }

        public List<Tag> Tags { get; set; }

    }

}
