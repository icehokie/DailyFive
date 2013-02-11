using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
//using System.Data.OleDb;
using System.Drawing.Drawing2D;
using System.Windows.Forms.Design;
using Owf.Controls;

namespace StudentActivityTracker
{

    public partial class ActivityInput : Form
    {
        static int MAX_COUNT = 2;
        static int PERIOD_COUNT = 2;

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        FormState fState;
        DateTime curDate = DateTime.Now;
        int PAGE_TOTAL = 5;
        List<Bitmap> bgList;
        DailyFiveDB db;
        int[,] activityPeriodMap;
        
        //Need to dynanimaclly determine this in future.
        int MAX_ACTIVITY_LEN = 15;

        public ActivityInput()
        {
            bgList = new List<Bitmap>();
            db = new DailyFiveDB(curDate);
            activityPeriodMap = new int[db.getActivityCount(), PERIOD_COUNT]; 
            InitializeComponent();


            //TODO: Probably need to figure out how to resize now that there is the capability
            fState = new FormState();
            //fState.Maximize(this);
            this.Text = "Daily Five";
            this.MinimizeBox = true;

        }

        /**
         * Creates the initial buttons and sets their data.
         */
        private void InitializeBoard()
        {
            int studentCount = db.getStudentCount();
            int activityCount = db.getActivityCount();

            TabControl bg = new TabControl();
            bg.Name = "backgroundPage";

            TabPage tp1 = new TabPage();
            tp1.Name = "studentPage0";
            tp1.TabIndex = 0;
            for (int i = 0; i < studentCount; i++)
            {
                //if we are at the maximum for this page, create a new one
                if (i > 0 && i % PAGE_TOTAL == 0)
                {
                    Button pageNext = new Button();
                    pageNext.Text = "Next";
                    pageNext.Click += new EventHandler(this.NextBtn_Click);
                    pageNext.Name = tp1.Name + "Next";
                    tp1.Controls.Add(pageNext);

                    bg.Controls.Add(tp1);

                    tp1 = new TabPage();
                    tp1.Name = "studentPage" + i / PAGE_TOTAL;
                    Button pagePrev = new Button();
                    pagePrev.Text = "Previous";
                    pagePrev.Name = tp1.Name + "Prev";
                    pagePrev.Click += new EventHandler(this.PrevBtn_Click);
                    tp1.Controls.Add(pagePrev);
                }

                GroupBox studentGroup = new GroupBox();
                studentGroup.Padding = DefaultPadding;
                int idNumber = db.getStudentIdForNumber(i);
                studentGroup.Name = "" + idNumber;

                GroupBox nameGroup = new GroupBox();
                nameGroup.Name = "nameGroup" + i;
                Label studentFirstName = new Label();
                studentFirstName.Name = "studentFirstName" + i;
                Label studentLastName = new Label();
                studentLastName.Name = "studentLastName" + i;

                studentFirstName.Text = db.getStudentFirstName(i);
                studentLastName.Text = db.getStudentLastName(i);

                studentFirstName.AutoSize = true;
                studentLastName.AutoSize = true;

                nameGroup.Controls.Add(studentFirstName);
                nameGroup.Controls.Add(studentLastName);

                studentGroup.Controls.Add(nameGroup);

                for (int j = 0; j < activityCount; j++)
                {
                    CheckBox newButton = new CheckBox();
                    newButton.Appearance = Appearance.Button;
                    newButton.Text = db.getActivityFromRow(j);
                    newButton.TextAlign = ContentAlignment.MiddleCenter;
                    newButton.Click += new EventHandler(this.ActivityBtn_Click);
                    newButton.Name = "student" + idNumber + "Activity" + j;
                    studentGroup.Controls.Add(newButton);
                }
                tp1.Controls.Add(studentGroup);
            }
            bg.TabPages.Add(tp1);
            CreateControlPage(ref bg);
            this.Controls.Add(bg);
        }

