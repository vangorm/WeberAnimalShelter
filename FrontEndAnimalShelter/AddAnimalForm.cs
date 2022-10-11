using DataAccessLayer;
using System;
using System.Windows.Forms;
using System.Linq;
using System.Reflection;

namespace FrontEndAnimalShelter
{
    public partial class AddAnimalForm : Form
    {
        /// <summary>
        /// This creates the form to add new animals
        /// </summary>
        public AddAnimalForm()
        {
            try { 
            InitializeComponent();
            AnimalMedical.animalDataTable dtAnimalTable = Utility.GetAnimals();

            #region data bind region

            AnimalMedical.speciesDataTable dtSpecies = Utility.GetSpecies();
            cmbBoxSpecies.DataSource = dtSpecies;
            cmbBoxSpecies.ValueMember = dtSpecies.species_idColumn.ColumnName;
            cmbBoxSpecies.DisplayMember = dtSpecies.species_nameColumn.ColumnName;
            cmbBoxSpecies.SelectedIndex = -1;

            AnimalMedical.kennelDataTable dtKennel = Utility.GetKennel();
            cmbKennel.DataSource = dtKennel;
            cmbKennel.ValueMember = dtKennel.kennel_idColumn.ColumnName;
            cmbKennel.DisplayMember = dtKennel.kennel_descriptionColumn.ColumnName;
            cmbKennel.SelectedIndex = -1;

            AnimalMedical.breedDataTable dtBreed = Utility.GetBreed();
            cmbBreed.DataSource = dtBreed;
            cmbBreed.ValueMember = dtBreed.breed_idColumn.ColumnName;
            cmbBreed.DisplayMember = dtBreed.breed_nameColumn.ColumnName;
            cmbBreed.SelectedIndex = -1;

            AnimalMedical.colorDataTable dtColor = Utility.GetColors();
            cmbColor.DataSource = dtColor;
            cmbColor.ValueMember = dtColor.color_idColumn.ColumnName;
            cmbColor.DisplayMember = dtColor.color_nameColumn.ColumnName;
            cmbColor.SelectedIndex = -1;
            #endregion

            cmbBoxSpecies.SelectionChangeCommitted += CmbBoxSpecies_SelectionChangeCommitted;
            }
            catch (Exception ex)
            {
                HandleError(MethodInfo.GetCurrentMethod().DeclaringType.Name, MethodInfo.GetCurrentMethod().Name, ex.Message);
            }

        }

        /// <summary>
        /// This identifies when the user changes a selection in the species cb
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CmbBoxSpecies_SelectionChangeCommitted(object sender, EventArgs e)
        {
            try { 
            AnimalMedical.breedDataTable dtBreed = Utility.GetBreed();

            if (cmbBoxSpecies.SelectedItem != null)
            {
                var breedList = dtBreed.Where(x => x.species_id.ToString().Equals(cmbBoxSpecies.SelectedValue.ToString())).ToList();
                cmbBreed.DataSource = breedList;
                cmbBreed.DisplayMember = dtBreed.breed_nameColumn.ColumnName;
                cmbBreed.SelectedValue = dtBreed.breed_idColumn.ColumnName;
            }
            else
                cmbBreed.DisplayMember = dtBreed.breed_nameColumn.ColumnName;
            }
            catch (Exception ex)
            {
                HandleError(MethodInfo.GetCurrentMethod().DeclaringType.Name, MethodInfo.GetCurrentMethod().Name, ex.Message);
            }
        }

