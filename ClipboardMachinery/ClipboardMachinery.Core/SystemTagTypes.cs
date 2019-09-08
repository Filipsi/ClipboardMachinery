using System;
using System.Collections.Generic;
using ClipboardMachinery.Core.DataStorage.Schema;
using MediaColor = System.Windows.Media.Color;

namespace ClipboardMachinery.Core {

    public static class SystemTagTypes {

        #region Colors

        internal static readonly Color DefaultDBColor = new Color {
            A = 255,
            R = 41,
            G = 128,
            B = 185
        };

        private static readonly Color YellowDBColor = new Color {
            A = 255,
            R = 241,
            G = 196,
            B = 15
        };

        public static readonly MediaColor DefaultColor = MediaColor.FromArgb(
            DefaultDBColor.A,
            DefaultDBColor.R,
            DefaultDBColor.G,
            DefaultDBColor.B
        );

        #endregion

        #region TagTypes

        public static readonly TagType SourceTagType = new TagType {
            Name = "source",
            Description = "Name of the process that was focused when clip was created.",
            Kind = typeof(string),
            Color = DefaultDBColor
        };

        public static readonly TagType CategoryTagType = new TagType {
            Name = "category",
            Description = "Describes a category to which the clip belongs to, useful for sorting.",
            Kind = typeof(string),
            Color = YellowDBColor
        };

        public static readonly IReadOnlyCollection<TagType> TagTypes = Array.AsReadOnly(
            new[] {
                SourceTagType,
                CategoryTagType
            }
        );

        #endregion

    }

}
