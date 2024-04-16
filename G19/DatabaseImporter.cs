using Microsoft.Data.SqlClient;

namespace G19_ProductImport
{
    public class DatabaseImporter
    {
        private readonly string _connectionString;

        public DatabaseImporter(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public void ImportData(IEnumerable<Category> categories/*, IEnumerable<Product> products*/)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                foreach (var category in categories)
                {
                    foreach (var product in category.Products)
                    {

                        string NewQuery = "insert into Categories (Name, IsActive) " +
                                      "values (@Name, @IsActive); SELECT SCOPE_IDENTITY()";
                        string QueryChecker = "select CategoryID from Categories " +
                                              "where Name = @Name";
                        string insertProduct = "insert into Products (CategoryID, Code, Name, Price, IsActive, CreateDate) " +
                                               "values (@CategoryID, @Code, @Name, @Price, @IsActive, @CreateDate)";

                        using (SqlConnection conn = new SqlConnection(_connectionString))
                        {
                            conn.Open();
                            SqlCommand selectCommand = new SqlCommand(QueryChecker, conn);
                            selectCommand.Parameters.AddWithValue("@Name", category.Name);
                            int categoryID = (int?)selectCommand.ExecuteScalar() ?? 0;

                            while (categoryID == 0)
                            {
                                SqlCommand categoryCommand = new SqlCommand(NewQuery, conn);
                                categoryCommand.Parameters.AddWithValue("@Name", category.Name);
                                categoryCommand.Parameters.AddWithValue("@IsActive", category.IsActive);

                                categoryID = Convert.ToInt32(categoryCommand.ExecuteScalar());
                            }

                            SqlCommand productCommand = new SqlCommand(insertProduct, conn);
                            productCommand.Parameters.AddWithValue("@CategoryID", categoryID);
                            productCommand.Parameters.AddWithValue("@Code", product.Code);
                            productCommand.Parameters.AddWithValue("@Name", product.Name);
                            productCommand.Parameters.AddWithValue("@Price", product.Price);
                            productCommand.Parameters.AddWithValue("@IsActive", product.IsActive);
                            productCommand.Parameters.AddWithValue("@CreateDate", DateTime.Now);
                            productCommand.Parameters.AddWithValue("@UpdateDate", DateTime.Today);

                            productCommand.ExecuteNonQuery();

                            Console.WriteLine("Data inserted successfully.");
                        }
                    }
                }
            }
        }
    }
}