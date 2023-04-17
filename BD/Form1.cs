using Npgsql;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ComboBox = System.Windows.Forms.ComboBox;

namespace BD
{
    public partial class Form1 : Form
    {
        private string connectionString = String.Format("Server=localhost;Port=5432;" +
                    "User Id=postgres;Password=Uhmmannie;Database=Northwind");
        private NpgsqlConnection connection;
        NpgsqlCommand cmd = new NpgsqlCommand();

        string select = "select * from products order by product_id";
        public Form1()
        {
            InitializeComponent();

            connection = new NpgsqlConnection(connectionString);
            connection.Open();

            comboBox1.Items.Add("Приостановлен");
            comboBox1.Items.Add("Продаваемый");

            CreateComboBox("select category_id from products order by category_id", comboBox2);

            comboBox3.Items.Add("Приостановлен");
            comboBox3.Items.Add("Продаваемый");

            CreateComboBox("select supplier_id from products order by supplier_id", comboBox4);

            CreateComboBox("select category_id from products order by category_id", comboBox5);

        }
        private void CreateComboBox(string query, ComboBox combobox)
        {
            List<string> list = new List<string>();

            cmd = new NpgsqlCommand(query, connection);
            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string item = reader[0].ToString();
                list.Add(item);
                foreach (string _item in list)
                {
                    if (!combobox.Items.Contains(_item))
                    {
                        combobox.Items.Add(_item);
                    }
                }
            }
            reader.Close();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            Select(select);
        }
        private void Form1_Close(object sender, EventArgs e)
        {
            connection.Close();
        }
        private void Select(string query)
        {
            cmd = new NpgsqlCommand(query, connection);
            NpgsqlDataReader reader = cmd.ExecuteReader();

            dataGridView1.Rows.Clear();

            while (reader.Read())
            {
                var product_id = reader[0];
                var product_name = reader[1];
                var supplier_id = reader[2];
                var category_id = reader[3];
                var quantity_per_unit = reader[4];
                var unit_price = reader[5];
                var units_in_stock = reader[6];
                var units_in_order = reader[7];
                var reorder_lever = reader[8];
                var discontinued = reader[9];

                dataGridView1.Rows.Add(product_id, product_name, supplier_id, category_id, quantity_per_unit, unit_price, units_in_stock, units_in_order,
                    reorder_lever, discontinued);
            }

            reader.Close();

        }
        private void delete_Button(object sender, EventArgs e)
        {
            if (Int64.TryParse(textBox10.Text, out Int64 parsedNumber))
            {
                string query = $"delete from products where product_id={parsedNumber}";
                cmd = new NpgsqlCommand(query, connection);
                cmd.ExecuteNonQuery();
                Select(select);
                textBox10.Text = null;
            }
            else
            {
                MessageBox.Show("Удаление: введите целое число");
            }
        }
        private void update_Button(object sender, EventArgs e)
        {
            Select(select);
        }
        private void search_Button(object sender, EventArgs e)
        {
            bool flag = false;
            string query = "select * from products where (";
            if (comboBox2.SelectedItem != null)
            {
                query += $"(category_id='{comboBox2.Text}')";
            }
            if (textBox12.Text != "")
            {
                List<string> list = new List<string>();

                string sql = $"select product_name from products";

                cmd = new NpgsqlCommand(sql, connection);
                NpgsqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string item = reader[0].ToString();
                    list.Add(item);
                }
                reader.Close();

                foreach (string _item in list)
                {
                    if (textBox12.Text == _item)
                    {
                        if (query.EndsWith(")")) query += " and";
                        query += $"(product_name='{textBox12.Text}')";
                        flag = true;
                        break;
                    }
                }
                if (flag == false) MessageBox.Show("Такого товара нет в базе данных");
            }
            if (comboBox3.SelectedItem != null)
            {
                int discontinued;
                if (comboBox3.Text == "Приостановлен") discontinued = 0;
                else discontinued = 1;
                if (query.EndsWith(")")) query += " and";
                query += $"(discontinued='{discontinued}')";
            }
            query += ");";
            if (comboBox2.Text == "" && textBox12.Text == "" && comboBox3.Text == "")
            {
                MessageBox.Show("Выберите критерии поиска");
            }
            if ((flag == true && textBox12.Text != "") || comboBox2.Text != "" || comboBox3.Text != "")
            {
                cmd = new NpgsqlCommand(query, connection);
                cmd.ExecuteNonQuery();
                Select(query);
                comboBox2.Text = null;
                textBox12.Text = null;
                comboBox3.Text = null;
            }
            
        }
        private void add_Button(object sender, EventArgs e)
        {
            bool flag = true;
            if (textBox2.Text != "" && comboBox1.SelectedItem != null)
            {
                string query = "insert into products(product_id, ";
                string values = "values((select max(product_id)+1 from products)"; // product_id
                //
                //product_name
                //
                if (textBox2.Text != "")
                {
                    if (values.EndsWith(")")) values += ", ";
                    if (query.EndsWith(")")) query += ", ";
                    values += $"('{textBox2.Text}')";
                    query += "product_name, ";
                }
                //
                //supplier_id
                //
                if (comboBox4.SelectedItem != null)
                {
                    if (values.EndsWith(")")) values += ", ";
                    values += $"({comboBox4.Text})";
                    query += "supplier_id, ";
                }
                //
                // category_id
                //
                if (comboBox5.SelectedItem != null)
                {
                    if (values.EndsWith(")")) values += ", ";
                    values += $"({comboBox5.Text})";
                    query += "category_id, ";
                }
                //
                // quantity_per_unit
                //
                if (textBox5.Text != "")
                {
                    if (values.EndsWith(")")) values += ", ";
                    values += $"('{textBox5.Text}')";
                    query += "quantity_per_unit, ";
                }
                //
                // unit_price
                //
                if (textBox6.Text != "")
                {
                    if (double.TryParse(textBox6.Text.Replace('.', ','), out double unit))
                    {
                        string t = unit.ToString().Replace(',', '.');
                        if (values.EndsWith(")")) values += ", ";
                        values += $"({t})";
                        query += "unit_price, ";
                    }
                    else
                    {
                        flag = false;
                        MessageBox.Show("Цена за единицу: введите число");
                    }
                }
                //
                // units_in_stock
                //
                if (textBox7.Text != "")
                {
                    if (Int64.TryParse(textBox7.Text, out Int64 stock))
                    {
                        if (values.EndsWith(")")) values += ", ";
                        values += $"({stock})";
                        query += "units_in_stock, ";
                    }
                    else
                    {
                        flag = false;
                        MessageBox.Show("Количество на складе: введите целое число");
                    }
                }
                //
                // units_on_order
                //
                if (textBox1.Text != "")
                {
                    if (Int64.TryParse(textBox1.Text, out Int64 order))
                    {
                        if (values.EndsWith(")")) values += ", ";
                        values += $"({order})";
                        query += "units_on_order, ";
                    }
                    else
                    {
                        flag = false;
                        MessageBox.Show("Количество под заказ: введите число");
                    }
                }
                //
                // reorder_level
                //
                if (textBox8.Text != "")
                {
                    if (Int64.TryParse(textBox8.Text, out Int64 level))
                    {
                        if (values.EndsWith(")")) values += ", ";
                        values += $"({level})";
                        query += "reorder_level, ";
                    }
                    else
                    {
                        flag = false;
                        MessageBox.Show("Уровень повторного заказа: введите число");
                    }
                }
                //
                // discontinued
                //
                if (comboBox1.SelectedItem != null)
                {
                    int discontinued;
                    if (comboBox1.Text == "Приостановлен") discontinued = 0;
                    else discontinued = 1;
                    if (values.EndsWith(")")) values += ", ";
                    values += $"({discontinued})";
                    query += "discontinued";
                }
                query += ") ";
                query += values;
                query += ");";
                if (flag == true)
                {
                    cmd = new NpgsqlCommand(query, connection);
                    cmd.ExecuteNonQuery();
                    Select(select);

                    textBox2.Text = null;
                    comboBox4.Text = null;
                    comboBox5.Text = null;
                    textBox5.Text = null;
                    textBox6.Text = null;
                    textBox7.Text = null;
                    textBox1.Text = null;
                    textBox8.Text = null;
                    comboBox1.Text = null;
                }
            }
            else
            {
                MessageBox.Show("Введите обязательные данные: Название продукта* и Статус*");
            }
        }
    }
}
