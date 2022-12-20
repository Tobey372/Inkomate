using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CsvHelper;
using System.Globalization;

namespace inkomate
{
    /// <summary>
    /// Interaction logic for ExportFile.xaml
    /// </summary>
    public partial class ExportFile : Window
    {

        Insurance insurance = null;
        Resident.DatabaseHandler databaseHandler = null;
        float allowance;
        public ExportFile()
        {
            InitializeComponent();
            databaseHandler = new Resident.DatabaseHandler();
            comboBoxInsurance.ItemsSource = databaseHandler.SelectAllInsurances();
            comboBoxInsurance.SelectionChanged += new SelectionChangedEventHandler(ComboBoxInsurance_SelectionChanged);
            buttonSetAllowanceValue.Click += new RoutedEventHandler(ButtonSetAllowanceValue_Click);
            buttonExportCSV.Click += new RoutedEventHandler(ButtonExportCSV_Click);
            refresh();
        }

        private void ComboBoxInsurance_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            insurance = (Insurance)comboBoxInsurance.SelectedItem;
            refresh();
        }

        private void ButtonSetAllowanceValue_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string allowance_string = textBoxAllowance.Text;

                if (String.IsNullOrEmpty(allowance_string))
                {
                    throw new Exception("Kein Pauschalwert vorhanden");
                }

                if (!float.TryParse(allowance_string, out float allowance))
                {
                    throw new Exception("Pauschalwert ist undgültig");
                }

                if (allowance <= 0)
                {
                    throw new Exception("Pauschalewert muss größer als 0 sein");
                }

                databaseHandler.SetAllowanceValue(allowance);
                refresh();
                MessageBox.Show("Pauschalwart wurde gesetzt");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonExportCSV_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = textBoxFileName.Text;

                if (String.IsNullOrEmpty(name))
                {
                    throw new Exception("Bitte zuerst einen Dateinamen auswählen");
                }

                if (insurance == null)
                {
                    throw new Exception("Bitte zuerst eine Versicherung auswählen");
                }

                System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
                dialog.Description = "Ornder auswählen für die CSV Datei";

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string path = dialog.SelectedPath;
                    if (!Directory.Exists(path))
                    {
                        throw new Exception("Ornder ist ungültig");
                    }

                    List<Resident.Exportable> exportables = databaseHandler.SelectResidentsWithProductsPerInsurance(insurance);
                    exportables = exportables.OrderBy(ex => ex.Floor).ToList();

                    path = path + @"\" + name + ".csv";

                    if (File.Exists(path))
                    {
                        throw new Exception("Datei exestiert bereits! Bitte einen anderen Namen auswählen oder Datei löschen");
                    }

                    using (var stream = File.Open(path, FileMode.Append))
                    using (var writer = new StreamWriter(stream, Encoding.UTF8))
                    using (var csv = new CsvWriter(writer, CultureInfo.CurrentCulture))
                    {
                        csv.WriteRecords(exportables);
                    }

                }

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void refresh()
        {
            allowance = databaseHandler.SelectAllowanceValue();
            textBoxAllowance.Text = allowance.ToString(); 

            if (insurance != null)
            {
                float totalAllowance = databaseHandler.SelectTotalAllowanceValuePerInsurance(insurance);
                float currentAllowance = databaseHandler.SelectTotalPriceOfResidentsPerInsurance(insurance);

                textBoxTotalAllowance.Text = totalAllowance.ToString();
                textBoxCurrentAllowance.Text = currentAllowance.ToString();

                if (currentAllowance > totalAllowance)
                {
                    textBoxCurrentAllowance.Background = Brushes.Red;
                }
                else
                {
                    textBoxCurrentAllowance.Background = Brushes.Green;
                }
            }
        }


    }
}
