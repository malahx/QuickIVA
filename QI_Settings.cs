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
using System.IO;
using UnityEngine;

namespace QuickIVA {

	public class QSettings : MonoBehaviour {

		public readonly static QSettings Instance = new QSettings();

		internal string FileConfig = KSPUtil.ApplicationRootPath + "GameData/" + Quick.MOD + "/Config.txt";

		[Persistent]
		public bool Enabled = true;
		[Persistent]
		public bool IVAatLaunch = false;
		[Persistent]
		public bool AutoHideUI = true;
		[Persistent]
		public bool DisableThirdPersonVessel = true;
		[Persistent]
		public bool DisableMapView = false;
		[Persistent]
		public bool DisableShowUIonIVA = true;
		[Persistent]
		public bool StockToolBar = true;
		[Persistent]
		public bool BlizzyToolBar = true;
		[Persistent]
		public bool KeyEnabled = true;
		[Persistent]
		public string KeyRecovery = "end";
		[Persistent]
		public string KeyEVA = "home";

		public void Save() {
			ConfigNode _temp = ConfigNode.CreateConfigFromObject(this, new ConfigNode());
			_temp.Save(FileConfig);
			Quick.Log ("Settings Saved");
		}
		public void Load() {
			if (File.Exists (FileConfig)) {
				try {
					ConfigNode _temp = ConfigNode.Load (FileConfig);
					ConfigNode.LoadObjectFromConfig (this, _temp);
				} catch {
					Save ();
				}
				Quick.Log ("Settings Loaded");
			} else {
				Save ();
			}
		}
	}
}