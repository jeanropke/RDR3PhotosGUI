using System.Collections.Generic;
using System.IO;

namespace RDR3PhotosGUI.RDR3Photos
{
    public static class Settings
    {
        private static string[] settingsWarnUp = new string[] { "profile_folder" };

        public static Dictionary<string, string> SettingsDic = new Dictionary<string, string>();

        public static void LoadSettings()
        {
            //Check if the file exists
            if (!File.Exists("settings.ini")) {
                File.Create("settings.ini").Dispose();
            }
            else
            {
                //Read all lines and set the value
                foreach (string line in File.ReadAllLines("settings.ini"))
                {
                    string[] value = line.Split('=');
                    //Only add the first file value
                    if (!SettingsDic.ContainsKey(value[0]))
                        SettingsDic.Add(value[0], value[1]);
                }
            }

            //In case the file is empty, add the value on memory
            foreach (string key in settingsWarnUp)
            {
                if (!SettingsDic.ContainsKey(key))
                    SettingsDic.Add(key, "");
            }
        }

        public static void SaveSettings()
        {
            foreach (KeyValuePair<string, string> entry in SettingsDic)
            {
                File.WriteAllText("settings.ini", $"{entry.Key}={entry.Value}\r\n");
            }
        }
    }
}
