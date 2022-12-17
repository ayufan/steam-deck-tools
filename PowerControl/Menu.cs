using PowerControl.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Forms.VisualStyles;
using System.Xml.Schema;

namespace PowerControl
{
    internal class Menu
    {
        public static readonly String[] Helpers =
        {
            "<C0=008040><C1=0080C0><C2=C08080><C3=FF0000><C4=FFFFFF><C250=FF8000>",
            "<A0=-4><A1=5><A2=-2><A5=-5><S0=-50><S1=50>",
        };

        public enum Colors : int
        {
            Green,
            Blue,
            Redish,
            Red,
            White
        }

        public abstract class MenuItem
        {
            public String Name { get; set; }
            public bool Visible { get; set; } = true;
            public bool Selectable { get; set; }

            protected string Color(String text, Colors index)
            {
                return String.Format("<C{1}>{0}<C>", text, (int)index);
            }

            public abstract string Render(MenuItem selected);

            public abstract void CreateMenu(ToolStripItemCollection collection);
            public abstract void Update();
            public abstract void Reset();

            public abstract void SelectNext();
            public abstract void SelectPrev();
        };

        public class MenuItemSeparator : MenuItem
        {
            private ToolStripItem toolStripItem;

            public MenuItemSeparator()
            {
                Selectable = false;
            }

            public override void CreateMenu(ToolStripItemCollection collection)
            {
                if (toolStripItem != null)
                    return;

                toolStripItem = new ToolStripSeparator();
                collection.Add(toolStripItem);
            }

            public override string Render(MenuItem selected)
            {
                return Color("---", Colors.Blue);
            }

            public override void SelectNext()
            {
            }

            public override void SelectPrev()
            {
            }

            public override void Update()
            {
            }

            public override void Reset()
            {
            }
        }

        public class MenuItemWithOptions : MenuItem
        {
            public delegate object CurrentValueDelegate();
            public delegate object[] OptionsValueDelegate();
            public delegate object ApplyValueDelegate(object selected);

            public IList<Object> Options { get; set; } = new List<Object>();
            public Object? SelectedOption { get; set; }
            public Object ActiveOption { get; set; }
            public int ApplyDelay { get; set; }
            public bool CycleOptions { get; set; } = true;
            public GameOptions Key { get; set; } = GameOptions.None;

            public CurrentValueDelegate CurrentValue { get; set; }
            public OptionsValueDelegate OptionsValues { get; set; }
            public ApplyValueDelegate ApplyValue { get; set; }
            public CurrentValueDelegate ResetValue { get; set; }

            private System.Windows.Forms.Timer delayTimer;
            private ToolStripMenuItem toolStripItem;

            public MenuItemWithOptions()
            {
                this.Selectable = true;

                GameProfilesController.Subscribe((profile) => {
                    int? option = profile.GetByKey(Key);
                    if (option != null)
                    {
                        SelectIndex((int)option, true);
                    }
                });
            }

            public override void Reset()
            {
                if (ResetValue == null)
                    return;

                var resetOption = ResetValue();
                if (resetOption == null || resetOption.Equals(ActiveOption))
                    return;

                SelectedOption = resetOption;
                onApply();
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
                    {
                        Options = result.ToList();
                        updateOptions();
                    }
                    else
                    {
                        Visible = false;
                    }
                }

                if (ActiveOption == null && Options.Count > 0)
                    ActiveOption = Options.First();

                onUpdateToolStrip();
            }

            private void scheduleApply(bool isUserInvoked = false)
            {
                if (delayTimer != null)
                    delayTimer.Stop();

                if (ApplyDelay == 0)
                {
                    onApply(isUserInvoked);
                    return;
                }

                delayTimer = new System.Windows.Forms.Timer();
                delayTimer.Interval = ApplyDelay > 0 ? ApplyDelay : 1;
                delayTimer.Tick += delegate (object? sender, EventArgs e)
                {
                    if (delayTimer != null)
                        delayTimer.Stop();

                    onApply(isUserInvoked);
                };
                delayTimer.Enabled = true;
            }

            private void onApply(bool isUserInvoked = false)
            {
                if (ApplyValue != null)
                    ActiveOption = ApplyValue(SelectedOption);
                else
                    ActiveOption = SelectedOption;

                SelectedOption = null;

                if (Key != GameOptions.None && isUserInvoked)
                {
                    GameProfilesController.SetValueByKey(Key, Options.IndexOf(ActiveOption));
                }

                onUpdateToolStrip();
            }

            private void onUpdateToolStrip()
            {
                if (toolStripItem == null)
                    return;

                foreach (ToolStripMenuItem item in toolStripItem.DropDownItems)
                    item.Checked = Object.Equals(item.Tag, SelectedOption ?? ActiveOption);

                toolStripItem.Visible = Visible && Options.Count > 0;
            }

            private void updateOptions()
            {
                if (toolStripItem == null)
                    return;

                toolStripItem.DropDownItems.Clear();

                foreach (var option in Options)
                {
                    var optionItem = new ToolStripMenuItem(option.ToString());
                    optionItem.Tag = option;
                    optionItem.Click += delegate (object? sender, EventArgs e)
                    {
                        SelectedOption = option;

                        onApply();
                    };
                    toolStripItem.DropDownItems.Add(optionItem);
                }
            }

