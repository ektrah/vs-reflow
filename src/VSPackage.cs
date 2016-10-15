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
    public sealed class VSPackage : Package { }
}
