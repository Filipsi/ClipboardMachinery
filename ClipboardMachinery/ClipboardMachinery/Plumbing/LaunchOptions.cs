using CommandLine;

namespace ClipboardMachinery.Plumbing {

    public class LaunchOptions {

        [Option('d', "disable-updater", Required = false, HelpText = "When specified, the application will not check for updates.")]
        public bool DisableUpdater { get; set; }

    }

}
