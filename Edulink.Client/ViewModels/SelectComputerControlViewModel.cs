using Edulink.Models;
using Edulink.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace Edulink.ViewModels
{
    public class SelectComputerControlViewModel : ViewModelBase
    {
        #region List
        private ObservableCollection<Computer> _computers;
        public ObservableCollection<Computer> Computers
        {
            get => _computers;
            set
            {
                if (_computers != value)
                {
                    _computers = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICollectionView ComputersView { get; }
        #endregion

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

        public bool SendTeacher
        {
            get => SelectedComputer?.IsTeacher == true;
            set
            {
                if (value == true)
                {
                    SelectedComputer = new Computer() { IsTeacher = value };
                }
                else
                {
                    SelectedComputer = null;
                }

                OnPropertyChanged();
            }
        }

        private bool _canSendTeacher;
        public bool CanSendTeacher
        {
            get => _canSendTeacher;
            set
            {
                if (_canSendTeacher != value)
                {
                    _canSendTeacher = value;
                    OnPropertyChanged();
                }
            }
        }

        public event EventHandler RequestNext;

        public SelectComputerControlViewModel()
        {
            _computers = new ObservableCollection<Computer>();
            ComputersView = CollectionViewSource.GetDefaultView(_computers);

            ComputersView.SortDescriptions.Add(new SortDescription(nameof(Computer.Name), ListSortDirection.Ascending));
        }

        #region Commands
        public ICommand NextCommand => new RelayCommand(execute => RequestNext?.Invoke(this, EventArgs.Empty), canExecute => SelectedComputer != null || SendTeacher);
        #endregion

        #region Methods
        public void UpdateList(List<Computer> computers)
        {
            if (computers == null) return;

            _computers.Clear();

            computers.ForEach(computer => Computers.Add(computer));
        }
        #endregion
    }
}
