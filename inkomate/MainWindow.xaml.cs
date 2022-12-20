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

namespace inkomate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Resident.DatabaseHandler residentDatabaseHandler;
        bool showExited = false;
        Resident.TableInfo info = null;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                buttonAddResident.Click += new RoutedEventHandler(ButtonAddResident_Click);
                buttonDeleteResident.Click += new RoutedEventHandler(ButtonDeleteResident_Click);
                buttonEditResident.Click += new RoutedEventHandler(ButtonEditResident_Click);
                buttonAddProductToResident.Click += new RoutedEventHandler(ButtonAddProductToResident_Click);
                buttonShowProducts.Click += new RoutedEventHandler(ButtonShowProducts_Click);
                buttonExportCSV.Click += new RoutedEventHandler(ButtonExportCSV_Click);
                checkBoxShowExited.Checked += new RoutedEventHandler(CheckBoxExited_Checked);
                checkBoxShowExited.Unchecked += new RoutedEventHandler(CheckBoxExited_Unchecked);
                listViewResidents.SelectionMode = SelectionMode.Single;


                residentDatabaseHandler = new Resident.DatabaseHandler();
                //List<Resident.TableInfo> tableInfos = residentDatabaseHandler.GetTableInfo();

                List<Resident.TableInfo> tableInfos = new List<Resident.TableInfo>
            {
                new Resident.TableInfo("first_name", "Vorname", true),
                new Resident.TableInfo("last_name", "Nachname", true),
                new Resident.TableInfo("ssn", "SVN", true),
                new Resident.TableInfo("floor", "Stockwerk"),
                new Resident.TableInfo("birth_date", "Geburtstag"),
                new Resident.TableInfo("entry_date", "Eintrittsdatum"),
                new Resident.TableInfo("exit_date", "Austrittsdatum"),
                new Resident.TableInfo("added", "Erstelldatum"),

            };
                comboBoxOrder.ItemsSource = tableInfos;
                comboBoxOrder.SelectedIndex = 7;
                comboBoxOrder.SelectionChanged += new SelectionChangedEventHandler(ComboBoxOrder_SelectionChanged);
                textBoxSearch.TextChanged += new TextChangedEventHandler(TextBoxSearch_TextChanged);
                textBoxSearch.IsEnabled = false;
                refresh();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ComboBoxOrder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxOrder.SelectedItem != null)
            {
                info = (Resident.TableInfo)comboBoxOrder.SelectedItem;
                if (info.Searchable)
                {
                    textBoxSearch.IsEnabled = true;
                }
                else
                {
                    textBoxSearch.IsEnabled = false;
                    textBoxSearch.Text = "";
                }
                refresh(showExited);
            }
            else
            {
                textBoxSearch.IsEnabled = false;
                textBoxSearch.Text = "";
            }
        }

        private void TextBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textBoxSearch.Text == "")
            {
                refresh(showExited);
            }

            if (info != null && info.Searchable)
            {
                List<Resident> residents = (List<Resident>)listViewResidents.ItemsSource;
                List<Resident> filtered = new List<Resident>();

                foreach (Resident resident in residents)
                {
                    if (info.Name == "first_name")
                    {
                        if (resident.FirstName.ToLower().Contains(textBoxSearch.Text.ToLower()))
                        {
                            filtered.Add(resident);
                        }
                    }
                    else if (info.Name == "last_name")
                    {
                        if (resident.LastName.ToLower().Contains(textBoxSearch.Text.ToLower()))
                        {
                            filtered.Add(resident);
                        }
                    }
                    else if (info.Name == "ssn")
                    {
                        if (resident.SSN.Contains(textBoxSearch.Text))
                        {
                            filtered.Add(resident);
                        }
                    }
                }

                listViewResidents.ItemsSource = filtered;
                List<Resident> temp = (List<Resident>)listViewResidents.ItemsSource;
                textBoxCount.Text = temp.Count.ToString();
            }
        }

        private void ButtonAddResident_Click(object sender, RoutedEventArgs e)
        {
            AddResidentWindow addResidentWindow = new AddResidentWindow();
            addResidentWindow.Show();
            this.Close();
        }

        private void ButtonDeleteResident_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Resident> residents = listViewResidents.SelectedItems.Cast<Resident>().ToList();
                if (residents.Count == 0)
                {
                    throw new Exception("Bitte zuerst ein einen Bewohner auswählen");
                }
                if (residents.Count > 1)
                {
                    throw new Exception("Nur ein Bewohner darf ausgewählt sein");
                }

                var result = MessageBox.Show("Soll der Bewohner - " + residents[0].ViewableName + " - wirklich gelöscht werden? Dieser Vorgang ist endgültig!", "Warning", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    residentDatabaseHandler.DeleteResident(residents[0]);
                    MessageBox.Show("Bewohner - " + residents[0].ViewableName + " - wurde gelöscht");
                    refresh(showExited);
                }
               
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonEditResident_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Resident> residents = listViewResidents.SelectedItems.Cast<Resident>().ToList();
                if (residents.Count == 0)
                {
                    throw new Exception("Bitte zuerst ein einen Bewohner auswählen");
                }
                if (residents.Count > 1)
                {
                    throw new Exception("Nur ein Bewohner darf ausgewählt sein");
                }

                AddResidentWindow addResidentWindow = new AddResidentWindow(residents[0]);
                addResidentWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonAddProductToResident_Click(object sender, RoutedEventArgs e)
        {  
            try
            {
                List<Resident> residents = listViewResidents.SelectedItems.Cast<Resident>().ToList();
                if (residents.Count == 0)
                {
                    throw new Exception("Bitte zuerst ein einen Bewohner auswählen");
                }
                if (residents.Count > 1)
                {
                    throw new Exception("Nur ein Bewohner darf ausgewählt sein");
                }

                AddProductToResident incontinenceAddToResident = new AddProductToResident(residents[0]);
                incontinenceAddToResident.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ButtonShowProducts_Click(object sender, RoutedEventArgs e)
        {
            ShowProducts showProducts = new ShowProducts();
            showProducts.Show();
            this.Close();
        }

        private void ButtonExportCSV_Click(object sender, RoutedEventArgs e)
        {
            ExportFile exportFile = new ExportFile();
            exportFile.Show();
        }

        private void CheckBoxExited_Checked(object sender, RoutedEventArgs e)
        {
            refresh(true);
            showExited = true;
        }

        private void CheckBoxExited_Unchecked(object sender, RoutedEventArgs e)
        {
            refresh(false);
            showExited = false;
        }

        private void refresh(bool exited = false)
        {
            listViewResidents.ItemsSource = residentDatabaseHandler.SelectAllResidents(info, exited);
            List<Resident> temp = (List<Resident>)listViewResidents.ItemsSource;
            textBoxCount.Text = temp.Count.ToString();
            
        }
    }
}
