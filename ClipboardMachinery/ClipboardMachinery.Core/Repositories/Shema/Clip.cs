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

        [Default(OrmLiteVariables.SystemUtc)]
        private DateTime Created { get; set; }

        private Dictionary<string, string> Tags { get; set; }

    }

}
