﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHARED_UHD_BIN
{
    public static class Shared
    {
        private const string VERSION = "B.1.3.0 (2025-04-20)";

        public static string HeaderText()
        {
            return "# github.com/JADERLINK/RE4-UHD-BIN-TOOL" + Environment.NewLine +
                   "# youtube.com/@JADERLINK" + Environment.NewLine +
                   "# RE4_X360PS3_BIN_TOOL by: JADERLINK" + Environment.NewLine +
                   "# Thanks to \"mariokart64n\" and \"CodeMan02Fr\"" + Environment.NewLine +
                   "# Material information by \"Albert\"" + Environment.NewLine +
                  $"# Version {VERSION}";
        }

        public static string HeaderTextSmd() 
        {
            return "// RE4_X360PS3_BIN_TOOL" + Environment.NewLine +
                   "// by: JADERLINK" + Environment.NewLine +
                   "// youtube.com/@JADERLINK" + Environment.NewLine +
                  $"// Version {VERSION}";       
        }
    }
}
