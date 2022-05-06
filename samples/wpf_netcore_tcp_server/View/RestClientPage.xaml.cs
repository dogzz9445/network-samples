using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using wpf_netcore_tcp_server.ViewModel;

namespace wpf_netcore_tcp_server.View
{
    /// <summary>
    /// RestClientPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RestClientPage : Page
    {
        public RestClientViewModel ViewModel;

        public RestClientPage()
        {
            InitializeComponent();

            ViewModel = new RestClientViewModel();
            DataContext = ViewModel;
        }
    }
}
