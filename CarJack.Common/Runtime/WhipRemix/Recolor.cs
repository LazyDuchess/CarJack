using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.IO;
using UnityEngine;

namespace CarJack.Common.WhipRemix
{
    public class Recolor : IDisposable
    {
        public RecolorProperties Properties;
        public Dictionary<string, RecolorMaterial> RecoloredMaterialByName;
        private List<UnityEngine.Object> _objects;

        public void CreateDefault(RecolorableCar car)
        {
            Properties = new();
            Properties.CarInternalName = car.Car.InternalName;
            Properties.RecolorGUID = Guid.NewGuid().ToString();
            Properties.RecolorDisplayName = "New Recolor";

            RecoloredMaterialByName = new();
            _objects = new();

            foreach(var material in car.RecolorableMaterials)
            {
                var recolorMaterial = new RecolorMaterial();
                recolorMaterial.OriginalMaterialName = material.name;

                Texture2D mainTex = null;
                Texture2D emission = null;

                var originalMainTex = material.GetTexture("_MainTex");
                if (originalMainTex != null)
                {
                    mainTex = new Texture2D(originalMainTex.width, originalMainTex.height);
                    mainTex.filterMode = originalMainTex.filterMode;
                    mainTex.SetPixels((originalMainTex as Texture2D).GetPixels());
                    mainTex.Apply();
                }

                var originalEmission = material.GetTexture("_Emission");
                if (originalEmission != null)
                {
                    emission = new Texture2D(originalEmission.width, originalEmission.height);
                    emission.filterMode = originalEmission.filterMode;
                    emission.SetPixels((originalEmission as Texture2D).GetPixels());
                    emission.Apply();
                }
                recolorMaterial.material = new Material(Shader.Find("Standard"));
                recolorMaterial.material.shader = material.shader;

                recolorMaterial.material.SetTexture("_MainTex", mainTex);
                recolorMaterial.material.SetTexture("_Emission", emission);

                if (mainTex != null)
                    _objects.Add(mainTex);

                if (emission != null)
                    _objects.Add(emission);

                RecoloredMaterialByName[material.name] = recolorMaterial;
            }
        }

        public void Save(string path)
        {
            var zip = ZipFile.Open(path, ZipArchiveMode.Create);
            var entry = zip.CreateEntry("properties.json", System.IO.Compression.CompressionLevel.Optimal);
            using (var stream = entry.Open())
            {
                using (var writer = new StreamWriter(stream))
                {
                    var data = JsonUtility.ToJson(Properties, true);
                    writer.Write(data);
                }
            }
            foreach(var recolorMaterial in RecoloredMaterialByName)
            {
                var mainTex = recolorMaterial.Value.material.GetTexture("_MainTex");
                var emission = recolorMaterial.Value.material.GetTexture("_Emission");

                if (mainTex != null)
                {
                    var mainTexData = (mainTex as Texture2D).EncodeToPNG();
                    entry = zip.CreateEntry($"{recolorMaterial.Value.OriginalMaterialName}_Main.png");
                    using (var stream = entry.Open())
                    {
                        using (var writer = new BinaryWriter(stream))
                        {
                            writer.Write(mainTexData);
                        }
                    }
                }

                if (emission != null)
                {
                    var emissionData = (emission as Texture2D).EncodeToPNG();
                    entry = zip.CreateEntry($"{recolorMaterial.Value.OriginalMaterialName}_Emission.png");
                    using (var stream = entry.Open())
                    {
                        using (var writer = new BinaryWriter(stream))
                        {
                            writer.Write(emissionData);
                        }
                    }
                }
            }
            zip.Dispose();
        }

        public void FromFile(string path)
        {
            var zip = ZipFile.Open(path, ZipArchiveMode.Read);
            using (var stream = zip.GetEntry("properties.json").Open())
            {
                using (var reader = new StreamReader(stream))
                {
                    var data = reader.ReadToEnd();
                    Properties = JsonUtility.FromJson<RecolorProperties>(data);
                }
            }
            RecoloredMaterialByName = new();
            _objects = new();
            var entries = zip.Entries;
            foreach(var entry in entries)
            {
                if (!entry.Name.ToLowerInvariant().EndsWith(".png")) continue;
                if (!entry.Name.Contains("_")) continue;
                var name = Path.GetFileNameWithoutExtension(entry.Name);
                var splitName = name.Split('_');
                var materialName = splitName[0];
                var textureName = splitName[1].ToLowerInvariant();
                var data = new byte[] { };

                using (var stream = entry.Open())
                {
                    using (var ms = new MemoryStream()) {
                        stream.CopyTo(ms);
                        data = ms.ToArray();
                    }
                }

                var texture = new Texture2D(2, 2);
                texture.LoadImage(data);
                _objects.Add(texture);

                if (!RecoloredMaterialByName.TryGetValue(materialName, out var result))
                {
                    result = new RecolorMaterial();
                    result.OriginalMaterialName = materialName;
                    result.material = new Material(Shader.Find("Standard"));
                    RecoloredMaterialByName[materialName] = result;
                }

                switch (textureName)
                {
                    case "main":
                        result.material.SetTexture("_MainTex", texture);
                        break;
                    case "emission":
                        result.material.SetTexture("_Emission", texture);
                        break;
                }
            }
            zip.Dispose();
        }

        public void Dispose()
        {
            foreach(var obj in _objects)
            {
                UnityEngine.Object.DestroyImmediate(obj);
            }
        }

        public class RecolorProperties
        {
            public string RecolorGUID;
            public string RecolorDisplayName;
            public string CarInternalName;
        }

        public class RecolorMaterial
        {
            public string OriginalMaterialName;
            public Material material;
        }
    }
}
