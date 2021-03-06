using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FireXR.Util
{
    public class JsonUtil
    {
        public static T ReadFileOrDefault<T>(string filename) where T : new()
        {
            if (!File.Exists(filename))
            {
                return new T();
            }
            bool valid = true;
            var json = File.ReadAllText(filename);
            if (string.IsNullOrEmpty(json))
            {
                return new T();
            }
            T item = JsonConvert.DeserializeObject<T>(json,
                new JsonSerializerSettings()
                {
                    DefaultValueHandling = DefaultValueHandling.Populate,
                    NullValueHandling = NullValueHandling.Ignore,
                    Error = (s, e) =>
                    {
                        e.ErrorContext.Handled = true;
                        valid = false;
                    }
                });
            return valid ? item : new T();
        }

        public static async Task WriteFileAsync<T>(string fullFileName, T jsonObject)
        {
            var directoryName = Path.GetDirectoryName(fullFileName);
            if (!string.IsNullOrEmpty(directoryName))
            {
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
            }
            using (var file = File.CreateText(fullFileName))
            {
                string json = JsonConvert.SerializeObject(jsonObject, Formatting.Indented,
                    new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Populate,
                    });

                file.Write(json);
            }
        }

        public static void WriteFile<T>(string fullFileName, T jsonObject)
        {
            var task = WriteFileAsync(fullFileName, jsonObject);
            task.Wait();
        }
    }
}
