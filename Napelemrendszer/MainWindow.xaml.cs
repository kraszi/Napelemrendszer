using Napelemrendszer.Model;
using Napelemrendszer.Model.EF;
using Napelemrendszer.View;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Napelemrendszer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow mainWindow;
        public static Employee loggedInUser;

        public MainWindow()
        {
            mainWindow = this;
            InitializeComponent();
            LoginView loginView = new LoginView();
            object content = loginView.Content;
            loginView.Content = null;
            loginView.Close();
            mainContent.Children.Add(content as UIElement);
        }
        // 2023.06.01. állapot
    }
}
