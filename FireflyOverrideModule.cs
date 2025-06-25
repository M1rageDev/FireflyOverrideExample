using UnityEngine;

namespace FireflyOverrideTest
{
    class FireflyOverrideModule : PartModule
    {
		FireflyReflection reflection;

		bool overrideActive = false;

		public override void OnStart(StartState state)
		{
			if (state == StartState.Editor)
			{
				// don't do anything in the editor
				return;
			}

			reflection = new FireflyReflection();

			// initialize the reflection stuff
			reflection.Initialize(vessel);
		}

		public override void OnUpdate()
		{
			if ((!overrideActive) || reflection.fxModule == null) return;

			// sets the override effect direction based on the vessel's surface velocity
			// note that this is in world-space, not in any relative space like surface or orbit
			reflection.SetField("overrideData.entryDirection", (Vector3)vessel.srf_velocity.normalized);
		}

		[KSPEvent(active = true, guiActive = true, guiActiveEditor = false, guiName = "Toggle Firefly Effects")]
		public void ToggleFireflyEffects()
        {
			// don't do anything if no reference to fxmodule
			if (reflection.fxModule == null) return;
			
			// toggle the effect
			overrideActive = !overrideActive;

			// set the override state in the Firefly module
			reflection.SetField("overridePhysics", overrideActive);
			if (overrideActive)
			{
				// fill the override data with basic values
				reflection.SetField("overrideData.overridenBy", part.name);
				reflection.SetField("overrideData.entryDirection", (Vector3)vessel.srf_velocity.normalized);
				reflection.SetField("overrideData.angleOfAttack", 0f);

				// set the effect strength and state
				// the strength is not in a range of 0-1, but pretty much goes from 0-4000, and 2100-2800 is a good range
				reflection.SetField("overrideData.effectStrength", 2800f);

				// the state is in a range of 0-1 though, so we can set it directly
				// it represents whether the effects are for mach effects (0) or atmospheric entry (1)
				reflection.SetField("overrideData.effectState", 1f);

				// set the body config
				// this method gets the config, and fallbacks to the default if it doesn't exist
				reflection.TryGetBodyConfig("test_config", true, out var cfg);
				reflection.SetField("overrideData.bodyConfig", cfg);

				// calling this function starts the effects
				// (Firefly's effects are disabled outside of the atmosphere, so they need to be re-enabled like this)
				reflection.CreateVesselFx();
			}
		}
	}
}
