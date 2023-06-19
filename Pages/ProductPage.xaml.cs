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
using ClothesStore.DataBase;
using ClothesStore.Scripts;

namespace ClothesStore.Pages
{
    /// <summary>
    /// Логика взаимодействия для ProductPage.xaml
    /// </summary>
    public partial class ProductPage : Page
    {
        List<Product> products;
        public ProductPage()
        {
            InitializeComponent();
            ListProducts.ItemsSource = AppData.context.Product.ToList();
        }

        private void AddProductInOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            if (products == null)
            {
                products = new List<Product>();
            }
            var selectedProduct = (Product)ListProducts.SelectedItem;
            if (selectedProduct != null)
            {
                products.Add(selectedProduct);
                MessageBox.Show("Товар добавлен в корзину");
                ShowOrder.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show("Выберите товар и нажмите кнопку Добавить");
            }
        }

        private void ShowOrder_Click(object sender, RoutedEventArgs e)
        {
            OrderView orderView = new OrderView(products);
            Manager.MainFrame.Navigate(new OrderView(products));
        }

        private void AddProductInOrderMouse_Click(object sender, RoutedEventArgs e)
        {
            if (products == null)
            {
                products = new List<Product>();
            }
            if ((sender as MenuItem).DataContext is Product productMenu)
            {
                products.Add(productMenu);
                MessageBox.Show("Товар добавлен в корзину");
                ShowOrder.Visibility = Visibility.Visible;
            }

        }
    }
}
