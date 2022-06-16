using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetCoreServer;
using Newtonsoft.Json;
using System.IO;
using SettingNetwork.Util;
using Newtonsoft.Json.Linq;
using FireXR.Protobuf;
using System.Configuration;

namespace SettingNetwork
{
    public class NetworkModule : Consumer<NetworkSettingsController, NetworkSettings>
    {
        public event EventHandler<Message> MessageReceived;
        public event EventHandler<Packet> PacketReceived;
        public event EventHandler<NetworkError> ErrorReceived;

        private readonly Dictionary<int, BaseUdpClient> _udpClients = new Dictionary<int, BaseUdpClient>();
        private readonly Dictionary<int, BaseTcpClient> _tcpClients = new Dictionary<int, BaseTcpClient>();
        private readonly Dictionary<int, BaseFileTcpClient> _fileTcpClients = new Dictionary<int, BaseFileTcpClient>();

        private NetworkSettings _settings;
        private BaseTcpServer _tcpServer;
        private BaseFileTcpServer _fileServer;
        private BaseUdpServer _udpServer;
        private HttpServer _httpServer;
        private HttpClientEx _httpClientEx;

        private string _apiURL;
        public string APIURL
        { 
            get
            {
                if (_settings == null)
                {
                    return null;
                }
                return _apiURL ??= PathUtil.Combine($"{_settings.ContentDelivery.Protocol}://{_settings.ContentDelivery.Address}:{_settings.ContentDelivery.Port}", _settings.ContentDelivery.ApiRoot);
            }
        }

        private string _contentURL;
        public string ContentURL
        {
            get
            {
                if (_settings == null)
                {
                    return null;
                }
                return _contentURL ??= PathUtil.Combine($"{_settings.ContentDelivery.Protocol}://{_settings.ContentDelivery.Address}:{_settings.ContentDelivery.Port}", _settings.ContentDelivery.ContentRoot);
            }
        }

        private string _svcCredentials;
        public string SvcCredentials
        {
            get
            {

                if (_settings == null)
                {
                    return null;
                }
                return _svcCredentials ??= Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes($"{_settings.ContentDelivery.Username}:{_settings.ContentDelivery.Password}"));
            }
        }

        private int _timeoutTcpSend = 30;

        public NetworkModule() : base()
        {
            Initialize();
        }

        public void ClearSessions()
        {
            foreach (var tcpClient in _tcpClients)
            {
                Log($"TCP Clinet Stopped... {tcpClient.Key}");
                tcpClient.Value.MessageReceived -= OnMessageReceived;
                tcpClient.Value.DisconnectAndStop();
                tcpClient.Value.Dispose();
            }
            _tcpClients.Clear();

            if (_tcpServer != null)
            {
                _tcpServer.MessageReceived -= OnMessageReceived;
                _tcpServer.Stop();
                Log("TCP Server Stopped...");
                _tcpServer.Dispose();
                _tcpServer = null;
            }
            if (_udpServer != null)
            {
                _udpServer.Stop();
                _udpServer.MessageReceived -= OnMessageReceived;
                Log("UDP Server Stopped...");
                _udpServer.Dispose();
                _udpServer = null;
            }
            if (_httpServer != null)
            {
                _httpServer.Stop();
                Log("HTTP Server Stopped...");
                _httpServer.Dispose();
                _httpServer = null;
            }
            if (_fileServer != null)
            {
                _fileServer.Stop();
                Log("File Server Stopped...");
                _fileServer.Dispose();
                _fileServer = null;
            }
        }

