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
using System.Windows.Shapes;
using System.ComponentModel;
using System.IO;

namespace inkomate
{
    /// <summary>
    /// Interaction logic for AddResidentWindow.xaml
    /// </summary>
    public partial class AddResidentWindow : Window
    {
        private string image = null;
        Resident.DatabaseHandler residentDatabaseHandler;
        bool update = false;
        Resident resident = null;

        public AddResidentWindow()
        {
            InitializeComponent();
            this.Closing += new CancelEventHandler(AddResidentWindows_Closing);
            buttonAddResident.Click += new RoutedEventHandler(ButtonAddResident_Click);
            buttonSelectImage.Click += new RoutedEventHandler(ButtonSelectImage_Click);

            residentDatabaseHandler = new Resident.DatabaseHandler();
            List<Insurance> insurances = residentDatabaseHandler.SelectAllInsurances();
            listViewInsurances.ItemsSource = insurances;
            List<int> floors = new List<int> { 1, 2, 3, 4, 5 };
            comboBoxFloor.ItemsSource = floors;
        }

        public AddResidentWindow(object temp)
        {
            InitializeComponent();
            if (temp != null)
            {
                resident = (Resident)temp;
                update = true;
                this.Title = "Bewohner bearbeiten - " + resident.LastName;
                buttonAddResident.Content = "Bewohner bearbeiten";
            }
            else
            {
                MessageBox.Show("No valid resident object found!");
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            this.Closing += new CancelEventHandler(AddResidentWindows_Closing);
            buttonAddResident.Click += new RoutedEventHandler(ButtonAddResident_Click);
            buttonSelectImage.Click += new RoutedEventHandler(ButtonSelectImage_Click);
            List<int> floors = new List<int> { 1, 2, 3, 4, 5 };
            comboBoxFloor.ItemsSource = floors;

            residentDatabaseHandler = new Resident.DatabaseHandler();
            List<Insurance> allInsurances = residentDatabaseHandler.SelectAllInsurances();
            List<Insurance> residentInsurances = residentDatabaseHandler.SelectInsurancesOfResident(resident);
            Insurance primaryInsurance = residentDatabaseHandler.SelectPrimaryInsurance(resident);
            List<Insurance> insurances = new List<Insurance>();
            bool added = false;

            foreach (Insurance insurance in allInsurances)
            {
                foreach (Insurance residentInsurance in residentInsurances)
                {
                    if (insurance.Equals(primaryInsurance))
                    {
                        residentInsurance.Primary = true;
                        residentInsurance.Selected = true;
                        insurances.Add(residentInsurance);
                        added = true;
                        break;
                    }
                    else if (insurance.Equals(residentInsurance))
                    {
                        residentInsurance.Selected = true;
                        insurances.Add(residentInsurance);
                        added = true;
                        break;
                    }
                }

                if (!added)
                {
                    insurances.Add(insurance);
                } 
                added = false;
            }

            listViewInsurances.ItemsSource = insurances;
            textboxFirstName.Text = resident.FirstName;
            textBoxLastName.Text = resident.LastName;
            textBoxSSN.Text = resident.SSN;
            comboBoxFloor.SelectedItem = resident.Floor;
            datePickerBirthDate.SelectedDate = resident.BirthDate;
            datePickerEntryDate.SelectedDate= resident.EntryDate;
            if (resident.ExitDate.Year != 1)
            {
                datePickerExitDate.SelectedDate = resident.ExitDate;
            }

            if (resident.Image != null  && File.Exists(resident.Image))
            {
                BitmapImage bitmapImage = null;

                try
                {
                    bitmapImage = new BitmapImage(new Uri(resident.Image));
                    image = resident.Image;
                }
                catch
                {
                    bitmapImage = null;
                }

                imageResidentPreview.Source = bitmapImage;
            }
        }

        private void ButtonSelectImage_Click(object sender, RoutedEventArgs e)
        {
            image = Resident.GetImagePath();
            if (image != null && File.Exists(image))
            {
                try
                {
                    imageResidentPreview.Source = new BitmapImage(new Uri(image));
                }
                catch
                {
                    MessageBox.Show("Ein nicht benutzbares Bild wurde ausgewählt");
                    this.Closing -= new CancelEventHandler(AddResidentWindows_Closing);
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
            }
        }

        private void AddResidentWindows_Closing(object sender, CancelEventArgs e)
        {
            var result = MessageBox.Show("Soll dieses Fenster wirklich geschlossen werden? Alle Änderungen gehen verloren", "Warning", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                e.Cancel = false;
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
            else
            {
                e.Cancel = true;            
            }
        }

        private void ButtonAddResident_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Closing -= new CancelEventHandler(AddResidentWindows_Closing);

                string firstName = textboxFirstName.Text;
                string lastName = textBoxLastName.Text;
                string ssn = textBoxSSN.Text;

                if (firstName == "" || lastName == "" || ssn == "")
                {
                    throw new Exception("Eingabe darf nicht leer sein");
                }


                bool birthDateSelected = datePickerBirthDate.SelectedDate.HasValue;
                bool entryDateSelected = datePickerEntryDate.SelectedDate.HasValue;
                bool exitDateSelected = datePickerExitDate.SelectedDate.HasValue;

                if (!birthDateSelected || !entryDateSelected)
                {
                    throw new Exception("Geburts- und Eintrittsdatum darf nicht leer sein");
                }

                DateTime birthDate = datePickerBirthDate.SelectedDate.Value;
                DateTime entryDate = datePickerEntryDate.SelectedDate.Value;
                DateTime exitDate = new DateTime(0001, 01, 01);

                if (exitDateSelected)
                {
                    exitDate = datePickerExitDate.SelectedDate.Value;
                }

                if (comboBoxFloor.SelectedItem == null)
                {
                    MessageBox.Show("Stockwerk muss ausgewählt werden");
                }

                int floor = (int)comboBoxFloor.SelectedItem;

                listViewInsurances.SelectAll();
                List<Insurance> insurances = listViewInsurances.SelectedItems.Cast<Insurance>().ToList();
                List<Insurance> selectedInsurances = new List<Insurance>();

                int count = 0;
                foreach (Insurance insurance in insurances)
                {
                    if (insurance.Primary)
                    {
                        count++;
                    }

                    if (count > 1)
                    {
                        throw new Exception("Nur eine Hauptversicherung darf ausgewählt werden");
                    }

                    if (insurance.Selected || insurance.Primary)
                    {
                        selectedInsurances.Add(insurance);
                    }
                }

                if (selectedInsurances.Count == 0)
                {
                    throw new Exception("Es muss zumindest eine Versicherung ausgewählt werden");
                }
                if (selectedInsurances.Count == 1)
                {
                    selectedInsurances[0].Primary = true;
                }
                else
                {
                    if (count == 0)
                    {
                        throw new Exception("Es muss zumindest eine Hauptversicherung ausgewählt werden");
                    }
                }

                int id = -1;
                if (update)
                {
                    id = resident.ID;
                }

                resident = new Resident(firstName, lastName, ssn, birthDate, entryDate, exitDate, image);
                resident.Insurances = selectedInsurances;
                resident.Floor = floor;
                if (update)
                {
                    resident.ID = id;
                    residentDatabaseHandler.UpdateResident(resident);
                }
                else
                {
                    residentDatabaseHandler.InsertNewResident(resident);
                }

                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
