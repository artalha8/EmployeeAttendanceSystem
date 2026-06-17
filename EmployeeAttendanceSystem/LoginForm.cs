using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EmployeeAttendanceSystem
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (txtUsername.Text == "admin" && txtPassword.Text == "1234")
            {
                
                this.Hide();

                
                Form1 dashboard = new Form1();
                dashboard.ShowDialog();

                
                this.Close();
            }
            else
            {
                
                MessageBox.Show("Invalid Username or Password. Please try again.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPassword.Clear(); 
            }
        }
    }
}
