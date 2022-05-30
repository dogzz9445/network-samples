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

namespace wpf_netcore_tcp_server.View
{



    /// <summary>
    /// VisualTreePage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class VisualTreePage : Page
    {
        public VisualTreePage()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                int count = VisualTreeHelper.GetChildrenCount(Panel);
                for (int i = 0; i < count; i++)
                {
                    var stackPanel = VisualTreeHelper.GetChild(Panel, i) as StackPanel;
                    var label      = VisualTreeHelper.GetChild(stackPanel, 0) as Label;
                    label.Content = i;
                }
            };
        }
    }








}
