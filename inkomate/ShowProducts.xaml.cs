using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace inkomate
{
    /// <summary>
    /// Interaction logic for ShowProducts.xaml
    /// </summary>
    public partial class ShowProducts : Window
    {
        Product.DatabaseHandler databaseHandler = null;
        Product selected = null;

        public ShowProducts()
        {
            InitializeComponent();
            this.Closing += new CancelEventHandler(ShowProducts_Closing);
            listViewProducts.SelectionChanged += new SelectionChangedEventHandler(ListViewProducts_SelectionChanged);
            databaseHandler = new Product.DatabaseHandler();
            refresh();

            buttonDeleteProduct.Click += new RoutedEventHandler(ButtonDeleteProduct_Click);
            buttonEditProduct.Click += new RoutedEventHandler(ButtonEditProduct_Click);
            buttonAddProduct.Click += new RoutedEventHandler(ButtonAddProduct_Click);
        }

        private void ShowProducts_Closing(object sender, CancelEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void ListViewProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listViewProducts.SelectedItem != null)
            {
                Product product = listViewProducts.SelectedItem as Product;
                selected = product;
                textBoxName.Text = product.Name;
                textBoxPrice.Text = product.Price.ToString();
                textBoxAmount.Text = product.Amount.ToString();
            }
        }

        private void ButtonDeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selected == null)
                {
                    throw new Exception("Bitte ein Inkontinenzprodukt auswählen");
                }

                var result = MessageBox.Show("Soll dieses Produkt wirklich gelöscht werden? Dieses Produkt wird dann von allen Bewohnern entfernt, welche dieses benutzen", "Warning", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    databaseHandler.DeleteProduct(selected);
                    refresh();
                }
               
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonEditProduct_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selected == null)
                {
                    throw new Exception("Bitte ein Inkontinenzprodukt auswählen");
                }

                string name = textBoxName.Text;
                string price_string = textBoxPrice.Text;
                string amount_string = textBoxAmount.Text;

                if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(price_string) || String.IsNullOrEmpty(amount_string))
                {
                    throw new Exception("Eingabe darf nicht leer sein");
                }

                if (!float.TryParse(price_string, out float price))
                {
                    throw new Exception("Preis ist nicht gültig");
                }

                if (price <= 0)
                {
                    throw new Exception("Preis darf nicht kleiner als null sein");
                }

                if (!int.TryParse(amount_string, out int amount))
                {
                    throw new Exception("Anzahl ist nicht gültig");
                }

                if (amount <= 0)
                {
                    throw new Exception("Anzahl darf nicht kleiner als null sein");
                }

                selected.Name = name;
                selected.Price = price;
                selected.Amount = amount;
                databaseHandler.UpdateProduct(selected);
                refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonAddProduct_Click(object sender, RoutedEventArgs e)
        {
            AddProduct addProduct = new AddProduct(this);
            addProduct.Show();
        }

        public void refresh()
        {
            listViewProducts.ItemsSource = databaseHandler.SelectAllProducts();
        }


    }
}
