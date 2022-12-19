namespace PowerControl.Menu
{
    public class MenuItemSeparator : MenuItem
    {
        private ToolStripItem toolStripItem = new ToolStripSeparator();

        public MenuItemSeparator()
        {
            Selectable = false;
        }

        public override void CreateMenu(System.Windows.Forms.ContextMenuStrip contextMenu)
        {
            contextMenu.Items.Add(toolStripItem);
        }

        public override string Render(MenuItem? selected)
        {
            return Color("---", Colors.Blue);
        }

        public override void SelectNext(int change)
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
