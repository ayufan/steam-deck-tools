using hidapi;
using PowerControl.External;
using static CommonHelpers.Log;

namespace SteamController.Devices
{
    public partial class SteamController
    {
        public SteamAction?[] AllActions { get; private set; }
        public SteamButton?[] AllButtons { get; private set; }
        public SteamAxis?[] AllAxises { get; private set; }

        public IEnumerable<SteamButton> HoldButtons
        {
            get
            {
                foreach (var action in AllButtons)
                {
                    if (action.Value)
                        yield return action;
                }
            }
        }

        private void InitializeActions()
        {
            var allActions = GetType().
                GetFields().
                Where((field) => field.FieldType.IsSubclassOf(typeof(SteamAction))).
                Select((field) => Tuple.Create(field, field.GetValue(this) as SteamAction)).
                ToList();

            allActions.ForEach((tuple) => tuple.Item2.Controller = this);
            allActions.ForEach((tuple) => tuple.Item2.Name = tuple.Item1.Name);

            AllActions = allActions.Select((tuple) => tuple.Item2).ToArray();
            AllAxises = allActions.Where((tuple) => tuple.Item2 is SteamAxis).Select((tuple) => tuple.Item2 as SteamAxis).ToArray();
            AllButtons = allActions.Where((tuple) => tuple.Item2 is SteamButton).Select((tuple) => tuple.Item2 as SteamButton).ToArray();
        }

        public IEnumerable<string> GetReport()
        {
            List<string> report = new List<string>();

            var buttons = AllButtons.Where((button) => button.Value).Select((button) => button.Name);
            if (buttons.Any())
                yield return String.Format("Buttons: {0}", String.Join(",", buttons));

            foreach (var axis in AllAxises)
            {
                if (!axis.Active)
                    continue;
                yield return String.Format("Axis: {0} = {1} [Delta: {2}]", axis.Name, axis.Value, axis.Value - axis.LastValue);
            }
        }
    }
}
