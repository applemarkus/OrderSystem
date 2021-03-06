﻿using OrderSystem.Data;
using OrderSystem.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Xml;
using OrderSystem.Enums;

namespace OrderSystem.Views.Pages
{
    /// <summary>
    /// The Order page
    /// </summary>
    public partial class OrderPage : AppPage
    {
        private ObservableCollection<Product> productList;
        private ObservableCollection<ProductLine> productTable;
        private ObservableCollection<Order> orderList;

        private ProductModel productModel;
        private OrderModel orderModel;
        private ProductLineModel productLineModel;

        public OrderPage()
        {
        }

        public override void LoadView()
        {
            InitializeComponent();
            LoadedView = true;
        }

        public override void LoadResources()
        {
            InitMembers();
            LoadProducts();
            LoadOrder();
            LoadedResources = true;
        }

        public override void ReloadResources()
        {
            LoadProducts();
            LoadOrder();
        }

        private void InitMembers()
        {
            productList = new ObservableCollection<Product>();
            productTable = new ObservableCollection<ProductLine>();
            orderList = new ObservableCollection<Order>();

            cbProduct.DataContext = this;
            dgProducts.DataContext = this;
            cbTimes.DataContext = this;

            productModel = (ProductModel)ModelRegistry.Get(ModelIdentifier.Product);
            productLineModel = (ProductLineModel)ModelRegistry.Get(ModelIdentifier.ProductLine);
            orderModel = (OrderModel)ModelRegistry.Get(ModelIdentifier.Order);
        }

        private void LoadProducts()
        {
            productList.Clear();

            foreach (Product p in productModel.GetAll())
            {
                productList.Add(p);
            }
        }

        private void LoadOrder()
        {
            orderList.Clear();

            foreach (Order o in orderModel.GetAvailableOrders())
            {
                orderList.Add(o);
            }
        }

        private void OnAddProductToTable(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cbProduct.SelectedIndex == -1)
                {
                    throw new Exception("Es muss ein Produkt ausgewählt werden.");
                }

                Product product = productList.ElementAt(cbProduct.SelectedIndex);
                uint quantity = (uint)(tbProductAmount.Value ?? 0);

                if (quantity <= 0)
                {
                    throw new Exception("Es müssen mehr als 0 Produkte bestellt werden.");
                }

                if (ProductExistInTable(product))
                {
                    AddQuantityToExistingProduct(quantity, product);
                }
                else
                {
                    productTable.Add(new ProductLine(quantity, product));
                }
                UpdateTotalPrice();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void AddQuantityToExistingProduct(uint quantity, Product product)
        {
            foreach (ProductLine line in productTable)
            {
                if (line.Product.Equals(product))
                {
                    line.Quantity += quantity;
                }
            }
        }

        private bool ProductExistInTable(Product product)
        {
            foreach (ProductLine line in productTable)
            {
                if (line.Product.Equals(product))
                {
                    return true;
                }
            }

            return false;
        }

        private void OnRemoveProduct(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgProducts.SelectedIndex == -1)
                {
                    throw new Exception("Es muss ein Produkt ausgewählt werden.");
                }

                productTable.RemoveAt(dgProducts.SelectedIndex);
                UpdateTotalPrice();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UpdateTotalPrice()
        {
            decimal price = 0;
            foreach (ProductLine line in productTable)
            {
                price += line.Price;
            }
            lbTotalPrice.Content = string.Format("Gesamt: € {0,00}", price);
        }

        private void OnCellValueChanged(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                //prevent endless loops
                dgProducts.RowEditEnding -= OnCellValueChanged;
                dgProducts.CommitEdit();
                UpdateTotalPrice();
                dgProducts.RowEditEnding += OnCellValueChanged;
            }
        }

        private void OnMakeOrder(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cbTimes.SelectedIndex == -1)
                {
                    throw new Exception("Bitte eine Uhrzeit auswählen.");
                }

                if (productTable.Count <= 0)
                {
                    throw new Exception("Es sind keine Produkte hinzugefügt worden.");
                }

                decimal sum = 0;
                foreach (ProductLine line in productTable)
                {
                    sum += line.Price;
                }

                if (sum > 1000)
                {
                    throw new Exception("Es dürfen nicht mehr als 1000€ bestellt werden.");
                }

                CreditModel creditModel = (CreditModel)ModelRegistry.Get(ModelIdentifier.Credit);

                if (sender.Equals(btOrderCredit))
                {
                    decimal credit = creditModel.GetCurrentCredit(Session.Instance.CurrentUserId);

                    if (credit < sum)
                    {
                        throw new Exception("Es is nicht ausreichend Guthaben vorhanden.");
                    }

                }

                Order o = (Order)cbTimes.SelectedValue;

                if (productLineModel.HasAlreadyOrdered(Session.Instance.CurrentUserId, o.Id))
                {
                    throw new Exception("Du hast bereits eine Bestellung für diese Uhrzeit abgegeben.");
                }

                if (!productLineModel.Submit(Session.Instance.CurrentUserId, o.Id, productTable.ToList()))
                {
                    throw new Exception("Reservierung konnte nicht aufgegeben werden.");
                }

                if (sender.Equals(btOrderAdmin))
                {
                    MessageBox.Show(
                    string.Format(
                        "Bestellung wird von einem Administrator bearbeitet. Bitte bezahle vor {0} Uhr bei ihm.",
                        o.Time));
                }
                else if (sender.Equals(btOrderCredit))
                {
                    if (!productLineModel.SetPaidOrder(o.Id, Session.Instance.CurrentUserId, true, PayType.Credit))
                    {
                        ClearOrder();
                        throw new Exception("Bestellungen konnte nicht über Guthaben bezahlt werden.");
                    }

                    if (creditModel.AddCredit(-sum, Session.Instance.CurrentUserId))
                    {
                        MessageBox.Show("Bestellung wurde mit Guthaben erfolgreich bezahlt.");
                    }
                }

                ClearOrder();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        
        private void ClearOrder()
        {
            cbTimes.SelectedIndex = -1;
            cbProduct.SelectedIndex = -1;
            tbProductAmount.Value = 1;
            productTable.Clear();
            UpdateTotalPrice();
            ReloadResources();
        }

        private void OnTimesSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            (new Thread(() =>
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    try
                    {
                        Order o = (Order)cbTimes.SelectedValue;

                        if (productLineModel.HasAlreadyOrdered(Session.Instance.CurrentUserId, o.Id))
                        {
                            throw new Exception("Du hast bereits eine Bestellung für diese Uhrzeit abgegeben.");
                        }
                    }
                    catch (NullReferenceException)
                    {
                        //ignore
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }));
            })).Start();
        }

        public ObservableCollection<Product> ProductList
        {
            get { return productList; }
        }

        public ObservableCollection<ProductLine> ProductTable
        {
            get { return productTable; }
        }

        public ObservableCollection<Order> OrderList
        {
            get { return orderList; }
        }
    }
}