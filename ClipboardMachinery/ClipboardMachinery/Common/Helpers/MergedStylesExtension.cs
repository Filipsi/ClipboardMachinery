﻿using System;
using System.Windows;
using System.Windows.Markup;

namespace ClipboardMachinery.Common.Helpers {

    [MarkupExtensionReturnType(typeof(Style))]
    public class MergedStylesExtension : MarkupExtension {

        #region Properties

        public Style BasedOn { get; set; }

        public Style MergeStyle { get; set; }

        #endregion

        #region Logic

        public override object ProvideValue(IServiceProvider serviceProvider) {
            if (MergeStyle == null) {
                return BasedOn;
            }

            Style newStyle = new Style(BasedOn.TargetType, BasedOn);
            MergeWithStyle(newStyle, MergeStyle);
            return newStyle;
        }

        private static void MergeWithStyle(Style style, Style mergeStyle) {
            // Recursively merge with any Styles this Style might be BasedOn
            if (mergeStyle.BasedOn != null) {
                MergeWithStyle(style, mergeStyle.BasedOn);
            }

            // Merge the Setters
            foreach (SetterBase setter in mergeStyle.Setters) {
                style.Setters.Add(setter);
            }

            // Merge the Triggers
            foreach (TriggerBase trigger in mergeStyle.Triggers) {
                style.Triggers.Add(trigger);
            }
        }

        #endregion

    }

}
