using System.Windows;
using WpfAppChamber;

namespace WpfAppCSV
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(PoleHasla.Password == "chamber")
            {
                (new MainWindow()).Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Hasło niepoprawne","Error");
            }
        }
    }
}
