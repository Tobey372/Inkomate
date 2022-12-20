using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace inkomate
{
    /// <summary>
    /// Interaction logic for AddProduct.xaml
    /// </summary>
    public partial class AddProduct : Window
    {
        ShowProducts showProducts;
        Product.DatabaseHandler databaseHandler;

        public AddProduct(ShowProducts showProducts)
        {
            InitializeComponent();
            this.Closing += new CancelEventHandler(AddProduct_Closing);
            buttonAddProduct.Click += new RoutedEventHandler(buttonAddProduct_Click);
            this.showProducts = showProducts;
            databaseHandler = new Product.DatabaseHandler();
        }

        private void AddProduct_Closing(object sender, CancelEventArgs e)
        {
            showProducts.refresh();
        }

        private void buttonAddProduct_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                string name = textBoxName.Text;
                string price_string = textBoxPrice.Text;
                string amount_string = textBoxAmount.Text;

                if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(price_string) || String.IsNullOrEmpty(amount_string))
                {
                    throw new Exception("Eingabe darf nicht leer sein");
                }

                if (!float.TryParse(price_string, out float price))
                {
                    throw new Exception("Preis ist ungültig");
                }

                if (price <= 0)
                {
                    throw new Exception("Preis darf nicht kleiner als 0 sein");
                }

                if (!int.TryParse(amount_string, out int amount))
                {
                    throw new Exception("Anzahl ist ungültig");
                }

                if (price <= 0)
                {
                    throw new Exception("Anzahl darf nicht kleiner als 0 sein");
                }

                databaseHandler.InsertNewProduct(new Product(name, price, amount));
                this.Close();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


    }
}
