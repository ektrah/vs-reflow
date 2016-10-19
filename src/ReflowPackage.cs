using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;

namespace Reflow
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.guidReflowPackageString)]
    [ProvideAutoLoad(VSConstants.VsEditorFactoryGuid.TextEditor_string)]
    [ProvideOptionPage(typeof(ReflowOptionPage), "Environment", "Reflow", 0, 0, true, new[] { "text", "markdown", "reflow" })]
    public sealed class ReflowPackage : Package
    {
        public static ReflowOptionPage Options { get; private set; }

        protected override void Initialize()
        {
            base.Initialize();
            Options = (ReflowOptionPage)GetDialogPage(typeof(ReflowOptionPage));
        }
    }
}
