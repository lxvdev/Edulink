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
                    if (value == 0)
                    {
                        SelectComputerViewModel.Computers.Clear();
                        RequestComputerList?.Invoke(this, EventArgs.Empty);
                        OnPropertyChanged(nameof(InitialStatusMessage));
                    }

                    _pageIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        #region Sharing status
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
                if (!_isReceivingList && !App.Client.Connected)
                {
                    return "SendFile.InitialStatusMessage.NotConnected";
                }
                else if (_isReceivingList)
                {
                    return "SendFile.InitialStatusMessage.LoadingList";
                }
                else if (!_isReceivingList && !_sharingToStudents && !_sharingToTeacher)
                {
                    return "SendFile.InitialStatusMessage.SharingNotAllowed";
                }

                return "";
            }
        }
        #endregion

        #region Receiving status
        private bool _isReceivingList;
        public bool IsReceivingList
        {
            get => _isReceivingList;
            set
            {
                if (_isReceivingList != value)
                {
                    _isReceivingList = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(InitialStatusMessage));
                }
            }
        }

        private bool _isReceivingResponse;
        public bool IsReceivingResponse
        {
            get => _isReceivingResponse;
            set
            {
                if (_isReceivingResponse != value)
                {
                    _isReceivingResponse = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanChangeFile));
                }
            }
        }
        #endregion

        #region Target computer
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

        #region File sharing
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
                    OnPropertyChanged(nameof(IsFileSelected));
                }
            }
        }

        public bool IsFileSelected => File != null;

        private bool _isSendingFile;
        public bool IsSendingFile
        {
            get => _isSendingFile;
            set
            {
                if (_isSendingFile != value)
                {
                    _isSendingFile = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanChangeFile));
                }
            }
        }

        private int _percentageSent;
        public int Progress
        {
            get => _percentageSent;
            set
            {
                if (_percentageSent != value)
                {
                    _percentageSent = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isFileSent;
        public bool IsFileSent
        {
            get => _isFileSent;
            set
            {
                if (_isFileSent != value)
                {
                    _isFileSent = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        public bool CanChangeFile => !IsSendingFile && !IsReceivingResponse;

        public event EventHandler RequestComputerList;

        public SendFileWindowViewModel()
        {
            SelectComputerViewModel = new SelectComputerControlViewModel();
            SelectComputerViewModel.RequestNext += _computerControlViewModel_RequestNext;
            RequestComputerList?.Invoke(this, EventArgs.Empty);
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
        public ICommand ListPageCommand => new RelayCommand(execute => GoToListPage(), canExecute => !_isReceivingList && SelectComputerViewModel.Computers.Any());

        private void GoToListPage()
        {
            PageIndex = 1;
        }

        public ICommand SendFilePageCommand => new RelayCommand(execute => GoToSendFilePage(),
                                                                canExecute => !_isReceivingList && SelectComputerViewModel.SelectedComputer != null);

        private void GoToSendFilePage()
        {
            TargetComputer = SelectComputerViewModel.SelectedComputer;
            PageIndex = 2;
        }
        public event EventHandler RequestSendFile;

        public ICommand SendCommand => new RelayCommand(execute => HandleSendCommandAsync(), canExecute => IsFileSelected && !CanChangeFile);

        private void HandleSendCommandAsync()
        {
            RequestSendFile?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Methods
        public void UpdateList(List<Computer> computers)
        {
            SelectComputerViewModel.UpdateList(computers);

            IsReceivingList = false;

            if (ListPageCommand.CanExecute(this))
            {
                ListPageCommand.Execute(this);
            }
        }

        public void SetSharingStatus(bool student, bool teacher)
        {
            SharingToStudents = student;
            SharingToTeacher = teacher;

            // Update sharing status on select computer view model
            SelectComputerViewModel.CanSendTeacher = SharingToTeacher;

            IsReceivingList = false;

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

        public void SetFile(FileInfo file) => File = file;

        public void Reset()
        {
            PageIndex = 0;
            IsReceivingList = false;
            IsReceivingResponse = false;
            TargetComputer = null;
            SharingToStudents = false;
            SharingToTeacher = false;
            File = null;
            IsSendingFile = false;
            IsFileSent = false;
            Progress = 0;
            SelectComputerViewModel.Reset();
        }
        #endregion
    }
}