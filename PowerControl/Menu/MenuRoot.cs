using System.Text;

namespace PowerControl.Menu
{
    public class MenuRoot : MenuItem
    {
        public IList<MenuItem> Items { get; set; } = new List<MenuItem>();
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
            VisibleChanged();
        }

        public override void SelectPrev()
        {
            if (Show())
                return;

            if (Selected != null)
            {
                Selected.SelectPrev();
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
            VisibleChanged();
        }
    }
}