namespace PowerControl.Menu
{
    public class MenuItemWithOptions : MenuItem
    {
        public IList<string> Options { get; set; } = new List<string>();
        public string? SelectedOption { get; private set; }
        public string? ActiveOption { get; set; }
        public string? ProfileOption { get; set; }
        public int ApplyDelay { get; set; }
        public bool CycleOptions { get; set; } = true;
        public string? PersistentKey { get; set; }
        public bool PersistOnCreate { get; set; } = true;

        public IList<MenuItemWithOptions> Impacts { get; set; } = new List<MenuItemWithOptions>();

        public Func<string?>? CurrentValue { get; set; }
        public Func<string[]?>? OptionsValues { get; set; }
        public Func<string, string?>? ApplyValue { get; set; }
        public Action<MenuItemWithOptions, string?, string>? ImpactedBy { get; set; }
        public Action? AfterApply { get; set; }
        public Func<string?>? ResetValue { get; set; }

        public event Action<MenuItemWithOptions, String?, String> ValueChanged;

        private System.Windows.Forms.Timer delayTimer = new System.Windows.Forms.Timer();
        private ToolStripMenuItem toolStripItem = new ToolStripMenuItem();
        private bool runAfterApply = false;

        public MenuItemWithOptions()
        {
            this.Selectable = true;

            ValueChanged += delegate { };

            delayTimer.Tick += delegate (object? sender, EventArgs e)
            {
                if (delayTimer != null)
                    delayTimer.Stop();

                FinalizeSet();
            };
        }

        public override void Reset()
        {
            if (ResetValue == null)
                return;

            var resetOption = ResetValue();
            if (resetOption == null || resetOption == ActiveOption)
                return;

            Set(resetOption, true, false);
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

        public void Set(String value, bool immediate, bool refresh)
        {
            if (delayTimer != null)
                delayTimer.Stop();

            SelectedOption = value;
            runAfterApply = refresh;

            if (ApplyDelay == 0 || immediate)
            {
                FinalizeSet();
                return;
            }

            delayTimer.Interval = ApplyDelay > 0 ? ApplyDelay : 1;
            delayTimer.Enabled = true;
        }

        private void FinalizeSet()
        {
            var wasOption = ActiveOption;

            if (ApplyValue != null && SelectedOption != null)
            {
                try
                {
                    ActiveOption = ApplyValue(SelectedOption);
                }
                catch (Exception e)
                {
                    CommonHelpers.Log.TraceException("FinalizeSet", Name, e);
                    Update();
                }
            }
            else
                ActiveOption = SelectedOption;

            SelectedOption = null;

            if (wasOption != ActiveOption && ActiveOption != null)
            {
                if (AfterApply != null)
                    AfterApply();

                foreach (var impact in Impacts)
                {
                    if (impact.ImpactedBy is not null)
                        impact.ImpactedBy(this, wasOption, ActiveOption);
                    impact.Update();
                }

                ValueChanged(this, wasOption, ActiveOption);
            }
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
                    item.Click += delegate { Set(option, true, true); };
                    toolStripItem.DropDownItems.Add(item);
                }

                AddMenuItemsToModifyProfile(
                    PowerControl.Options.Profiles.Controller?.AutostartProfileSettings,
                    toolStripItem.DropDownItems
                );

                toolStripItem.Visible = Visible && Options.Count > 0;
            };
        }

        private void AddMenuItemsToModifyProfile(Helper.ProfileSettings? profileSettings, ToolStripItemCollection dropDownItems)
        {
            if (profileSettings is null || PersistentKey is null)
                return;

            dropDownItems.Add(new ToolStripSeparator());

            var headingItem = new ToolStripMenuItem(profileSettings.ProfileName + ": ");
            dropDownItems.Add(headingItem);

            var persistedValue = profileSettings.GetValue(PersistentKey);

            foreach (var option in Options)
            {
                var item = new ToolStripMenuItem("Set: " + option);
                item.Checked = option == persistedValue;
                item.Click += delegate { profileSettings.SetValue(PersistentKey, option); };
                headingItem.DropDownItems.Add(item);
            }

            if (persistedValue is not null)
            {
                headingItem.Text += persistedValue;
                headingItem.Checked = true;

                headingItem.DropDownItems.Add(new ToolStripSeparator());
                var unsetItem = headingItem.DropDownItems.Add("Unset");
                unsetItem.Click += delegate { profileSettings.DeleteKey(PersistentKey); };
            }
            else
            {
                headingItem.Text += "Not set";
            }
        }

        private void SelectIndex(int index)
        {
            if (Options.Count == 0)
                return;

            Set(Options[Math.Clamp(index, 0, Options.Count - 1)], false, true);
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

            if (ProfileOption != null)
            {
                if (ProfileOption != ActiveOption && ProfileOption != SelectedOption)
                    output += " (profile: " + optionText(ProfileOption) + ")";
                else
                    output += " [P]";
            }

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

        public static IEnumerable<MenuItemWithOptions> Order(IEnumerable<MenuItemWithOptions> items)
        {
            HashSet<MenuItemWithOptions> processed = new HashSet<MenuItemWithOptions>();

            // Try to run iteratively up to 10 times
            for (int i = 0; i < 10; i++)
            {
                List<MenuItemWithOptions> leftItems = new List<MenuItemWithOptions>();

                foreach (var item in items)
                {
                    bool valid = item.Impacts.All((impactsItem) => processed.Contains(impactsItem));

                    if (valid)
                    {
                        processed.Add(item);
                        yield return item;
                    }
                    else
                    {
                        leftItems.Add(item);
                    }
                }

                if (leftItems.Count() == 0)
                    yield break;

                items = leftItems;
            }

            CommonHelpers.Log.TraceLine("PowerControl: Failed to order items: {0}",
                string.Join(", ", items.Select((item) => item.Name)));

            foreach (var item in items)
            {
                yield return item;
            }
        }
    }
}
