using MySql.Data.MySqlClient;
using Org.BouncyCastle.Tls;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Calendar_System
{
    public class Database
    {
        private String connectionString = "server=csitmariadb.eku.edu;user=student;password=Maroon@21?;database=csc340_db;";
        //test connection to database
        //author: Harrison Mesplay
        public bool TestConnection()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    conn.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            
        }
        //login function that checks the database for the username and password and returns a tuple with the success of the login, if the user is a manager, and the employee ID
        //author: Harrison Mesplay
        public (bool success, bool isManager, int userID) Login(string username, string password)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT userID, isManager FROM 340_calendar_users WHERE username = @user AND password = @pass";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@user", username);
                    cmd.Parameters.AddWithValue("@pass", password);
                    MySqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        int id = reader.GetInt32("userID");
                        bool isManager = reader.GetBoolean("isManager");

                        return (true, isManager, id);
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return (false, false, -1);
        }
        //gets all events for a user on a specific date, used in the calendar form when a user clicks on a date to show them all events they have on that date
        //author: Harrison Mesplay
        public List<Event> GetEventsForUserOnDate(int userID, DateTime date)
        {
            List<Event> events = new List<Event>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"SELECT e.*
                             FROM `340_calendar_event` e
                             JOIN `340_calendar_event_participants` ep
                             ON e.eventID = ep.eventID
                             WHERE ep.userID = @userID
                             AND DATE(e.startTime) = @date";

                    MySqlCommand cmd = new MySqlCommand(query, conn);

                    cmd.Parameters.AddWithValue("@userID", userID);
                    cmd.Parameters.AddWithValue("@date", date.Date);

                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        Event e = new Event
                        {
                            EventID = reader.GetInt32("eventID"),
                            Title = reader.GetString("eventTitle"),
                            StartTime = reader.GetDateTime("startTime"),
                            EndTime = reader.GetDateTime("endTime"),
                            Description = reader.GetString("description")
                        };

                        events.Add(e);
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading events: " + ex.Message);
            }

            return events;
        }
        //checks if the user has any events on the selected date, returns true if they do and false if they don't
        //was suppose to be user in the calendar form but made the program super laggy having to check the database for every day on the calendar,
        //so instead we will just check if the user has any events on the selected date when they click on a date in the calendar
        //author: Harrison Mesplay
        public bool HasEventOnDate(int userID, DateTime date)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"SELECT COUNT(*)
                             FROM `340_calendar_event` e
                             JOIN `340_calendar_event_participants` ep
                             ON e.eventID = ep.eventID
                             WHERE ep.userID = @userID
                             AND DATE(e.startTime) = @date";

                    MySqlCommand cmd = new MySqlCommand(query, conn);

                    cmd.Parameters.AddWithValue("@userID", userID);
                    cmd.Parameters.AddWithValue("@date", date.Date);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());

                    conn.Close();
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error checking events: " + ex.Message);
                return false;
            }
        }
        //gets all events for a user in a specific month, used in the calendar form to show the user all events they have in a month when they click on the month name
        //author: Harrison Mesplay
        public List<Event> GetEventsForUserInMonth(int userID, int month, int year)
        {
            List<Event> events = new List<Event>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"SELECT e.*
                             FROM `340_calendar_event` e
                             JOIN `340_calendar_event_participants` ep
                             ON e.eventID = ep.eventID
                             WHERE ep.userID = @userID
                             AND MONTH(e.startTime) = @month
                             AND YEAR(e.startTime) = @year";

                    MySqlCommand cmd = new MySqlCommand(query, conn);

                    cmd.Parameters.AddWithValue("@userID", userID);
                    cmd.Parameters.AddWithValue("@month", month);
                    cmd.Parameters.AddWithValue("@year", year);

                    MySqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        Event e = new Event
                        {
                            EventID = reader.GetInt32("eventID"),
                            Title = reader.GetString("eventTitle"),
                            StartTime = reader.GetDateTime("startTime"),
                            EndTime = reader.GetDateTime("endTime"),
                            Description = reader.GetString("description")
                        };

                        events.Add(e);
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading monthly events: " + ex.Message);
            }

            return events;
        }
        //adds an event to the database and adds the current user as a participant in the event, used in the create event form when a user creates a new event
        //author: Harrison Mesplay
        public void AddEvent(int userID, string title, DateTime startTime, DateTime endTime, string description)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    //insert event
                    string insertEventQuery = @"INSERT INTO `340_calendar_event`
                                        (eventTitle, startTime, endTime, description)
                                        VALUES (@title, @start, @end, @desc)";

                    MySqlCommand cmd = new MySqlCommand(insertEventQuery, conn);

                    cmd.Parameters.AddWithValue("@title", title);
                    cmd.Parameters.AddWithValue("@start", startTime);
                    cmd.Parameters.AddWithValue("@end", endTime);
                    cmd.Parameters.AddWithValue("@desc", description);

                    cmd.ExecuteNonQuery();

                    //get new event ID
                    int eventID = (int)cmd.LastInsertedId;

                    //insert participant (current user)
                    string insertParticipantQuery = @"INSERT INTO `340_calendar_event_participants`
                                              (eventID, userID)
                                              VALUES (@eventID, @userID)";

                    MySqlCommand cmd2 = new MySqlCommand(insertParticipantQuery, conn);

                    cmd2.Parameters.AddWithValue("@eventID", eventID);
                    cmd2.Parameters.AddWithValue("@userID", userID);

                    cmd2.ExecuteNonQuery();

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving event: " + ex.Message);
            }
        }

        public (List<string> nameList,List<int> IDList) getValidParticipants() {
            /*  Method: getValidParticipants
             *      Helper method for meeting1Button_Click. Returns a list of Employees whom
             *      the manager may select as participants for the metings.
             *  Author: Samuel Griggs
             */

            List<string> nList = new List<string>();
            List<int> IDList = new List<int>();

            MySqlConnection conn = new MySqlConnection(connectionString);
            try {
                conn.Open();

                // Query for all employees below the manager
                String query = "SELECT * FROM 340_calendar_users WHERE isManager = 0";

                MySqlCommand cmd = new MySqlCommand(query, conn);

                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read()) {
                    nList.Add(reader.GetString("username"));
                    IDList.Add(reader.GetInt32("userID"));
                }
                reader.Close();
            }
            catch (Exception ex) {
                throw;
            }
            conn.Close();

            return (nList, IDList);
        }

        public void createMeeting(Event ev, List<int> participants) {
            /*  Method: createMeeting
             *      Takes an event and participants for it, then creates the meeting and
             *      participants within the database.
             *      Stores 'length' in a 2400 format. ie. 1530 = 3:30 PM
             *  Author: Samuel Griggs
             */
            MySqlConnection conn = new MySqlConnection(connectionString);
            try {
                conn.Open();

                // Grabs the most recent event entry with the same name
                string query = "SELECT eventID FROM 340_calendar_event WHERE eventTitle = @name";
                MySqlCommand cmd = new MySqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@name", ev.Title);

                MySqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                int eventID = reader.GetInt32("eventID");
                reader.Close();

                // Create the meeting
                string cmdStr = "INSERT INTO 340_calendar_meeting (length, eventID) VALUES (@len, @ID)";
                cmd = new MySqlCommand(cmdStr, conn);

                TimeSpan length = ev.EndTime - ev.StartTime;
                

                cmd.Parameters.AddWithValue("@len", Int32.Parse(length.ToString(@"hhmm")));
                cmd.Parameters.AddWithValue("@ID", eventID);

                cmd.ExecuteNonQuery();

                // Now insert meeting participants
                cmdStr = "INSERT INTO 340_calendar_event_participants (eventID, userID) VALUES (@eID, @uID)";
                cmd = new MySqlCommand(cmdStr, conn);

                cmd.Parameters.AddWithValue("@eID", eventID);
                //cmd.Parameters.Add("@uID", MySqlDbType.Int32);
                foreach (int i in participants) {
                    cmd.Parameters.AddWithValue("@uID", i);

                    cmd.ExecuteNonQuery();
                    cmd.Parameters.RemoveAt("@uID");
                }
                

            }
            catch (Exception ex) {
                Debug.WriteLine(ex.StackTrace);
                throw;
            }
            conn.Close();

        }
    }
}   
