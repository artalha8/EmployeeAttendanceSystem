using System;
using System.Data.SqlClient;

namespace EmployeeAttendanceSystem
{
    public class DatabaseHelper
    {
        
        private string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=EmployeeAttendanceDB;Integrated Security=True;";

        public SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}