using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using Newtonsoft.Json;
using SettingNetwork.Util;

namespace SettingNetwork
{
    public class ContentDeliveryServerInfo : BindableBase
    {
        #region 속성
        [JsonProperty("useContentDelivery")]
        private bool? _useContentDelivery;
        [JsonProperty("localFileRoot")]
        private string _localFileRoot;
        [JsonProperty("address")]
        private string _address;
        [JsonProperty("port")]
        private int? _port;
        [JsonProperty("apiRoot")]
        private string _apiRoot;
        [JsonProperty("contentRoot")]
        private string _contentRoot;

        [JsonIgnore]
        public bool? UseContentDelivery { get => _useContentDelivery; set => SetProperty(ref _useContentDelivery, value); }
        [JsonIgnore]
        public string LocalFileRoot { get => _localFileRoot; set => SetProperty(ref _localFileRoot, value); }
        [JsonIgnore]
        public string Address { get => _address; set => SetProperty(ref _address, value); }
        [JsonIgnore]
        public int? Port { get => _port; set => SetProperty(ref _port, value); }
        [JsonIgnore]
        public string ApiRoot { get => _apiRoot; set => SetProperty(ref _apiRoot, value); }
        [JsonIgnore]
        public string ContentRoot { get => _contentRoot; set => SetProperty(ref _contentRoot, value); }
        #endregion
        public ContentDeliveryServerInfo() : this(null) { }

        public ContentDeliveryServerInfo(
            bool? useContentDelivery = null,
            string address = null,
            int? port = null,
            string apiRoot = null,
            string contentRoot = null, 
            string localFileRoot = null)
        {
            _useContentDelivery = useContentDelivery ?? false;
            _localFileRoot = localFileRoot ?? "Media/";
            _address = address ?? "192.168.0.222";
            _port = port ?? 8000;
            _apiRoot = apiRoot ?? "/";
            _contentRoot = contentRoot ?? "/media/";
        }
    }


    public class ComputerInfo : BindableBase
    {
        #region 속성
        [JsonProperty("id")]
        private int? _id;
        [JsonProperty("ipAddress")]
        private string _ipAddress;
        [JsonProperty("name")]
        private string _name;
        [JsonProperty("description")]
        private string _description;
        [JsonProperty("type")]
        private string _type;
        [JsonProperty("useTcpServer")]
        private bool? _useTcpServer;
        [JsonProperty("tcpPort")]
        private int? _tcpPort;
        [JsonProperty("useFileTcpServer")]
        private bool? _useFileTcpServer;
        [JsonProperty("fileTcpPort")]
        private int? _fileTcpPort;
        [JsonProperty("useUdpServer")]
        private bool? _useUdpServer;
        [JsonProperty("udpPort")]
        private int? _udpPort;
        [JsonProperty("useHttpServer")]
        private bool? _useHttpServer;
        [JsonProperty("httpPort")]
        private int? _httpPort;

        [JsonIgnore]
        public int? Id { get => _id; set => SetProperty(ref _id, value); }
        [JsonIgnore]
        public string IpAddress { get => _ipAddress; set => SetProperty(ref _ipAddress, value); }
        [JsonIgnore]
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        [JsonIgnore]
        public string Description { get => _description; set => SetProperty(ref _description, value); }
        [JsonIgnore]
        public string Type { get => _type; set => SetProperty(ref _type, value); }
        [JsonIgnore]
        public bool? UseTcpServer { get => _useTcpServer; set => SetProperty(ref _useTcpServer, value); }
        [JsonIgnore]
        public int? TcpPort { get => _tcpPort; set => SetProperty(ref _tcpPort, value); }
        [JsonIgnore]
        public bool? UseUdpServer { get => _useUdpServer; set => SetProperty(ref _useUdpServer, value); }
        [JsonIgnore]
        public int? UdpPort { get => _udpPort; set => SetProperty(ref _udpPort, value); }
        [JsonIgnore]
        public bool? UseHttpServer { get => _useHttpServer; set => SetProperty(ref _useHttpServer, value); }
        [JsonIgnore]
        public int? HttpPort { get => _httpPort; set => SetProperty(ref _httpPort, value); }
        [JsonIgnore]
        public bool? UseFileTcpServer { get => _useFileTcpServer; set => SetProperty(ref _useFileTcpServer, value); }
        [JsonIgnore]
        public int? FileTcpPort { get => _fileTcpPort; set => SetProperty(ref _fileTcpPort, value); }
        #endregion

        #region 생성자
        public ComputerInfo() : this(null) { }

