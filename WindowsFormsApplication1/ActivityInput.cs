using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Data.OleDb;
using System.Drawing.Drawing2D;
using System.Windows.Forms.Design;
using Owf.Controls;

namespace StudentActivityTracker
{

    public partial class ActivityInput : Form
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        DataSet ds1;
        FormState fState;
        DateTime curDate = DateTime.Now;
        int PAGE_TOTAL = 5;
        List<Bitmap> bgList;
        
        //Need to dynanimaclly determine this in future.
        int MAX_ACTIVITY_LEN = 15;

        public ActivityInput()
        {
            bgList = new List<Bitmap>();
            InitializeComponent();
            ConnectToAccess();

            fState = new FormState();
            fState.Maximize(this);
            this.Text = "Daily Five";
            this.MinimizeBox = true;

        }

        public void ConnectToAccess()
        {
            ds1 = new DataSet();
            System.Data.OleDb.OleDbDataAdapter da;

            System.Data.OleDb.OleDbConnection conn;
            conn = new System.Data.OleDb.OleDbConnection();
            //conn.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;" +
                //@"Data source= C:\Users\Justin\Documents\DailyFive.mdb;";
            conn.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;" +
                @"Data source= DailyFive.mdb;";

            
            try
            {
                conn.Open();
                String sql = "SELECT ActivityName From Activities";
                da = new System.Data.OleDb.OleDbDataAdapter(sql, conn);
                da.Fill(ds1, "Activities");
                sql = "SELECT * From Students";
                da = new System.Data.OleDb.OleDbDataAdapter(sql, conn);
                da.Fill(ds1, "Students");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to connect to data source");
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private void SetToParentSize(Control ctrl, Control parent)
        {
            ctrl.Top = parent.Top;
            ctrl.Left = parent.Left;
            ctrl.Size = parent.Size;
        }

        private void DisplayActivities()
        {
            int windowWidth = Control.FromHandle(GetForegroundWindow()).Width;
            int windowHeight = Control.FromHandle(GetForegroundWindow()).Height;
            int vPadding = (int)(windowHeight / 60.0);
            int hPadding = (int)(windowWidth / 80.0);
            int hSize = (int)(windowWidth / 5.4);
            int vSize = (int)(windowHeight / 1.45);
            int studentCount = ds1.Tables["Students"].Rows.Count;
            int activityCount = ds1.Tables["Activities"].Rows.Count;
            int buttonHeight = (int)(vSize - vSize / 4 - 5 * 1.0) / activityCount;
            Font buttonFont = new Font("Arial", buttonHeight / 3);
            Font navFont = new Font("Arial", buttonHeight / 6);

            int strLength = MAX_ACTIVITY_LEN < 8 ? 8 : MAX_ACTIVITY_LEN;
            String tempSize = "";
            for (int i = 0; i < strLength; i++)
            {
                tempSize = tempSize + "A";
            }
            float prevBFontSize = buttonFont.Size;
            SizeF sizeB;
            using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(new Bitmap(1, 1)))
            {
                sizeB = graphics.MeasureString(tempSize, buttonFont);
            }

            float desiredBSize = buttonHeight - 5;
            float maxBFontHeight = (buttonFont.Size * desiredBSize) / sizeB.Height;
            float maxBFontWidth = (buttonFont.Size * (hSize - 15)) / sizeB.Width;
            float newBFontSize = maxBFontHeight < maxBFontWidth ? maxBFontHeight : maxBFontWidth;
            buttonFont = new Font("Arial", newBFontSize);
            navFont = new Font("Arial", newBFontSize);

            Point listOffset = new Point(0,5);

            DataRow dr = ds1.Tables["Students"].Rows[0];
            TabControl bg = new TabControl();
            bg.Name = "backgroundPage";
            TabPage tp1 = new TabPage();
            tp1.Name = "studentPage0";
            tp1.TabIndex = 0;
            //tp1.BackColor = Color.GhostWhite;

            bg.Height = windowHeight;
            bg.Width = windowWidth;

            Point topLeft = new Point(4, 8);

            bg.Location = new Point(0, 0);

            DataRow dr2 = ds1.Tables["Students"].Rows[0];
            for (int i = 0; i < studentCount; i++)
            {
                if (i > 0 && i % PAGE_TOTAL == 0)
                {
                    Button pageNext = new Button();
                    pageNext.Location = new Point(hSize * 4 + hSize / 2 + hPadding * 5, (int)(vSize + (windowHeight-vSize)/2.5 + vPadding * 2));
                    pageNext.Width = (int)(hSize / 2.0);
                    pageNext.Height = (int)(buttonHeight - 5);
                    pageNext.Text = "Next";
                    pageNext.Click += new EventHandler(this.NextBtn_Click);
                    pageNext.Font = navFont;
                    tp1.Controls.Add(pageNext);

                    bg.Controls.Add(tp1);

                    tp1 = new TabPage();
                    Button pagePrev = new Button();
                    pagePrev.Location = new Point(hSize * 4 + hPadding * 5, (int)(vSize + (windowHeight - vSize) / 2.5 + vPadding * 2));
                    pagePrev.Width = (int)(hSize / 2.0);
                    pagePrev.Height = (int)(buttonHeight - 5);
                    pagePrev.Text = "Previous";
                    pagePrev.Click += new EventHandler(this.PrevBtn_Click);
                    pagePrev.Font = navFont;
                    tp1.Controls.Add(pagePrev);
                    tp1.Name = "studentPage" + i / PAGE_TOTAL;
                }
                //Point curLoc = new Point((int)(((i%10)%5+1)*hPadding +((i%10)%5) * hSize), 
                    //(int)((((i%10)/5)+1)*vPadding + ((i%10)/5)*vSize));
                Point curLoc = new Point((int)((i%5+1)*hPadding +(i%5) * hSize), 
                      (int)((windowHeight - vSize)/2.5));

                GroupBox studentGroup = new GroupBox();
                //studentGroup.BackColor = Color.Gainsboro;
                studentGroup.Padding = DefaultPadding;
                studentGroup.Height = vSize;
                studentGroup.Width = hSize;
                studentGroup.Location = curLoc;
                studentGroup.Name = ""+i;
                //studentGroup.Paint += new PaintEventHandler(panel1_Paint);
                //studentGroup.BackgroundImage = bmp;
                //studentGroup.ForeColor = Color.Aqua;

                GroupBox nameGroup = new GroupBox();
                nameGroup.Width = hSize-7;
                nameGroup.Height = vSize/4;
                nameGroup.Location = topLeft;
                //nameGroup.BackColor = Color.GhostWhite;

                Label studentFirstName = new Label();
                Label studentLastName = new Label();

                studentFirstName.Text = ds1.Tables["Students"].Rows[i].ItemArray.GetValue(1).ToString();
                studentLastName.Text = ds1.Tables["Students"].Rows[i].ItemArray.GetValue(2).ToString();
                float prevFontSize = studentFirstName.Font.Size;
                SizeF sizeFirst, sizeLast;
                using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(new Bitmap(1, 1)))
                {
                    sizeFirst = graphics.MeasureString(studentFirstName.Text, studentFirstName.Font );
                    sizeLast = graphics.MeasureString(studentLastName.Text, studentLastName.Font );
                }

                float desiredSize = nameGroup.Height/(float)(3.0);
                float maxFontHeight = (studentFirstName.Font.Size * desiredSize) / sizeFirst.Height;
                float maxFontWidth = sizeFirst.Width < sizeLast.Width ? (studentLastName.Font.Size * (nameGroup.Width - 5)) / sizeLast.Width
                    : (studentFirstName.Font.Size * (nameGroup.Width - 5)) / sizeFirst.Width;
                float newFontSize = maxFontHeight < maxFontWidth ? maxFontHeight : maxFontWidth;
                Font nameFont = new Font("Arial", newFontSize);

                studentFirstName.AutoSize = true;
                studentLastName.AutoSize = true;

                studentFirstName.Font = nameFont;
                studentLastName.Font = nameFont;

                using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(new Bitmap(1, 1)))
                {
                    sizeFirst = graphics.MeasureString(studentFirstName.Text, studentFirstName.Font );
                    sizeLast = graphics.MeasureString(studentLastName.Text, studentLastName.Font );
                }

                studentFirstName.Location = new Point((int)(studentGroup.Width - sizeFirst.Width) / 2, nameGroup.Height / 6);
                studentLastName.Location = new Point((int)(studentGroup.Width - sizeLast.Width) / 2, (int)(nameGroup.Height - sizeLast.Height - nameGroup.Height/6 ));

                nameGroup.Controls.Add(studentFirstName);
                nameGroup.Controls.Add(studentLastName);

                studentGroup.Controls.Add(nameGroup);

                CreateButtonBackgrounds(nameGroup.Width - 8,buttonHeight - 5 );
                for (int j = 0; j < activityCount; j++)
                {
                    CheckBox newButton = new CheckBox();
                    newButton.Appearance = Appearance.Button;
                    newButton.Text = ds1.Tables["Activities"].Rows[j].ItemArray.GetValue(0).ToString();
                    newButton.Font = buttonFont;
                    newButton.Location = new Point (nameGroup.Left + 5, nameGroup.Bottom + 5 + j*buttonHeight);
                    newButton.Height = buttonHeight - 5;
                    newButton.Width = nameGroup.Width - 8;
                    newButton.TextAlign = ContentAlignment.MiddleCenter;
                    newButton.Click += new EventHandler(this.ActivityBtn_Click);
                    studentGroup.Controls.Add(newButton);
                }
                tp1.Controls.Add(studentGroup);
            }
            bg.TabPages.Add(tp1);

