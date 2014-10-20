using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using AmadeusW.Ambilight;
using AmadeusW.Ambilight.Helpers;

namespace AmadeusW.Ambilight.DataClasses
{
    [Serializable]
    public class Preset : INotifyPropertyChanged
    {
        #region GUI bound fields

        private string m_Name;
        private string m_Trigger;
        private Helpers.HexColor m_Compensation;
        private int m_MaxRed;
        private int m_MaxGreen;
        private int m_MaxBlue;
        private Helpers.HexColor m_MinColor;
        private int m_DarkEnhanceDuration;
        private Helpers.HexColor m_MaxColor;
        private int m_FlashEnhanceDuration;
        private float m_MonotoneEnhance;
        private int m_Framerate;
        private int m_AveragingParam;
        private ScreenshotLogicCPP.Sector[] m_Sectors;
        private int m_VerticalSectors;
        private int m_TopSectors;
        private int m_BottomSectors;
        private int m_VerticalSectorWidth;
        private int m_HorizontalSectorHeight;

        public string Name { get { return m_Name; } set { m_Name = value; this.sendPropertyChanged("Name"); } }
        public string Trigger { get { return m_Trigger; } set { m_Trigger = value; this.sendPropertyChanged("Trigger"); } }
        public Helpers.HexColor Compensation { get { return m_Compensation; } set { m_Compensation = value; this.sendPropertyChanged("Compensation"); } }
        public int MaxRed { get { return m_MaxRed; } set { m_MaxRed = value; this.sendPropertyChanged("MaxRed"); } }
        public int MaxGreen { get { return m_MaxGreen; } set { m_MaxGreen = value; this.sendPropertyChanged("MaxGreen"); } }
        public int MaxBlue { get { return m_MaxBlue; } set { m_MaxBlue = value; this.sendPropertyChanged("MaxBlue"); } }
        public Helpers.HexColor MinColor { get { return m_MinColor; } set { m_MinColor = value; this.sendPropertyChanged("MinColor"); } }
        public int DarkEnhanceDuration { get { return m_DarkEnhanceDuration; } set { m_DarkEnhanceDuration = value; this.sendPropertyChanged("DarkEnhanceDuration"); } }
        public Helpers.HexColor MaxColor { get { return m_MaxColor; } set { m_MaxColor = value; this.sendPropertyChanged("MaxColor"); } }
        public int FlashEnhanceDuration { get { return m_FlashEnhanceDuration; } set { m_FlashEnhanceDuration = value; this.sendPropertyChanged("FlashEnhanceDuration"); } }
        public float MonotoneEnhance { get { return m_MonotoneEnhance; } set { m_MonotoneEnhance = value; this.sendPropertyChanged("MonotoneEnhance"); } }
        public int Framerate { get { return m_Framerate; } set { m_Framerate = value; this.sendPropertyChanged("Framerate"); } }
        public int AveragingParam { get { return m_AveragingParam; } set { m_AveragingParam = value; this.sendPropertyChanged("AveragingParam"); } }
        public ScreenshotLogicCPP.Sector[] Sectors { get { return m_Sectors; } set { m_Sectors = value; this.sendPropertyChanged("Sectors"); } }
        public int VerticalSectors { get { return m_VerticalSectors; } set { m_VerticalSectors = value; this.sendPropertyChanged("VerticalSectors"); } }
        public int TopSectors { get { return m_TopSectors; } set { m_TopSectors = value; this.sendPropertyChanged("TopSectors"); } }
        public int BottomSectors { get { return m_BottomSectors; } set { m_BottomSectors = value; this.sendPropertyChanged("BottomSectors"); } }
        public int VerticalSectorWidth { get { return m_VerticalSectorWidth; } set { m_VerticalSectorWidth = value; this.sendPropertyChanged("VerticalSectorWidth"); } }
        public int HorizontalSectorHeight { get { return m_HorizontalSectorHeight; } set { m_HorizontalSectorHeight = value; this.sendPropertyChanged("HorizontalSectorHeight"); } }

        #endregion

        private string _nameFromFile;

        #region Constructor

