namespace PowerControl.Menu
{
    public abstract class MenuItem
    {
        public static readonly String[] OSDHelpers =
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

        public String Name { get; set; } = "";
        public bool Visible { get; set; } = true;
        public bool Selectable { get; set; }

        protected string Color(String text, Colors index)
        {
            return String.Format("<C{1}>{0}<C>", text, (int)index);
        }

        public abstract string Render(MenuItem? selected);

        public abstract void CreateMenu(System.Windows.Forms.ContextMenuStrip contextMenu);
        public abstract void Update();
        public abstract void Reset();

        public abstract void SelectNext(int change);
    }
}
