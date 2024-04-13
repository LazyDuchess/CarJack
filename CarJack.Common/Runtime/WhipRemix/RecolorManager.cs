using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CarJack.Common.WhipRemix
{
    public static class RecolorManager
    {
        public static string RecolorFolder;
        public static Dictionary<string, Recolor> RecolorsByGUID;
        public static void Initialize(string recolorFolder)
        {
            RecolorFolder = recolorFolder;
        }

        public static void UnloadRecolors()
        {
            foreach(var recolor in RecolorsByGUID)
            {
                recolor.Value.Dispose();
            }
            RecolorsByGUID = new();
        }

        public static void LoadRecolors()
        {
            RecolorsByGUID = new();
            var recolorPaths = Directory.GetFiles(RecolorFolder, "*.whipremix", SearchOption.AllDirectories);
            foreach(var recolorPath in recolorPaths)
            {
                try
                {
                    var recolor = new Recolor();
                    recolor.FromFile(recolorPath);
                    RecolorsByGUID[recolor.Properties.RecolorGUID] = recolor;
                }
                catch(Exception e)
                {
                    Debug.LogError($"CarJack WhipRemix: Failed to load recolor {recolorPath}!\nException:\n{e}");
                }
            }
        }
    }
}
