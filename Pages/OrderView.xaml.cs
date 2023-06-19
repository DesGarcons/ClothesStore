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

namespace ClothesStore.Pages
{
    /// <summary>
    /// Логика взаимодействия для OrderView.xaml
    /// </summary>
    public partial class OrderView : Page
    {
        List<ProductOrder> productOrders;
        Order order;
        decimal AllProductPrice;
        decimal AllProductDiscount;

        public OrderView(List<Product> products)
        {
            InitializeComponent();
            order = new Order();
            productOrders = new List<ProductOrder>();
            PickUpPointCB.ItemsSource = AppData.context.PickUpPoint.ToList();
            try
            {
                order.ID = AppData.context.Order.Max(p => p.ID) + 1;
            }
            catch
            {
                order.ID = 1;
            }
            
            foreach (var product in products)
            {
                var existingProductInOrder = productOrders.FirstOrDefault(x => x.ProductID == product.ID);
                if (existingProductInOrder != null)
                {
                    existingProductInOrder.QuantityProductInOrder++;
                }
                else
                {
                    var productOrder = new ProductOrder()
                    {
                        ProductID = product.ID,
                        Product = product,
                        OrderID = order.ID,
                        QuantityProductInOrder = 1
                    };
                    productOrders.Add(productOrder);
                }
            }
            ListProductsInOrder.ItemsSource = productOrders;
            CalCul(productOrders);

        }

