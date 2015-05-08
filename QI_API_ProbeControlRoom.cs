/* 
QuickIVA
Copyright 2015 Malah

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>. 
*/

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace QuickIVA {

	public class QProbeControlRoom {
		internal static bool isLoaded = false;
		internal static bool WasActive = false;
		private static object Instance;
		private static MethodInfo Method_startIVA;
		private static PropertyInfo Property_vesselCanStockIVA;
		private static FieldInfo Field_isActive;

		internal static void Awake() {
			AssemblyLoader.LoadedAssembyList _assemblies = AssemblyLoader.loadedAssemblies;
			Type _type = null;
			foreach (var _assembly in _assemblies) {
				if (_assembly.name == "ProbeControlRoom") {
					Type[] types = _assembly.assembly.GetExportedTypes ();
					foreach (Type type in types) {
						if (type.FullName == "ProbeControlRoom.ProbeControlRoom") {
							_type = type;
							break;
						}
					}
				}
			}
			if (_type != null) {
				Instance = Activator.CreateInstance(_type);
				if (Instance != null) {
					Method_startIVA = _type.GetMethod ("startIVA", new Type[]{ }, new ParameterModifier[]{ });
					Field_isActive = _type.GetField ("isActive", BindingFlags.Static | BindingFlags.Public);
					Property_vesselCanStockIVA = _type.GetProperty ("vesselCanStockIVA");
					if (Method_startIVA == null) {
						Quick.Warning ("No ProbeControlRoom.startIVA()");
						return;
					}
					if (Property_vesselCanStockIVA == null) {
						Quick.Warning ("No ProbeControlRoom.vesselCanStockIVA");
						return;
					}
					if (Field_isActive == null) {
						Quick.Warning ("No ProbeControlRoom.isActive");
						return;
					}
					isLoaded = true;
				} else {
					Quick.Warning ("No ProbeControlRoom.Instance");
				}
			} else {
				Quick.Warning ("No ProbeControlRoom.Assembly");
			}
		}

		internal static bool startIVA() {
			if (!isLoaded || Instance == null) {
				return false;
			}
			return (bool)Method_startIVA.Invoke (Instance, null);
		}

		internal static bool vesselCanStockIVA {
			get {
				if (!isLoaded) {
					return true;
				}
				return (bool)Property_vesselCanStockIVA.GetValue (null, null);
			}
		}

		internal static bool isActive {
			get {
				if (!isLoaded) {
					return false;
				}
				return (bool)Field_isActive.GetValue(null);
			}
			set {
				Field_isActive.SetValue (null, value);
			}
		}
	}
}