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

using Ab3d.OculusWrap;

namespace GG2PlayerScale
{
    public enum OvrButton : uint
    {
        None = 0x00000000,
        A = 0x00000001, /// A button on XBox controllers and right Touch controller. Select button on Oculus Remote.
        B = 0x00000002, /// B button on XBox controllers and right Touch controller. Back button on Oculus Remote.
        RThumb = 0x00000004, /// Right thumbstick on XBox controllers and Touch controllers. Not present on Oculus Remote.
        RShoulder = 0x00000008, /// Right shoulder button on XBox controllers. Not present on Touch controllers or Oculus Remote.

        X = 0x00000100,  /// X button on XBox controllers and left Touch controller. Not present on Oculus Remote.
        Y = 0x00000200,  /// Y button on XBox controllers and left Touch controller. Not present on Oculus Remote.
        LThumb = 0x00000400,  /// Left thumbstick on XBox controllers and Touch controllers. Not present on Oculus Remote.
        LShoulder = 0x00000800,  /// Left shoulder button on XBox controllers. Not present on Touch controllers or Oculus Remote.

        Up = 0x00010000,  /// Up button on XBox controllers and Oculus Remote. Not present on Touch controllers.
        Down = 0x00020000,  /// Down button on XBox controllers and Oculus Remote. Not present on Touch controllers.
        Left = 0x00040000,  /// Left button on XBox controllers and Oculus Remote. Not present on Touch controllers.
        Right = 0x00080000,  /// Right button on XBox controllers and Oculus Remote. Not present on Touch controllers.
        Enter = 0x00100000,  /// Start on XBox 360 controller. Menu on XBox One controller and Left Touch controller. Should be referred to as the Menu button in user-facing documentation.
        Back = 0x00200000,  /// Back on Xbox 360 controller. View button on XBox One controller. Not present on Touch controllers or Oculus Remote.
        VolUp = 0x00400000,  /// Volume button on Oculus Remote. Not present on XBox or Touch controllers.
        VolDown = 0x00800000,  /// Volume button on Oculus Remote. Not present on XBox or Touch controllers.
        Home = 0x01000000,  /// Home button on XBox controllers. Oculus button on Touch controllers and Oculus Remote.

        // Bit mask of all buttons that are for private usage by Oculus
        Private = VolUp | VolDown | Home,

        // Bit mask of all buttons on the right Touch controller
        RMask = A | B | RThumb | RShoulder,

        // Bit mask of all buttons on the left Touch controller
        LMask = X | Y | LThumb | LShoulder | Enter,
    };

    public class OculusTouchWrapper
    {
        /// <summary>
        /// Oculus wrapper.
        /// </summary>
        private OvrWrap64 _ovrWrapper = null;

        /// <summary>
        /// Checks if an Oculus device is available.
        /// </summary>
        /// <returns></returns>
        private bool IsOculusAvailable()
        {
            bool result = false;
            try
            {
                InitParams initializationParameters = new InitParams();
                initializationParameters.Flags = InitFlags.Invisible | InitFlags.RequestVersion;
                initializationParameters.RequestedMinorVersion = 8;
                initializationParameters.ConnectionTimeoutMS = 0;
                initializationParameters.UserData = IntPtr.Zero;
                initializationParameters.LogCallback = null;

                _ovrWrapper = OvrWrap64.Create64();

                Result res = _ovrWrapper.Initialize(initializationParameters);

                if (res < Result.Success)
                {
                    result = false;
                }
                else
                {
                    result = true;
                }                
            } catch(Exception ex)
            {
                result = false;
                _ovrWrapper = null;
            }
            return result;
        }

        /// <summary>
        /// HMD data.
        /// </summary>
        private IntPtr _hmd;

        /// <summary>
        /// Creates a button status wrapper for an Oculus touch device.
        /// </summary>
        public OculusTouchWrapper()
        {
            if(!this.IsOculusAvailable())
            {
                throw new NotSupportedException("Oculus not supported on this computer");
            }
        }

        /// <summary>
        /// Shut down Oculus libraries.
        /// </summary>
        ~OculusTouchWrapper()
        {
            CloseConnection();

            if(_ovrWrapper != null)
            {
                _ovrWrapper.Shutdown();
            }            
        }

        /// <summary>
        /// Ensures a connection to the HMD.
        /// </summary>
        /// <returns></returns>
        private bool EnsureConnection()
        {
            if(this._hmd != IntPtr.Zero)
            {
                return true;
            }

            this._hmd = IntPtr.Zero;
            GraphicsLuid luid = new GraphicsLuid();
            Result res = _ovrWrapper.Create(ref this._hmd, ref luid);

            if (res < Result.Success || this._hmd == IntPtr.Zero)
            {
                this._hmd = IntPtr.Zero;
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Close the Oculus connection.
        /// </summary>
        /// <returns></returns>
        private void CloseConnection()
        {
            if (this._hmd == IntPtr.Zero)
            {
                return;
            }

            this._ovrWrapper.Destroy(this._hmd);
            this._hmd = IntPtr.Zero;
        }

        /// <summary>
        /// Gets the pressed buttons on the device.
        /// </summary>
        /// <returns></returns>
        public OvrButton GetButtonState()
        {
            OvrButton buttonsPressed = OvrButton.None;

            try
            {
                if (EnsureConnection())
                {
                    InputState input = new InputState();
                    bool success = false;
                    Result res = _ovrWrapper.GetInputState(this._hmd, ControllerType.Active, ref input);

                    if (res < Result.Success)
                    {
                        CloseConnection();
                        if (EnsureConnection())
                        {
                            res = _ovrWrapper.GetInputState(this._hmd, ControllerType.Active, ref input);
                            if (res >= Result.Success)
                            {
                                success = true;
                            }
                        }
                    }
                    else
                    {
                        success = true;
                    }

                    if (success)
                    {
                        buttonsPressed = (OvrButton)(input.Buttons);
                    }
                }
            } catch(Exception ex)
            {
                //Whatever
            }

            return buttonsPressed;
        }
    }
}
