using PhoneCamWithAndroidCam.ViewModels;
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
    /// Logique d'interaction pour PipelineChooserWindow.xaml
    /// </summary>
    public partial class PipelineChooserView : UserControl
    {
        public PipelineChooserView(PipelineChooserViewModel viewModel) : base()
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
