using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace BoltKezeloApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private StoreManager store = new StoreManager();

        public MainWindow()
        {
            InitializeComponent();
            LoadProducts("termekek.txt");
            LoadCustomers("vasarlok.txt");
            RefreshViews();
        }

        private void LoadProducts(string fileName)
        {
            if (!File.Exists(fileName)) return;
            string[] lines = File.ReadAllLines(fileName);
            foreach (string line in lines)
            {
                string[] parts = line.Split(';');
                if (parts.Length == 4)
                {
                    string code = parts[0];
                    string name = parts[1];
                    decimal price = decimal.Parse(parts[2]);
                    int stock = int.Parse(parts[3]);

                    Product product = new Product
                    {
                        ProductCode = code,
                        Name = name,
                        Price = price,
                        Stock = stock
                    };

                    store.Products.Add(product);
                }
            }
        }

        private void LoadCustomers(string fileName)
        {
            if (!File.Exists(fileName)) return;
            string[] lines = File.ReadAllLines(fileName);
            foreach (string line in lines)
            {
                string[] parts = line.Split(';');
                if (parts.Length >= 2)
                {
                    string id = parts[0];
                    string name = parts[1];

                    Customer customer = new Customer { CustomerId = id, Name = name };

                    for (int i = 2; i < parts.Length; i++)
                    {
                        string[] data = parts[i].Split(',');
                        if (data.Length == 2)
                        {
                            string productCode = data[0];
                            int quantity = int.Parse(data[1]);

                            Product product = store.Products.FirstOrDefault(p => p.ProductCode == productCode);
                            if (product != null)
                            {
                                PurchasedItem item = new PurchasedItem
                                {
                                    Product = product,
                                    Quantity = quantity
                                };
                                customer.Purchases.Add(item);
                            }
                        }
                    }
                    store.Customers.Add(customer);
                }
            }
        }

        private void RefreshViews()
        {
            lvProducts.ItemsSource = null;
            lvProducts.ItemsSource = store.Products;

            List<CustomerViewModel> customerViewModels = new List<CustomerViewModel>();
            foreach (Customer customer in store.Customers)
            {
                CustomerViewModel vm = new CustomerViewModel
                {
                    Name = customer.Name,
                    Purchases = customer.Purchases
                        .Select(p => $"{p.Product.Name} - {p.Quantity} db")
                        .ToList()
                };
                customerViewModels.Add(vm);
            }
            tvCustomers.ItemsSource = customerViewModels;
        }


        private void MenuAdd_Click(object sender, RoutedEventArgs e)
        {
            MainView.Visibility = Visibility.Collapsed;
            StatisticsView.Visibility = Visibility.Collapsed;
            AddView.Visibility = Visibility.Visible;
        }
        private void MenuMainView_Click(object sender, RoutedEventArgs e)
        {
            MainView.Visibility = Visibility.Visible;
            StatisticsView.Visibility = Visibility.Collapsed;
            AddView.Visibility = Visibility.Collapsed;
        }

        private void MenuStatistics_Click(object sender, RoutedEventArgs e)
        {
            ShowAllPurchases();
            MainView.Visibility = Visibility.Collapsed;
            StatisticsView.Visibility = Visibility.Visible;
            AddView.Visibility = Visibility.Collapsed;
        }

        private void ShowAllPurchases()
        {
            Dictionary<string, int> stats = new Dictionary<string, int>();

            foreach (Customer customer in store.Customers)
            {
                foreach (PurchasedItem purchase in customer.Purchases)
                {
                    string code = purchase.Product.ProductCode;
                    int quantity = purchase.Quantity;

                    if (stats.ContainsKey(code))
                        stats[code] += quantity;
                    else
                        stats[code] = quantity;
                }
            }

            if (stats.Any())
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Termék vásárlási statisztika:\n");

                foreach (var entry in stats.OrderByDescending(x => x.Value))
                {
                    Product p = store.Products.FirstOrDefault(prod => prod.ProductCode == entry.Key);
                    if (p != null)
                    {
                        sb.AppendLine($"{p.Name} ({p.ProductCode}) - {entry.Value} db");
                    }
                }

                tbStatistics.Text = sb.ToString();
            }
            else
            {
                tbStatistics.Text = "Nincs vásárlási adat.";
            }
        }

        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
           
            string code = tbProductCode.Text.Trim();
            string name = tbProductName.Text.Trim();
            if (!decimal.TryParse(tbProductPrice.Text.Trim(), out decimal price))
            {
                MessageBox.Show("Az ár nem érvényes szám.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!int.TryParse(tbProductStock.Text.Trim(), out int stock))
            {
                MessageBox.Show("A készlet nem érvényes szám.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Kérlek töltsd ki a termék kódját és nevét.", "Hiányzó adat", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

          
            if (store.Products.Any(p => p.ProductCode == code))
            {
                MessageBox.Show("Ez a termékkód már létezik.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

           
            Product newProduct = new Product
            {
                ProductCode = code,
                Name = name,
                Price = price,
                Stock = stock
            };

            store.Products.Add(newProduct);

            
            RefreshViews();

            tbProductCode.Clear();
            tbProductName.Clear();
            tbProductPrice.Clear();
            tbProductStock.Clear();

            MessageBox.Show("Termék sikeresen hozzáadva!", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);

            
            MainView.Visibility = Visibility.Visible;
            AddView.Visibility = Visibility.Collapsed;
        }

        private void BtnAddCustomer_Click(object sender, RoutedEventArgs e)
        {
            string id = tbCustomerId.Text.Trim();
            string name = tbCustomerName.Text.Trim();

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Kérlek töltsd ki az azonosítót és a nevet.", "Hiányzó adat", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            
            if (store.Customers.Any(c => c.CustomerId == id))
            {
                MessageBox.Show("Ez az azonosító már használatban van.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Customer newCustomer = new Customer
            {
                CustomerId = id,
                Name = name
            };

            store.Customers.Add(newCustomer);

            RefreshViews();

            tbCustomerId.Clear();
            tbCustomerName.Clear();

            MessageBox.Show("Vásárló sikeresen hozzáadva!", "Siker", MessageBoxButton.OK, MessageBoxImage.Information);

            MainView.Visibility = Visibility.Visible;
            AddView.Visibility = Visibility.Collapsed;
        }





    }


    public class Product
    {
        public string ProductCode { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }

    public class PurchasedItem
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
    }

    public class Customer
    {
        public string Name { get; set; }
        public string CustomerId { get; set; }
        public List<PurchasedItem> Purchases { get; set; } = new List<PurchasedItem>();
    }

    public class StoreManager
    {
        public List<Product> Products { get; set; } = new List<Product>();
        public List<Customer> Customers { get; set; } = new List<Customer>();
    }

    public class CustomerViewModel
    {
        public string Name { get; set; }
        public List<string> Purchases { get; set; }
    }
}
