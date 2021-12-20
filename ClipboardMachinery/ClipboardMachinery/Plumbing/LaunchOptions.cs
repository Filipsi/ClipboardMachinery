using CommandLine;

namespace ClipboardMachinery.Plumbing {

    public class LaunchOptions {

        [Option('b', "updater-branch", Required = false, HelpText = "When specified, updater will use given AppVeyor branch instead of GitHub releases.")]
        public string UpdaterBranch { get; set; }

    }

}
