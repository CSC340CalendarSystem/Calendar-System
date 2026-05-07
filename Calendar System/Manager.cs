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
    public partial class Manager : Form
    {
        int ID;
        Database db;

        int chosenDay;
        int chosenMonth = 3;
        int chosenYear = 2026;
        int chosenHour;
        int chosenMinute;

        List<int> partID = null;

        public Manager(int EmployeeID, Database db)
        {
            this.ID = EmployeeID;
            this.db = db;

            InitializeComponent();
            newEventPanel.Hide();
            participantsPanel.Hide();
        }

        private void tableLayoutPanel7_Paint(object sender, PaintEventArgs e)
        {

        }

        //  -  -  Selecting Date  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -

        private void button_Click(object sender, EventArgs e) {
            /*  Method: button_Click        
             *      Generic method for all the days in the Manager Calendar. 
             *      Grabs the day using the .Text parameter of the pressed Button.
             *  Author: Samuel Griggs
             */
            Button button = (Button)sender;

            chosenDay = Int32.Parse(button.Text);

            selectedDateL.Text = "3/" + chosenDay + "/26";
        }

        //  -  -  Creating Event  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -

        private void addEventB_Click(object sender, EventArgs e) {
            Event ev = new Event();
            newEventPanel.Show();
        }

        private void finishEventButton_Click(object sender, EventArgs e) {
            try {
                /*  Method: finishEventButton_Click
                 *      Takes input from a Manager for the creation of an event.
                 *      Creates an event in the database using the acquired values.
                 *  Author: Samuel Griggs
                 */

                if (partID==null && !eventNameBox.Text.Equals("") && !startTimeBox.Text.Equals("") && !endTimeBox.Text.Equals("") && Int32.Parse(endTimeBox.Text)>Int32.Parse(startTimeBox.Text)) {
                    // Creation of Regular event
                    string name = eventNameBox.Text;
                    string desc = descriptionBox.Text;

                    chosenHour = Int32.Parse(startTimeBox.Text) / 100;
                    chosenMinute = Int32.Parse(startTimeBox.Text) % 100;
                    DateTime startTime = new DateTime(chosenYear, chosenMonth, chosenDay, chosenHour, chosenMinute, 0);

                    chosenHour = Int32.Parse(endTimeBox.Text) / 100;
                    chosenMinute = Int32.Parse(endTimeBox.Text) % 100;
                    DateTime endTime = new DateTime(chosenYear, chosenMonth, chosenDay, chosenHour, chosenMinute, 0);

                    db.AddEvent(ID, name, startTime, endTime, desc);

                    newEventPanel.Hide();
                }
                else if (partID != null && !eventNameBox.Text.Equals("") && !startTimeBox.Text.Equals("") && !endTimeBox.Text.Equals("") && Int32.Parse(endTimeBox.Text) > Int32.Parse(startTimeBox.Text)) {
                    // Creation of meeting and event for it
                    string name = eventNameBox.Text;
                    string desc = descriptionBox.Text;

                    chosenHour = Int32.Parse(startTimeBox.Text) / 100;
                    chosenMinute = Int32.Parse(startTimeBox.Text) % 100;
                    DateTime startTime = new DateTime(chosenYear, chosenMonth, chosenDay, chosenHour, chosenMinute, 0);

                    chosenHour = Int32.Parse(endTimeBox.Text) / 100;
                    chosenMinute = Int32.Parse(endTimeBox.Text) % 100;
                    DateTime endTime = new DateTime(chosenYear, chosenMonth, chosenDay, chosenHour, chosenMinute, 0);

                    Event ev = new Event();
                    ev.Title = name;
                    ev.StartTime = startTime;
                    ev.EndTime = endTime;
                    ev.Description = desc;

                    db.AddEvent(ID, name, startTime, endTime, desc);

                    partID.Add(ID);
                    // Create the meeting event
                    db.createMeeting(ev, partID);

                    newEventPanel.Hide();
                    participantsPanel.Hide();
                }
                else
                    MessageBox.Show("Name and times are required. End time must be later than start.");
            }
            catch (FormatException) {
                MessageBox.Show("One or both times are Not a Number");
            }
            catch (Exception ex) {

                throw;
            }
        }

        private void cancelNewEventButton_Click(object sender, EventArgs e) {
            /*  Method: cancelNewEventButton_Click
             *      Function that simply cancels the creation of current event
             *  Author: Samuel Griggs
             */
            eventNameBox.Text = "New Meeting";
            startTimeBox.Text = "";
            endTimeBox.Text = "";
            descriptionBox.Text = "";
            partID = null;

            newEventPanel.Hide();
            participantsPanel.Hide();
        }

        //  -  -  Creating Meeting  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -  -

        private void meeting1Button_Click(object sender, EventArgs e) {
            /*  Method: meeting1Button_Click
             *      Occurs when a manager clicks the 'Create Meeting' button. Allows
             *      for the creation of a meeting even, which is essentially just an event but with 
             *      multiple participants
             *  Author: Samuel Griggs
             */
            partButton1.Hide();
            partButton2.Hide();
            partButton3.Hide();
            partButton4.Hide();
            partButton5.Hide();
            partButton6.Hide();
            partButton7.Hide();
            partButton8.Hide();

            var participants = db.getValidParticipants();
            //List<string> nList = participants.nameList;
            //List<int> IDList = participants.IDList;
            partID = new List<int>();

            switch(participants.nameList.Count) {
                case 8:
                    partButton8.Text = participants.nameList[7] + "|--|" + participants.IDList[7];
                    partButton8.Show();
                    goto case 7;
                case 7:
                    partButton7.Text = participants.nameList[6] + "|--|" + participants.IDList[6];
                    partButton7.Show();
                    goto case 6;
                case 6:
                    partButton6.Text = participants.nameList[5] + "|--|" + participants.IDList[5];
                    partButton6.Show();
                    goto case 5;
                case 5:
                    partButton5.Text = participants.nameList[4] + "|--|" + participants.IDList[4];
                    partButton5.Show();
                    goto case 4;
                case 4:
                    partButton4.Text = participants.nameList[3] + "|--|" + participants.IDList[3];
                    partButton4.Show();
                    goto case 3;
                case 3:
                    partButton3.Text = participants.nameList[2] + "|--|" + participants.IDList[2];
                    partButton3.Show();
                    goto case 2;
                case 2:
                    partButton2.Text = participants.nameList[1] + "|--|" + participants.IDList[1];
                    partButton2.Show();
                    goto case 1;
                case 1:
                    partButton1.Text = participants.nameList[0] + "|--|" + participants.IDList[0];
                    partButton1.Show();
                    break;
                case 0:
                    MessageBox.Show("No valid Participants for meeting");
                    break;
                default:
                    // Should only occur if greater than 8 Participants
                    goto case 8;
            }


            newEventPanel.Show();
            participantsPanel.Show();

        }

        private void partButton_Click(object sender, EventArgs e) {
            /*  Method: partButton_Click
             *  Generic function for buttons that contain participants
             *  Author: Samuel Griggs
             */
            Button button = (Button)sender;

            int num = Int32.Parse(button.Text.Substring(button.Text.IndexOf("|--|")+4));
            if(!partID.Contains(num)) {
                partID.Add(num);
            }
        }
    }
}
