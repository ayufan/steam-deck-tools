namespace FanControl
{
    partial class FanControlForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FanControlForm));
            this.fanLoopTimer = new System.Windows.Forms.Timer(this.components);
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparatorEndOfModes = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemStartupOnBootContext = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemAlwaysOnTopContext = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fanModeSelectMenu = new System.Windows.Forms.ToolStripComboBox();
            this.controlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemStartupOnBoot = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemAlwaysOnTop = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.propertyGridUpdateTimer = new System.Windows.Forms.Timer(this.components);
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.helpLabel = new System.Windows.Forms.Label();
            this.sensorWarningLabel = new System.Windows.Forms.Label();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenu.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // fanLoopTimer
            // 
            this.fanLoopTimer.Enabled = true;
            this.fanLoopTimer.Interval = 250;
            this.fanLoopTimer.Tick += new System.EventHandler(this.fanLoopTimer_Tick);
            // 
            // notifyIcon
            // 
            this.notifyIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notifyIcon.BalloonTipText = "Test";
            this.notifyIcon.ContextMenuStrip = this.contextMenu;
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "Steam Deck Fan Control";
            this.notifyIcon.DoubleClick += new System.EventHandler(this.formShow_Event);
            // 
            // contextMenu
            // 
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2,
            this.toolStripSeparator3,
            this.toolStripSeparatorEndOfModes,
            this.toolStripMenuItemAlwaysOnTopContext,
            this.toolStripMenuItemStartupOnBootContext,
            this.toolStripMenuItem3,
            this.toolStripMenuItem5,
            this.toolStripSeparator1,
            this.toolStripMenuItem1});
            this.contextMenu.Name = "fanModeSelectMenu";
            this.contextMenu.Size = new System.Drawing.Size(211, 194);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(210, 24);
            this.toolStripMenuItem2.Text = "&Show";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.formShow_Event);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(207, 6);
            // 
            // toolStripSeparatorEndOfModes
            // 
            this.toolStripSeparatorEndOfModes.Name = "toolStripSeparatorEndOfModes";
            this.toolStripSeparatorEndOfModes.Size = new System.Drawing.Size(207, 6);            // 
            // toolStripMenuItemAlwaysOnTopContext
            // 
            this.toolStripMenuItemAlwaysOnTopContext.Name = "toolStripMenuItemAlwaysOnTopContext";
            this.toolStripMenuItemAlwaysOnTopContext.Size = new System.Drawing.Size(210, 24);
            this.toolStripMenuItemAlwaysOnTopContext.Text = "&Always on Top";
            this.toolStripMenuItemAlwaysOnTopContext.Click += new System.EventHandler(this.toolStripMenuItemAlwaysOnTop_Click);
            // 
            // toolStripMenuItemStartupOnBootContext
            // 
            this.toolStripMenuItemStartupOnBootContext.Name = "toolStripMenuItemStartupOnBootContext";
            this.toolStripMenuItemStartupOnBootContext.Size = new System.Drawing.Size(210, 24);
            this.toolStripMenuItemStartupOnBootContext.Text = "Run On &Startup";
            this.toolStripMenuItemStartupOnBootContext.Click += new System.EventHandler(this.toolStripMenuItemStartupOnBoot_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(210, 24);
            this.toolStripMenuItem3.Text = "&Check for Updates";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.checkForUpdates_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(207, 6);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(210, 24);
            this.toolStripMenuItem1.Text = "&Exit";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.formClose_Event);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fanModeSelectMenu,
            this.controlToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(4, 1, 0, 1);
            this.menuStrip1.Size = new System.Drawing.Size(438, 30);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fanModeSelectMenu
            // 
            this.fanModeSelectMenu.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.fanModeSelectMenu.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.fanModeSelectMenu.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.fanModeSelectMenu.Name = "fanModeSelectMenu";
            this.fanModeSelectMenu.Size = new System.Drawing.Size(155, 28);
            this.fanModeSelectMenu.SelectedIndexChanged += new System.EventHandler(this.fanModeSelectMenu_SelectedIndexChanged);
            // 
            // controlToolStripMenuItem
            // 
            this.controlToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemAlwaysOnTop,
            this.toolStripMenuItemStartupOnBoot,
            this.toolStripMenuItem4,
            this.toolStripSeparator2,
            this.exitToolStripMenuItem});
            this.controlToolStripMenuItem.Name = "controlToolStripMenuItem";
            this.controlToolStripMenuItem.Size = new System.Drawing.Size(72, 28);
            this.controlToolStripMenuItem.Text = "&Control";
            // 
            // toolStripMenuItemAlwaysOnTop
            // 
            this.toolStripMenuItemAlwaysOnTop.Name = "toolStripMenuItemAlwaysOnTop";
            this.toolStripMenuItemAlwaysOnTop.Size = new System.Drawing.Size(192, 26);
            this.toolStripMenuItemAlwaysOnTop.Text = "&Always on Top";
            this.toolStripMenuItemAlwaysOnTop.Click += new System.EventHandler(this.toolStripMenuItemAlwaysOnTop_Click);
            // 
            // toolStripMenuItemStartupOnBoot
            // 
            this.toolStripMenuItemStartupOnBoot.Name = "toolStripMenuItemStartupOnBoot";
            this.toolStripMenuItemStartupOnBoot.Size = new System.Drawing.Size(192, 26);
            this.toolStripMenuItemStartupOnBoot.Text = "Run On &Startup";
            this.toolStripMenuItemStartupOnBoot.Click += new System.EventHandler(this.toolStripMenuItemStartupOnBoot_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(192, 26);
            this.toolStripMenuItem4.Text = "Help";
            this.toolStripMenuItem4.Click += new System.EventHandler(this.help_DoubleClick);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(189, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(192, 26);
            this.exitToolStripMenuItem.Text = "&Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.formClose_Event);
            // 
            // propertyGridUpdateTimer
            // 
            this.propertyGridUpdateTimer.Enabled = true;
            this.propertyGridUpdateTimer.Interval = 1000;
            this.propertyGridUpdateTimer.Tick += new System.EventHandler(this.propertyGridUpdateTimer_Tick);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.helpLabel, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.sensorWarningLabel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.propertyGrid1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 30);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(438, 454);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label1.ForeColor = System.Drawing.Color.Red;
            this.label1.Location = new System.Drawing.Point(2, 394);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(434, 40);
            this.label1.TabIndex = 9;
            this.label1.Text = "This application is highly experimental.\r\nUse at your own risk!";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // helpLabel
            // 
            this.helpLabel.AutoSize = true;
            this.helpLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.helpLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.helpLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.helpLabel.Font = new System.Drawing.Font("Segoe UI", 9F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point);
            this.helpLabel.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.helpLabel.Location = new System.Drawing.Point(2, 434);
            this.helpLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.helpLabel.Name = "helpLabel";
            this.helpLabel.Size = new System.Drawing.Size(434, 20);
            this.helpLabel.TabIndex = 8;
            this.helpLabel.Text = "https://steam-deck-tools.ayufan.dev";
            this.helpLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.helpLabel.DoubleClick += new System.EventHandler(this.help_DoubleClick);
            // 
            // sensorWarningLabel
            // 
            this.sensorWarningLabel.AutoSize = true;
            this.sensorWarningLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sensorWarningLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.sensorWarningLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.sensorWarningLabel.ForeColor = System.Drawing.Color.Red;
            this.sensorWarningLabel.Location = new System.Drawing.Point(2, 334);
            this.sensorWarningLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.sensorWarningLabel.Name = "sensorWarningLabel";
            this.sensorWarningLabel.Size = new System.Drawing.Size(434, 60);
            this.sensorWarningLabel.TabIndex = 6;
            this.sensorWarningLabel.Text = "Some sensors are missing.\r\nThe fan behavior might be incorrect.\r\nWhich might resu" +
    "lt in device overheating.\r\n";
            this.sensorWarningLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.sensorWarningLabel.Visible = false;
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.DisabledItemForeColor = System.Drawing.SystemColors.ControlText;
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid1.HelpVisible = false;
            this.propertyGrid1.Location = new System.Drawing.Point(2, 1);
            this.propertyGrid1.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(434, 332);
            this.propertyGrid1.TabIndex = 1;
            this.propertyGrid1.ToolbarVisible = false;
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(210, 24);
            this.toolStripMenuItem5.Text = "Help";
            // 
            // FanControlForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(438, 484);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FanControlForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Steam Deck Fan Control";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FanControlForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FanControlForm_FormClosed);
            this.contextMenu.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer fanLoopTimer;
        private NotifyIcon notifyIcon;
        private ContextMenuStrip contextMenu;
        private MenuStrip menuStrip1;
        private ToolStripSeparator toolStripSeparatorEndOfModes;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripComboBox fanModeSelectMenu;
        private ToolStripMenuItem controlToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Timer propertyGridUpdateTimer;
        private TableLayoutPanel tableLayoutPanel1;
        private Label sensorWarningLabel;
        private PropertyGrid propertyGrid1;
        private ToolStripMenuItem toolStripMenuItemStartupOnBoot;
        private ToolStripSeparator toolStripSeparator2;
        private Label label1;
        private Label helpLabel;
        private ToolStripMenuItem toolStripMenuItemAlwaysOnTop;
        private ToolStripMenuItem toolStripMenuItem2;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem toolStripMenuItemStartupOnBootContext;
        private ToolStripMenuItem toolStripMenuItemAlwaysOnTopContext;
        private ToolStripMenuItem toolStripMenuItem3;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem toolStripMenuItem4;
        private ToolStripMenuItem toolStripMenuItem5;
    }
}