        /// <summary>
        /// New preset constructor
        /// </summary>
        public Preset()
        {
            m_Name = "New preset";
            m_Trigger = "always on";
            m_Compensation = new Helpers.HexColor("ffffff");
            m_MaxRed = 100;
            m_MaxGreen = 80;
            m_MaxBlue = 40;
            m_MinColor = new Helpers.HexColor("000000");
            m_DarkEnhanceDuration = 200;
            m_MaxColor = new Helpers.HexColor("ffffff");
            m_FlashEnhanceDuration = 20;
            m_MonotoneEnhance = 2;
            m_Framerate = 18;
            m_AveragingParam = 8;
            m_Sectors = new ScreenshotLogicCPP.Sector[Config.numberOfLeds];
            createListOfSectors(); // Initialize the list
            m_VerticalSectors = 6;
            m_TopSectors = 7;
            m_BottomSectors = 6;
            m_VerticalSectorWidth = 300;
            m_HorizontalSectorHeight = 300;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="original">Another preset to copy</param>
        public Preset(Preset original)
        {
            m_Name = original.Name;
            m_Trigger = original.Trigger;
            m_Compensation = new Helpers.HexColor(original.Compensation.HexValue);
            m_MaxRed = original.MaxRed;
            m_MaxGreen = original.MaxGreen;
            m_MaxBlue = original.MaxBlue;
            m_MinColor = new Helpers.HexColor(original.MinColor.HexValue);
            m_DarkEnhanceDuration = original.DarkEnhanceDuration;
            m_MaxColor = new Helpers.HexColor(original.MaxColor.HexValue);
            m_FlashEnhanceDuration = original.FlashEnhanceDuration;
            m_MonotoneEnhance = original.MonotoneEnhance;
            m_Framerate = original.Framerate;
            m_AveragingParam = original.AveragingParam;
            m_Sectors = original.Sectors; // TODO: make sure they are taken by value
            m_VerticalSectors = original.VerticalSectors;
            m_TopSectors = original.TopSectors;
            m_BottomSectors = original.BottomSectors;
            m_VerticalSectorWidth = original.VerticalSectorWidth;
            m_HorizontalSectorHeight = original.HorizontalSectorHeight;
        }

        #endregion

        #region INotifyPropertyChanged members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Private methods

        private void sendPropertyChanged(string property)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        private void createListOfSectors()
        {
            for (int i = 0; i < Config.numberOfSectors; i++)
            {
                m_Sectors[i] = new ScreenshotLogicCPP.Sector(0, 0, 0, 0, i);
            }
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region Serialization and deserialization

        private const string EXTENSION = ".xml";

        private static string GetStorageLocation()
        {
            string rootDirectory = Environment.GetEnvironmentVariable("LocalAppData");
            string localDirectory = @"Aurora\presets\";
            string targetLocation = Path.Combine(rootDirectory, localDirectory);
            if (!Directory.Exists(targetLocation))
            {
                Directory.CreateDirectory(targetLocation);
            }
            return targetLocation;
        }

        internal void Save()
        {
            try
            {
                // Insert code to set properties and fields of the object.
                XmlSerializer mySerializer = new XmlSerializer(typeof(Preset));
                // To write to a file, create a StreamWriter object.
                StreamWriter myWriter = new StreamWriter(GetStorageLocation() + Name + EXTENSION);
                mySerializer.Serialize(myWriter, this);
                myWriter.Close();
            }
            catch
            {
                // Don't continue
                return;
            }
            // Now that we've saved the preset, see if it was loaded from a different filename.
            // If that's true, it means that this preset got renamed. Remove old file.
            if (_nameFromFile != Name)
            {
                Remove(checkOriginalName: true);
            }
        }

        internal void Remove(bool checkOriginalName = false)
        {
            var targetPath = GetStorageLocation()
                            + (checkOriginalName ? _nameFromFile : Name)
                            + EXTENSION;
            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }
        }

        internal static IEnumerable<Preset> LoadPresets()
        {
            var files = Directory.EnumerateFiles(GetStorageLocation());
            foreach (var file in files)
            {
                if (file.EndsWith(EXTENSION))
                {
                    Preset deserializedPreset;
                    try
                    {
                        XmlSerializer mySerializer = new XmlSerializer(typeof(Preset));
                        // To read the file, create a FileStream.
                        FileStream myFileStream = new FileStream(file, FileMode.Open);
                        // Call the Deserialize method and cast to the object type.
                        deserializedPreset = (Preset)mySerializer.Deserialize(myFileStream);
                        // This bit will be useful when user renames the preset.
                        deserializedPreset._nameFromFile = deserializedPreset.Name;
                    }
                    catch
                    {
                        // Try another file
                        continue;
                    }

                    // Add it to the collection
                    yield return deserializedPreset;
                }
            }
        }

        #endregion
    }
}
