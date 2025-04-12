using Edulink.Classes;
using Edulink.Models;
using Edulink.MVVM;
using System;
using System.Windows.Input;

namespace Edulink.ViewModels
{
    public class ReceiveFileWindowViewModel : ClosableViewModel
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

        private string _fileName;
        public string FileName
        {
            get => _fileName;
            set
            {
                if (_fileName != value)
                {
                    _fileName = value;
                    OnPropertyChanged();
                }
            }
        }

        private long _fileLength;
        public long FileLength
        {
            get => _fileLength;
            set
            {
                if (_fileLength != value)
                {
                    _fileLength = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _saveDirectory;
        public string SaveDirectory
        {
            get => _saveDirectory;
            set
            {
                if (_saveDirectory != value)
                {
                    _saveDirectory = value;
                    OnPropertyChanged();
                }
            }
        }

        public event EventHandler RequestAccept;

        public event EventHandler RequestDecline;

        public ReceiveFileWindowViewModel(Computer sourceComputer, string fileName, long fileLength, string saveDirectory)
        {
            _sourceComputer = sourceComputer;
            _fileName = fileName;
            _fileLength = fileLength;
            _saveDirectory = saveDirectory;
        }

        public ReceiveFileWindowViewModel() { }

        public ICommand AcceptCommand => new RelayCommand(execute => RequestAccept?.Invoke(this, EventArgs.Empty));

        public ICommand DeclineCommand => new RelayCommand(execute => RequestDecline?.Invoke(this, EventArgs.Empty));
    }
}
