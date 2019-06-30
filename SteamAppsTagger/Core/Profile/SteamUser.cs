using Indieteur.VDFAPI;
using MimiJson;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SteamAppsTagger
{
    public class SteamUser
    {
        public string Name { get; private set; }
        public Int64 Id { get; private set; }

        public ObservableCollection<string> Tags { get; private set; } = new ObservableCollection<string>();
        public ObservableCollection<AppInfo> Apps { get; private set; } = new ObservableCollection<AppInfo>();

        private string _sharedConfigPath;
        private VDFData _sharedConfig;

        public SteamUser(string folderName)
        {
            Id = DirNametoID64(folderName);
            Name = folderName;
            _sharedConfigPath = Path.Combine(Settings.SteamInstallPath, "userdata", Name, "7", "remote", "sharedconfig.vdf");
        }

        public void UpdateAppsFromWeb()
        {
            var inputJson = $"{{%22steamid%22:{Id},%22include_appinfo%22:false,%22include_played_free_games%22:false}}";
            var request = (HttpWebRequest)WebRequest.Create($"https://api.steampowered.com/IPlayerService/GetOwnedGames/v1/?key={Settings.Instance.SteamWebApiAuthKey}&input_json={inputJson}");

            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";

            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            var json = JsonValue.Parse(responseString);
            foreach (var info in json["response"]["games"].Array)
                Apps.Add(new AppInfo(info));

            UpdateTags();
        }

        public void UpdateAppsFromSharedConfig()
        {
            _sharedConfig = new VDFData(File.ReadAllText(_sharedConfigPath), false);

            var apps = _sharedConfig.Nodes[0].Node("Software").Node("Valve").Node("Steam").Node("apps").Nodes;
            foreach (var app in apps)
            {
                var appId = app.Name;
                var steamApp = Apps.FirstOrDefault(a => a.Id.ToString() == appId);
                if (steamApp == null)
                {
                    int id;
                    if (int.TryParse(app.Name, out id))
                    {
                        steamApp = new AppInfo(id);
                        Apps.Add(steamApp);
                    }
                }
                if (steamApp != null)
                    if (app.ContainsNode("tags"))
                        foreach (var tag in app.Node("tags").Keys.Select(k => k.Value))
                            steamApp.SetTag(tag, true);
            }
            UpdateTags();
        }

        public void Save()
        {
            var apps = _sharedConfig.Nodes[0].Node("Software").Node("Valve").Node("Steam").Node("apps");
            foreach (var app in Apps)
            {
                var vdfApp = apps.Node(app.Id.ToString());
                if (vdfApp == null)
                {
                    vdfApp = new VDFNode(app.Id.ToString(), _sharedConfig, apps);
                    apps.Nodes.Add(vdfApp);
                }
                var tagsNode = vdfApp.Node("tags");
                if (tagsNode != null)
                    foreach (var key in tagsNode.Keys.Select(k => k.Name).ToArray())
                        tagsNode.Keys.CleanRemoveKey(key);
                int keyIndex = 0;
                foreach (var tag in app.Tags.Where(p => p.Value).Select(p => p.Key))
                {
                    if (tagsNode == null)
                        tagsNode = new VDFNode("tags", _sharedConfig, vdfApp);
                    tagsNode.Keys.Add(new VDFKey(keyIndex.ToString(), tag, tagsNode));
                    keyIndex++;
                }
            }
            _sharedConfig.SaveToFile(_sharedConfigPath, true);
        }

        public void UpdateTags()
        {
            var tags = new List<string>();
            foreach (var app in Apps)
                tags.AddRange(app.Tags.Keys);
            tags = tags.Distinct().OrderBy(t => t).ToList();

            foreach (var app in Apps)
                foreach (var tag in tags)
                    app.AddTag(tag, false);

            var delTags = Tags.Except(tags).ToArray();
            foreach (var tag in delTags)
                Tags.Remove(tag);

            foreach (var tag in tags)
                if (!Tags.Contains(tag))
                    Tags.Add(tag);

            Apps.Sort((a1, a2) =>
            {
                return a1.Name.CompareTo(a2.Name);
            });
        }

        public static Int64 DirNametoID64(string cId)
        {
            Int64 res;
            if (Int64.TryParse(cId, out res))
            {
                return (res + 0x0110000100000000);
            }
            return 0;
        }
    }
}
