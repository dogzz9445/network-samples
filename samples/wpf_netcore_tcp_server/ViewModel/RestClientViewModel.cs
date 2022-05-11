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

        #endregion

        public RestClientViewModel()
        {
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
                AddLog(result.Body);
            });
        }

        private DelegateCommand _sendGetRequestTest2;
        public DelegateCommand SendGetRequestTest2
        {
            get => _sendGetRequestTest2 ??= new DelegateCommand(async () =>
            {
                var result = await HomeViewModel.Instance.NetworkManager.GetRequestAPIAsync(@"/schedules/");
                AddLog(result.Body);
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
        #endregion

    }
}
