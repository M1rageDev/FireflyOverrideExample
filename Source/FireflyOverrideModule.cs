using FireflyAPI;
using UnityEngine;

namespace FireflyOverrideExample
{
	class FireflyOverrideModule : PartModule
	{
		[KSPField(guiActive = true, guiActiveEditor = false, guiName = "Power")]
		[UI_FloatRange(minValue = 0f, maxValue = 1f, stepIncrement = 0.01f)]
		public float power = 0f;

		bool overrideActive = false;
		bool previousActive = false;

		IFireflyModule fxModule;

		public override void OnStart(StartState state)
		{
			if (state == StartState.Editor)
			{
				// don't do anything in the editor
				return;
			}

			if (!FireflyAPIManager.IsFireflyInstalled)
			{
				// self destruct
				Destroy(this);
				return;
			}

			FireflyAPIManager.TryFindModule(vessel, out fxModule);
		}

		public override void OnUpdate()
		{
			if (fxModule == null) return;

			// handles if the override is active or not
			overrideActive = power > 0f;
			fxModule.OverridePhysics = overrideActive;
			if (overrideActive && !previousActive)
			{
				// firefly disables itself automatically, so we need to reconfigure the effects if going from off to on
				ConfigureEffects();
			}
			previousActive = overrideActive;

			// sets the data dynamically every frame
			fxModule.OverrideEntryDirection = vessel.srf_velocity.normalized;
			fxModule.OverrideEffectStrength = power * 2800f;
		}

		void ConfigureEffects()
		{
			// fill the override data with basic values
			fxModule.ResetOverride();
			fxModule.OverridenBy = part.name;
			fxModule.OverrideAngleOfAttack = 0f;

			// sets the override effect direction based on the vessel's surface velocity
			// note that this is in world-space, not in any relative space like surface or orbit
			fxModule.OverrideEntryDirection = vessel.srf_velocity.normalized;

			// set the effect strength and state
			// the strength is not in a range of 0-1, but pretty much goes from 0-4000, and 2100-2800 is a good range
			fxModule.OverrideEffectStrength = 2800f;

			// the state is in a range of 0-1 though, so we can set it directly
			// it represents whether the effects are for mach effects (0) or atmospheric entry (1)
			fxModule.OverrideEffectState = 1f;

			// set the body config
			// this property sets the body config by name, and fallbacks to the default if it doesn't exist
			fxModule.OverrideBodyConfigName = "test_config";

			// calling this function starts the effects
			// (Firefly's effects are disabled outside of the atmosphere, so they need to be re-enabled like this)
			fxModule.CreateVesselFx();

			// calling this function reloads the effect colors
			// this is also needed in addition to the method above, since if the effects were already loaded (eg. in atmosphere), they won't change colors
			fxModule.ReloadCommandBuffer();
		}
	}

	/*
    class FireflyOverrideModule : PartModule
    {
		[KSPField(isPersistant = true, guiActiveEditor = false, guiName = "Power")]
		[UI_FloatRange(minValue = 0f, maxValue = 1.0f, stepIncrement = 0.01f)]
		public float power = 0f;

		bool overrideActive = false;
		bool previousActive = false;

		IFireflyModule fxModule;

		public override void OnStart(StartState state)
		{
			if (state == StartState.Editor)
			{
				// don't do anything in the editor
				return;
			}

			if (!FireflyAPIManager.IsFireflyInstalled)
			{
				// self destruct
				Events["ToggleFireflyEffects"].active = false;
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

			fxModule.OverridePhysics = power > 0f;
			if (fxModule.OverridePhysics && !previousActive)
			{
				fxModule.CreateVesselFx();
			}
			previousOverridePhysics = fxModule.OverridePhysics;
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
				// this property sets the body config by name, and fallbacks to the default if it doesn't exist
				fxModule.OverrideBodyConfigName = "test_config";

				// calling this function starts the effects
				// (Firefly's effects are disabled outside of the atmosphere, so they need to be re-enabled like this)
				fxModule.CreateVesselFx();

				// calling this function reloads the effect colors
				// this is also needed in addition to the method above, since if the effects were already loaded (eg. in atmosphere), they won't change colors
				fxModule.ReloadCommandBuffer();
			}
		}
	}
	*/
}
