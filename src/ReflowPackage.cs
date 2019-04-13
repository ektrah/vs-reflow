using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;

namespace Reflow
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.guidReflowPackageString)]
    [ProvideAutoLoad(VSConstants.VsEditorFactoryGuid.TextEditor_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideOptionPage(typeof(ReflowOptionPage), "Environment", "Reflow", 0, 0, true, new[] { "text", "markdown", "reflow" })]
    public sealed class ReflowPackage : AsyncPackage
    {
        public static ReflowOptionPage Options { get; private set; }

        protected override async System.Threading.Tasks.Task InitializeAsync(System.Threading.CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);
            await JoinableTaskFactory.SwitchToMainThreadAsync();
            Options = (ReflowOptionPage)GetDialogPage(typeof(ReflowOptionPage));
        }
    }
}
