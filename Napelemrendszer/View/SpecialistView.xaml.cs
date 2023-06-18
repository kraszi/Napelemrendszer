using Napelemrendszer.Model.EF;
using Napelemrendszer.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
using System.Net.Http;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using Napelemrendszer.Utilities;
using Newtonsoft.Json;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Data.SqlTypes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Napelemrendszer.View
{
    /// <summary>
    /// Interaction logic for SpecialistView.xaml
    /// </summary>
    public partial class SpecialistView : System.Windows.Window
    {
        public SpecialistView()
        {
            InitializeComponent();
            ComboBoxUpgrade();
        }
        private void ComboBoxUpgrade()
        {
            using (var context = new localNapelemEntities())
            {
                A4alkatresz_combobox.Items.Clear();
                A4projekt_combobox.Items.Clear();
                A5projekt_combobox.Items.Clear();
                A6projekt_combobox.Items.Clear();
                A7projekt_combobox.Items.Clear();

                var projects = context.Projets.ToList();
                foreach (var p in projects)
                {
                    string fullName = "ID: " + p.ID + "     Név: " + p.Title + "    Cím: " + p.Address;
                    A4projekt_combobox.Items.Add(fullName);
                    A5projekt_combobox.Items.Add(fullName);
                    A6projekt_combobox.Items.Add(fullName);
                    A7projekt_combobox.Items.Add(fullName);
                }

                var storages = context.Storages.ToList();
                foreach (var p in storages)
                {
                    string fullName = "ID: " + p.ID + "     Név: " + p.ProductName + "    Ár: " + p.Price;
                    A4alkatresz_combobox.Items.Add(fullName);
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

        // 2. Mérföldkő -- Szakember funkciók A.1 --> START
        // Feladat:
        // Új projekt létrehozása („New”), helyszín, leírás megadása, megrendelő adatok rögzítése
        public async void ujProjekt_onClick(object sender, RoutedEventArgs e)
        {
            int current_empID = MainWindow.loggedInUser.ID;
            int empId = current_empID;
            var helyszin = helyszin_textbox.Text;
            var leiras = leiras_textbox.Text;
            if (helyszin != "" || leiras != "")
            {
                using (var client = RestHelper.GetRestClient())
                {
                    var ujProjekt = new
                    {
                        employeeId = empId,
                        address = helyszin,
                        title = leiras
                    };

                    var sum = 0;
                    List<Projet> listam = new List<Projet>();
                    foreach (var p in listam)
                    {
                        sum++;
                    }

                    var ujProgress = new
                    {
                        ProjectID = (sum + 1),
                        New = System.DateTime.Now
                    };


                    var json = JsonConvert.SerializeObject(ujProjekt);
                    var response = await client.PostAsync("api/Project", new StringContent(json, Encoding.UTF8, "application/json"));
                    string status = response.StatusCode.ToString();

                    var json2 = JsonConvert.SerializeObject(ujProgress);
                    var response2 = await client.PostAsync("api/Progress", new StringContent(json2, Encoding.UTF8, "application/json"));
                    string status2 = response2.StatusCode.ToString();

                    if (response.IsSuccessStatusCode && response2.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Új projekt létrehozva.");
                        leiras_textbox.Clear();
                        helyszin_textbox.Clear();
                        ComboBoxUpgrade();
                        menuProjektekListazasa_onClick(null, null);
                    }
                    else
                    {
                        MessageBox.Show("Hiba: " + status);
                        leiras_textbox.Clear();
                        helyszin_textbox.Clear();
                        return;
                    }

                }
            }//ellenorzes vege
            else
            {
                MessageBox.Show("Valami nincs megadva!");
            }
        }
        // 2. Mérföldkő -- Szakember funkciók A.1 --> END


        // 2. Mérföldkő -- Szakember funkciók A.2 --> START
        // Feladat:
        // A projektek listázása, állapotok ellenőrzése
        public async void menuProjektekListazasa_onClick(object sender, RoutedEventArgs e)
        {
            InitializeComponent();
            using (var client = RestHelper.GetRestClient())
            {
                var response = await client.GetAsync("api/Project");
                var content = await response.Content.ReadAsStringAsync();
                var projects = JsonConvert.DeserializeObject<List<Projet>>(content);

                var response2 = await client.GetAsync("api/Progress");
                var content2 = await response2.Content.ReadAsStringAsync();
                var progress = JsonConvert.DeserializeObject<List<Progress>>(content2);

                projektekListView.DataContext = projects;
                progressListView.DataContext = progress;
            }

        }
      
        // 2. Mérföldkő -- Szakember funkciók A.2 --> END


        // 2. Mérföldkő -- Szakember funkciók A.3 --> START
        // Feladat:
        // Alkatrészek listázása, azok árának, elérhetőségének ellenőrzése
        public async void menuAlkatreszek_onClick(object sender, RoutedEventArgs e)
        {
            InitializeComponent();
            using (var client = RestHelper.GetRestClient())
            {
                var response = await client.GetAsync("api/Storage");
                var content = await response.Content.ReadAsStringAsync();
                var storages = JsonConvert.DeserializeObject<List<Storage>>(content);


                alkatreszekListView.DataContext = storages;
            }

        }
        // 2. Mérföldkő -- Szakember funkciók A.3 --> END


        // 2. Mérföldkő -- Szakember funkciók A.4 --> START
        // Feladat:
        // Kiválasztott alkatrészek projekthez rendelése („Draft”)
        private async void A4hozzarendeles_onClick(object sender, RoutedEventArgs e)
        {
           using (var client = RestHelper.GetRestClient())
            {
                int valasztottProjektID = A4projekt_combobox.SelectedIndex + 1;

                int valasztottAlkatreszID = A4alkatresz_combobox.SelectedIndex + 1;

                if (A4mennyiseg_textbox.Text != "")
                {
                    if (valasztottProjektID > 0 && valasztottAlkatreszID > 0)
                        {

                            int mennyiseg = int.Parse(A4mennyiseg_textbox.Text);


                        var componentObject = new
                        {
                            componentID = valasztottAlkatreszID,
                            quantity = mennyiseg,
                            projectID = valasztottProjektID
                        };


                        var response3 = await client.GetAsync("api/Progress");
                        var content3 = await response3.Content.ReadAsStringAsync();
                        var progr = JsonConvert.DeserializeObject<List<Progress>>(content3);

                        var _New = progr[valasztottProjektID - 1].New;
                        var _Draft = System.DateTime.Now;
                        var _Wait = progr[valasztottProjektID - 1].Wait;
                        var _Scheduled = progr[valasztottProjektID - 1].Scheduled;
                        var _InProgress = progr[valasztottProjektID - 1].InProgress;
                        var _Completed = progr[valasztottProjektID - 1].Completed;
                        var _Failed = progr[valasztottProjektID - 1].Failed;

                        var ujProgress = new
                        {
                            ID = valasztottProjektID,
                            New = _New,
                            Draft = _Draft,
                            Wait = _Wait,
                            Scheduled = _Scheduled,
                            InProgress = _InProgress,
                            Completed = _Completed,
                            Failed = _Failed
                        };

                            if (_Failed == null)
                            {
                                if (mennyiseg > 0)
                                {
                                    System.Windows.Forms.DialogResult res = (DialogResult)MessageBox.Show("Biztosan hozzárendeli?", "Mentés", (MessageBoxButton)MessageBoxButtons.OKCancel, (MessageBoxImage)MessageBoxIcon.Information);
                                    if (res == System.Windows.Forms.DialogResult.OK)
                                    {
                                        var request = new HttpRequestMessage(HttpMethod.Put, "api/Progress?id=" + valasztottProjektID);
                                        var content1 = new StringContent(JsonConvert.SerializeObject(ujProgress), Encoding.UTF8, "application/json");
                                        request.Content = content1;
                                        var response1 = await client.SendAsync(request);


                                        var json = JsonConvert.SerializeObject(componentObject);
                                        var response2 = await client.PostAsync("api/ComponentsToProject", new StringContent(json, Encoding.UTF8, "application/json"));


                                        var response6 = await client.GetAsync("api/Storage");
                                        var content6 = await response6.Content.ReadAsStringAsync();
                                        var storages = JsonConvert.DeserializeObject<List<Storage>>(content6);


                                        var _row = storages[valasztottAlkatreszID - 1].Row;
                                        var _column = storages[valasztottAlkatreszID - 1].Column;
                                        var _shelf = storages[valasztottAlkatreszID - 1].Shelf;
                                        var _cell = storages[valasztottAlkatreszID - 1].Cell;
                                        var _productName = storages[valasztottAlkatreszID - 1].ProductName;
                                        var _price = storages[valasztottAlkatreszID - 1].Price;
                                        var _maxQuantity = storages[valasztottAlkatreszID - 1].MaxQuantity;
                                        var _quantity = storages[valasztottAlkatreszID - 1].Quantity;
                                        var _reserved = storages[valasztottAlkatreszID - 1].Reserved;
                                        if (_reserved == null) { _reserved = 0; }
                                        _reserved += mennyiseg;

                                        var storageObject = new
                                        {
                                            id = valasztottAlkatreszID,
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

                                        var request7 = new HttpRequestMessage(HttpMethod.Put, "api/Storage?id=" + valasztottAlkatreszID);
                                        var content7 = new StringContent(JsonConvert.SerializeObject(storageObject), Encoding.UTF8, "application/json");
                                        request7.Content = content7;
                                        var response7 = await client.SendAsync(request7);

                                        if (response7.IsSuccessStatusCode)
                                        {
                                            MessageBox.Show("Alkatrész sikeresen hozzárendelve!");
                                            A4mennyiseg_textbox.Clear();
                                            ComboBoxUpgrade();
                                            menuProjektekListazasa_onClick(null, null);
                                            menuAlkatreszek_onClick(null, null);
                                        }
                                        else
                                        {
                                            MessageBox.Show("Hiba.");
                                            A4mennyiseg_textbox.Clear();
                                            ComboBoxUpgrade();
                                            return;
                                        }
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Hiba: mennyiség 0");
                                }

                            }//failed
                            else
                            {
                                MessageBox.Show("Projekt már lezárva, mint Failed!");
                            }
                        }//indexes
                        else
                        {
                            string message = "Nincs kiválasztva projekt vagy alkatrész!";
                            MessageBox.Show(message, "Nincs kiválasztva projekt vagy alaktrész!");
                        }
                    }//mennyiseg
                    else
                    {
                        MessageBox.Show("Nincs mennyiség megadva!");
                    }
                }//using 

        }
        // 2. Mérföldkő -- Szakember funkciók A.4 --> END


        // 2. Mérföldkő -- Szakember funkciók A.5 --> START
        // Feladat:
        // Becsült munkavégzési idő rögzítése, munkadíj meghatározása
        private async void munkaIdo_onClick(object sender, RoutedEventArgs e) {
            
            using (var client = RestHelper.GetRestClient()) {

                int valasztottidx = A5projekt_combobox.SelectedIndex +1;
                if (valasztottidx > 0)
                {
                    var response = await client.GetAsync("api/Project");
                    var content = await response.Content.ReadAsStringAsync();
                    var projects = JsonConvert.DeserializeObject<List<Projet>>(content);

                    var _EmployeeID = projects[valasztottidx - 1].EmployeeID;
                    var _ProgressID = projects[valasztottidx - 1].ProgressID;
                    var _Title = projects[valasztottidx - 1].Title;
                    var _Address = projects[valasztottidx - 1].Address;
                    var _ComponentsID = projects[valasztottidx - 1].ComponentsID;
                    var _WorkPrice = int.Parse(munkaDij_textbox.Text);
                    var _FullPrice = projects[valasztottidx - 1].FullPrice;
                    var _RequiredTime = int.Parse(munkaIdo_textbox.Text);

                    var munkaido = new
                    {
                        ID = valasztottidx,
                        EmployeeID = _EmployeeID,
                        ProgressID = _ProgressID,
                        Title = _Title,
                        Address = _Address,
                        ComponentsID = _ComponentsID,
                        WorkPrice = _WorkPrice,
                        FullPrice = _FullPrice,
                        RequiredTime = _RequiredTime
                    };

                
                    if (munkaIdo_textbox.Text != "" && munkaDij_textbox.Text != "")
                    {
                        System.Windows.Forms.DialogResult res = (DialogResult)MessageBox.Show("Biztosan menti?", "Mentés", (MessageBoxButton)MessageBoxButtons.OKCancel, (MessageBoxImage)MessageBoxIcon.Information);
                        if (res == System.Windows.Forms.DialogResult.OK)
                        {
                            var request = new HttpRequestMessage(HttpMethod.Put, "api/Project?id=" + valasztottidx);
                            var content1 = new StringContent(JsonConvert.SerializeObject(munkaido), Encoding.UTF8, "application/json");
                            request.Content = content1;
                            var response1 = await client.SendAsync(request);
                            var status = response1.StatusCode;
                            if (status.ToString() == "NoContent")
                            {
                                MessageBox.Show("Munkadíj megadva.");

                                munkaIdo_textbox.Clear();
                                munkaDij_textbox.Clear();
                                ComboBoxUpgrade();
                                menuProjektekListazasa_onClick(sender, e);
                            }
                            else
                            {
                                MessageBox.Show("Error. " + status.ToString());
                            }
                            if (res == System.Windows.Forms.DialogResult.Cancel)
                            {
                                this.Close();
                            }
                        }
                        }
                        else
                        {
                            string message = "Nincs valami megadva!";
                            MessageBox.Show(message, "Nincs valami megadva!");
                        }

                    }
                    else
                    {
                        string message = "Nincs kiválasztva projekt!";
                        MessageBox.Show(message, "Nincs kiválasztva projekt!");
                    }
                }
        }
        // 2. Mérföldkő -- Szakember funkciók A.5 --> END


        // 3. Mérföldkő -- Szakember funkciók A.6 --> START
        // Feladat:
        // Árkalkuláció elkészítése, ha minden alkatrész elérhető a raktárban („Wait”, „Scheduled”)
        private async void A6arkalkulacio_onClick(object sender, RoutedEventArgs e)
        {
            using (var client = RestHelper.GetRestClient())
            {
                int valasztottidx = A6projekt_combobox.SelectedIndex + 1;

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

                int sum = 0;
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
                                int mennyiseg = (int)c.Quantity;
                                int ar = (int)s.Price;
                                sum += (mennyiseg * ar);
                            }
                        }
                    }
                }
                if (trueSum != osszesSum)
                {
                    var _New = progress[valasztottidx - 1].New;
                    var _Draft = progress[valasztottidx - 1].Draft;
                    var _Wait = System.DateTime.Now;
                    var _Scheduled = progress[valasztottidx - 1].Scheduled;
                    var _InProgress = progress[valasztottidx - 1].InProgress;
                    var _Completed = progress[valasztottidx - 1].Completed;
                    var _Failed = progress[valasztottidx - 1].Failed;

                    var falseLevonhato = new
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
                    if (_Failed == null)
                    {
                        if (_Wait != null)
                        {
                            var request = new HttpRequestMessage(HttpMethod.Put, "api/Progress?id=" + valasztottidx);
                            var content5 = new StringContent(JsonConvert.SerializeObject(falseLevonhato), Encoding.UTF8, "application/json");
                            request.Content = content5;
                            var response5 = await client.SendAsync(request);
                            var status5 = response5.StatusCode;

                            if (response5.IsSuccessStatusCode)
                            {
                                MessageBox.Show("Nincs minden alkatrész raktáron, ezért a projekt állapota Wait lett.");
                                A6projekt_combobox.Items.Clear();
                                ComboBoxUpgrade();
                                menuProjektekListazasa_onClick(sender, e);
                            }
                            else
                            {
                                MessageBox.Show("Hiba: " + status5);
                                A6projekt_combobox.Items.Clear();
                                ComboBoxUpgrade();
                                menuProjektekListazasa_onClick(sender, e);
                                return;
                            }
                        }//wait
                        else
                        {
                            MessageBox.Show("Projekt még mindig Wait állapotban!");
                        }
                    }//failed
                    else
                    {
                        MessageBox.Show("Projekt már lezárva, mint Failed!");
                    }
                }

                else //levonhato == true
                {
                    var _New = progress[valasztottidx - 1].New;
                    var _Draft = progress[valasztottidx - 1].Draft;
                    var _Wait = progress[valasztottidx - 1].Wait;
                    var _Scheduled = System.DateTime.Now;
                    var _InProgress = progress[valasztottidx - 1].InProgress;
                    var _Completed = progress[valasztottidx - 1].Completed;
                    var _Failed = progress[valasztottidx - 1].Failed;

                    var falseLevonhato = new
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
                    if (_Failed == null) 
                    { 
                        if (_Scheduled != null)
                        {
                            var request6 = new HttpRequestMessage(HttpMethod.Put, "api/Progress?id=" + valasztottidx);
                            var content6 = new StringContent(JsonConvert.SerializeObject(falseLevonhato), Encoding.UTF8, "application/json");
                            request6.Content = content6;
                            var response6 = await client.SendAsync(request6);
                            var status6 = response6.StatusCode;

                            if (response2.IsSuccessStatusCode)
                            {
                                MessageBox.Show("Minden alkatrész raktáron, ezért a projekt állapota Scheduled lett.");
                                A6projekt_combobox.Items.Clear();
                                ComboBoxUpgrade();
                                menuProjektekListazasa_onClick(sender, e);
                            }
                            else
                            {
                                MessageBox.Show("Hiba: " + status6);
                                A6projekt_combobox.Items.Clear();
                                ComboBoxUpgrade();
                                menuProjektekListazasa_onClick(sender, e);
                                return;
                            }



                            var _EmployeeID = projects[valasztottidx - 1].EmployeeID;
                            var _ProgressID = projects[valasztottidx - 1].ProgressID;
                            var _Title = projects[valasztottidx - 1].Title;
                            var _Address = projects[valasztottidx - 1].Address;
                            var _ComponentsID = projects[valasztottidx - 1].ComponentsID;
                            var _WorkPrice = projects[valasztottidx - 1].WorkPrice;
                            sum += (int)_WorkPrice;
                            var _FullPrice = sum;
                            var _RequiredTime = projects[valasztottidx - 1].RequiredTime;

                            var arkalkulacio = new
                            {
                                ID = valasztottidx,
                                EmployeeID = _EmployeeID,
                                ProgressID = _ProgressID,
                                Title = _Title,
                                Address = _Address,
                                ComponentsID = _ComponentsID,
                                WorkPrice = _WorkPrice,
                                FullPrice = _FullPrice,
                                RequiredTime = _RequiredTime
                            };
                            if (valasztottidx > 0)
                            {

                                System.Windows.Forms.DialogResult res = (DialogResult)MessageBox.Show("Biztosan menti?", "Mentés", (MessageBoxButton)MessageBoxButtons.OKCancel, (MessageBoxImage)MessageBoxIcon.Information);
                                if (res == System.Windows.Forms.DialogResult.OK)
                                {
                                    var request = new HttpRequestMessage(HttpMethod.Put, "api/Project?id=" + valasztottidx);
                                    var content1 = new StringContent(JsonConvert.SerializeObject(arkalkulacio), Encoding.UTF8, "application/json");
                                    request.Content = content1;
                                    var response1 = await client.SendAsync(request);
                                    var status = response1.StatusCode;
                                    if (status.ToString() == "NoContent")
                                    {
                                        MessageBox.Show("Árkalkuláció kiszámítva: " + sum);
                                        A6projekt_combobox.Items.Clear();
                                        ComboBoxUpgrade();
                                        menuProjektekListazasa_onClick(sender, e);
                                    }
                                    else
                                    {
                                        MessageBox.Show("Error. " + status.ToString());
                                    }
                                    if (res == System.Windows.Forms.DialogResult.Cancel)
                                    {
                                        this.Close();
                                    }

                                }

                            }//ellenorzes
                            else
                            {
                                string message = "Nincs kiválasztva projekt!";
                                MessageBox.Show(message, "Nincs kiválasztva projekt!");
                            }

                        }//scheduled
                        else
                        {
                            MessageBox.Show("Projekt már Scheduled állapotban van!");
                        }
                    }//failed
                    else
                    {
                        MessageBox.Show("Projekt már lezárva, mint Failed!");
                    }
                }
            }
        }

        // 3. Mérföldkő -- Szakember funkciók A.6 --> END


        // 3. Mérföldkő -- Szakember funkciók A.7 --> START
        // Feladat:
        // Projekt lezárása ( „Completed”, „Failed” )
        private async void A7complete_onClick(object sender, RoutedEventArgs e)
        {
            using (var client = RestHelper.GetRestClient())
            {
                int valasztottProjektID = A7projekt_combobox.SelectedIndex + 1;
                var response3 = await client.GetAsync("api/Progress");
                var content3 = await response3.Content.ReadAsStringAsync();
                var progr = JsonConvert.DeserializeObject<List<Progress>>(content3);

                var completeTest = progr[valasztottProjektID - 1].Completed;

                if (completeTest == null)
                {
                    var _New = progr[valasztottProjektID - 1].New;
                    var _Draft = progr[valasztottProjektID - 1].Draft;
                    var _Wait = progr[valasztottProjektID - 1].Wait;
                    var _Scheduled = progr[valasztottProjektID - 1].Scheduled;
                    var _InProgress = progr[valasztottProjektID - 1].InProgress;
                    var _Completed = System.DateTime.Now;
                    var _Failed = progr[valasztottProjektID - 1].Failed;

                    var ujProgress = new
                    {
                        ID = valasztottProjektID,
                        New = _New,
                        Draft = _Draft,
                        Wait = _Wait,
                        Scheduled = _Scheduled,
                        InProgress = _InProgress,
                        Completed = _Completed,
                        Failed = _Failed
                    };

                    if (_Failed == null)
                    {
                        if (_InProgress != null)
                        {
                            if (valasztottProjektID > 0)
                            {
                                MessageBoxResult result = MessageBox.Show("Valóban lezárja, mint Completed?", "Completed Projekt", MessageBoxButton.YesNo);

                                if (result == MessageBoxResult.Yes)
                                {
                                    var request = new HttpRequestMessage(HttpMethod.Put, "api/Progress?id=" + valasztottProjektID);
                                    var content1 = new StringContent(JsonConvert.SerializeObject(ujProgress), Encoding.UTF8, "application/json");
                                    request.Content = content1;
                                    var response1 = await client.SendAsync(request);
                                    var status = response1.StatusCode;
                                    if (status.ToString() == "NoContent")
                                    {
                                        MessageBox.Show("Lezárva (Completed)");

                                        ComboBoxUpgrade();
                                        menuProjektekListazasa_onClick(sender, e);
                                    }
                                    else
                                    {
                                        MessageBox.Show("Error. " + status.ToString());
                                    }

                                }
                                else
                                {
                                    this.Close();
                                }
                            }
                            else
                            {
                                MessageBox.Show("Nincs projekt kiválasztva!");
                            }

                        }
                        else
                        {
                            MessageBox.Show("Nem lezárható! (állapot nem InProgress)");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Projekt már lezárva, mint Failed.");
                    }
                }
                else
                {
                    MessageBox.Show("Projekt már lezárva, mint Completed.");
                }
            }
        }

        private async void A7fail_onClick(object sender, RoutedEventArgs e)
        {
            using (var client = RestHelper.GetRestClient())
            {
                int valasztottProjektID = A7projekt_combobox.SelectedIndex + 1;
                var response3 = await client.GetAsync("api/Progress");
                var content3 = await response3.Content.ReadAsStringAsync();
                var progr = JsonConvert.DeserializeObject<List<Progress>>(content3);

                var failedTest = progr[valasztottProjektID - 1].Failed;

                if (failedTest == null)
                {
                    var _New = progr[valasztottProjektID - 1].New;
                    var _Draft = progr[valasztottProjektID - 1].Draft;
                    var _Wait = progr[valasztottProjektID - 1].Wait;
                    var _Scheduled = progr[valasztottProjektID - 1].Scheduled;
                    var _InProgress = progr[valasztottProjektID - 1].InProgress;
                    var _Completed = progr[valasztottProjektID - 1].Completed;
                    var _Failed = System.DateTime.Now;

                    var ujProgress = new
                    {
                        ID = valasztottProjektID,
                        New = _New,
                        Draft = _Draft,
                        Wait = _Wait,
                        Scheduled = _Scheduled,
                        InProgress = _InProgress,
                        Completed = _Completed,
                        Failed = _Failed
                    };


                    if (_Completed == null)
                    {
                        if (valasztottProjektID > 0)
                        {
                            MessageBoxResult result = MessageBox.Show("Valóban lezárja, mint Failed?", "Failed Projekt", MessageBoxButton.YesNo);

                            if (result == MessageBoxResult.Yes)
                            {
                                var request = new HttpRequestMessage(HttpMethod.Put, "api/Progress?id=" + valasztottProjektID);
                                var content1 = new StringContent(JsonConvert.SerializeObject(ujProgress), Encoding.UTF8, "application/json");
                                request.Content = content1;
                                var response1 = await client.SendAsync(request);
                                var status = response1.StatusCode;
                                if (status.ToString() == "NoContent")
                                {
                                    MessageBox.Show("Lezárva (Failed)");

                                    ComboBoxUpgrade();
                                    menuProjektekListazasa_onClick(sender, e);
                                }
                                else
                                {
                                    MessageBox.Show("Error. " + status.ToString());
                                }

                            }
                            else
                            {
                                this.Close();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Nincs projekt kiválasztva!");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Projekt már lezárva, mint Completed!");
                    }
                }
                else
                {
                    MessageBox.Show("Projekt már lezárva, mint Failed!");
                }
            }
        }

        private void alkatreszekListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        // 3. Mérföldkő -- Szakember funkciók A.7 --> END

    }
}