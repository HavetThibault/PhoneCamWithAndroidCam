using PhoneCamWithAndroidCam.ViewModels.TemplateManagement;
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
using System.Windows.Shapes;
using Wpf.Common.Controls;

namespace PhoneCamWithAndroidCam.Views
{
    /// <summary>
    /// Logique d'interaction pour ManageTemplateWindow.xaml
    /// </summary>
    public partial class ManageTemplateControl : UserControl
    {
        public ManageTemplateControl(ManageTemplateViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
