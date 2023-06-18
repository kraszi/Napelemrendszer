using Napelemrendszer.Model.EF;
using Napelemrendszer.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Napelemrendszer.View
{
    /// <summary>
    /// Interaction logic for WarehousemanView.xaml
    /// </summary>
    public partial class WarehousemanView : System.Windows.Window
    {
        public WarehousemanView()
        {
            InitializeComponent();
            ComboBoxProjektekFeltoltesW();
        }
        private void ComboBoxProjektekFeltoltesW()
        {
            using (var context = new localNapelemEntities())
            {
                C1projekt_combobox.Items.Clear();
                C2projekt_combobox.Items.Clear();
                C3projekt_combobox.Items.Clear();
                var projects = context.Projets.ToList();
                foreach (var p in projects)
                {
                    string fullName = "ID: " + p.ID + "     Név: " + p.Title + "    Cím: " + p.Address;
                    C1projekt_combobox.Items.Add(fullName);
                    C2projekt_combobox.Items.Add(fullName);
                    C3projekt_combobox.Items.Add(fullName);
                }

            }
        }
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.mainWindow.mainContent.Children.Clear();
            MainWindow.loggedInUser = null;
            LoginView loginView = new LoginView();
            object content = loginView.Content;
            loginView.Content = null;
            loginView.Close();
            MainWindow.mainWindow.mainContent.Children.Add(content as UIElement);
        }

        // 3. Mérföldkő -- Raktáros funkciók C.1 --> START
        // Feladat:
        // Projektek listázása, projekt kiválasztása kivételezéshez, projekt státuszának automatikus beállítása („InProgress”)
        private async void C1kivetelezes_onClick(object sender, RoutedEventArgs e)
        {
            InitializeComponent();
            using (var client = RestHelper.GetRestClient())
            {
                int valasztottidx = C1projekt_combobox.SelectedIndex + 1;
                try
                {
                    if (valasztottidx > 0)
                    {
                        var response = await client.GetAsync("api/Project");
                        var content = await response.Content.ReadAsStringAsync();
                        var projects = JsonConvert.DeserializeObject<List<Projet>>(content);

                        var response2 = await client.GetAsync("api/ComponentsToProject?projectId=" + valasztottidx);
                        var content2 = await response2.Content.ReadAsStringAsync();
                        var ctp = JsonConvert.DeserializeObject<List<ComponentsToProject>>(content2);

                        var response3 = await client.GetAsync("api/Storage");
                        var content3 = await response3.Content.ReadAsStringAsync();
                        var storages = JsonConvert.DeserializeObject<List<Storage>>(content3);

                        var response4 = await client.GetAsync("api/Progress");
                        var content4 = await response4.Content.ReadAsStringAsync();
                        var progress = JsonConvert.DeserializeObject<List<Progress>>(content4);

                        int trueSum = 0;
                        int osszesSum = 0;

                        foreach (var c in ctp)
                        {
                            if (c.ProjectID == valasztottidx)
                            {
                                foreach (var s in storages)
                                {
                                    if (c.ComponentID == s.ID)
                                    {
                                        if (c.Quantity <= s.Quantity)
                                        {
                                            trueSum++;
                                        }
                                        osszesSum++;

                                    }
                                }
                            }
                        }
                        if (trueSum != osszesSum)
                        {
                            System.Windows.MessageBox.Show("Nincs elég alkatrész a raktárban!");
                        }
                        else //levonhato == true
                        {

                            var _New = progress[valasztottidx - 1].New;
                            var _Draft = progress[valasztottidx - 1].Draft;
                            var _Wait = progress[valasztottidx - 1].Wait;
                            var _Scheduled = progress[valasztottidx - 1].Scheduled;
                            var _InProgress = System.DateTime.Now;
                            var _Completed = progress[valasztottidx - 1].Completed;
                            var _Failed = progress[valasztottidx - 1].Failed;

                            var trueLevonhato = new
                            {
                                ID = valasztottidx,
                                New = _New,
                                Draft = _Draft,
                                Wait = _Wait,
                                Scheduled = _Scheduled,
                                InProgress = _InProgress,
                                Completed = _Completed,
                                Failed = _Failed
                            };

                            var megnezem = progress[valasztottidx - 1].InProgress;
                            if (megnezem == null)
                            {
                                if (_Scheduled != null)
                                {
                                    var request6 = new HttpRequestMessage(HttpMethod.Put, "api/Progress?id=" + valasztottidx);
                                    var content6 = new StringContent(JsonConvert.SerializeObject(trueLevonhato), Encoding.UTF8, "application/json");
                                    request6.Content = content6;
                                    var response6 = await client.SendAsync(request6);

                                    foreach (var c in ctp)
                                    {
                                        if (c.ProjectID == valasztottidx)
                                        {
                                            foreach (var s in storages)
                                            {
                                                if (c.ComponentID == s.ID)
                                                {
                                                    if (c.Quantity <= s.Quantity)
                                                    {
                                                        var _id = s.ID;
                                                        var _row = storages[c.ComponentID - 1].Row;
                                                        var _column = storages[c.ComponentID - 1].Column;
                                                        var _shelf = storages[c.ComponentID - 1].Shelf;
                                                        var _cell = storages[c.ComponentID - 1].Cell;
                                                        var _productName = storages[c.ComponentID - 1].ProductName;
                                                        var _price = storages[c.ComponentID - 1].Price;
                                                        var _maxQuantity = storages[c.ComponentID - 1].MaxQuantity;
                                                        var _quantity = (storages[c.ComponentID - 1].Quantity);
                                                        _quantity -= c.Quantity;
                                                        var _reserved = (storages[c.ComponentID - 1].Reserved);
                                                        _reserved -= c.Quantity;


                                                        var storageObject = new
                                                        {
                                                            id = _id,
                                                            row = _row,
                                                            column = _column,
                                                            shelf = _shelf,
                                                            cell = _cell,
                                                            productName = _productName,
                                                            price = _price,
                                                            maxQuantity = _maxQuantity,
                                                            quantity = _quantity,
                                                            reserved = _reserved
                                                        };

                                                        var request1 = new HttpRequestMessage(HttpMethod.Put, "api/Storage?id=" + s.ID);
                                                        var content1 = new StringContent(JsonConvert.SerializeObject(storageObject), Encoding.UTF8, "application/json");
                                                        request1.Content = content1;
                                                        var response1 = await client.SendAsync(request1);
                                                        var status1 = response1.StatusCode;
                                                    }
                                                    else
                                                    {
                                                        System.Windows.MessageBox.Show("Nincs elég alkatrész!");
                                                    }
                                                }
                                            }
                                        }
                                    } //foreach vegEnd

                                    System.Windows.MessageBox.Show("Alkatrészek kivételezve, státusz InProgress-re állítva!");
                                    ComboBoxProjektekFeltoltesW();
                                    C1kivetelezes_onClick(sender, e);
                                }//scheduled vege
                                else
                                {
                                    System.Windows.MessageBox.Show("Projekt még nincs Scheduled állapotban!");
                                }
                            }//inprogress vege
                            else
                            {
                                System.Windows.MessageBox.Show("Projekt már InProgress állapotban van!");
                            }
                        }//levonhato else
                    }//0 vege
                    else
                    {
                        System.Windows.MessageBox.Show("Nincs kiválasztva projekt!");
                    }
                }
                catch (Exception exception)
                {
                    System.Windows.MessageBox.Show("HIBA: " + exception.Message);
                }

            }//using vege

        }
        // 3. Mérföldkő -- Raktáros funkciók C.1 --> END


        // 3. Mérföldkő -- Raktáros funkciók C.2 --> START
        // Feladat:
        // Projekthez tartozó alkatrészek és azok elhelyezkedésének listázása (sor, oszlop, polc, rekesz kezelése)
        private async void C2alkatreszek_onClick(object sender, RoutedEventArgs e)
        {
            InitializeComponent();
            using (var client = RestHelper.GetRestClient())
            {
                int valasztottProjektID = C2projekt_combobox.SelectedIndex + 1;
                if (valasztottProjektID > 0)
                {
                    var response = await client.GetAsync("api/ComponentsToProject?projectId=" + valasztottProjektID);
                    var content = await response.Content.ReadAsStringAsync();
                    var ctpkell = JsonConvert.DeserializeObject<List<ComponentsToProject>>(content);

                    var response2 = await client.GetAsync("api/Storage");
                    var content2 = await response2.Content.ReadAsStringAsync();
                    var stor = JsonConvert.DeserializeObject<List<Storage>>(content2);

                    List<Storage> storkell = new List<Storage>();

                    foreach (var c in ctpkell)
                    {
                        if (c.ProjectID == valasztottProjektID)
                        {
                            foreach (var s in stor)
                            {
                                if (c.ComponentID == s.ID)
                                {
                                    storkell.Add(s);
                                }
                            }
                        }
                    }
                    string status = response.StatusCode.ToString();
                    string status2 = response2.StatusCode.ToString();
                    if (response.IsSuccessStatusCode && response2.IsSuccessStatusCode)
                    {
                        C2component_listView.DataContext = ctpkell;
                        C2storage_listView.DataContext = storkell;
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Hiba: " + status + ", " + status2);

                        return;
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("Nincs kiválasztva projekt!");
                }
            }
        }
        // 3. Mérföldkő -- Raktáros funkciók C.2 --> END


        // 3. Mérföldkő -- Raktáros funkciók C.3 --> START
        // Feladat:
        // Az alkatrészek összegyűjtése során megteendő útvonal optimalizálása
        private async void C3utvonal_onClick(object sender, RoutedEventArgs e)
        {
            InitializeComponent();
            using (var client = RestHelper.GetRestClient())
            {
                int valasztottProjektID = C3projekt_combobox.SelectedIndex + 1;
                if (valasztottProjektID > 0)
                {
                    var response = await client.GetAsync("api/ComponentsToProject?projectId=" + valasztottProjektID);
                    var content = await response.Content.ReadAsStringAsync();
                    var ctpkell = JsonConvert.DeserializeObject<List<ComponentsToProject>>(content);

                    var response2 = await client.GetAsync("api/Storage");
                    var content2 = await response2.Content.ReadAsStringAsync();
                    var stor = JsonConvert.DeserializeObject<List<Storage>>(content2);

                    List<Storage> storkell = new List<Storage>();

                    foreach (var c in ctpkell)
                    {
                        if (c.ProjectID == valasztottProjektID)
                        {
                            foreach (var s in stor)
                            {
                                if (c.ComponentID == s.ID)
                                {
                                    storkell.Add(s);
                                }
                            }
                        }
                    }

                    List<Storage> sorted = storkell.OrderBy(s => s.Cell).ToList();

                    string status = response.StatusCode.ToString();
                    string status2 = response2.StatusCode.ToString();
                    if (response.IsSuccessStatusCode && response2.IsSuccessStatusCode)
                    {
                        C3storage_listView.DataContext = sorted;
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Hiba: " + status + ", " + status2);
                        return;
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("Nincs kiválasztva projekt!");
                }
            }
        }
        // 3. Mérföldkő -- Raktáros funkciók C.3 --> END

    }


}