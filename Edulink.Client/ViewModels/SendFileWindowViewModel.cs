using Edulink.Classes;
using Edulink.Models;
using Edulink.MVVM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace Edulink.ViewModels
{
    public class SendFileWindowViewModel : ViewModelBase
    {
        public SelectComputerControlViewModel SelectComputerViewModel { get; }

        private int _pageIndex = 0;
        public int PageIndex
        {
            get => _pageIndex;
            set
            {
                if (_pageIndex != value)
                {
                    _pageIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        #region Sharing and Receiving Status Properties
        private bool _receiving;
        public bool Receiving
        {
            get => _receiving;
            set
            {
                if (_receiving != value)
                {
                    _receiving = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(InitialStatusMessage));
                }
            }
        }

        private bool _sharingToStudents;
        public bool SharingToStudents
        {
            get => _sharingToStudents;
            set
            {
                if (_sharingToStudents != value)
                {
                    _sharingToStudents = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(InitialStatusMessage));
                }
            }
        }

        private bool _sharingToTeacher;
        public bool SharingToTeacher
        {
            get => _sharingToTeacher;
            set
            {
                if (_sharingToTeacher != value)
                {
                    _sharingToTeacher = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(InitialStatusMessage));
                }
            }
        }

        public string InitialStatusMessage
        {
            get
            {
                if (!_receiving && !App.Client.Connected)
                {
                    return "SendFile.InitialStatusMessage.NotConnected";
                }
                else if (_receiving)
                {
                    return "SendFile.InitialStatusMessage.LoadingList";
                }
                else if (!_receiving && !_sharingToStudents && !_sharingToTeacher)
                {
                    return "SendFile.InitialStatusMessage.SharingNotAllowed";
                }

                return "";
            }
        }
        #endregion

        #region Computer Properties
        private Computer _targetComputer;
        public Computer TargetComputer
        {
            get => _targetComputer;
            set
            {
                if (_targetComputer != value)
                {
                    _targetComputer = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TargetName));
                }
            }
        }

        public string TargetName => TargetComputer?.IsTeacher == false ? TargetComputer.Name : LocalizedStrings.Instance["SendFile.Send.SendFileTo.Teacher"];
        #endregion

        #region File sharing Properties
        private FileInfo _fileInfo;
        public FileInfo File
        {
            get => _fileInfo;
            set
            {
                if (_fileInfo != value)
                {
                    _fileInfo = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FileSelected));
                }
            }
        }

        public bool FileSelected => File != null;

        private bool _sendingFile;
        public bool SendingFile
        {
            get => _sendingFile;
            set
            {
                if (_sendingFile != value)
                {
                    _sendingFile = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        public SendFileWindowViewModel()
        {
            SelectComputerViewModel = new SelectComputerControlViewModel();
            SelectComputerViewModel.RequestNext += _computerControlViewModel_RequestNext;
        }

        #region Event handlers
        private void _computerControlViewModel_RequestNext(object sender, EventArgs e)
        {
            if (SendFilePageCommand.CanExecute(sender))
            {
                SendFilePageCommand.Execute(sender);
            }
        }
        #endregion

        #region Commands
        public ICommand ListPageCommand => new RelayCommand(execute => GoToListPage(), canExecute => !_receiving && SelectComputerViewModel.Computers.Any());

        private void GoToListPage()
        {
            PageIndex = 1;
        }

        public ICommand SendFilePageCommand => new RelayCommand(execute => GoToSendFilePage(),
                                                                canExecute => !_receiving && SelectComputerViewModel.SelectedComputer != null);

        private void GoToSendFilePage()
        {
            TargetComputer = SelectComputerViewModel.SelectedComputer;
            PageIndex = 2;
        }
        #endregion

        #region Methods
        public void UpdateList(List<Computer> computers)
        {
            SelectComputerViewModel.UpdateList(computers);

            SetReceivingStatus(false);

            if (ListPageCommand.CanExecute(this))
            {
                ListPageCommand.Execute(this);
            }
        }

        public void SetReceivingStatus(bool value) => Receiving = value;

        public void SetSharingStatus(bool student, bool teacher)
        {
            SharingToStudents = student;
            SharingToTeacher = teacher;

            // Update sharing status on select computer view model
            SelectComputerViewModel.CanSendTeacher = SharingToTeacher;

            SetReceivingStatus(false);

            // Go to send file page if only sharing to the teacher is allowed
            if (SharingToTeacher && !SelectComputerViewModel.Computers.Any())
            {
                SelectComputerViewModel.SendTeacher = true;

                if (SendFilePageCommand.CanExecute(this))
                {
                    SendFilePageCommand.Execute(this);
                }
            }
        }

        public void SetFile(FileInfo file)
        {
            if (file == null) return;

            File = file;
        }
        #endregion
    }
}