        public void Initialize()
        {
            _settings = NetworkSettingsController.Global.Context;
            if (_settings == null)
            {
                throw new ArgumentException("settings cannot be null");
            }
            int hostId = _settings.HostId ?? 0;
            ComputerInfo hostComputerInfo = null;
            foreach (var setting in _settings.ComputerInfos)
            {
                if (setting.Id == hostId)
                {
                    hostComputerInfo = setting;
                    break;
                }
            }
            if (_settings.ContentDelivery.UseContentDelivery == true)
            {
                _httpClientEx = new HttpClientEx(_settings.ContentDelivery.Address, _settings.ContentDelivery.Port ?? 8000); 
            }
            if (hostComputerInfo == null)
            {
                Log("Cannot initialize NetworkManager, there isn't any setting about host computer");
                return;
            }
            if (hostComputerInfo.UseTcpServer ?? false)
            {
                _tcpServer = new BaseTcpServer(IPAddress.Any, hostComputerInfo.TcpPort ?? 0);
                _tcpServer.Start();
                Log("TCP Server Started...");
                _tcpServer.MessageReceived += OnMessageReceived;
            }
            if (hostComputerInfo.UseUdpServer ?? false)
            {
                _udpServer = new BaseUdpServer(IPAddress.Any, hostComputerInfo.UdpPort ?? 0);
                _udpServer.Start();
                Log("UDP Server Started...");
            }
            if (hostComputerInfo.UseHttpServer ?? false)
            {
                _httpServer = new HttpServer(IPAddress.Any, hostComputerInfo.HttpPort ?? 0);
                _httpServer.Start();
                Log("HTTP Server Started...");
            }
            if (hostComputerInfo.UseFileTcpServer ?? false)
            {
                _fileServer = new BaseFileTcpServer(IPAddress.Any, hostComputerInfo.FileTcpPort ?? 0,
                                                    _settings.IsSaveFileAbsolutePath ?? false, _settings.FileSavedDirectory);
                _fileServer.Start();
                Log("File Server Started...");
            }

            _apiURL = PathUtil.Combine($"{_settings.ContentDelivery.Protocol}://{_settings.ContentDelivery.Address}:{_settings.ContentDelivery.Port}", _settings.ContentDelivery.ApiRoot);
            _contentURL ??= PathUtil.Combine($"{_settings.ContentDelivery.Protocol}://{_settings.ContentDelivery.Address}:{_settings.ContentDelivery.Port}", _settings.ContentDelivery.ContentRoot);
            _svcCredentials ??= Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes($"{_settings.ContentDelivery.Username}:{_settings.ContentDelivery.Password}"));
        }

        public void RefreshSessions()
        {
            ClearSessions();
            Initialize();
        }

        #region TCP UDP 통신
        public async void SendTCP(int destinationId, string message)
        {
            await Task.Yield();
            if (_settings == null)
            {
                return;
            }
            if (!_tcpClients.ContainsKey(destinationId))
            {
                ComputerInfo destinationComputerInfo = null;
                foreach (var computerInfo in _settings.ComputerInfos)
                {
                    if (computerInfo.Id == destinationId)
                    {
                        destinationComputerInfo = computerInfo;
                        break;
                    }
                }
                if (destinationComputerInfo == null)
                {
                    return;
                }
                var ipAddress = destinationComputerInfo.IpAddress;
                var port = destinationComputerInfo.TcpPort;
                var tcpClient = new BaseTcpClient(ipAddress, port ?? 5000);
                tcpClient.MessageReceived += OnMessageReceived;
                tcpClient.ConnectAsync();
                Log($"Create TCP client at ip [{ipAddress}] port [{port}]");
                _tcpClients.Add(destinationId, tcpClient);
            }

            if (_tcpClients[destinationId] == null)
            {
                return;
            }

            int timeTrySend = 0;
            while (!_tcpClients[destinationId].IsConnected)
            {
                if (timeTrySend > _timeoutTcpSend)
                {
                    return;
                }
                Thread.Sleep(1000);
                timeTrySend += 1;
            }

            byte[] bufMessage = Encoding.UTF8.GetBytes(message);
            int length = bufMessage.Length;
            byte[] buffer = new byte[4 + length];
            byte[] bufLength = BitConverter.GetBytes(length);
            Array.Copy(bufLength, 0, buffer, 0, 4);
            Array.Copy(bufMessage, 0, buffer, 4, length);
            _tcpClients[destinationId].SendAsync(buffer);
            Log($"Sended to [{destinationId}] message [{message}]");
        }

        public void SendUDP(int destinationId, string message)
        {
            if (_settings == null)
            {
                return;
            }
            // TODO:
        }

        public void Send(int destinationId, string data, ProtocolType protocolType = ProtocolType.Tcp)
        {
            Message message = new Message();
            message.Type = MessageType.None;
            message.Data = data;
            Send(destinationId, message, protocolType);
        }

        public void Send(int destinationId, Message message, ProtocolType protocolType = ProtocolType.Tcp)
        {
            message.Type = MessageType.None;

            switch (protocolType)
            {
                case ProtocolType.Tcp:
                    SendTCP(destinationId, JsonConvert.SerializeObject(message));
                    break;
                case ProtocolType.Udp:
                    SendUDP(destinationId, JsonConvert.SerializeObject(message));
                    break;
                default:
                    break;
            }
        }

        public void Send<T>(int destinationId, T packet, ProtocolType protocolType = ProtocolType.Tcp) where T : Packet
        {
            packet.From = _settings.HostId ?? 0;
            packet.To = destinationId;

            Message message = new Message();
            message.Type = MessageType.Packet;
            message.Data = JsonConvert.SerializeObject(packet);

            switch (protocolType)
            {
                case ProtocolType.Tcp:
                    SendTCP(destinationId, JsonConvert.SerializeObject(message));
                    break;
                case ProtocolType.Udp:
                    SendUDP(destinationId, JsonConvert.SerializeObject(message));
                    break;
                default:
                    break;
            }
        }

        public void SendFile(int destinationId, string filename)
        {
            if (!_fileTcpClients.ContainsKey(destinationId))
            {
                ComputerInfo destinationComputerInfo = null;
                foreach (var computerInfo in _settings.ComputerInfos)
                {
                    if (computerInfo.Id == destinationId)
                    {
                        destinationComputerInfo = computerInfo;
                        break;
                    }
                }
                if (destinationComputerInfo == null)
                {
                    return;
                }
                var ipAddress = destinationComputerInfo.IpAddress;
                var port = destinationComputerInfo.FileTcpPort;
                var fileTcpClient = new BaseFileTcpClient(ipAddress, port ?? 5002);
                _fileTcpClients.Add(destinationId, fileTcpClient);
            }
            if (_fileTcpClients[destinationId] == null)
            {
                return;
            }
            _fileTcpClients[destinationId].SendFile(filename);
        }
        #endregion

        #region Http 통신
        //public HttpResponse GetRequestAPI(string url)
        //{
        //    if (_httpClientEx == null)
        //    {
        //        return null;
        //    }
        //    if (_settings.ContentDelivery.UseContentDelivery == false)
        //    {
        //        return null;
        //    }
        //    var fullUrl = PathUtil.Combine(_settings.ContentDelivery.ApiRoot, url);
        //    var task = _httpClientEx.SendGetRequest(fullUrl);
        //        task.Wait();
        //    var response = task.Result;
        //    if (response.Status >= 300)
        //    {
        //        return null;
        //    }
        //    return response;
        //}

        public HttpResponse GetRequestFile(string url)
        {
            if (_httpClientEx == null)
            {
                return null;
            }
            if (_settings.ContentDelivery.UseContentDelivery == false)
            {
                return null;
            }
            var fileName = Path.GetFileName(url);
            var fullUrl = PathUtil.Combine(_settings.ContentDelivery.ContentRoot, url);
            var task = _httpClientEx.SendGetRequest(fullUrl);
            task.Wait();
            var response = task.Result;

            Log($"FileDonwload: {fileName} from {fullUrl}");

            var fullFilePath = Path.GetFullPath(PathUtil.Combine(_settings.ContentDelivery.LocalFileRoot, fileName));
            using (var fileStream = new FileStream(fullFilePath, FileMode.Create, FileAccess.Write))
            {
                fileStream.Write(response.BodyBytes, 0, (int)response.BodyLength);
            }
            return response;
        }

        public async Task<HttpResponse> GetRequestFileAsync(string url)
        {
            if (_httpClientEx == null)
            {
                return null;
            }
            if (_settings.ContentDelivery.UseContentDelivery == false)
            {
                return null;
            }
            var fileName = Path.GetFileName(url);
            var fullUrl = PathUtil.Combine(_settings.ContentDelivery.ContentRoot, url);
            var response = await _httpClientEx.SendGetRequest(fullUrl);

            Log($"FileDonwload: {fileName} from {fullUrl}");

            var fullFilePath = Path.GetFullPath(PathUtil.Combine(_settings.ContentDelivery.LocalFileRoot, fileName));
            using (var fileStream = new FileStream(fullFilePath, FileMode.Create, FileAccess.Write))
            {
                fileStream.Write(response.BodyBytes, 0, (int)response.BodyLength);
            }

            return response;
        }

        // TODO:
        public async Task<HttpResponse> PostRequestFileAsync(string url, string filename)
        {

            if (_httpClientEx == null)
            {
                return null;
            }
            if (_settings.ContentDelivery.UseContentDelivery == false)
            {
                return null;
            }
            var fileName = Path.GetFileName(url);
            var fullUrl = PathUtil.Combine(_settings.ContentDelivery.ContentRoot, url);
            var response = await _httpClientEx.SendGetRequest(fullUrl);

            Log($"FileDonwload: {fileName} from {fullUrl}");

            var fullFilePath = Path.GetFullPath(PathUtil.Combine(_settings.ContentDelivery.LocalFileRoot, fileName));
            using (var fileStream = new FileStream(fullFilePath, FileMode.Create, FileAccess.Write))
            {
                fileStream.Write(response.BodyBytes, 0, (int)response.BodyLength);
            }

            return response;
        }

        //public async Task<HttpResponse> PostRequestAPIAsync(string url, string content)
        //{
        //    if (_httpClientEx == null)
        //    {
        //        Log("Error: UseContentDelivery is false");
        //        return null;
        //    }
        //    if (_settings.ContentDelivery.UseContentDelivery == false)
        //    {
        //        return null;
        //    }
        //    var fullUrl = PathUtil.Combine(_settings.ContentDelivery.ApiRoot, url);
        //    var response = await _httpClientEx.SendPostRequest(fullUrl, content);
        //    return response;
        //}

        //private string _authToken;
        //public string AuthToken 
        //{
        //    get
        //    {
        //        if (_authToken == null)
        //        {
        //            User test = new User() { username="test", password="test" };
        //            string json = JsonConvert.SerializeObject(test);
        //            var fullUrl = PathUtil.Combine(_settings.ContentDelivery.ApiRoot, "/api-token-auth/");
        //            HttpRequest request = new HttpRequest();
        //            request.MakePostRequest(fullUrl, json, contentType: "application/json; charset=UTF-8");
        //            var task = _httpClientEx.SendRequest(request);
        //            //var task = _httpClientEx.SendGetRequest(fullUrl);
        //            task.Wait();
        //            var response = task.Result;
        //            if (response.Status == 200)
        //            {
        //                var jobject = JObject.Parse(response.Body);
        //                _authToken = jobject["token"].ToString();
        //            }
        //            //Task.Factory.StartNew(async () =>
        //            //{
        //            //    await File.WriteAllTextAsync("log.txt", response.ToString());
        //            //});
        //        }
        //        return _authToken;
        //    }
        //}


        //public async Task<HttpResponse> GetRequestAPIAsync(string url)
        //{
        //    if (_httpClientEx == null)
        //    {
        //        Log("Error: UseContentDelivery is false");
        //        return null;
        //    }
        //    if (_settings.ContentDelivery.UseContentDelivery == false)
        //    {
        //        return null;
        //    }
        //    var fullUrl = PathUtil.Combine(_settings.ContentDelivery.ApiRoot, url);
        //    var response = await _httpClientEx.SendGetRequest(fullUrl);
        //    if (response.Status >= 300)
        //    {
        //        Log("Error: HttpResponse code is over 300");
        //        return null;
        //    }
        //    return response;
        //}

        public async Task<string> RequestAPIAsync(string url, string content, string method = "Get")
        {
            if (_settings.ContentDelivery.UseContentDelivery == false)
            {
                return null;
            }

            string result = null;
            try
            {
                var fullUrl = PathUtil.Combine(APIURL, url);
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(fullUrl);
                request.KeepAlive = false;
                request.ContentType = "application/json; charset=UTF-8";
                request.Headers.Add("Authorization", "Basic " + SvcCredentials);
                request.Headers.Add("Accept", "*/*");
                request.Method = method;
                if (content != null)
                {
                    var reqBody = Encoding.UTF8.GetBytes(content);
                    request.ContentLength = reqBody.Length;
                    var reqStream = request.GetRequestStream();
                    await reqStream.WriteAsync(reqBody, 0, reqBody.Length);
                    reqStream.Close();
                }

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return null;
                }
                result = new StreamReader(response.GetResponseStream()).ReadToEnd();

            }
            catch (WebException exception)
            {

            }

            return result;
        }

        public async Task<string> GetRequestAPIAsync(string url)
        {
            return await RequestAPIAsync(url, null, "GET");
        }

        public async Task<string> PostRequestAPIAsync(string url, string content)
        {
            return await RequestAPIAsync(url, content, "POST");
        }

        public async Task<string> PutRequestAPIAsync(string url, string content)
        {
            return await RequestAPIAsync(url, content, "PUT");
        }

        public async Task<string> PatchRequestAPIAsync(string url, string content)
        {
            return await RequestAPIAsync(url, content, "PATCH");
        }

        public async Task<string> DeleteRequestAPIAsync(string url)
        {
            return await RequestAPIAsync(url, null, "DELETE");
        }
        #endregion


        private void OnMessageReceived(object sender, Message message)
        {
            if (message.Type == MessageType.Packet)
            {
                PacketReceived?.Invoke(sender, JsonConvert.DeserializeObject<Packet>(message.Data));
            }
            else
            {
                MessageReceived?.Invoke(sender, message);
            }
        }

        private void OnErrorReceived(object sender, NetworkError error)
        {
            ErrorReceived?.Invoke(sender, error);
        }

        protected void OnNetworkSettingChanged()
        {
            RefreshSessions();
        }

        protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnNetworkSettingChanged();
        }

        private void Log(string log)
        {
            if (_settings == null)
            {
                return;
            }
            if (!(_settings.UseDebug ?? false))
            {
                return;
            }

            Console.WriteLine(log);
        }

    }
}
