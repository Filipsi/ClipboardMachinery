using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Bibliotheque.FileSystem;
using Caliburn.Micro;
using ClipboardMachinery.Events.Collection;
using ClipboardMachinery.Models;
using ClipboardMachinery.ViewModels;
using Newtonsoft.Json;

namespace ClipboardMachinery.FileSystem {

    [JsonObject(MemberSerialization.OptIn)]
    public class ClipFile : JsonFile, IHandle<ItemFavoriteChanged<ClipViewModel>>, IHandle<ItemRemoved<ClipViewModel>> {

        #region Properties

        [JsonProperty("favorites")]
        public List<ClipModel> Favorites {
            get; private set;
        }

        #endregion

        public ClipFile() : base(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "clips.json")) {
            Favorites = new List<ClipModel>();

            if (Exists) {
                Load();
            }
            else {
                Save();
            }
        }

        #region File

        public sealed override void Load() {
            base.Load();

            foreach (ClipModel clip in Favorites) {
                clip.IsFavorite = true;
            }

            Favorites.Sort((x, y) => DateTime.Compare(y.Created, x.Created));
        }

        public sealed override void Save()
            => base.Save();

        #endregion

        #region Handlers

        public void Handle(ItemFavoriteChanged<ClipViewModel> message) {
            if (message.Item.Model.IsFavorite) {
                Favorites.Add(message.Item.Model);
            } else {
                Favorites.Remove(message.Item.Model);
            }

            Save();
        }

        public void Handle(ItemRemoved<ClipViewModel> message) {
            if (message.Item.Model.IsFavorite) {
                Favorites.Remove(message.Item.Model);
                Save();
            }
        }

        #endregion

    }

}
