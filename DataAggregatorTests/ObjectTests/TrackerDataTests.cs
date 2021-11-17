using Moq;
using NUnit.Framework;
using AccusiteDataAggregator.Objects;
using System;
using System.Xml;
using System.IO;

namespace DataAggregatorTests
{
    public class TrackerDataTests
    {
        private TrackerData trackerData;
        private string testSN = "139123";
        private readonly Mock<FileWrapper> fileWrapperMock = new();
        private readonly Mock<DirectoryWrapper> directoryWrapperMock = new();

        private readonly string normalExcContainerFolder = @"\\brownsharpefxtr\mfg\RectImages\SN139123\L0.35H0.95\";
        private readonly string NWExcContainerFolder = @"\\brownsharpefxtr\mfg\RectImages\SN139123_NW\L0.35H0.95\";
        private readonly string KronosExcFolder = @"\\kronon\RectImages\SN139123\L0.35H0.95\";
        private readonly string dataOutputPath = @"\\castor\Production\Manufacturing\Accusite\DataAggregation\";

        private readonly decimal testAverage0 = Math.Round((Math.Abs((decimal)-0.39330895538158644) + Math.Abs((decimal)-0.24868767564533506) + Math.Abs((decimal)0.13543130233699713)) / 3, 3);
        private readonly decimal testAverage1 = Math.Round((Math.Abs((decimal)-0.38330895538158644) + Math.Abs((decimal)-0.23868767564533506) + Math.Abs((decimal)0.12543130233699713)) / 3, 3);
        private readonly decimal testAverage2 = Math.Round((Math.Abs((decimal)-0.37330895538158644) + Math.Abs((decimal)-0.22868767564533506) + Math.Abs((decimal)0.11543130233699713)) / 3, 3);

        private readonly string[] exc0 = {
                "624.539\t3869.7\t-0.39330895538158644",
                "622.831\t3616.48\t-0.24868767564533506",
                "645.217\t1077.68\t0.13543130233699713"
        };
        private readonly string[] exc1 = {
                "624.539\t3869.7\t-0.38330895538158644",
                "622.831\t3616.48\t-0.23868767564533506",
                "645.217\t1077.68\t0.12543130233699713"
        };
        private readonly string[] exc2 = {
                "624.539\t3869.7\t-0.37330895538158644",
                "622.831\t3616.48\t-0.22868767564533506",
                "645.217\t1077.68\t0.11543130233699713"
        };
        [SetUp]
        public void Setup()
        {
            trackerData = new(testSN, fileWrapperMock.Object, directoryWrapperMock.Object);
            _ = fileWrapperMock.Setup(x => x.ReadAllLines(It.Is<string>(str => str.Contains("res_0exc.txt")))).Returns(exc0);
            _ = fileWrapperMock.Setup(x => x.ReadAllLines(It.Is<string>(str => str.Contains("res_1exc.txt")))).Returns(exc1);
            _ = fileWrapperMock.Setup(x => x.ReadAllLines(It.Is<string>(str => str.Contains("res_2exc.txt")))).Returns(exc2);
            _ = fileWrapperMock.Setup(x => x.WriteAllLines(It.IsAny<string>(), It.IsAny<string[]>()));
            _ = fileWrapperMock.Setup(x => x.Copy(It.IsAny<string>(), It.IsAny<string>()));
            _ = fileWrapperMock.Setup(x => x.Delete(It.IsAny<string>()));
            _ = directoryWrapperMock.Setup(x => x.CreateDirectory(It.IsAny<string>()));
        }

        [Test]
        public void TestSNValidation()
        {
            //Serial numbers in valid format: only numbers, with or without "SN" prefix.
            Assert.DoesNotThrow(() => trackerData = new(testSN, fileWrapperMock.Object, directoryWrapperMock.Object));
            Assert.DoesNotThrow(() => trackerData = new($"SN{testSN}", fileWrapperMock.Object, directoryWrapperMock.Object));

            //Invalid format -- too short
            _ = Assert.Throws<ArgumentException>(() => trackerData = new("13912"));

            //Invalid format -- invalid characters
            _ = Assert.Throws<ArgumentException>(() => trackerData = new("123123SN"));
        }