        private void PlusQuantity_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).DataContext is ProductOrder orderProduct)
            {
                var productInOrder = productOrders.Find(x => x.ProductID == orderProduct.ProductID);
                if (productInOrder != null)
                {
                    productInOrder.QuantityProductInOrder++;
                }
                ListProductsInOrder.ItemsSource = productOrders;
                CalCul(productOrders);
                ListProductsInOrder.Items.Refresh();
            }
        }

        private void MinusQantity_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).DataContext is ProductOrder orderProduct)
            {
                var productInOrder = productOrders.Where(p => p.ProductID == orderProduct.ProductID);
                if (productInOrder != null)
                {
                    productInOrder.FirstOrDefault().QuantityProductInOrder--;
                    if (productInOrder.FirstOrDefault().QuantityProductInOrder == 0)
                    {
                        var result = MessageBox.Show("Товар будет удален из корзины. Вы желаете продолжить?", "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (result == MessageBoxResult.Yes)
                        {
                            productOrders.Remove(productInOrder.FirstOrDefault());
                        }
                        else
                        {
                            productInOrder.FirstOrDefault().QuantityProductInOrder = 1;
                        }
                    }
                }
                ListProductsInOrder.ItemsSource = productOrders;
                CalCul(productOrders);
                ListProductsInOrder.Items.Refresh();
            }
        }
        public void CalCul(List<ProductOrder> productOrders)
        {
            AllProductDiscount = 0;
            AllProductPrice = 0;

            foreach (var item in productOrders)
            {
                decimal ProductPrice = item.Product.Price * item.QuantityProductInOrder;
                AllProductPrice += ProductPrice;
                if (item.Product.Discount != null)
                {
                    decimal ProductDiscount = (decimal)(item.Product.Discount * item.Product.Price * item.QuantityProductInOrder / 100);
                    AllProductDiscount += ProductDiscount;
                }
                AllProductPrice -= AllProductDiscount;
            }
            TotalDiscount.Text = $"{AllProductDiscount.ToString()} Руб";
            TotalPrice.Text = $"{AllProductPrice.ToString()} Руб";
        }

        private void FormOrder_Click(object sender, RoutedEventArgs e)
        {
            if (PickUpPointCB.SelectedItem != null)
            {
                try
                {
                    var MaxgetCode = AppData.context.Order.Max(p => p.PickUpPointCode);
                    var newOrder = new Order()
                    {
                        DateDelivery = DateDelivery(productOrders),
                        StatusID = 1,
                        PickUpPointID = ((PickUpPoint)PickUpPointCB.SelectedItem).ID,
                        PickUpPointCode = MaxgetCode + 1
                    };
                    AppData.context.Order.Add(newOrder);

                    foreach (var item in productOrders)
                    {
                        var productOrder = new ProductOrder()
                        {
                            ProductID = item.ProductID,
                            OrderID = newOrder.ID,
                            QuantityProductInOrder = item.QuantityProductInOrder
                        };
                        AppData.context.ProductOrder.Add(productOrder);
                    }
                    AppData.context.SaveChanges();
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine("Информация о заказе\n");
                    stringBuilder.AppendLine($"\nНомер заказа: {newOrder.ID}\n");
                    stringBuilder.AppendLine($"Пункт выдачи: {((PickUpPoint)PickUpPointCB.SelectedItem).Name}\n");
                    stringBuilder.AppendLine($"Дата доставки: {newOrder.DateDelivery}\n");
                    stringBuilder.AppendLine($"Код для получения: {newOrder.PickUpPointCode}\n");
                    stringBuilder.AppendLine($"Общая сумма скидки : {AllProductDiscount.ToString()} Руб\n");
                    stringBuilder.AppendLine($"Итоговая стоимость заказа: {AllProductPrice.ToString()} Руб\n");
                    stringBuilder.AppendLine($"\nСписок продуктов: \n");
                    foreach(var item in productOrders)
                    {
                        var product = productOrders.Where(p => p.ProductID == item.ProductID);
                        stringBuilder.AppendLine($"{item.Product.Name} - {item.QuantityProductInOrder} шт\n");
                    }

                    var message = stringBuilder.ToString();
                    MessageBox.Show(message, "Информация о заказе", MessageBoxButton.OK, MessageBoxImage.Information);
                    ListProductsInOrder.ItemsSource = null;
                    PickUpPointCB.ItemsSource = null;
                    TotalDiscount.Text = null;
                    TotalPrice.Text = null;


                }
                catch
                {
                    order.PickUpPointCode = 100;
                    var newOrder = new Order()
                    {
                        DateDelivery = DateDelivery(productOrders),
                        StatusID = 1,
                        PickUpPointID = ((PickUpPoint)PickUpPointCB.SelectedItem).ID,
                        PickUpPointCode = order.PickUpPointCode
                    };
                    AppData.context.Order.Add(newOrder);

                    foreach (var item in productOrders)
                    {
                        var productOrder = new ProductOrder()
                        {
                            ProductID = item.ProductID,
                            OrderID = newOrder.ID,
                            QuantityProductInOrder = item.QuantityProductInOrder
                        };
                        AppData.context.ProductOrder.Add(productOrder);
                    }
                    AppData.context.SaveChanges();

                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine("Информация о заказе\n");
                    stringBuilder.AppendLine($"\nНомер заказа: {newOrder.ID}\n");
                    stringBuilder.AppendLine($"Пункт выдачи: {((PickUpPoint)PickUpPointCB.SelectedItem).Name}\n");
                    stringBuilder.AppendLine($"Дата доставки: {newOrder.DateDelivery}\n");
                    stringBuilder.AppendLine($"Код для получения: {newOrder.PickUpPointCode}\n");
                    stringBuilder.AppendLine($"Общая сумма скидки : {AllProductDiscount.ToString()} Руб\n");
                    stringBuilder.AppendLine($"Итоговая стоимость заказа: {AllProductPrice.ToString()} Руб\n");
                    stringBuilder.AppendLine($"\nСписок продуктов: \n");
                    foreach (var item in productOrders)
                    {
                        var product = productOrders.Where(p => p.ProductID == item.ProductID);
                        stringBuilder.AppendLine($"{item.Product.Name} - {item.QuantityProductInOrder} шт\n");
                    }

                    var message = stringBuilder.ToString();
                    MessageBox.Show(message, "Информация о заказе", MessageBoxButton.OK, MessageBoxImage.Information);
                    ListProductsInOrder.ItemsSource = null;
                    PickUpPointCB.ItemsSource = null;
                    TotalDiscount.Text = null;
                    TotalPrice.Text = null;
                }
              
            }
            else
            {
                MessageBox.Show("Выберите пункт выдачи", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static DateTime DateDelivery(List<ProductOrder> productOrders)
        {
            var DateProduct = productOrders.Any(p => p.Product.Quantity < 3);
            if (DateProduct)
            {
                return DateTime.Now.AddDays(6);
            }
            else
            {
                return DateTime.Now.AddDays(3);
            }
        }
    }
}
