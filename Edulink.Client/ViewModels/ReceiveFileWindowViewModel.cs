using Edulink.Classes;
using Edulink.Models;
using Edulink.MVVM;
using System;
using System.IO;
using System.Windows.Input;

namespace Edulink.ViewModels
{
    public class ReceiveFileWindowViewModel : ViewModelBase
    {
        private bool _receivingFile;
        public bool ReceivingFile
        {
            get => _receivingFile;
            set
            {
                if (_receivingFile != value)
                {
                    _receivingFile = value;
                    OnPropertyChanged();
                }
            }
        }

        private Computer _sourceComputer;
        public Computer SourceComputer
        {
            get => _sourceComputer;
            set
            {
                if (_sourceComputer != value)
                {
                    _sourceComputer = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SourceName => SourceComputer?.IsTeacher == false ? SourceComputer.Name : LocalizedStrings.Instance["ReceiveFile.ReceiveFileFrom.Teacher"];

        private FileInfo _file;
        public FileInfo File
        {
            get => _file;
            set
            {
                if (_file != value)
                {
                    _file = value;
                    OnPropertyChanged();
                }
            }
        }

        public event EventHandler RequestAccept;

        public ReceiveFileWindowViewModel(Computer sourceComputer, FileInfo fileInfo)
        {
            _sourceComputer = sourceComputer;
            _file = fileInfo;
        }

        public ReceiveFileWindowViewModel() { }

        public ICommand AcceptCommand => new RelayCommand(execute => HandleAccept());

        private void HandleAccept()
        {
            RequestAccept?.Invoke(this, EventArgs.Empty);
        }
    }
}
