using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace telephone_book
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        const string DISCARD_CHANGES_ERROR = "Discard your changes?";
        const string EMPTY_FIELD_ERROR = "Name and Phone cannot be empty.";
        const string DELETE_MESSAGE = "Are you sure want to delete this?";

        static AppData db;
        protected static AppData App
        {
            get
            {
                if (db == null)
                {
                    db = new AppData();
                }
                return db;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string fileName = string.Format("{0}//data.dat", Application.StartupPath);
            if (File.Exists(fileName))
            {
                App.TelephoneBook.ReadXml(fileName);
            }
            telephoneBookBindingSource.DataSource = App.TelephoneBook;
            SetEditMode(false);
            App.TelephoneBook.AcceptChanges();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {

        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            try
            {
                if (IsOkToCreateNew())
                {
                    AddEmptyContact();
                }
            }
            catch(Exception ex)
            {
                ExceptionHandler(ex);
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            SetEditMode(true);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(DISCARD_CHANGES_ERROR, "Message", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                App.TelephoneBook.RejectChanges();
                telephoneBookBindingSource.ResetBindings(false);
                SetEditMode(false);
            }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            try
            {
                telephoneBookBindingSource.EndEdit();
                if (IsOkToSave())
                {
                    SaveChanges();
                    SetEditMode(false);
                }

            }
            catch (Exception ex)
            {
                ExceptionHandler(ex);
                
            }
        }

        private void dataGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (dataGridView.RowCount == 0) return;
                if (MessageBox.Show(DELETE_MESSAGE, "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    telephoneBookBindingSource.RemoveCurrent();
                    SaveChanges();
                    SetEditMode(false);
                }
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSearch.Text))
            {
                var query = from o in App.TelephoneBook
                            where o.Phone.Contains(txtSearch.Text) || o.Name.Contains(txtSearch.Text)
                            select o;
                dataGridView.DataSource = query.ToList();
            }
            else
            {
                dataGridView.DataSource = telephoneBookBindingSource;
            }
        }

        private void ExceptionHandler(Exception ex)
        {
            if (ex.Message == DISCARD_CHANGES_ERROR)
            {
                if (MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                {
                    App.TelephoneBook.RejectChanges();
                    AddEmptyContact();
                }
            }
            else if (ex.Message == EMPTY_FIELD_ERROR)
            {
                MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                App.TelephoneBook.RejectChanges();
            }
        }

        const int PHONE_DATA_GRID_INDEX = 1;

        private void SetEditMode(bool mode)
        {
            txtSearch.Text = "";
            if (mode == true)
            {
                panel.Enabled = true;
                dataPanel.Enabled = false;
                btnEdit.Enabled = false;
                if (dataGridView.RowCount == 0) return;
                if (dataGridView.CurrentCell.ColumnIndex == PHONE_DATA_GRID_INDEX)
                {
                    txtPhone.Focus();
                }  
                else
                {
                    txtName.Focus();
                }
            }
            else
            {
                panel.Enabled = false;
                dataPanel.Enabled = true;
                txtSearch.Focus();
                btnEdit.Enabled = dataGridView.RowCount > 0 ? true : false;
            }
        }

        private void AddEmptyContact()
        {
            SetEditMode(true);
            App.TelephoneBook.AddTelephoneBookRow(App.TelephoneBook.NewTelephoneBookRow());
            telephoneBookBindingSource.MoveLast();
        }

        private bool IsOkToCreateNew()
        {
            if (panel.Enabled && telephoneBookBindingSource.DataSource != null)
            {
                throw new Exception(DISCARD_CHANGES_ERROR);
            }
            return true;
        }

        private bool IsOkToSave()
        {
            if (string.IsNullOrEmpty(txtName.Text) || string.IsNullOrEmpty(txtPhone.Text))
            {
                throw new Exception(EMPTY_FIELD_ERROR);
            }
            return true;
        }

        private void SaveChanges()
        {
            App.TelephoneBook.AcceptChanges();
            App.TelephoneBook.WriteXml(string.Format("{0}//data.dat", Application.StartupPath));
        }
    }
}
