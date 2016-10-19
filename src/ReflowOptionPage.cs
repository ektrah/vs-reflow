using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace Reflow
{
    public sealed class ReflowOptionPage : DialogPage
    {
        public const int DefaultLineLength = 80;

        private int _preferredLineLength = DefaultLineLength;

        [Category("General")]
        [DisplayName("Preferred Line Length")]
        [Description("Sets the preferred line length for reflowing a paragraph.")]
        [DefaultValue(DefaultLineLength)]
        public int PreferredLineLength
        {
            get { return _preferredLineLength; }
            set { _preferredLineLength = value; }
        }
    }
}
