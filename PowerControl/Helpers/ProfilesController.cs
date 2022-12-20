﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonHelpers;
using PowerControl.Helper;
using PowerControl.Helpers;
using PowerControl.Menu;

namespace PowerControl.Helpers
{
    public static class ProfilesController
    {
        private const string FpsKey = "FPSLimit";
        private const string RefreshRateKey = "RefreshRate";
        private const string IsTroubledKey = "IsTroubled";
        private const string DefaultName = "Default";

        private static string CurrentGame = string.Empty;
        private static ProfileSettings DefaultSettings = new ProfileSettings(DefaultName);
        private static ProfileSettings? CurrentSettings;
        private static string[] troubledGames = { "dragonageinquisition" };
        private static bool IsWriteLocked = false;

        private static System.Windows.Forms.Timer? timer; 

        public static void Initialize()
        {
            Options.RefreshRate.Instance?.SetValueChanged((_, _, newValue) =>
            {
                SetValue(RefreshRateKey, newValue);
            });

            Options.FPSLimit.Instance?.SetValueChanged((_, _, newValue) =>
            {
                SetValue(FpsKey, newValue);
            });

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000;
            timer.Tick += (_, _) =>
            {
                timer.Stop();

                RefreshProfiles();

                timer.Start();
            };
            timer.Start();
        }

        private static void RefreshProfiles()
        {
            if (!DeviceManager.IsDeckOnlyDisplay())
            {
                CurrentGame = string.Empty;
                return;
            }

            string? gameName = RTSS.GetCurrentGameName();

            if (gameName == null && CurrentGame != DefaultName)
            {
                CurrentGame = DefaultName;
                CurrentSettings = null;

                ApplyProfile();
            }

            if (gameName != null && CurrentGame != gameName)
            {
                CurrentGame = gameName;
                CurrentSettings = ProfileSettings.CheckIfExists(CurrentGame) ?
                    new ProfileSettings(CurrentGame) : null;

                ApplyProfile();
            }
        }

        private static void ApplyProfile()
        {
            IsWriteLocked = true;

            if (GetBoolValue(IsTroubledKey))
            {
                Thread.Sleep(6500);
            }

            Options.RefreshRate.Instance?.Set(GetValue(RefreshRateKey), true);
            Options.FPSLimit.Instance?.Set(GetValue(FpsKey), true);

            IsWriteLocked = false;
        }

        private static void SetBoolValue(string key, bool value)
        {
            var settings = CurrentSettings ?? DefaultSettings;

            settings.Set(key, value);
        }

        private static bool GetBoolValue(string key)
        {
            var settings = CurrentSettings ?? DefaultSettings;

            return settings.Get(key, false);
        }

        private static void SetValue(string key, string value)
        {
            if (IsWriteLocked)
            {
                return;
            }

            var settings = CurrentSettings ?? DefaultSettings;
            settings.Set(key, value);
        }

        private static string GetValue(string key)
        {
            if (CurrentSettings == null)
            {
                return GetDefaultValue(key);
            }

            return CurrentSettings.Get(key, GetDefaultValue(key));
        }

        private static string GetDefaultValue(string key)
        {
            return DefaultSettings.Get(key, GetOptionByKey(key)?.ResetValue?.Invoke() ?? string.Empty);
        }

        private static MenuItemWithOptions? GetOptionByKey(string key)
        {
            switch (key)
            {
                case FpsKey:
                    return Options.FPSLimit.Instance;
                case RefreshRateKey:
                    return Options.RefreshRate.Instance;
            }

            return null;
        }
    }
}