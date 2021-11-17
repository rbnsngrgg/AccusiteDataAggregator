using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Globalization;

namespace AccusiteDataAggregator.Objects
{
    internal class TrackerData
    {
        public ExcData FixtureNormalExc { get; private set; }
        public ExcData FixtureNoWindowsExc { get; private set; }
        public ExcData KronosNormalExc { get; private set; }
        public string SerialNumber { get; private set; }
        private IFileWrapper FileWrapper { get; set; }
        private IDirectoryWrapper DirectoryWrapper { get; set; }

        public TrackerData(string serialNumber)
        {
            SerialNumber = SerialNumberIsValid(serialNumber) ? serialNumber
                : throw new ArgumentException($"TrackerData: Invalid serial number format \"{serialNumber}\" ");
            FileWrapper = new FileWrapper();
            DirectoryWrapper = new DirectoryWrapper();
        }

        public TrackerData(string serialNumber, IFileWrapper fileWrapper, IDirectoryWrapper directoryWrapper)
        {
            SerialNumber = SerialNumberIsValid(serialNumber) ? serialNumber
                : throw new ArgumentException($"TrackerData: Invalid serial number format \"{serialNumber}\" ");
            FileWrapper = fileWrapper;
            DirectoryWrapper = directoryWrapper;
        }


        public void SetFixtureNormalExc(string excContainerFolder)
        {
            decimal normalExc0 = Math.Round(GetAverageErrorFromFile(Path.Combine(excContainerFolder, "res_0exc.txt")), 3);
            decimal normalExc1 = Math.Round(GetAverageErrorFromFile(Path.Combine(excContainerFolder, "res_1exc.txt")), 3);
            decimal normalExc2 = Math.Round(GetAverageErrorFromFile(Path.Combine(excContainerFolder, "res_2exc.txt")), 3);
            FixtureNormalExc = new() { res0 = normalExc0, res1 = normalExc1, res2 = normalExc2 };
        }
        public void SetFixtureNWExc(string excContainerFolder)
        {
            decimal NWExc0 = Math.Round(GetAverageErrorFromFile(Path.Combine(excContainerFolder, "res_0exc.txt")), 3);
            decimal NWExc1 = Math.Round(GetAverageErrorFromFile(Path.Combine(excContainerFolder, "res_1exc.txt")), 3);
            decimal NWExc2 = Math.Round(GetAverageErrorFromFile(Path.Combine(excContainerFolder, "res_2exc.txt")), 3);
            FixtureNoWindowsExc = new() { res0 = NWExc0, res1 = NWExc1, res2 = NWExc2 };
        }
        public void SetKronosExc(string excContainerFolder)
        {
            decimal KronosExc0 = Math.Round(GetAverageErrorFromFile(Path.Combine(excContainerFolder, "res_0exc.txt")), 3);
            decimal KronosExc1 = Math.Round(GetAverageErrorFromFile(Path.Combine(excContainerFolder, "res_1exc.txt")), 3);
            decimal KronosExc2 = Math.Round(GetAverageErrorFromFile(Path.Combine(excContainerFolder, "res_2exc.txt")), 3);
            KronosNormalExc = new() { res0 = KronosExc0, res1 = KronosExc1, res2 = KronosExc2 };
        }
        public void WriteData(string outputFolderPath)
        {
            string resultDirectory = Path.Combine(outputFolderPath, $"SN{SerialNumber}");
            string fileName = $"SN{SerialNumber}_Exc.log";
            _ = DirectoryWrapper.CreateDirectory(resultDirectory);
            string[] lines = {
                "RunType\tRes_0Exc\tRes_1Exc\tRes_2Exc",
                $"FixtureNormal\t{FixtureNormalExc.res0}\t{FixtureNormalExc.res1}\t{FixtureNormalExc.res2}",
                $"FixtureNoWindows\t{FixtureNoWindowsExc.res0}\t{FixtureNoWindowsExc.res1}\t{FixtureNoWindowsExc.res2}",
                $"Kronos\t{KronosNormalExc.res0}\t{KronosNormalExc.res1}\t{KronosNormalExc.res2}"
            };
            FileWrapper.WriteAllLines(Path.Combine(resultDirectory, fileName), lines);
        }

        public void CopyGraphImages(string excContainerFolder, string outputFolderPath, string fileNamePrefix)
        {
            string resultDirectory = Path.Combine(outputFolderPath, $"SN{SerialNumber}");
            string copyName = $"{fileNamePrefix}_SlitErr_exc.png";
            string copyPath = Path.Combine(resultDirectory, copyName);
            foreach (string file in DirectoryWrapper.GetFiles(excContainerFolder))
            {
                if (file.Contains("SlitErr_exc.png"))
                {
                    if (FileWrapper.Exists(copyPath))
                    {
                        FileWrapper.Delete(copyPath);
                    }
                    FileWrapper.Copy(file, copyPath);
                }
            }
        }
        private decimal GetAverageErrorFromFile(string filePath)
        {
            decimal avgError = 0;
            int count = 0;
            foreach (string line in FileWrapper.ReadAllLines(filePath))
            {
                decimal value;
                value = Math.Abs(decimal.Parse(
                    line.Split('\t')[2],
                    NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture));
                //try { value = Math.Abs(decimal.Parse(line.Split('\t')[2], CultureInfo.InvariantCulture)); }
                //catch (FormatException e) { value = Math.Abs(decimal.Parse(line.Split('\t')[2], )); }
                avgError += value;
                count++;
            }
            return avgError / count;
        }
        internal static bool SerialNumberIsValid(string sn) => Regex.IsMatch(sn, @"^(?:SN){0,1}\d{6}$");
    }

    internal struct ExcData
    {
        public decimal res0;
        public decimal res1;
        public decimal res2;
    }
}
