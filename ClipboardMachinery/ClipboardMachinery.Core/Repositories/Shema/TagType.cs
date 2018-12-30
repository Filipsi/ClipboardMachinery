using ServiceStack.DataAnnotations;
using System;

namespace ClipboardMachinery.Core.Repositories.Shema {

    public class TagType {

        [Required]
        public string Name { get; set; }

        [Required]
        public Type Type { get; set; }

    }

}
