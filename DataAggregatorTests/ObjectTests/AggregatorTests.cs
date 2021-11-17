using Moq;
using NUnit.Framework;
using AccusiteDataAggregator.Objects;

using System;
using System.Xml;

namespace DataAggregatorTests
{
    public class AggregatorTests
    {
        private readonly Mock<ADAConfig> configMock = new();
        private readonly Mock<DirectoryWrapper> directoryWrapperMock = new();
        private readonly string testSN = "139123";

        [OneTimeSetUp]
        public void MockReturnSetup()
        {
            //Block default init method
            _ = configMock.Setup(x => x.Init());

            _ = configMock.SetupGet(x => x.ImagerFixtureImagesPath).Returns(@"\\brownsharpefxtr\mfg\RectImages\");
            _ = configMock.SetupGet(x => x.KronosRectImagesPath).Returns(@"\\kronon\RectImages\");
            Aggregator.Init(configMock.Object, directoryWrapperMock.Object);
        }
        [SetUp]
        public void SetUp()
        {
            _ = directoryWrapperMock.Setup(
                x => x.GetDirectories(
                    It.Is<string>(str => str == @"\\brownsharpefxtr\mfg\RectImages\")
                    )
                ).Returns(new string[] { @"\\brownsharpefxtr\mfg\RectImages\SN139123_NW", @"\\brownsharpefxtr\mfg\RectImages\SN139123" });
            _ = directoryWrapperMock.Setup(
                x => x.GetDirectories(
                    It.Is<string>(str => str == @"\\kronon\RectImages\")
                    )
                ).Returns(new string[] { @"\\kronon\RectImages\SN139123" });
            //Set up valid NW run
            _ = directoryWrapperMock.Setup(
                x => x.GetDirectories(
                    It.Is<string>(str => str == @"\\brownsharpefxtr\mfg\RectImages\SN139123_NW")
                    )
                ).Returns(new string[] { @"\\brownsharpefxtr\mfg\RectImages\SN139123_NW\L0.35H0.95" });
            _ = directoryWrapperMock.Setup(
                x => x.GetFiles(
                    It.Is<string>(str => str == @"\\brownsharpefxtr\mfg\RectImages\SN139123_NW\L0.35H0.95")
                    )
                ).Returns(new string[] {
                    @"\\brownsharpefxtr\mfg\RectImages\SN139123_NW\L0.35H0.95\res_0exc.txt",
                    @"\\brownsharpefxtr\mfg\RectImages\SN139123_NW\L0.35H0.95\res_1exc.txt",
                    @"\\brownsharpefxtr\mfg\RectImages\SN139123_NW\L0.35H0.95\res_2exc.txt"
                });
            //Set up valid normal fixture run
            _ = directoryWrapperMock.Setup(
                x => x.GetDirectories(
                    It.Is<string>(str => str == @"\\brownsharpefxtr\mfg\RectImages\SN139123")
                    )
                ).Returns(new string[] { @"\\brownsharpefxtr\mfg\RectImages\SN139123\L0.35H0.95" });
            _ = directoryWrapperMock.Setup(
                x => x.GetFiles(
                    It.Is<string>(str => str == @"\\brownsharpefxtr\mfg\RectImages\SN139123\L0.35H0.95")
                    )
                ).Returns(new string[] {
                    @"\\brownsharpefxtr\mfg\RectImages\SN139123\L0.35H0.95\res_0exc.txt",
                    @"\\brownsharpefxtr\mfg\RectImages\SN139123\L0.35H0.95\res_1exc.txt",
                    @"\\brownsharpefxtr\mfg\RectImages\SN139123\L0.35H0.95\res_2exc.txt"
                });
            //Set up valid kronos normal run
            _ = directoryWrapperMock.Setup(
                x => x.GetDirectories(
                    It.Is<string>(str => str == @"\\kronon\RectImages\SN139123")
                    )
                ).Returns(new string[] { @"\\kronon\RectImages\SN139123\L0.35H0.95" });
            _ = directoryWrapperMock.Setup(
                x => x.GetFiles(
                    It.Is<string>(str => str == @"\\kronon\RectImages\SN139123\L0.35H0.95")
                    )
                ).Returns(new string[] {
                    @"\\kronon\RectImages\SN139123\L0.35H0.95\res_0exc.txt",
                    @"\\kronon\RectImages\SN139123\L0.35H0.95\res_1exc.txt",
                    @"\\kronon\RectImages\SN139123\L0.35H0.95\res_2exc.txt"
                });
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Aggregator.Reset();
        }

        [Test]
        public void TestInitialization()
        {
            Assert.IsInstanceOf<ADAConfig>(Aggregator.Config);
        }

        [Test]
        public void TestCheckValidDataRunTrue()
        {
            Assert.IsTrue(Aggregator.TrackerHasValidData(testSN));
        }

        //Test NW run conditions
        [Test]
        public void TestNWRunNotPresent()
        {
            _ = directoryWrapperMock.Setup(
                x => x.GetDirectories(
                    It.Is<string>(str => str == @"\\brownsharpefxtr\mfg\RectImages\")
                    )
                ).Returns(new string[] { @"\\brownsharpefxtr\mfg\RectImages\SN139123" });

            Assert.IsFalse(Aggregator.TrackerHasValidData(testSN));
        }
        [Test]
        public void TestNWRunNoValidDataFolder()
        {
            //Valid folder should not include appended date
            _ = directoryWrapperMock.Setup(
                x => x.GetDirectories(
                    It.Is<string>(str => str == @"\\brownsharpefxtr\mfg\RectImages\SN139123_NW")
                    )
                ).Returns(new string[] { @"\\brownsharpefxtr\mfg\RectImages\SN139123_NW\L0.35H0.95_2021-11-01" });
            Assert.IsFalse(Aggregator.TrackerHasValidData(testSN));
        }
        [Test]
        public void TestNWRunDataNotPresent()
        {
            _ = directoryWrapperMock.Setup(
                x => x.GetFiles(
                    It.Is<string>(str => str == @"\\brownsharpefxtr\mfg\RectImages\SN139123_NW\L0.35H0.95")
                    )
                ).Returns(new string[] {
                    @"\\brownsharpefxtr\mfg\RectImages\SN139123_NW\L0.35H0.95\res_0.txt",
                    @"\\brownsharpefxtr\mfg\RectImages\SN139123_NW\L0.35H0.95\res_1.txt",
                    @"\\brownsharpefxtr\mfg\RectImages\SN139123_NW\L0.35H0.95\res_2.txt"
                });

            Assert.IsFalse(Aggregator.TrackerHasValidData(testSN));
        }

        [Test]
        public void TestFixtureNormalRunFalse()
        {
            _ = directoryWrapperMock.Setup(
                x => x.GetDirectories(
                    It.Is<string>(str => str == @"\\brownsharpefxtr\mfg\RectImages\")
                    )
                ).Returns(new string[] { @"\\brownsharpefxtr\mfg\RectImages\SN139123_NW" });

            Assert.IsFalse(Aggregator.TrackerHasValidData(testSN));
        }
        [Test]
        public void TestNormalRunNoValidDataFolder()
        {
            //Valid folder should not include appended date
            _ = directoryWrapperMock.Setup(
                x => x.GetDirectories(
                    It.Is<string>(str => str == @"\\brownsharpefxtr\mfg\RectImages\SN139123")
                    )
                ).Returns(new string[] { @"\\brownsharpefxtr\mfg\RectImages\SN139123_NW\L0.35H0.95_2021-11-01" });
            Assert.IsFalse(Aggregator.TrackerHasValidData(testSN));
        }
        [Test]
        public void TestNormalRunDataNotPresent()
        {
            _ = directoryWrapperMock.Setup(
                x => x.GetFiles(
                    It.Is<string>(str => str == @"\\brownsharpefxtr\mfg\RectImages\SN139123\L0.35H0.95")
                    )
                ).Returns(new string[] {
                    @"\\brownsharpefxtr\mfg\RectImages\SN139123\L0.35H0.95\res_0.txt",
                    @"\\brownsharpefxtr\mfg\RectImages\SN139123\L0.35H0.95\res_1.txt",
                    @"\\brownsharpefxtr\mfg\RectImages\SN139123\L0.35H0.95\res_2.txt"
                });

            Assert.IsFalse(Aggregator.TrackerHasValidData(testSN));
        }

        [Test]
        public void TestKronosNormalRunFalse()
        {
            _ = directoryWrapperMock.Setup(
                x => x.GetDirectories(
                    It.Is<string>(str => str == @"\\brownsharpefxtr\mfg\RectImages\")
                    )
                ).Returns(new string[] { @"\\brownsharpefxtr\mfg\RectImages\SN139123_NW", @"\\brownsharpefxtr\mfg\RectImages\SN139123" });
            _ = directoryWrapperMock.Setup(
                x => x.GetDirectories(
                    It.Is<string>(str => str == @"\\kronon\RectImages\")
                    )
                ).Returns(new string[] { @"\\kronon\RectImages\SN139123_123456" });
            Assert.IsFalse(Aggregator.TrackerHasValidData(testSN));
        }
        [Test]
        public void TestKronosRunNoValidDataFolder()
        {
            //Valid folder should not include appended date
            _ = directoryWrapperMock.Setup(
                x => x.GetDirectories(
                    It.Is<string>(str => str == @"\\kronon\RectImages\SN139123")
                    )
                ).Returns(new string[] { @"\\kronon\RectImages\SN139123_NW\L0.35H0.95_2021-11-01" });
            Assert.IsFalse(Aggregator.TrackerHasValidData(testSN));
        }
        [Test]
        public void TestKronosRunDataNotPresent()
        {
            _ = directoryWrapperMock.Setup(
                x => x.GetFiles(
                    It.Is<string>(str => str == @"\\kronon\RectImages\SN139123\L0.35H0.95")
                    )
                ).Returns(new string[] {
                    @"\\kronon\RectImages\SN139123\L0.35H0.95\res_0.txt",
                    @"\\kronon\RectImages\SN139123\L0.35H0.95\res_1.txt",
                    @"\\kronon\RectImages\SN139123\L0.35H0.95\res_2.txt"
                });

            Assert.IsFalse(Aggregator.TrackerHasValidData(testSN));
        }
    }
}
