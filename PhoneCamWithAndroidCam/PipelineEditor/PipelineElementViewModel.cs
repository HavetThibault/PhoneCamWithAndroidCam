using Helper.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneCamWithAndroidCam.PipelineEditor
{
    public class PipelineElementViewModel
    {
        private Action<object> _deleteAction;

        public string Name { get; }
        public RelayCommand DeleteCommand { get; }

        public PipelineElementViewModel(string name, Action<object> deleteAction)
        {
            _deleteAction = deleteAction;
            Name = name;
            DeleteCommand = new(Delete);
        }

        private void Delete(object parameter)
        {
            _deleteAction(this);
        }
    }
}
