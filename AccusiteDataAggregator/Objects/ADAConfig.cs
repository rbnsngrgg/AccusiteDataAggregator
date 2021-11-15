using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AccusiteDataAggregator.Objects
{
    public class ADAConfig
    {
        public virtual string FileName { get; } = "ADAConfig.xml";
        public virtual string FilePath { get; private set; }
        public virtual string ImagerFixtureImagesPath { get; private set; }
        public virtual string KronosRectImagesPath { get; private set; }
        public virtual string OutputFolderPath { get; private set; }

        private readonly IFileWrapper fileWrapper;
        private readonly IDirectoryWrapper directoryWrapper;

        public ADAConfig()
        {
            fileWrapper = new FileWrapper();
            directoryWrapper = new DirectoryWrapper();
        }

        public ADAConfig(IFileWrapper fileWrapper, IDirectoryWrapper directoryWrapper)
        {
            this.fileWrapper = fileWrapper;
            this.directoryWrapper = directoryWrapper;
        }

        public virtual void Init()
        {
            FilePath = Path.Combine(directoryWrapper.GetCurrentDirectory(), FileName);
            if (!fileWrapper.Exists(FilePath))
            {
                CreateConfig();
            }
            LoadConfig();
        }

        private void CreateConfig()
        {
            MemoryStream memStream = new();
            XmlWriter writer = XmlWriter.Create(memStream);

            writer.WriteStartDocument();
            writer.WriteStartElement("Aggregator");

            writer.WriteAttributeString("ImagerFixtureImagesPath", @"\\brownsharpefxtr\mfg\RectImages\");
            writer.WriteAttributeString("KronosRectImagesPath", @"\\kronon\RectImages\");
            writer.WriteAttributeString("OutputFolderPath", @"\\castor\Production\Manufacturing\Accusite\DataAggregation\");


            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();

            memStream.Position = 0;
            using StreamReader streamReader = new(memStream);
            string xml = streamReader.ReadToEnd();
            fileWrapper.WriteAllText(FilePath, xml);
        }

        private void LoadConfig()
        {
            XmlDocument config = new();
            config.LoadXml(string.Join(' ', fileWrapper.ReadAllText(FilePath)));
            XmlNodeList elements = config.GetElementsByTagName("Aggregator");
            if (elements != null)
            {
                try
                {
                    ImagerFixtureImagesPath = elements[0].Attributes.GetNamedItem("ImagerFixtureImagesPath").Value;
                    KronosRectImagesPath = elements[0].Attributes.GetNamedItem("KronosRectImagesPath").Value;
                    OutputFolderPath = elements[0].Attributes.GetNamedItem("OutputFolderPath").Value;
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"ADAConfig.LoadConfig: {ex.Message}");
                }
            }
        }
    }
}
