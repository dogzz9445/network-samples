using Common;
using Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpf_netcore_tcp_server.ViewModel
{
    public class HomeViewModel : BindableBase
    {
        private NetworkManager _networkManager;

        public HomeViewModel()
        {
             _networkManager = new NetworkManager();
        }
    }
}
