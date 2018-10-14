using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardMachinery.Core.Repositories.Shema {

    public class Clip {

        [AutoIncrement]
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public DateTime Created { get; set; }

        public Dictionary<string, string> Tags { get; set; }

    }

}