        /**
         * This draws the board and sets the location of all the controls.  Seperated out
         * from the Initialization to allow resize
         * 
         * NOTE: THIS SHOULD ONLY BE CALLED AFTER InitializeBoard
         */
        private void DrawBoard()
        {
            //This breaks if it's called if we aren't the active window....
            int windowWidth = Control.FromHandle(GetForegroundWindow()).Width;
            int windowHeight = Control.FromHandle(GetForegroundWindow()).Height;
            int vPadding = (int)(windowHeight / 60.0);
            int hPadding = (int)(windowWidth / 80.0);
            int hSize = (int)(windowWidth / 5.4);
            int vSize = (int)(windowHeight / 1.45);
            int studentCount = db.getStudentCount();
            int activityCount = db.getActivityCount();
            int buttonHeight = (int)(vSize - vSize / 4 - 5 * 1.0) / activityCount;
            Font buttonFont = new Font("Arial", buttonHeight / 3);
            Font navFont = new Font("Arial", buttonHeight / 6);
            TabControl bg = (TabControl)this.Controls.Find("backgroundPage", false)[0];

            //TODO::Figure out what it going on here....
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
            //ENDTODO

            float desiredBSize = buttonHeight - 5;
            float maxBFontHeight = (buttonFont.Size * desiredBSize) / sizeB.Height;
            float maxBFontWidth = (buttonFont.Size * (hSize - 15)) / sizeB.Width;
            float newBFontSize = maxBFontHeight < maxBFontWidth ? maxBFontHeight : maxBFontWidth;
            buttonFont = new Font("Arial", newBFontSize);
            navFont = new Font("Arial", newBFontSize);
            Point listOffset = new Point(0, 5);
            TabPage tp1 = (TabPage)bg.Controls.Find("studentPage0", false)[0];

            bg.Height = windowHeight;
            bg.Width = windowWidth;

            Point topLeft = new Point(4, 8);
            
            for (int i = 0; i < studentCount; i++)
            {
                //If we are at the end of the page get the next one
                if (i > 0 && i % PAGE_TOTAL == 0)
                {
                    Button pageNext = (Button)tp1.Controls.Find(tp1.Name + "Next", false)[0];
                    pageNext.Location = new Point(hSize * 4 + hSize / 2 + hPadding * 5, (int)(vSize + (windowHeight - vSize) / 2.5 + vPadding * 2));
                    pageNext.Width = (int)(hSize / 2.0);
                    pageNext.Height = (int)(buttonHeight - 5);
                    pageNext.Font = navFont;

                    tp1 = (TabPage)bg.Controls.Find("studentPage" + i / PAGE_TOTAL, false)[0]; ;
                    Button pagePrev = (Button)tp1.Controls.Find(tp1.Name+"Prev",false)[0];
                    pagePrev.Location = new Point(hSize * 4 + hPadding * 5, (int)(vSize + (windowHeight - vSize) / 2.5 + vPadding * 2));
                    pagePrev.Width = (int)(hSize / 2.0);
                    pagePrev.Height = (int)(buttonHeight - 5);
                    pagePrev.Font = navFont;
                }
                Point curLoc = new Point((int)((i % 5 + 1) * hPadding + (i % 5) * hSize),
                      (int)((windowHeight - vSize) / 2.5));

                int idNumber = db.getStudentIdForNumber(i);
                GroupBox studentGroup = (GroupBox)tp1.Controls.Find("" + idNumber, false)[0];
                studentGroup.Height = vSize;
                studentGroup.Width = hSize;
                studentGroup.Location = curLoc;

                GroupBox nameGroup = (GroupBox)studentGroup.Controls.Find("nameGroup" + i, false)[0];
                nameGroup.Width = hSize - 7;
                nameGroup.Height = vSize / 4;
                nameGroup.Location = topLeft;

                Label studentFirstName = (Label)nameGroup.Controls.Find("studentFirstName"+i, false)[0];
                Label studentLastName = (Label)nameGroup.Controls.Find("studentLastName"+i, false)[0];

                //TODO::If we can get the longest name beforehand we can calculate this once for all
                FormatStudentName(ref studentFirstName, ref studentLastName, nameGroup);

                CreateButtonBackgrounds(nameGroup.Width - 8, buttonHeight - 5);
                for (int j = 0; j < activityCount; j++)
                {
                    CheckBox newButton = (CheckBox)studentGroup.Controls.Find("student" + idNumber + "Activity" + j, false)[0];
                    newButton.Font = buttonFont;
                    newButton.Location = new Point(nameGroup.Left + 5, nameGroup.Bottom + 5 + j * buttonHeight);
                    newButton.Height = buttonHeight - 5;
                    newButton.Width = nameGroup.Width - 8;
                }
            }
            FormatControlPage(bg);
        }

