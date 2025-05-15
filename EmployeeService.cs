using MySql.Data.MySqlClient;
using ConsoleTables;

namespace ConsoleAdoDotNet
{
    public class EmployeeService
    {
        private readonly string _connectionString;

        public EmployeeService(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task AddEmployee()
        {
            Console.WriteLine("\nAdd New Employee");

            Console.Write("First Name: ");
            string firstName = Console.ReadLine()!;

            Console.Write("Last Name: ");
            string lastName = Console.ReadLine()!;

            Console.Write("Email: ");
            string email = Console.ReadLine()!;

            Console.Write("Department: ");
            string department = Console.ReadLine()!;

            Console.Write("Hire Date (yyyy-mm-dd): ");

            if (!DateTime.TryParse(Console.ReadLine(), out DateTime hireDate))
            {
                Console.WriteLine("Invalid date format.");
                return;
            }

            Console.Write("Salary: ");

            if (!decimal.TryParse(Console.ReadLine(), out decimal salary))
            {
                Console.WriteLine("Invalid salary format.");
                return;
            }

            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                string query = "INSERT INTO employees (id,firstname, lastname, email, department, hiredate, salary) " +
                                "VALUES (@id,@firstname, @lastname, @email, @department, @hiredate, @salary)";


                MySqlCommand command = new MySqlCommand(query, connection);
                 command.Parameters.AddWithValue("@id", Guid.NewGuid());
                 command.Parameters.AddWithValue("@firstname", firstName);
                 command.Parameters.AddWithValue("@lastname", lastName);
                 command.Parameters.AddWithValue("@email", email);
                 command.Parameters.AddWithValue("@department", department);
                 command.Parameters.AddWithValue("@Hiredate", hireDate);
                 command.Parameters.AddWithValue("@salary", salary);

                try
                {
                    await connection.OpenAsync();
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                    {
                        Console.WriteLine("Employee added successfully!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        public async Task ViewAllEmployees()
        {
            Console.WriteLine("\nAll Employees");

            ConsoleTable table = new("Name", "Email", "Department", "Hire Date", "Salary");

            using (MySqlConnection connection = new(_connectionString))
            {
                string query = "SELECT * FROM Employees";
                MySqlCommand command = new(query, connection);

                try
                {
                    await connection.OpenAsync();
                    var reader =  command.ExecuteReader();

                    while (reader.Read())
                    {
                        table.AddRow($"{reader["FirstName"]} {reader["LastName"]}", reader["Email"], reader["Department"], ((DateTime)reader["HireDate"]).ToShortDateString(), reader["Salary"]);
                    }

                    table.Write(Format.Default);

                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        public async Task ViewEmployeeById()
        {
            Console.Write("\nEnter Employee ID: ");

            if (!Guid.TryParse(Console.ReadLine(), out Guid id))
            {
                Console.WriteLine("Invalid ID format.");
                return;
            }

            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Employees WHERE id = @id";
                MySqlCommand command =  new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);

                try
                {
                    await connection.OpenAsync();
                    var reader = await command.ExecuteReaderAsync();

                    if (reader.HasRows)
                    {
                        reader.Read();
                        Console.WriteLine("\nEmployee Details");
                        Console.WriteLine($"ID: {reader["id"]}");
                        Console.WriteLine($"Name: {reader["firstname"]} {reader["lastname"]}");
                        Console.WriteLine($"Email: {reader["email"]}");
                        Console.WriteLine($"Department: {reader["department"]}");
                        Console.WriteLine($"Hire Date: {((DateTime)reader["hiredate"]).ToShortDateString()}");
                        Console.WriteLine($"Salary: {reader["salary"]}");
                    }
                    else
                    {
                        Console.WriteLine("Employee not found.");
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        public async Task  UpdateEmployee()
        {
            Console.Write("\nEnter Employee ID to update: ");

            if (!Guid.TryParse(Console.ReadLine(), out Guid id))
            {
                Console.WriteLine("Invalid ID format.");
                return;
            }

            // First check if employee exists
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                string checkQuery = "SELECT COUNT(*) FROM Employees WHERE Id = @Id";
                MySqlCommand checkCommand = new MySqlCommand(checkQuery, connection);
                checkCommand.Parameters.AddWithValue("@Id", id);

                try
                {
                    await connection.OpenAsync();
                    int employeeCount = (int)checkCommand.ExecuteScalar();
                    if (employeeCount == 0)
                    {
                        Console.WriteLine("Employee not found.");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return;
                }
            }

            Console.WriteLine("\nEnter new details (leave blank to keep current value):");

            Console.Write("First Name: ");
            string firstName = Console.ReadLine()!;

            Console.Write("Last Name: ");
            string lastName = Console.ReadLine()!;

            Console.Write("Email: ");
            string email = Console.ReadLine()!;

            Console.Write("Department: ");
            string department = Console.ReadLine()!;

            Console.Write("Hire Date (yyyy-mm-dd): ");
            string hireDateStr = Console.ReadLine()!;
            DateTime? hireDate = null;

            if (!string.IsNullOrEmpty(hireDateStr) && DateTime.TryParse(hireDateStr, out DateTime parsedDate))
            {
                hireDate = parsedDate;
            }

            Console.Write("Salary: ");
            string salaryStr = Console.ReadLine()!;
            decimal? salary = null;
            if (!string.IsNullOrEmpty(salaryStr) && decimal.TryParse(salaryStr, out decimal parsedSalary))
            {
                salary = parsedSalary;
            }

            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                string query = "UPDATE Employees SET " +
                               "firstname = ISNULL(@firstname, firstname), " +
                               "lastname = ISNULL(@lastname , lastname ), " +
                               "email = ISNULL(@email, email), " +
                               "department = ISNULL(@department, department), " +
                               "hiredate = ISNULL(@hiredate, hiredate), " +
                               "salary = ISNULL(@salary, salary) " +
                               "WHERE id = @id";

                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@firstname", string.IsNullOrEmpty(firstName) ? (object)DBNull.Value : firstName);
                command.Parameters.AddWithValue("@lastname", string.IsNullOrEmpty(lastName) ? (object)DBNull.Value : lastName);
                command.Parameters.AddWithValue("@email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email);
                command.Parameters.AddWithValue("@department", string.IsNullOrEmpty(department) ? (object)DBNull.Value : department);
                command.Parameters.AddWithValue("@hiredate", hireDate.HasValue ? (object)hireDate.Value : DBNull.Value);
                command.Parameters.AddWithValue("@salary", salary.HasValue ? (object)salary.Value : DBNull.Value);

                try
                {
                    await connection.OpenAsync();
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        Console.WriteLine("Employee updated successfully!");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        public async Task DeleteEmployee()
        {
            Console.Write("\nEnter Employee ID to delete: ");

            if (!Guid.TryParse(Console.ReadLine(), out Guid id))
            {
                Console.WriteLine("Invalid ID format.");
                return;
            }

            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                string query = "DELETE FROM employees WHERE id = @id";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);

                try
                {
                    await connection.OpenAsync();
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        Console.WriteLine("Employee deleted successfully!");
                    }
                    else
                    {
                        Console.WriteLine("Employee not found.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}
