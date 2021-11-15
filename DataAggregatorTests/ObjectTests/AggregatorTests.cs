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
        private readonly Mock<FileWrapper> fileWrapperMock = new();
        private readonly Mock<DirectoryWrapper> directoryWrapperMock = new();
        private readonly string testSN = "139123";

        [OneTimeSetUp]
        public void MockReturnSetup()
        {
            //Block default init method
            _ = configMock.Setup(x => x.Init());

            _ = configMock.SetupGet(x => x.ImagerFixtureImagesPath).Returns(@"\\brownsharpefxtr\mfg\RectImages\");

            Aggregator.Init(configMock.Object, fileWrapperMock.Object, directoryWrapperMock.Object);
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


            Assert.IsTrue(Aggregator.TrackerHasValidData(testSN));
        }

        [Test]
        public void TestCheckNWRunFalse()
        {
            _ = directoryWrapperMock.Setup(
                x => x.GetDirectories(
                    It.Is<string>(str => str == @"\\brownsharpefxtr\mfg\RectImages\")
                    )
                ).Returns(new string[] { @"\\brownsharpefxtr\mfg\RectImages\SN139123" });

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
    }
}
