using System;
using System.Reflection;
using System.Windows;

namespace ClipboardMachinery {

    public partial class App : Application {

        #region Properties

        public static readonly Version CurrentVersion = Assembly.GetEntryAssembly()?.GetName().Version;
        public static readonly Version DevelopmentVersion = new Version(0, 0, 0, 0);

        #endregion

    }

}
