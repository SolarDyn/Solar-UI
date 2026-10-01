// GameBindings.cs
// Solar Dynamics 2026

#region

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Solar.UI;

public class GameBindings
{
    public class Units
    {
        public static bool IsImperial()
        {
            try
            {
                return PlayerSettings.unitSystem == PlayerSettings.UnitSystem.Imperial;
            }
            catch (Exception e)
            {
                Plugin.Log(e.ToString());
                return false;
            }
        }

        public static float ConvertAltitude_ToDisplay(float meters)
        {
            return IsImperial() ? meters * 3.28084f : meters;
        }

        public static float ConvertVerticalSpeed_ToDisplay(float metersPerSecond)
        {
            return IsImperial() ? metersPerSecond * 196.850394f : metersPerSecond;
        }

        public static float ConvertSpeed_ToDisplay(float metersPerSecond)
        {
            return IsImperial() ? metersPerSecond * 1.94384f : metersPerSecond * 3.6f;
        }

        public static float ConvertSpeed_FromDisplay(float displayValue)
        {
            return IsImperial() ? displayValue / 1.94384f : displayValue / 3.6f;
        }

        public static string GetAltitudeUnit()
        {
            return IsImperial() ? "ft" : "m";
        }

        public static string GetVerticalSpeedUnit()
        {
            return IsImperial() ? "fpm" : "m/s";
        }

        public static string GetSpeedUnit()
        {
            return IsImperial() ? "kts" : "km/h";
        }
    }

    public class GameState
    {
        public static bool IsGamePaused()
        {
            try
            {
                return GameplayUI.GameIsPaused;
            }
            catch (NullReferenceException e)
            {
                Plugin.Log(e.ToString());
                return false;
            }
        }

        public static FactionHQ GetCurrentFactionHQ()
        {
            try
            {
                return Player.Aircraft.GetAircraft().NetworkHQ;
            }
            catch (NullReferenceException e)
            {
                Plugin.Log(e.ToString());
                return null;
            }
        }

        public static bool IsChatboxActive()
        {
            try
            {
                return SceneSingleton<MessageUI>.i.chat.isActiveAndEnabled;
            }
            catch (NullReferenceException e)
            {
                Plugin.Log(e.ToString());
                return false;
            }
        }
    }

    public class Player
    {
        public class Aircraft
        {
            public static global::Aircraft GetAircraft(bool silent = false)
            {
                try
                {
                    return SceneSingleton<CombatHUD>.i.aircraft;
                }
                catch (NullReferenceException e)
                {
                    if (!silent)
                        Plugin.Log(e.ToString());
                    return null;
                }
            }

            public static string GetPlatformName()
            {
                try
                {
                    return SceneSingleton<CombatHUD>.i.aircraft.GetAircraftParameters().aircraftName;
                }
                catch (NullReferenceException e)
                {
                    Plugin.Log(e.ToString());
                    return "Unknown";
                }
            }

            public static void ToggleAutoControl()
            {
                try
                {
                    SceneSingleton<CombatHUD>.i.ToggleAutoControl();
                }
                catch (NullReferenceException e)
                {
                    Plugin.Log(e.ToString());
                }
            }

            public static bool IsRadarJammed()
            {
                try
                {
                    global::Aircraft aircraft = GetAircraft();
                    if (!aircraft) return false;
                    TargetDetector targetDetector = aircraft.radar;
                    if (targetDetector is Radar radar) return radar.IsJammed();
                    return false;
                }
                catch (Exception e)
                {
                    Plugin.Log(e.ToString());
                    return false;
                }
            }

            public class Countermeasures
            {
                private static List<CountermeasureManager.CountermeasureStation> GetStationsList()
                {
                    try
                    {
                        CountermeasureManager currentManager =
                            SceneSingleton<CombatHUD>.i.aircraft.countermeasureManager;
                        return SceneSingleton<CombatHUD>.i.aircraft.countermeasureManager.countermeasureStations;
                    }
                    catch (NullReferenceException e)
                    {
                        Plugin.Log(e.ToString());
                        return null;
                    }
                }