        /// <summary>
        /// This handles storage when a user clicks submit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSubmit_Click(object sender, EventArgs e)
        {
           try
            {
                Decimal weight = string.IsNullOrEmpty(txtBxWeight.Text) ? 0 : decimal.Parse(txtBxWeight.Text);

                //the animalId must be unique.
                AnimalMedical.animalDataTable dtAnimalTable = Utility.GetAnimals();

                var findAnimalId = dtAnimalTable.Where(x => x.db_bridge_id == txtBxAnimalID.Text).ToList();

                //if the user selects an animal id that has already been taken the program will end. 
                if (findAnimalId.Count > 0)
                {
                    MessageBox.Show("The animal id " + txtBxAnimalID.Text + " has already been taken.\nChoose a different animal id!");
                    return;
                }

                string birthdate = dateTimePickerDateOfBirth.Value.ToString("yyyy-MM-dd");
                string intakeDate = dateTimePickerIntakeDate.Value.ToString("yyyy-MM-dd");
                string dueOutDate = dateTimePickerDueOutDate.Value.ToString("yyyy-MM-dd");
                int gender = cmbBoxGender.SelectedIndex;  //TODO: this is a string in this database, 
                int speciesID = int.Parse(cmbBoxSpecies.SelectedValue.ToString());

                //int kennelid = 0;
                //AnimalMedical.kennelDataTable kennelTable = Utility.GetKennel();
                //var findKennelName = kennelTable.Where(x => x.kennel_description == txtBxKennel.Text).Select(y => y.kennel_id).ToList();
                //if (findKennelName.Count > 0) {kennelid = findKennelName.First(); }
                int kennelId = 0;
                if (!string.IsNullOrEmpty(cmbKennel.SelectedValue.ToString()))
                {
                    kennelId = (int)cmbKennel.SelectedValue;
                }
                int colorId = 0;
                if (!string.IsNullOrEmpty(cmbColor.SelectedValue.ToString()))
                {
                    colorId = (int)cmbColor.SelectedValue;
                }
                int breedId = 0;
                if (!string.IsNullOrEmpty(cmbBreed.SelectedValue.ToString()))
                {
                    breedId = (int)cmbBreed.SelectedValue;
                }
                Utility.SaveAnimal(txtBxAnimalID.Text, txtBxName.Text, gender.ToString(), birthdate, txtBxMicrochipID.Text,
                     dueOutDate, intakeDate, txtBxNotes.Text, weight, kennelId, speciesID, ckbAltered.Checked,
                     colorId, breedId, ckbAdopted.Checked, ckbActive.Checked);
                Utility.SaveBreed(dtAnimalTable.Last().animal_id, breedId);
                Utility.SaveColor(dtAnimalTable.Last().animal_id, colorId);
                // Utility.SaveNotes();TODO

                MessageBox.Show($"Animal {txtBxAnimalID.Text} has been saved.");

                Reset();
            }
            catch (Exception ex)
            {
                HandleError(MethodInfo.GetCurrentMethod().DeclaringType.Name, MethodInfo.GetCurrentMethod().Name, ex.Message);
            }
        }

        /// <summary>
        /// When the user presses reset button, content on page resets
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReset_Click(object sender, EventArgs e)
        {
            try 
            { 
                Reset();
            }
            catch (Exception ex)
            {
                HandleError(MethodInfo.GetCurrentMethod().DeclaringType.Name, MethodInfo.GetCurrentMethod().Name, ex.Message);
            }
        }

        /// <summary>
        /// This function resets all information entered into fields by the user
        /// </summary>
        private void Reset()
        {
            try
            {
                txtBxAnimalID.Text = "";
                txtBxMicrochipID.Text = "";
                txtBxName.Text = "";
                txtBxNotes.Text = "";
                txtBxWeight.Text = "";

                ckbAltered.Checked = false;

                dateTimePickerDateOfBirth.Value = DateTime.Today;
                dateTimePickerDueOutDate.Value = DateTime.Today;
                dateTimePickerIntakeDate.Value = DateTime.Today;

                cmbBoxGender.SelectedItem = default;
                cmbBoxSpecies.SelectedItem = default;
                cmbKennel.SelectedItem = default;
                cmbColor.SelectedItem = default;
                cmbBreed.SelectedItem = default;
            }
             catch (Exception ex)
            {
                throw new Exception(MethodInfo.GetCurrentMethod().DeclaringType.Name + " " + MethodInfo.GetCurrentMethod().Name + " ->" + ex.Message);
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
