using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data.SQLite;
using Microsoft.Win32;
using System.Globalization;
using System.IO;
using System.Windows.Media.Imaging;
using System.Reflection;
using CsvHelper.Configuration.Attributes;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace inkomate
{
    internal class Resident
    {

        private int id;
        private string firstName;
        private string lastName;
        private string ssn;
        private DateTime birthDate;
        private DateTime entryDate;
        private DateTime exitDate;
        private string image;
        private List<Insurance> insurances;
        private List<Product> products;
        private int floor;

        public int ID 
        {
            get {
                return id; 
            } 
            set 
            {
                if (value >= 0)
                {
                    id = value;
                }
                else
                {
                    throw new Exception("Id kann nicht leer sein");
                }
                   
            } 
        }

        public int Floor
        {
            get { return floor; }
            set { floor = value; }
        }

        public string FirstName { get { return firstName; } set { firstName = value; } }

        public string LastName { get { return lastName; } set { lastName = value; } }

        public string ViewableName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }

        public string SSN 
        { 
            get 
            { 
                return ssn; 
            } 
            set 
            { 
                if (value.Length == 10)
                {
                    foreach (char c in value)
                    {
                        if (!Char.IsDigit(c))
                        {
                            throw new Exception("SVN darf nur Zahlen beinhalten");
                        }
                    }
                    ssn = value;
                }
                else
                {
                    throw new Exception("SVN muss exakt 10 Zeichen lang sein");
                }
            } 
        }

        public DateTime BirthDate { get { return birthDate; } set { birthDate = value; } }

        public string DataBaseBirthDate
        {
            get
            {
                return String.Format("{0:dd-MM-yyyy}", BirthDate);
            }
        }

        public DateTime EntryDate 
        { 
            get 
            { 
                return entryDate; 
            } 
            set 
            { 
                if (BirthDate != null)
                {
                    if (DateTime.Compare(value, BirthDate) < 0)
                    {
                        throw new Exception("Eintrittsdatum darf nicht vor dem Geburtsdatum sein");
                    }
                }

                entryDate = value;
            } 
        }

        public string DataBaseEntryDate
        {
            get
            {
                return String.Format("{0:dd-MM-yyyy}", EntryDate);
            }
        }

        public string ViewableEntryDate { get { return DataBaseEntryDate; } }

        public DateTime ExitDate 
        {
            get 
            {
                return exitDate; 
            } 
            set 
            {
                if (exitDate.Year != 1)
                {
                    if (BirthDate != null)
                    {
                        if (DateTime.Compare(value, BirthDate) < 0)
                        {
                            throw new Exception("Austrittsdatum darf nicht vor dem Geburtsdatum sein");
                        }
                    }

                    if (EntryDate != null)
                    {
                        if (DateTime.Compare(value, EntryDate) < 0)
                        {
                            throw new Exception("Austrittsdatum darf nicht vor dem Eintrittsdatum sein");
                        }
                    }
                }

                exitDate = value;
            } 
        }

        public string DataBaseExitDate
        {
            get
            {
                if (ExitDate.Year == 1)
                {
                    return null;
                }

                return String.Format("{0:dd-MM-yyyy}", ExitDate);
            }
        }

        public string ViewableExitDate 
        { 
            get 
            { 
                if (DataBaseExitDate == null)
                {
                    return "/";
                }
                return DataBaseExitDate;
            } 
        }

        public string Image { get { return image; } set { image = value; } }

        public string ViewableImage
        {
            get
            {
                if (Image != null && File.Exists(Image))
                {
                    try
                    {
                        BitmapImage bitmapImage = new BitmapImage(new Uri(image));
                        return Image;
                    }
                    catch
                    {
                        return null;
                    }
                }
                return null;
            }
        }

        public string ViewablePrimaryInsurance
        {
            get
            {
                Insurance insurance = new Resident.DatabaseHandler().SelectPrimaryInsurance(this);
                if (insurance != null)
                {
                    return insurance.Name;
                }
                return "/";
            }
        }

        public List<Insurance> Insurances { get { return insurances; } set { insurances = value; } }

        public List<Product> Products { get; set; }

        public Resident(string firstName, string lastName, string ssn, DateTime birthDate, DateTime entryDate, DateTime exitDate, string image)
        {
            FirstName = firstName;
            LastName = lastName;
            SSN = ssn;
            BirthDate = birthDate;
            EntryDate = entryDate;
            ExitDate = exitDate;
            Image = image;
        }

        public Resident(Int64 id, String first_name, String last_name, String ssn, Int64 floor, String birth_date, String entry_date, String exit_date, String image)
        {
            ID = (int)id;
            FirstName = first_name;
            LastName = last_name;
            SSN = ssn;
            BirthDate = DateTime.ParseExact(birth_date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            EntryDate = DateTime.ParseExact(entry_date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            if (exit_date != null)
            {
                ExitDate = DateTime.ParseExact(exit_date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            }
            else
            {
                ExitDate = new DateTime(1, 1, 1);
            }
            Image = image;
            Floor = (int)floor;
        }

        static public string GetImagePath()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Bild des Bewohners auswählen";
            ofd.Filter = "Image Files (*.bmp;*.png;*.jpg)|*.bmp;*.png;*.jpg";
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;

            Nullable<bool> result = ofd.ShowDialog();
            if (result == true)
            {
                return ofd.FileName;
            }
            return null;        
        }

        public class DatabaseHandler
        {
            private string connectionString;

            public string ConnectionString { get { return connectionString; } set { connectionString = value; } }

            public DatabaseHandler()
            {
                ConnectionString = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "../inkomate.sqlite");

            }

            public List<Insurance> SelectAllInsurances()
            {
                using (var connection = new SQLiteConnection("Data Source=" + ConnectionString))
                {
                    
                    string sql = "select * from insurances";
                    var insurances = connection.Query<Insurance>(sql);
                    return (List<Insurance>)insurances;
                }
            }

            public void InsertNewResident(Resident resident)
            {
                try
                {
                    using (var connection = new SQLiteConnection("Data Source=" + ConnectionString))
                    {
                        var parameters = new
                        {
                            first_name = resident.FirstName,
                            last_name = resident.LastName,
                            ssn = resident.SSN,
                            birth_date = resident.DataBaseBirthDate,
                            entry_date = resident.DataBaseEntryDate,
                            exit_date = resident.DataBaseExitDate,
                            image = resident.Image,
                            floor = resident.Floor,
                        };

                        string sql = "insert into residents (first_name, last_name, ssn, birth_date, entry_date, exit_date, image, floor) " +
                            "values (@first_name, @last_name, @ssn, @birth_date, @entry_date, @exit_date, @image, @floor);" +
                            "SELECT last_insert_rowid()";

                        var id = connection.QuerySingle<int>(sql, parameters);
                        resident.ID = id;
                    }

                    InsertInsurances(resident);

                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("UNIQUE"))
                    {
                        throw new Exception("Bewohner mit dieser SVN wurde bereits hinzugefügt");
                    }
                }
            }

            public void InsertInsurances(Resident resident)
            {
                if (String.IsNullOrEmpty(resident.ID.ToString()))
                {
                    throw new ArgumentNullException("Faulty resident can not be used for inserts");
                }

                using (var connection = new SQLiteConnection("Data Source=" + ConnectionString))
                {
                    foreach (Insurance insurance in resident.Insurances)
                    {
                        var parameters = new
                        {
                            insurance_id = insurance.ID,
                            resident_id = resident.ID,
                            primary = insurance.DateBasePrimary,
                        };

                        string sql = @"insert into insurances_to_residents (insurance_id, resident_id, 'primary') " +
                            "values (@insurance_id, @resident_id, @primary)";

                        connection.Execute(sql, parameters);
                    }
                }
            }

            public int SelectCountOfResidents(bool exited = false)
            {
                using (var connection = new SQLiteConnection("Data Source=" + ConnectionString))
                {
                    string sql = @"select count(*) from residents where exit_date is NULL";

                    if (exited)
                    {
                        sql = @"select count(*) from residents";
                    }
                    
                    var count = connection.Query<int>(sql);
                    
                    if (count.Count() == 1)
                    {
                        return count.ElementAt(0);
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

            public Insurance SelectPrimaryInsurance(Resident resident)
            {
                if (String.IsNullOrEmpty(resident.ID.ToString()))
                {
                    return null;
                }

                using (var connection = new SQLiteConnection("Data Source=" + ConnectionString))
                {     
                    string sql = @"select ins.id, ins.name, ins.interval from insurances as ins " +
                        "inner join insurances_to_residents as instores on ins.id = instores.insurance_id " +
                        "inner join residents as res on res.id = instores.resident_id and res.id = @id " +
                        "where instores.'primary' = 1 " +
                        "limit 1";
                    var insurances = connection.Query<Insurance>(sql, new {id = resident.ID});
                    insurances = (List<Insurance>)insurances;
                    
                    if (insurances.Count() == 1)
                    {
                        return insurances.ElementAt(0);
                    }
                    return null;  
                }
            }

            public List<Insurance> SelectInsurancesOfResident(Resident resident)
            {
                if (String.IsNullOrEmpty(resident.ID.ToString()))
                {
                    return null;
                }

                using (var connection = new SQLiteConnection("Data Source=" + ConnectionString))
                {
                    string sql = @"select ins.id, ins.name, ins.interval from insurances as ins " +
                        "inner join insurances_to_residents as instores on ins.id = instores.insurance_id " +
                        "inner join residents as res on res.id = instores.resident_id and res.id = @id";
                    var insurances = connection.Query<Insurance>(sql, new { id = resident.ID });
                    return (List<Insurance>)insurances;  
                }
            }

            public List<Resident> SelectAllResidents(TableInfo info, bool exited = false)
            {
                using (var connection = new SQLiteConnection("Data Source=" + ConnectionString))
                {
                    string orderBy = "added";
                    if (info != null)
                    {
                        orderBy = info.Name;
                    }

                    string sql;
                    if (exited)
                    {
                        sql = "select * from residents";
                        
                    }
                    else
                    {
                        sql = "select * from residents where exit_date is NULL";
                    }

                    var temp = connection.Query<Resident>(sql);

                    List<Resident> residents = (List<Resident>)temp;
                    
                    if (orderBy == "first_name")
                    {
                        residents = residents.OrderBy(r => r.FirstName).ToList();
                    }
                    else if (orderBy == "last_name")
                    {
                        residents = residents.OrderBy(r => r.LastName).ToList();
                    }
                    else if (orderBy == "ssn")
                    {
                        residents = residents.OrderBy(r => r.SSN).ToList();
                    }
                    else if (orderBy == "floor")
                    {
                        residents = residents.OrderBy(r => r.Floor).ToList();
                    }
                    else if (orderBy == "birth_date")
                    {
                        residents = residents.OrderBy(r => r.BirthDate).ToList();
                    }
                    else if (orderBy == "entry_date")
                    {
                        residents = residents.OrderBy(r => r.EntryDate).ToList();
                    }
                    else if (orderBy == "exit_date")
                    {
                        residents = residents.OrderBy(r => r.ExitDate).ToList();
                    }
                    else if (orderBy == "added")
                    {
                        residents = residents.OrderBy(r => r.ID).ToList();
                    }
                    else
                    {
                        residents = residents.OrderBy(r => r.ID).ToList();
                    }

                    return residents;
                    
                }
            }

            public void DeleteResident(Resident resident)
            {
                if (String.IsNullOrEmpty(resident.ID.ToString()))
                {
                    throw new Exception("Resident wihtout ID can not be deleted");
                }

                using (var connection = new SQLiteConnection("Data Source=" + ConnectionString))
                {
                    string sql = @"delete from residents where id = @id; delete from insurances_to_residents where resident_id = @id; delete from products_to_residents where resident_id = @id";
                    connection.Execute(sql, new { id = resident.ID });
                }
            }

            public void UpdateResident(Resident resident)
            {            
                try
                {
                    using (var connection = new SQLiteConnection("Data Source=" + ConnectionString))
                    {
                        var parameters = new
                        {
                            first_name = resident.FirstName,
                            last_name = resident.LastName,
                            ssn = resident.SSN,
                            birth_date = resident.DataBaseBirthDate,
                            entry_date = resident.DataBaseEntryDate,
                            exit_date = resident.DataBaseExitDate,
                            image = resident.Image,
                            id = resident.ID,
                            floor = resident.Floor,
                        };

                        string sql = @"update residents " +
                            "set first_name = @first_name, last_name = @last_name, ssn = @ssn, birth_date = @birth_date, " +
                            "entry_date = @entry_date, exit_date = @exit_date, image = @image, floor = @floor " +
                            "where id = @id; " +
                            "delete from insurances_to_residents where resident_id = @id";

                        connection.Execute(sql, parameters);
                    }

                    InsertInsurances(resident);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("UNIQUE"))
                    {
                        throw new Exception("Bewohner mit dieser SVN wurde bereits hinzugefügt");
                    }
                }
            }

            public void DeleteImageFromResident(Resident resident)
            {
                using (var connection = new SQLiteConnection("Data Source=" + ConnectionString))
                {
                    var parameters = new
                    {
                        image = "",
                        id = resident.ID,
                    };

                    string sql = @"update residents " +
                        "set image = @image " +
                        "where id = @id";

                    connection.Execute(sql, parameters);
                }
            }

            public void InsertProduct(Product product, int amount, Resident resident)
            {
                try
                {
                    using (var connection = new SQLiteConnection("Data Source=" + ConnectionString))
                    {

                        var parameters = new
                        {
                            product_id = product.ID,
                            resident_id = resident.ID,
                            amount,
                        };

                        string sql = @"insert into products_to_residents (product_id, resident_id, amount) " +
                            "values (@product_id, @resident_id, @amount)";

                        connection.Execute(sql, parameters);

                    }
                }
                catch(Exception ex)
                {
                    if (ex.Message.Contains("UNIQUE"))
                    {
                        throw new Exception("Diesem Bewohner wurde dieses Inkontinenzprodukt bereits hinzugefügt");
                    }
                }
            }

            //private List<Resident> SelectResidentsPerInsurance(Insurance insurance)
            //{
            //    using (var connection = new SQLiteConnection("Data Source=" + ConnectionString))
            //    {
            //        string sql = "select res.id, res.first_name, res.last_name, res.ssn, res.floor, res.birth_date, res.entry_date, res.exit_date, res.image from residents as res " +
            //            "inner join insurances_to_residents as intores on res.id = intores.resident_id and intores.'primary' = 1 " +
            //            "inner join insurances as in on intores.insurance_id = @insurance_id";
            //        var residents = connection.Query<Resident>(sql, new {insurance_id = insurance.ID});
            //        return (List<Resident>)residents;
            //    }
            //}

            //public List<Resident> SelectResidentsWithProductsPerInsurance(Insurance insurance)
            //{
            //    List<Resident> residents = this.SelectResidentsPerInsurance(insurance);

            //    using (var connection = new SQLiteConnection("Data Source=" + ConnectionString))
            //    {
            //        foreach(Resident resident in residents)
            //        {
            //            string sql = "select pro.id, pro.name, pro.price, pro.amount, protores.amount from products as pro " +
            //                "inner join products_to_residents as protores on pro.id = protores.product_id " +
            //                "inner join residents as res on res.id = @resident_id";
            //            var products = connection.Query<Product>(sql, new {resident_id = resident.ID});
            //            resident.Products = (List<Product>)products;
            //        }
            //    }
            //    return residents;
            //}

            public List<Exportable> SelectResidentsWithProductsPerInsurance(Insurance insurance)
            {
                using (var connection = new SQLiteConnection("Data Source=" + ConnectionString))
                {

                    string sql = "select res.first_name as firstName, res.last_name as lastName, res.ssn, res.floor, pro.name as productName, (pro.price * pro.amount * protores.amount) as totalPrice, protores.amount " +
                        "from residents as res " +
                        "inner join insurances_to_residents as intores " +
                        "on intores.resident_id = res.id and intores.'primary' = 1 " +
                        "inner join insurances as ins on ins.id = intores.insurance_id and ins.id = @insurance_id " +
                        "inner join products_to_residents as protores " +
                        "on res.id = protores.resident_id " +
                        "inner join products as pro " +
                        "on pro.id = protores.product_id " +
                        "where res.exit_date is NULL";

                    var exportables = connection.Query<Exportable>(sql, new { insurance_id = insurance.ID });
                    return (List<Exportable>)exportables;
                    
                }
            }

            public float SelectAllowanceValue()
            {
                using (var connection = new SQLiteConnection("Data Source=" + ConnectionString))
                {
                    string sql = "select value from allowances limit 1";
                    var allowances = connection.Query<Double>(sql);
                    List<Double> temp = (List<Double>)allowances;
                    if (temp.Count == 0)
                    {
                        return 26;
                    }
                    else
                    {
                        return (float)temp[0];
                    }
                }
            }

            public void SetAllowanceValue(float value)
            {
                using (var connection = new SQLiteConnection("Data Source=" + ConnectionString))
                {
                    string sql = "delete from allowances; insert into allowances (value) values (@value)";
                    connection.Execute(sql, new {value});
                }
            }

            public float SelectTotalAllowanceValuePerInsurance(Insurance insurance)
            {
                using (var connection = new SQLiteConnection("Data Source=" + ConnectionString))
                {
                    string sql = "select (count(*) * (select value from allowances limit 1)) as value " +
                        "from residents as res " +
                        "inner join insurances_to_residents as intores " +
                        "on intores.resident_id = res.id and intores.'primary' = 1 " +
                        "inner join insurances as ins " +
                        "on ins.id = intores.insurance_id and ins.id = @insurance_id " +
                        "where res.exit_date is NULL";

                    var allowances = connection.Query<Double>(sql, new {insurance_id = insurance.ID });
                    List<Double> temp = (List<Double>)allowances;
                    if (temp.Count == 0)
                    {
                        return 0;
                    }
                    else
                    {
                        return (float)temp[0];
                    }
                }
            }

            public float SelectTotalPriceOfResidentsPerInsurance(Insurance insurance)
            {
                using (var connection = new SQLiteConnection("Data Source=" + ConnectionString))
                {
                    string sql = "select sum(pro.price * pro.amount * protores.amount * 1.095) as value " +
                       "from residents as res " +
                       "inner join insurances_to_residents as intores " +
                       "on intores.resident_id = res.id and intores.'primary' = 1 and res.exit_date is NULL " +
                       "inner join insurances as ins on ins.id = intores.insurance_id and ins.id = @insurance_id " +
                       "inner join products_to_residents as protores " +
                       "on res.id = protores.resident_id " +
                       "inner join products as pro " +
                       "on pro.id = protores.product_id ";

                    var allowances = connection.Query<Allowance>(sql, new { insurance_id = insurance.ID });
                    List<Allowance> temp = (List<Allowance>)allowances;
                    if (temp.Count == 0)
                    {
                        return 0;
                    }
                    else
                    {
                        return temp[0].Value;
                    }
                }
            }

            public void DeleteProductFromResident(Resident resident, Product product)
            {
                using (var connection = new SQLiteConnection("Data Source=" + ConnectionString))
                {
                    string sql = "delete from products_to_residents where resident_id = @resident_id and product_id = @product_id";
                    connection.Execute(sql, new { resident_id = resident.ID, product_id = product.ID });
                }
            }

            public void UpdateProductOfResident(Resident resident, Product product, int amount)
            {
                using (var connection = new SQLiteConnection("Data Source=" + ConnectionString))
                {
                    string sql = "update products_to_residents set amount = @amount where resident_id = @resident_id and product_id = @product_id";
                    connection.Execute(sql, new { resident_id = resident.ID, product_id = product.ID, amount });
                }
            }    
            
            public List<TableInfo> GetTableInfo()
            {
                using (var connection = new SQLiteConnection("Data Source=" + ConnectionString))
                {
                    string sql = "PRAGMA table_info(residents)";
                    var tableInfos = connection.Query<TableInfo>(sql);
                    return (List<TableInfo>)tableInfos;
                }
            }
        }

        public class Exportable
        {
            private float price;

            [Name("Vorname")]
            public string FirstName { get; set; }
            [Name("Nachname")]
            public string LastName { get; set; }
            [Name("SVN")]
            public string SSN { get; set; }
            [Name("Stockwerk")]
            public int Floor { get; set; }
            [Name("Produkt")]
            public string ProductName { get; set; }
            [Name("Anzahl")]
            public int Amount { get; set; }
            [Name("Preis [€]")]
            public float Price { 
                get 
                { 
                    return (float)Math.Round(price, 4);
                }
                set { price = value; }
            }

            [Name("MWST 9.5% [€]")]
            public float TaxOfPrice
            {
                get
                {
                    return (float)Math.Round((Price * 0.095), 4);
                }
            }

            [Name("Gesamtpreis [€]")]
            public float PriceWithTax
            {
                get
                {
                    return (float)Math.Round((Price * 1.095), 4);
                }
            }

            public Exportable(String firstName, String lastName, String ssn, Int64 floor, String productName, Double totalPrice, Int64 amount)
            {
                FirstName = firstName;
                LastName = lastName;
                SSN = ssn;
                Floor = (int)floor;
                ProductName = productName;
                Amount = (int)amount;
                Price = (float)totalPrice;
            }

            public Exportable(String firstName, String lastName, String ssn, Int64 floor, String productName, Int64 totalPrice, Int64 amount)
            {
                FirstName = firstName;
                LastName = lastName;
                SSN = ssn;
                Floor = (int)floor;
                ProductName = productName;
                Amount = (int)amount;
                Price = (float)totalPrice;
            }
        }

        public class TableInfo
        {
            public string Name { get; set; }
            public string ViewableName { get; set; }

            public bool Searchable { get; set; }

            public TableInfo(string name, string viewableName, bool searchable = false)
            {
                Name = name;
                ViewableName = viewableName;
                Searchable = searchable;
            }

            public override string ToString()
            {
                return ViewableName;
            }
        }

        public class Allowance
        {
            public float Value { get; set; }

            public Allowance()
            {

            }
        }
    }
}
