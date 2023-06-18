using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using Napelemrendszer.Utilities;
using Napelemrendszer.Model.EF;
using Napelemrendszer.Model;

namespace Napelemrendszer.View
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            // Retrieve the username and password from the textboxes on the UI
            string username = userName_textbox.Text;
            string password = password_passwordbox.Password;
            SHA256 sha256 = SHA256.Create();
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] hashBytes = sha256.ComputeHash(passwordBytes);
            string passwordHash = Convert.ToBase64String(hashBytes);

            // Create an HttpClient instance to send the request
            using (var client = RestHelper.GetRestClient())
            {
                try
                {
                    var user = new
                    {
                        username = username,
                        password = passwordHash,
                    };

                    var json = JsonConvert.SerializeObject(user);
                    var response = await client.PostAsync($"api/Employee/login", new StringContent(json, Encoding.UTF8, "application/json"));

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        var loginResponse = JsonConvert.DeserializeObject<Employee>(responseBody);

                        if (loginResponse.UserType == Usertype.Specialist.ToString())
                        {
                            //MessageBox.Show("Login successful! Welcome " + loginResponse.Name + ", you logged in as a SPECIALIST");
                            MainWindow.loggedInUser = loginResponse;
                            MainWindow.mainWindow.mainContent.Children.Clear();
                            SpecialistView specialistView = new SpecialistView();
                            object content = specialistView.Content;
                            specialistView.Content = null;
                            specialistView.Close();
                            MainWindow.mainWindow.mainContent.Children.Add(content as UIElement);
                        }
                        else if (loginResponse.UserType == Usertype.Manager.ToString())
                        {
                            //MessageBox.Show("Login successful! Welcome " + loginResponse.Name + ", you logged in as a MANAGER");
                            MainWindow.loggedInUser = loginResponse;
                            MainWindow.mainWindow.mainContent.Children.Clear();
                            ManagerView managerView = new ManagerView();
                            object content = managerView.Content;
                            managerView.Content = null;
                            managerView.Close();
                            MainWindow.mainWindow.mainContent.Children.Add(content as UIElement);
                        }
                        else if (loginResponse.UserType == Usertype.Warehouseman.ToString())
                        {
                            //MessageBox.Show("Login successful! Welcome " + loginResponse.Name + ", you logged in as a WAREHOUSEMAN");
                            MainWindow.loggedInUser = loginResponse;
                            MainWindow.mainWindow.mainContent.Children.Clear();
                            WarehousemanView warehousemanView = new WarehousemanView();
                            object content = warehousemanView.Content;
                            warehousemanView.Content = null;
                            warehousemanView.Close();
                            MainWindow.mainWindow.mainContent.Children.Add(content as UIElement);
                        }

                    }
                    else
                    {
                        MessageBox.Show("Login failed. Please check your username and password.");
                    }
                }
                catch (Exception excp)
                {
                    MessageBox.Show("Exception: " + excp.Message);
                }
            }
        }

    }
}
