using System;
using System.Collections.Generic;
using ClipboardMachinery.Core.DataStorage.Schema;
using System.Windows.Media;

namespace ClipboardMachinery.Core {

    public static class SystemTagTypes {

        #region Colors

        internal static readonly ColorEntity DefaultDBColor = new ColorEntity {
            A = 255,
            R = 41,
            G = 128,
            B = 185
        };

        public static readonly Color DefaultColor = Color.FromArgb(
            DefaultDBColor.A,
            DefaultDBColor.R,
            DefaultDBColor.G,
            DefaultDBColor.B
        );

        private static readonly ColorEntity YellowDBColor = new ColorEntity {
            A = 255,
            R = 241,
            G = 196,
            B = 15
        };

        private static readonly ColorEntity DarkBlueDBColor = new ColorEntity {
            A = 255,
            R = 96,
            G = 125,
            B = 139
        };

        private static readonly ColorEntity GreenColor = new ColorEntity {
            A = 255,
            R = 39,
            G = 174,
            B = 96
        };

        #endregion

        #region TagTypes

        public static readonly TagTypeEntity SourceTagType = new TagTypeEntity {
            Name = "source",
            Description = "Name of the process that was focused when clip was created.",
            Kind = typeof(string),
            Color = DefaultDBColor
        };

        public static readonly TagTypeEntity CreatedTagType = new TagTypeEntity {
            Name = "created",
            Description = "Timestamp created when clip was added to the clipboard.",
            Kind = typeof(DateTime),
            Color = DarkBlueDBColor
        };

        public static readonly TagTypeEntity CategoryTagType = new TagTypeEntity {
            Name = "category",
            Description = "Describes a category to which the clip belongs to, useful for sorting.",
            Kind = typeof(string),
            Color = YellowDBColor
        };

        public static readonly TagTypeEntity WorkspaceTagType = new TagTypeEntity {
            Name = "workspace",
            Description = "Specifies a working directory used by script runners.",
            Kind = typeof(Uri),
            Color = GreenColor
        };

        public static readonly IReadOnlyCollection<TagTypeEntity> TagTypes = Array.AsReadOnly(
            new[] {
                SourceTagType,
                CreatedTagType,
                CategoryTagType,
                WorkspaceTagType
            }
        );

        #endregion

    }

}
