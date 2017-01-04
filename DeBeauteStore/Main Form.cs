using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;

namespace DeBeauteStore
{
    public partial class MainForm :Form
    {
        DeBeauteStore db = new DeBeauteStore();
        Customer selectedCustomerData;
        Product selectedProductData;

        int customerID;
        int productID;
        decimal purchasePrice;
        decimal defaultSalePrice;

        DialogResult action;
        CultureInfo indonesiaCulture = new CultureInfo("id-ID");

        public MainForm()
        {
            InitializeComponent();
            indonesiaCulture.NumberFormat.NumberDecimalSeparator = ".";
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            PopulateCustomers();
            PopulateProducts();
            PopulateTransactions();

            // Create the ToolTip and associate with the Form container.
            ToolTip toolTip1 = new ToolTip();

            // Set up the delays for the ToolTip.
            toolTip1.InitialDelay = 1000;

            // Force the ToolTip text to be displayed whether or not the form is active.
            toolTip1.ShowAlways = true;

            // Set up the ToolTip text for the Button and Checkbox.
            toolTip1.SetToolTip(this.btnNewCustomer, "Add new customer.");
            toolTip1.SetToolTip(this.btnDeleteCustomer, "Remove selected customer from the list.");
            toolTip1.SetToolTip(this.btnSaveorUpdateCustomer, "Save above customer details.");
        }

        #region Populating data
        // Populating the customers for list box.
        private void PopulateCustomers()
        {
            var customersData = db.Customers.Select(rows => rows).OrderBy(rows => rows.Name);
            lbCustomers.DisplayMember = "Name";
            lbCustomers.ValueMember = "CustomerID";
            lbCustomers.DataSource = customersData.ToList();
        }

        // Populating the customers for list box.
        private void PopulateProducts()
        {
            var productsData = db.Products.Select(rows => rows).OrderBy(rows => rows.Name);
            lbProducts.DisplayMember = "Name";
            lbProducts.ValueMember = "ProductID";
            lbProducts.DataSource = productsData.ToList();
        }
        #endregion

