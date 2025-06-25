using System.Reflection;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace FireflyOverrideTest
{
    class FireflyReflection
    {
		public static FireflyReflection Instance { get; private set; }

		public Assembly fireflyAssembly;
		public Dictionary<string, FieldInfo> fieldCache = new Dictionary<string, FieldInfo>();
		public Dictionary<string, MethodInfo> methodCache = new Dictionary<string, MethodInfo>();

		// holder for the ConfigManager instance and type
		public object configMgr;
		public Type configMgrType;

		// holder for the AtmoFxModule instance and type
		public object fxModule;
		public Type fxModuleType;
		
		public FireflyReflection()
		{
			Instance = this;
		}

		public void Initialize(Vessel vessel)
		{
			// find the AtmoFxModule in the vessel
			foreach (VesselModule module in vessel.vesselModules)
			{
				Type type = module.GetType();

				if (type.Name == "AtmoFxModule")
				{
					fxModuleType = type;
					fxModule = module;
				}
			}

			// load the Firefly assembly
			AssemblyLoader.LoadedAssembly kspAssembly = AssemblyLoader.loadedAssemblies.FirstOrDefault(a => a.name.Equals("Firefly", StringComparison.OrdinalIgnoreCase));
			fireflyAssembly = kspAssembly.assembly;

			// find ConfigManager
			configMgrType = fireflyAssembly.GetType("Firefly.ConfigManager");
			configMgr = configMgrType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);

			// cache the fields and methods
			FindFields();
			FindMethods();
		}

		void FindFields()
		{
			FieldInfo overridePhysicsField = fxModuleType.GetField("overridePhysics", BindingFlags.Public | BindingFlags.Instance);
			FieldInfo overrideDataField = fxModuleType.GetField("overrideData", BindingFlags.Public | BindingFlags.Instance);

			fieldCache.Add("overridePhysics", overridePhysicsField);
			fieldCache.Add("overrideData", overrideDataField);

			// iterate through the override data fields and add them to the cache
			IterateFields(overrideDataField);
		}

		// recursively iterates child fields
		void IterateFields(FieldInfo parent, int iter = 0)
		{
			if (iter > 2) return;  // limit recursion

			// get the parent field type
			Type parentType = parent.FieldType;

			// iterate through the public fields
			FieldInfo[] fields = parentType.GetFields(BindingFlags.Public | BindingFlags.Instance);
			for (int i = 0; i < fields.Length; i++)
			{
				FieldInfo field = fields[i];

				// only add fields that are declared in the parent type (no inherited fields)
				if (field.DeclaringType != parentType) continue;
				fieldCache.Add(parent.Name + '.' + field.Name, field);

				// check if the child field has more child fields
				FieldInfo[] childFields = field.FieldType.GetFields(BindingFlags.Public | BindingFlags.Instance);
				if (childFields.Length > 0)
				{
					// iterate through them recursively
					IterateFields(field, iter + 1);
				}
			}
		}

		void FindMethods()
		{
			MethodInfo createVesselFx = fxModuleType.GetMethod("CreateVesselFx", BindingFlags.Public | BindingFlags.Instance);
			methodCache.Add("CreateVesselFx", createVesselFx);

			MethodInfo tryGetBodyConfig = configMgrType.GetMethod("TryGetBodyConfig", BindingFlags.Public | BindingFlags.Instance);
			methodCache.Add("TryGetBodyConfig", tryGetBodyConfig);
		}

		public bool TryGetBodyConfig(string name, bool fallback, out object config)
		{
			if (configMgr == null)
			{
				config = null;
				return false;
			}

			object output = null;
			object[] parameters = new object[] { name, fallback, output };

			bool success = (bool)methodCache["TryGetBodyConfig"].Invoke(configMgr, parameters);

			config = parameters[2];
			return success;
		}

		public void CreateVesselFx()
		{
			if (fxModule == null) return;

			methodCache["CreateVesselFx"].Invoke(fxModule, null);
		}

		public object GetField(string key)
		{
			string[] tokens = key.Split('.');

			if (tokens.Length == 1)
			{
				fieldCache[key].GetValue(fxModule);
			}
			else if (tokens.Length == 2)
			{
				// if the key is a nested field, we need to get the parent field first
				object parentValue = fieldCache[tokens[0]].GetValue(fxModule);

				// now we can get the child field
				return fieldCache[key].GetValue(parentValue);
			}

			return null;
		}

		public void SetField(string key, object value)
		{
			string[] tokens = key.Split('.');

			if (tokens.Length == 1)
			{
				fieldCache[key].SetValue(fxModule, value);
			}
			else if (tokens.Length == 2)
			{
				// if the key is a nested field, we need to get the parent field first
				object parentValue = fieldCache[tokens[0]].GetValue(fxModule);

				// now we can set the child field
				fieldCache[key].SetValue(parentValue, value);
				return;
			}
		}
	}
}