            TabPage controlPage = new TabPage();
            Button saveButton = new Button();
            saveButton.Text = "Save";
            controlPage.Controls.Add(saveButton);
            saveButton.Click += new EventHandler(this.SaveBtn_Click);

            DateTimePicker controlDate = new DateTimePicker();
            controlDate.Value = curDate;
            controlDate.Left = saveButton.Left;
            controlDate.Top = saveButton.Bottom + 5;
            controlPage.Controls.Add(controlDate);
            controlDate.ValueChanged += new EventHandler(this.ControlDate_ValueChanged);


            bg.TabPages.Add(controlPage);




            this.Controls.Add(bg);
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            base.OnPaint(e);
            
            Panel panel1 = (Panel)sender;
            ControlPaint.DrawBorder3D(e.Graphics, panel1.ClientRectangle, Border3DStyle.Raised, Border3DSide.All);
        }

        void CreateButtonBackgrounds(int height, int width)
        {
            bgList.Clear();
            Bitmap bmp = new Bitmap(height, width);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                Rectangle r = new Rectangle(0, 0, bmp.Width, bmp.Height);
                using (LinearGradientBrush br = new LinearGradientBrush(
                                                    r,
                                                    System.Drawing.Color.Gainsboro,
                                                    System.Drawing.Color.Silver,
                                                    System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                {
                    g.FillRectangle(br, r);
                }
            }
            bgList.Add(bmp);

            bmp = new Bitmap(height, width);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                Rectangle r = new Rectangle(0, 0, bmp.Width, bmp.Height);
                using (LinearGradientBrush br = new LinearGradientBrush(
                                                    r,
                                                    System.Drawing.Color.LightSkyBlue,
                                                    System.Drawing.Color.LightBlue,
                                                    System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                {
                    g.FillRectangle(br, r);
                }
            }
            bgList.Add(bmp);

            bmp = new Bitmap(height, width);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                Rectangle r = new Rectangle(0, 0, bmp.Width, bmp.Height);
                using (LinearGradientBrush br = new LinearGradientBrush(
                                                    r,
                                                    System.Drawing.Color.Orange,
                                                    System.Drawing.Color.OrangeRed,
                                                    System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                {
                    g.FillRectangle(br, r);
                }
            }
            bgList.Add(bmp);
        }

        Bitmap GetBackgroundForClick(String number = "0")
        {
            int numVal;
            try
            {
                numVal = Convert.ToInt32(number);
                if (numVal > bgList.Count - 1)
                {
                    numVal = bgList.Count - 1;
                }
                else if (number == null)
                {
                    numVal = bgList.Count - 1;
                }
            }
            catch (Exception e)
            {
                numVal = bgList.Count - 1;
            }
            return bgList[numVal];
        }

        void LoadDate()
        {
            System.Data.OleDb.OleDbConnection conn;
            conn = new System.Data.OleDb.OleDbConnection();
            //conn.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;" +
                //@"Data source= C:\Users\Justin\Documents\DailyFive.mdb;";
            conn.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;" +
                @"Data source= DailyFive.mdb;";

            try
            {
                conn.Open();
                OleDbDataAdapter da = new OleDbDataAdapter();
                Boolean isSunday = false;
                String earlierActivitiesString = "";
                String includeEarlierActivities = "";

                if (!(curDate.DayOfWeek > DayOfWeek.Sunday))
                {
                    isSunday = true;
                }
                else
                {
                    DateTime date = curDate;
                    date = date.AddDays(-1);
                    earlierActivitiesString = "(";
                    for(int i = (int)date.DayOfWeek; i >= (int)DayOfWeek.Sunday; i-- )
                    {
                        earlierActivitiesString = earlierActivitiesString + includeEarlierActivities +
                            "SchoolDay = '" + date.Date.ToString("d") + "'";
                        includeEarlierActivities = " OR ";
                        date = date.AddDays(-1);
                    }
                    earlierActivitiesString = earlierActivitiesString + ")";
                }

                String sql = "SELECT * FROM StudentActivityJoin WHERE SchoolDay='" + curDate.Date.ToString("d") + "'"
                    +includeEarlierActivities + earlierActivitiesString;
                DataSet joinRecords = new DataSet();
                da = new OleDbDataAdapter(sql, conn);
                da.Fill(joinRecords, "dailyJoins");
                TabControl tc = Controls["backgroundPage"] as TabControl;
                int studentCount = ds1.Tables["Students"].Rows.Count;
                int activityCount = ds1.Tables["Activities"].Rows.Count;
                TabPage tp = tc.Controls["studentPage0"] as TabPage;

                for (int i = 0; i < studentCount; i++)
                {
                    GroupBox studentGroup = tp.Controls["" + i] as GroupBox;
                    foreach (var ctl in studentGroup.Controls)
                    {
                        if (ctl is CheckBox)
                        {
                            CheckBox curButton = (CheckBox)ctl;
                            string s = curButton.Text;
                            DataRow[] currentlyCheckedRows =
                                joinRecords.Tables["dailyJoins"].Select("Student ='" + i
                                        + "' AND Activity = '" + curButton.Text + "' AND SchoolDay = '"
                                        + curDate.Date.ToString("d") + "'");

                            if (currentlyCheckedRows.Length > 0)
                            {
                                String period = currentlyCheckedRows[0].Field<string>("Period");
                                curButton.Tag = period;
                                curButton.Checked = true;
                                curButton.BackgroundImage = GetBackgroundForClick(period);
                            }
                            else
                            {
                                curButton.Checked = false;
                                curButton.BackgroundImage = GetBackgroundForClick();
                            }
                            if (!isSunday)
                            {
                                DataRow[] totalActivityCount = joinRecords.Tables["dailyJoins"].Select("Student = '" + i
                                    + "' AND " + earlierActivitiesString);
                                DataRow[] previouslyUsedActivities =
                                    joinRecords.Tables["dailyJoins"].Select("Student = '" + i
                                        + "' AND Activity = '" + curButton.Text + "' AND "
                                        + earlierActivitiesString);
                                if (totalActivityCount.Length >= activityCount)
                                {
                                    curButton.Visible = true;
                                }
                                else if (previouslyUsedActivities.Length > 0)
                                {
                                    curButton.Visible = false;
                                }
                                else
                                {
                                    curButton.Visible = true;
                                }
                            }
                            else
                            {
                                curButton.Visible = true;
                            }
                        }
                    }
                    if ((i + 1) % PAGE_TOTAL == 0)
                    {
                        //if there's an exact even amount this returns null for
                        //tp but won't advance to the next loop
                        tp = tc.Controls["studentPage" + ((i + 1) / PAGE_TOTAL)] as TabPage;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to Load Data");
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveChanges();
            base.OnFormClosing(e);
        }

        void SaveChanges()
        {

            System.Data.OleDb.OleDbConnection conn;
            conn = new System.Data.OleDb.OleDbConnection();
            //conn.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;" +
                //@"Data source= C:\Users\Justin\Documents\DailyFive.mdb;";
            conn.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;" +
                @"Data source= DailyFive.mdb;";

            try
            {
                conn.Open();
                OleDbDataAdapter da = new OleDbDataAdapter();
                String sql = "SELECT * FROM StudentActivityJoin WHERE SchoolDay='" + curDate.Date.ToString("d") + "'";
                DataSet joinRecords = new DataSet();
                da = new OleDbDataAdapter(sql, conn);
                da.Fill(joinRecords, "dailyJoins");
                OleDbCommandBuilder cb = new OleDbCommandBuilder(da);

                TabControl tc = Controls["backgroundPage"] as TabControl;
                int studentCount = ds1.Tables["Students"].Rows.Count;
                TabPage tp = tc.Controls["studentPage0"] as TabPage;

                int oldJoin = joinRecords.Tables["dailyJoins"].Rows.Count;
                for (int i = oldJoin - 1; i >= 0; i--)
                {
                    joinRecords.Tables["dailyJoins"].Rows[i].Delete();
                    da.Update(joinRecords, "dailyJoins");
                }
                for (int i = 0; i < studentCount; i++)
                {
                    GroupBox studentGroup = tp.Controls["" + i] as GroupBox;
                    foreach (var ctl in studentGroup.Controls)
                    {
                        if (ctl is CheckBox)
                        {
                            CheckBox curButton = (CheckBox)ctl;
                            if (curButton.Checked)
                            {
                                DataRow dr = joinRecords.Tables["dailyJoins"].NewRow();
                                dr["Student"] = ""+i;
                                dr["SchoolDay"] = curDate.Date.ToString("d");
                                dr["Activity"] = curButton.Text;
                                dr["Period"] = curButton.Tag;
                                joinRecords.Tables["dailyJoins"].Rows.Add(dr);
                                da.Update(joinRecords, "dailyJoins");

                            }
                        }
                    }
                    if ((i + 1) % PAGE_TOTAL == 0)
                    {
                        //if there's an exact even amount this returns null for
                        //tp but won't advance to the next loop
                        tp = tc.Controls["studentPage" + ((i + 1) / PAGE_TOTAL)] as TabPage;
                    }

                }
                string message = "Successfully Saved Daily Five Records";
                string caption = "Save Completed";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                MessageBox.Show(message,caption,buttons);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to Save Data");
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        private void ControlDate_ValueChanged(object sender, EventArgs e)
        {
            SaveChanges();
            DateTimePicker dtp = (DateTimePicker)sender;
            curDate = dtp.Value;
            LoadDate();
        }

        void Page_Load(object sender, EventArgs e)
        {
            // Write status to file.
            DisplayActivities();
            LoadDate();
        }

        void ActivityBtn_Click(Object sender,
                           EventArgs e)
        {
            System.Data.OleDb.OleDbConnection conn;
            conn = new System.Data.OleDb.OleDbConnection();
            //conn.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;" +
            //@"Data source= C:\Users\Justin\Documents\DailyFive.mdb;";
            conn.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;" +
                @"Data source= DailyFive.mdb;";

            // When the button is clicked,
            // change the button text, and disable it.
            CheckBox clickedButton = (CheckBox)sender;
            GroupBox parentGroup = clickedButton.Parent as GroupBox;
            int activityCount = ds1.Tables["Activities"].Rows.Count;
            int buttonCount = parentGroup.Controls.Count;
            int invisibleCount = 0;
            int clickedCount = 0;
            List<int> chosenPeriods = new List<int>();
            foreach (var ctl in parentGroup.Controls)
            {
                if (ctl is CheckBox)
                {
                    CheckBox curButton = (CheckBox)ctl;
                    if (!curButton.Equals(clickedButton) )
                    {
                        if (curButton.Checked)
                        {
                            clickedCount++;
                            int curTag = 0;
                            try
                            {
                                curTag = Convert.ToInt32(curButton.Tag);
                            }
                            catch
                            {
                            }

                            chosenPeriods.Add(curTag);
                        }
                        if (!curButton.Visible)
                        {
                            invisibleCount++;
                        }
                    }
                }
            }
            if (clickedButton.Checked && clickedCount < 2)
            {
                bool found = false;
                for (int i = 0; i < 2 && !found; i++)
                {
                    if (chosenPeriods.Count == 0)
                    {
                        found = true;
                        clickedButton.BackgroundImage = GetBackgroundForClick("" + (i + 1));
                        clickedButton.Tag = "" +( i + 1);
                    }
                    else if (chosenPeriods.Min() == i + 1)
                    {
                        chosenPeriods.Remove(chosenPeriods.Min());
                    }
                    else
                    {
                        found = true;
                        clickedButton.BackgroundImage = GetBackgroundForClick("" + (i + 1));
                        clickedButton.Tag = "" + (i + 1);
                    }
                }

            }
            if (clickedCount > 1)
            {
                clickedButton.Checked = false;
            }
            //if we just checked the last box, make the rest of them visible
            else if ((invisibleCount >= activityCount - 1) && clickedButton.Checked)
            {
                foreach (var ctl in parentGroup.Controls)
                {
                    if (ctl is CheckBox)
                    {
                        CheckBox curButton = (CheckBox)ctl;
                        if (!curButton.Equals(clickedButton))
                        {
                            curButton.Visible = true;
                            curButton.Checked = false;
                        }
                    }
                }
            }
                //if we just unchecked a box, make sure we're not in a scenario where we need to make everything invisible
            else if (!clickedButton.Checked)
            {
                clickedButton.BackgroundImage = GetBackgroundForClick();
                try
                {
                    conn.Open();
                    OleDbDataAdapter da = new OleDbDataAdapter();
                    String earlierActivitiesString = "";
                    String includeEarlierActivities = "";
                    //if we are sunday we can stop here (truthfully could stop if not thursday...)
                    if ((curDate.DayOfWeek > DayOfWeek.Sunday))
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
                        String sql = "SELECT * FROM StudentActivityJoin WHERE Student = '" + parentGroup.Name + "' AND "
                            + earlierActivitiesString;
                        DataSet joinRecords = new DataSet();
                        da = new OleDbDataAdapter(sql, conn);
                        da.Fill(joinRecords, "dailyJoins");

                        DataRow[] previouslyUsedActivities =
                           joinRecords.Tables["dailyJoins"].Select("Activity = '" + clickedButton.Text +"'");

                        //If we just unchecked this box and it was the final one to be used, make sure
                        //nothing else is visible or checked
                        if (previouslyUsedActivities.Length < 1 &&
                                (joinRecords.Tables["dailyJoins"].Rows.Count >= activityCount - 1))
                        {
                            foreach (var ctl in parentGroup.Controls)
                            {
                                if (ctl is CheckBox)
                                {
                                    CheckBox curButton = (CheckBox)ctl;
                                    if (!curButton.Equals(clickedButton))
                                    {
                                        curButton.Visible = false;
                                        curButton.Checked = false;
                                        curButton.BackgroundImage = GetBackgroundForClick();
                                        curButton.Tag = "";
                                    }
                                }
                            }
                        }
                    }

                }
                catch
                {
                    MessageBox.Show("Failed in the clicked button");
                }
                finally
                {
                    conn.Close();
                }
            }

        }

        void SaveBtn_Click(Object sender,
           EventArgs e)
        {
            SaveChanges();
        }

        void NextBtn_Click(Object sender,
                   EventArgs e)
        {
            TabControl backgroundPage = this.Controls["backgroundPage"] as TabControl;
            backgroundPage.SelectedIndex = backgroundPage.SelectedIndex + 1;
        }

        void PrevBtn_Click(Object sender,
           EventArgs e)
        {
            TabControl backgroundPage = this.Controls["backgroundPage"] as TabControl;
            backgroundPage.SelectedIndex = backgroundPage.SelectedIndex - 1;
        }


    } //End of Class

    /// <summary>
    /// Selected Win AI Function Calls
    /// </summary>

    public class WinApi
    {
        [DllImport("user32.dll", EntryPoint = "GetSystemMetrics")]
        public static extern int GetSystemMetrics(int which);

        [DllImport("user32.dll")]
        public static extern void
            SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter,
                         int X, int Y, int width, int height, uint flags);

        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;
        private static IntPtr HWND_TOP = IntPtr.Zero;
        private const int SWP_SHOWWINDOW = 64; // 0x0040

        public static int ScreenX
        {
            get { return GetSystemMetrics(SM_CXSCREEN); }
        }

        public static int ScreenY
        {
            get { return GetSystemMetrics(SM_CYSCREEN); }
        }

        public static void SetWinFullScreen(IntPtr hwnd)
        {
            SetWindowPos(hwnd, HWND_TOP, 0, 0, ScreenX, ScreenY, SWP_SHOWWINDOW);
        }
    }

    /// <summary>
    /// Class used to preserve / restore state of the form
    /// </summary>
    public class FormState
    {
        private FormWindowState winState;
        private FormBorderStyle brdStyle;
        private bool topMost;
        private Rectangle bounds;

        private bool IsMaximized = false;

        public void Maximize(Form targetForm)
        {
            if (!IsMaximized)
            {
                IsMaximized = true;
                Save(targetForm);
                targetForm.WindowState = FormWindowState.Maximized;
                //targetForm.FormBorderStyle = FormBorderStyle.None;
                targetForm.TopMost = true;
                WinApi.SetWinFullScreen(targetForm.Handle);
            }
        }

        public void Save(Form targetForm)
        {
            winState = targetForm.WindowState;
            brdStyle = targetForm.FormBorderStyle;
            topMost = targetForm.TopMost;
            bounds = targetForm.Bounds;
        }

        public void Restore(Form targetForm)
        {
            targetForm.WindowState = winState;
            targetForm.FormBorderStyle = brdStyle;
            targetForm.TopMost = topMost;
            targetForm.Bounds = bounds;
            IsMaximized = false;
        }
    }
}



