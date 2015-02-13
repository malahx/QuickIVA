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
using UnityEngine;

namespace QuickIVA {

	public class Quick : MonoBehaviour {

		public readonly static string VERSION = "1.01";
		public readonly static string MOD = "QuickIVA";
		private static bool isdebug = true;

		internal static void Log(string msg) {
			if (isdebug) {
				Debug.Log (MOD + "(" + VERSION + "): " + msg);
			}
		}
		internal static void Warning(string msg) {
			if (isdebug) {
				Debug.LogWarning (MOD + "(" + VERSION + "): " + msg);
			}
		}
	}

	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class QuickIVA : QIVA {

		private void Awake() {
			QSettings.Instance.Load ();
			if (QSettings.Instance.Enabled) {
				GameEvents.onVesselChange.Add (OnVesselChange);
				GameEvents.onLaunch.Add (OnLaunch);
				GameEvents.onGameSceneLoadRequested.Add (OnGameSceneLoadRequested);
				GameEvents.onShowUI.Add (OnShowUI);
				GameEvents.onCrewBoardVessel.Add (OnCrewBoardVessel);
				GameEvents.onCrewOnEva.Add (OnCrewOnEva);
			}
		}

		private void OnDestroy() {
			GameEvents.onVesselChange.Remove (OnVesselChange);
			GameEvents.onLaunch.Remove (OnLaunch);
			GameEvents.onGameSceneLoadRequested.Remove (OnGameSceneLoadRequested);
			GameEvents.onShowUI.Remove (OnShowUI);
			GameEvents.onCrewBoardVessel.Remove (OnCrewBoardVessel);
			GameEvents.onCrewOnEva.Add (OnCrewOnEva);
		}

		private void OnVesselChange(Vessel vessel) {
			BlockedGoIVA = false;
			DisableALL (false);
		}

		// Go IVA at Launch
		private void OnLaunch(EventReport EventReport) {
			if (QSettings.Instance.IVAatLaunch) {
				GoIVA (FlightGlobals.ActiveVessel);
			}
		}

		// Disable all things at Scene Load Request
		private void OnGameSceneLoadRequested(GameScenes GameScene) {
			DisableALL (false);
		}

		// Disable UI
		private void OnShowUI() {
			if (QSettings.Instance.DisableShowUIonIVA) {
				if (isIVA) {
					hasShowUI = true;
				}
			}
		}

		// Which kerbal go to EVA
		private void OnCrewOnEva(GameEvents.FromToAction<Part, Part> part) {
			ScrMsg (true, part.to.protoModuleCrew[0].KerbalRef);
		}

		// Select the good kerbal on IVA after an EVA
		private void OnCrewBoardVessel(GameEvents.FromToAction<Part, Part> part) {
			ProtoCrewMember _pcrew = part.to.protoModuleCrew.Find (item => item.KerbalRef.crewMemberName == part.from.vessel.vesselName);
			if (_pcrew != null) {
				EVAKerbal = _pcrew.KerbalRef;
			}
		}

		// Disable third person view on CAMERA_MODE, CAMERA_NEXT, FOCUS_NEXT_VESSEL, FOCUS_PREV_VESSEL
		private void FixedUpdate() {
			if (QSettings.Instance.Enabled) {
				if (HighLogic.LoadedSceneIsFlight) {
					if (FlightGlobals.ready && !FlightDriver.Pause) {
						Vessel _vessel = FlightGlobals.ActiveVessel;
						if (GameSettings.CAMERA_MODE.GetKeyDown ()) {
							DisableThirdPersonVesselFixed (true);
						}
						if (GameSettings.CAMERA_NEXT.GetKeyDown ()) {
							if (VesselSeats (_vessel).Count == 1) {
								DisableThirdPersonVesselFixed (true);
							}
						}
						if (GameSettings.FOCUS_NEXT_VESSEL.GetKeyDown () || GameSettings.FOCUS_PREV_VESSEL.GetKeyDown ()) {
							if (VesselIsAlone (_vessel)) {
								DisableThirdPersonVesselFixed (true);
							}
						}
					}
				}
			}
		}

		// Keyboard shortcuts
		private void Update() {
			if (QSettings.Instance.KeyEnabled) {
				if (HighLogic.LoadedSceneIsFlight) {
					if (FlightGlobals.ready && !FlightDriver.Pause) {
						if (Input.GetKeyDown (QSettings.Instance.KeyRecovery)) {
							GoRecovery (FlightGlobals.ActiveVessel);
						}
						if (Input.GetKeyDown (QSettings.Instance.KeyEVA)) {
							GoEVA (FlightGlobals.ActiveVessel);
						}
					}
				}
			}
		}

		// Disable third person view, Go IVA on Load, Which Kerbal you are on IVA
		private void LateUpdate() {
			if (QSettings.Instance.Enabled) {
				if (HighLogic.LoadedSceneIsFlight) {
					DisableThirdPersonVesselFixed (false);
					if (FlightGlobals.ready && !FlightDriver.Pause) {
						if (IVAIsInstantiate && !isIVA && !isMAP) {
							Vessel _vessel = FlightGlobals.ActiveVessel;
							if (CheckVessel (_vessel)) {
								GoIVA (_vessel);
							}
						}
						if (isIVA && GameSettings.CAMERA_NEXT.GetKeyDown ()) {
							Vessel _vessel = FlightGlobals.ActiveVessel;
							if (VesselSeats (_vessel).Count > 1) {
								if (CheckEVAUnlocked (_vessel)) {
									Kerbal _kerbal = CheckIVAKerbal (_vessel);
									if (_kerbal != null) {
										Log (string.Format ("IVA switch to {0}({1}) experienceTrait: {2}, partName: ({3}){4}", _kerbal.crewMemberName, _kerbal.protoCrewMember.seatIdx, _kerbal.protoCrewMember.experienceTrait.Title, _kerbal.protoCrewMember.seat.part.partInfo.category, _kerbal.protoCrewMember.seat.part.name));
										ScrMsg (false, _kerbal);
									} else {
										Log ("Can't find the current Kerbal");
									}
								}
							}
						}
						ShowOrHideUI ();
					}
				}
			}
		}
	}
}