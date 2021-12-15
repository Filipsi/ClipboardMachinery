using ServiceStack.DataAnnotations;
using System;

namespace ClipboardMachinery.Core.DataStorage.Schema {

    public class TagType {

        [PrimaryKey]
        public string Name { get; set; }

        [Required]
        public Type Kind { get; set; }

        public byte Priority { get; set; }

        public string Description { get; set; }

        public Color Color { get; set; }

    }

}
