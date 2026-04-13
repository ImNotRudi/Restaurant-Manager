using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class Table : INotifyPropertyChanged
    {
        private int _guestCount;
        public int Number { get; set; }
        public int Capacity { get; set; }
        public bool IsOccupied => GuestCount > 0;
        public int GuestCount
        {
            get => _guestCount;
            set
            {
                _guestCount = value;
                OnPropertyChanged(nameof(GuestCount));
                OnPropertyChanged(nameof(Color));
                OnPropertyChanged(nameof(IsOccupied));
            }
        }

        public string Color => GuestCount > 0 ? "#DC2626" : "#059669";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}