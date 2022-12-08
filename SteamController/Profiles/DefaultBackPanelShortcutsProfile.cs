namespace SteamController.Profiles
{
    public abstract class DefaultBackPanelShortcutsProfile : DefaultGuideShortcutsProfile
    {
        internal abstract ProfilesSettings.BackPanelSettings BackPanelSettings { get; }

        public override Status Run(Context c)
        {
            if (base.Run(c).IsDone)
            {
                return Status.Done;
            }

            BackPanelShortcuts(c);

            return Status.Continue;
        }

        protected virtual void BackPanelShortcuts(Context c)
        {
            var settings = BackPanelSettings;

            c.Keyboard[settings.L4_KEY] = c.Steam.BtnL4;
            c.Keyboard[settings.L5_KEY] = c.Steam.BtnL5;
            c.Keyboard[settings.R4_KEY] = c.Steam.BtnR4;
            c.Keyboard[settings.R5_KEY] = c.Steam.BtnR5;
        }
    }
}
