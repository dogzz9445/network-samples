using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SettingNetwork
{
    public class NetworkSettingsController : Provider<NetworkSettings>
    {
        private string _contextFilePath;
        private string _contextFileName;
        public string ContextFullFileName { get => Path.Combine(_contextFilePath, _contextFileName); }

        private bool _isSaveContext;
        public bool IsSaveContext { get => _isSaveContext; set => _isSaveContext = value; }

        public NetworkSettingsController() : this(null) { }

        public NetworkSettingsController(
            bool? isSaveContext = null,
            string filepath = null,
            string filename = null) : base()
        {
            _isSaveContext = isSaveContext ?? true;
            _contextFilePath = filepath ?? "";
            _contextFileName = filename ?? "network_settings.json";

            PropertyChanged += (s, e) => Save();

            Load();
        }

        public override void Load()
        {
            _global = this;
            Context = ReadFileOrDefault<NetworkSettings>(ContextFullFileName);
            RaisePropertyChangedEvent();
        }

        public async void Save()
        {
            if (IsSaveContext)
            {
                //await WriteFileAsync(ContextFullFileName, Context);
            }
        }

        #region Json 파일 읽기 쓰기 기본 메서드
        public static T ReadFileOrDefault<T>(string filename) where T : new()
        {
            if (File.Exists(filename))
            {
                var json = File.ReadAllText(filename);
                if (!string.IsNullOrEmpty(json))
                {
                    return JsonConvert.DeserializeObject<T>(json,
                        new JsonSerializerSettings()
                        {
                            DefaultValueHandling = DefaultValueHandling.Populate,
                            NullValueHandling = NullValueHandling.Ignore
                        });
                }
            }
            return new T();
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
                using (var writer = new JsonTextWriter(file))
                {
                    string json = JsonConvert.SerializeObject(jsonObject, Formatting.Indented,
                        new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    await writer.WriteRawAsync(json);
                }
            }
        }
        #endregion
    }
}
