namespace PowerControl.Menu
{
    public class MenuItemWithOptions : MenuItem
    {
        public IList<string> Options { get; set; } = new List<string>();
        public string? SelectedOption { get; private set; }
        public string? ActiveOption { get; set; }
        public int ApplyDelay { get; set; }
        public bool CycleOptions { get; set; } = true;
        public string? PersistentKey;

        public Func<string?>? CurrentValue { get; set; }
        public Func<string[]?>? OptionsValues { get; set; }
        public Func<string, string?>? ApplyValue { get; set; }
        public Action? AfterApply { get; set; }
        public Func<string?>? ResetValue { get; set; }

        public event Action<MenuItemWithOptions, String?, String> ValueChanged;

        private System.Windows.Forms.Timer delayTimer = new System.Windows.Forms.Timer();
        private ToolStripMenuItem toolStripItem = new ToolStripMenuItem();

        public MenuItemWithOptions()
        {
            this.Selectable = true;

            ValueChanged += delegate { };

            delayTimer.Tick += delegate (object? sender, EventArgs e)
            {
                if (delayTimer != null)
                    delayTimer.Stop();

                FinalizeSet(delayTimer.Interval > ApplyDelay);
            };
        }

        public override void Reset()
        {
            if (ResetValue == null)
                return;

            var resetOption = ResetValue();
            if (resetOption == null || resetOption == ActiveOption)
                return;

            Set(resetOption, 0);
        }

        public override void Update()
        {
            if (CurrentValue != null)
            {
                var result = CurrentValue();
                if (result != null)
                {
                    ActiveOption = result;
                    Visible = true;
                }
                else
                {
                    Visible = false;
                }
            }

            if (OptionsValues != null)
            {
                var result = OptionsValues();
                if (result != null)
                    Options = result.ToList();
                else
                    Visible = false;
            }

            if (ActiveOption == null && Options.Count > 0)
                ActiveOption = Options.First();
        }

        public void Set(String value, int overrideDelay = -1, bool silent = false)
        {
            if (delayTimer != null)
                delayTimer.Stop();

            SelectedOption = value;

            if (ApplyDelay == 0 || overrideDelay == 0)
            {
                FinalizeSet(silent);
                return;
            }

            delayTimer.Interval = overrideDelay > 0 ? overrideDelay : ApplyDelay > 0 ? ApplyDelay : 1;
            delayTimer.Enabled = true;
        }

        private void FinalizeSet(bool silent = false)
        {
            var wasOption = ActiveOption;

            if (ApplyValue != null && SelectedOption != null)
            {
                ActiveOption = ApplyValue(SelectedOption);

                if (AfterApply != null && !silent)
                    AfterApply();
            }
            else
                ActiveOption = SelectedOption;

            SelectedOption = null;

            if (wasOption != ActiveOption && ActiveOption != null && !silent)
                ValueChanged(this, wasOption, ActiveOption);
        }

        public override void CreateMenu(System.Windows.Forms.ContextMenuStrip contextMenu)
        {
            toolStripItem.Text = Name;
            contextMenu.Items.Add(toolStripItem);
            contextMenu.Opening += delegate
            {
                Update();

                toolStripItem.DropDownItems.Clear();

                foreach (var option in Options)
                {
                    var item = new ToolStripMenuItem(option);
                    item.Checked = option == (SelectedOption ?? ActiveOption);
                    item.Click += delegate { Set(option, 0); };
                    toolStripItem.DropDownItems.Add(item);
                }

                toolStripItem.Visible = Visible && Options.Count > 0;
            };
        }

        private void SelectIndex(int index)
        {
            if (Options.Count == 0)
                return;

            Set(Options[Math.Clamp(index, 0, Options.Count - 1)]);
        }

        public override void SelectNext(int change)
        {
            int index = Options.IndexOf(SelectedOption ?? ActiveOption ?? "");
            if (index < 0)
            {
                if (change > 0)
                    SelectIndex(0); // select first
                else
                    SelectIndex(Options.Count); // select last
                return;
            }

            if (CycleOptions)
                SelectIndex((index + change + Options.Count) % Options.Count);
            else
                SelectIndex(index + change);
        }

        public override string Render(MenuItem? selected)
        {
            string output = "";

            if (selected == this)
                output += Color(Name + ":", Colors.White).PadRight(30);
            else
                output += Color(Name + ":", Colors.Blue).PadRight(30);

            output += optionText(SelectedOption ?? ActiveOption);

            if (SelectedOption != null && ActiveOption != SelectedOption)
                output += " (active: " + optionText(ActiveOption) + ")";

            return output;
        }

        private String optionText(String? option)
        {
            String text;

            if (option is null)
                text = Color("?", Colors.White);
            else if (option == (SelectedOption ?? ActiveOption))
                text = Color(option, Colors.Red);
            else if (option == ActiveOption)
                text = Color(option, Colors.White);
            else
                text = Color(option, Colors.Green);

            return text;
        }
    }
}
