namespace PowerControl.Menu
{
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
}