        /**
         * Place the control page controls in the correct spot
         */
        private void FormatControlPage(TabControl bg)
        {
            TabPage controlPage = (TabPage)bg.Controls.Find("controlPage", false)[0];
            Button saveButton = (Button)controlPage.Controls.Find("controlSave", false)[0];
            DateTimePicker controlDate = (DateTimePicker)controlPage.Controls.Find("controlDate", false)[0];
            controlDate.Left = saveButton.Left;
            controlDate.Top = saveButton.Bottom + 5;
        }

        /**
         * Calculates the size the student name font can be based on the length and the size of the group.
         */
        private void FormatStudentName(ref Label studentFirstName, ref Label studentLastName, GroupBox nameGroup)
        {
            SizeF sizeFirst, sizeLast;
            using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(new Bitmap(1, 1)))
            {
                sizeFirst = graphics.MeasureString(studentFirstName.Text, studentFirstName.Font);
                sizeLast = graphics.MeasureString(studentLastName.Text, studentLastName.Font);
            }

            float desiredSize = nameGroup.Height / (float)(3.0);
            float maxFontHeight = (studentFirstName.Font.Size * desiredSize) / sizeFirst.Height;
            float maxFontWidth = sizeFirst.Width < sizeLast.Width ? (studentLastName.Font.Size * (nameGroup.Width - 5)) / sizeLast.Width
                : (studentFirstName.Font.Size * (nameGroup.Width - 5)) / sizeFirst.Width;
            float newFontSize = maxFontHeight < maxFontWidth ? maxFontHeight : maxFontWidth;

            Font nameFont = new Font("Arial", newFontSize);
            studentFirstName.Font = nameFont;
            studentLastName.Font = nameFont;

            using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(new Bitmap(1, 1)))
            {
                sizeFirst = graphics.MeasureString(studentFirstName.Text, studentFirstName.Font);
                sizeLast = graphics.MeasureString(studentLastName.Text, studentLastName.Font);
            }

