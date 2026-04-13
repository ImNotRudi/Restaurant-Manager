using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Linq;
using System;
using WpfApp1.Repositories; 
using WpfApp1.Model;        

namespace WpfApp1
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly IGuestRepository _repository;
        private Guest _selectedGuest;

        public ObservableCollection<Table> Tables { get; set; }
        public ObservableCollection<Guest> WaitingList { get; set; }
        public ObservableCollection<Guest> SeatedList { get; set; } = new ObservableCollection<Guest>();
        public ObservableCollection<Guest> CompletedList { get; set; } = new ObservableCollection<Guest>();
        // Statystyki
        public int WaitingCount => WaitingList.Sum(g => g.PeopleCount);
        public int CurrentGuests => Tables.Sum(t => t.GuestCount);
        public int FreeSeats => Tables.Sum(t => t.Capacity) - CurrentGuests;
        public int FreeTables => Tables.Count(t => t.GuestCount == 0);
        public int OccupiedTables => Tables.Count(t => t.GuestCount > 0);
        public string Occupancy => Tables.Sum(t => t.Capacity) == 0 ? "0%" : $"{(int)((double)CurrentGuests / Tables.Sum(t => t.Capacity) * 100)}%";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public MainWindow()
        {
            InitializeComponent();

            _repository = new GuestRepository();
            var currentlySeated = _repository.ReadAll()
                .Where(g => g.TableNumber != null && string.IsNullOrEmpty(g.CheckoutTime))
                .ToList();

            SeatedList = new ObservableCollection<Guest>(currentlySeated);

            var guestsInDb = _repository.ReadAll()
                .Where(g => g.TableNumber == null && string.IsNullOrEmpty(g.CheckoutTime))
                .ToList();

            var completedGuests = _repository.ReadAll()
            .Where(g => !string.IsNullOrEmpty(g.CheckoutTime))
            .ToList();

            CompletedList = new ObservableCollection<Guest>(completedGuests);

            WaitingList = new ObservableCollection<Guest>(guestsInDb);

            Tables = new ObservableCollection<Table>
            {
                new Table { Number = 1, Capacity = 4, GuestCount = 0 },
                new Table { Number = 2, Capacity = 2, GuestCount = 0 },
                new Table { Number = 3, Capacity = 6, GuestCount = 0 },
                new Table { Number = 4, Capacity = 4, GuestCount = 0 },
                new Table { Number = 5, Capacity = 2, GuestCount = 0 },
                new Table { Number = 6, Capacity = 6, GuestCount = 0 },
                new Table { Number = 7, Capacity = 4, GuestCount = 0 },
                new Table { Number = 8, Capacity = 2, GuestCount = 0 },
                new Table { Number = 9, Capacity = 6, GuestCount = 0 }
            };

            var seatedGuests = _repository.ReadAll()
                .Where(g => g.TableNumber != null && string.IsNullOrEmpty(g.CheckoutTime))
                .ToList();

            foreach (var guest in seatedGuests)
            {
                var table = Tables.FirstOrDefault(t => t.Number == guest.TableNumber);
                if (table != null) table.GuestCount = guest.PeopleCount;
            }

            this.DataContext = this;
            RefreshStatistics();
        }
        private void RefreshCompletedList()
        {
            if (_repository == null) return;

            DateTime today = DateTime.Today;

            var history = _repository.ReadAll()
                .Where(g => !string.IsNullOrEmpty(g.CheckoutTime) && g.RecordDate.Date == today)
                .OrderByDescending(g => g.CheckoutTime)
                .ToList();

            CompletedList.Clear();
            foreach (var guest in history)
            {
                CompletedList.Add(guest);
            }
        }
        private void Table_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var table = (Table)button.DataContext;

            if (table.GuestCount == 0 && _selectedGuest != null)
            {
                if (_selectedGuest.PeopleCount > table.Capacity)
                {
                    MessageBox.Show(
                        $"Ten stolik jest za mały dla tej grupy!\n\nMiejsca przy stoliku: {table.Capacity}\nLiczba gości: {_selectedGuest.PeopleCount}",
                        "Błąd usadzania",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return; 
                }

                table.GuestCount = _selectedGuest.PeopleCount;

                _selectedGuest.TableNumber = table.Number;
                _selectedGuest.SeatedTime = DateTime.Now.ToString("HH:mm");
                _selectedGuest.IsSelected = false;

                _repository.Update(_selectedGuest);

                WaitingList.Remove(_selectedGuest);
                SeatedList.Add(_selectedGuest);
                _selectedGuest = null;

                RefreshStatistics();
                OnPropertyChanged(nameof(WaitingCount));
            }
            else if (table.GuestCount > 0)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Czy na pewno chcesz zwolnić stolik nr {table.Number} i zakończyć wizytę gości?",
                    "Potwierdzenie zwolnienia stolika",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var guestAtTable = _repository.ReadAll()
                        .FirstOrDefault(g => g.TableNumber == table.Number && string.IsNullOrEmpty(g.CheckoutTime));

                    if (guestAtTable != null)
                    {
                        guestAtTable.CheckoutTime = DateTime.Now.ToString("HH:mm");
                        _repository.Update(guestAtTable);

                        var guestToRemove = SeatedList.FirstOrDefault(g => g.Id == guestAtTable.Id);
                        if (guestToRemove != null)
                        {
                            SeatedList.Remove(guestToRemove);
                            CompletedList.Insert(0, guestToRemove); 
                        }
                    }

                    table.GuestCount = 0;
                    RefreshStatistics();
                }
            }
            else
            {
                MessageBox.Show(
                    "Najpierw wybierz gościa z listy oczekujących (Waiting List) po prawej stronie!",
                    "Informacja",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void ToManager_Click(object sender, RoutedEventArgs e)
        {
            var history = _repository.ReadAll()
                .Where(g => !string.IsNullOrEmpty(g.CheckoutTime))
                .OrderByDescending(g => g.CheckoutTime)
                .ToList();

            CompletedList.Clear();
            foreach (var guest in history)
            {
                CompletedList.Add(guest);
            }
            RefreshManagerActivity();
            WorkerView.Visibility = Visibility.Collapsed;
            ManagerView.Visibility = Visibility.Visible;
        }

        

        private void ToWorker_Click(object sender, RoutedEventArgs e)
        {
            ManagerView.Visibility = Visibility.Collapsed;
            WorkerView.Visibility = Visibility.Visible;
        }
        private void AddGuest_Click(object sender, RoutedEventArgs e)
        {
            CreateWindow createWin = new CreateWindow();
            createWin.Owner = this;

            if (createWin.ShowDialog() == true)
            {
                Guest guestFromWindow = createWin.NewGuest;
                guestFromWindow.RecordDate = DateTime.Now; 

                _repository.Create(guestFromWindow);

                WaitingList.Add(guestFromWindow);

                OnPropertyChanged(nameof(WaitingList));
                OnPropertyChanged(nameof(WaitingCount));
            }
        }

        private void SelectGuest_Click(object sender, MouseButtonEventArgs e)
        {
            var border = (Border)sender;
            var clickedGuest = (Guest)border.DataContext;

            if (_selectedGuest != null) _selectedGuest.IsSelected = false;

            _selectedGuest = clickedGuest;
            _selectedGuest.IsSelected = true;
        }

        private void RefreshStatistics()
        {
            OnPropertyChanged(nameof(CurrentGuests));
            OnPropertyChanged(nameof(FreeSeats));
            OnPropertyChanged(nameof(FreeTables));
            OnPropertyChanged(nameof(OccupiedTables));
            OnPropertyChanged(nameof(Occupancy));
        }
        private void RefreshManagerActivity()
        {
            if (_repository == null) return;

            DateTime today = DateTime.Today;

            var allToday = _repository.ReadAll()
                .Where(g => g.RecordDate.Date == today)
                .OrderByDescending(g => g.ArrivalTime) 
                .ToList();

            CompletedList.Clear();
            foreach (var guest in allToday)
            {
                CompletedList.Add(guest);
            }
        }


        private bool _isDarkMode = false;
        private void DarkMode_Click(object sender, RoutedEventArgs e)
        {
            _isDarkMode = !_isDarkMode;
            var themeUri = _isDarkMode
                ? new Uri("DarkMode.xaml", UriKind.Relative)
                : new Uri("LightMode.xaml", UriKind.Relative);

            ResourceDictionary dict = new ResourceDictionary { Source = themeUri };
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(dict);

            (sender as Button).Content = _isDarkMode ? "☀️" : "🌙";
        }
    }
}