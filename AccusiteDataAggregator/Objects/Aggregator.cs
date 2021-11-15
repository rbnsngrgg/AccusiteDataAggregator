using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

#nullable enable
[assembly: InternalsVisibleTo("DataAggregatorTests")]
namespace AccusiteDataAggregator.Objects
{
    internal static class Aggregator
    {
        public static ADAConfig Config { get; private set; } = new();
        private static IFileWrapper FileWrapper { get; set; } = new FileWrapper();
        private static IDirectoryWrapper DirectoryWrapper { get; set; } = new DirectoryWrapper();

        public static void Init()
        {
            Config.Init();
        }

        public static void Init(ADAConfig config, IFileWrapper fileWrapper, IDirectoryWrapper directoryWrapper)
        {
            Config = config;
            Config.Init();
            FileWrapper = fileWrapper;
            DirectoryWrapper = directoryWrapper;
        }

        public static void Reset()
        {
            Config = new();
        }

        public static bool TrackerHasValidData(string sn)
        {
            return HasNoWindowsRun(sn) && HasFixtureNormalRun(sn);
        }

        private static bool HasNoWindowsRun(string sn)
        {
            foreach(string folder in DirectoryWrapper.GetDirectories(Config.ImagerFixtureImagesPath))
            {
                if (folder.Contains("NW") && folder.Contains(sn)) { return true; }
            }
            return false;
        }

        private static bool HasFixtureNormalRun(string sn)
        {
            foreach (string folder in DirectoryWrapper.GetDirectories(Config.ImagerFixtureImagesPath))
            {
                if (Path.GetFileNameWithoutExtension(folder) == $"SN{sn}") { return true; }
            }
            return false;
        }

        private static bool HasKronosNormalRun(string sn)
        {
            foreach (string folder in DirectoryWrapper.GetDirectories(Config.KronosRectImagesPath))
            {
                if (Path.GetFileNameWithoutExtension(folder) == $"SN{sn}") { return true; }
            }
            return false;
        }
    }
}
#nullable disable
