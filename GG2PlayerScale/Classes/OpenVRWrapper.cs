/**
 * Copyright 2018, haolink
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Valve.VR;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using System.IO;

namespace GG2PlayerScale
{
    class OpenVRWrapper : VRWrapper
    {
        /// <summary>
        /// OpenVR wrapper.
        /// </summary>
        private CVRSystem _system = null;

        /// <summary>
        /// Should we use OpenVR legacy input?
        /// </summary>
        private bool _useLegacyInput;

        /// <summary>
        /// Are we using Legacy input?
        /// </summary>
        public bool UseLegacyInput { get { return _useLegacyInput; } }

        /// <summary>
        /// Master handle.
        /// </summary>
        private ulong actionHandle;

        /// <summary>
        /// Modern action: Viewport reset
        /// </summary>
        private ulong actionResetViewport;

        /// <summary>
        /// Modern action: Viewport reset - semi action by pressing right grip
        /// </summary>
        private ulong actionResetViewportRightGrip;

        /// <summary>
        /// Modern action: Viewport reset - semi action by pressing left grip
        /// </summary>
        private ulong actionResetViewportLeftGrip;

        /// <summary>
        /// Controller type.
        /// </summary>
        private string _controllerType;

        /// <summary>
        /// Controller type.
        /// </summary>
        public string ControllerType { get { return _controllerType; } }

        /// <summary>
        /// Action Set Array.
        /// </summary>
        private VRActiveActionSet_t[] mActionSetArray;

        /// <summary>
        /// Loads an action from the VR manifest.
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="handle"></param>
        /// <returns></returns>
        private bool LoadAction(string actionName, ref ulong handle)
        {
            EVRInputError ioError = OpenVR.Input.GetActionHandle(actionName, ref handle);
            return (ioError == EVRInputError.None);           
        }

        /// <summary>
        /// Checks if it can connect to OpenVR.
        /// </summary>
        /// <returns></returns>
        private bool AttemptOpenVRConnection(ref CVRSystem system)
        {
            system = null;

            EVRInitError eie = new EVRInitError();
            try
            {
                system = OpenVR.Init(ref eie, EVRApplicationType.VRApplication_Background);
            }
            catch (Exception ex)
            {
                system = null;
                //MessageBox.Show("Err 1: " + ex.Message);
            }

            if (eie == EVRInitError.Init_NoServerForBackgroundApp)
            {
                system = null;
                return true;
            }
            if (system == null)
            {
                return false;
            }

            this._useLegacyInput = true;
            this._controllerType = null;

            EVRApplicationError appError = OpenVR.Applications.AddApplicationManifest(Path.GetFullPath("./manifest/app.vrmanifest"), false);
            if (appError != EVRApplicationError.None)
            {
                return true; //Will use legacy input
            }

            EVRInputError ioError = OpenVR.Input.SetActionManifestPath(Path.GetFullPath("./manifest/actions.json"));
            if (ioError != EVRInputError.None)
            {
                return true; //Will use legacy input
            }

            if (!LoadAction("/actions/default/in/reset_viewport", ref this.actionResetViewport))
            {
                return true; //Will use legacy input
            }
            if (!LoadAction("/actions/default/in/reset_viewport_leftgrip", ref this.actionResetViewportLeftGrip))
            {
                return true; //Will use legacy input
            }
            if (!LoadAction("/actions/default/in/reset_viewport_rightgrip", ref this.actionResetViewportRightGrip))
            {
                return true; //Will use legacy input
            }
            
            if (OpenVR.Input.GetActionSetHandle("/actions/default", ref this.actionHandle) != EVRInputError.None)
            {
                return true; //Will use legacy input
            }

            this._useLegacyInput = false; //Actions loaded successfully, we can use the modern input.

            return true;
        }

        /// <summary>
        /// Checks if SteamVR is available.
        /// </summary>
        /// <returns></returns>
        private bool IsOpenVRAvailable()
        {
            this._system = null;
            CVRSystem system = null;
            if (!AttemptOpenVRConnection(ref system))
            {
                return false;
            }

            if (system != null)
            {
                this._system = system;
            }
          
            return true;
        }

        /// <summary>
        /// Creates a button status wrapper for an OpenVR device.
        /// </summary>
        public OpenVRWrapper()
        {
            if (!this.IsOpenVRAvailable())
            {
                throw new NotSupportedException("OpenVR not supported on this computer");
            }
        }


        /// <summary>
        /// Shut down Oculus libraries.
        /// </summary>
        ~OpenVRWrapper()
        {
            if (_system != null)
            {
                OpenVR.Shutdown();
                _system = null;
            }
        }

        /// <summary>
        /// Runs through all application events and acknowledges a SteamVR close event.
        /// </summary>
        private bool PreventClosure()
        {
            if (this._system == null)
            {
                return false;
            }

            VREvent_t ev = new VREvent_t();
            
            while (this._system.PollNextEvent(ref ev, (uint)Marshal.SizeOf(ev)))
            {
                if (ev.eventType == (uint)(EVREventType.VREvent_Quit))
                {
                    this._system.AcknowledgeQuit_Exiting();
                    OpenVR.Shutdown();
                    this._system = null;
                    return false;
                }
            }

            return true;
        }        

        /// <summary>
        /// Gets the pressed buttons on the device.
        /// </summary>
        /// <returns></returns>
        public bool GetButtonState(out VRControllerState_t? leftHand, out VRControllerState_t? rightHand)
        {
            leftHand = null;
            rightHand = null;

            if (this._system == null)
            {
                CVRSystem system = null;
                AttemptOpenVRConnection(ref system);
                if (system == null)
                {
                    return false;
                }

                this._system = system;
            }

            try
            {
                if (!this.PreventClosure())
                {
                    return false;
                }

                uint leftHandIndex = this._system.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);
                uint rightHandIndex = this._system.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);

                leftHand = this.GetHandState(leftHandIndex);
                rightHand = this.GetHandState(rightHandIndex);
            }
            catch(Exception ex)
            {
                leftHand = null;
                rightHand = null;
                return false;
            }

            return (leftHand != null && rightHand != null);           
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        private VRControllerState_t? GetHandState(uint controllerId)
        {
            if(controllerId == OpenVR.k_unTrackedDeviceIndexInvalid)
            {
                return null;
            }

            if (!this._system.IsTrackedDeviceConnected(controllerId))
            {
                return null;
            }

            if (this._system.GetTrackedDeviceClass(controllerId) != ETrackedDeviceClass.Controller)
            {
                return null;
            }

            VRControllerState_t cstate = new VRControllerState_t();
            if (this._system.GetControllerState(controllerId, ref cstate, (uint)Marshal.SizeOf(cstate)))
            {
                return cstate;
            }

            return null;
        }

        private bool UpdateEventState()
        {
            /*var vrEvents = new List<VREvent_t>();
            var vrEvent = new VREvent_t();
            try
            {
                while (OpenVR.System.PollNextEvent(ref vrEvent, (uint)(Marshal.SizeOf(vrEvent))))
                {
                    vrEvents.Add(vrEvent);
                }
            } */

            if (!this.PreventClosure())
            {
                return false;
            }

            // #6 Update action set
            if (mActionSetArray == null)
            {
                var actionSet = new VRActiveActionSet_t
                {
                    ulActionSet = actionHandle,
                    ulRestrictedToDevice = OpenVR.k_ulInvalidActionSetHandle,
                    nPriority = 0
                };
                mActionSetArray = new VRActiveActionSet_t[] { actionSet };
            }

            var errorUAS = EVRInputError.InvalidHandle;

            try
            {
                errorUAS = OpenVR.Input.UpdateActionState(mActionSetArray, (uint)Marshal.SizeOf(typeof(VRActiveActionSet_t)));
            } 
            catch(Exception ex)
            {
                errorUAS = EVRInputError.InvalidHandle;
            }
            if (errorUAS != EVRInputError.None)
            {
                return false;
            }

            return true;
        } 

        public bool CheckActionState(ulong actionCode)
        {            
            ulong leftHandle = 0;
            OpenVR.Input.GetInputSourceHandle(OpenVR.k_pchPathUserHandLeft, ref leftHandle);
            ulong rightHandle = 0;
            OpenVR.Input.GetInputSourceHandle(OpenVR.k_pchPathUserHandRight, ref rightHandle);
            ulong[] handles = new ulong[] { leftHandle, rightHandle };

            bool pressed = false;

            if (this._controllerType == null)
            {
                InputOriginInfo_t originInfo = new InputOriginInfo_t();
                StringBuilder sb = new StringBuilder(256);
                ETrackedPropertyError err = ETrackedPropertyError.TrackedProp_Success;
                OpenVR.Input.GetOriginTrackedDeviceInfo(leftHandle, ref originInfo, (uint)Marshal.SizeOf(originInfo));
                this._system.GetStringTrackedDeviceProperty(originInfo.trackedDeviceIndex, ETrackedDeviceProperty.Prop_ControllerType_String, sb, 256, ref err);

                this._controllerType = sb.ToString();
            }           

            foreach (var handle in handles)
            {
                InputDigitalActionData_t action = new InputDigitalActionData_t();
                uint size = (uint)Marshal.SizeOf(typeof(InputDigitalActionData_t));
                EVRInputError ioError = OpenVR.Input.GetDigitalActionData(actionCode, ref action, size, handle);
                if (ioError != EVRInputError.None)
                {
                    continue;
                }

                if (action.bState)
                {
                    pressed = true;
                }
            }

            return pressed;
        }

        public bool ShouldCreateScreenshot()
        {
            return false;
        }

        public bool ShouldResetViewport()
        {
            if (!this._useLegacyInput && this.UpdateEventState())
            {
                bool knucklesAction = CheckActionState(actionResetViewport);

                if (knucklesAction)
                {
                    return true;
                }

                return (CheckActionState(actionResetViewportLeftGrip) && CheckActionState(actionResetViewportRightGrip));
            }
            else
            {
                VRControllerState_t? leftHandN, rightHandN;

                if (this.GetButtonState(out leftHandN, out rightHandN))
                {
                    if (leftHandN != null && rightHandN != null)
                    {
                        VRControllerState_t leftHand = leftHandN.Value;
                        VRControllerState_t rightHand = rightHandN.Value;
                        ulong buttonsLeft = leftHand.ulButtonPressed;
                        ulong buttonsRight = rightHand.ulButtonPressed;

                        //Grip button: 1 << k_EButton_Grip = 4

                        ulong buttonStateLeft = (buttonsLeft & (ulong)4);
                        ulong buttonStateRight = (buttonsRight & (ulong)4);

                        if (buttonStateRight > 0 && buttonStateLeft > 0)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }            

            return false;
        }
    }
}
