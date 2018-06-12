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

        public SteamUser(string folderName)
        {
            Id = DirNametoID64(folderName);
            Name = folderName;
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
