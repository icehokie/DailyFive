using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace StudentActivityTracker
{
    class DailyFiveUpdater
    {
        System.Data.OleDb.OleDbDataAdapter versionAdapter;
        DataSet versionData;
        DataSet activityMap;
        DataSet studentMap;
        DataSet joinMap;

        const String DataSource = "DailyFive.mdb";
        System.Data.OleDb.OleDbConnection conn;
        const int VERSION_NUMBER = 1;

        public DailyFiveUpdater()
        {
            connectToAccess();
            conn.Open();
            int version = getVersionNumber();
            switch (version)
            {
                case 0:
                    version0();
                    goto default;
                default:
                    updateToLatestVersion();
                    MessageBox.Show("Successfully updated from version " + version + " to version "
                        + VERSION_NUMBER + ".");
                    break;
                case VERSION_NUMBER:
                    break;
            }
            conn.Close();
        }

        private void updateToLatestVersion()
        {
            string updateVersion = "UPDATE SystemSettings SET Version = " + VERSION_NUMBER;
            OleDbCommand command = new OleDbCommand(updateVersion, conn);
            command.ExecuteNonQuery();
        }


        /**
         * Change activity name to id...Oh wow why did i use access....
         * 
         */ 
        private void version0()
        {
            string createSystemSettings ="CREATE TABLE SystemSettings(ID  int, Version String)";
            OleDbCommand command = new OleDbCommand(createSystemSettings, conn);
            command.ExecuteNonQuery();

            string addPeriodMaxColumn = "ALTER TABLE Activities ADD PeriodMax String";
            command.CommandText = addPeriodMaxColumn;
            command.ExecuteNonQuery();

            string addPeriodColumn = "ALTER TABLE StudentActivityJoin ADD Period String";
            command.CommandText = addPeriodColumn;
            command.ExecuteNonQuery();

            string updatePeriodColumn = "UPDATE StudentActivityJoin SET Period ='1'";
            command.CommandText = updatePeriodColumn;
            command.ExecuteNonQuery();

            System.Data.OleDb.OleDbDataAdapter activityAdapter = new System.Data.OleDb.OleDbDataAdapter();
            activityMap = new DataSet();
            String sql = "SELECT * From Activities";
            activityAdapter = new System.Data.OleDb.OleDbDataAdapter(sql, conn);
            activityAdapter.Fill(activityMap, "Activities");

            System.Data.OleDb.OleDbDataAdapter studentAdapter = new System.Data.OleDb.OleDbDataAdapter();
            studentMap = new DataSet();
            sql = "SELECT * From Students";
            studentAdapter = new System.Data.OleDb.OleDbDataAdapter(sql, conn);
            studentAdapter.Fill(studentMap, "Students");

            int activityCount = activityMap.Tables["Activities"].Rows.Count;
            for (int i = 0; i < activityCount; i++)
            {
                string activityName = activityMap.Tables["Activities"].Rows[i].ItemArray.GetValue(1).ToString();
                string updateJointable = @"UPDATE StudentActivityJoin SET Activity = '"
                + activityMap.Tables["Activities"].Rows[i].ItemArray.GetValue(0).ToString()
                + "' WHERE Activity = '" + activityName + "'";
                command.CommandText = updateJointable;
                command.ExecuteNonQuery();
            }

            int studentCount = studentMap.Tables["Students"].Rows.Count;
            string updateJointable2 = @"UPDATE StudentActivityJoin SET Student = 't' & Student";
            command.CommandText = updateJointable2;
            command.ExecuteNonQuery();
            for (int i = 0; i < studentCount; i++)
            {
                int tens = i / 10 + 1;
                string studentId = studentMap.Tables["Students"].Rows[i].ItemArray.GetValue(0).ToString();
                string updateJointable = @"UPDATE StudentActivityJoin SET Student = '"
                    + studentId + "' WHERE Student  = 't" + i + "'";
                command.CommandText = updateJointable;
                command.ExecuteNonQuery();
            }

            string updateVersion = "INSERT INTO SystemSettings VALUES(1," + VERSION_NUMBER +")";
            command = new OleDbCommand(updateVersion, conn);
            command.ExecuteNonQuery();

        }

        private void connectToAccess()
        {
            conn = new System.Data.OleDb.OleDbConnection();
            conn.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;" +
                @"Data source= " + DataSource + ";";
        }

        private int getVersionNumber()
        {
            versionAdapter = new System.Data.OleDb.OleDbDataAdapter();
            String sql = "SELECT * FROM SystemSettings";
            int version = 0;
            try
            {
                versionData = new DataSet();
                versionAdapter = new System.Data.OleDb.OleDbDataAdapter(sql, conn);
                versionAdapter.Fill(versionData, "Version");
                version = atoi(versionData.Tables["Version"].Rows[0].ItemArray.GetValue(0).ToString());
            }
            catch
            {
                //failed getting version number so we must be version 0
            }

            return version;

        }

        private int atoi(String integer)
        {
            int toReturn = 0;
            try
            {
                toReturn = Convert.ToInt32(integer);
            }
            catch
            {
                //Should never get here...
            }
            return toReturn;
        }

    }
}
