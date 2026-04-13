using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp1
{
    public partial class CreateWindow : Window
    {
        public Guest NewGuest { get; private set; }

        public CreateWindow()
        {
            InitializeComponent();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameInput.Text) || !int.TryParse(CountInput.Text, out int count))
            {
                MessageBox.Show("Please enter valid data.");
                return;
            }

            NewGuest = new Guest
            {
                Name = NameInput.Text,
                PeopleCount = count,
                ArrivalTime = DateTime.Now.ToString("HH:mm"),
                SeatedTime = "-"
            };

            DialogResult = true; 
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
