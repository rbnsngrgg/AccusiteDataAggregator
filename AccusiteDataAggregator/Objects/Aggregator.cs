using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#nullable enable
[assembly: InternalsVisibleTo("DataAggregatorTests")]
namespace AccusiteDataAggregator.Objects
{
    internal static class Aggregator
    {
        public static ADAConfig Config { get; private set; } = new();
        private static IDirectoryWrapper DirectoryWrapper { get; set; } = new DirectoryWrapper();

        public static void Init() => Config.Init();

        public static void Init(ADAConfig config, IDirectoryWrapper directoryWrapper)
        {
            Config = config;
            Config.Init();
            DirectoryWrapper = directoryWrapper;
        }

        public static void Reset() => Config = new();

        public static void CollectSingleTrackerData(string sn)
        {
            if (TrackerHasValidData(sn))
            {
                TrackerData tracker = new(sn);
                string fixtureNormalExcFolder = Path.Combine(
                    Path.Combine(Config.ImagerFixtureImagesPath, $"SN{sn}"), "L0.35H0.95");
                string fixtureNWExcFolder = Path.Combine(
                    Path.Combine(Config.ImagerFixtureImagesPath, $"SN{sn}_NW"), "L0.35H0.95");
                string kronosExcFolder = Path.Combine(
                    Path.Combine(Config.KronosRectImagesPath, $"SN{sn}"), "L0.35H0.95");
                tracker.SetFixtureNormalExc(fixtureNormalExcFolder);
                tracker.SetFixtureNWExc(fixtureNWExcFolder);
                tracker.SetKronosExc(kronosExcFolder);
                tracker.WriteData(Config.OutputFolderPath);
                tracker.CopyGraphImages(fixtureNormalExcFolder, Config.OutputFolderPath, "FixtureNormal");
                tracker.CopyGraphImages(fixtureNWExcFolder, Config.OutputFolderPath, "FixtureNoWindows");
                tracker.CopyGraphImages(kronosExcFolder, Config.OutputFolderPath, "Kronos");
            }
            else
            {
                throw new ArgumentNullException($"Tracker {sn} does not have all of the required data.");
            }
        }
        public static int CollectAllTrackerData()
        {
            int dataCount = 0;
            foreach (string folder in DirectoryWrapper.GetDirectories(Config.KronosRectImagesPath))
            {
                string folderName = Path.GetFileName(folder);
                if (TrackerData.SerialNumberIsValid(folderName))
                {
                    try
                    {
                        CollectSingleTrackerData(folderName.Replace("SN", ""));
                        dataCount++;
                    }
                    catch { }
                }
            }
            return dataCount;
        }
        public static bool TrackerHasValidData(string sn) => HasNoWindowsRun(sn) && HasFixtureNormalRun(sn) && HasKronosNormalRun(sn);

        private static bool HasNoWindowsRun(string sn)
        {
            foreach (string folder in DirectoryWrapper.GetDirectories(Config.ImagerFixtureImagesPath))
            {
                if (folder.Contains("NW") && folder.Contains(sn))
                {
                    foreach (string subfolder in DirectoryWrapper.GetDirectories(folder))
                    {
                        if (MatchDataFolderName(subfolder) && HasAllResFiles(subfolder)) { return true; }
                    }
                }
            }
            return false;
        }
        private static bool HasFixtureNormalRun(string sn)
        {
            foreach (string folder in DirectoryWrapper.GetDirectories(Config.ImagerFixtureImagesPath))
            {
                if (Path.GetFileNameWithoutExtension(folder) == $"SN{sn}")
                {
                    foreach (string subfolder in DirectoryWrapper.GetDirectories(folder))
                    {
                        if (MatchDataFolderName(subfolder) && HasAllResFiles(subfolder)) { return true; }
                    }
                }
            }
            return false;
        }
        private static bool HasKronosNormalRun(string sn)
        {
            foreach (string folder in DirectoryWrapper.GetDirectories(Config.KronosRectImagesPath))
            {
                if (Path.GetFileNameWithoutExtension(folder) == $"SN{sn}")
                {
                    foreach (string subfolder in DirectoryWrapper.GetDirectories(folder))
                    {
                        if (MatchDataFolderName(subfolder) && HasAllResFiles(subfolder)) { return true; }
                    }
                }
            }
            return false;
        }

        private static bool MatchDataFolderName(string folderPath) => Regex.IsMatch(Path.GetFileName(folderPath), @"L\d{1}\.\d{2}H\d{1}\.\d{2}$");
        private static bool HasAllResFiles(string folderPath)
        {
            bool file0 = false;
            bool file1 = false;
            bool file2 = false;
            foreach (string file in DirectoryWrapper.GetFiles(folderPath))
            {
                string fileName = Path.GetFileName(file);
                if (Regex.IsMatch(fileName, @"^res_\d{1}exc.txt$"))
                {
                    if (fileName.Contains("0")) { file0 = true; }
                    else if (fileName.Contains("1")) { file1 = true; }
                    else if (fileName.Contains("2")) { file2 = true; }
                }
            }
            return file0 && file1 && file2;
        }
    }
}
#nullable disable
