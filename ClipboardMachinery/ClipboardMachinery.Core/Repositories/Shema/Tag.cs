using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardMachinery.Core.Repositories.Shema {

    public class Tag {

        [Reference]
        public TagType Type { get; set; }

        [Required]
        public object Value { get; set; }

    }

}
