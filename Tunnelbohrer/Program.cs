using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {


        /*
 * R e a d m e
 * -----------
 * 
 * In this file you can include any instructions or other comments you want to have injected onto the 
 * top of your final script. You can safely delete this file if you do not want any such comments.
 */

        List<IMyShipDrill> d = new List<IMyShipDrill>();
        List<IMyPistonBase> p = new List<IMyPistonBase>();
        IMyPistonBase piston_hinten, piston_vorne;
        IMyShipConnector connector_vorne, connector_hinten;
        IMyShipMergeBlock mergeblock_vorne, mergeblock_hinten;
        IMyInteriorLight light;
        IMyTextPanel lcdCargo;
        IMyProjector projector_hinten, projector_vorne;


        int start, stop;
        String connected;
        String currentStep;
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            IMyBlockGroup drills = GridTerminalSystem.GetBlockGroupWithName("TunnelbohrerDrills");
            drills.GetBlocksOfType<IMyShipDrill>(d, drill => drill.Enabled = true);

            IMyBlockGroup piston_vorschub = GridTerminalSystem.GetBlockGroupWithName("TunnelbohrerPistonVorschub");
            piston_vorschub.GetBlocksOfType<IMyPistonBase>(p);

            connector_hinten = GridTerminalSystem.GetBlockWithName("TunnelbohrerConnectorHinten") as IMyShipConnector;
            connector_vorne = GridTerminalSystem.GetBlockWithName("TunnelbohrerConnectorVorne") as IMyShipConnector;
            mergeblock_vorne = GridTerminalSystem.GetBlockWithName("TunnelbohrerMergeBlockVorne") as IMyShipMergeBlock;
            mergeblock_hinten = GridTerminalSystem.GetBlockWithName("TunnelbohrerMergeBlockHinten") as IMyShipMergeBlock;

            piston_hinten = GridTerminalSystem.GetBlockWithName("TunnelbohrerPistonHinten") as IMyPistonBase;
            piston_vorne = GridTerminalSystem.GetBlockWithName("TunnelbohrerPistonVorne") as IMyPistonBase;
            piston_hinten.MaxLimit = 2.0f;
            piston_vorne.MaxLimit = 2.0f;

            projector_hinten = GridTerminalSystem.GetBlockWithName("TunnelbohrerProjectorHinten") as IMyProjector;
            projector_vorne = GridTerminalSystem.GetBlockWithName("TunnelbohrerProjectorVorne") as IMyProjector;

            light = GridTerminalSystem.GetBlockWithName("Tunnelbohrer lightOnOff") as IMyInteriorLight;
            lcdCargo = GridTerminalSystem.GetBlockWithName("Tunnelbohrer [LCD]") as IMyTextPanel;

            connected = "Connected";

            currentStep = "STEP_1";
                 

            public void Save()
            {

            }

            public void Main()
            {
                string cargoStand = lcdCargo.GetText();

                start = cargoStand.IndexOf("]") + 2;
                stop = cargoStand.Length - start - 1;
                String str = cargoStand.Substring(start, stop);
                float cargoStandInt = Single.Parse(str);

                Echo("Current Cargo: " + cargoStandInt);
                Echo("Current Stage: " + currentStep);

                switch (currentStep)
                {
                    case "STEP_1":

                        if (light.Enabled && (cargoStandInt <= 50f))
                        {
                            if (mergeblock_hinten.IsConnected && (mergeblock_vorne.IsConnected == false))
                            {
                                projector_hinten.Enabled = true;

                                setVelocity(p, 0.2f);

                                if (getCurrentPos(p) == 29.5f)
                                {
                                    projector_hinten.Enabled = false;
                                    currentStep = "STEP_2";
                                }
                            }
                            else
                            {
                                fehlerTunnelbohrer(currentStep);
                            }
                        }

                        else
                        {
                            setVelocity(p, 0f);
                            fehlerTunnelbohrer(currentStep);
                        }
                        break;

                    case "STEP_2":
                        if (mergeblock_vorne.IsConnected)
                        {
                            connector_vorne.Connect();
                            currentStep = "STEP_3";
                        }
                        else
                        {
                            mergeblock_vorne.Enabled = true;
                            if (piston_vorne.CurrentPosition < 2f)
                            {
                                piston_vorne.Velocity = 0.5f;
                            }
                        }
                        break;

                    case "STEP_3":
                        if (mergeblock_hinten.IsConnected && piston_hinten.CurrentPosition <= 1)
                        {
                            connector_hinten.Disconnect();
                            mergeblock_hinten.Enabled = false;
                            piston_hinten.Velocity = -0.5f;
                        }
                        else
                        {
                            currentStep = "STEP_4";
                        }
                        break;

                    case "STEP_4":

                        if (getCurrentPos(p) > 0f)
                        {
                            projector_vorne.Enabled = true;
                            setVelocity(p, -0.1f);
                        }
                        else
                        {
                            projector_vorne.Enabled = false;
                            currentStep = "STEP_5";
                        }
                        break;

                    case "STEP_5":
                        if (mergeblock_hinten.IsConnected)
                        {
                            connector_hinten.Connect();
                            currentStep = "STEP_3";
                        }
                        else
                        {
                            mergeblock_hinten.Enabled = true;
                            if (piston_hinten.CurrentPosition < 2f)
                            {
                                piston_hinten.Velocity = 0.5f;
                            }
                        }

                        break;

                    case "STEP_6":

                        if (mergeblock_vorne.IsConnected && piston_vorne.CurrentPosition <= 1)
                        {
                            connector_vorne.Disconnect();
                            mergeblock_vorne.Enabled = false;
                            piston_vorne.Velocity = -0.5f;
                        }
                        else
                        {
                            currentStep = "STEP_1";
                        }
                        break;
                }
            }
            public void setVelocity(List<IMyPistonBase> pistList, float velocity)
            {
                foreach (IMyPistonBase pist in pistList)
                {
                    pist.Velocity = velocity;
                }
            }

            public float getCurrentPos(List<IMyPistonBase> pistList)
            {
                float pos = 0;
                foreach (IMyPistonBase pist in pistList)
                {
                    pos += pist.CurrentPosition;
                }
                return pos;
            }

            public void fehlerTunnelbohrer(String Stufe)
            {
                Echo("Fehler in " + Stufe);

            }
        }

        

    }

}
