using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace inkomate
{
    internal class Product
    {
        private int id;
        private string name;
        private int amount;
        private float price;

        public int ID
        {
            get
            {
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
                    throw new Exception("Id can not be less than 0");
                }

            }
        }

        public string Name { get { return name; } set { name = value; } }

        public int Amount
        {
            get
            {
                return amount;
            }
            set
            {
                if (value >= 0)
                {
                    amount = value;
                }
                else
                {
                    throw new Exception("Anzahl darf nicht kleiner als 0 sein");
                }

            }
        }

        public float Price
        {
            get
            {
                return price;
            }
            set
            {
                if (value >= 0)
                {
                    price = value;
                }
                else
                {
                    throw new Exception("Preis darf nicht kleiner als 0 sein");
                }

            }
        }

        public float PackagePrice
        {
            get
            {
                return (float)(Math.Round(Price * Amount, 4));
            }
        }

        public int TotalAmount
        {
            get; set;
        }

        public float TotalPrice
        {
            get
            {
                return (float)(Math.Round(TotalAmount * PackagePrice, 4));
            }
        }

        public float TaxOfPrice
        {
            get
            {
                return (float)Math.Round((TotalPrice * 0.095), 4);
            }
        }

        public float PriceWithTax
        {
            get
            {
                return (float)Math.Round((TotalPrice * 1.095), 4);
            }
        }

        public override string ToString()
        {
            return this.Name;
        }

        public Product(string name, float price, int amount)
        {
            this.Name = name;
            this.Price = price;
            this.Amount = amount;
        }

        public Product(Int64 id, String name, Decimal price, Int64 amount)
        {
            this.ID = (int)id;
            this.Name = name;
            this.Price = (float)price;
            this.Amount = (int)amount;
        }

        public Product(Int64 id, String name, Decimal price, Int64 amount, Int64 totalAmount)
        {
            this.ID = (int)id;
            this.Name = name;
            this.Price = (float)price;
            this.Amount = (int)amount;
            this.TotalAmount = (int)totalAmount;
        }

        public class DatabaseHandler
        {
            private string connectionString;

            public string ConnectionString { get { return connectionString; } set { connectionString = value; } }

            public DatabaseHandler()
            {
                ConnectionString = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "../../../inkomate.sqlite");
            }

            public List<Product> SelectAllProducts()
            {
                using (var connection = new SQLiteConnection("Data Source=" + ConnectionString))
                {
                    string sql = "select * from products";
                    var temp = connection.Query<Product>(sql);
                    List<Product> products = (List<Product>)temp;
                    return products.OrderBy(p => p.Name).ToList();
                }
            }

            public void InsertNewProduct(Product product)
            {
                try
                {
                    using (var connection = new SQLiteConnection("Data Source=" + ConnectionString))
                    {
                        var parameters = new
                        {
                            name = product.Name,
                            price = product.Price,
                            amount = product.Amount,
                        };

                        string sql = "insert into products (name, price, amount) " +
                            "values (@name, @price, @amount)";

                        connection.Execute(sql, parameters);
                    }
                }
                catch(Exception ex)
                {
                    if (ex.Message.Contains("UNIQUE"))
                    {
                        throw new Exception("Inkontinezprodukt mit diesem Namen wurde bereits hinzugefügt");
                    }
                }
            }

            public List<Product> SelectProductsOfResident(Resident resident)
            {
                using (var connection = new SQLiteConnection("Data Source=" + ConnectionString))
                {
                    string sql = "select pro.id, pro.name, pro.price, pro.amount, protores.amount as totalAmount from residents as res " +
                        "inner join products_to_residents as protores " +
                        "on protores.resident_id = res.id and res.id = @resident_id " +
                        "inner join products as pro " +
                        "on pro.id = protores.product_id";
                    var products = connection.Query<Product>(sql, new {resident_id = resident.ID});
                    return (List<Product>)products;
                }
            }

            public void UpdateProduct(Product product)
            {
                try
                {
                    using (var connection = new SQLiteConnection("Data Source=" + ConnectionString))
                    {
                        var parameters = new
                        {
                            name = product.Name,
                            price = product.Price,
                            amount = product.Amount,
                            id = product.ID
                        };

                        string sql = "update products " +
                            "set name = @name, price = @price, amount = @amount " +
                            "where id = @id";

                        connection.Execute(sql, parameters);
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("UNIQUE"))
                    {
                        throw new Exception("Inkontinezprodukt mit diesem Namen wurde bereits hinzugefügt");
                    }
                }
            }

            public void DeleteProduct(Product product)
            {
                using (var connection = new SQLiteConnection("Data Source=" + ConnectionString))
                {

                    string sql = "delete from products where id = @id; delete from products_to_residents where product_id = @id";

                    connection.Execute(sql, new { id = product.ID });
                }
            }
        }


    }
}
