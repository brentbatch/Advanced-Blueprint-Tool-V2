#if false

// pre-compiled in a different project to work around github workflow issues



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StandAloneFileBrowser
{
    public struct ExtensionFilter
    {
        public string Name;
        public string[] Extensions;

        public ExtensionFilter(string filterName, params string[] filterExtensions)
        {
            Name = filterName;
            Extensions = filterExtensions;
        }
    }
}

#endif