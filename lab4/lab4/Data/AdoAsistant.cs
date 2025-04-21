using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace lab4.Data
{
    public class AdoAssistant
    {
        private readonly string connectionString = System.Configuration.ConfigurationManager
            .ConnectionStrings["connectionStringName"].ConnectionString;

        private DataTable dtProducts;

        public void ClearCache()
        {
            this.dtProducts = null;
        }

        // Метод для завантаження даних із таблиці "Товари"
        public DataTable LoadProducts()
        {
            // Завантажимо таблицю лише один раз, якщо вона вже існує і не пуста.
            if (dtProducts != null && dtProducts.Rows.Count > 0)
                return dtProducts;

            // Якщо таблиця ще не завантажена або пуста, завантажуємо знову.
            dtProducts = new DataTable();

            // Створюємо об'єкт підключення
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // SQL-запит для вибору даних із таблиці "Товари"
                // Using aliases for easier C# binding
                string query = "SELECT [Артикул] AS Article, [Назва] AS Name, [Одиниця виміру] AS Unit, [Кількість] AS Quantity, [Ціна] AS Price FROM [dbo].[Товари]";
                SqlCommand command = new SqlCommand(query, connection); // Створюємо об'єкт команди
                SqlDataAdapter adapter = new SqlDataAdapter(command); // Створюємо об'єкт читання

                try
                {
                    // Метод Fill відкриває з'єднання, якщо воно закрите, виконує запит і закриває з'єднання.
                    adapter.Fill(dtProducts);
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("SQL Error loading products: " + ex.Message); // Показуємо детальну SQL-помилку
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка підключення до БД при завантаженні товарів: " + ex.Message); // Інші помилки
                }
            }
            return dtProducts;
        }

        // Метод для додавання нового товару
        public bool AddProduct(int article, string name, string unit, int quantity, decimal price)
        {
            string query = "INSERT INTO [dbo].[Товари] ([Артикул], [Назва], [Одиниця виміру], [Кількість], [Ціна]) VALUES (@Article, @Name, @Unit, @Quantity, @Price)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);

                // Додаємо параметри
                command.Parameters.AddWithValue("@Article", article);
                command.Parameters.AddWithValue("@Name", string.IsNullOrEmpty(name) ? (object)DBNull.Value : name); // Handle potential nulls
                command.Parameters.AddWithValue("@Unit", string.IsNullOrEmpty(unit) ? (object)DBNull.Value : unit); // Handle potential nulls
                command.Parameters.AddWithValue("@Quantity", quantity); // Assuming quantity can be nullable in DB based on image, handle appropriately if needed
                command.Parameters.AddWithValue("@Price", price);       // Assuming price can be nullable in DB based on image, handle appropriately if needed


                try
                {
                    connection.Open();
                    int result = command.ExecuteNonQuery(); // ExecuteNonQuery повертає кількість змінених рядків
                    return result > 0; // Повертаємо true, якщо хоча б один рядок був доданий
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("SQL Error adding product: " + ex.Message);
                    return false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка підключення до БД при додаванні товару: " + ex.Message);
                    return false;
                }
            }
        }

        // Метод для видалення товару за його Артикулом
        public bool DeleteProduct(int article) // Modified parameter and SQL
        {
            string query = "DELETE FROM [dbo].[Товари] WHERE [Артикул] = @Article";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Article", article);

                try
                {
                    connection.Open();
                    int result = command.ExecuteNonQuery();
                    if (result > 0)
                    {
                        // MessageBox.Show("Товар успішно видалено!"); // Optional: confirmation message
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Товар з таким артикулом не знайдено.");
                        return false;
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("SQL Error deleting product: " + ex.Message);
                    return false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка при видаленні товару: " + ex.Message);
                    return false;
                }
            }
        }

        // Метод для оновлення даних товару
        public bool UpdateProduct(int article, string name, string unit, int quantity, decimal price) // Modified parameters and SQL
        {
            string query = @"UPDATE [dbo].[Товари] SET
                                [Назва] = @Name,
                                [Одиниця виміру] = @Unit,
                                [Кількість] = @Quantity,
                                [Ціна] = @Price
                             WHERE [Артикул] = @Article";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@Article", article);
                command.Parameters.AddWithValue("@Name", string.IsNullOrEmpty(name) ? (object)DBNull.Value : name);
                command.Parameters.AddWithValue("@Unit", string.IsNullOrEmpty(unit) ? (object)DBNull.Value : unit);
                command.Parameters.AddWithValue("@Quantity", quantity); // Handle null if necessary
                command.Parameters.AddWithValue("@Price", price);       // Handle null if necessary


                try
                {
                    connection.Open();
                    int result = command.ExecuteNonQuery();
                    if (result > 0)
                    {
                        //MessageBox.Show("Дані товару успішно оновлено!");
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("Товар з таким артикулом не знайдено для оновлення.");
                        return false;
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("SQL Error updating product: " + ex.Message);
                    return false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка при оновленні товару: " + ex.Message);
                    return false;
                }
            }
        }
    }
}