                public static int GetCurrentIndex()
                {
                    try
                    {
                        return SceneSingleton<CombatHUD>.i.aircraft.countermeasureManager.activeIndex;
                    }
                    catch (NullReferenceException e)
                    {
                        Plugin.Log(e.ToString());
                        return -1;
                    }
                }

                public static int GetIRFlareAmmo()
                {
                    try
                    {
                        List<CountermeasureManager.CountermeasureStation> stationsList = GetStationsList();
                        CountermeasureManager.CountermeasureStation IRStation = stationsList[HasECMPod() ? 1 : 0];
                        int count = IRStation.ammo;
                        return count;
                    }
                    catch (NullReferenceException e)
                    {
                        Plugin.Log(e.ToString());
                        return 0;
                    }
                }

                public static int GetIRFlareMaxAmmo()
                {
                    try
                    {
                        List<CountermeasureManager.CountermeasureStation> stationsList = GetStationsList();
                        CountermeasureManager.CountermeasureStation IRStation = stationsList[HasECMPod() ? 1 : 0];
                        List<Countermeasure> countermeasuresList = IRStation.countermeasures;
                        FlareEjector ejectorStation = (FlareEjector)countermeasuresList[0];
                        int maxCount = ejectorStation.GetMaxAmmo();
                        return maxCount;
                    }
                    catch (NullReferenceException e)
                    {
                        Plugin.Log(e.ToString());
                        return 0;
                    }
                }

                public static int GetJammerAmmo()
                {
                    try
                    {
                        List<CountermeasureManager.CountermeasureStation> stationsList = GetStationsList();
                        CountermeasureManager.CountermeasureStation JammerStation = stationsList[HasECMPod() ? 0 : 1];
                        List<Countermeasure> countermeasuresList = JammerStation.countermeasures;
                        RadarJammer jammerStation = (RadarJammer)countermeasuresList[0];
                        PowerSupply supply = jammerStation.powerSupply;
                        int charge = (int)(supply.GetCharge() * 100f);
                        return charge;
                    }
                    catch (NullReferenceException e)
                    {
                        Plugin.Log(e.ToString());
                        return 0;
                    }
                }

                public static bool HasIRFlare()
                {
                    try
                    {
                        List<CountermeasureManager.CountermeasureStation> stationsList = GetStationsList();
                        return stationsList.Count > 0;
                    }
                    catch (NullReferenceException e)
                    {
                        Plugin.Log(e.ToString());
                        return false;
                    }
                }

                public static bool HasECMPod()
                {
                    try
                    {
                        List<CountermeasureManager.CountermeasureStation> stationsList = GetStationsList();

                        if (stationsList != null && stationsList.Count > 0)
                        {
                            CountermeasureManager.CountermeasureStation firstStation = stationsList[0];
                            List<Countermeasure> countermeasuresList = firstStation.countermeasures;

                            if (countermeasuresList != null && countermeasuresList.Count > 0)
                                return countermeasuresList[0] is RadarJammer;
                        }

                        return false;
                    }
                    catch (Exception e)
                    {
                        Plugin.Log(e.ToString());
                        return false;
                    }
                }

                public static bool HasJammer()
                {
                    try
                    {
                        List<CountermeasureManager.CountermeasureStation> stationsList = GetStationsList();
                        return stationsList.Count > 1;
                    }
                    catch (NullReferenceException e)
                    {
                        Plugin.Log(e.ToString());
                        return false;
                    }
                }

                public static bool IsFlareSelected()
                {
                    try
                    {
                        if (HasECMPod())
                            return GetCurrentIndex() == 1;
                        return GetCurrentIndex() == 0;
                    }
                    catch (NullReferenceException e)
                    {
                        Plugin.Log(e.ToString());
                        return false;
                    }
                }

