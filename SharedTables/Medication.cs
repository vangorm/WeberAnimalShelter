using System;
using System.Reflection;
using System.Linq;
using DataAccessLayer;
using System.Windows.Forms;

namespace SharedTables
{
    public class Medication
    {
        public Medication() { }

        /// <summary>
        /// Formats the medication table
        /// </summary>
        /// <param name="dgmedicationTable"></param>
        /// <returns></returns>
        public DataGridView FormatMedicationTable(DataGridView dgmedicationTable)
        {
            try
            {
                AnimalMedical.medicationDataTable medicationTable = Utility.GetMedication();

                dgmedicationTable.Columns["medication_id"].Visible = false;
                dgmedicationTable.Columns["unit_id"].Visible = false;
                //Rename the headers
                dgmedicationTable.Columns["medication_name"].HeaderText = "Medication Name";
                dgmedicationTable.Columns["concentration"].HeaderText = "Concentration";
                dgmedicationTable.Columns["dose"].HeaderText = "Dose";
                //Adding unit name column
                DataGridViewColumn unitColumn = new DataGridViewColumn();
                unitColumn.CellTemplate = dgmedicationTable.Columns["medication_name"].CellTemplate;
                unitColumn.HeaderText = "Unit";
                unitColumn.Name = "unit_name";
                dgmedicationTable.Columns.Add(unitColumn);

                return dgmedicationTable;
            }
            catch (Exception ex)
            {
                throw new Exception(MethodInfo.GetCurrentMethod().DeclaringType.Name + " " + MethodInfo.GetCurrentMethod().Name + " ->" + ex.Message);
            }
        }

        /// <summary>
        /// Gets the type of unit from the database
        /// </summary>
        /// <param name="dgMedicationTable"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public DataGridView GetUnitName(DataGridView dgMedicationTable)
        {
            try
            {
                if (dgMedicationTable.Columns.Contains("unit_name"))
                {
                    AnimalMedical.unitDataTable dtUnitTable = Utility.GetUnit();

                    foreach (DataGridViewRow row in dgMedicationTable.Rows)
                    {
                        if (row.Cells["unit_id"].Value != null)
                        {
                            var unit = dtUnitTable.Where(x => x.unit_id.ToString() == row.Cells["unit_id"].Value.ToString()).Select(y => y.unit_name).ToList();
                            if (unit.Count > 0)
                                row.Cells["unit_name"].Value = unit.First();
                        }
                    }
                }
                return dgMedicationTable;
            }
            catch (Exception ex)
            {
                throw new Exception(MethodInfo.GetCurrentMethod().DeclaringType.Name + " " + MethodInfo.GetCurrentMethod().Name + " ->" + ex.Message);
            }
        }
    }

}
