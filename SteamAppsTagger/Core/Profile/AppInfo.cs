using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MimiJson;

namespace SteamAppsTagger
{
    public class AppInfo
    {
        private static Dictionary<int, string> _steamApps;

        static AppInfo()
        {
            var request = (HttpWebRequest)WebRequest.Create("https://api.steampowered.com/ISteamApps/GetAppList/v2/");

            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";

            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            var json = JsonValue.Parse(responseString);
            _steamApps = new Dictionary<int, string>();
            foreach (var info in json["applist"]["apps"].Array)
                _steamApps.Add(info["appid"], info["name"]);
        }

        private static string GetAppName(int appId)
        {
            if (_steamApps.ContainsKey(appId))
                return _steamApps[appId];
            else
                return appId.ToString();
        }

        public string Name { get; private set; }
        public int Id { get; private set; }
        public Dictionary<string, bool> Tags { get; private set; } = new Dictionary<string, bool>();

        public AppInfo(JsonValue info)
        {
            Id = info["appid"];
            Name = GetAppName(Id);
        }

        public void AddTag(string tag, bool enabled)
        {
            if (!Tags.ContainsKey(tag))
                Tags.Add(tag, enabled);
        }

        public void SetTag(string tag, bool enabled)
        {
            if (Tags.ContainsKey(tag))
                Tags[tag] = enabled;
            else
                Tags.Add(tag, enabled);
        }
    }
}