                public static void SetIRFlare()
                {
                    try
                    {
                        if (HasECMPod())
                            SceneSingleton<CombatHUD>.i.aircraft.countermeasureManager.activeIndex = 1;
                        else
                            SceneSingleton<CombatHUD>.i.aircraft.countermeasureManager.activeIndex = 0;
                    }
                    catch (NullReferenceException e)
                    {
                        Plugin.Log(e.ToString());
                    }
                }

                public static void SetJammer()
                {
                    try
                    {
                        if (HasECMPod())
                            SceneSingleton<CombatHUD>.i.aircraft.countermeasureManager.activeIndex = 0;
                        else
                            SceneSingleton<CombatHUD>.i.aircraft.countermeasureManager.activeIndex = 1;
                    }
                    catch (NullReferenceException e)
                    {
                        Plugin.Log(e.ToString());
                    }
                }
            }

            public class Weapons
            {
                public static string GetActiveStationName()
                {
                    try
                    {
                        string name = SceneSingleton<CombatHUD>.i.aircraft.weaponManager.currentWeaponStation.WeaponInfo
                            .shortName;
                        if (name == "")
                            return SceneSingleton<CombatHUD>.i.aircraft.weaponManager.currentWeaponStation.WeaponInfo
                                .weaponName;

                        return name;
                    }
                    catch (NullReferenceException e)
                    {
                        Plugin.Log(e.ToString());
                        return "Unknown Weapon";
                    }
                }

                public static int GetActiveStationAmmo()
                {
                    try
                    {
                        if (GetStationCount() == 0)
                            return 0;
                        return SceneSingleton<CombatHUD>.i.aircraft.weaponManager.currentWeaponStation.Ammo;
                    }
                    catch (NullReferenceException e)
                    {
                        Plugin.Log(e.ToString());
                        return 0;
                    }
                }

                public static string GetActiveStationAmmoString()
                {
                    try
                    {
                        if (GetStationCount() == 0)
                            return "0";
                        return SceneSingleton<CombatHUD>.i.aircraft.weaponManager.currentWeaponStation.GetAmmoReadout();
                    }
                    catch (NullReferenceException e)
                    {
                        Plugin.Log(e.ToString());
                        return "0";
                    }
                }

                public static float GetActiveStationReloadProgress()
                {
                    try
                    {
                        return SceneSingleton<CombatHUD>.i.aircraft.weaponManager.currentWeaponStation
                            .GetReloadStatusMax();
                    }
                    catch (NullReferenceException e)
                    {
                        Plugin.Log(e.ToString());
                        return 0f;
                    }
                }

                public static WeaponStatus GetWeaponStatus()
                {
                    try
                    {
                        CombatHUD currentCombatHUD = SceneSingleton<CombatHUD>.i;
                        GameObject topRightPanel = currentCombatHUD.topRightPanel;
                        WeaponStatus weaponStatus = topRightPanel.GetComponentInChildren<WeaponStatus>();
                        return weaponStatus;
                    }
                    catch (NullReferenceException e)
                    {
                        Plugin.Log(e.ToString());
                        return null;
                    }
                    catch (IndexOutOfRangeException)
                    {
                        return null;
                    } // this means the game is paused and there is no weapon status displayed
                }

                public static Image GetActiveStationImage()
                {
                    try
                    {
                        WeaponStatus currentWeaponStatus = GetWeaponStatus();
                        Image weaponImage = currentWeaponStatus.weaponImage;
                        return weaponImage;
                    }
                    catch (NullReferenceException e)
                    {
                        Plugin.Log(e.ToString());
                        return null;
                    }
                }

                public static string GetStationNameByIndex(int index)
                {
                    try
                    {
                        if (index < GetStationCount())
                        {
                            if (SceneSingleton<CombatHUD>.i.aircraft.weaponStations[index].WeaponInfo.shortName == "")
                                return SceneSingleton<CombatHUD>.i.aircraft.weaponStations[index].WeaponInfo.weaponName;
                            return SceneSingleton<CombatHUD>.i.aircraft.weaponStations[index].WeaponInfo.shortName;
                        }

                        Plugin.Log("[BD] Station index out of range !");
                        return null;
                    }
                    catch (NullReferenceException e)
                    {
                        Plugin.Log(e.ToString());
                        return "Unknown Weapon";
                    }
                }

