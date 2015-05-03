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
	public class QIVA : Quick {

		protected bool isGoneIVA = false;
		protected bool hasShowUI = false;
		protected bool NoMoreGoIVA = false;
		protected Kerbal EVAKerbal = null;
		protected ScreenMessage ScreenMsg = null;

		protected bool DisableThirdPersonVesselTmpPAUSE = false;

		protected bool BlockedGoIVA {
			get {
				return isGoneIVA || NoMoreGoIVA;
			}
			set {
				isGoneIVA = value;
				NoMoreGoIVA = value;
				if (!value) {
					hasShowUI = false;
				}
			}
		}

		protected bool IVAIsInstantiate {
			get {
				if (CameraManager.Instance == null) {
					return false;
				}
				return CameraManager.Instance.enabled;
			}
		}

		protected bool isIVA {
			get {
				if (!IVAIsInstantiate) {
					return false;
				}
				return CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.IVA;
			}
		}

		protected bool isMAP {
			get {
				if (!IVAIsInstantiate) {
					return false;
				}
				return CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.Map;
			}
		}

		protected Kerbal CheckIVAKerbal(Vessel vessel) {
			List<InternalSeat> _seats = VesselSeats (vessel);
			foreach (InternalSeat _seat in _seats) {
				Kerbal _kerbal = _seat.kerbalRef;
				if (_kerbal.eyeTransform == InternalCamera.Instance.transform.parent) {
					return _kerbal;
				}
			}
			return null;
		}

		protected bool CheckEVAUnlocked (Vessel vessel) {
			return GameVariables.Instance.EVAIsPossible(GameVariables.Instance.UnlockedEVA (ScenarioUpgradeableFacilities.GetFacilityLevel (SpaceCenterFacility.AstronautComplex)), vessel);
		}

		protected bool CheckVessel(Vessel vessel) {
			return !vessel.isEVA && !vessel.packed && (vessel.situation != Vessel.Situations.PRELAUNCH || !QSettings.Instance.IVAatLaunch);
		}

		protected bool KerbalIsOnVessel (Vessel vessel, Kerbal kerbal) {
			if (kerbal == null) {
				return false;
			}
			return vessel.GetVesselCrew ().Contains (kerbal.protoCrewMember);
		}

		protected List<InternalSeat> VesselSeats(Vessel vessel) {
			bool _hasOnlyPlaceholder;
			return VesselSeats (vessel, true, out _hasOnlyPlaceholder);
		}

		protected List<InternalSeat> VesselSeats(Vessel vessel, bool withPlaceholder, out bool hasOnlyPlaceholder) {
			int _index = 0;
			hasOnlyPlaceholder = true;
			List<Part> _parts = vessel.parts;
			List<InternalSeat> _result = new List<InternalSeat> ();
			foreach (Part _part in _parts) {
				if (_part.internalModel != null) {
					if (_part.internalModel.internalName != "Placeholder" || withPlaceholder) {
						hasOnlyPlaceholder = false;
						List<InternalSeat> _seats = _part.internalModel.seats;
						if (_seats.Count > 0) {
							foreach (InternalSeat _seat in _seats) {
								if (_seat.taken && _seat.kerbalRef != null) {
									Kerbal _kerbal = _seat.kerbalRef;
									if (_kerbal.state == Kerbal.States.ALIVE) {
										if (_part.partInfo.category == PartCategories.Pods) {
											_result.Insert (_index, _seat);
											_index++;
										} else {
											_result.Add (_seat);
										}
									}
								}
							}
						}
					}
				}
			}
			return _result;
		}
			
		protected bool VesselHasSeatTaken(Vessel vessel) {
			bool _hasOnlyPlaceholder;
			Kerbal IVAKerbal;
			return VesselHasSeatTaken(vessel, out IVAKerbal, out _hasOnlyPlaceholder);
		}

		protected bool VesselHasSeatTaken(Vessel vessel, out Kerbal IVAKerbal, out bool hasOnlyPlaceholder) {
			hasOnlyPlaceholder = true;
			bool _result = false;
			Kerbal _first = null;
			Kerbal _firstPilot = null;
			List<InternalSeat> _seats = VesselSeats (vessel);
			foreach (InternalSeat _seat in _seats) {
				Kerbal _kerbal = _seat.kerbalRef;
				if (_first == null) {
					_first = _kerbal;
				}
				if (_firstPilot == null && _kerbal.protoCrewMember.experienceTrait.Title == "Pilot") {
					_firstPilot = _kerbal;
				}
				_result = true;
			}
			IVAKerbal = (_firstPilot != null ? _firstPilot : _first);
			return _result;
		}

		protected bool VesselHasCrewAlive (Vessel vessel) {
			Kerbal _first;
			return  VesselHasCrewAlive (vessel, out _first);
		}

		protected bool VesselHasCrewAlive (Vessel vessel, out Kerbal first) {
			first = null;
			List<ProtoCrewMember> _crews = vessel.GetVesselCrew ();
			foreach (ProtoCrewMember protoCrewMember in _crews) {
				Kerbal _kerbal = protoCrewMember.KerbalRef;
				if (_kerbal.state == Kerbal.States.ALIVE) {
					first = _kerbal;
					return true;
				}
			}
			return false;
		}

		protected bool VesselIsAlone (Vessel vessel) {
			List<Vessel> _vessels = FlightGlobals.Vessels;
			foreach (Vessel _vessel in _vessels) {
				if (_vessel != vessel && _vessel.loaded) {
					return false;
				}
			}
			return true;
		}

		protected void DisableALL(bool enable) {
			//DisableShowUIonIVA (enable);
			//DisableThirdPersonVessel (enable);
			HideUIonIVA (enable);
			DisableMapView (enable);
		}

		// It bug the MapView
		/*internal void DisableShowUIonIVA(bool enable) {
			if (enable) {
				if (QSettings.Instance.AutoHideUI) {
					if (isGoneIVA && !InputLockManager.IsLocked ((ControlTypes)QSettings.idTOGGLEUI)) {
						GameEvents.onHideUI.Fire ();
						if (QSettings.Instance.DisableShowUIonIVA) {
							GameSettings.TOGGLE_UI.inputLockMask = QSettings.idTOGGLEUI;
							InputLockManager.SetControlLock ((ControlTypes)QSettings.idTOGGLEUI, MOD + "TOGGLEUI");
							Log ("Disable ShowUI on IVA");
						}
					}
				}
			} else {				
				if (QSettings.Instance.AutoHideUI) {
					GameEvents.onShowUI.Fire ();
				}
				if (InputLockManager.IsLocked ((ControlTypes)QSettings.idTOGGLEUI)) {
					InputLockManager.RemoveControlLock (MOD + "TOGGLEUI");
					GameSettings.TOGGLE_UI.inputLockMask = new ulong();
					Log ("Enable ShowUI on IVA");
				}
			}
		}*/

		protected void ShowOrHideUI() {
			if (QSettings.Instance.AutoHideUI) {
				if (!isIVA && !hasShowUI) {
					GameEvents.onShowUI.Fire ();
					hasShowUI = true;
					Log ("Show UI");
				} else if (isIVA && hasShowUI) {
					GameEvents.onHideUI.Fire ();
					hasShowUI = false;
					Log ("Hide UI on IVA");
				}
			}
		}

		protected void HideUIonIVA(bool enable) {
			if (QSettings.Instance.AutoHideUI) {
				if (isGoneIVA) {
					if (enable) {
						GameEvents.onHideUI.Fire ();
						Log ("Auto Hide UI");
					} else {				
						GameEvents.onShowUI.Fire ();
						Log ("Auto Show UI");
					}
				}
			}
		}

		protected void DisableMapView(bool enable) {
			if (enable) {
				if (QSettings.Instance.DisableMapView) {
					if (isGoneIVA && !InputLockManager.IsLocked (ControlTypes.MAP)) {
						InputLockManager.SetControlLock (ControlTypes.MAP, MOD + "MAP");
						Log ("Disable MapView shortcut");
					}
				}
			} else {
				if (InputLockManager.IsLocked (ControlTypes.MAP)) {
					InputLockManager.RemoveControlLock (MOD + "MAP");
					Log ("Enable MapView shortcut");
				}
			}
		}

		// It bug the MapView
		/*internal void DisableThirdPersonVessel(bool enable) {
			if (enable) {
				if (QSettings.Instance.DisableThirdPersonVessel) {
					if (isGoneIVA) {
						GameSettings.CAMERA_MODE.inputLockMask = QSettings.idCAMERAMODE;
						InputLockManager.SetControlLock ((ControlTypes)QSettings.idCAMERAMODE, MOD + "CAMERAMODES");
						if (ActiveVesselSeatsTakenCount == 1) {
							GameSettings.CAMERA_NEXT.inputLockMask = QSettings.idCAMERAMODE;
							Log ("Disable Camera Next (only one seat taken)");
						}
						Log ("Disable Third Person Vessel View");
					}
				}
			} else {
				if (InputLockManager.IsLocked ((ControlTypes)QSettings.idCAMERAMODE)) {
					InputLockManager.RemoveControlLock (MOD + "CAMERAMODES");
					GameSettings.CAMERA_MODE.inputLockMask = new ulong();
					GameSettings.CAMERA_NEXT.inputLockMask = new ulong();
					Log ("Enable Third Person Vessel View");
				}
			}
		}*/

		protected void DisableThirdPersonVesselFixed(bool enable) {
			if (enable) {
				if (QSettings.Instance.DisableThirdPersonVessel) {
					if (isGoneIVA) {
						if (!FlightDriver.Pause && isIVA) {
							FlightDriver.SetPause (true);
							DisableThirdPersonVesselTmpPAUSE = true;
							Warning ("You can't switch camera.");
						}
					}
				}
			} else {
				if (DisableThirdPersonVesselTmpPAUSE) {
					if (FlightDriver.Pause) {
						FlightDriver.SetPause (false);
						GameEvents.onHideUI.Fire ();
					}
					DisableThirdPersonVesselTmpPAUSE = false;
				}
			}
		}

		protected void ScrMsg(bool simple, Kerbal kerbal) {
			if (ScreenMsg != null) {
				ScreenMsg.duration = 0;
			}
			if (simple) {
				ScreenMsg = ScreenMessages.PostScreenMessage (string.Format ("{0} ({1})", kerbal.crewMemberName, kerbal.protoCrewMember.experienceTrait.Title), 5, ScreenMessageStyle.LOWER_CENTER);
			} else {
				ScreenMsg = ScreenMessages.PostScreenMessage (string.Format ("{1} ({2}){0}({3}) {4}", Environment.NewLine, kerbal.crewMemberName, kerbal.protoCrewMember.experienceTrait.Title, kerbal.protoCrewMember.seat.part.partInfo.category, kerbal.protoCrewMember.seat.part.name), 5, ScreenMessageStyle.LOWER_CENTER);
			}
		}

		protected void GoRecovery(Vessel vessel) {
			if (vessel.IsRecoverable) {
				GameEvents.OnVesselRecoveryRequested.Fire (vessel);
			} else {
				ScreenMessages.PostScreenMessage ("[QuickIVA] This vessel is net recoverable.", 5, ScreenMessageStyle.UPPER_CENTER);
			}
		}

		protected void GoEVA(Vessel vessel) {
			if (CheckEVAUnlocked (vessel)) {
				if (!vessel.isEVA && !vessel.packed) {
					if (vessel.GetCrewCount () > 0) {
						Kerbal _first;
						if (VesselHasCrewAlive (vessel, out _first)) {
							Kerbal _kerbal = (isIVA ? CheckIVAKerbal (vessel) : _first);
							FlightEVA.SpawnEVA (_kerbal);
							CameraManager.Instance.SetCameraFlight ();
							Log (string.Format ("GoEVA {0}({1}) experienceTrait: {2}", _kerbal.crewMemberName, _kerbal.protoCrewMember.seatIdx, _kerbal.protoCrewMember.experienceTrait.Title));
						} else {
							ScreenMessages.PostScreenMessage ("[QuickIVA] This vessel has no crew alive.", 5, ScreenMessageStyle.UPPER_CENTER);
						}
					} else {
						ScreenMessages.PostScreenMessage ("[QuickIVA] This vessel has no crew.", 5, ScreenMessageStyle.UPPER_CENTER);
					}
				} else {
					ScreenMessages.PostScreenMessage ("[QuickIVA] You can't EVA an EVA.", 5, ScreenMessageStyle.UPPER_CENTER);
				}
			} else {
				ScreenMessages.PostScreenMessage ("[QuickIVA] EVA is locked:" + GameVariables.Instance.GetEVALockedReason (vessel, vessel.GetVesselCrew()[0]), 5, ScreenMessageStyle.UPPER_CENTER);
			}
		}

		protected void GoIVA(Vessel vessel) {
			if (!BlockedGoIVA && HighLogic.CurrentGame.Parameters.Flight.CanIVA) {
				if (vessel.GetCrewCount () > 0) {
					if (VesselHasCrewAlive (vessel)) {
						bool _VesselhasOnlyPlaceholder;
						Kerbal _IVAKerbal;
						if (VesselHasSeatTaken (vessel, out _IVAKerbal, out _VesselhasOnlyPlaceholder)) {
							if (EVAKerbal != null) {
								if (vessel.GetVesselCrew ().Contains (EVAKerbal.protoCrewMember)) {
									_IVAKerbal = EVAKerbal;
								} else {
									EVAKerbal = null;
								}
							}
							if (_IVAKerbal != null) {
								if (KerbalIsOnVessel (vessel, _IVAKerbal)) {
									isGoneIVA = CameraManager.Instance.SetCameraIVA (_IVAKerbal, true);
									Log (string.Format("Go IVA on {0}({1}) experienceTrait: {2}, partName: ({3}){4}", _IVAKerbal.crewMemberName, _IVAKerbal.protoCrewMember.seatIdx, _IVAKerbal.protoCrewMember.experienceTrait.Title, _IVAKerbal.protoCrewMember.seat.part.partInfo.category, _IVAKerbal.protoCrewMember.seat.part.name));
									ScrMsg (false, _IVAKerbal);
								} else {
									isGoneIVA = CameraManager.Instance.SetCameraIVA ();
									Log ("Go IVA (first Kerbal selected)");
								}
							} else {
								isGoneIVA = CameraManager.Instance.SetCameraIVA ();
							}
							if (isGoneIVA) {
								DisableALL (true);
								EVAKerbal = null;
							} else {
								Warning ("Can't Go IVA now!");
							}
						} else if (_VesselhasOnlyPlaceholder) {
							NoMoreGoIVA = true;
							DisableALL (false);
							Warning ("Disable GoIVA, it seems that this vessel has no IVA!");
						}
					}
				} else {
					NoMoreGoIVA = true;
					DisableALL (false);
					Warning ("Disable GoIVA, this vessel has no crew!");
				}
			}
		}
	}
}