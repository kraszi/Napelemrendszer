using Napelemrendszer.Model.EF;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Linq;
using System.Reflection;
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
using MessageBox = System.Windows.MessageBox;

using System.Net.Http;
using Napelemrendszer.Utilities;
using Newtonsoft.Json;
using Napelemrendszer.ViewModel;

namespace Napelemrendszer.View
{
    /// <summary>
    /// Interaction logic for ManagerView.xaml
    /// </summary>
    public partial class ManagerView : Window
    {

        public ManagerView()
        {
            InitializeComponent();
            ComboBoxesUpdate();
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

        // 1. Mérföldkő -- Raktárvezető funkciók B.1 --> START
        // Feladat:
        // Új alkatrészek felvétele a rendszerbe (név, ár, rekeszenként a maximálisan elhelyezhető darabszám)
        public async void ujFelvetel_onClick(object sender, RoutedEventArgs e)
        {
            int _row = int.Parse(sor_textbox.Text);
            int _column = int.Parse(oszlop_textbox.Text);
            int _shelf = int.Parse(szint_textbox.Text);
            var _name = nev_textbox.Text;
            int _max = int.Parse(maxdarab_textbox.Text);
            int _cell = ((_row - 1) * 24 + (_column - 1) * 6 + _shelf);
            int _price = int.Parse(ar_textbox.Text);

            using (var httpClient = RestHelper.GetRestClient())
            {
                var response = await httpClient.GetAsync("api/Storage");
                var responseContent = await response.Content.ReadAsStringAsync();
                var alkatreszek = JsonConvert.DeserializeObject<List<Storage>>(responseContent);
                var selecteditem = alkatreszek.Any(item => item.Cell == _cell);
                if (selecteditem)
                {
                    string message = "A(z) " + _cell.ToString() + ". rekeszben már van alkatrész!";
                    MessageBox.Show(message, "Foglalt rekesz");
                }
                else
                {
                    var ujStorage = new
                    {
                        row = _row,
                        column = _column,
                        shelf = _shelf,
                        productName = _name,
                        maxQuantity = _max,
                        cell = _cell,
                        price = _price
                    };
                    var json = JsonConvert.SerializeObject(ujStorage);
                    var response1 = await httpClient.PostAsync("api/Storage", new StringContent(json, Encoding.UTF8, "application/json"));
                    string status = response1.StatusCode.ToString();
                    if (response1.IsSuccessStatusCode)
                    {
                        // Item created successfully
                        MessageBox.Show("Új alkatrész hozzáadva!");
                        sor_textbox.Clear();
                        oszlop_textbox.Clear();
                        szint_textbox.Clear();
                        nev_textbox.Clear();
                        maxdarab_textbox.Clear();
                        ar_textbox.Clear();
                    }
                    else
                    {
                        MessageBox.Show("Hiba: " + status);
                        return;
                    }
                }
            }
            ComboBoxesUpdate();
        }
        // 1. Mérföldkő -- Raktárvezető funkciók B.1 --> END


        // 1. Mérföldkő -- Raktárvezető funkciók B.2 --> START
        // Feladat:
        // Árak módosítása
        private void ArModositasComboBoxUpdate()
        {
            if (ar_combobox != null)
            {
                ar_combobox.Items.Clear();
            }

            using (var context = new localNapelemEntities())
            {
                var storages = context.Storages.ToList();
                foreach (var stor in storages)
                {
                    string fullName = "Név: " + stor.ProductName + "     Ár: " + stor.Price + " Ft";
                    ar_combobox.Items.Add(fullName);
                }
            }
        }
        public async void arModositas_onClick(object sender, RoutedEventArgs e)
        {
            using (var client = RestHelper.GetRestClient())
            {
                int valasztottIdx = ar_combobox.SelectedIndex + 1;
                try
                {
                    var response = await client.GetAsync("api/Storage");
                    var content = await response.Content.ReadAsStringAsync();
                    var storages = JsonConvert.DeserializeObject<List<Storage>>(content);

                    var _row = storages[valasztottIdx - 1].Row;
                    var _column = storages[valasztottIdx - 1].Column;
                    var _shelf = storages[valasztottIdx - 1].Shelf;
                    var _cell = storages[valasztottIdx - 1].Cell;
                    var _productName = storages[valasztottIdx - 1].ProductName;
                    int _price = int.Parse(ujar_textbox.Text);
                    var _maxQuantity = storages[valasztottIdx - 1].MaxQuantity;
                    var _quantity = storages[valasztottIdx - 1].Quantity;
                    var _reserved = storages[valasztottIdx - 1].Reserved;

                    var storageObject = new
                    {
                        id = valasztottIdx,
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

                    if (valasztottIdx > 0)
                    {
                        if (ujar_textbox.Text != "")
                        {
                            System.Windows.Forms.DialogResult res = (DialogResult)MessageBox.Show("Biztosan módosítja az árat?", "Ár módosítása", (MessageBoxButton)MessageBoxButtons.OKCancel, (MessageBoxImage)MessageBoxIcon.Information);
                            if (res == System.Windows.Forms.DialogResult.OK)
                            {
                                var request = new HttpRequestMessage(HttpMethod.Put, "api/Storage?id=" + valasztottIdx);
                                var content1 = new StringContent(JsonConvert.SerializeObject(storageObject), Encoding.UTF8, "application/json");
                                request.Content = content1;
                                var response1 = await client.SendAsync(request);
                                var status = response1.StatusCode;
                                if (status.ToString() == "NoContent")
                                {
                                    MessageBox.Show("Alkatrész ármódosítása sikeres.");
                                    ujar_textbox.Clear();
                                    ar_combobox.Items.Clear();
                                }
                                else
                                {
                                    MessageBox.Show("Hiba: " + status.ToString());
                                }

                            }
                            if (res == System.Windows.Forms.DialogResult.Cancel)
                            {
                                this.Close();
                            }
                        }
                        else
                        {
                            string message = "Nincs új ár megadva!";
                            MessageBox.Show(message, "Nincs új ár megadva!");
                        }

                    }
                    else
                    {
                        string message = "Nincs kiválasztva alkatrész!";
                        MessageBox.Show(message, "Nincs kiválasztva alkatrész!");
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show("HIBA: " + exception.Message);
                }
            }
            ComboBoxesUpdate();
        }
        // 1. Mérföldkő -- Raktárvezető funkciók B.2 --> END


        // 2. Mérföldkő -- Raktárvezető funkciók B.5 --> START
        // Feladat:
        // Beérkező anyagok felvétele a rendszerben, a tároláshoz a rekesz meghatározása
        private void BeerkezoFelvetelComboBoxUpdate()
        {
            if (beerkezo_nev_combobox != null)
            {
                beerkezo_nev_combobox.Items.Clear();
            }

            using (var context = new localNapelemEntities())
            {
                var storages = context.Storages.ToList();

                foreach (var stor in storages)
                {
                    string fullName = "Név: " + stor.ProductName + "     Sor: " + stor.Row + " Oszlop: " + stor.Column + " Szint: " + stor.Shelf + "     Mennyiség: " + stor.Quantity + "/" + stor.MaxQuantity;
                    beerkezo_nev_combobox.Items.Add(fullName);
                }
            }
        }

        private void beerkezoFelvetel_onClick(object sender, RoutedEventArgs e)
        {
            using (var context = new localNapelemEntities())
            {

                int valasztottIdx = beerkezo_nev_combobox.SelectedIndex + 1;
                int szam = int.Parse(beerkezo_darab_textbox.Text);

                var product = context.Storages.FirstOrDefault(p => p.ID == valasztottIdx);

                if (valasztottIdx > 0)
                {
                    if (beerkezo_darab_textbox.Text != "")
                    {
                        if (product.Quantity == null)
                        {
                            product.Quantity = 0;
                        }
                        if (product.Quantity + szam > product.MaxQuantity)
                        {
                            string message = "Nincs hely!";
                            MessageBox.Show(message, "Nincs hely!");

                        }
                        else
                        {
                            product.Quantity += szam;
                            context.SaveChanges();

                            beerkezo_darab_textbox.Clear();
                            beerkezo_nev_combobox.Items.Clear();
                            {
                                var storages = context.Storages.ToList();

                                foreach (var stor in storages)
                                {
                                    string fullName = "Név: " + stor.ProductName + "     Sor: " + stor.Row + " Oszlop: " + stor.Column + " Szint: " + stor.Shelf + "     Mennyiség: " + stor.Quantity + "/" + stor.MaxQuantity;
                                    beerkezo_nev_combobox.Items.Add(fullName);
                                }

                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Nincs megadva mennyiség!");
                    }
                }
                else
                {
                    MessageBox.Show("Nincs kiválasztva alkatrész!");
                }

            }

            ComboBoxesUpdate();
        }
        // 2. Mérföldkő -- Raktárvezető funkció B.5 --> END


        // 2. Mérföldkő -- Raktárvezető funkció B.6 --> START
        // Feladat:
        // Rekeszeknél a maximálisan elhelyezhető darabszám kezelése
        private void LimitModositasComboBoxUpdate()
        {
            if (limit_combobox != null)
            {
                limit_combobox.Items.Clear();
            }

            using (var context = new localNapelemEntities())
            {
                var storages = context.Storages.ToList();

                foreach (var stor in storages)
                {
                    string fullName = "Név: " + stor.ProductName + "     Sor: " + stor.Row + " Oszlop: " + stor.Column + " Szint: " + stor.Shelf + "     Limit: " + stor.MaxQuantity;
                    limit_combobox.Items.Add(fullName);
                }
            }
        }
        public async void limitModositas_onClick(object sender, RoutedEventArgs e)
        {
            using (var client = RestHelper.GetRestClient())
            {
                int valasztottIdx = limit_combobox.SelectedIndex + 1;
                try
                {
                    var response = await client.GetAsync("api/Storage");
                    var content = await response.Content.ReadAsStringAsync();
                    var storages = JsonConvert.DeserializeObject<List<Storage>>(content);

                    var _row = storages[valasztottIdx - 1].Row;
                    var _column = storages[valasztottIdx - 1].Column;
                    var _shelf = storages[valasztottIdx - 1].Shelf;
                    var _cell = storages[valasztottIdx - 1].Cell;
                    var _productName = storages[valasztottIdx - 1].ProductName;
                    var _price = storages[valasztottIdx - 1].Price;
                    var _maxQuantity = int.Parse(ujlimit_textbox.Text);
                    var _quantity = storages[valasztottIdx - 1].Quantity;
                    var _reserved = storages[valasztottIdx - 1].Reserved;

                    var storageObject = new
                    {
                        id = valasztottIdx,
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

                    if (valasztottIdx > 0)
                    {
                        if (ujlimit_textbox.Text != "")
                        {
                            System.Windows.Forms.DialogResult res = (DialogResult)MessageBox.Show("Biztosan módosítja a limitet?", "Limit módosítása", (MessageBoxButton)MessageBoxButtons.OKCancel, (MessageBoxImage)MessageBoxIcon.Information);
                            if (res == System.Windows.Forms.DialogResult.OK)
                            {
                                var request = new HttpRequestMessage(HttpMethod.Put, "api/Storage?id=" + valasztottIdx);
                                var content1 = new StringContent(JsonConvert.SerializeObject(storageObject), Encoding.UTF8, "application/json");
                                request.Content = content1;
                                var response1 = await client.SendAsync(request);
                                var status = response1.StatusCode;
                                if (status.ToString() == "NoContent")
                                {
                                    MessageBox.Show("Alkatrész limit módosítása sikeres.");
                                    ujlimit_textbox.Clear();
                                    limit_combobox.Items.Clear();
                                }
                                else
                                {
                                    MessageBox.Show("Hiba: " + status.ToString());
                                }

                            }
                            if (res == System.Windows.Forms.DialogResult.Cancel)
                            {
                                this.Close();
                            }
                        }
                        else
                        {
                            string message = "Nincs új limit megadva!";
                            MessageBox.Show(message, "Nincs új limit megadva!");
                        }

                    }
                    else
                    {
                        string message = "Nincs kiválasztva alkatrész!";
                        MessageBox.Show(message, "Nincs kiválasztva alkatrész!");
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show("HIBA: " + exception.Message);
                }
            }
            ComboBoxesUpdate();
        }
        // 2. Mérföldkő -- Raktárvezető funkció B.6 --> END


        // 3. Mérföldkő -- Raktárvezető funkció B.3 --> START
        // Feladat:
        // Hiányzó alkatrészek listázása (lefoglalások figyelembevétele mellett, a rendelések támogatására)
        private async void hianyzok_onClick(object sender, RoutedEventArgs e)
        {
            InitializeComponent();

            using (var client = RestHelper.GetRestClient())
            {
                var response = await client.GetAsync("api/Storage");
                var content = await response.Content.ReadAsStringAsync();
                var storages = JsonConvert.DeserializeObject<List<Storage>>(content);
                var lista = new List<Storage>();
                var szukseges = new List<int>();

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Hiba");
                }
                else
                {
                    foreach (var s in storages)
                    {
                        if (s.Quantity == 0)
                        {
                            lista.Add(s);
                            szukseges.Add((int)(s.MaxQuantity + s.Reserved - s.Quantity));

                        }
                    }
                    hianyzokListView.DataContext = lista;
                    szuksegesListView.DataContext = szukseges;

                }
            }
            ComboBoxesUpdate();
        }
        // 3. Mérföldkő -- Raktárvezető funkció B.3 --> END


        // 3. Mérföldkő -- Raktárvezető funkció B.4 --> START
        // Feladat:
        // Hiányzó, de már előre lefoglalt alkatrészek listázása
        private async void lefoglaltHianyzok_onClick(object sender, RoutedEventArgs e)
        {
            InitializeComponent();

            using (var client = RestHelper.GetRestClient())
            {
                var response = await client.GetAsync("api/Storage");
                var content = await response.Content.ReadAsStringAsync();
                var storages = JsonConvert.DeserializeObject<List<Storage>>(content);
                var lista = new List<Storage>();

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Hiba");
                }
                else
                {
                    foreach (var s in storages)
                    {
                        if (s.Quantity == 0 || s.Quantity == null)
                        {
                            lista.Add(s);
                        }
                    }
                    lefoglaltHianyzokListView.DataContext = lista;
                }
            }
            ComboBoxesUpdate();
        }
        // 3. Mérföldkő -- Raktárvezető funkció B.4 --> END


        private void ComboBoxesUpdate() 
        {
            ArModositasComboBoxUpdate();
            BeerkezoFelvetelComboBoxUpdate();
            LimitModositasComboBoxUpdate();
        }

    }
}