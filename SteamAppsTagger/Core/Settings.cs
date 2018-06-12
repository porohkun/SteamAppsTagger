using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace SteamAppsTagger
{
    [Serializable]
    public class WindowStateSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string strPropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyName));
        }

        private double width = 640;
        private double height = 480;
        private WindowState state;

        public event EventHandler WidthChanged;
        public event EventHandler HeightChanged;
        public event EventHandler StateChanged;

        [XmlAttribute]
        public double Width
        {
            get { return width; }
            set
            {
                if (value != width)
                {
                    width = value;
                    NotifyPropertyChanged(nameof(Width));
                }
            }
        }

        [XmlAttribute]
        public double Height
        {
            get { return height; }
            set
            {
                if (value != height)
                {
                    height = value;
                    NotifyPropertyChanged(nameof(Height));
                }
            }
        }

        [XmlAttribute]
        public WindowState State
        {
            get { return state; }
            set
            {
                if (value != state)
                {
                    state = value;
                    NotifyPropertyChanged(nameof(State));
                }
            }
        }
    }

    [Serializable]
    public class Settings
    {
        public static string AppDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Steam Apps Tagger");
        public static string SettingsPath => Path.Combine(AppDataPath, "settings.xml");
        public static string SteamInstallPath
        {
            get
            {
                if (string.IsNullOrEmpty(_steamInstallPath))
                    using (RegistryKey parentKey = Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\Valve\Steam"))
                        _steamInstallPath = parentKey.GetValue("InstallPath").ToString();
                return _steamInstallPath;
            }
        }
        public static Settings Instance { get; set; } = Load();

        private static string _steamInstallPath = null;

        public WindowStateSettings MainWindow { get; set; }
        public string SteamWebApiAuthKey { get; } = "4104BE91E7FCCDE0C913D86E9E5509B9";

        public Settings()
        {
            MainWindow = new WindowStateSettings() { Width = 640, Height = 480 };
            MainWindow.PropertyChanged += SaveByPropertyChanged;
        }

        private static Settings Load()
        {
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(Settings));
            if (File.Exists(SettingsPath))
            {
                using (var stream = File.OpenRead(SettingsPath))
                {
                    try
                    {
                        return (Settings)serializer.Deserialize(stream);
                    }
                    catch { }
                }
            }
            return new Settings();
        }

        private void SaveByPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Save();
        }

        public static void Save()
        {
            if (!Directory.Exists(AppDataPath))
                Directory.CreateDirectory(AppDataPath);
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(Settings));
            try
            {
                using (var stream = File.Open(SettingsPath, FileMode.Create))
                {
                    serializer.Serialize(stream, Instance);
                }
            }
            catch { }
        }
    }
}
