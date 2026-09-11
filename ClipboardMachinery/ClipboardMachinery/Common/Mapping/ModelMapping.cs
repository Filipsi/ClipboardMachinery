using System.Linq;
using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Components.Tag;
using ClipboardMachinery.Components.TagType;
using ClipboardMachinery.Core.DataStorage.Schema;
using System.Windows.Media;
using ClipboardMachinery.Core;

namespace ClipboardMachinery.Common.Mapping {

    public static class ModelMapping {

        public static ClipModel ToModel(this ClipEntity clip) {
            if (clip == null) {
                return null;
            }

            return new ClipModel(clip.Id, clip.Content, clip.Tags?.Select(ToModel)) {
                Presenter = clip.Presenter
            };
        }

        public static TagModel ToModel(this TagEntity tag) {
            if (tag == null) {
                return null;
            }

            TagTypeEntity type = tag.Type;

            return new TagModel {
                Id = tag.Id,
                TypeName = type?.Name,
                Value = tag.Value?.ToString(),
                ValueKind = type?.Kind,
                Description = type?.Description,
                Priority = type?.Priority ?? 0,
                Color = type?.Color.ToColor()
            };
        }

        public static TagTypeModel ToModel(this TagTypeEntity tagType) {
            if (tagType == null) {
                return null;
            }

            return new TagTypeModel {
                Name = tagType.Name,
                Kind = tagType.Kind,
                Priority = tagType.Priority,
                Description = tagType.Description,
                Color = tagType.Color.ToColor() ?? SystemTagTypes.DefaultColor
            };
        }

        public static Color? ToColor(this ColorEntity color) {
            if (color == null) {
                return null;
            }

            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

    }

}
