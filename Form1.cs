using System.Data.SqlClient;
using Notificator.SharedServices;
using System.Configuration;
using Microsoft.Extensions.Logging;
using System.Windows.Forms;
using Notificator.SharedServices.Models;

namespace Notificator.GUI
{
    public partial class Form1 : Form
    {

        private IEventRepository _eventRepository;

        public List<EventModel> events = null;

        public Form1()
        {
            string connectionString = ConfigurationManager.AppSettings["ConnectionString"];
            _eventRepository = new EventRepository(connectionString);

            InitializeComponent();

            this.PopulateListBox();
        }

        /// <summary>
        ///  Populates the list box with events from the repository.
        /// </summary>
        private void PopulateListBox()
        {
            this.events = _eventRepository.GetAll().ToList();
            listBox1.DisplayMember = "Name";
            listBox1.DataSource = events;
        }

        /// <summary>
        ///  Handles the selection of an item in the list box.
        /// </summary>
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                EventModel selectedEvent = (EventModel)listBox1.SelectedItem;

                textBox1.Text = selectedEvent.Name;
                richTextBox1.Text = selectedEvent.Description;
                dateTimePicker1.Value = selectedEvent.Date;
                checkBox1.Checked = selectedEvent.Status;

                this.PanelStatus(true);

                buttonDelete.Visible = true;
            }
        }

        /// <summary>
        ///  Handles the click event of the "Save" button.
        /// </summary>
        private async void buttonSave_Click(object sender, EventArgs e)
        {
            try
            {
                //Update event
                if (listBox1.SelectedItem != null)
                {
                    EventModel selectedEvent = (EventModel)listBox1.SelectedItem;
                    selectedEvent.Name = textBox1.Text;
                    selectedEvent.Description = richTextBox1.Text;
                    selectedEvent.Date = dateTimePicker1.Value;
                    selectedEvent.Status = checkBox1.Checked;

                    await Task.Run(() =>
                    {
                        _eventRepository.Update(selectedEvent);
                    });
                }
                //Create new event
                else
                {
                    EventModel newEvent = new EventModel()
                    {
                        Name = textBox1.Text,
                        Description = richTextBox1.Text,
                        Date = dateTimePicker1.Value,
                        Status = checkBox1.Checked,
                    };

                    await Task.Run(() =>
                    {
                        _eventRepository.Create(newEvent);
                    });

                    this.PanelStatus(false);

                    //Refresh events list
                    this.PopulateListBox();
                }

                MessageBox.Show("Operation completed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException !=  null ? ex.InnerException.Message : ex.Message);
            }
        }

        /// <summary>
        ///  Handles the click event of the "New Event" button
        /// </summary>
        private void buttonNewEvent_Click(object sender, EventArgs e)
        {
            listBox1.SelectedIndex = -1;

            textBox1.Text = "";
            richTextBox1.Text = "";
            dateTimePicker1.Value = DateTime.Now;
            checkBox1.Checked = false;

            this.PanelStatus(true);

            buttonDelete.Visible = false;
        }

        /// <summary>
        ///  Handles the click event of the "Cancel" button
        /// </summary>
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            listBox1.SelectedIndex = -1;

            textBox1.Text = "";
            richTextBox1.Text = "";
            dateTimePicker1.Value = DateTime.Now;
            checkBox1.Checked = false;

            this.PanelStatus(false);

            buttonDelete.Visible = false;
        }

        /// <summary>
        ///  Handles the click event of the "Delete" button
        /// </summary>
        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                EventModel selectedEvent = (EventModel)listBox1.SelectedItem;

                DialogResult result = MessageBox.Show("Are you sure you want to proceed?", "Alert", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    _eventRepository.Delete(selectedEvent.Id);

                    this.PanelStatus(false);

                    //Refresh events list
                    this.PopulateListBox();
                }
            }
        }

        /// <summary>
        ///  Controls the status (enabled/disabled) of controls within the panel
        /// </summary>
        private void PanelStatus(bool status)
        {
            foreach (Control control in panel1.Controls)
            {
                control.Enabled = status;
            }
        }

    }
}