using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Calendar_System
{
    public partial class Employee : Form
    {
        //calendar buttons
        private List<Button> calendarButtons = new List<Button>();
        //current date
        private DateTime currentDateTime = DateTime.Now;
        //user selected date to view meetings
        private DateTime selectedDate;
        private List<Event> allEvents = new List<Event>();
        //current user ID
        private int userID;
        //database reference
        private Database db;
        public Employee(int userID, Database db)
        {
            InitializeComponent();
            //store userID and database reference for later use
            this.userID = userID;
            this.db = db;

            //update selected date label to current date
            selectedDateL.Text = currentDateTime.ToString("M/d/yyyy");

            //organize group boxes
            selectedMeetingBox.Hide();
            addEventBox.Hide();
            editEventBox.Hide();
            selectedDayBox.Show();

            //create calendar buttons
            BuildCalendar();

            //update mainmenu
            this.Load += MainMenu_Load;
        }
        //load meetings for current day
        //author: Harrison Mesplay
        private void MainMenu_Load(object sender, EventArgs e)
        {
            selectedDate = currentDateTime.Date;

            allEvents = db.GetEventsForUserInMonth(userID, currentDateTime.Month, currentDateTime.Year);

            UpdateCalendar();
            LoadMeetingsForSelectedDate();
        }
        //create calendar buttons
        //author: Harrison Mesplay
        private void BuildCalendar()
        {
            calendarButtons.Clear();

            // loop through rows and columns manually
            for (int row = 0; row < calendarPanel.RowCount; row++)
            {
                for (int col = 0; col < calendarPanel.ColumnCount; col++)
                {
                    Control c = calendarPanel.GetControlFromPosition(col, row);

                    if (c is Button)
                    {
                        calendarButtons.Add((Button)c);
                    }
                }
            }

            DateTime firstDay = new DateTime(currentDateTime.Year, currentDateTime.Month, 1);

            // go backwards until Sunday
            DateTime startDate = firstDay;

            while (startDate.DayOfWeek != DayOfWeek.Sunday)
            {
                startDate = startDate.AddDays(-1);
            }

            for (int i = 0; i < calendarButtons.Count; i++)
            {
                DateTime thisDate = startDate.AddDays(i);

                Button btn = calendarButtons[i];

                btn.Text = thisDate.Day.ToString();
                btn.Enabled = true;

                //store full date in tag
                btn.Tag = thisDate;
                //gray btn not in this month
                if (thisDate.Month != currentDateTime.Month)
                {
                    btn.ForeColor = Color.LightGray;
                }
                else
                {
                    btn.ForeColor = Color.Black;
                }
            }

            monthL.Text = currentDateTime.ToString("MMMM yyyy");
        }
        //update calendar button colors based on rules
        //author: Harrison Mesplay
        private void UpdateCalendar()
        {
            foreach (var btn in calendarButtons)
            {
                if (btn.Text == "")
                {
                    btn.BackColor = Color.White;
                    continue;
                }

                DateTime thisDate = (DateTime)btn.Tag;

                //color rules
                if (thisDate.Date == selectedDate.Date)
                {
                    btn.BackColor = Color.LightGray;
                    btn.ForeColor = Color.Black;
                }
                else if (HasEventOnDate(thisDate))
                {
                    btn.BackColor = Color.LightBlue;
                    //gray btn not in this month
                    if (thisDate.Month != currentDateTime.Month)
                    {
                        btn.ForeColor = Color.LightGray;
                    }
                    else
                    {
                        btn.ForeColor = Color.Black;
                    }
                }
                else
                {
                    btn.BackColor = Color.White;
                    //gray btn not in this month
                    if (thisDate.Month != currentDateTime.Month)
                    {
                        btn.ForeColor = Color.LightGray;
                    }
                    else
                    {
                        btn.ForeColor = Color.Black;
                    }
                }
            }
        }
        //check if there are meetings on a given date
        //author: Harrison Mesplay
        private bool HasEventOnDate(DateTime date)
        {
            foreach (Event e in allEvents)
            {
                if (e.StartTime.Date == date.Date)
                {
                    return true;
                }
            }
            return false;
        }
        //load meetings for selected date when date button is clicked
        //author: Harrison Mesplay
        private void DateButton_Click(object sender, EventArgs e)
        {
            //button ref
            Button clicked = sender as Button;
            //if empty, return
            if (clicked.Text == "")
            {
                return;
            }

            DateTime selected = (DateTime)clicked.Tag;
            selectedDate = selected;

            //update selected date label
            selectedDateL.Text = selectedDate.ToString("M/d/yyyy");
            selectedDateL2.Text = selectedDate.ToString("M/d/yyyy");
            editingDateL.Text = selectedDate.ToString("M/d/yyyy");
            addingDateL.Text = selectedDate.ToString("M/d/yyyy");

            //update
            UpdateCalendar();
            LoadMeetingsForSelectedDate();

            //show/hide boxes
            selectedMeetingBox.Hide();
            addEventBox.Hide(); 
            editEventBox.Hide();
            selectedDayBox.Show();
        }
        //load meetings for selected date and create buttons for each meeting
        //author: Harrison Mesplay
        private void LoadMeetingsForSelectedDate()
        {
            //find meetings on selected date
            List<Event> events = db.GetEventsForUserOnDate(userID, selectedDate);

            //clear previous buttons
            meetingListBox.Controls.Clear();

            foreach (Event e in events)
            {
                //create button for each meeting
                Button btn = new Button();
                //title
                btn.Text = $"{e.Title}\n{e.StartTime:h:mm tt} - {e.EndTime:h:mm tt}";

                //position, style, font, etc.
                btn.UseVisualStyleBackColor = true;
                btn.Height = 60;
                btn.Dock = DockStyle.Top;
                btn.Font = new Font("Microsoft Sans Serif", 14.25F);
                btn.TextAlign = ContentAlignment.MiddleLeft;
                btn.Margin = new Padding(3, 3, 3, 3);

                //store event in tag
                btn.Tag = e;

                //add click event to load meeting details
                btn.Click += meetingButton_Click;

                //add button to panel
                meetingListBox.Controls.Add(btn);
            }
        }
        //show meeting details when meeting button is clicked
        //author: Harrison Mesplay
        private void meetingButton_Click(object sender, EventArgs e)
        {
            Button clicked = sender as Button;
            Event selectedEvent = clicked.Tag as Event;
            showMeetingsDetails(selectedEvent);
        }
        //show meeting details in selectedMeetingBox
        //author: Harrison Mesplay
        private void showMeetingsDetails(Event e)
        {
            //switch panels
            selectedDayBox.Hide();
            addEventBox.Hide();
            editEventBox.Hide();
            selectedMeetingBox.Show();

            //update labels with meeting details
            TitleL2.Text = e.Title;
            selectedDateL2.Text = e.StartTime.ToString("M/d/yyyy");
            startTimeD.Text = e.StartTime.ToString("hh:mm tt");
            endTimeD.Text = e.EndTime.ToString("hh:mm tt");
            DescriptionD.Text = e.Description;

            //part added by Alex Owens:
            //only show Edit/Delete buttons if the user owns this event
            bool userOwnsEvent = db.IsEventOwnedByUser(e.EventID, userID);
            EditEventB.Visible  = userOwnsEvent;
            DeleteEventB.Visible = userOwnsEvent;
        }
        //sends user to add event panel
        private void addEventB_Click(object sender, EventArgs e)
        {
            selectedDayBox.Hide();
            selectedMeetingBox.Hide();
            editEventBox.Hide();
            addEventBox.Show();
        }
        //user clicks confirm after filling out add event form
        private void saveEventBtn_Click(object sender, EventArgs e)
        {
            // get values from inputs
            string title = AddTitleText.Text;
            string description = AddDescText.Text;
            //time
            DateTime startInput = DateTime.Parse(AddStartText.Text);
            DateTime endInput = DateTime.Parse(AddEndText.Text);

            DateTime startTime = selectedDate.Date.Add(startInput.TimeOfDay);
            DateTime endTime = selectedDate.Date.Add(endInput.TimeOfDay);

            //title validation
            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Title is required.");
                return;
            }
            //time validation
            if (endTime <= startTime)
            {
                MessageBox.Show("End time must be after start time.");
                return;
            }
            //can schedule (no time conflicts)
            //check for time conflicts
            List<Event> events = db.GetEventsForUserOnDate(userID, selectedDate);

            foreach (Event ev in events)
            {
                if (startTime < ev.EndTime && endTime > ev.StartTime)
                {
                    MessageBox.Show("Time conflict with another event.");
                    return;
                }
            }
            //call database method to add event
            db.AddEvent(userID, title, startTime, endTime, description);

            //refresh UI
            UpdateCalendar();
            LoadMeetingsForSelectedDate();

            //clear textfields
            AddTitleText.Text = "";
            AddDescText.Text = "";
            AddStartText.Text = "";
            AddEndText.Text = "";

            //switch back to day view
            addEventBox.Hide();
            selectedDayBox.Show();
        }


        //user clicks the Edit Event button while viewing a meeting's details
        //also pre fills the edit form with the current event's data
        //author: Alex Owens
        private void editEventB_Click(object sender, EventArgs e)
        {
             if (currentEvent == null) return;

            //(users may not edit events they did not create)
            if (!db.IsEventOwnedByUser(currentEvent.EventID, userID))
            {
                MessageBox.Show("You cannot edit an event that was assigned to you by a manager.");
                return;
            }

            //pre-fill the edit fields with the existing event data
            textBox8.Text = currentEvent.Title;
            textBox7.Text = currentEvent.Description;
            textBox6.Text = currentEvent.StartTime.ToString("h:mm tt");
            textBox5.Text = currentEvent.EndTime.ToString("h:mm tt");

            //for updating the date label
            editingDateL.Text = currentEvent.StartTime.ToString("M/d/yyyy");

            //for witching to the edit panel
            selectedMeetingBox.Hide();
            selectedDayBox.Hide();
            addEventBox.Hide();
            editEventBox.Show();
        }
        //when user clicks Confirm on the edit form, validate and save changes
        //author: Alex Owens
        private void saveEditBtn_Click(object sender, EventArgs e)
        {
            if (currentEvent == null) return;

            string title       = textBox8.Text.Trim();
            string description = textBox7.Text.Trim();

            //parsing start/end times: keep the original date
            DateTime startInput, endInput;
            if (!DateTime.TryParse(textBox6.Text.Trim(), out startInput))
            {
                MessageBox.Show("Invalid start time. Please use a format like 9:00 AM.");
                return;
            }
            if (!DateTime.TryParse(textBox5.Text.Trim(), out endInput))
            {
                MessageBox.Show("Invalid end time. Please use a format like 10:00 AM.");
                return;
            }

            DateTime startTime = currentEvent.StartTime.Date.Add(startInput.TimeOfDay);
            DateTime endTime   = currentEvent.StartTime.Date.Add(endInput.TimeOfDay);

            //for validation
            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Title is required.");
                return;
            }
            if (endTime <= startTime)
            {
                MessageBox.Show("End time must be after start time.");
                return;
            }

            //for checking for time conflicts with other events on the same day
            List<Event> events = db.GetEventsForUserOnDate(userID, selectedDate);
            foreach (Event ev in events)
            {
                //event itself being edited doesnt count in this check
                if (ev.EventID == currentEvent.EventID) continue;

                if (startTime < ev.EndTime && endTime > ev.StartTime)
                {
                    MessageBox.Show("Time conflict with another event.");
                    return;
                }
            }

            //saving to database
            db.UpdateEvent(currentEvent.EventID, title, startTime, endTime, description);

            //events cache refreshed
            allEvents = db.GetEventsForUserInMonth(userID, currentDateTime.Month, currentDateTime.Year);

            //currentEvent changed to reflect new values
            currentEvent.Title       = title;
            currentEvent.Description = description;
            currentEvent.StartTime   = startTime;
            currentEvent.EndTime     = endTime;

            //ui refresh
            UpdateCalendar();
            LoadMeetingsForSelectedDate();

            MessageBox.Show("Event updated successfully");

            //lastly, return to the day view
            editEventBox.Hide();
            selectedDayBox.Show();
        }
        //when User clicks Cancel on the edit form, go back to day view without saving
        //author: Alex Owens
        private void editCancel_Click(object sender, EventArgs e)
        {
            editEventBox.Hide();
            selectedDayBox.Show();
        }
        //user clicks the Delete Event button while viewing a meeting's details
        //author: Alex Owens
        private void deleteEventB_Click(object sender, EventArgs e)
        {
            if (currentEvent == null) return;

            //(users may not delete events they did not create)
            if (!db.IsEventOwnedByUser(currentEvent.EventID, userID))
            {
                MessageBox.Show("You cannot delete an event that was assigned to you by a manager.");
                return;
            }

            DialogResult result = MessageBox.Show(
                $"Are you sure you want to delete \"{currentEvent.Title}\"?",
                "Confirm Delete",
                MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                db.DeleteEvent(currentEvent.EventID);

                //refresh events cache
                allEvents = db.GetEventsForUserInMonth(userID, currentDateTime.Month, currentDateTime.Year);

                //refresh UI
                UpdateCalendar();
                LoadMeetingsForSelectedDate();

                MessageBox.Show("Event deleted successfully.");

                //lastly, return to the day view
                selectedMeetingBox.Hide();
                selectedDayBox.Show();

                currentEvent = null;
            }
        }
        //user clicks previous month button
        //author: Harrison Mesplay
        private void prevMonth_Click(object sender, EventArgs e)
        {
            currentDateTime = currentDateTime.AddMonths(-1);

            //update events for new month
            allEvents = db.GetEventsForUserInMonth(userID, currentDateTime.Month, currentDateTime.Year);

            //set selected date to first of month to avoid issues with months that have less days
            selectedDate = new DateTime(currentDateTime.Year, currentDateTime.Month, 1);

            //rebuild calendar for new month
            BuildCalendar();
            UpdateCalendar();
            LoadMeetingsForSelectedDate();
        }
        //user clicks next month button
        //author: Harrison Mesplay
        private void nextMonth_Click(object sender, EventArgs e)
        {
            currentDateTime = currentDateTime.AddMonths(1);

            //update events for new month
            allEvents = db.GetEventsForUserInMonth(userID, currentDateTime.Month, currentDateTime.Year);

            //set selected date to first of month to avoid issues with months that have less days
            selectedDate = new DateTime(currentDateTime.Year, currentDateTime.Month, 1);

            //rebuild calendar for new month
            BuildCalendar();
            UpdateCalendar();
            LoadMeetingsForSelectedDate();
        }
        //user clicks logout button
        //author: Harrison Mesplay
        private void logoutButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to logout?", "Confirm", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                //delete meeting code here
                MessageBox.Show("Logout Sucessful!");
                this.Close();
                new Login().Show();
            }
        }
        //user clicks cancel button on add event form
        //author: Harrison Mesplay
        private void addCancel_Click(object sender, EventArgs e)
        {
            //switch back to day view
            addEventBox.Hide();
            selectedDayBox.Show();
        }
    }
}