                public static int GetStationAmmoByIndex(int index)
                {
                    try
                    {
                        if (index < GetStationCount())
                            return SceneSingleton<CombatHUD>.i.aircraft.weaponStations[index].Ammo;

                        Plugin.Log("[BD] Station index out of range !");
                        return 0;
                    }
                    catch (NullReferenceException e)
                    {
                        Plugin.Log(e.ToString());
                        return 0;
                    }
                }

                public static int GetStationMaxAmmoByIndex(int index)
                {
                    try
                    {
                        if (index < GetStationCount())
                            return SceneSingleton<CombatHUD>.i.aircraft.weaponStations[index].FullAmmo;

                        Plugin.Log("[BD] Station index out of range !");
                        return 0;
                    }
                    catch (NullReferenceException e)
                    {
                        Plugin.Log(e.ToString());
                        return 0;
                    }
                }

                public static int GetStationCount()
                {
                    try
                    {
                        return SceneSingleton<CombatHUD>.i.aircraft.weaponStations.Count;
                    }
                    catch (NullReferenceException e)
                    {
                        Plugin.Log(e.ToString());
                        return 5;
                    }
                }

                public static void SetActiveStation(byte index)
                {
                    try
                    {
                        global::Aircraft aircraft = GetAircraft();

                        if (aircraft == null)
                            return;

                        if (index >= aircraft.weaponStations.Count)
                        {
                            Plugin.Log($"[BD] Station index {index} out of range!");
                            return;
                        }

                        aircraft.SetActiveStation(index);
                        SceneSingleton<CombatHUD>.i.ShowWeaponStation(aircraft.weaponStations[index]);
                    }
                    catch (NullReferenceException e)
                    {
                        Plugin.Log(e.ToString());
                    }
                }
            }
        }

        public class TargetList
        {
            public static void AddTargets(List<Unit> units, bool muteSound = false)
            {
                try
                {
                    CombatHUD currentCombatHUD = SceneSingleton<CombatHUD>.i;
                    Dictionary<Unit, HUDUnitMarker> markerLookup = currentCombatHUD.markerLookup;
                    AudioClip selectSound = currentCombatHUD.selectSound;
                    List<Unit> currentTargets = [.. units];
                    currentTargets.Reverse();
                    foreach (Unit t_unit in currentTargets)
                        if (markerLookup.ContainsKey(t_unit))
                        {
                            markerLookup[t_unit].SelectMarker();
                            Aircraft.GetAircraft().weaponManager.AddTargetList(t_unit);
                        }

                    if (!muteSound)
                        SoundManager.PlayInterfaceOneShot(selectSound);
                }
                catch (NullReferenceException e)
                {
                    Plugin.Log(e.ToString());
                }
            }

            public static void DeselectAll()
            {
                try
                {
                    SceneSingleton<CombatHUD>.i.DeselectAll();
                }
                catch (NullReferenceException e)
                {
                    Plugin.Log(e.ToString());
                }
            }

            public static void DeselectUnit(Unit unit)
            {
                try
                {
                    SceneSingleton<CombatHUD>.i.DeSelectUnit(unit);
                }
                catch (NullReferenceException e)
                {
                    Plugin.Log(e.ToString());
                }
            }

            public static List<Unit> GetTargets()
            {
                try
                {
                    CombatHUD currentCombatHUD = SceneSingleton<CombatHUD>.i;
                    List<Unit> targetList = currentCombatHUD.targetList;
                    return [.. targetList];
                }
                catch (NullReferenceException e)
                {
                    Plugin.Log(e.ToString());
                    return [];
                }
            }
        }
    }
}