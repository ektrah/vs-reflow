using System.ComponentModel.Composition;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace Reflow
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    public sealed class ReflowTextViewCreationListener : IWpfTextViewCreationListener
    {
        private readonly IVsEditorAdaptersFactoryService _adapterFactory;

        [ImportingConstructor]
        public ReflowTextViewCreationListener(IVsEditorAdaptersFactoryService adapterFactory)
        {
            _adapterFactory = adapterFactory;
        }

        public void TextViewCreated(IWpfTextView textView)
        {
            IVsTextView textViewAdapter = _adapterFactory.GetViewAdapter(textView);
            ReflowCommand command = new ReflowCommand(textViewAdapter);
            IOleCommandTarget next;
            ErrorHandler.ThrowOnFailure(textViewAdapter.AddCommandFilter(command, out next));
            command.Next = next;
        }
    }
}
