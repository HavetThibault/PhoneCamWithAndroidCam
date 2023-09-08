using MahApps.Metro.Controls;
using PhoneCamWithAndroidCam.ViewModels.PipelineEditor;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Common.Controls;

namespace PhoneCamWithAndroidCam.Views
{
    /// <summary>
    /// Logique d'interaction pour PipelineEditorControl.xaml
    /// </summary>
    public partial class PipelineEditorControl : UserControl
    {
        private PipelineEditorViewModel _viewModel;

        public PipelineEditorControl(PipelineEditorViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            _viewModel = viewModel;
        }

        private void PipelineElementMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is FrameworkElement pipelineUIElement && e.LeftButton == MouseButtonState.Pressed)
                DragDrop.DoDragDrop(pipelineUIElement, pipelineUIElement.DataContext, DragDropEffects.Move);
        }

        private void DropPipelineElement(object sender, DragEventArgs e)
        {
            int newIndex = _viewModel.GetPipelineElementIndex((PipelineElementViewModel)((FrameworkElement)sender).DataContext);
            int previousIndex = _viewModel.GetPipelineElementIndex((PipelineElementViewModel)e.Data.GetData(typeof(PipelineElementViewModel)));
            if(newIndex != previousIndex)
                _viewModel.ChangePipelineElementsPlace(previousIndex, newIndex);
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            var deleteButton = ((Grid)sender).FindChild<SymbolButton>("DeleteButton");
            var animation = new DoubleAnimation
            {
                To = deleteButton.ActualWidth - 4, // Move the button back below the TextBlock
                Duration = TimeSpan.FromSeconds(0.3),
            };
            deleteButton.RenderTransform.BeginAnimation(
                TranslateTransform.XProperty,
                animation);
        }

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            var animation = new DoubleAnimation
            {
                To = 0, // Move the button back below the TextBlock
                Duration = TimeSpan.FromSeconds(0.3)
            };

            var deleteButton = ((Grid)sender).FindChild<SymbolButton>("DeleteButton");
            deleteButton.RenderTransform.BeginAnimation(
                TranslateTransform.XProperty,
                animation);
        }
    }
}
