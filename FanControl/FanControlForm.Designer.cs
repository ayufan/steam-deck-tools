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
            this.fanModeSelectNotifyMenu = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fanModeSelectMenu = new System.Windows.Forms.ToolStripComboBox();
            this.controlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.propertyGridUpdateTimer = new System.Windows.Forms.Timer(this.components);
            this.contextMenu.SuspendLayout();
            this.menuStrip1.SuspendLayout();
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
            this.notifyIcon.Visible = true;
            this.notifyIcon.DoubleClick += new System.EventHandler(this.formShow_Event);
            // 
            // contextMenu
            // 
            this.contextMenu.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fanModeSelectNotifyMenu,
            this.toolStripSeparator1,
            this.toolStripMenuItem1});
            this.contextMenu.Name = "fanModeSelectMenu";
            this.contextMenu.Size = new System.Drawing.Size(182, 96);
            // 
            // fanModeSelectNotifyMenu
            // 
            this.fanModeSelectNotifyMenu.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.fanModeSelectNotifyMenu.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.fanModeSelectNotifyMenu.Name = "fanModeSelectNotifyMenu";
            this.fanModeSelectNotifyMenu.Size = new System.Drawing.Size(121, 40);
            this.fanModeSelectNotifyMenu.SelectedIndexChanged += new System.EventHandler(this.fanModeSelect_SelectedValueChanged);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(178, 6);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(181, 38);
            this.toolStripMenuItem1.Text = "Exit";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.formClose_Event);
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.DisabledItemForeColor = System.Drawing.SystemColors.ControlText;
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid1.HelpVisible = false;
            this.propertyGrid1.Location = new System.Drawing.Point(0, 44);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(712, 885);
            this.propertyGrid1.TabIndex = 0;
            this.propertyGrid1.ToolbarVisible = false;
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fanModeSelectMenu,
            this.controlToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(712, 44);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fanModeSelectMenu
            // 
            this.fanModeSelectMenu.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.fanModeSelectMenu.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.fanModeSelectMenu.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.fanModeSelectMenu.Name = "fanModeSelectMenu";
            this.fanModeSelectMenu.Size = new System.Drawing.Size(121, 40);
            this.fanModeSelectMenu.SelectedIndexChanged += new System.EventHandler(this.fanModeSelect_SelectedValueChanged);
            // 
            // controlToolStripMenuItem
            // 
            this.controlToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.controlToolStripMenuItem.Name = "controlToolStripMenuItem";
            this.controlToolStripMenuItem.Size = new System.Drawing.Size(113, 40);
            this.controlToolStripMenuItem.Text = "&Control";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(184, 44);
            this.exitToolStripMenuItem.Text = "&Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.formClose_Event);
            // 
            // propertyGridUpdateTimer
            // 
            this.propertyGridUpdateTimer.Enabled = true;
            this.propertyGridUpdateTimer.Interval = 1000;
            this.propertyGridUpdateTimer.Tick += new System.EventHandler(this.propertyGridUpdateTimer_Tick);
            // 
            // FanControlForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 32F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(712, 929);
            this.Controls.Add(this.propertyGrid1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FanControlForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Steam Deck Fan Control";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FanControlForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FanControlForm_FormClosed);
            this.contextMenu.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer fanLoopTimer;
        private NotifyIcon notifyIcon;
        private PropertyGrid propertyGrid1;
        private ContextMenuStrip contextMenu;
        private MenuStrip menuStrip1;
        private ToolStripComboBox fanModeSelectNotifyMenu;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripComboBox fanModeSelectMenu;
        private ToolStripMenuItem controlToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Timer propertyGridUpdateTimer;
    }
}