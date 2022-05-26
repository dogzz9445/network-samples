using SettingNetwork.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using NetCoreServer;
using System.IO;
using FireXR.Protobuf;
using Newtonsoft.Json;
using FireXR;

namespace wpf_netcore_tcp_server.ViewModel
{
    public class RestClientViewModel : BindableBase
    {
        #region Property
        private StringBuilder _outputLog = new StringBuilder();
        public string OutputLog { get => _outputLog.ToString(); }

        private string _targetURL = "/schedules/";
        public string TargetURL { get => _targetURL; set => SetProperty(ref _targetURL, value); }

        private string _targetFileURL = "/test.json";
        public string TargetFileURL { get => _targetFileURL; set => SetProperty(ref _targetFileURL, value); }

        private ScenarioDB _db;


        #endregion

        public RestClientViewModel()
        {
            _db = new ScenarioDB(HomeViewModel.Instance.NetworkManager);
        }


        public void AddLog(string log)
        {
            _outputLog.Clear();
            _outputLog.AppendLine(log);
            RaisePropertyChangedEvent("OutputLog");
        }

        #region Commands
        private DelegateCommand _sendGetRequestTest1;
        public DelegateCommand SendGetRequestTest1
        {
            get => _sendGetRequestTest1 ??= new DelegateCommand(async () =>
            {
                var result = await HomeViewModel.Instance.NetworkManager.GetRequestAPIAsync(TargetURL);
                AddLog(result);
            });
        }

        private DelegateCommand _sendGetRequestTest2;
        public DelegateCommand SendGetRequestTest2
        {
            get => _sendGetRequestTest2 ??= new DelegateCommand(async () =>
            {
                var result = await HomeViewModel.Instance.NetworkManager.GetRequestAPIAsync(@"/schedules/");
                AddLog(result);
            });
        }

        private DelegateCommand _sendGetRequestTest3;
        public DelegateCommand SendGetRequestTest3
        {
            get => _sendGetRequestTest3 ??= new DelegateCommand(() =>
            {
                _db.PullTransforms();
                if (_db.Transforms.Count == 0)
                {
                    return;
                }
                var transform = _db.Transforms.FirstOrDefault();
                AddLog(transform.Value.Name);
            });
        }

        private DelegateCommand _sendGetRequestTest4;
        public DelegateCommand SendGetRequestTest4
        {
            get => _sendGetRequestTest4 ??= new DelegateCommand(() =>
            {
                _db.PullAll();
            });
        }


        private DelegateCommand _sendGetRequestUpload;
        public DelegateCommand SendGetRequestUpload
        {
            get => _sendGetRequestUpload ??= new DelegateCommand(() =>
            {
                _db.UploadAll();
            });
        }


        private DelegateCommand _sendGetRequestDownload;
        public DelegateCommand SendGetRequestDownload
        {
            get => _sendGetRequestDownload ??= new DelegateCommand(() =>
            {
                _db.DownloadAll();
            });
        }

        private DelegateCommand _sendGetRequestFileTest1;
        public DelegateCommand SendGetRequestFileTest1
        {
            get => _sendGetRequestFileTest1 ??= new DelegateCommand(async () =>
            {
                var result = await HomeViewModel.Instance.NetworkManager.GetRequestFileAsync(TargetFileURL);
                AddLog(result.Status.ToString());
            });
        }

        private DelegateCommand _sendPostRequestFileTest1;
        public DelegateCommand SendPostRequestFileTest1
        {
            get => _sendPostRequestFileTest1 ??= new DelegateCommand(async () =>
            {
                Transform transform = new Transform();
                transform.ID = 2323;
                transform.Type = TransformType.Position;
                transform.Name = "asdasd";
                transform.Desc = "asdasdasd";
                var result = await HomeViewModel.Instance.NetworkManager.PostRequestAPIAsync("/unit/transform/", JsonConvert.SerializeObject(transform));
                if (result != null)
                { 
                   AddLog(result.ToString());
                }
            });
        }
        #endregion

    }
}
