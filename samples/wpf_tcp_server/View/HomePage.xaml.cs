using System.Windows.Controls;
using wpf_tcp_server.ViewModel;

namespace wpf_tcp_server.View
{
    /// <summary>
    /// HomePage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HomePage : Page
    {
        private HomeViewModel _viewModel;
        
        public HomePage()
        {
            InitializeComponent();

            _viewModel = new HomeViewModel();
            DataContext = _viewModel;
        }
    }
}
