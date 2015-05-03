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
using System.Collections;
using UnityEngine;

namespace QuickIVA {
	[KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
	public class QStockToolbar : MonoBehaviour {

		[KSPField(isPersistant = true)] internal static QBlizzyToolbar BlizzyToolbar;
	
		internal static bool Enabled {
			get {
				return QSettings.Instance.StockToolBar;
			}
		}

		private ApplicationLauncher.AppScenes AppScenes = ApplicationLauncher.AppScenes.SPACECENTER;
		private static string TexturePath = Quick.MOD + "/Textures/StockToolBar";

		private void OnClick() { 
			QGUI.Settings ();
		}
			
		private Texture2D GetTexture {
			get {
				return GameDatabase.Instance.GetTexture(TexturePath, false);
			}
		}

		[KSPField(isPersistant = true)] private ApplicationLauncherButton appLauncherButton;

		internal static bool isAvailable {
			get {
				return ApplicationLauncher.Ready && ApplicationLauncher.Instance != null;
			}
		}

		internal static QStockToolbar Instance {
			get;
			private set;
		}

		internal void Awake() {
			Instance = this;
			QGUI.Awake ();
			if (BlizzyToolbar == null) BlizzyToolbar = new QBlizzyToolbar ();
			GameEvents.onGUIApplicationLauncherDestroyed.Add (AppLauncherDestroyed);
			GameEvents.onGameSceneLoadRequested.Add (AppLauncherDestroyed);
			GameEvents.onGUIApplicationLauncherUnreadifying.Add (AppLauncherDestroyed);
		}

		internal void Start() {
			if (!HighLogic.LoadedSceneIsGame) {
				return;
			}
			QSettings.Instance.Load ();
			BlizzyToolbar.Start ();
			StartCoroutine (AppLauncherReady ());
		}
			
		internal IEnumerator AppLauncherReady() {
			if (!Enabled || !HighLogic.LoadedSceneIsGame) {
				yield break;
			}
			while (!isAvailable) {
				yield return 0;
			}
			Init ();
		}

		internal void AppLauncherDestroyed(GameScenes gameScenes) {
			AppLauncherDestroyed ();
		}

		internal void AppLauncherDestroyed() {
			Destroy ();
		}

		internal void OnDestroy() {
			BlizzyToolbar.OnDestroy ();
			GameEvents.onGUIApplicationLauncherDestroyed.Remove (AppLauncherDestroyed);
			GameEvents.onGameSceneLoadRequested.Remove (AppLauncherDestroyed);
			GameEvents.onGUIApplicationLauncherUnreadifying.Remove (AppLauncherDestroyed);
		}

		private void Init() {
			if (!isAvailable) {
				return;
			}
			if (appLauncherButton == null) {
				appLauncherButton = ApplicationLauncher.Instance.AddModApplication (OnClick, OnClick, null, null, null, null, AppScenes, GetTexture);
			}
		}

		private void Destroy() {
			if (!isAvailable) {
				return;
			}
			if (appLauncherButton != null) {
				ApplicationLauncher.Instance.RemoveModApplication (appLauncherButton);
				appLauncherButton = null;
			}
		}

		internal void Set(bool SetTrue, bool force = false) {
			if (!isAvailable) {
				return;
			}
			if (appLauncherButton != null) {
				if (SetTrue) {
					if (appLauncherButton.State == RUIToggleButton.ButtonState.FALSE) {
						appLauncherButton.SetTrue (force);
					}
				} else {
					if (appLauncherButton.State == RUIToggleButton.ButtonState.TRUE) {
						appLauncherButton.SetFalse (force);
					}
				}
			}
		}

		internal void Reset() {
			if (appLauncherButton != null) {
				Set (false);
				if (!Enabled) {
					Destroy ();
				}
			}
			if (Enabled) {
				Init ();
			}
		}

		private void OnGUI() {
			QGUI.OnGUI ();
		}
	}
}