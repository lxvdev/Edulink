using Edulink.Communication.Models;
using Edulink.Models;
using Edulink.MVVM;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace Edulink.ViewModels
{
    public class SendFileWindowViewModel : ViewModelBase
    {
        // TODO: Add a status message
        private bool _receiving = true;
        public bool Receiving
        {
            get => _receiving;
            set
            {
                if (_receiving != value)
                {
                    _receiving = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _sharingEnabled = false;
        public bool SharingEnabled
        {
            get => _sharingEnabled;
            set
            {
                if (_sharingEnabled != value)
                {
                    _sharingEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

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

        private ObservableCollection<Computer> _computers;

        public ICollectionView ComputersView { get; }

        private Computer _selectedComputer;
        public Computer SelectedComputer
        {
            get => _selectedComputer;
            set
            {
                if (_selectedComputer != value)
                {
                    _selectedComputer = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isTeacher;
        public bool IsTeacher
        {
            get => _isTeacher;
            set
            {
                if (_isTeacher != value)
                {
                    _isTeacher = value;
                    OnPropertyChanged();
                }
            }
        }

        public SendFileWindowViewModel()
        {
            _computers = new ObservableCollection<Computer>();
            ComputersView = CollectionViewSource.GetDefaultView(_computers);

            ComputersView.SortDescriptions.Add(new SortDescription(nameof(Computer.Name), ListSortDirection.Ascending));

            if (App.Client.Connected)
            {
                Task.Run(() => App.Client.Helper.SendCommandAsync(new EdulinkCommand() { Command = Commands.ComputerList.ToString() }));
            }
            else
            {
                Receiving = false;
                SharingEnabled = false;
            }
        }

        public ICommand ListPageCommand => new RelayCommand(execute => GoToListPage(), canExecute => !_receiving && _computers != null);

        private void GoToListPage() => PageIndex = 1;

        public void UpdateList(List<Computer> computers)
        {
            if (computers == null) return;

            _computers.Clear();

            computers.ForEach(computer => _computers.Add(computer));

            SharingEnabled = true;
            Receiving = false;

            if (ListPageCommand.CanExecute(this))
            {
                ListPageCommand.Execute(this);
            }
        }

        public void SetSharingStatus(bool enabled)
        {
            SharingEnabled = enabled;
            Receiving = false;
        }
    }
}
