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
using System.Net;
using Indieteur.VDFAPI;

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
        private VDFNode _localConfig;

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
            var localconfigs = new Dictionary<string, VDFNode>();
            foreach (var userdirpath in Directory.GetDirectories(userdatapath, "*", SearchOption.TopDirectoryOnly))
            {
                var userid = Path.GetFileName(userdirpath);
                var localconfig = new VDFData(File.ReadAllText(Path.Combine(userdirpath, "config", "localconfig.vdf")), false).Nodes[0];
                localconfigs.Add(userid, localconfig);
                var username = "Unknown";
                try { username = localconfig.Node("friends").Node(userid).Key("name"); } catch { }
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
            _localConfig = localconfigs[_userId];

            _steamUser = new SteamUser(_userId);
            _steamUser.UpdateAppsFromWeb();
            _steamUser.UpdateAppsFromSharedConfig();
            DataContext = _steamUser;

        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Steam app must be closed");
            _steamUser.Save();
        }
    }
}
