using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.IO;
using System.Text;
using Final_ProjectCS.Models;

namespace Final_ProjectCS.Models
{
    public class Market
    {
        private List<User> users = new List<User>();
        private List<Product> products = new List<Product>();
        private List<Admin> admins = new List<Admin> { new Admin { Username = "admin", Password = "admin123" } };
        private User currentUser = null;
        private Admin currentAdmin = null;

        private const string UsersFilePath = "users.json";
        private const string ProductsFilePath = "products.json";
        private const string AdminsFilePath = "admins.json";

        public Market()
        {
            LoadData();
        }

        public void Run()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Welcome to the Market");
                string[] options = { "User Login", "User Registration", "Admin Login", "Exit" };
                int choice = GetMenuChoice(options);

                switch (choice)
                {
                    case 0:
                        UserLogin();
                        break;
                    case 1:
                        UserRegistration();
                        break;
                    case 2:
                        AdminLogin();
                        break;
                    case 3:
                        SaveData();
                        return;
                }
            }
        }

        private int GetMenuChoice(string[] options)
        {
            int index = 0;
            ConsoleKey key;

            do
            {
                Console.Clear();
                for (int i = 0; i < options.Length; i++)
                {
                    if (i == index)
                    {
                        Console.Write("> ");
                    }
                    else
                    {
                        Console.Write("  ");
                    }
                    Console.WriteLine(options[i]);
                }

                key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.UpArrow)
                {
                    index = (index == 0) ? options.Length - 1 : index - 1;
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    index = (index == options.Length - 1) ? 0 : index + 1;
                }

            } while (key != ConsoleKey.Enter);

            return index;
        }

        private void UserLogin()
        {
            Console.Clear();
            Console.Write("Enter username: ");
            string username = Console.ReadLine();
            Console.Write("Enter password: ");
            string password = MaskedInput();

            currentUser = users.FirstOrDefault(u => u.Username == username && u.Password == password);
            if (currentUser != null)
            {
                UserPanel();
            }
            else
            {
                Console.WriteLine("Invalid credentials");
                Console.ReadLine();
            }
        }

        private void UserRegistration()
        {
            Console.Clear();
            Console.Write("Enter username: ");
            string username = Console.ReadLine();
            Console.Write("Enter password: ");
            string password = MaskedInput();
            Console.Write("Enter email: ");
            string email = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email))
            {
                Console.WriteLine("All fields are required.");
                Console.ReadLine();
                return;
            }

            if (!email.Contains('@'))
            {
                Console.WriteLine("Invalid Email");
                Console.ReadLine();
                return;
            }

            users.Add(new User { Username = username, Password = password, Email = email });
            Console.WriteLine("Registration successful");
            Console.ReadLine();
        }

        private void AdminLogin()
        {
            Console.Clear();
            Console.Write("Enter admin username: ");
            string username = Console.ReadLine();
            Console.Write("Enter admin password: ");
            string password = MaskedInput();

            currentAdmin = admins.FirstOrDefault(a => a.Username == username && a.Password == password);
            if (currentAdmin != null)
            {
                AdminPanel();
            }
            else
            {
                Console.WriteLine("Invalid credentials");
                Console.ReadLine();
            }
        }

        private void UserPanel()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Welcome, " + currentUser.Username);
                string[] options = { "View Categories", "View Cart", "View Profile", "View Purchase History", "Update Personal Information", "Change Password", "Logout" };
                int choice = GetMenuChoice(options);

                switch (choice)
                {
                    case 0:
                        ViewCategories();
                        break;
                    case 1:
                        ViewCart();
                        break;
                    case 2:
                        ViewProfile();
                        break;
                    case 3:
                        ViewPurchaseHistory();
                        break;
                    case 4:
                        UpdatePersonalInformation();
                        break;
                    case 5:
                        ChangePassword();
                        break;
                    case 6:
                        currentUser = null;
                        return;
                }
            }
        }

        private void ViewCategories()
        {
            Console.Clear();
            var categories = products.Select(p => p.Category).Distinct().ToList();
            int choice = GetMenuChoice(categories.Append("Back").ToArray());

            if (choice < categories.Count)
            {
                ViewProducts(categories[choice]);
            }
        }

        private void ViewProducts(string category)
        {
            Console.Clear();
            var categoryProducts = products.Where(p => p.Category == category && p.Stock > 0).ToList();
            var productNames = categoryProducts.Select(p => $"{p.Name} - ${p.Price} - Stock: {p.Stock}").ToArray();
            int choice = GetMenuChoice(productNames.Append("Back").ToArray());

            if (choice < categoryProducts.Count)
            {
                AddToCart(categoryProducts[choice]);
            }
        }

        private void AddToCart(Product product)
        {
            Console.Clear();
            Console.WriteLine($"Adding {product.Name} to cart.");
            if (currentUser.Cart.ContainsKey(product.Name))
            {
                currentUser.Cart[product.Name]++;
            }
            else
            {
                currentUser.Cart[product.Name] = 1;
            }
            product.Stock--;
            Console.WriteLine("Product added to cart. Press any key to continue.");
            Console.ReadLine();
        }

        private void ViewCart()
        {
            Console.Clear();
            double totalPrice = 0;
            List<string> cartItems = new List<string>();

            foreach (var item in currentUser.Cart)
            {
                var product = products.First(p => p.Name == item.Key);
                double price = product.Price * item.Value;
                totalPrice += price;
                cartItems.Add($"{item.Key} - {item.Value} pcs - ${price}");
            }

            cartItems.Add($"Total Price: ${totalPrice}");
            cartItems.Add("Confirm Cart");
            cartItems.Add("Remove Item");
            cartItems.Add("Back");

            while (true)
            {
                int choice = GetMenuChoice(cartItems.ToArray());

                switch (choice)
                {
                    case int n when (n < currentUser.Cart.Count):
                        // This is a product display, no action required
                        break;
                    case int n when (n < currentUser.Cart.Count):
                        // This is a product display, no action required
                        break;
                    case int n when (n == currentUser.Cart.Count + 1):
                        // Confirm Cart
                        ConfirmCart(totalPrice);
                        return;
                    case int n when (n == currentUser.Cart.Count + 2):
                        // Remove Item
                        RemoveItem();
                        return;
                    case int n when (n == currentUser.Cart.Count + 3):
                        // Back
                        SaveData();
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        Console.ReadLine();
                        break;
                }
            }
        }



        private void ConfirmCart(double totalPrice)
        {
            Console.Clear();
            Console.WriteLine($"Total Price: ${totalPrice}");
            Console.Write("Enter payment amount: ");
            if (double.TryParse(Console.ReadLine(), out double payment))
            {
                if (payment >= totalPrice)
                {
                    double change = payment - totalPrice;
                    Console.WriteLine($"Payment successful. Your change is ${change}");
                    foreach (var item in currentUser.Cart)
                    {
                        currentUser.PurchaseHistory.Add(item.Key + " - " + item.Value + " pcs");
                    }
                    currentUser.Cart.Clear();
                }
                else
                {
                    Console.WriteLine("Insufficient payment.");
                }
            }
            else
            {
                Console.WriteLine("Invalid payment amount.");
            }
            Console.ReadLine();
        }

        private void RemoveItem()
        {
            Console.Clear();
            var cartItems = currentUser.Cart.Keys.ToList();
            int choice = GetMenuChoice(cartItems.Append("Back").ToArray());

            if (choice < cartItems.Count)
            {
                var product = products.First(p => p.Name == cartItems[choice]);
                product.Stock += currentUser.Cart[product.Name];
                currentUser.Cart.Remove(product.Name);
                Console.WriteLine("Item removed. Press any key to continue.");
                Console.ReadLine();
            }
        }

        private void ViewProfile()
        {
            Console.Clear();
            Console.WriteLine($"Username: {currentUser.Username}");
            Console.WriteLine($"Email: {currentUser.Email}");
            Console.ReadLine();
        }

        private void ViewPurchaseHistory()
        {
            Console.Clear();
            foreach (var history in currentUser.PurchaseHistory)
            {
                Console.WriteLine(history);
            }
            Console.ReadLine();
        }

        private void UpdatePersonalInformation()
        {
            Console.Clear();
            Console.Write("Enter new email: ");
            string newEmail = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newEmail) && newEmail.Contains('@'))
            {
                currentUser.Email = newEmail;
                Console.WriteLine("Email updated successfully.");
            }
            else
            {
                Console.WriteLine("Invalid email.");
            }
            Console.ReadLine();
        }

        private void ChangePassword()
        {
            Console.Clear();
            Console.Write("Enter new password: ");
            string newPassword = MaskedInput();
            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                currentUser.Password = newPassword;
                Console.WriteLine("Password changed successfully.");
            }
            else
            {
                Console.WriteLine("Invalid password.");
            }
            Console.ReadLine();
        }

        private void AdminPanel()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Welcome, Admin");
                string[] options = { "Manage Stock", "Add Category", "View Users", "View Reports", "View Product Stocks", "Logout" };
                int choice = GetMenuChoice(options);

                switch (choice)
                {
                    case 0:
                        ManageStock();
                        break;
                    case 1:
                        AddCategory();
                        break;
                    case 2:
                        ViewUsers();
                        break;
                    case 3:
                        ViewReports();
                        break;
                    case 4:
                        ViewStocks();
                        break;
                    case 5:
                        currentAdmin = null;
                        return;
                }
            }
        }

        private void ViewStocks()
        {
            Console.Clear();
            Console.WriteLine("Product Stocks:");
            foreach (var product in products)
            {
                Console.WriteLine($"{product.Name} - {product.Stock} pcs");
            }
            Console.ReadLine();
        }

        private void ManageStock()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Manage Stock");
                string[] options = { "Add Product", "Update Product", "Delete Product", "Back" };
                int choice = GetMenuChoice(options);

                switch (choice)
                {
                    case 0:
                        AddProduct();
                        break;
                    case 1:
                        UpdateProduct();
                        break;
                    case 2:
                        DeleteProduct();
                        break;
                    case 3:
                        return;
                }
            }
        }

        private void DeleteProduct()
        {
            Console.Clear();
            var productNames = products.Select(p => p.Name).ToArray();
            int choice = GetMenuChoice(productNames.Append("Back").ToArray());

            if (choice < products.Count)
            {
                var productToDelete = products[choice];
                products.Remove(productToDelete);
                Console.WriteLine($"Product '{productToDelete.Name}' deleted successfully.");
            }
            else
            {
                Console.WriteLine("Invalid choice.");
            }
            Console.ReadLine();
        }

        private void AddProduct()
        {
            Console.Clear();
            Console.Write("Enter product name: ");
            string name = Console.ReadLine();
            Console.Write("Enter category: ");
            string category = Console.ReadLine();
            Console.Write("Enter stock: ");
            if (int.TryParse(Console.ReadLine(), out int stock) && stock >= 0)
            {
                Console.Write("Enter price: ");
                if (double.TryParse(Console.ReadLine(), out double price) && price >= 0)
                {
                    products.Add(new Product { Name = name, Category = category, Stock = stock, Price = price });
                    Console.WriteLine("Product added successfully.");
                }
                else
                {
                    Console.WriteLine("Invalid price.");
                }
            }
            else
            {
                Console.WriteLine("Invalid stock.");
            }
            Console.ReadLine();
        }

        private void UpdateProduct()
        {
            Console.Clear();
            var productNames = products.Select(p => p.Name).ToArray();
            int choice = GetMenuChoice(productNames.Append("Back").ToArray());

            if (choice < products.Count)
            {
                var product = products[choice];
                Console.Write("Enter new stock: ");
                if (int.TryParse(Console.ReadLine(), out int newStock) && newStock >= 0)
                {
                    Console.Write("Enter new price: ");
                    if (double.TryParse(Console.ReadLine(), out double newPrice) && newPrice >= 0)
                    {
                        product.Stock = newStock;
                        product.Price = newPrice;
                        Console.WriteLine("Product updated successfully.");
                    }
                    else
                    {
                        Console.WriteLine("Invalid price.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid stock.");
                }
            }
            else
            {
                Console.WriteLine("Invalid choice.");
            }
            Console.ReadLine();
        }

        private void AddCategory()
        {
            Console.Clear();
            Console.Write("Enter new category name: ");
            string newCategory = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(newCategory))
            {
                // Check if the category already exists
                if (products.Any(p => p.Category.Equals(newCategory, StringComparison.OrdinalIgnoreCase)))
                {
                    Console.WriteLine($"Category '{newCategory}' already exists.");
                }
                else
                {
                    // Add the new category to products
                    products.Add(new Product { Category = newCategory });
                    Console.WriteLine($"Category '{newCategory}' added successfully.");
                }
            }
            else
            {
                Console.WriteLine("Invalid category name.");
            }
            Console.ReadLine();
        }

        private void ViewReports()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Sales Reports");
                string[] options = { "View Sales Report", "Reset Sales Reports", "Back" };
                int choice = GetMenuChoice(options);

                switch (choice)
                {
                    case 0:
                        Console.Clear();
                        DisplaySalesReport();
                        break;
                    case 1:
                        ResetSalesReports();
                        break;
                    case 2:
                        return;
                }
            }
        }

        private void DisplaySalesReport()
        {
            var report = from user in users
                         from item in user.PurchaseHistory
                         group item by item into g
                         select new { Product = g.Key, Count = g.Count() };

            double totalRevenue = 0;

            foreach (var entry in report)
            {
                var product = products.First(p => p.Name == entry.Product.Split(" - ")[0]);
                double revenue = entry.Count * product.Price;
                totalRevenue += revenue;
                Console.WriteLine($"{entry.Product} - Sold: {entry.Count} - Revenue: ${revenue}");
            }

            Console.WriteLine($"\nTotal Revenue: ${totalRevenue}");
            Console.ReadLine();
        }

        private void ResetSalesReports()
        {
            foreach (var user in users)
            {
                user.PurchaseHistory.Clear();
            }
            Console.WriteLine("Sales reports reset successfully.");
            Console.ReadLine();
        }

        private void ViewUsers()
        {
            Console.Clear();
            foreach (var user in users)
            {
                Console.WriteLine($"Username: {user.Username}, Email: {user.Email}");
            }
            Console.ReadLine();
        }


        private void LoadData()
        {
            if (File.Exists(UsersFilePath))
            {
                var usersData = File.ReadAllText(UsersFilePath);
                users = JsonSerializer.Deserialize<List<User>>(usersData);
            }

            if (File.Exists(ProductsFilePath))
            {
                var productsData = File.ReadAllText(ProductsFilePath);
                products = JsonSerializer.Deserialize<List<Product>>(productsData);
            }

            if (File.Exists(AdminsFilePath))
            {
                var adminsData = File.ReadAllText(AdminsFilePath);
                admins = JsonSerializer.Deserialize<List<Admin>>(adminsData);
            }
        }

        private void SaveData()
        {
            var usersData = JsonSerializer.Serialize(users);
            File.WriteAllText(UsersFilePath, usersData);

            var productsData = JsonSerializer.Serialize(products);
            File.WriteAllText(ProductsFilePath, productsData);

            var adminsData = JsonSerializer.Serialize(admins);
            File.WriteAllText(AdminsFilePath, adminsData);
        }

        private string MaskedInput()
        {
            var passwordBuilder = new StringBuilder();
            ConsoleKeyInfo keyInfo;

            while ((keyInfo = Console.ReadKey(true)).Key != ConsoleKey.Enter)
            {
                if (keyInfo.Key == ConsoleKey.Backspace && passwordBuilder.Length > 0)
                {
                    passwordBuilder.Length--;
                    Console.Write("\b \b");
                }
                else if (keyInfo.Key != ConsoleKey.Backspace)
                {
                    passwordBuilder.Append(keyInfo.KeyChar);
                    Console.Write("*");
                }
            }
            Console.WriteLine();
            return passwordBuilder.ToString();
        }
    }
}

