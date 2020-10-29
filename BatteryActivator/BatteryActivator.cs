using System;
using System.Collections.Generic;
using UnityEngine;

namespace BatteryActivator
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class BatteryActivator : MonoBehaviour
    {
        private static Rect windowPosition = new Rect(192, 96, 120, 48);
        private static GUIStyle windowStyle = null;
        private static float fixedWidth = 120;

        private static bool dismissed = false;
        private static Vessel vessel = null;

        private void OnGUI()
        {
            if (Event.current.type == EventType.Repaint || Event.current.isMouse)
            {
                windowStyle = new GUIStyle(HighLogic.Skin.window);

                if (FlightGlobals.ActiveVessel != null && vessel != FlightGlobals.ActiveVessel)
                {
                    vessel = FlightGlobals.ActiveVessel;
                    dismissed = false;
                }
            }

            drawGUI();
        }

        private void drawGUI()
        {
            try
            {
                if (!dismissed && ChargeUnavailable())
                {
                    windowPosition = GUILayout.Window(300671, windowPosition, OnWindow, "Battery Activator", GUILayout.Width(fixedWidth));
                }
            }
            catch (Exception ex)
            {
                print("BatteryActivator - exception: " + ex.Message);
            }
        }

        private void OnWindow(int windowID)
        {
            GUIStyle styleButton = new GUIStyle(GUI.skin.button);
            styleButton.normal.textColor = styleButton.focused.textColor = styleButton.hover.textColor = styleButton.active.textColor = Color.white;
            styleButton.padding = new RectOffset(0, 0, 0, 0);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Batteries On", styleButton, GUILayout.ExpandWidth(true)))
            {
                SwitchOn();
            }
            GUILayout.EndHorizontal();

            GUI.DragWindow();
        }

        private void SwitchOn()
        {
            foreach (Part part in FlightGlobals.ActiveVessel.parts)
            {
                foreach (PartResource resource in part.Resources)
                {
                    resource.flowState = true;
                }
            }
        }

        private bool ChargeUnavailable()
        {
            if (FlightGlobals.ActiveVessel != null && FlightGlobals.ActiveVessel.GetCrewCount() > 0)
            {
                List<ProtoCrewMember> crew = FlightGlobals.ActiveVessel.GetVesselCrew();

                foreach (ProtoCrewMember member in crew)
                {
                    if (member.trait != "Tourist")
                    {
                        return false;
                    }
                }
            }

            double availableAmount = 0.0;
            double totalAmount = 0.0;

            if (FlightGlobals.ActiveVessel != null)
            {
                foreach (Part part in FlightGlobals.ActiveVessel.parts)
                {
                    foreach (PartResource resource in part.Resources)
                    {
                        if (resource.resourceName == "ElectricCharge")
                        {
                            totalAmount += resource.amount;
                            if (resource.flowState)
                            {
                                availableAmount += resource.amount;
                            }
                        }
                    }
                }
            }

            return (availableAmount < 0.1f && totalAmount >= 0.1f);
        }
    }
}
