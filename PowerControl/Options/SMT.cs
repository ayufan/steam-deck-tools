using CommonHelpers;
using PowerControl.Helpers;

namespace PowerControl.Options
{
    public static class SMT
    {
        public static Menu.MenuItemWithOptions Instance = new Menu.MenuItemWithOptions()
        {
            Name = "SMT",
            PersistentKey = "SMT",
            ApplyDelay = 500,
            Options = { "No", "Yes" },
            ResetValue = () => { return "Yes"; },
            CurrentValue = delegate ()
            {
                if (!OSDHelpers.IsOSDForeground(out var processId))
                    return null;
                if (!ProcessorCores.HasSMTThreads())
                    return null;

                return ProcessorCores.IsUsingSMT(processId) ? "Yes" : "No";
            },
            ApplyValue = (selected) =>
            {
                if (!OSDHelpers.IsOSDForeground(out var processId))
                    return null;
                if (!ProcessorCores.HasSMTThreads())
                    return null;

                ProcessorCores.SetProcessSMT(processId, selected.ToString() == "Yes");

                return ProcessorCores.IsUsingSMT(processId) ? "Yes" : "No";
            }
        };
    }
}
