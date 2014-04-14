using System.IO;
using Newtonsoft.Json;

namespace ServerSideBot
{
    public class Config
    {
        public char CommandCharacter = '.';
        public char PrivateCharacter = '~';
        public string BotName = "Bot";
        public string BotChatColor = "255,255,255";


        public void Write(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static Config Read(string path)
        {
            return File.Exists(path) ? JsonConvert.DeserializeObject<Config>(File.ReadAllText(path)) 
                : new Config();
        }
    }
}
