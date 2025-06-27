using FireflyAPI;
using UnityEngine;

namespace FireflyOverrideTest
{
    class FireflyOverrideModule : PartModule
    {
		bool overrideActive = false;

		IFireflyModule fxModule;

		public override void OnStart(StartState state)
		{
			if (state == StartState.Editor)
			{
				// don't do anything in the editor
				return;
			}

			if (!FireflyAPIManager.IsFireflyInstalled())
			{
				// self destruct
				Destroy(this);
				return;
			}

			FireflyAPIManager.TryFindModule(vessel, out fxModule);
		}

		public override void OnUpdate()
		{
			if ((!overrideActive) || fxModule == null) return;

			// sets the override effect direction based on the vessel's surface velocity
			// note that this is in world-space, not in any relative space like surface or orbit
			fxModule.OverrideEntryDirection = vessel.srf_velocity.normalized;
		}

		[KSPEvent(active = true, guiActive = true, guiActiveEditor = false, guiName = "Toggle Firefly Effects")]
		public void ToggleFireflyEffects()
        {
			// don't do anything if no reference to fxmodule
			if (fxModule == null) return;
			
			// toggle the effect
			overrideActive = !overrideActive;

			// set the override state in the Firefly module
			fxModule.OverridePhysics = overrideActive;
			if (overrideActive)
			{
				// fill the override data with basic values
				fxModule.ResetOverride();
				fxModule.OverridenBy = part.name;
				fxModule.OverrideEntryDirection = vessel.srf_velocity.normalized;
				fxModule.OverrideAngleOfAttack = 0f;

				// set the effect strength and state
				// the strength is not in a range of 0-1, but pretty much goes from 0-4000, and 2100-2800 is a good range
				fxModule.OverrideEffectStrength = 2800f;

				// the state is in a range of 0-1 though, so we can set it directly
				// it represents whether the effects are for mach effects (0) or atmospheric entry (1)
				fxModule.OverrideEffectState = 1f;

				// set the body config
				// this method gets the config, and fallbacks to the default if it doesn't exist
				FireflyAPIManager.ConfigManager.TryGetBodyConfig("test_config", true, out var cfg);
				fxModule.OverrideBodyConfig = cfg;

				// calling this function starts the effects
				// (Firefly's effects are disabled outside of the atmosphere, so they need to be re-enabled like this)
				fxModule.CreateVesselFx();

				// calling this function reloads the effect colors
				// this is also needed in addition to the method above, since if the effects were already loaded (eg. in atmosphere), they won't change colors
				fxModule.ReloadCommandBuffer();
			}
		}
	}
}
