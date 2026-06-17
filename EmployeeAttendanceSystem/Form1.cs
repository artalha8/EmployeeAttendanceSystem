using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace EmployeeAttendanceSystem
{
    public partial class Form1 : Form
    {

        DatabaseHelper dbHelper = new DatabaseHelper();

        int selectedEmployeeId = 0;

        public Form1()
        {
            InitializeComponent();
            LoadEmployees();
            LoadChartData(); // Added this!
        }


        private void LoadEmployees()
        {
            try
            {
                using (SqlConnection conn = dbHelper.GetConnection())
                {
                    
                    string query = "SELECT * FROM Employees";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dataTable = new DataTable();

                    
                    adapter.Fill(dataTable);

                    
                    dgvEmployees.DataSource = dataTable;
                    // Bonus Feature: Update status bar with record count
                    lblStatusCount.Text = "Total Employees: " + dataTable.Rows.Count.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAddEmployee_Click(object sender, EventArgs e)
        {
            // 1. SQA Validation: Prevent empty submissions to avoid database crashes
            if (string.IsNullOrWhiteSpace(txtFullName.Text) ||
                string.IsNullOrWhiteSpace(txtDepartment.Text) ||
                string.IsNullOrWhiteSpace(txtContact.Text))
            {
                MessageBox.Show("Please fill in all fields before adding an employee.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Stop the code right here so it doesn't try to save
            }

            try
            {
                using (SqlConnection conn = dbHelper.GetConnection())
                {
                    // 2. The SQL Insert command. 
                    // We use parameters (@Name, @Dept) instead of direct text to prevent SQL Injection hacking!
                    string query = "INSERT INTO Employees (FullName, Department, ContactNumber) VALUES (@Name, @Dept, @Contact)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // 3. Attach the text from your textboxes to the SQL command
                        cmd.Parameters.AddWithValue("@Name", txtFullName.Text);
                        cmd.Parameters.AddWithValue("@Dept", txtDepartment.Text);
                        cmd.Parameters.AddWithValue("@Contact", txtContact.Text);

                        // 4. Open connection and execute
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                // 5. User Experience: Let them know it worked!
                MessageBox.Show("Employee added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 6. Clear the text boxes for the next entry
                txtFullName.Clear();
                txtDepartment.Clear();
                txtContact.Clear();

                // 7. Refresh the grid so the new employee appears instantly
                LoadEmployees();
            }
            catch (Exception ex)
            {
                // Catch any unexpected database errors gracefully
                MessageBox.Show("Error adding employee: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvEmployees_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Ensure the user clicked an actual row and not the header
            if (e.RowIndex >= 0)
            {
                // Get the specific row that was clicked
                DataGridViewRow row = dgvEmployees.Rows[e.RowIndex];

                // Store the ID so our Edit/Delete buttons know who to target
                selectedEmployeeId = Convert.ToInt32(row.Cells["EmployeeID"].Value);

                // Fill the text boxes with the data from that row
                txtFullName.Text = row.Cells["FullName"].Value.ToString();
                txtDepartment.Text = row.Cells["Department"].Value.ToString();
                txtContact.Text = row.Cells["ContactNumber"].Value.ToString();

                LoadIndividualAttendance(selectedEmployeeId);
            }
        }

        private void btnEditEmployee_Click(object sender, EventArgs e)
        {
            // SQA Validation: Ensure an employee is actually selected
            if (selectedEmployeeId == 0)
            {
                MessageBox.Show("Please select an employee from the list to edit.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = dbHelper.GetConnection())
                {
                    // SQL command to update specific fields for the selected ID
                    string query = "UPDATE Employees SET FullName=@Name, Department=@Dept, ContactNumber=@Contact WHERE EmployeeID=@Id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", txtFullName.Text);
                        cmd.Parameters.AddWithValue("@Dept", txtDepartment.Text);
                        cmd.Parameters.AddWithValue("@Contact", txtContact.Text);
                        cmd.Parameters.AddWithValue("@Id", selectedEmployeeId); // The ID we saved earlier

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Employee updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Reset our selection and clear boxes
                selectedEmployeeId = 0;
                txtFullName.Clear();
                txtDepartment.Clear();
                txtContact.Clear();

                // Refresh the grid to show the updated data
                LoadEmployees();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating employee: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDeleteEmployee_Click(object sender, EventArgs e)
        {
            // SQA Validation: Ensure an employee is actually selected
            if (selectedEmployeeId == 0)
            {
                MessageBox.Show("Please select an employee from the list to delete.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // User Experience: Ask for confirmation before permanently deleting data
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete this employee?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (dialogResult == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = dbHelper.GetConnection())
                    {
                        string query = "DELETE FROM Employees WHERE EmployeeID=@Id";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", selectedEmployeeId);

                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Employee deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    selectedEmployeeId = 0;
                    txtFullName.Clear();
                    txtDepartment.Clear();
                    txtContact.Clear();

                    LoadEmployees();
                }
                catch (SqlException ex)
                {
                    // Advanced Database Safety: If the employee has attendance records, SQL Server will block the deletion.
                    if (ex.Number == 547)
                    {
                        MessageBox.Show("Cannot delete this employee because they have attendance records attached. Delete their attendance first.", "Constraint Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    }
                    else
                    {
                        MessageBox.Show("Error deleting employee: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnMarkAttendance_Click(object sender, EventArgs e)
        {
            // SQA Validation 1: Ensure an employee is selected from the grid first
            if (selectedEmployeeId == 0)
            {
                MessageBox.Show("Please select an employee from the list to mark attendance.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // SQA Validation 2: Ensure a status is selected
            if (string.IsNullOrWhiteSpace(cmbStatus.Text))
            {
                MessageBox.Show("Please select an attendance status (Present, Absent, Leave).", "Status Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = dbHelper.GetConnection())
                {
                    conn.Open();

                    // SQA Validation 3: Prevent duplicate attendance entries for the same day
                    string checkQuery = "SELECT COUNT(*) FROM Attendance WHERE EmployeeID = @Id AND AttendanceDate = @Date";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Id", selectedEmployeeId);
                        checkCmd.Parameters.AddWithValue("@Date", dtpAttendanceDate.Value.Date);

                        int existingRecords = (int)checkCmd.ExecuteScalar();
                        if (existingRecords > 0)
                        {
                            MessageBox.Show("Attendance for this employee has already been marked for this date.", "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return; // Stop the code
                        }
                    }

                    // If no duplicates exist, insert the new attendance record
                    string insertQuery = "INSERT INTO Attendance (EmployeeID, AttendanceDate, Status) VALUES (@Id, @Date, @Status)";
                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@Id", selectedEmployeeId);
                        insertCmd.Parameters.AddWithValue("@Date", dtpAttendanceDate.Value.Date);
                        insertCmd.Parameters.AddWithValue("@Status", cmbStatus.Text);

                        insertCmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Attendance marked successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                cmbStatus.SelectedIndex = -1; // Clear the dropdown
                LoadChartData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error marking attendance: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // This method pulls summary statistics for the Charting Module (10 Marks)
        private void LoadChartData()
        {
            try
            {
                using (SqlConnection conn = dbHelper.GetConnection())
                {
                    // SQL command to group and count the attendance records
                    string query = "SELECT Status, COUNT(*) as TotalCount FROM Attendance GROUP BY Status";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        // Clear any old, default data out of the chart first
                        chartAttendance.Series[0].Points.Clear();
                        chartAttendance.Series[0].Name = "Attendance Breakdown";

                        // Read the grouped data from the database
                        while (reader.Read())
                        {
                            string statusName = reader["Status"].ToString();
                            int statusCount = Convert.ToInt32(reader["TotalCount"]);

                            // Add the data points directly to the chart
                            chartAttendance.Series[0].Points.AddXY(statusName, statusCount);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // SQA: Gracefully handle situations where the chart cannot load
                MessageBox.Show("Error loading chart data: " + ex.Message, "Chart Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void txtSearchName_TextChanged(object sender, EventArgs e)
        {
            // If the user deletes all text and the filter is on "All", reload the whole list
            if (string.IsNullOrWhiteSpace(txtSearchName.Text) && cmbSearchDept.Text == "All")
            {
                LoadEmployees();
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = dbHelper.GetConnection())
                {
                    // 1. The base query looks for any name that contains the typed letters
                    string query = "SELECT * FROM Employees WHERE FullName LIKE @Name";

                    // 2. If they picked a specific department (and not 'All'), we add that to the filter
                    if (cmbSearchDept.Text != "All" && !string.IsNullOrWhiteSpace(cmbSearchDept.Text))
                    {
                        query += " AND Department = @Dept";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // The "%" symbols tell SQL to find the text anywhere in the name (partial match)
                        cmd.Parameters.AddWithValue("@Name", "%" + txtSearchName.Text + "%");

                        if (cmbSearchDept.Text != "All" && !string.IsNullOrWhiteSpace(cmbSearchDept.Text))
                        {
                            cmd.Parameters.AddWithValue("@Dept", cmbSearchDept.Text);
                        }

                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        // 3. Update the grid with the filtered results
                        dgvEmployees.DataSource = dataTable;

                        // Bonus: Update your status bar to show how many records matched!
                        lblStatusCount.Text = "Search Results: " + dataTable.Rows.Count.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error searching: " + ex.Message, "Search Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void LoadIndividualAttendance(int employeeId)
        {
            try
            {
                using (SqlConnection conn = dbHelper.GetConnection())
                {
                    // Get the dates and status, ordered with the newest at the top
                    string query = "SELECT AttendanceDate as Date, Status FROM Attendance WHERE EmployeeID = @Id ORDER BY AttendanceDate DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", employeeId);

                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        // Attach it to your new grid
                        dgvIndividualAttendance.DataSource = dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading history: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



    }
}