        public ComputerInfo(
            int? id = null,
            string ipAddress = null,
            string name = null,
            string description = null,
            string type = null,
            int? tcpPort = null,
            int? udpPort = null,
            int? httpPort = null, bool? useTcpServer = null, bool? useUdpServer = null, bool? useHttpServer = null, bool? useFileTcpServer = null, int? fileTcpPort = null)
        {
            Id = id ?? 0;
            IpAddress = ipAddress ?? IPAddress.None.ToString();
            Name = name ?? "기본";
            Description = description ?? "기본 컴퓨터 구성";
            Type = type ?? "기본 클라이언트";
            _useTcpServer = useTcpServer ?? true;
            _tcpPort = tcpPort ?? 5000;
            _useUdpServer = useUdpServer ?? true;
            _udpPort = udpPort ?? 5000;
            _useHttpServer = useHttpServer ?? false;
            _httpPort = httpPort ?? 5001;
            UseFileTcpServer = useFileTcpServer ?? true;
            _fileTcpPort = fileTcpPort ?? 5002;
        }

        #endregion

        #region 비교 구문
        public override bool Equals(object obj)
        {
            return obj is ComputerInfo info &&
                   _id == info._id &&
                   _ipAddress == info._ipAddress &&
                   _name == info._name &&
                   _description == info._description &&
                   _type == info._type &&
                   _useTcpServer == info._useTcpServer &&
                   _tcpPort == info._tcpPort &&
                   _useFileTcpServer == info._useFileTcpServer &&
                   _fileTcpPort == info._fileTcpPort &&
                   _useUdpServer == info._useUdpServer &&
                   _udpPort == info._udpPort &&
                   _useHttpServer == info._useHttpServer &&
                   _httpPort == info._httpPort;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(_id);
            hash.Add(_ipAddress);
            hash.Add(_name);
            hash.Add(_description);
            hash.Add(_type);
            hash.Add(_useTcpServer);
            hash.Add(_tcpPort);
            hash.Add(_useFileTcpServer);
            hash.Add(_fileTcpPort);
            hash.Add(_useUdpServer);
            hash.Add(_udpPort);
            hash.Add(_useHttpServer);
            hash.Add(_httpPort);
            return hash.ToHashCode();
        }

        #endregion
    }

    public class NetworkSettings : BindableBase
    {
        #region 속성
        [JsonProperty("useDebug")]
        private bool? _useDebug;
        [JsonProperty("hostId")]
        private int? _hostId;
        [JsonProperty("isSaveFileAbsolutePath")]
        private bool? _isSaveFileAbsolutePath;
        [JsonProperty("fileSavedDirectory")]
        private string _fileSavedDirectory;
        [JsonProperty("contentDelivery")]
        private ContentDeliveryServerInfo _contentDelivery;
        [JsonProperty("computerInfos")]
        private ObservableCollection<ComputerInfo> _computerInfos;

        [JsonIgnore]
        public bool? UseDebug { get => _useDebug; set => SetProperty(ref _useDebug, value); }
        [JsonIgnore]
        public int? HostId { get => _hostId; set => SetProperty(ref _hostId, value); }
        [JsonIgnore]
        public ContentDeliveryServerInfo ContentDelivery { get => _contentDelivery; set => SetObservableProperty(ref _contentDelivery, value); }
        [JsonIgnore]
        public ObservableCollection<ComputerInfo> ComputerInfos { get => _computerInfos; set => SetCollectionProperty(ref _computerInfos, value); }
        [JsonIgnore]
        public bool? IsSaveFileAbsolutePath { get => _isSaveFileAbsolutePath; set => SetProperty(ref _isSaveFileAbsolutePath, value); }
        [JsonIgnore]
        public string FileSavedDirectory { get => _fileSavedDirectory; set => SetProperty(ref _fileSavedDirectory, value); }
        #endregion

        #region 생성자
        public NetworkSettings() : this(null) { }

        public NetworkSettings(bool? useDebug = null, int? hostId = null, ObservableCollection<ComputerInfo> computerInfos = null, bool? isSaveFileAbsolutePath = null, string fileSavedDirectory = null, ContentDeliveryServerInfo contentDelivery = null)
        {
            UseDebug = useDebug ?? false;
            HostId = hostId ?? 0;
            ComputerInfos = computerInfos ?? new ObservableCollection<ComputerInfo>();
            IsSaveFileAbsolutePath = isSaveFileAbsolutePath ?? false;
            FileSavedDirectory = fileSavedDirectory ?? "./Data";
            ContentDelivery = contentDelivery ?? new ContentDeliveryServerInfo();
        }

        public override bool Equals(object obj)
        {
            return obj is NetworkSettings settings &&
                   _useDebug == settings._useDebug &&
                   _hostId == settings._hostId &&
                   _isSaveFileAbsolutePath == settings._isSaveFileAbsolutePath &&
                   _fileSavedDirectory == settings._fileSavedDirectory &&
                   EqualityComparer<ObservableCollection<ComputerInfo>>.Default.Equals(_computerInfos, settings._computerInfos);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_useDebug, _hostId, _isSaveFileAbsolutePath, _fileSavedDirectory, _computerInfos);
        }

        public void Add(ComputerInfo computerInfo)
        {
            computerInfo.PropertyChanged += RaisePropertyChangedEvent;
            ComputerInfos.Add(computerInfo);
            RaisePropertyChangedEvent();
        }
        #endregion

        #region 비교 구문
        #endregion
    }
}
