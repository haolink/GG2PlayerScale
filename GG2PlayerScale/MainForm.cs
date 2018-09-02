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
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Globalization;

using GG2PlayerScale.WinAPI;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;

using GGVREditor;
using Valve.VR;

namespace GG2PlayerScale
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// Oculus wrapper to access touch controller settings.
        /// </summary>
        private OculusTouchWrapper _oculusWrapper;

        /// <summary>
        /// Wrapper for OpenVR.
        /// </summary>
        private OpenVRWrapper _openVRWrapper;

        /// <summary>
        /// JSON INI file.
        /// </summary>
        private JSONINIFile _jsonIni;

        /// <summary>
        /// Current height display.
        /// </summary>
        private FrmCurrentHeight _frmCurrentHeight;

        /// <summary>
        /// Calculates scale over time.
        /// </summary>
        private ScaleOverTimeCalculator _scaleCalculator;

        /// <summary>
        /// Default player height.
        /// </summary>
        public const float DEFAULT_HEIGHT = 164.0f;

        public MainForm()
        {
            InitializeComponent();

            this._jsonIni = new JSONINIFile();
            this._playerHeight = DEFAULT_HEIGHT;

            string pHeight = this._jsonIni.Read("Height", null, "invalid");
            float pHeightF;
            if(float.TryParse(pHeight, out pHeightF) && pHeightF > 100 && pHeightF < 250)
            {
                this._playerHeight = pHeightF;
            }

            MainForm.WriteTextboxThreadSafe(txtPlayerHeight, this._playerHeight.ToString(CultureInfo.InvariantCulture));
            this._jsonIni.Write("Height", this._playerHeight.ToString(CultureInfo.InvariantCulture));

            try
            {
                _oculusWrapper = new OculusTouchWrapper();
            } catch(Exception ex)
            {
                _oculusWrapper = null;
            }

            if(this._oculusWrapper == null)
            {
                try
                {
                    this._openVRWrapper = new OpenVRWrapper();
                }
                catch(Exception ex)
                {
                    this._openVRWrapper = null;
                }
            }

            this._frmCurrentHeight = new FrmCurrentHeight();


            this._scaleCalculator = new ScaleOverTimeCalculator(this._frmCurrentHeight.lblMultiplicationTime);

            _threadEnd = false;
            _hasBeenCapturing = false;
            _playerScale = 1.0f;
            _memEditor = new MemEditor64("GalGun2-Win64-Shipping.exe");

            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            this._workThread = new Thread(RunLoop);
            this._workThread.Start();

            this._frmCurrentHeight.Show();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            FrmCurrentHeight frm = this._frmCurrentHeight;
            this._frmCurrentHeight = null;

            frm.MustNotClose = false;
            frm.Close();
            frm = null;

            this._threadEnd = true;
        }

        /// <summary>
        /// Background thread controlling the app
        /// </summary>
        private Thread _workThread;

        /// <summary>
        /// True to end the thread!
        /// </summary>
        private bool _threadEnd;

        /// <summary>
        /// Memory editor component.
        /// </summary>
        private MemEditor64 _memEditor;

        /// <summary>
        /// Is the main code patched?
        /// </summary>
        private bool _isPatched;

        /// <summary>
        /// Address of the patch data.
        /// </summary>
        private long _patchAddress;

        /// <summary>
        /// Player scale.
        /// </summary>
        private float _playerScale;

        /// <summary>
        /// Adjust the view height.
        /// </summary>
        private bool _adjustHeight;

        /// <summary>
        /// Player's real height.
        /// </summary>
        private float _playerHeight;

        /// <summary>
        /// A screenshot has been captured.
        /// </summary>
        private bool _hasBeenCapturing;

        /// <summary>
        /// Thread loops.
        /// </summary>
        public void RunLoop()
        {
            while (!_threadEnd)
            {
                this.LoopMain();
                GC.Collect();
                Thread.Sleep(100);
            }
            
            this.UnpatchGame();
        }

        /// <summary>
        /// Internal application loop.
        /// </summary>
        private void LoopMain()
        {

            this._adjustHeight = false;
            if (this._oculusWrapper != null)
            {
                OvrButton buttons = this._oculusWrapper.GetButtonState();

                if ((buttons & (OvrButton.B | OvrButton.Y)) != 0)
                {
                    this._adjustHeight = true;
                }

                if ((buttons & (OvrButton.A | OvrButton.X)) != 0)
                {
                    if (!_hasBeenCapturing)
                    {
                        this.CaptureScreenshot();
                        _hasBeenCapturing = true;
                    }
                }
                else
                {
                    _hasBeenCapturing = false;
                }
            }
            else if (this._openVRWrapper != null)
            {
                VRControllerState_t? leftHandN, rightHandN;

                if(this._openVRWrapper.GetButtonState(out leftHandN, out rightHandN))
                {
                    if(leftHandN != null && rightHandN != null)
                    {
                        VRControllerState_t leftHand = leftHandN.Value;
                        VRControllerState_t rightHand = rightHandN.Value;
                        ulong buttonsLeft = leftHand.ulButtonPressed;
                        ulong buttonsRight = rightHand.ulButtonPressed;

                        if(((buttonsLeft  & (1L << ((int)(EVRButtonId.k_EButton_Grip)))) != 0) &&
                           ((buttonsRight & (1L << ((int)(EVRButtonId.k_EButton_Grip)))) != 0)) {
                            this._adjustHeight = true;
                        }
                    }
                }
            }

            if (this._adjustHeight)
            {
                WriteLabelThreadSafe(this.lblButtonPressed, "x");
            }
            else
            {
                WriteLabelThreadSafe(this.lblButtonPressed, ""); ;
            }

            this.ReadPlayerScale();
            this.ReadPlayerHeight();

            if (_memEditor.Connect())
            {
                if (!this._isPatched)
                {
                    this.PatchMemory();
                }

                this.UpdateScale();
                WriteLabelThreadSafe(this.lblConnect, "Connected to Gal*Gun2\r\nBase: " + this._memEditor.MainModuleAddress.ToString("X16") + "\r\nHook: " + this._patchAddress.ToString("X16"));
            }
            else
            {
                WriteLabelThreadSafe(this.lblConnect, "Executable doesn't seem to be running");
                this._isPatched = false;
            }
        }

        /// <summary>
        /// Captures an Oculus Mirror screenshot.
        /// </summary>
        private void CaptureScreenshot()
        {
            Process[] processes = Process.GetProcessesByName("OculusMirror");
            Process proc = null;
            if(processes.Length > 0)
            {
                proc = processes[0];                
            }
            else
            {
                return;
            }
            var rect = new WindowRect();
            APIMethods.GetClientRect(proc.MainWindowHandle, ref rect);

            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(bmp);
            IntPtr hdcBitmap = graphics.GetHdc();

            APIMethods.PrintWindow(proc.MainWindowHandle, hdcBitmap, 1);
            
            graphics.ReleaseHdc();
            graphics.Dispose();

            string picFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            try
            {
                string targetFolder = picFolder + @"\GG2VR";
                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }

                DateTime dt = DateTime.Now;
                string fn = dt.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture);
                int add = 1;
                string addS = "";
                while(File.Exists(targetFolder + "\\" + fn + addS + ".png"))
                {
                    add++;
                    addS = add.ToString();
                }
                bmp.Save(targetFolder + "\\" + fn + addS + ".png", ImageFormat.Png);
            } catch(Exception ex)
            {
                //Apparently I'm not allowed to write the library. Ah well.
            }            
        }

        /// <summary>
        /// Reads player scale.
        /// </summary>
        private void ReadPlayerScale()
        {
            float newScale = 1.0f;
            if(this._scaleCalculator.Enabled)
            {
                this._playerScale = this._scaleCalculator.GetCurrentScale();
            }
            else if(float.TryParse(txtPlayerScale.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out newScale))
            {
                if(newScale > 0)
                {
                    this._playerScale = newScale;
                }                
            }
        }        

        /// <summary>
        /// Reads player height.
        /// </summary>
        private void ReadPlayerHeight()
        {
            float newHeight = DEFAULT_HEIGHT;
            if (float.TryParse(txtPlayerHeight.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out newHeight))
            {
                if(newHeight != this._playerHeight && newHeight > 100 && newHeight < 250)
                {
                    this._playerHeight = newHeight;
                    this._jsonIni.Write("Height", this._playerHeight.ToString(CultureInfo.InvariantCulture));
                    //this._jsonIni.SaveData();
                }
            }            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="label"></param>
        /// <param name="text"></param>
        public static void WriteLabelThreadSafe(Label label, string text)
        {
            if(label.InvokeRequired)
            {
                try
                {
                    label.Invoke((MethodInvoker)delegate
                    {
                        WriteLabelThreadSafe(label, text);
                    });
                } catch (Exception ex)
                {
                    //Ah well
                }                
            }
            else
            {
                label.Text = text;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="label"></param>
        /// <param name="text"></param>
        public static void WriteTextboxThreadSafe(TextBox textBox, string text)
        {
            if (textBox.InvokeRequired)
            {
                try
                {
                    textBox.Invoke((MethodInvoker)delegate
                    {
                        WriteTextboxThreadSafe(textBox, text);
                    });
                }
                catch (Exception ex)
                {
                    //Ah well
                }
            }
            else
            {
                textBox.Text = text;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="label"></param>
        /// <param name="text"></param>
        public static void WriteButtonLabelThreadSafe(Button button, string text)
        {
            if (button.InvokeRequired)
            {
                try
                {
                    button.Invoke((MethodInvoker)delegate
                    {
                        WriteButtonLabelThreadSafe(button, text);
                    });
                }
                catch (Exception ex)
                {
                    //Ah well
                }
            }
            else
            {
                button.Text = text;
            }
        }

        /// <summary>
        /// Patches GG2 memory.
        /// </summary>
        public void PatchMemory()
        {
            Thread.Sleep(3000);

            long address = 0;
            if(!this._memEditor.AllocateMemoryBlock(2048, out address))
            {
                this._isPatched = false;
                this._patchAddress = 0;
                return;
            }

            int modulo = (int)(address % 16);
            if(modulo > 0)
            {
                address += (16 - modulo);
            }

            this._patchAddress = address;
            byte[] patchCode = new byte[]
            {
                //Data matrix - 0x50 bytes long
                //0     1     2     3     4     5     6     7     8     9     A     B     C     D     E     F
                0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xCF, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,

                //Assembly code patch 1: 0x70 bytes long
                //0     1     2     3     4     5     6     7     8     9     A     B     C     D     E     F
                0x0F, 0x28, 0x89, 0x90, 0x01, 0x00, 0x00, 0x50, 0x53, 0x48, 0x8B, 0x01, 0x8B, 0x98, 0x30, 0xF9,
                0xFF, 0xFF, 0x81, 0xFB, 0x52, 0x00, 0x6F, 0x00, 0x75, 0x2E, 0x90, 0x90, 0x90, 0x90, 0x48, 0xBB,
                0x00, 0x00, 0x47, 0x80, 0x5F, 0x02, 0x00, 0x00, 0x8B, 0x03, 0x89, 0x43, 0x28, 0x8B, 0x81, 0x64,
                0x01, 0x00, 0x00, 0x89, 0x43, 0x04, 0x89, 0x43, 0x38, 0x0F, 0x28, 0x43, 0x10, 0x0F, 0x5C, 0x43,
                0x20, 0x0F, 0x59, 0x43, 0x30, 0x0F, 0x5C, 0xC8, 0x5B, 0x58, 0x48, 0x8B, 0xC2, 0x0F, 0x28, 0xC1,
                0xF3, 0x0F, 0x11, 0x0A, 0xFF, 0x25, 0x00, 0x00, 0x00, 0x00, 0xE1, 0x1A, 0x5D, 0x26, 0xF6, 0x7F,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,

                //Assembly code patch 2: 0xA0 bytes long
                //0     1     2     3     4     5     6     7     8     9     A     B     C     D     E     F                
                0x0F, 0x28, 0x89, 0x90, 0x01, 0x00, 0x00, 0x50, 0x53, 0x8B, 0x81, 0xBC, 0x01, 0x00, 0x00, 0x3D,
                0x11, 0x00, 0x02, 0x00, 0x75, 0x63, 0x90, 0x90, 0x90, 0x90, 0x8B, 0x81, 0xCC, 0x01, 0x00, 0x00,
                0x3D, 0x00, 0x00, 0x80, 0x3F, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x8B, 0x81, 0x7C, 0x01, 0x00,
                0x00, 0x3D, 0x00, 0x00, 0x80, 0x3F, 0x75, 0x41, 0x90, 0x90, 0x90, 0x90, 0x8B, 0x81, 0x18, 0x01,
                0x00, 0x00, 0x83, 0xF8, 0x08, 0x75, 0x32, 0x90, 0x90, 0x90, 0x90, 0x8B, 0x81, 0x00, 0x01, 0x00,
                0x00, 0x83, 0xF8, 0x00, 0x74, 0x23, 0x90, 0x90, 0x90, 0x90, 0x8B, 0x81, 0xA8, 0x01, 0x00, 0x00,
                0x3D, 0x00, 0x00, 0x80, 0x3F, 0x75, 0x12, 0x90, 0x90, 0x90, 0x90, 0x48, 0xBB, 0x00, 0x00, 0x47,
                0x80, 0x5F, 0x02, 0x00, 0x00, 0x0F, 0x58, 0x4B, 0x40, 0x5B, 0x58, 0x0F, 0x29, 0x4B, 0x10, 0x0F,
                0x28, 0x81, 0xA0, 0x01, 0x00, 0x00, 0xFF, 0x25, 0x00, 0x00, 0x00, 0x00, 0x59, 0xD3, 0x5C, 0x26,
                0xF6, 0x7F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };

            byte[] _patchAddressBytes = BitConverter.GetBytes(this._patchAddress);
            Array.Copy(_patchAddressBytes, 0, patchCode, 0x70, 8);
            Array.Copy(_patchAddressBytes, 0, patchCode, 0x12D, 8);

            _patchAddressBytes = BitConverter.GetBytes(((long)(this._memEditor.MainModuleAddress + 0x4F1AE1)));
            Array.Copy(_patchAddressBytes, 0, patchCode, 0xAA, 8);
            _patchAddressBytes = BitConverter.GetBytes(((long)(this._memEditor.MainModuleAddress + 0x4ED359)));
            Array.Copy(_patchAddressBytes, 0, patchCode, 0x14C, 8);

            this._memEditor.WriteMemory(this._patchAddress, patchCode);

            //Wait until patching.
            Thread.Sleep(100);

            //First patch
            byte[] jumpCode = new byte[]
            {
                //Basic Jump command
                0xFF, 0x25, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x90, 0x90, 0x90
            };
            _patchAddressBytes = BitConverter.GetBytes(((long)(this._patchAddress + 0x50)));
            Array.Copy(_patchAddressBytes, 0, jumpCode, 0x06, 8);

            this._memEditor.WriteMemory(this._memEditor.MainModuleAddress + 0x4F1AD0, jumpCode, true);

            //Second patch
            jumpCode = new byte[]
            {
                //Basic Jump command
                0xFF, 0x25, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x90, 0x90, 0x90, 0x90
            };
            _patchAddressBytes = BitConverter.GetBytes(((long)(this._patchAddress + 0xC0)));
            Array.Copy(_patchAddressBytes, 0, jumpCode, 0x06, 8);

            this._memEditor.WriteMemory(this._memEditor.MainModuleAddress + 0x4ED347, jumpCode, true);

            this._isPatched = true;
        }

        /// <summary>
        /// Updates the scale.
        /// </summary>
        public void UpdateScale()
        {
            if(!this._isPatched)
            {
                return;
            }

            double scale = this._playerScale;

            this._memEditor.WriteFloat(this._patchAddress, (float)scale);

            float currentScale = (float)scale;
            if(!this._memEditor.ReadFloat(this._patchAddress + 0x28, out currentScale) || currentScale < 0)
            {
                currentScale = (float)scale;
            }

            float offset = 0.0f;
            float normalHeight = 0.0f;
            float worldScale = (float)scale * 100.0f;

            if(_memEditor.ReadFloat(this._patchAddress + 0x04, out normalHeight))
            {
                float defaultOffset = (this._playerHeight - DEFAULT_HEIGHT) * normalHeight / DEFAULT_HEIGHT;

                if (this._adjustHeight)
                {
                    offset = normalHeight * (1.0f - (float)currentScale);
                    worldScale = 100.0f;
                }
                else if (Math.Abs(1 - scale / currentScale) > 0.001)
                {
                    offset = normalHeight * ((float)scale - currentScale) + defaultOffset * (float)scale;
                }
                else
                {
                    offset = defaultOffset * (float)scale;
                }
            }            

            this._memEditor.WriteFloat(this._patchAddress + 0x48, offset);

            long worldScaleOffset = 0;

            if (this._memEditor.ResolvePointer(new long[]
                {
                    this._memEditor.MainModuleAddress + 0x02A39720,
                    0x30, 0x388, 0x20, 0x300
                }, out worldScaleOffset))
            {
                this._memEditor.WriteFloat(worldScaleOffset + 0x470, worldScale, true);
            };

            this.UpdateHeightDisplay();
            this.UpdateScaleDisplay();
        }

        /// <summary>
        /// Display of current height.
        /// </summary>
        private float _heightCmDisplay = -1.0f;

        /// <summary>
        /// Updates the player height box.
        /// </summary>
        private void UpdateHeightDisplay()
        {
            float currentHeight = (float)(this._playerHeight * this._playerScale);
            currentHeight = (float)Math.Round(currentHeight, ((currentHeight < 99.95) ? 1 : 0));

            string fieldValue = "";
            if(this.UpdateStringFromFloat(ref this._heightCmDisplay, currentHeight, ref fieldValue))
            {
                MainForm.WriteLabelThreadSafe(this._frmCurrentHeight.lblPlayerHeight, fieldValue + "cm");
            }            
        }

        /// <summary>
        /// Display of current height.
        /// </summary>
        private float _heightScaleDisplay = -1.0f;

        /// <summary>
        /// Updates the player height box.
        /// </summary>
        private void UpdateScaleDisplay()
        {
            string fieldValue = "";
            if (this.UpdateStringFromFloat(ref this._heightScaleDisplay, this._playerScale * 100.0f, ref fieldValue))
            {
                MainForm.WriteLabelThreadSafe(this._frmCurrentHeight.lblPlayerScale, fieldValue + "%");
            }
        }

        /// <summary>
        /// Checks if a string needs to be updated.
        /// </summary>
        /// <param name="oldFloat"></param>
        /// <param name="newFloat"></param>
        /// <param name="outputString"></param>
        /// <returns></returns>
        public bool UpdateStringFromFloat(ref float oldFloat, float newFloat, ref string outputString)
        {
            outputString = "";

            float calcFloat = (float)Math.Round(newFloat, ((newFloat < 99.95) ? 1 : 0));
            if(calcFloat != oldFloat)
            {
                oldFloat = calcFloat;
                if (calcFloat < 100)
                {
                    outputString = String.Format(CultureInfo.InvariantCulture, "{0:0.0}", new object[] { calcFloat });
                }
                else
                {
                    outputString = calcFloat.ToString(CultureInfo.InvariantCulture);
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public void UnpatchGame()
        {
            if(!this._isPatched || !this._memEditor.IsConnected())
            {
                return;
            }

            float worldScale = 100.0f;
            long worldScaleOffset = 0;
            if (this._memEditor.ResolvePointer(new long[]
                {
                    this._memEditor.MainModuleAddress + 0x02A39720,
                    0x30, 0x388, 0x20, 0x300
                }, out worldScaleOffset))
            {
                this._memEditor.WriteFloat(worldScaleOffset + 0x470, worldScale, true);
            };

            byte[] originalCode = new byte[] 
            {
                0x0F, 0x28, 0x89, 0x90, 0x01, 0x00, 0x00, 0x48, 0x8B, 0xC2, 0x0F, 0x28, 0xC1, 0xF3, 0x0F, 0x11, 0x0A
            };            

            this._memEditor.WriteMemory(this._memEditor.MainModuleAddress + 0x4F1AD0, originalCode, true);

            byte[] originalCode2 = new byte[]
            {
                0x0F, 0x28, 0x89, 0x90, 0x01, 0x00, 0x00, 0x0F, 0x29, 0x4B, 0x10, 0x0F, 0x28, 0x81, 0xA0, 0x01, 0x00, 0x00
            };
            this._memEditor.WriteMemory(this._memEditor.MainModuleAddress + 0x4ED347, originalCode2, true);

            this._isPatched = false;
        }

        /// <summary>
        /// Starts the scaling process (or stops it) if possible.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnProcessStart_Click(object sender, EventArgs e)
        {
            float targetScale = 1.0f, targetMinutes = 0.0f;

            if(!float.TryParse(this.txtTargetScale.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out targetScale) ||
               !float.TryParse(this.txtTargetMinutes.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out targetMinutes) ||
               targetScale <= 0.0f || targetMinutes <= 0)
            {
                MessageBox.Show("Input values are invalid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if(this._scaleCalculator.Enabled)
            {
                float newScale = this._scaleCalculator.StopProcess();
                MainForm.WriteTextboxThreadSafe(this.txtPlayerScale, newScale.ToString(CultureInfo.InvariantCulture));
                MainForm.WriteButtonLabelThreadSafe(this.btnProcessPause, "Pause");
                MainForm.WriteButtonLabelThreadSafe(this.btnProcessStart, "Start process");
                this.ResetFields(true);
                return;
            }

            this.ReadPlayerScale();
            float currentScale = this._playerScale;

            int seconds = (int)Math.Round(targetMinutes * 60.0);

            this._scaleCalculator.StartProcess(currentScale, targetScale, seconds);
            MainForm.WriteButtonLabelThreadSafe(this.btnProcessPause, "Pause");
            MainForm.WriteButtonLabelThreadSafe(this.btnProcessStart, "Stop process");
            this.ResetFields(false);
        }

        /// <summary>
        /// Resets the form fields.
        /// </summary>
        /// <param name="enableDefaultFields">Reenable them (true) or disable them (false).</param>
        private void ResetFields(bool enableDefaultFields)
        {
            this.txtPlayerHeight.Enabled = enableDefaultFields;
            this.txtPlayerScale.Enabled = enableDefaultFields;
            this.txtTargetMinutes.Enabled = enableDefaultFields;
            this.txtTargetScale.Enabled = enableDefaultFields;
            this.btnProcessPause.Enabled = !enableDefaultFields;
        }

        /// <summary>
        /// Pauses or unpauses the process.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnProcessPause_Click(object sender, EventArgs e)
        {
            if (!this._scaleCalculator.Enabled)
            {
                return;
            }

            if (this._scaleCalculator.Paused)
            {
                this._scaleCalculator.Resume();
                MainForm.WriteButtonLabelThreadSafe(this.btnProcessPause, "Pause");
            }
            else
            {
                this._scaleCalculator.Pause();
                MainForm.WriteButtonLabelThreadSafe(this.btnProcessPause, "Resume");
            }                        
        }
    }
}
