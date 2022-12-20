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
    /// Interaction logic for AddIncontinenceAddToResident.xaml
    /// </summary>
    public partial class AddProductToResident : Window
    {
        Product.DatabaseHandler productDatabaseHandler;
        Resident.DatabaseHandler residentDatabaseHandler;
        Resident resident = null;
        Product selectedProduct = null;

        public AddProductToResident(object temp)
        {
            InitializeComponent();
            if (temp != null)
            {
                resident= (Resident)temp;
            }
            else
            {
                MessageBox.Show("No resident has been selected!");
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }

            this.Title = "Inkontinezprodukte bearbeiten - " + resident.LastName;
            this.Closing += new CancelEventHandler(AddProductToResident_Closing);
            productDatabaseHandler = new Product.DatabaseHandler();
            residentDatabaseHandler = new Resident.DatabaseHandler();
            comboBoxProducts.ItemsSource = productDatabaseHandler.SelectAllProducts();
            buttonAddProductToResident.Click += new RoutedEventHandler(ButtonAddProductToResident);
            buttonEditProductOfResident.Click += new RoutedEventHandler(ButtonEditProductOfResident_Click);
            buttonDeleteProductOfResident.Click += new RoutedEventHandler(ButtonDeleteProductOfResident_Click);
            listViewProducts.SelectionMode = SelectionMode.Single;
            listViewProducts.SelectionChanged += new SelectionChangedEventHandler(ListViewProducts_SelectionChanged);
            refresh();
        }

        private void ButtonAddProductToResident(object sender, RoutedEventArgs e)
        {
            try
            {
                if (textBoxAmount.Text == "")
                {
                    throw new Exception("Anzahl darf nicht leer sein");
                }

                if (comboBoxProducts.SelectedItem == null)
                {
                    throw new Exception("Kein Inkontinezprodukt wurde ausgewählt");
                }

                Product product = (Product)comboBoxProducts.SelectedItem;

                if (!int.TryParse(textBoxAmount.Text, out int amount))
                {
                    throw new Exception("Anzahl ist ungültig");
                }

                if (amount <= 0)
                {
                    throw new Exception("Anzahl darf nicht kleiner als 0 sein");
                }

                residentDatabaseHandler.InsertProduct(product, amount, resident);
                refresh();              
            }
            catch(Exception ex) 
            { 
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonEditProductOfResident_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                if (textBoxAmount.Text == "")
                {
                    throw new Exception("Anzahl darf nicht leer sein");
                }

                if (selectedProduct == null)
                {
                    throw new Exception("Kein Inkontinezprodukt wurde ausgewählt");
                }

                if (!int.TryParse(textBoxAmount.Text, out int amount))
                {
                    throw new Exception("Anzahl ist ungültig");
                }

                if (amount <= 0)
                {
                    throw new Exception("Anzahl darf nicht kleiner als 0 sein");
                }

                residentDatabaseHandler.UpdateProductOfResident(resident, selectedProduct, amount);
                refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonDeleteProductOfResident_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedProduct == null)
                {
                    throw new Exception("Kein Inkontinezprodukt wurde ausgewählt");
                }

                residentDatabaseHandler.DeleteProductFromResident(resident, selectedProduct);
                refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ListViewProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listViewProducts.SelectedItem != null)
            {
                selectedProduct = (Product)listViewProducts.SelectedItem;
                textBoxAmount.Text = selectedProduct.TotalAmount.ToString();
            }
        }

        private void AddProductToResident_Closing(object sender, CancelEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void refresh()
        {
            listViewProducts.ItemsSource = productDatabaseHandler.SelectProductsOfResident(resident);
        }


    }
}
