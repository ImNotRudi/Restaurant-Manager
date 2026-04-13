using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class Guest : System.ComponentModel.INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int PeopleCount { get; set; }
        public int? TableNumber { get; set; }
        public string SeatedTime { get; set; } = "";
        public string ArrivalTime { get; set; } = "";
        public bool IsSeated => TableNumber.HasValue && string.IsNullOrEmpty(CheckoutTime);
        public bool IsCompleted => !string.IsNullOrEmpty(CheckoutTime);
        private string _checkoutTime = "";
        public string CheckoutTime
        {
            get => _checkoutTime;
            set { _checkoutTime = value; OnPropertyChanged(nameof(CheckoutTime)); }
        }
        public DateTime RecordDate { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }
}