        // Populating customer's transactions
        private void PopulateCustomerTransactions(int customerID)
        {
            dgvCustomerTransactions.DataSource = db.Sales.Where(customer => customer.CustomerID == customerID).Select(rows => new
            {
                SaleID = rows.SaleID,
                Date = rows.Date,
                PaymentMethod = rows.PaymentMethod,
                DeliveryType = rows.DeliveryType,
                Total = rows.Total
            });

            // Data grid transaction
            dgvCustomerTransactions.Columns["Total"].DefaultCellStyle.FormatProvider = indonesiaCulture;
            dgvCustomerTransactions.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Data grid details transaction
            dgvCustomerDetailsTransaction.Columns["SalePrice"].DefaultCellStyle.FormatProvider = indonesiaCulture;
            dgvCustomerDetailsTransaction.Columns["SubTotal"].DefaultCellStyle.FormatProvider = indonesiaCulture;
            dgvCustomerDetailsTransaction.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        // Populating all transactions
        private void PopulateTransactions()
        {
            dgvTransactions.DataSource = db.Sales.Select(rows => new
            {
                Customer = rows.Customer.Name,
                Date = rows.Date,
                PaymentMethod = rows.PaymentMethod,
                DeliveryType = rows.DeliveryType,
                Total = rows.Total
            });



            // Data grid transaction
            dgvTransactions.Columns["TransactionTotal"].DefaultCellStyle.FormatProvider = indonesiaCulture;
            dgvTransactions.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        #region Customer and Product
        // Auto populate data from selected customer.
        private void listbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            #region Customer list change
            // Method if the event is on list box customers.
            if (((ListBox)sender).Name.ToString() == lbCustomers.Name)
            {
                // Query into database to select the data from selected customer and shows the data to the textboxes.
                customerID = (int)lbCustomers.SelectedValue;

                selectedCustomerData = (db.Customers.Where(rows => rows.CustomerID == customerID).Count() > 0) ? db.Customers.Where(rows => rows.CustomerID == customerID).FirstOrDefault() : null;

                if (selectedCustomerData != null)
                {
                    tbCustomerName.Text = selectedCustomerData.Name;
                    tbCustomerPhoneNumber.Text = selectedCustomerData.PhoneNumber;
                    tbCustomerAddress.Text = selectedCustomerData.Address;
                    tbCustomerCity.Text = selectedCustomerData.City;
                }
                else
                {
                    MessageBox.Show("Cannot find customer details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                PopulateCustomerTransactions(customerID);

                // Change the button name to reflect the event - Update product or Update customer
                gbCustomerDetails.Text = "Customer Details";
                btnSaveorUpdateCustomer.Text = "Update";
                btnSaveorUpdateCustomer.Name = "btnUpdateCustomer";
                btnDeleteCustomer.Show();
            }
            #endregion
            #region Product list change
            // Method if the event is on list box products.
            else if (((ListBox)sender).Name.ToString() == lbProducts.Name)
            {
                // Query into database to select the data from selected product and shows the data to the textboxes.
                productID = (int)lbProducts.SelectedValue;

                selectedProductData = (db.Products.Where(rows => rows.ProductID == productID).Count() > 0) ? db.Products.Where(rows => rows.ProductID == productID).FirstOrDefault() : null;

                if (selectedCustomerData != null)
                {
                    tbProductName.Text = selectedProductData.Name;
                    nudProductQuantity.Value = (int)selectedProductData.Quantity;
                    tbProductPurchasePrice.Text = string.Format(indonesiaCulture, "{0:C}", selectedProductData.PurchasePrice);
                    tbProductSalePrice.Text = string.Format(indonesiaCulture, "{0:C} ", selectedProductData.DefaultSalesPrice);
                    tbProductProfit.Text = string.Format(indonesiaCulture, "{0:C} ", selectedProductData.DefaultSalesPrice - selectedProductData.PurchasePrice);
                }
                else
                {
                    MessageBox.Show("Cannot find product details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                // Change the button name to reflect the event - Update product or Update customer
                gbProductDetails.Text = "Product Details";
                btnSaveorUpdateProduct.Text = "Update";
                btnSaveorUpdateProduct.Name = "btnUpdateProduct";
                btnDeleteProduct.Show();
            }
            #endregion
        }

        // New method for Customer and Product.
        private void btnNewCustomernandProduct_Click(object sender, EventArgs e)
        {
            #region New Customer
            if (((Button)sender).Name == btnNewCustomer.Name)
            {
                // Preparing the textboxes for entering new details.
                tbCustomerName.Text = String.Empty;
                tbCustomerPhoneNumber.Text = String.Empty;
                tbCustomerAddress.Text = String.Empty;
                tbCustomerCity.Text = String.Empty;

                tbCustomerName.ReadOnly = false;
                tbCustomerPhoneNumber.ReadOnly = false;
                tbCustomerAddress.ReadOnly = false;
                tbCustomerCity.ReadOnly = false;

                gbCustomerDetails.Text = "Customer Details - New Customer";

                btnSaveorUpdateCustomer.Text = "Save";
                btnSaveorUpdateCustomer.Name = "btnSaveCustomer";

                btnDeleteCustomer.Hide();
            }
            #endregion
            #region New Product
            else if (((Button)sender).Name == btnNewProduct.Name)
            {
                // Preparing the textboxes for entering new details.
                tbProductName.Text = String.Empty;
                nudProductQuantity.Value = 1;
                tbProductPurchasePrice.Text = tbProductPurchasePrice.Text = string.Format(indonesiaCulture, "{0:C}", 0);
                tbProductSalePrice.Text = tbProductSalePrice.Text = string.Format(indonesiaCulture, "{0:C}", 0);
                tbProductProfit.Text = tbProductProfit.Text = string.Format(indonesiaCulture, "{0:C}", 0);

                tbProductName.ReadOnly = false;
                nudProductQuantity.ReadOnly = false;
                tbProductPurchasePrice.ReadOnly = false;
                tbProductSalePrice.ReadOnly = false;

                gbProductDetails.Text = "Product Details - New Product";

                btnSaveorUpdateProduct.Text = "Save";
                btnSaveorUpdateProduct.Name = "btnSaveProduct";

                btnDeleteProduct.Hide();
            }
            #endregion
        }

        // Cannot delete customer who has made transactions.
        private void btnDeleteCustomerandProduct_Click(object sender, EventArgs e)
        {
            #region Delete Customer
            if (((Button)sender).Name == btnDeleteCustomer.Name)
            {
                action = MessageBox.Show("Are you sure you want to delete this customer \"" + selectedCustomerData.Name + "\"?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (action == DialogResult.Yes)
                {
                    try
                    {
                        db.Customers.DeleteOnSubmit(selectedCustomerData);
                        db.SubmitChanges();
                        MessageBox.Show("Customer \"" + selectedCustomerData.Name + "\" has been successfully deleted.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch
                    {
                        MessageBox.Show("Failed to delete this customer. Customer \"" + selectedCustomerData.Name + "\" already made transactions.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                PopulateCustomers();
            }
            #endregion
            #region Delete Product
            else if (((Button)sender).Name == btnDeleteProduct.Name)
            {
                action = MessageBox.Show("Are you sure you want to delete this product \"" + selectedProductData.Name + "\"?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (action == DialogResult.Yes)
                {
                    try
                    {
                        db.Products.DeleteOnSubmit(selectedProductData);
                        db.SubmitChanges();
                        MessageBox.Show("Product \"" + selectedProductData.Name + "\" has been successfully deleted.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch
                    {
                        MessageBox.Show("Failed to delete this product. Product \"" + selectedProductData.Name + "\" already has transactions.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                PopulateProducts();
            }
            #endregion
        }

        private void btnSaveCustomerandProduct_Click(object sender, EventArgs e)
        {
            #region Save Customer
            if (((Button)sender).Name == btnSaveorUpdateCustomer.Name)
            {
                // Save button method when adding new customer.
                if (gbCustomerDetails.Text == "Customer Details - New Customer")
                {
                    // Preparing the data before submitting to database. At least new customer must have a name.
                    if (tbCustomerName.Text == String.Empty)
                    {
                        MessageBox.Show("Please enter Customer name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        Customer newCustomer = new Customer();
                        newCustomer.Name = tbCustomerName.Text;
                        newCustomer.PhoneNumber = tbCustomerPhoneNumber.Text;
                        newCustomer.Address = tbCustomerAddress.Text;
                        newCustomer.City = tbCustomerCity.Text;

                        action = MessageBox.Show("Save new customer?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (action == DialogResult.Yes)
                        {
                            try
                            {
                                db.Customers.InsertOnSubmit(newCustomer);
                                db.SubmitChanges();
                                MessageBox.Show("New customer \"" + tbCustomerName.Text + "\" has been successfully created.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            catch
                            {
                                MessageBox.Show("Failed to save data.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        PopulateCustomers();
                    }
                }
                // Save button method when updating customer.
                else
                {
                    string customerName = selectedCustomerData.Name;
                    // Preparing the data before submitting to database. At least new customer must have a name.
                    if (tbCustomerName.Text == String.Empty)
                    {
                        MessageBox.Show("Please enter Customer name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        selectedCustomerData.Name = tbCustomerName.Text;
                        selectedCustomerData.PhoneNumber = tbCustomerPhoneNumber.Text;
                        selectedCustomerData.Address = tbCustomerAddress.Text;
                        selectedCustomerData.City = tbCustomerCity.Text;

                        action = MessageBox.Show("Update customer details?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (action == DialogResult.Yes)
                        {
                            try
                            {
                                db.SubmitChanges();
                                MessageBox.Show("Customer \"" + customerName + "\" has been successfully updated.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                PopulateCustomers();
                            }
                            catch
                            {
                                MessageBox.Show("Failed to save data.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            #endregion
            #region Save Product
            else if (((Button)sender).Name == btnSaveorUpdateProduct.Name)
            {
                // Save button method when adding new product.
                if (gbProductDetails.Text == "Product Details - New Product")
                {
                    // Preparing the data before submitting to database. At least new product must have a name. Default Quantity is 1.
                    if (tbProductName.Text == String.Empty)
                    {
                        MessageBox.Show("Please enter Product name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        purchasePrice = Decimal.Parse(tbProductPurchasePrice.Text, NumberStyles.Currency, indonesiaCulture);
                        defaultSalePrice = Decimal.Parse(tbProductSalePrice.Text, NumberStyles.Currency, indonesiaCulture);

                        Product newProduct = new Product();
                        newProduct.Name = tbProductName.Text;
                        newProduct.Quantity = (int)nudProductQuantity.Value;
                        newProduct.PurchasePrice = purchasePrice;
                        newProduct.DefaultSalesPrice = defaultSalePrice;

                        action = MessageBox.Show("Save new product?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (action == DialogResult.Yes)
                        {
                            try
                            {
                                db.Products.InsertOnSubmit(newProduct);
                                db.SubmitChanges();
                                MessageBox.Show("New product \"" + tbProductName.Text + "\" has been successfully created.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                PopulateProducts();
                            }
                            catch
                            {
                                MessageBox.Show("Failed to save data.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                // Save button method when updating product.
                else
                {
                    string productName = selectedProductData.Name;
                    // Preparing the data before submitting to database. At least new customer must have a name.
                    if (tbProductName.Text == String.Empty)
                    {
                        MessageBox.Show("Please enter Product name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        purchasePrice = Decimal.Parse(tbProductPurchasePrice.Text, NumberStyles.Currency, indonesiaCulture);
                        defaultSalePrice = Decimal.Parse(tbProductSalePrice.Text, NumberStyles.Currency, indonesiaCulture);

                        selectedProductData.Name = tbProductName.Text;
                        selectedProductData.Quantity = (int)nudProductQuantity.Value;
                        selectedProductData.PurchasePrice = purchasePrice;
                        selectedProductData.DefaultSalesPrice = defaultSalePrice;

                        action = MessageBox.Show("Update product details?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (action == DialogResult.Yes)
                        {
                            try
                            {
                                db.SubmitChanges();
                                MessageBox.Show("Product \"" + productName + "\" has been successfully updated.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                PopulateProducts();
                            }
                            catch
                            {
                                MessageBox.Show("Failed to save data.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            #endregion
        }

        // Search the inputed name from databases and show its to list box.
        private void tbSearchCustomerandProduct_TextChanged(object sender, EventArgs e)
        {
            #region Search Customer
            if (((TextBox)sender).Name.ToString() == tbSearchCustomers.Name)
            {
                var searchResults = db.Customers.Where(rows => rows.Name.ToLower().Contains(tbSearchCustomers.Text.ToLower())).Select(rows => rows).ToList();

                lbCustomers.DataSource = searchResults.ToList();
            }
            #endregion
            #region Search Product
            else if (((TextBox)sender).Name.ToString() == tbSearchProducts.Name)
            {
                var searchResults = db.Products.Where(rows => rows.Name.ToLower().Contains(tbSearchProducts.Text.ToLower())).Select(rows => rows).ToList();

                lbProducts.DataSource = searchResults.ToList();
            }
            #endregion
        }

        private void textBoxProduct_Enter(object sender, EventArgs e)
        {
            if (tbProductPurchasePrice.Text == string.Format(indonesiaCulture, "{0:C}", 0) && ((TextBox)sender).Name == tbProductPurchasePrice.Name)
            {
                tbProductPurchasePrice.Text = String.Empty;
            }
            else if (tbProductSalePrice.Text == string.Format(indonesiaCulture, "{0:C}", 0) && ((TextBox)sender).Name == tbProductSalePrice.Name)
            {
                tbProductSalePrice.Text = String.Empty;
            }
        }

        private void textBoxProduct_Leave(object sender, EventArgs e)
        {
            decimal value;

            #region Purchase Price event
            if (((TextBox)sender).Name == tbProductPurchasePrice.Name)
            {
                if (decimal.TryParse(tbProductPurchasePrice.Text, NumberStyles.Currency, indonesiaCulture, out value))
                {
                    tbProductPurchasePrice.Text = String.Format(indonesiaCulture, "{0:C}", value);
                }
                else
                {
                    tbProductPurchasePrice.Text = string.Format(indonesiaCulture, "{0:C}", 0);
                }
            }
            #endregion
            #region Sale Price event
            else if (((TextBox)sender).Name == tbProductSalePrice.Name)
            {
                if (decimal.TryParse(tbProductSalePrice.Text, NumberStyles.Currency, indonesiaCulture, out value))
                {
                    tbProductSalePrice.Text = String.Format(indonesiaCulture, "{0:C}", value);
                }
                else
                {
                    tbProductSalePrice.Text = string.Format(indonesiaCulture, "{0:C}", 0);
                }
            }
            #endregion

            purchasePrice = decimal.Parse(tbProductPurchasePrice.Text, NumberStyles.Currency, indonesiaCulture);
            defaultSalePrice = decimal.Parse(tbProductSalePrice.Text, NumberStyles.Currency, indonesiaCulture);
            tbProductProfit.Text = String.Format(indonesiaCulture, "{0:C}", defaultSalePrice - purchasePrice);
        }

        private void textBoxNumberOnly_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && e.KeyChar != (char)8; //8 is for backspace
        }

        private void textBoxPhoneNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            //8 is for backspace and 43 is for "+" symbol.
            e.Handled = !char.IsDigit(e.KeyChar) && e.KeyChar != (char)8 && e.KeyChar != (char)43;
        }

        private void dgvCustomerTransactions_Paint(object sender, PaintEventArgs e)
        {
            DataGridView sndr = (DataGridView)sender;
            if (sndr.Rows.Count == 0) // <-- if there are no rows in the DataGridView when it paints, then it will create your message
            {
                using (Graphics grfx = e.Graphics)
                {
                    // write text on top of the white rectangle just created
                    grfx.DrawString("*Does not have transactions*", new Font("Bookman Old Style", 16), Brushes.Red, new PointF(sndr.Location.X, sndr.ColumnHeadersHeight + 1));
                }
            }
        }

        private void dgvCustomerTransactions_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCustomerTransactions.SelectedRows.Count > 0)
            {
                int saleID = (int)dgvCustomerTransactions.SelectedRows[0].Cells[0].Value;
                if (db.SaleDetails.Where(rows => rows.SaleID == saleID).Count() > 0)
                {
                    dgvCustomerDetailsTransaction.DataSource = db.SaleDetails.Where(rows => rows.SaleID == saleID).Select(rows => new
                    {
                        ProductName = rows.Product.Name,
                        Quantity = rows.Quantity,
                        SalePrice = rows.SalePrice,
                        SubTotal = rows.Quantity * rows.SalePrice
                    });
                }
            }
            else
            {
                dgvCustomerDetailsTransaction.Rows.Clear();
            }
        }
        #endregion
    }
}
