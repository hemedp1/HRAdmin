using HRAdmin.Forms;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace HRAdmin.UserControl
{
    public partial class UC_R_DetailsRoom : System.Windows.Forms.UserControl
    {
        private string loggedInUser;
        private string loggedInDepart;
        private List<MeetingSpace> _meetingSpaces = new List<MeetingSpace>();
        private static List<MeetingSpaceConfig> _spaceConfigs = new List<MeetingSpaceConfig>();
        private static readonly string ConfigFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "HRAdmin\\meeting_spaces.config");
        private readonly Timer _refreshTimer;

        public UC_R_DetailsRoom(string username)
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
            loggedInUser = username;

            _refreshTimer = new Timer
            {
                Interval = 3000
            };
            _refreshTimer.Tick += RefreshTimer_Tick;

            SetupControl();
        }

        private void SetupControl()
        {
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.GetType().GetMethod("SetStyle", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.Invoke(flowLayoutPanel1, new object[] { ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true });
            cmbDWM.SelectedIndexChanged += cmbDWM_SelectedIndexChanged;
            this.Load += UC_R_DetailsRoom_Load;
            this.VisibleChanged += UC_R_DetailsRoom_VisibleChanged;
            this.SizeChanged += UC_R_DetailsRoom_SizeChanged;
            cmbDWM.SelectedIndex = 0;
        }

        private void UC_R_DetailsRoom_SizeChanged(object sender, EventArgs e)
        {
            AdjustPanelWidths();
        }

        private void AdjustPanelWidths()
        {
            if (flowLayoutPanel1 == null || flowLayoutPanel1.Width == 0) return;

            foreach (var space in _meetingSpaces)
            {
                space.Panel.Width = flowLayoutPanel1.ClientSize.Width - 5;
                space.Grid.Width = space.Panel.Width - 10;

                var btn = space.Panel.Controls.OfType<Button>().FirstOrDefault();
                if (btn != null)
                {
                    btn.Location = new Point(space.Panel.Width - btn.Width - 5, 5);
                }
            }
        }

        private void RestoreSpaces()
        {
            ClearExistingSpaces();
            foreach (var config in _spaceConfigs)
            {
                var space = new MeetingSpace(config.Name, flowLayoutPanel1);
                flowLayoutPanel1.Controls.Add(space.Panel);
                _meetingSpaces.Add(space);

                space.Panel.Visible = !config.IsHidden;
                var btnHide = space.Panel.Controls.OfType<Button>().FirstOrDefault();
                if (btnHide != null)
                {
                    btnHide.Text = config.IsHidden ? "Show" : "";
                }
            }
        }

        private void CreateSpaceFromConfig(MeetingSpaceConfig config)
        {
            var space = new MeetingSpace(config.Name, flowLayoutPanel1);
            flowLayoutPanel1.Controls.Add(space.Panel);
            _meetingSpaces.Add(space);
        }

        private void UC_R_DetailsRoom_Load(object sender, EventArgs e)
        {
            BeginInvoke(new Action(() =>
            {
                cmbDWM.SelectedIndex = 0;
                LoadSpaceConfigs();
                RestoreSpaces();
                ApplyFilter("Daily");
                StartAutoRefresh();
            }));
        }

        private void UC_R_DetailsRoom_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                RestoreSpaces();
                ApplyFilter("Daily");
                StartAutoRefresh();
            }
            else
            {
                StopAutoRefresh();
            }
        }

        private void StartAutoRefresh()
        {
            if (!_refreshTimer.Enabled)
            {
                _refreshTimer.Start();
            }
        }

        private void StopAutoRefresh()
        {
            if (_refreshTimer.Enabled)
            {
                _refreshTimer.Stop();
            }
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            if (this.Visible && cmbDWM.SelectedItem != null)
            {
                RefreshData();
            }
        }

        private void RefreshData()
        {
            if (!IsNetworkAvailable())
            {
                MessageBox.Show("Network connection is unavailable. Please check your connection and try again.",
                    "Network Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string currentFilter = cmbDWM.SelectedItem?.ToString() ?? "Daily";
                ApplyFilter(currentFilter);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during auto-refresh: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsNetworkAvailable()
        {
            try
            {
                return NetworkInterface.GetIsNetworkAvailable();
            }
            catch
            {
                return false;
            }
        }

        private void cmbDWM_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbDWM.SelectedItem != null)
            {
                ApplyFilter(cmbDWM.SelectedItem.ToString());
            }
        }

        private void ApplyFilter(string filterType)
        {
            if (!IsNetworkAvailable())
            {
                MessageBox.Show("No network connection. Displaying last known data.",
                    "Network Unavailable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();
                    string roomQuery = "SELECT DISTINCT MeetingRoom FROM tbl_MeetingSchedule WHERE MeetingRoom IS NOT NULL";
                    SqlCommand roomCmd = new SqlCommand(roomQuery, con);
                    SqlDataReader dr = roomCmd.ExecuteReader();

                    var newRoomNames = new List<string>();
                    while (dr.Read())
                    {
                        newRoomNames.Add(dr["MeetingRoom"].ToString());
                    }
                    dr.Close();

                    var existingConfigs = _spaceConfigs.ToDictionary(c => c.Name, c => c);
                    _spaceConfigs.Clear();
                    foreach (var roomName in newRoomNames)
                    {
                        if (existingConfigs.TryGetValue(roomName, out var config))
                        {
                            _spaceConfigs.Add(config);
                        }
                        else
                        {
                            _spaceConfigs.Add(new MeetingSpaceConfig { Name = roomName });
                        }
                    }

                    var spacesToRemove = _meetingSpaces.Where(s => !newRoomNames.Contains(s.Label.Text)).ToList();
                    foreach (var space in spacesToRemove)
                    {
                        flowLayoutPanel1.Controls.Remove(space.Panel);
                        space.Panel.Dispose();
                        _meetingSpaces.Remove(space);
                    }

                    foreach (var roomName in newRoomNames)
                    {
                        var existingSpace = _meetingSpaces.FirstOrDefault(s => s.Label.Text == roomName);
                        if (existingSpace == null)
                        {
                            var config = _spaceConfigs.First(c => c.Name == roomName);
                            var space = new MeetingSpace(roomName, flowLayoutPanel1);
                            space.Panel.Visible = !config.IsHidden;
                            flowLayoutPanel1.Controls.Add(space.Panel);
                            _meetingSpaces.Add(space);
                            var btnHide = space.Panel.Controls.OfType<Button>().FirstOrDefault();
                            if (btnHide != null)
                            {
                                btnHide.Text = config.IsHidden ? "Show" : "";
                            }
                        }
                        var spaceToUpdate = _meetingSpaces.First(s => s.Label.Text == roomName);
                        if (spaceToUpdate.Panel.Visible)
                        {
                            LoadMeetingData(spaceToUpdate.Grid, roomName, filterType);
                        }
                    }

                    SaveSpaceConfigs();
                    AdjustPanelWidths();
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database connection error: {ex.Message}. Please check your network or database server.",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading meeting rooms: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                flowLayoutPanel1.ResumeLayout();
                this.ResumeLayout();
            }
        }

        private void LoadMeetingData(DataGridView grid, string roomName, string filterType)
        {
            if (!IsNetworkAvailable())
            {
                return;
            }

            grid.SuspendLayout();

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    string query = GetFilterQuery(filterType);
                    DateTime today = DateTime.Today;
                    TimeSpan nowTime = DateTime.Now.TimeOfDay;
                    DateTime now = DateTime.Now;

                    con.Open();
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Today", today);
                    cmd.Parameters.AddWithValue("@RoomName", roomName);
                    cmd.Parameters.AddWithValue("@NowTime", nowTime);
                    cmd.Parameters.AddWithValue("@Now", now);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    grid.DataSource = null;
                    grid.DataSource = dt;
                    FormatDataGridView(grid);
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error for {roomName}: {ex.Message}",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data for {roomName}: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                grid.ResumeLayout();
            }
        }

        private string GetFilterQuery(string filterType)
        {
            switch (filterType)
            {
                case "Daily":
                    return @"SELECT Person, MeetingTitle AS [Title],
                            FORMAT(StartTime, 'hh\:mm') + ' to ' + FORMAT(EndTime, 'hh\:mm') AS Time, 
                            MeetingDate AS [Date], Department AS [Section]
                            FROM tbl_MeetingSchedule 
                            WHERE CAST(MeetingDate AS DATE) = @Today 
                            AND MeetingRoom = @RoomName
                            AND (MeetingDate > @Now OR (MeetingDate = CAST(@Now AS DATE) AND EndTime >= CAST(@Now AS TIME)))
                            ORDER BY StartTime ASC";

                case "Weekly":
                    return @"SELECT Person, MeetingTitle AS [Title],
                            FORMAT(StartTime, 'hh\:mm') + ' to ' + FORMAT(EndTime, 'hh\:mm') AS Time, 
                            MeetingDate AS [Date], Department AS [Section]
                            FROM tbl_MeetingSchedule 
                            WHERE DATEDIFF(WEEK, 0, MeetingDate) = DATEDIFF(WEEK, 0, @Today)
                            AND YEAR(MeetingDate) = YEAR(@Today) 
                            AND MeetingRoom = @RoomName
                            AND (MeetingDate > @Now OR (MeetingDate = CAST(@Now AS DATE) AND EndTime >= CAST(@Now AS TIME)))
                            ORDER BY MeetingDate ASC, StartTime ASC";

                case "Monthly":
                    return @"SELECT Person, MeetingTitle AS [Title],
                            FORMAT(StartTime, 'hh\:mm') + ' to ' + FORMAT(EndTime, 'hh\:mm') AS Time, 
                            MeetingDate AS [Date], Department AS [Section]
                            FROM tbl_MeetingSchedule 
                            WHERE MONTH(MeetingDate) = MONTH(@Today)
                            AND YEAR(MeetingDate) = YEAR(@Today) 
                            AND MeetingRoom = @RoomName
                            AND (MeetingDate > @Now OR (MeetingDate = CAST(@Now AS DATE) AND EndTime >= CAST(@Now AS TIME)))
                            ORDER BY MeetingDate ASC, StartTime ASC";

                default:
                    return string.Empty;
            }
        }

        private void FormatDataGridView(DataGridView grid)
        {
            grid.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.MidnightBlue,
                Font = new Font("Arial", 11),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                SelectionBackColor = Color.OrangeRed,
                SelectionForeColor = Color.White,
                Padding = new Padding(3)
            };

            grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                SelectionBackColor = Color.OrangeRed,
                SelectionForeColor = Color.White
            };

            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Arial", 11, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft
            };

            grid.EnableHeadersVisualStyles = false;
            grid.RowHeadersVisible = false;
            grid.AllowUserToAddRows = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            if (grid.Columns.Count >= 3)
            {
                grid.Columns["Person"].Width = 135;
                grid.Columns["Title"].Width = 345;
                grid.Columns["Time"].Width = 135;
                grid.Columns["Date"].Width = 135;
                grid.Columns["Section"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
        }

        private void ClearExistingSpaces()
        {
            foreach (var space in _meetingSpaces)
            {
                flowLayoutPanel1.Controls.Remove(space.Panel);
                space.Panel.Dispose();
            }
            _meetingSpaces.Clear();
        }

        private void LoadSpaceConfigs()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    using (var stream = File.OpenRead(ConfigFilePath))
                    {
                        var formatter = new BinaryFormatter();
                        _spaceConfigs = (List<MeetingSpaceConfig>)formatter.Deserialize(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configurations: {ex.Message}");
                _spaceConfigs = new List<MeetingSpaceConfig>();
            }
        }

        public static void SaveSpaceConfigs()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigFilePath));
                using (var stream = File.Create(ConfigFilePath))
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, _spaceConfigs);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving configurations: {ex.Message}");
            }
        }

        [Serializable]
        private class MeetingSpaceConfig
        {
            public string Name { get; set; }
            public bool IsHidden { get; set; }
        }

        private class MeetingSpace
        {
            public Panel Panel { get; }
            public DataGridView Grid { get; }
            public Label Label { get; }
            private bool _isHidden; // Track visibility state

            public MeetingSpace(string name, FlowLayoutPanel container)
            {
                _isHidden = false; // Initialize to false
                Panel = new Panel
                {
                    BorderStyle = BorderStyle.None,
                    Height = 200,
                    Width = container.ClientSize.Width - 5,
                    Margin = new Padding(0, 0, 0, 5),
                    Tag = this,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
                };

                Label = new Label
                {
                    Text = name,
                    Location = new Point(5, 5),
                    AutoSize = true,
                    Font = new Font("Arial", 12, FontStyle.Bold)
                };

                Grid = new DataGridView
                {
                    Name = "dgvSpace_" + name.Replace(" ", "_"),
                    Location = new Point(5, 30),
                    BackgroundColor = Color.White,
                    Width = Panel.Width - 10,
                    Height = Panel.Height - 40,
                    Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom,
                    CellBorderStyle = DataGridViewCellBorderStyle.None,
                    RowTemplate = { Height = 26 }
                };

                string iconPath = Path.Combine(Application.StartupPath, "Img", "hidden.png");

                var btnRemove = new Button
                {
                    Size = new Size(30, 30),
                    Location = new Point(Panel.Width - 35, 3),
                    Image = Image.FromFile(iconPath),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.Transparent
                };
                btnRemove.FlatAppearance.BorderSize = 0;


                // Event handlers for hover effect
                btnRemove.MouseEnter += (s, e) =>
                {
                    btnRemove.BackColor = Color.OrangeRed;
                };
                btnRemove.MouseLeave += (s, e) =>
                {
                    btnRemove.BackColor = Color.Transparent;
                };

                btnRemove.Click += (s, e) => ToggleVisibility();

                Panel.Controls.Add(Label);
                Panel.Controls.Add(Grid);
                Panel.Controls.Add(btnRemove);
            }

            private void RemoveSpace()
            {
                if (Panel.Parent != null)
                {
                    Panel.Parent.Controls.Remove(Panel);
                    _spaceConfigs.RemoveAll(c => c.Name == Label.Text);
                    UC_R_DetailsRoom.SaveSpaceConfigs();
                }
            }

            public void SetVisibility(bool isVisible)
            {
                _isHidden = !isVisible; // Sync _isHidden with the desired visibility
                Panel.Visible = isVisible;

                var config = _spaceConfigs.FirstOrDefault(c => c.Name == Label.Text);
                if (config != null)
                {
                    config.IsHidden = _isHidden;
                    UC_R_DetailsRoom.SaveSpaceConfigs();
                }
            }

            private void ToggleVisibility()
            {
                _isHidden = !_isHidden;
                Panel.Visible = !_isHidden;

                var btnHide = Panel.Controls.OfType<Button>().FirstOrDefault();
                if (btnHide != null)
                {
                    // No text update as per your request
                }

                var config = _spaceConfigs.FirstOrDefault(c => c.Name == Label.Text);
                if (config != null)
                {
                    config.IsHidden = _isHidden;
                    UC_R_DetailsRoom.SaveSpaceConfigs();
                }
            }
        }

        private void CreateNewMeetingSpace()
        {
            string spaceName = "Meeting Space " + (_spaceConfigs.Count + 1);
            var config = new MeetingSpaceConfig { Name = spaceName };
            _spaceConfigs.Add(config);
            SaveSpaceConfigs();
            CreateSpaceFromConfig(config);
        }

        private void addControls(System.Windows.Forms.UserControl userControl)
        {
            if (Form_Home.sharedPanel != null && Form_Home.sharedLabel != null)
            {
                Form_Home.sharedPanel.Controls.Clear();
                userControl.Dock = DockStyle.Fill;
                Form_Home.sharedPanel.Controls.Add(userControl);
                userControl.BringToFront();
            }
            else
            {
                MessageBox.Show("Panel not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            Form_Home.sharedLabel.Text = "HR && Admin";
            Form_Home.sharedButton.Visible = false;
            Form_Home.sharedButtonew.Visible = false;
            Form_Home.sharedButtonBC.Visible = false;
            Form_Home.sharedButton2.Visible = false;
            Form_Home.sharedButton3.Visible = false;
            Form_Home.sharedButtonbtnApp.Visible = false;
            Form_Home.sharedButtonbtnWDcar.Visible = false;
            Form_Home.sharedbuttonInspect.Visible = false;
            Form_Home.sharedbtn_Accident.Visible = false;
            UC_A_Admin ug = new UC_A_Admin(loggedInUser, loggedInDepart);
            addControls(ug);
        }

        private void btnShowALL_Click(object sender, EventArgs e)
        {
            foreach (var space in _meetingSpaces)
            {
                space.SetVisibility(true); // Sync state with visibility
            }

            SaveSpaceConfigs();
            flowLayoutPanel1.Invalidate();
            AdjustPanelWidths();
        }
    }
}