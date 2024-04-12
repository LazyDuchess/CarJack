using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarJack.Common.WhipRemix
{
    public static class RecolorManager
    {
        public static string RecolorFolder;
        public static void Initialize(string recolorFolder)
        {
            RecolorFolder = recolorFolder;
        }
    }
}
