using ServiceStack.DataAnnotations;
using System;

namespace ClipboardMachinery.Core.Data.Schema {

    public class TagType {

        [PrimaryKey]
        public string Name { get; set; }

        [Required]
        public Type Kind { get; set; }

        public Color Color { get; set; }

    }

}
