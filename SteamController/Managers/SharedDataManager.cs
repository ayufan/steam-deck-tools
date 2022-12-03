using CommonHelpers;

namespace SteamController.Managers
{
    public sealed class SharedDataManager : Manager
    {
        SharedData<SteamControllerSetting> sharedData = SharedData<SteamControllerSetting>.CreateNew();

        public override void Tick(Context context)
        {
            if (sharedData.GetValue(out var value) && value.DesiredProfile != "")
            {
                context.SelectProfile(value.DesiredProfile);
            }

            sharedData.SetValue(new SteamControllerSetting()
            {
                CurrentProfile = context.CurrentProfile?.Name ?? "",
                SelectableProfiles = SelectableProfiles(context).JoinWithN(),
            });
        }

        private IEnumerable<String> SelectableProfiles(Context context)
        {
            return context.Profiles.
                Where((profile) => profile.Selected(context) || profile.Visible).
                Select((profile) => profile.Name);
        }
    }
}
