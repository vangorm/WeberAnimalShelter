using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using DataAccessLayer;

namespace FrontEndAnimalShelter
{
    public partial class AddPrescriptionForm : Form
    {
        /// <summary>
        /// Stores a list of animal IDs
        /// </summary>
        List<int> animaIds = new List<int>();

        /// <summary>
        /// Stores a medication row
        /// </summary>
        DataGridViewRow medicationRow;

        /// <summary>
        /// Initializes the prescription form
        /// </summary>
        public AddPrescriptionForm()
        {
            try 
            {
                InitializeComponent();
                MoreInitializing();
            }
            catch (Exception ex)
            {
                HandleError(MethodInfo.GetCurrentMethod().DeclaringType.Name, MethodInfo.GetCurrentMethod().Name, ex.Message);
            }
        }

        /// <summary>
        /// Populates a prescription form list for the selected animals
        /// </summary>
        /// <param name="selectedAnimals"></param>
        public AddPrescriptionForm(DataGridViewSelectedRowCollection selectedAnimals)
        {
            try
            {
                InitializeComponent();
                MoreInitializing();
                foreach (DataGridViewRow animal in selectedAnimals)
                {
                    txtAnimalid.Text += animal.Cells["db_bridge_id"].Value.ToString() + " ";
                    animaIds.Add((int)animal.Cells["animal_id"].Value);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(MethodInfo.GetCurrentMethod().DeclaringType.Name + " " + MethodInfo.GetCurrentMethod().Name + " ->" + ex.Message);
            }
        }

        /// <summary>
        /// Formats the table
        /// </summary>
        private void FormatTable()
        {
            try
            {
                AnimalMedical.unitDataTable dtUnitTable = Utility.GetUnit();
                SharedTables.Medication medication = new SharedTables.Medication();
                dgMedicationTable = medication.FormatMedicationTable(dgMedicationTable);

                dgMedicationTable.Columns["dose"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader;

                //remove the first column with no information in it.
                dgMedicationTable.RowHeadersVisible = false;
            }
            catch (Exception ex)
            {
                throw new Exception(MethodInfo.GetCurrentMethod().DeclaringType.Name + " " + MethodInfo.GetCurrentMethod().Name + " ->" + ex.Message);
            }
        }

        /// <summary>
        /// Handles submit click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                AnimalMedical.animalDataTable aniamlDB = Utility.GetAnimals();

                string validIds = "";
                string invalidIds = "";
                if (animaIds.Count == 0)  //mutliple animals were not selected from the grid
                {
                    var animalIdResults = aniamlDB.Where(x => x.db_bridge_id == txtAnimalid.Text).Select(y => y.animal_id).ToList();
                    animaIds.Add(animalIdResults[0]); //collect the database animal id
                }

                int frequencyid = 0;
                AnimalMedical.frequencyDataTable dtFrequency = Utility.GetFrequency();
                var frequencyidResult = dtFrequency.Where(x => (x.num_days == (int)numDays.Value) && (x.num_days == (int)numTimesPerDay.Value) && (x.desc_value == txtFrequencyDesc.Text)).Select(y => y.frequency_id).ToList();
                if (frequencyidResult.Count == 0)
                {
                    Utility.SaveFrequency((int)numDays.Value, (int)numTimesPerDay.Value, txtFrequencyDesc.Text);
                    dtFrequency = Utility.GetFrequency();
                    AnimalMedical.frequencyRow lastrow = dtFrequency.Last();
                    frequencyid = lastrow.frequency_id;
                }
                else
                {
                    frequencyid = frequencyidResult.First();
                }

                foreach (int id in animaIds)
                {
                    //Check if animal has been added to the database
                    var validId = aniamlDB.Where(x => x.animal_id == id).ToList();  //does animal exist in database
                    if (validId.Count > 0) //animal does exist in the database
                    {
                        validIds += id + " ";
                        int adminMethodId = 0;
                        if (!string.IsNullOrEmpty(cmbAdminMethod.SelectedValue.ToString()))
                        {
                            adminMethodId = (int)cmbAdminMethod.SelectedValue;
                        }

                        Utility.SavePrescription(validId[0].animal_id, int.Parse(medicationRow.Cells["medication_id"].Value.ToString()), txtDose.Text, adminMethodId, dateStart.Value, dateEnd.Value, txtStaff.Text, frequencyid, txtNotes.Text);
                        if (txtAdminStaff.Text.Length > 0)
                        {
                            //TODO validate employee number
                            Utility.SaveMedicationAdministrationLog(validId[0].animal_id, int.Parse(txtAdminStaff.Text), int.Parse(medicationRow.Cells["medication_id"].Value.ToString()), dateGiven.Value);
                        }
                    }
                    else  //animal id is not valid (not in database)
                    {
                        invalidIds += id + " ";
                    }
                }
                if (!string.IsNullOrEmpty(validIds))
                {
                    MessageBox.Show($"Prescriptions for animals {validIds} have been saved.");
                }
                if (string.IsNullOrEmpty(invalidIds))
                {
                    this.Close();
                }
                else
                {
                    MessageBox.Show($"An invalid animal ID has been entered. The animal needs to be added to the database or there was a typo: {invalidIds}");
                    txtAnimalid.Text = invalidIds;
                    txtAnimalid.Focus();
                }
            }
            catch (Exception ex)
            {
                HandleError(MethodInfo.GetCurrentMethod().DeclaringType.Name, MethodInfo.GetCurrentMethod().Name, ex.Message);
            }
        }

        /// <summary>
        /// Initializes the data
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void MoreInitializing()
        {
            try
            {
                AnimalMedical.medicationDataTable dtMedicationTable = Utility.GetMedication();
                dgMedicationTable.DataSource = dtMedicationTable;
                FormatTable();

                #region Data Binding
                AnimalMedical.admin_methodDataTable dtAdminMethod = Utility.GetAdminMethod();

                cmbAdminMethod.DataSource = dtAdminMethod;
                cmbAdminMethod.ValueMember = dtAdminMethod.admin_method_idColumn.ColumnName;  //value is the species ID code
                cmbAdminMethod.DisplayMember = dtAdminMethod.admin_method_nameColumn.ColumnName;  //value displayed is the species name
                cmbAdminMethod.SelectedItem = null;
                #endregion

                dgMedicationTable.CellClick += DgMedicationTable_CellClick;
                dgMedicationTable.DataBindingComplete += DgMedicationTable_DataBindingComplete;
            }
            catch (Exception ex)
            {
                throw new Exception(MethodInfo.GetCurrentMethod().DeclaringType.Name + " " + MethodInfo.GetCurrentMethod().Name + " ->" + ex.Message);
            }
        }

        /// <summary>
        /// Binds medication table to data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DgMedicationTable_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                dgMedicationTable.ClearSelection();  //clears any initial row selection that was not performed by user

                SharedTables.Medication medication = new SharedTables.Medication();
                dgMedicationTable = medication.GetUnitName(dgMedicationTable);
            }
            catch (Exception ex)
            {
                HandleError(MethodInfo.GetCurrentMethod().DeclaringType.Name, MethodInfo.GetCurrentMethod().Name, ex.Message);
            }
        }

        /// <summary>
        ///When user clicks on grid cell the entire row is selected. We collect that row information here 
        /// </summary>
        private void DgMedicationTable_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                //Cell click selects the entire row
                DataGridView dg = (DataGridView)sender;
                medicationRow = dg.SelectedRows[0];  //only one row can be selected
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
