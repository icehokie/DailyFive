using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace StudentActivityTracker
{
    /**
     * DB interface for the daily five program
     */ 
    class DailyFiveDB
    {
        const String DataSource = "DailyFive.mdb";
        DataSet dataMap;

        DataSet weeklyData;
        DataSet dailyData;
        System.Data.OleDb.OleDbDataAdapter dailyAdapter;

        System.Data.OleDb.OleDbConnection conn;
        DateTime curDate;

        //Two internal strings for determining SQL Selects based on sundays
        private String earlierActivitiesString;
        private String includeEarlierActivities;

        /**Do the initial connect for the current date
         */ 
        public DailyFiveDB(DateTime initialDate)
        {
            connectToAccess(initialDate);
        }

        /**
         * Initialize the date-independent data, this doesn't currently change during 
         * program execution
         */ 
        private void loadPersistentData()
        {
            System.Data.OleDb.OleDbDataAdapter da;
            try
            {
                conn.Open();
                String sql = "SELECT ActivityName, PeriodMax, Id From Activities";
                da = new System.Data.OleDb.OleDbDataAdapter(sql, conn);
                da.Fill(dataMap, "Activities");
                sql = "SELECT * From Students";
                da = new System.Data.OleDb.OleDbDataAdapter(sql, conn);
                da.Fill(dataMap, "Students");
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Failed to connect to data source");
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        /**
         * Initialize the connection to the access database
         * 
         */ 
        private void connectToAccess(DateTime initialDate)
        {
            dataMap = new DataSet();
            weeklyData = new DataSet();
            dailyData = new DataSet();
            curDate = initialDate;

            conn = new System.Data.OleDb.OleDbConnection();
            conn.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;" +
                @"Data source= "+DataSource+";";
            loadPersistentData();
            loadDate(curDate);
        }

        /**
         * Return true if currently loaded date  is sunday, otherwise return false
         */
        private bool isCurrentDateSunday()
        {
            if (!(curDate.DayOfWeek > DayOfWeek.Sunday))
            {
                return true;
            }
            return false;
        }

        /**
         * Deletes all data for the current data. Should be used in the save process only
         */
        public void clearDailyData()
        {
            try
            {
                conn.Open();
                DataSet joinRecords = new DataSet();
                dailyAdapter.Fill(joinRecords, "dailyJoins");
                System.Data.OleDb.OleDbCommandBuilder cb = new System.Data.OleDb.OleDbCommandBuilder(dailyAdapter);

                int oldJoin = joinRecords.Tables["dailyJoins"].Rows.Count;
                for (int i = oldJoin - 1; i >= 0; i--)
                {
                    joinRecords.Tables["dailyJoins"].Rows[i].Delete();
                    dailyAdapter.Update(joinRecords, "dailyJoins");
                }
            }
            catch (Exception ex)
            {

                System.Windows.Forms.MessageBox.Show("Failed to Clear Data");
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        /**
         * Populate the student-activity joins for the currently loaded date
         */ 
        public void loadDate(DateTime curDate)
        {
            try
            {
                conn.Open();
                System.Data.OleDb.OleDbDataAdapter weeklyDataAdapter = new System.Data.OleDb.OleDbDataAdapter();
                dailyAdapter = new System.Data.OleDb.OleDbDataAdapter();
                earlierActivitiesString = "";
                includeEarlierActivities = "";

                //There are no 'earlier activities' if today is sunday
                if (!isCurrentDateSunday())
                {
                    DateTime date = curDate;
                    date = date.AddDays(-1);
                    earlierActivitiesString = "(";
                    for (int i = (int)date.DayOfWeek; i >= (int)DayOfWeek.Sunday; i--)
                    {
                        earlierActivitiesString = earlierActivitiesString + includeEarlierActivities +
                            "SchoolDay = '" + date.Date.ToString("d") + "'";
                        includeEarlierActivities = " OR ";
                        date = date.AddDays(-1);
                    }
                    earlierActivitiesString = earlierActivitiesString + ")";
                }
                String weeklySQL = "SELECT * FROM StudentActivityJoin WHERE SchoolDay='" + curDate.Date.ToString("d") + "'"
                    + includeEarlierActivities + earlierActivitiesString;
                weeklyDataAdapter = new System.Data.OleDb.OleDbDataAdapter(weeklySQL, conn);
                weeklyDataAdapter.Fill(weeklyData, "dailyJoins");

                String dailySQL = "SELECT * FROM StudentActivityJoin WHERE SchoolDay='" + curDate.Date.ToString("d") + "'";
                dailyAdapter = new System.Data.OleDb.OleDbDataAdapter(dailySQL, conn);
                dailyAdapter.Fill(dailyData, "dailyJoins");
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Failed to Load Data");
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        /**
         * Gets the period that the activity was completed for the student + activity pair
         * for the currently loaded date.
         * 
         * If it wasn't selected return '0'
         */
        public String getCurrentStudentActivityPeriod(int studentId, int activityId)
        {
            DataRow[] currentlyCheckedRows =
                weeklyData.Tables["dailyJoins"].Select("Student ='" + studentId
                + "' AND Activity = '" + activityId + "' AND SchoolDay = '"
                + curDate.Date.ToString("d") + "'");

            String period = "0";
            if (currentlyCheckedRows.Length > 0)
            {
                period = currentlyCheckedRows[0].Field<string>("Period");
            }
            return period;
        }

        /**
         * Creates a new row to set the activity to checked for the student.
         * 
         * Runs off the assumption that this row doesn't exist, should probably add some fail safes though
         */ 
        public void SetCheckedForStudent(int studentId, int activityId, String period)
        {
            try
            {
                conn.Open();

                DataRow dr = dailyData.Tables["dailyJoins"].NewRow();
                dr["Student"] = "" + studentId;
                dr["SchoolDay"] = curDate.Date.ToString("d");
                dr["Activity"] = "" + activityId;
                dr["Period"] = period;
                dailyData.Tables["dailyJoins"].Rows.Add(dr);
                dailyAdapter.Update(dailyData, "dailyJoins");
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Failed to Save Data");
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        /**
         * Get the number of Activites this students has previously selected this week
         */ 
        public int totalStudentActivitesForWeek(int studentId)
        {
            //Save time is today is Sunday
            if (isCurrentDateSunday())
            {
                return 0;
            }
            return weeklyData.Tables["dailyJoins"].Select("Student = '" + studentId
                                    + "' AND " + earlierActivitiesString).Length;
        }

        /**
         * Returns true if the student that is passed in has already selected this activity prior to today.
         */ 
        public bool activityUsedEarlierInWeek(int student, int activityId)
        {
            //Save time if today is sunday
            if (isCurrentDateSunday())
            {
                return false;
            }
            return weeklyData.Tables["dailyJoins"].Select("Student = '" + student
                + "' AND Activity = '" + activityId + "' AND "
                + earlierActivitiesString).Length > 0;
        }

        /**
         * Get the number of students
         */ 
        public int getStudentCount()
        {
            return dataMap.Tables["Students"].Rows.Count;
        }

        /**
         * Get the number of Activities
         */ 
        public int getActivityCount()
        {
            return dataMap.Tables["Activities"].Rows.Count;
        }

        /**
         * Return the first name of the passed in student
         */ 
        public String getStudentFirstName(int studentNumber)
        {
            return dataMap.Tables["Students"].Rows[studentNumber].ItemArray.GetValue(1).ToString();
        }

        /**
         * Return the last name of the passed in student
         */ 
        public String getStudentLastName(int studentNumber)
        {
            return dataMap.Tables["Students"].Rows[studentNumber].ItemArray.GetValue(2).ToString();
        }

        /**
         * Return the activity name of the passed in activity id.
         */
        public String getActivityFromId(int activityId)
        {
            int row = getActivityRowFromKey(activityId);
            return dataMap.Tables["Activities"].Rows[getActivityRowFromKey(activityId)].ItemArray.GetValue(0).ToString();
        }

        /**
         * Return the activity name of the passed in row number.
         */
        public String getActivityFromRow(int rowNumber)
        {
            return dataMap.Tables["Activities"].Rows[rowNumber].ItemArray.GetValue(0).ToString();
        }

        /**
         * Returns whether or not tha passed in activity has a max per period
         */ 
        public bool activityHasPeriodMax(int activityId)
        {
            int row = getActivityRowFromKey(activityId);
            if (dataMap.Tables["Activities"].Rows[row].ItemArray.GetValue(1).ToString() != "")
            {
                return true;
            }
            return false;
        }

        /**
         * Returns the max for the activity period if it has one, -1 otherwise
         */ 
        public int getActivityPeriodMax(int activityId)
        {
            int row = getActivityRowFromKey(activityId);
            if (dataMap.Tables["Activities"].Rows[row].ItemArray.GetValue(1).ToString() != "")
            {
                return atoi(dataMap.Tables["Activities"].Rows[row].ItemArray.GetValue(1).ToString());
            }
            return -1;
        }

        /**
         * Return the key associated with the activity name
         */ 
        public int getActivityKey(String activityName)
        {
            int max = getActivityCount();
            for (int i = 0; i < max; i++)
            {
                if (dataMap.Tables["Activities"].Rows[i].ItemArray.GetValue(0).ToString() == activityName)
                {
                    return atoi(dataMap.Tables["Activities"].Rows[i].ItemArray.GetValue(2).ToString());
                }
            }
            //didn't find it
            return - 1;
        }

        /**
         * Return the row associated with the activity name
         */
        public int getActivityRow(String activityName)
        {
            int max = getActivityCount();
            for (int i = 0; i < max; i++)
            {
                if (dataMap.Tables["Activities"].Rows[i].ItemArray.GetValue(0).ToString() == activityName)
                {
                    return i;
                }
            }
            //didn't find it
            return -1;
        }

        public int getActivityKeyFromRow(int row)
        {
            return atoi(dataMap.Tables["Activities"].Rows[row].ItemArray.GetValue(2).ToString());
        }

        public int getActivityRowFromKey(int key)
        {
            int total = getActivityCount();
            for (int i = 0; i < total; i++)
            {
                if (key == atoi(dataMap.Tables["Activities"].Rows[i].ItemArray.GetValue(2).ToString()))
                {
                    return i;
                }
            }
            return -1;
        }

        /**
         * Convert a string to an int
         */
        private int atoi(String a)
        {
            int toReturn = -1;
            try
            {
                toReturn = Convert.ToInt32(a);
            }
            catch
            {
                //if we get here then there was not a student for that number
            }
            return toReturn;
        }

        /**
         * Pass in the number of the student and get that student's id
         */ 
        public int getStudentIdForNumber(int studentNumber)
        {
            return atoi(dataMap.Tables["Students"].Rows[studentNumber].ItemArray.GetValue(0).ToString());
        }
    }
}
