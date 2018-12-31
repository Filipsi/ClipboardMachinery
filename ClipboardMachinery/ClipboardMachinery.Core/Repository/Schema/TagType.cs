using ServiceStack.DataAnnotations;
using System;
using System.Windows.Media;

namespace ClipboardMachinery.Core.Repository.Schema {

    public class TagType {

        [PrimaryKey]
        public string Name { get; set; }

        [Required]
        public Type Type { get; set; }

        public Color Color { get; set; }

    }

}
