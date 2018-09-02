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

namespace GG2PlayerScale
{
    class OpenVRWrapper
    {
        /// <summary>
        /// OpenVR wrapper.
        /// </summary>
        private CVRSystem _system = null;

        /// <summary>
        /// Checks if an Oculus device is available.
        /// </summary>
        /// <returns></returns>
        private bool IsOpenVREnabled()
        {
            CVRSystem system = null;
            this._system = null;
            try
            {
                EVRInitError eie = new EVRInitError();
                system = OpenVR.Init(ref eie, EVRApplicationType.VRApplication_Background);    
            }
            catch (Exception ex)
            {
                system = null;                
            }
            
            if(system == null)
            {
                return false;
            }

            this._system = system;
            return true;
        }

        /// <summary>
        /// Creates a button status wrapper for an OpenVR device.
        /// </summary>
        public OpenVRWrapper()
        {
            if (!this.IsOpenVREnabled())
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
        /// Gets the pressed buttons on the device.
        /// </summary>
        /// <returns></returns>
        public bool GetButtonState(out VRControllerState_t? leftHand, out VRControllerState_t? rightHand)
        {
            leftHand = null;
            rightHand = null;

            try
            {
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
    }
}
