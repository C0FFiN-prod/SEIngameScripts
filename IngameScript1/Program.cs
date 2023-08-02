using Sandbox.Game.EntityComponents;
using Sandbox.Gui;
//using Sandbox.ModAPI;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        // This file contains your actual script.
        //
        // You can either keep all your code here, or you can create separate
        // code files to make your program easier to navigate while coding.
        //
        // In order to add a new utility class, right-click on your project, 
        // select 'New' then 'Add Item...'. Now find the 'Space Engineers'
        // category under 'Visual C# Items' on the left hand side, and select
        // 'Utility Class' in the main area. Name it in the box below, and
        // press OK. This utility class will be merged in with your code when
        // deploying your final script.
        //
        // You can also simply create a new utility class manually, you don't
        // have to use the template if you don't want to. Just do so the first
        // time to see what a utility class looks like.
        // 
        // Go to:
        // https://github.com/malware-dev/MDK-SE/wiki/Quick-Introduction-to-Space-Engineers-Ingame-Scripts
        //
        // to learn more about ingame scripts.
        IMyTextSurface debug = null;
        List<IMyGyro> gyros;
        IMyShipController controller = null;
        int k = 20;
        public Program()
        {
            gyros = new List<IMyGyro>();
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            Func<IMyTerminalBlock, bool> check = b => b.IsSameConstructAs(Me);
            debug = Me.GetSurface(0);
            Echo("Debug: " + (debug.Name ?? "none"));
            GridTerminalSystem.GetBlocksOfType<IMyShipController>(blocks, check);
            //controller = (IMyShipController)blocks.Find(b => ((IMyShipController)b).IsUnderControl); 
            try
            {
                controller = blocks.First() as IMyShipController;
            }catch(Exception) { }
            Echo("Controller: "+(controller!=null?controller.CustomName:"none")+"\n");
            GridTerminalSystem.GetBlocksOfType(gyros, check);
            Echo("Gyros: "+gyros.Count+"\n");
            if(gyros.Count>0) {
                foreach (var gyro in gyros)
                {
                    gyro.GyroOverride = true;
                }
            }
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {
            Vector3D gNormalized = controller.GetNaturalGravity().Normalized();
            MatrixD gyroMatrix, cMatrix = controller.WorldMatrix;
            Vector3D cUp = cMatrix.Up;
            Vector3D cLeft = cMatrix.Left;
            Vector3D cForward = cMatrix.Forward;
            double gUp = gNormalized.Dot(cUp);
            double gLeft = gNormalized.Dot(cLeft);
            double gForward = gNormalized.Dot(cForward);
            double roll, pitch, yaw;
            roll = Math.Atan2(gLeft,-gUp)*k;
            pitch = controller.RotationIndicator.X != 0 ? controller.RotationIndicator.X : (float)-Math.Atan2(gForward, -gUp)*k;
            yaw = controller.RotationIndicator.Y;
            Vector3D gyroInput = new Vector3D(pitch, yaw, roll), gI;
            foreach (var gyro in gyros)
            {
                gyroMatrix = cMatrix * MatrixD.Transpose(gyro.WorldMatrix);
                gI = Vector3D.Rotate(gyroInput, gyroMatrix);
                gyro.Pitch = (float)gI.X;
                gyro.Yaw = (float)gI.Y;
                gyro.Roll = (float)gI.Z;
            }
        }
    }
}