        [Test]
        public void TestSetFixtureNormalExc()
        {
            trackerData.SetFixtureNormalExc(normalExcContainerFolder);
            Assert.AreEqual(testAverage0, trackerData.FixtureNormalExc.res0);
            Assert.AreEqual(testAverage1, trackerData.FixtureNormalExc.res1);
            Assert.AreEqual(testAverage2, trackerData.FixtureNormalExc.res2);
        }
        [Test]
        public void TestSetFixtureNWExc()
        {
            trackerData.SetFixtureNWExc(NWExcContainerFolder);
            Assert.AreEqual(testAverage0, trackerData.FixtureNoWindowsExc.res0);
            Assert.AreEqual(testAverage1, trackerData.FixtureNoWindowsExc.res1);
            Assert.AreEqual(testAverage2, trackerData.FixtureNoWindowsExc.res2);

        }
        [Test]
        public void TestKronosExc()
        {
            trackerData.SetKronosExc(KronosExcFolder);
            Assert.AreEqual(testAverage0, trackerData.KronosNormalExc.res0);
            Assert.AreEqual(testAverage1, trackerData.KronosNormalExc.res1);
            Assert.AreEqual(testAverage2, trackerData.KronosNormalExc.res2);
        }

        [Test]
        public void TestWriteData()
        {
            trackerData.SetFixtureNormalExc(normalExcContainerFolder);
            trackerData.SetFixtureNWExc(NWExcContainerFolder);
            trackerData.SetKronosExc(KronosExcFolder);
            trackerData.WriteData(dataOutputPath);
            directoryWrapperMock.Verify(x => x.CreateDirectory(It.Is<string>(str => str == $@"{dataOutputPath}SN{testSN}")), Times.Once);
            fileWrapperMock.Verify(x => x.WriteAllLines(
                It.Is<string>(str => str == $@"\\castor\Production\Manufacturing\Accusite\DataAggregation\SN{testSN}\SN{testSN}_Exc.log"),
                It.Is<string[]>(arr => arr[0] == "RunType\tRes_0Exc\tRes_1Exc\tRes_2Exc"
                    && arr[1] == $"FixtureNormal\t{trackerData.FixtureNormalExc.res0}\t{trackerData.FixtureNormalExc.res1}\t{trackerData.FixtureNormalExc.res2}"
                    && arr[2] == $"FixtureNoWindows\t{trackerData.FixtureNoWindowsExc.res0}\t{trackerData.FixtureNoWindowsExc.res1}\t{trackerData.FixtureNoWindowsExc.res2}"
                    && arr[3] == $"Kronos\t{trackerData.KronosNormalExc.res0}\t{trackerData.KronosNormalExc.res1}\t{trackerData.KronosNormalExc.res2}")
                ), Times.Once);
        }

        [Test]
        public void TestCopyGraphImages()
        {
            string resultDirectory = Path.Combine(dataOutputPath, $"SN{testSN}");
            string copyName = $"TestPrefix_SlitErr_exc.png";
            string copyPath = Path.Combine(resultDirectory, copyName);
            string originalFile = Path.Combine(normalExcContainerFolder, "SlitErr_exc.png");
            _ = directoryWrapperMock.Setup(x => x.GetFiles(It.Is<string>(str => str == normalExcContainerFolder))).Returns(new string[]
                {
                    Path.Combine(normalExcContainerFolder, "SlitErr_exc.png")
                }); ;
            _ = fileWrapperMock.Setup(x => x.Exists(It.IsAny<string>())).Returns(true);

            trackerData.CopyGraphImages(normalExcContainerFolder, dataOutputPath, "TestPrefix");

            fileWrapperMock.Verify(x => x.Exists(It.Is<string>(str => str == copyPath)), Times.Once);
            //Call delete on file if it already exists, to allow new copy
            fileWrapperMock.Verify(x => x.Delete(It.Is<string>(str => str == copyPath)), Times.Once);
            fileWrapperMock.Verify(x => x.Copy(
                It.Is<string>(str => str == originalFile),
                It.Is<string>(str => str == copyPath)), Times.Once);
        }
    }
}
