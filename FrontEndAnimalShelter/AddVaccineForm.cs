using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using DataAccessLayer;
using System.Reflection;

namespace FrontEndAnimalShelter
{
    public partial class AddVaccineForm : Form
    {
        List<int> animaIds = new List<int>();
        DataGridViewRow vaccineRow;
        int employeeId = 0;

        /// <summary>
        /// Populates the vaccine form
        /// </summary>
        /// <exception cref="Exception"></exception>
        public AddVaccineForm()
        {
            try
            {
                InitializeComponent();
                MoreInitializing();
            }
            catch (Exception ex)
            {
                throw new Exception(MethodInfo.GetCurrentMethod().DeclaringType.Name + " " + MethodInfo.GetCurrentMethod().Name + " ->" + ex.Message);
            }

        }

        /// <summary>
        /// Adds animals to the vaccine form
        /// </summary>
        /// <param name="selectedAnimals"></param>
        /// <exception cref="Exception"></exception>
        public AddVaccineForm(DataGridViewSelectedRowCollection selectedAnimals)
        {
            try
            {
                InitializeComponent();
                MoreInitializing();
                foreach (DataGridViewRow animal in selectedAnimals)
                {
                    txtAnimalId.Text += animal.Cells["db_bridge_id"].Value.ToString() + " ";
                    animaIds.Add((int)animal.Cells["animal_id"].Value);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(MethodInfo.GetCurrentMethod().DeclaringType.Name + " " + MethodInfo.GetCurrentMethod().Name + " ->" + ex.Message);
            }
        }

        /// <summary>
        /// Initializes
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void MoreInitializing()
        {
            try
            {
                #region Data Binding
                AnimalMedical.vaccineDataTable dtVaccineTable = new AnimalMedical.vaccineDataTable();
                dtVaccineTable = Utility.GetVaccine();
                dgVaccineTable.DataSource = dtVaccineTable;
                #endregion

                FormatTable();

                #region Events
                dgVaccineTable.DataBindingComplete += DgVaccineTable_DataBindingComplete;
                dgVaccineTable.CellClick += DgVaccineTable_CellClick;
                txtEmpId.Leave += TxtEmpId_Leave;
                #endregion
            }
            catch (Exception ex)
            {
                throw new Exception(MethodInfo.GetCurrentMethod().DeclaringType.Name + " " + MethodInfo.GetCurrentMethod().Name + " ->" + ex.Message);
            }

        }

        private void TxtEmpId_Leave(object sender, EventArgs e)
        {
            try
            {
                if (txtEmpId.TextLength > 0)
                {
                    AnimalMedical.employeeDataTable dtEmployeeTable = Utility.GetEmployees();
                    var employeeResults = dtEmployeeTable.Where(x => x.employee_id == int.Parse(txtEmpId.Text)).ToList();
                    if (employeeResults.Count == 0)
                    {
                        //MessageBox.Show("Employee ID is not valid.");
                        errorEmpId.SetError(txtEmpId, "Employee ID is not valid");
                    }
                    else
                    {
                        employeeId = int.Parse(txtEmpId.Text);
                        errorEmpId.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(MethodInfo.GetCurrentMethod().DeclaringType.Name, MethodInfo.GetCurrentMethod().Name, ex.Message);
            }
        }

        /// <summary>
        /// Identifies when a row is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DgVaccineTable_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                DataGridView dg = (DataGridView)sender;
                vaccineRow = dg.Rows[0];  //only one row can be selected
            }
            catch (Exception ex)
            {
                HandleError(MethodInfo.GetCurrentMethod().DeclaringType.Name, MethodInfo.GetCurrentMethod().Name, ex.Message);
            }
        }

        /// <summary>
        /// Clears the selection on the vaccine table
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DgVaccineTable_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                dgVaccineTable.ClearSelection();
            }
            catch (Exception ex)
            {
                HandleError(MethodInfo.GetCurrentMethod().DeclaringType.Name, MethodInfo.GetCurrentMethod().Name, ex.Message);
            }
        }

        /// <summary>
        /// Formats the table
        /// </summary>
        private void FormatTable()
        {
            try
            {
                dgVaccineTable.Columns["vaccine_id"].Visible = false;
                dgVaccineTable.Columns["vaccine_name"].HeaderText = "Vaccine Name";
                dgVaccineTable.Columns["lot_id"].HeaderText = "Lot ID";
                dgVaccineTable.Columns["expiration"].HeaderText = "Expiration";
                dgVaccineTable.Columns["concentration"].HeaderText = "Concentration";
                dgVaccineTable.RowHeadersVisible = false;
            }
            catch (Exception ex)
            {
                HandleError(MethodInfo.GetCurrentMethod().DeclaringType.Name, MethodInfo.GetCurrentMethod().Name, ex.Message);
            }

        }

        /// <summary>
        /// Handles Button Submit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                string validIds = "";
                string invalidIds = "";
                AnimalMedical.animalDataTable aniamlDB = Utility.GetAnimals();

                if (animaIds.Count == 0)  //mutliple animals were not selected from the grid
                {
                    var animalIdResults = aniamlDB.Where(x => x.db_bridge_id == txtAnimalId.Text).Select(y => y.animal_id).ToList();
                    animaIds.Add(animalIdResults[0]); //collect the database animal id
                }

                foreach (int id in animaIds)
                {
                    //Check if animal has been added to the database
                    var validId = aniamlDB.Where(x => x.animal_id == id).ToList();  //does animal exist in database
                    if (validId.Count > 0) //animal does exist in the database
                    {
                        validIds += id + " ";
                        Utility.SaveAdministeredVaccine(validId[0].animal_id, (int)vaccineRow.Cells["vaccine_id"].Value, int.Parse(txtEmpId.Text), dateGiven.Value, dateDue.Value);
                    }
                    else  //animal id is not valid (not in database)
                    {
                        invalidIds += id + " ";
                    }
                }
                if (!string.IsNullOrEmpty(validIds))
                {
                    MessageBox.Show($"Vaccines for animals {validIds} have been saved.");
                }
                if (string.IsNullOrEmpty(invalidIds))
                {
                    this.Close();
                }
                else
                {
                    MessageBox.Show($"An invalid animal ID has been entered. The animal needs to be added to the database or there was a typo: {invalidIds}");
                    txtAnimalId.Text = invalidIds;
                    txtAnimalId.Focus();
                }
            }
            catch (Exception ex)
            {
                HandleError(MethodInfo.GetCurrentMethod().DeclaringType.Name, MethodInfo.GetCurrentMethod().Name, ex.Message);
            }
        }

        /// <summary>
        /// For handling errors
        /// </summary>
        /// <param name="sClass"></param>
        /// <param name="sMethod"></param>
        /// <param name="sMessage"></param>
        private void HandleError(string sClass, string sMethod, string sMessage)
        {
            try
            {
                MessageBox.Show(sClass + " " + sMethod + " -> " + sMessage);
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText("C:\\Error.txt", Environment.NewLine + "HandleError Exception: " + ex.Message);
            }
        }

    }
}