            studentFirstName.Location = new Point((int)(nameGroup.Width - sizeFirst.Width) / 2, nameGroup.Height / 6);
            studentLastName.Location = new Point((int)(nameGroup.Width - sizeLast.Width) / 2, (int)(nameGroup.Height - sizeLast.Height - nameGroup.Height / 6));
        }

        /**
         * Creates the controls for the control page
         */
        private void CreateControlPage(ref TabControl bg)
        {
            TabPage controlPage = new TabPage();
            controlPage.Name = "controlPage";
            Button saveButton = new Button();
            saveButton.Name = "controlSave";
            saveButton.Text = "Save";
            controlPage.Controls.Add(saveButton);
            saveButton.Click += new EventHandler(this.SaveBtn_Click);

            DateTimePicker controlDate = new DateTimePicker();
            controlDate.Name = "controlDate";
            controlDate.Value = curDate;
            controlDate.Left = saveButton.Left;
            controlDate.Top = saveButton.Bottom + 5;
            controlPage.Controls.Add(controlDate);
            controlDate.ValueChanged += new EventHandler(this.ControlDate_ValueChanged);

            bg.TabPages.Add(controlPage);
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            base.OnPaint(e);
            
            Panel panel1 = (Panel)sender;
            ControlPaint.DrawBorder3D(e.Graphics, panel1.ClientRectangle, Border3DStyle.Raised, Border3DSide.All);
        }

        /**
         * Triggers when the form is resized to redraw the board
         */
        private void Form_Resize(object sender, System.EventArgs e)
        {
            DrawBoard();
        }


        /**
         *Drawing to create the button backgrounds when clicked...
         *
         * TODO::Can probably be cleaned up after activity number is added to db
         */
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


        /**
         * Method to determine which color the background should be 
         * based on whether it was the first or second clicked item
         */
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

        /**
         * Reset the activity -> period Map
         */
        void clearActivityPeriodMap()
        {
            int activityCount = db.getActivityCount();
            for (int i = 0; i < activityCount; i++)
            {
                for (int j = 0; j < PERIOD_COUNT; j++)
                {
                    activityPeriodMap[i, j] = 0;
                }
            }
        }


        /**
         * Set the activity visibility and color based on the joins
         * for the current date/week
         */
        void LoadDate()
        {
            clearActivityPeriodMap();
            int studentCount = db.getStudentCount();
            int activityCount = db.getActivityCount();

            //Loop through all the students
            for (int i = 0; i < studentCount; i++)
            {
                int idNumber = db.getStudentIdForNumber(i);
                //Load a student group to save time on the 'find's for the activities in the next loop
                GroupBox studentGroup = (GroupBox)this.Controls.Find("" + idNumber, true)[0];

                //Loop through each activity for this student
                for(int j = 0; j < activityCount; j++)
                {
                    CheckBox curActivity = (CheckBox)studentGroup.Controls.Find("student" + idNumber + "Activity" + j, false)[0];
                    String period = db.getCurrentStudentActivityPeriod(idNumber,db.getActivityKeyFromRow(j));
                    
                    //if the box actually is checked
                    if (period != "0")
                    {
                        curActivity.Tag = period;
                        curActivity.Checked = true;
                        activityPeriodMap[j,atoi(period) -1]++;
                    }
                    else
                    {
                        curActivity.Checked = false;
                    }
                    curActivity.BackgroundImage = GetBackgroundForClick(period);
                    
                    //if all the activities have already been used, need to make everything available
                    if (db.totalStudentActivitesForWeek(idNumber) >= activityCount)
                    {
                        curActivity.Visible = true;
                    }
                    //if all activites have not been used but this activity has, should not be selectable
                    else if (db.activityUsedEarlierInWeek(idNumber, db.getActivityKeyFromRow(j)))
                    {
                        curActivity.Visible = false;
                    }
                    //if it hasn't been used, make it visible
                    else
                    {
                        curActivity.Visible = true;
                    }
                }
            }
        }

        /**
         * Called when Form is closed, only purpose is to save changes to DB
         */
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveChanges();
            base.OnFormClosing(e);
        }

        /**
         * 
         */
        void SaveChanges()
        {
            int studentCount = db.getStudentCount();
            int activityCount = db.getActivityCount();
            
            //Clear data for today and we'll do clean inserts for the rest
            db.clearDailyData();

            //Loop through all the students
            for (int i = 0; i < studentCount; i++)
            {
                int idNumber  = db.getStudentIdForNumber(i);
                //Load a student group to save time on the 'find's for the activities in the next loop
                GroupBox studentGroup = (GroupBox)this.Controls.Find("" + idNumber, true)[0];

                //Loop through each activity for this student
                for (int j = 0; j < activityCount; j++)
                {
                    CheckBox curActivity = (CheckBox)studentGroup.Controls.Find("student" + idNumber + "Activity" + j, false)[0];
                    if (curActivity.Checked)
                    {
                        db.SetCheckedForStudent(idNumber, db.getActivityKeyFromRow(j), (String)curActivity.Tag);
                    }
                }
            }
            string message = "Successfully Saved Daily Five Records";
            string caption = "Save Completed";
            MessageBoxButtons buttons = MessageBoxButtons.OK;
            MessageBox.Show(message, caption, buttons);
        }

        /**Whenever the date field on the control page changes values, call
         * this method which saves the changes and loads a new date
         */
        private void ControlDate_ValueChanged(object sender, EventArgs e)
        {
            SaveChanges();
            DateTimePicker dtp = (DateTimePicker)sender;
            curDate = dtp.Value;
            LoadDate();
        }

        /**
         * Method called when the form loads where we initialize and
         * do an initial drawing
         */
        void Page_Load(object sender, EventArgs e)
        {
            InitializeBoard();
            DrawBoard();
            LoadDate();
        }

        /**
         * Handler for when any of the activity buttons are checekd
         */ 
        void ActivityBtn_Click(Object sender,
                           EventArgs e)
        {
            CheckBox clickedButton = (CheckBox)sender;
            //GroupBox parentGroup = clickedButton.Parent as GroupBox;
            int activityCount = db.getActivityCount();

            int clickedCount = 0;
            bool allOthersInvisible = true;
            List<int> chosenPeriods = new List<int>();
            populateOtherStudentActivityInformation(clickedButton, ref chosenPeriods,
                ref clickedCount, ref allOthersInvisible);

            bool isValidClick = false;
            String error = "";
            int newPeriod = -1;
            if (clickedButton.Checked)
            {
                newPeriod = getPotentialPeriod(ref clickedButton, ref clickedCount, ref chosenPeriods);
                isValidClick = validateClick(clickedButton, clickedCount, newPeriod, ref error);
            }
            //This logic needs to change to allow more than 2
            if (isValidClick)
            {
                checkButton(ref clickedButton, newPeriod);
                if (allOthersInvisible && clickedButton.Checked)
                {
                    checkLastBoxEarly(ref clickedButton);
                }
            }

            else if (clickedButton.Checked)
            {
                //if we have previously reached the 'max' selected for the day, cannot check this
                clickedButton.Checked = false;
            }
            else if (!clickedButton.Checked)
            {
                uncheckButton(ref clickedButton);
            }
        }

        private bool validateClick(CheckBox clickedButton, int otherButtonsClicked, int period, ref String error)
        {
            if (otherButtonsClicked >= MAX_COUNT)
            {
                error = "You are at the max amount for today, please unselect before trying again";
                return false;
            }
            int activityNumber = db.getActivityKey(clickedButton.Text);
            if (shouldPerformActivityCountValidation() && db.activityHasPeriodMax(activityNumber))
            {
                if(db.getActivityPeriodMax(activityNumber) <= getActivtyCountForPeriod(db.getActivityRowFromKey(activityNumber), period)) 
                {
                    error = "The maximum for this activity has been reached, please select a different one.";
                    return false;
                }
            }
            return true;
        }

        /**
         * Uses the activity period map to return the number of times that activity has been used in that
         * period today.
         */
        private int getActivtyCountForPeriod(int activityNumber, int period)
        {
            return activityPeriodMap[activityNumber, period-1];
        }

        /**
         * Expandable method for turning off the validation capability
         */ 
        private bool shouldPerformActivityCountValidation()
        {
            //Don't perform validation if in the past.
            if (curDate.Date < DateTime.Now.Date)
            {
                return false;
            }
            return true;
        }

        /**
         * Cleaner way to convert strings to integers.  Will crash and burn if it's not valid
         * but makes the calls in the actual methods look much better.
         */ 
        public int atoi(String integer)
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

        /**
         * Whenever we uncheck a button, we need to verify that we don't need to make the other activities
         * invisible and unchecked.
         */ 
        private void uncheckButton(ref CheckBox clickedButton)
        {
            clickedButton.BackgroundImage = GetBackgroundForClick();
            GroupBox parentGroup = clickedButton.Parent as GroupBox;
            int studentId = atoi(parentGroup.Name);
            int activityNumber = db.getActivityKey(clickedButton.Name);

            int activityCount = db.getActivityCount();
            int prevActivites = db.totalStudentActivitesForWeek(studentId);

            if (prevActivites == activityCount - 1 && !db.activityUsedEarlierInWeek(studentId, activityNumber))
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

        /**
         * Check the box and format it correctly
         */
        private void checkButton(ref CheckBox clickedButton, int period)
        {
            clickedButton.BackgroundImage = GetBackgroundForClick("" + period);
            clickedButton.Tag = "" + (period);
            activityPeriodMap[db.getActivityRow(clickedButton.Text), period - 1]++;
        }

        /**
         * return the potential period of the clicked button based on other information
         */ 
        private int getPotentialPeriod(ref CheckBox clickedButton, ref int clickedCount, ref List<int> chosenPeriods)
        {
            bool found = false;
            //Figure out what period we are by counting and finding the first available number
            for (int i = 0; i <= clickedCount && !found; i++)
            {
                //if other periods are still checked and the lowest number still isn't available
                if (chosenPeriods.Count > 0 && chosenPeriods.Min() == i + 1)
                {
                    chosenPeriods.Remove(chosenPeriods.Min());
                }
                else
                {
                    return i+1;
                }
            }
            return -1;

        }

        /**
         * Perform necessary updates when the last box is checked.
         */ 
        private void checkLastBoxEarly(ref CheckBox lastChecked)
        {
            GroupBox parentGroup = lastChecked.Parent as GroupBox;
            foreach (var ctl in parentGroup.Controls)
            {
                if (ctl is CheckBox)
                {
                    CheckBox curButton = (CheckBox)ctl;
                    //We already set the last checked buttons style elsewhere
                    curButton.Visible = true;
                    curButton.Checked = false;
                }
            }

            //This was reset in the loop so make sure we set this back to checked
            lastChecked.Checked = true;
        }

        /**
         * Get the parent group of the button and check the checked state of all the other activities
         */ 
        private void populateOtherStudentActivityInformation(CheckBox clickedButton, ref List<int> chosenPeriods,
            ref int clickedCount, ref bool allOthersInvisible)
        {
            GroupBox parentGroup = clickedButton.Parent as GroupBox;
            foreach (var ctl in parentGroup.Controls)
            {
                if (ctl is CheckBox)
                {
                    CheckBox curButton = (CheckBox)ctl;

                    //We don't want to include ourself in these counts
                    if (!curButton.Equals(clickedButton))
                    {
                        if (curButton.Checked)
                        {
                            clickedCount++;
                            int curTag = atoi((String)curButton.Tag);
                            chosenPeriods.Add(curTag);
                        }
                        
                        //If we haven't found a button that's visible...
                        if (allOthersInvisible && curButton.Visible)
                        {
                            allOthersInvisible = false;
                        }
                    }
                }
            }
        }

        /**
         * Simple handler that just saves whenever the save button is clicked
         * on the control page
         */
        void SaveBtn_Click(Object sender,
           EventArgs e)
        {
            SaveChanges();
        }

        /**
         *Simple handler that goes to the next page whenever the next button is clicked
         *Works on the assumption there isn't going to be a 'next' button on a page that doesn't
         *actually have a 'next' page
         */
        void NextBtn_Click(Object sender,
                   EventArgs e)
        {
            TabControl backgroundPage = this.Controls["backgroundPage"] as TabControl;
            backgroundPage.SelectedIndex = backgroundPage.SelectedIndex + 1;
        }

        /**
         * *Simple handler that goes to the prev page whenever the prev button is clicked
         * Works on the assumption there isn't going to be a 'prev' button on a page that doesn't
         * actually have a 'prev' page
         */
        void PrevBtn_Click(Object sender,
           EventArgs e)
        {
            TabControl backgroundPage = this.Controls["backgroundPage"] as TabControl;
            backgroundPage.SelectedIndex = backgroundPage.SelectedIndex - 1;
        }


    } //End of Class


}



