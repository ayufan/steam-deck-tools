using System.Text;

namespace PowerControl.Menu
{
    public class MenuRoot : MenuItem
    {
        public IList<MenuItem> Items { get; } = new List<MenuItem>();
        public MenuItem? Selected;
        public event Action VisibleChanged;

        public MenuRoot()
        {
            VisibleChanged += delegate { };
        }

        public MenuItem? this[String name]
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
            VisibleChanged();
        }

        public override string Render(MenuItem? parentSelected)
        {
            var sb = new StringBuilder();

            sb.AppendJoin("", OSDHelpers);
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
            VisibleChanged();
            return true;
        }

        public void Next(int change)
        {
            if (Show())
                return;

            int index = -1;
            if (Selected is not null)
                index = Items.IndexOf(Selected);
            if (index < 0 && change < 0)
                index = Items.Count; // Select last item if want to iterate down

            for (int i = 0; i < Items.Count; i++)
            {
                index = (index + change + Items.Count) % Items.Count;
                var item = Items[index];
                if (item.Visible && item.Selectable)
                {
                    Selected = item;
                    VisibleChanged();
                    return;
                }
            }
        }

        public override void SelectNext(int change)
        {
            if (Show())
                return;

            if (Selected != null)
            {
                Selected.SelectNext(change);
                VisibleChanged();
            }
        }

        public MenuItem? Select(String name)
        {
            Selected = this[name];
            if (Selected is null)
                return null;

            Show();
            VisibleChanged();
            return Selected;
        }
    }
}
