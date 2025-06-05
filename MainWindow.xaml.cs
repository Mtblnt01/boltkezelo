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
using System.Net.NetworkInformation;

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

        private void MenuMainView_Click(object sender, RoutedEventArgs e)
        {
            MainView.Visibility = Visibility.Visible;
            StatisticsView.Visibility = Visibility.Collapsed;
        }

        private void MenuStatistics_Click(object sender, RoutedEventArgs e)
        {
            ShowAllPurchases();
            MainView.Visibility = Visibility.Collapsed;
            StatisticsView.Visibility = Visibility.Visible;
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