            public override void CreateMenu(ToolStripItemCollection collection)
            {
                if (toolStripItem != null)
                    return;

                toolStripItem = new ToolStripMenuItem();
                toolStripItem.Text = Name;
                updateOptions();
                collection.Add(toolStripItem);
            }

            private void SelectIndex(int index, bool isImmediate = false)
            {
                if (Options.Count == 0)
                    return;

                int selectedIndex = Math.Clamp(index, 0, Options.Count - 1);
                SelectedOption = Options[selectedIndex];

                if (isImmediate)
                {
                    onApply();
                }
                else
                {
                    scheduleApply(true);
                }
            }

            public override void SelectNext()
            {
                int index = Options.IndexOf(SelectedOption ?? ActiveOption);
                if (index < 0)
                    SelectIndex(0); // select first
                else if (CycleOptions)
                    SelectIndex((index + 1) % Options.Count);
                else
                    SelectIndex(index + 1);
            }

            public override void SelectPrev()
            {
                int index = Options.IndexOf(SelectedOption ?? ActiveOption);
                if (index < 0)
                    SelectIndex(Options.Count - 1); // select last
                else if (CycleOptions)
                    SelectIndex((index - 1 + Options.Count) % Options.Count);
                else
                    SelectIndex(index - 1);
            }

            private String optionText(Object option)
            {
                String text;

                if (option == null)
                    text = Color("?", Colors.White);
                else if (Object.Equals(option, SelectedOption ?? ActiveOption))
                    text = Color(option.ToString(), Colors.Red);
                else if (Object.Equals(option, ActiveOption))
                    text = Color(option.ToString(), Colors.White);
                else
                    text = Color(option.ToString(), Colors.Green);

                return text;
            }

            public override string Render(MenuItem selected)
            {
                string output = "";

                if (selected == this)
                    output += Color(Name + ":", Colors.White).PadRight(30);
                else
                    output += Color(Name + ":", Colors.Blue).PadRight(30);

                output += optionText(SelectedOption ?? ActiveOption);

                if (SelectedOption != null && !Object.Equals(ActiveOption, SelectedOption))
                    output += " (active: " + optionText(ActiveOption) + ")";

                return output;
            }
        }

        public class MenuRoot : MenuItem
        {
            public IList<MenuItem> Items { get; set; } = new List<MenuItem>();

            public MenuItem Selected;

            public delegate void VisibleChangedDelegate();
            public VisibleChangedDelegate? VisibleChanged;

            public MenuItem this[String name]
            {
                get
                {
                    foreach (var item in Items)
                    {
                        if (item.Name == name)
                            return item;
                    }
                    return null;
                }
            }

            public override void CreateMenu(ToolStripItemCollection collection)
            {
                foreach (var item in Items)
                    item.CreateMenu(collection);
            }
            public override void Update()
            {
                foreach (var item in Items)
                    item.Update();
            }

            public override void Reset()
            {
                foreach (var item in Items)
                    item.Reset();

                if (VisibleChanged != null)
                    VisibleChanged();
            }

            public override string Render(MenuItem parentSelected)
            {
                var sb = new StringBuilder();

                sb.AppendJoin("", Helpers);
                if (Name != "")
                    sb.AppendLine(Color(Name, Colors.Blue));

                foreach (var item in Items)
                {
                    if (!item.Visible)
                        continue;
                    var lines = item.Render(Selected).Split("\r\n").Select(line => "  " + line);
                    foreach (var line in lines)
                        sb.AppendLine(line);
                }

                return sb.ToString();
            }

            public bool Show()
            {
                if (Visible)
                    return false;

                Visible = true;
                Update();

                if (VisibleChanged != null)
                    VisibleChanged();
                return true;
            }

            public void Prev()
            {
                if (Show())
                    return;

                int index = Items.IndexOf(Selected);
                if (index < 0)
                    index = Items.Count; // select last item

                for (int i = 0; i < Items.Count; i++)
                {
                    index = (index - 1 + Items.Count) % Items.Count;
                    var item = Items[index];
                    if (item.Visible && item.Selectable)
                    {
                        Selected = item;
                        if (VisibleChanged != null)
                            VisibleChanged();
                        return;
                    }
                }
            }

            public void Next()
            {
                if (Show())
                    return;

                int index = Items.IndexOf(Selected);
                if (index < 0)
                    index = -1; // select first item

                for (int i = 0; i < Items.Count; i++)
                {
                    index = (index + 1) % Items.Count;
                    var item = Items[index];
                    if (item.Visible && item.Selectable)
                    {
                        Selected = item;
                        if (VisibleChanged != null)
                            VisibleChanged();
                        return;
                    }
                }
            }

            public override void SelectNext()
            {
                if (Show())
                    return;

                if (Selected != null)
                {
                    Selected.SelectNext();
                    if (VisibleChanged != null)
                        VisibleChanged();
                }
            }

            public void SelectNext(String name)
            {
                var item = this[name];
                if (item is null)
                    return;

                Show();
                Selected = item;
                item.SelectNext();
                if (VisibleChanged != null)
                    VisibleChanged();
            }

            public override void SelectPrev()
            {
                if (Show())
                    return;

                if (Selected != null)
                {
                    Selected.SelectPrev();
                    if (VisibleChanged != null)
                        VisibleChanged();
                }
            }

            public void SelectPrev(String name)
            {
                var item = this[name];
                if (item is null)
                    return;

                Show();
                Selected = item;
                item.SelectPrev();
                if (VisibleChanged != null)
                    VisibleChanged();
            }
        }
    }
}
