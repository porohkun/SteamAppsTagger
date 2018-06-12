using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using SteamAppsTagger.VDF;
using System.Net;

//VDF - valve data format
//http://cdn.akamai.steamstatic.com/steam/apps/457140/capsule_sm_120.jpg

namespace SteamAppsTagger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SteamUser _steamUser;
        private string _userId;
        private VdfDocument _localConfig;
        private VdfDocument _sharedConfig;

        public MainWindow()
        {
            InitializeComponent();
        }

        private bool CheckSteamProcess()
        {
            var allProcceses = Process.GetProcesses();
            var steamProcesses = allProcceses.Where(p => p.ProcessName.ToLower().Contains("steam") && p.Id != Process.GetCurrentProcess().Id).ToArray();
            return steamProcesses.Length == 0;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //check steam running
            //while (!CheckSteamProcess())
            //    MessageBox.Show("Steam app must be closed");

            //get steam users
            var userdatapath = Path.Combine(Settings.SteamInstallPath, "userdata");
            var users = new Dictionary<string, string>();
            var localconfigs = new Dictionary<string, VdfDocument>();
            foreach (var userdirpath in Directory.GetDirectories(userdatapath, "*", SearchOption.TopDirectoryOnly))
            {
                var userid = Path.GetFileName(userdirpath);
                var localconfig = VdfDocument.ParceFile(Path.Combine(userdirpath, "config", "localconfig.vdf"));
                localconfigs.Add(userid, localconfig);
                var username = "Unknown";
                try { username = localconfig["friends"][userid]["name"]; } catch { }
                users.Add(userid, username);
            }
            switch (users.Count)
            {
                case 0: MessageBox.Show("Steam users not found"); Close(); break;
                case 1: _userId = users.Keys.First(); break;
                default:
                    var suw = new SelectUserWindow(users);
                    suw.ShowDialog();
                    if (string.IsNullOrEmpty(suw.SelectedUser))
                        Close();
                    _userId = suw.SelectedUser;
                    break;
            }

            _steamUser = new SteamUser(_userId);
            _steamUser.UpdateAppsFromWeb();
            DataContext = _steamUser;

            //get steamapps
            _localConfig = localconfigs[_userId];
            _sharedConfig = VdfDocument.ParceFile(Path.Combine(Settings.SteamInstallPath, "userdata", _userId, "7", "remote", "sharedconfig.vdf"));

            var apps = _sharedConfig["Software"]["Valve"]["Steam"]["apps"].Object;
            foreach (var app in apps)
            {
                var appId = app.Key;
                var steamApp = _steamUser.Apps.FirstOrDefault(a => a.Id.ToString() == appId);
                if (steamApp != null)
                {
                    if (app.Value.Object.ContainsKey("tags"))
                        foreach (var tag in app.Value["tags"].Object.Select(t => t.Value.Text))
                            steamApp.SetTag(tag, true);
                }
            }
            _steamUser.UpdateTags();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Steam app must be closed");
        }
    }
}
