using Spectrum.API;
using Spectrum.API.Interfaces.Plugins;
using Spectrum.API.Interfaces.Systems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Spectrum.API.Configuration;
using System.IO;
using Harmony;
using System.Reflection;
using Events.Car;
using Events.LocalClient;
using Events;
using Events.Stunt;
using Events.Local;

namespace StupidHorn
{
    public class Entry : IPlugin
    {
		static float buttonDownTimer_;
		static bool buttonReleasedSinceLastHornTriggered_ = true;
        static float multiplier = 20;
	
        public void Initialize(IManager manager, string ipcIdentifier)
        {
            var harmony = HarmonyInstance.Create("com.Larnin.StupidHorn");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
		
		[HarmonyPatch(typeof(HornGadget), "GadgetUpdateLocal")]
        internal class HornGadgetGadgetUpdateLocal
        {
            static bool Prefix(HornGadget __instance, LocalPlayerControlledCar carLogicLocal, InputStates inputStates, float dt)
            {
                var playerEvents = __instance.GetType().GetField("playerEvents_", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance) as PlayerEvents;
				bool isPressed_ = inputStates.GetState(InputAction.Horn).isPressed_;
				bool isReleased_ = inputStates.GetState(InputAction.Horn).isReleased_;
				if (isPressed_)
					Entry.buttonDownTimer_ += dt;
					
				if (Entry.buttonReleasedSinceLastHornTriggered_ && (Entry.buttonDownTimer_ >= __instance.maxButtonDownTime_ || isReleased_))
				{
					float percent = Mathf.InverseLerp(0f, __instance.maxButtonDownTime_, Entry.buttonDownTimer_) * Entry.multiplier;
					Entry.buttonReleasedSinceLastHornTriggered_ = false;
					Vector3 vector = __instance.transform.position;
					vector += __instance.transform.lossyScale.z * __instance.bubbleOffset_;
                    playerEvents.Broadcast<Horn.Data>(new Horn.Data(percent, vector));
				}
				if (isReleased_)
				{
                    Entry.buttonReleasedSinceLastHornTriggered_ = true;
                    Entry.buttonDownTimer_ = 0f;
				}
				return false;
			}
		}
    }
}
