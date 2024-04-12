using CarJack.Common;
using CarJack.Common.WhipRemix;
using CommonAPI.Phone;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarJack.Plugin
{
    public class RecolorApp : CustomApp
    {
        private const string NewRecolorFolder = "WhipRemix";
        public override bool Available => false;
        public override void OnAppInit()
        {
            base.OnAppInit();
            CreateIconlessTitleBar("WhipRemix");
            ScrollView = PhoneScrollView.Create(this);
            var newRecolorButton = PhoneUIUtility.CreateSimpleButton("Create New Recolor");
            newRecolorButton.OnConfirm += () =>
            {
                var carController = CarController.Instance;
                var recolor = new Recolor();
                recolor.CreateDefault(carController.CurrentCar.GetComponent<RecolorableCar>());

                var recolorDirectory = Path.Combine(RecolorManager.RecolorFolder, NewRecolorFolder);

                if (!Directory.Exists(recolorDirectory))
                    Directory.CreateDirectory(recolorDirectory);

                var path = GetUniquePath(Path.Combine(recolorDirectory, $"{recolor.Properties.RecolorDisplayName}.whipremix"));
                recolor.Properties.RecolorDisplayName = Path.GetFileNameWithoutExtension(path);
                recolor.Save(path);
            };
            ScrollView.AddButton(newRecolorButton);
        }

        private string GetUniquePath(string path)
        {
            if (!File.Exists(path))
                return path;
            var pathIndex = 2;
            var extension = Path.GetExtension(path);
            var noExtension = Path.GetFileNameWithoutExtension(path);
            var directory = Path.GetDirectoryName(path);
            while (File.Exists(Path.Combine(directory, $"{noExtension} {pathIndex}{extension}")))
                pathIndex++;
            return Path.Combine(directory, $"{noExtension} {pathIndex}{extension}");
        }
    }
}
