using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Bibliotheque.FileSystem;
using Caliburn.Micro;
using ClipboardMachinery.ViewModels;
using Newtonsoft.Json;

namespace ClipboardMachinery.FileSystem {

    internal class ClipFile : JsonFile {

        public static readonly ClipFile Instance = new ClipFile(
            Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)
        );

        [JsonProperty("favorites")]
        public List<ClipViewModel> Favorites { private set; get; } = new List<ClipViewModel>();

        private ClipFile(string path) : base(Path.Combine(path, "clips.json")) {
            if (Exists) {
                Load();
            }
            else {
                Save();
            }
        }

        public sealed override void Load() {
            base.Load();

            foreach (ClipViewModel favorite in Favorites) {
                IoC.BuildUp(favorite);
                favorite.IsFavorite = true;
            }

            Favorites.Sort((x, y) => DateTime.Compare(y.Created, x.Created));
        }

        public sealed override void Save()
            => base.Save();

    }

}
