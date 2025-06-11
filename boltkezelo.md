# Bolt kezelo alkalmazás 


# Feladat leírása :

    Kettő txt-ből beolvassa az hogy kicsoda micsodát vett külön külön listázva , az alkalmazás három fő nézetből áll: Főnézet, Statisztika nézet és Hozzáadás nézet.


# Grid 

    DockPanel-t használ, felső menüvel, majd alatta három Grid (MainView, StatisticsView, AddView), amelyek a nézeteket tartalmazzák a MainView két oszlopban jeleníti meg a termékeket ListView-ban és a vásárlókat TreeView-ban. A StatisticsView egy egyszerű statisztikát jelenít meg TextBlock-ban, míg az AddView űrlapokat tartalmaz új termék és vásárló hozzáadásához


```c#

 <Menu DockPanel.Dock="Top">
            <MenuItem Header="Nézetek">
                <MenuItem Header="Főnézet" Click="MenuMainView_Click"/>
                <MenuItem Header="Statisztika" Click="MenuStatistics_Click"/>
                <MenuItem Header="Hozzáadás" Click="MenuAdd_Click"/>
            </MenuItem>
        </Menu>

        <Grid x:Name="MainView" Margin="10">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*" />
                <ColumnDefinition Width="*" />
            </Grid.ColumnDefinitions>

            <GroupBox Header="Termékek" Grid.Column="0" Margin="5">
                <ListView x:Name="lvProducts">
             
            </GroupBox>

            <GroupBox Header="Vásárlók" Grid.Column="1" Margin="5">
              
        </Grid>


        <Grid x:Name="StatisticsView" Visibility="Collapsed" Margin="10">
            
        </Grid>


        <Grid x:Name="AddView" Visibility="Collapsed" Margin="10">
            <StackPanel Orientation="Vertical" Margin="10" VerticalAlignment="Top">
                <GroupBox Header="Új termék hozzáadása" Margin="0,0,0,10">
                  
                </GroupBox>

                <GroupBox Header="Új vásárló hozzáadása">
                
            </StackPanel>
        </Grid>

```

# Code behind 


A c# kódban látható hogy a StoreManager osztály kezeli az adatokat (termékek és vásárlók listáit) a betöltés szöveges fájlokból történik.
Több osztály is van felvéve a kódban minden dolog külön külön dolgot kapott 

```c#

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

```


# LoadProducts

A loadproductsnak a file nevét kell átadjuk hogy utána abból kitudja szedni a file összes sorát de viszont csak akkor jó hogyha a formátuma kód;név;ár;készlet 
a metódus előszőr ellenőrzi hogy létezik-e a megadott nevű file és ha nem akkor kilép. Ha ez sikeres volt akkor a ReadAllLines-al minden sort beolvas és a sorokat ";" alatt spliteli szét. A sornakj muszaj hogy a megadott formatumba legyen megírva más esetben nem fog működni és crashel a program


```c#
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

```

# LoadCustomers

Egy filebol kiolvassa a vásárlók adatait de ez is cask egy megadott formátumban jó ami mégpedig a azonosító;név;termékkód1,mennyiség1;termékkód2,mennyiség2;... 
egy vásárló több mindent is vásárolhat. Ha a file nem létezik akkor a metódus kilép azonban ha létezik a file akkor itt is beolvassa a file összes sorát és betölti azokat egy tömbbe. Splitelve ";" alapján a sorokat feldaraboljuk az első kettő a vásárló neve es idja. Ezek alapjan csinal egy Customer objektumot , az összes többi része a sornak végtelen mennyiségbe tartalmazhat termékkódot és mennyiséget. Minden vásárlást köülön külön szétszed vessző alapján splitelve majd productCode alapján megkeresi a terméket a store.Products listában ha megtalálja csinál egy PurchasedItems objektumot és hozzáadja a vásárlóhoz.


```c#
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

```

# RefreshViews

