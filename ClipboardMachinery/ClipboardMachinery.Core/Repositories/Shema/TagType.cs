using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardMachinery.Core.Repositories.Shema {

    public class TagType {

        [Required]
        public string Name { get; set; }

        [Required]
        public Type Type { get; set; }

    }

}
