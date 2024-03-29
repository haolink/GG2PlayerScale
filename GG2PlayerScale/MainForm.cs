﻿/**
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
using System.Resources;
using Microsoft.Win32;

namespace GG2PlayerScale
{
    public partial class MainForm : Form
    {
        public enum VRMode
        {
            Any,
            Oculus,
            OpenVR
        }

        const long PATCH_JUMPIN_OFFSET1 = 0x4F12F0; //4F1AD0
        const long PATCH_JUMPOUT_OFFSET1 = 0x4F1301; //4F1301

        const long PATCH_JUMPIN_OFFSET2 = 0x4ECBF7; //4ED347
        const long PATCH_JUMPOUT_OFFSET2 = 0x4ECC09; //4ED359

        /// <summary>
        /// Oculus wrapper to access touch controller settings.
        /// </summary>
        private OculusTouchWrapper _oculusWrapper;

        /// <summary>
        /// Wrapper for OpenVR.
        /// </summary>
        private OpenVRWrapper _openVRWrapper;

        /// <summary>
        /// Generic wrapper, should phase out the Oculus and OpenVR wrappers.
        /// </summary>
        private VRWrapper _vrWrapper;

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
        /// Resets the scale over time.
        /// </summary>
        private ScaleOverTimeCalculator _scaleResetManager;

        /// <summary>
        /// Default player height.
        /// </summary>
        public const float DEFAULT_HEIGHT = 164.0f;

        /// <summary>
        /// Reset time.
        /// </summary>
        public const double RESET_TIME = 0.2;

        /// <summary>
        /// Is paused?
        /// </summary>
        private bool _paused;

        /// <summary>
        /// Is paused?
        /// </summary>
        private bool _scalePaused;

        /// <summary>
        /// Assembly Manager.
        /// </summary>
        private AssemblyPatchManager _patchManager;

        /// <summary>
        /// Is VR available?
        /// </summary>
        private bool _vrAvailable;
        
        /// <summary>
        /// Default text for reset world scale.
        /// </summary>
        private string _resetDefaultText;

        /// <summary>
        /// Last controller type.
        /// </summary>
        private string _lastControllerType;

        public MainForm(VRMode vrMode = VRMode.Any)
        {
            InitializeComponent();

            this._jsonIni = new JSONINIFile();
            //this._playerHeight = DEFAULT_HEIGHT;
            this._eyeHeight = 160.0f;
            this.UpdateEyeHeightDisplay();

            this._paused = false;
            this._scalePaused = false;

            this.cbTargetTimeUnit.SelectedIndex = 0;

            string pHeight = this._jsonIni.Read("Height", null, "invalid");
            float pHeightF;
            /*if(float.TryParse(pHeight, out pHeightF) && pHeightF > 100 && pHeightF < 250)
            {
                this._playerHeight = pHeightF;
            }*/

            this._lastControllerType = null;
            this._resetDefaultText = this.gbScaleReset.Text;
            
            //MainForm.WriteTextboxThreadSafe(txtPlayerHeight, this._playerHeight.ToString(CultureInfo.InvariantCulture));
            //this._jsonIni.Write("Height", this._playerHeight.ToString(CultureInfo.InvariantCulture));

            this._subtitleOffset = (float)(this._jsonIni.ReadFloat("SubtitleOffset", null, 30.0));
            if(this._subtitleOffset < -20 || this._subtitleOffset > 100)
            {
                this._subtitleOffset = 30.0f;
            }
            //this.txtSubtitleOffset.Text = this._subtitleOffset.ToString(CultureInfo.InvariantCulture);

            this.txtTargetScale.Text = this._jsonIni.ReadFloat("ProcessSpeed", null, 0.5).ToString(CultureInfo.InvariantCulture);
            this.txtTargetTimeValue.Text = this._jsonIni.ReadFloat("ProcessTime", null, 30).ToString(CultureInfo.InvariantCulture);
            this.cbTargetTimeUnit.SelectedIndex = this._jsonIni.ReadInt("ProcessTimeUnit", null, 0);
            this.chkEnableEndScale.Checked = this._jsonIni.ReadBool("ProcessHasEndScale", null, false);
            this.txtEndScale.Text = this._jsonIni.ReadFloat("ProcessEndScale", null, 0.1).ToString(CultureInfo.InvariantCulture);

            this.chkResetScaleGradually.Checked = this._jsonIni.ReadBool("ResetGradually", null, true);
            this.chkResetWorldScale.Checked = this._jsonIni.ReadBool("ResetWorldScale", null, true);

            this.chkEnableEndScale_CheckedChanged(null, new EventArgs());

            this.chkResetScaleGradually.CheckedChanged += SaveCheckImmediate;
            this.chkResetWorldScale.CheckedChanged += SaveCheckImmediate;

            this._oculusWrapper = null;

            if(vrMode == VRMode.Any)
            {
                string xrRuntime = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Khronos\OpenXR\1", "ActiveRuntime", null);

                if (xrRuntime != null)
                {
                    string xrJson = Path.GetFileName(xrRuntime).ToLowerInvariant();
                    if (xrJson == "steamxr_win64.json" || xrJson == "steam_runtime.json")
                    {
                        vrMode = VRMode.OpenVR;
                    }
                    if (xrJson == "oculus_openxr_64.json")
                    {
                        vrMode = VRMode.Oculus;
                    }
                }
            }

            if(vrMode != VRMode.OpenVR)
            {
                try
                {
                    _oculusWrapper = new OculusTouchWrapper();
                    this._vrAvailable = true;
                }
                catch (Exception ex)
                {
                    _oculusWrapper = null;
                    this._vrAvailable = false;
                }
            }

            if (vrMode != VRMode.Oculus)
            {
                if (this._oculusWrapper == null)
                {
                    try
                    {
                        this._openVRWrapper = new OpenVRWrapper();
                        this._vrAvailable = true;
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show("Err 3: " + ex.Message);
                        this._openVRWrapper = null;
                        this._vrAvailable = false;
                    }
                }
            }

            if (this._vrAvailable)
            {
                if (this._openVRWrapper != null)
                {
                    this._vrWrapper = this._openVRWrapper;
                }
                else if (this._oculusWrapper != null)
                {
                    this._vrWrapper = this._oculusWrapper;
                }
            }

            if(this._oculusWrapper != null)
            {
                this.gbScaleReset.Text += " (Hold B or Y to activate)";
            }
            else if(this._openVRWrapper != null)
            {
                this.gbScaleReset.Text += " (Hold both grips at the same time to activate)";
            }

            this.chkResetWorldScale.Enabled = this._vrAvailable;
            this.chkResetScaleGradually.Enabled = this._vrAvailable;

            this._frmCurrentHeight = new FrmCurrentHeight();


            this._scaleCalculator = new ScaleOverTimeCalculator(this._frmCurrentHeight.lblMultiplicationTime);
            this._scaleCalculator.Completed += ScaleProgessCompleted;

            this._scaleResetManager = new ScaleOverTimeCalculator(null);

            _threadEnd = false;
            _hasBeenCapturing = false;
            _playerScale = 1.0f;            
            _memEditor = new MemEditor64("GalGun2-Win64-Shipping.exe");

            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            try
            {
                this._patchManager = new AssemblyPatchManager();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
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
        /// Address of the assembly patch data.
        /// </summary>
        private long _patchAddress;

        /// <summary>
        /// Address of the array patch data.
        /// </summary>
        private long _arrayPatchAddress;

        /// <summary>
        /// Player scale.
        /// </summary>
        private float _playerScale;

        /// <summary>
        /// Subtitle offset.
        /// </summary>
        private float _subtitleOffset;

        /// <summary>
        /// Adjust the view height.
        /// </summary>
        private bool _adjustHeight;

        /// <summary>
        /// Player's real height. (not needed anymore)
        /// </summary>
        //private float _playerHeight;

        /// <summary>
        /// Player's eye height.
        /// </summary>
        private float _eyeHeight;

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
                DateTime dt = DateTime.Now;

                this.LoopMain();
                GC.Collect();

                TimeSpan neededTime = DateTime.Now - dt;

                //Target frame rate 90 FPS - thus: 22.2 ms
                double ms = 11.1 - neededTime.TotalMilliseconds;
                ms = Math.Max(5.0, ms); //Wait at least 5 ms
                int msInt = (int)Math.Floor(ms);
                Thread.Sleep(msInt);
            }
            
            this.UnpatchGame();
        }

        /// <summary>
        /// Internal application loop.
        /// </summary>
        private void LoopMain()
        {
            bool wasAdjusting = this._adjustHeight;

            this._adjustHeight = false;
            if (this._vrWrapper != null)
            {
                if (this._vrWrapper.ShouldResetViewport())
                {
                    this._adjustHeight = true;
                }

                if (this._vrWrapper.ShouldCreateScreenshot())
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

            /*if (this._oculusWrapper != null)
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
                        
                        //Grip button: 1 << k_EButton_Grip = 4

                        ulong buttonStateLeft = (buttonsLeft & (ulong)4);
                        ulong buttonStateRight = (buttonsRight & (ulong)4);

                        if (buttonStateRight > 0 && buttonStateLeft > 0) {
                            this._adjustHeight = true;
                        }
                    }
                }
            }*/

            if (this._adjustHeight)
            {
                WriteLabelThreadSafe(this.lblButtonPressed, "x");
            }
            else
            {
                WriteLabelThreadSafe(this.lblButtonPressed, ""); ;
            }

            this.ReadPlayerScale();
            //this.ReadPlayerHeight();
            this.ReadSubtitleOffset();            

            if (_memEditor.Connect())
            {
                if (!this._isPatched)
                {
                    this.PatchMemory();

                    this.ResumeProcessManager();
                }

                if (this.chkResetScaleGradually.Checked)
                {
                    try
                    {
                        if (this._adjustHeight && !wasAdjusting)
                        {
                            if (this._scaleResetManager.Enabled)
                            {
                                this._scaleResetManager.StopProcess();
                            }
                            this._scaleResetManager.StartProcess(this._playerScale, (float)(1.0 / this._playerScale), RESET_TIME, 0, 1.0f, RESET_TIME);
                        }
                        else if (!this._adjustHeight && wasAdjusting)
                        {
                            float sScale = 1.0f;
                            if (this._scaleResetManager.Enabled)
                            {
                                sScale = this._scaleResetManager.GetCurrentScale();
                                this._scaleResetManager.StopProcess();
                            }
                            this._scaleResetManager.StartProcess(sScale, this._playerScale / sScale, RESET_TIME, 0, this._playerScale, RESET_TIME);
                        }
                    } catch(Exception ex)
                    {
                        //Don't crash, just resume!
                    }
                }

                try
                {
                    this.UpdateScale();
                    WriteLabelThreadSafe(this.lblConnect, "Connected to Gal*Gun2\r\nBase: " + this._memEditor.MainModuleAddress.ToString("X16") + "\r\nHook: " + this._patchAddress.ToString("X16") + " / " + this._arrayPatchAddress.ToString("X16"));
                    
                    if (this._openVRWrapper != null && !this._openVRWrapper.UseLegacyInput && this._openVRWrapper.ControllerType != this._lastControllerType)
                    {
                        this._lastControllerType = this._openVRWrapper.ControllerType.ToLowerInvariant();

                        if (this._lastControllerType == "knuckles" || this._lastControllerType == "holographic_controller")
                        {
                            WriteGroupBoxThreadSafe(this.gbScaleReset, this._resetDefaultText + " (Press a thumbstick down to activate)");
                        } 
                        else
                        {
                            WriteGroupBoxThreadSafe(this.gbScaleReset, this._resetDefaultText + " (Hold both grips at the same time to activate)");
                        }
                    }
                } 
                catch(Exception ex)
                {
                    //Don't crash, just resume!
                }
                
                
            }
            else
            {
                this.PauseProcessManager();
                WriteLabelThreadSafe(this.lblConnect, "Executable doesn't seem to be running");
                this._isPatched = false;
            }
        }

        /// <summary>
        /// Captures an Oculus Mirror screenshot.
        /// </summary>
        private void CaptureScreenshot()
        {
            Process[] processes = Process.GetProcessesByName("GalGun2-Win64-Shipping");
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
        /*private void ReadPlayerHeight()
        {
            float newHeight = DEFAULT_HEIGHT;
            if (float.TryParse(txtPlayerHeight.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out newHeight))
            {
                if(newHeight != this._playerHeight && newHeight >= 100 && newHeight <= 2500)
                {
                    this._playerHeight = newHeight;
                    this._jsonIni.Write("Height", this._playerHeight.ToString(CultureInfo.InvariantCulture));
                    //this._jsonIni.SaveData();
                }
            }            
        }*/

        /// <summary>
        /// Reads subtitle offset.
        /// </summary>
        private void ReadSubtitleOffset()
        {
            float newOffset = this._subtitleOffset;
            /*if (float.TryParse(txtSubtitleOffset.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out newOffset))
            {
                if (newOffset != this._subtitleOffset && newOffset >= -20 && newOffset <= 100)
                {
                    this._playerHeight = newOffset;
                    this._jsonIni.WriteFloat("SubtitleOffset", this._subtitleOffset);
                    //this._jsonIni.SaveData();
                }
            }*/
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="label"></param>
        /// <param name="text"></param>
        public static void WriteLabelThreadSafe(Label label, string text)
        {
            if(label == null)
            {
                return;
            }

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
        public static void WriteGroupBoxThreadSafe(GroupBox gb, string text)
        {
            if (gb == null)
            {
                return;
            }

            if (gb.InvokeRequired)
            {
                try
                {
                    gb.Invoke((MethodInvoker)delegate
                    {
                        WriteGroupBoxThreadSafe(gb, text);
                    });
                }
                catch (Exception ex)
                {
                    //Ah well
                }
            }
            else
            {
                gb.Text = text;
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

            long arrayAddress = 0;
            if (!this._memEditor.AllocateMemoryBlock(2048, out arrayAddress))
            {
                this._isPatched = false;
                this._arrayPatchAddress = 0;
                return;
            }

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
            modulo = (int)(arrayAddress % 16);
            if (modulo > 0)
            {
                arrayAddress += (16 - modulo);
            }

            this._patchAddress = address;
            this._arrayPatchAddress = arrayAddress;

            byte[] patchCode = this._patchManager.AssemblyCode;

            byte[] _patchAddressBytes = BitConverter.GetBytes(this._patchAddress);
            Array.Copy(_patchAddressBytes, 0, patchCode, this._patchManager.BaseAddressOffsets[0], 8);
            Array.Copy(_patchAddressBytes, 0, patchCode, this._patchManager.BaseAddressOffsets[1], 8);

            _patchAddressBytes = BitConverter.GetBytes(this._arrayPatchAddress);
            Array.Copy(_patchAddressBytes, 0, patchCode, this._patchManager.ArrayAddressOffsets[0], 8);

            _patchAddressBytes = BitConverter.GetBytes(((long)(this._memEditor.MainModuleAddress + PATCH_JUMPOUT_OFFSET1)));
            Array.Copy(_patchAddressBytes, 0, patchCode, this._patchManager.Patch1LeavingOffset, 8);
            _patchAddressBytes = BitConverter.GetBytes(((long)(this._memEditor.MainModuleAddress + PATCH_JUMPOUT_OFFSET2)));
            Array.Copy(_patchAddressBytes, 0, patchCode, this._patchManager.Patch2LeavingOffset, 8);

            this._memEditor.WriteMemory(this._patchAddress, patchCode);

            //Wait until patching.
            Thread.Sleep(100);

            //First patch
            byte[] jumpCode = new byte[]
            {
                //Basic Jump command
                0xFF, 0x25, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x90, 0x90, 0x90
            };
            _patchAddressBytes = BitConverter.GetBytes(((long)(this._patchAddress + this._patchManager.Patch1EntryOffset)));
            Array.Copy(_patchAddressBytes, 0, jumpCode, 0x06, 8);

            this._memEditor.WriteMemory(this._memEditor.MainModuleAddress + PATCH_JUMPIN_OFFSET1, jumpCode, true);

            //Second patch
            jumpCode = new byte[]
            {
                //Basic Jump command
                0xFF, 0x25, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x90, 0x90, 0x90, 0x90
            };
            _patchAddressBytes = BitConverter.GetBytes(((long)(this._patchAddress + this._patchManager.Patch2EntryOffset)));
            Array.Copy(_patchAddressBytes, 0, jumpCode, 0x06, 8);

            this._memEditor.WriteMemory(this._memEditor.MainModuleAddress + PATCH_JUMPIN_OFFSET2, jumpCode, true);

            this._isPatched = true;
        }

        /// <summary>
        /// Writes the player eye height.
        /// </summary>
        private void UpdateEyeHeightDisplay()
        {
            MainForm.WriteLabelThreadSafe(this._lblEyeHeight, this._eyeHeight.ToString("0") + " cm");
            MainForm.WriteLabelThreadSafe(this._lblPlayerHeight, (this._eyeHeight * 1.06f).ToString("0") + " cm");
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

            long eyeHeightPointer = 0;
            if (this._memEditor.ResolvePointer(new long[]
                {
                    this._memEditor.MainModuleAddress + 0x02A4DB20,
                    0x50, 0x130
                }, out eyeHeightPointer))
            {
                byte[] eyeHeightBytes = this._memEditor.ReadMemory(eyeHeightPointer + 0x88, 4);
                int eyeHeightInt = BitConverter.ToInt32(eyeHeightBytes, 0);
                float nEyeHeight = (float)eyeHeightInt;
                if (nEyeHeight != this._eyeHeight)
                {
                    this._eyeHeight = nEyeHeight;
                    this.UpdateEyeHeightDisplay();
                }               
            };
            this._memEditor.WriteFloat(this._patchAddress + this._patchManager.ScaleOffset, (float)scale);

            float currentScale = (float)scale;
            if(!this._memEditor.ReadFloat(this._patchAddress + this._patchManager.CurrentScaleOffset, out currentScale) || currentScale < 0)
            {
                currentScale = (float)scale;
            }

            float offset = 0.0f;
            float normalHeight = 0.0f;
            float worldScale = (float)scale * 100.0f;

            if(_memEditor.ReadFloat(this._patchAddress + this._patchManager.DefaultSceneHeightOffset, out normalHeight))
            {
                float defaultOffset = 0.0f; //(this._playerHeight - DEFAULT_HEIGHT) * normalHeight / DEFAULT_HEIGHT;

                if (this._adjustHeight || this._scaleResetManager.Enabled)
                {
                    float resetScale = 1.0f;
                    float extraOffset = 0.0f;
                    if(this._scaleResetManager.Enabled)
                    {
                        resetScale = this._scaleResetManager.GetCurrentScale();

                        double timeSinceStart = (DateTime.Now - this._scaleResetManager.StartTime).TotalSeconds;
                        double factor = Math.Min(RESET_TIME, Math.Max(0, timeSinceStart / RESET_TIME)) / RESET_TIME;

                        if (this._adjustHeight)
                        {
                            factor = 1.0 - factor;
                        }

                        extraOffset = (float)(defaultOffset * (float)resetScale * factor);
                    }

                    offset = normalHeight * (resetScale - (float)currentScale) + extraOffset;

                    if(this.chkResetWorldScale.Checked)
                    {
                        worldScale = resetScale * 100.0f;
                    }
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

            bool isDokiDoki = false;
            bool isRendezvous = false;
            long dokiPointer = 0;
            if (this._memEditor.ResolvePointer(new long[]
                {
                    this._memEditor.MainModuleAddress + 0x02A27070,
                    0x720, 0x20, 0x268, 0x500
                }, out dokiPointer))
            {
                byte[] isDokiBytes = this._memEditor.ReadMemory(dokiPointer + 0xA0, 1);
                isDokiDoki = (isDokiBytes[0] == 1);
            };
            if (this._memEditor.ResolvePointer(new long[]
                {
                    this._memEditor.MainModuleAddress + 0x02A27070,
                    0xDF0, 0x30
                }, out dokiPointer))
            {
                byte[] isRendezvousBytes = this._memEditor.ReadMemory(dokiPointer + 0x118, 1);
                if (isRendezvousBytes != null)
                {
                    isRendezvous = (isRendezvousBytes[0] == 34);
                }                
            };

            byte[] setDoki = new byte[1] { 0 };
            if(isDokiDoki)
            {
                setDoki = new byte[1] { 1 };
            }
            else if(isRendezvous)
            {
                setDoki = new byte[1] { 2 };
            }
            this._memEditor.WriteMemory(this._patchAddress + this._patchManager.DokiDokiSwitch, setDoki);

            this._memEditor.WriteFloat(this._patchAddress + this._patchManager.CurrentCameraOffset, offset);

            //Fixed cameras:

            //Camera rotation
            float rZ = 0.0f;
            this._memEditor.ReadFloat(this._patchAddress + this._patchManager.CameraOrientationOffset + 0, out rZ);
            float rW = 0.0f;
            this._memEditor.ReadFloat(this._patchAddress + this._patchManager.CameraOrientationOffset + 4, out rW);

            //Camera yaw
            float siny_cosp = 2 * (rW * rZ);
            float cosy_cosp = 1 - 2 * (rZ * rZ);
            float yaw = (float)Math.Atan2(siny_cosp, cosy_cosp);

            float rotFactorX = (float)Math.Cos(yaw);
            float rotFactorY = (float)Math.Sin(yaw);
            
            uint arrayOffset = 0x00;

            float deathOffsetZ = -145.0f + this._eyeHeight * (float)scale;
            this.UpdateCameraTranslationVectors(ref arrayOffset, BitConverter.GetBytes((uint)0xC1A00000), rotFactorX * (1 - (float)scale) * 75.0f, rotFactorY * (1 - (float)scale) * 75.0f, deathOffsetZ);

            float devilDeathOffsetZ = 110.0f - DEFAULT_HEIGHT + this._eyeHeight * (float)scale;
            this.UpdateCameraTranslationVectors(ref arrayOffset, BitConverter.GetBytes((uint)0xC2DC0000), rotFactorX * (1 - (float)scale) * 105.0f, rotFactorY * (1 - (float)scale) * 105.0f, devilDeathOffsetZ);

            float cKissIdent = 60 - this._eyeHeight; //-100.0f + (1 - currentScale) * 150.0f;
            float cKissDateIdent = 55 - this._eyeHeight; //-105.0f + (1 - currentScale) * 150.0f;

            this.UpdateCameraTranslationVectors(ref arrayOffset, BitConverter.GetBytes(cKissIdent), rotFactorX * (1 - (float)scale) * 40.0f, rotFactorY * (1 - (float)scale) * 40.0f, (float)(50.0f * (scale - 1.0f)));
            this.UpdateCameraTranslationVectors(ref arrayOffset, BitConverter.GetBytes(cKissDateIdent), rotFactorX * (1 - (float)scale) * 40.0f, rotFactorY * (1 - (float)scale) * 40.0f, (float)(45.0f * (scale - 1.0f)));

            this.UpdateCameraTranslationVectors(ref arrayOffset, null, 0.0f, 0.0f, 0.0f);

            /*if (scale <= 1.0f)
            {*/
            if (scale <= 1.0f)
            {
                this._memEditor.WriteFloat(this._patchAddress + this._patchManager.DokiDokiCameraOffset + 0, rotFactorX * (1 - (float)scale) * 50.0f);
                this._memEditor.WriteFloat(this._patchAddress + this._patchManager.DokiDokiCameraOffset + 4, rotFactorY * (1 - (float)scale) * 50.0f);
            } 
            else
            {
                this._memEditor.WriteFloat(this._patchAddress + this._patchManager.DokiDokiCameraOffset + 0, 0.0f);
                this._memEditor.WriteFloat(this._patchAddress + this._patchManager.DokiDokiCameraOffset + 4, 0.0f);
            }
                

                float eyeHeight = this._eyeHeight * (float)scale;
                float offsetDokiZ = -64.0f + 87.0f * (1 - currentScale) + eyeHeight;
                /*if (eyeHeight < 250)
                {*/
                    this._memEditor.WriteFloat(this._patchAddress + this._patchManager.DokiDokiCameraOffset + 8, offsetDokiZ);
                /*}
                else
                {
                    this._memEditor.WriteFloat(this._patchAddress + this._patchManager.DokiDokiCameraOffset + 8, 0.0f);
                }*/

            if (scale <= 1.0f)
            {
                this._memEditor.WriteFloat(this._patchAddress + this._patchManager.RendezvousCameraOffset + 0, rotFactorX * (1 - (float)scale) * 100.0f);
                this._memEditor.WriteFloat(this._patchAddress + this._patchManager.RendezvousCameraOffset + 4, rotFactorY * (1 - (float)scale) * 100.0f);

            }
            else
            {
                this._memEditor.WriteFloat(this._patchAddress + this._patchManager.RendezvousCameraOffset + 0, 0.0f);
                this._memEditor.WriteFloat(this._patchAddress + this._patchManager.RendezvousCameraOffset + 4, 0.0f);
            }

                eyeHeight = this._eyeHeight * 0.93f * (float)scale;
                float offsetRend = (-1) * 150.0f * currentScale + eyeHeight;
                /*if (eyeHeight < 250)
                {*/
                    this._memEditor.WriteFloat(this._patchAddress + this._patchManager.RendezvousCameraOffset + 8, offsetRend);
                /*}
                else
                {
                    this._memEditor.WriteFloat(this._patchAddress + this._patchManager.RendezvousCameraOffset + 8, 0.00f);
                } */               
            /*} 
            else
            {
                this._memEditor.WriteFloat(this._patchAddress + this._patchManager.DokiDokiCameraOffset + 0, 0.0f);
                this._memEditor.WriteFloat(this._patchAddress + this._patchManager.DokiDokiCameraOffset + 4, 0.0f);
                this._memEditor.WriteFloat(this._patchAddress + this._patchManager.DokiDokiCameraOffset + 8, 0.0f);

                this._memEditor.WriteFloat(this._patchAddress + this._patchManager.RendezvousCameraOffset + 0, 0.0f);
                this._memEditor.WriteFloat(this._patchAddress + this._patchManager.RendezvousCameraOffset + 4, 0.0f);
                this._memEditor.WriteFloat(this._patchAddress + this._patchManager.RendezvousCameraOffset + 8, 0.0f);
            }*/
            
            

            /*//Block default death camera - camera value: -20.0, offset: 0x00
            this._memEditor.WriteMemory(this._arrayPatchAddress + 0x00, BitConverter.GetBytes((long)0xC1A00000));
            this._memEditor.WriteFloat(this._arrayPatchAddress + 0x10, deathOffsetX);
            this._memEditor.WriteFloat(this._arrayPatchAddress + 0x14, deathOffsetY);
            this._memEditor.WriteFloat(this._arrayPatchAddress + 0x18, deathOffsetZ);

            //Block devil death camera - camera value: -110.0, offset: 0x20
            this._memEditor.WriteMemory(this._arrayPatchAddress + 0x20, BitConverter.GetBytes((long)0xC2DC0000));
            this._memEditor.WriteFloat(this._arrayPatchAddress + 0x30, deathOffsetX);
            this._memEditor.WriteFloat(this._arrayPatchAddress + 0x34, deathOffsetY);
            this._memEditor.WriteFloat(this._arrayPatchAddress + 0x38, devilDeathOffsetZ);

            this._memEditor.WriteMemory(this._arrayPatchAddress + 0x40, BitConverter.GetBytes((long)0x422A0000));
            this._memEditor.WriteFloat(this._arrayPatchAddress + 0x50, 0.0f);
            this._memEditor.WriteFloat(this._arrayPatchAddress + 0x54, 0.0f);
            this._memEditor.WriteFloat(this._arrayPatchAddress + 0x58, (float)(42.5f * (scale - 1.0f)));

            this._memEditor.WriteMemory(this._arrayPatchAddress + 0x60, BitConverter.GetBytes((long)0x42160000));
            this._memEditor.WriteFloat(this._arrayPatchAddress + 0x70, 0.0f);
            this._memEditor.WriteFloat(this._arrayPatchAddress + 0x74, 0.0f);
            this._memEditor.WriteFloat(this._arrayPatchAddress + 0x78, (float)(37.5f * (scale - 1.0f)));*/

            //End block - offset: 0x60
            //this._memEditor.WriteMemory(this._arrayPatchAddress + 0x80, BitConverter.GetBytes((long)0x00000000));

            long worldScaleOffset = 0;

            /*original = new long[]
            {
                this._memEditor.MainModuleAddress + 0x02A39720,
                    0x30, 0x388, 0x20, 0x300 
            }*/

            if (this._memEditor.ResolvePointer(new long[]
                {
                    this._memEditor.MainModuleAddress + 0x02A3A8A0,
                    0x30, 0x388, 0x20, 0x300
                }, out worldScaleOffset))
            {
                this._memEditor.WriteFloat(worldScaleOffset + 0x470, worldScale, true);
            };

            float nearPlane = 10.0f;
            nearPlane *= (float)Math.Max(0.01f, scale);
            this._memEditor.WriteFloat(this._memEditor.MainModuleAddress + 0x02689C84, nearPlane);

            //this._memEditor.WriteFloat(this._patchAddress + this._patchManager.SubtitleOffset, this._subtitleOffset);

            this.UpdateHeightDisplay();
            this.UpdateScaleDisplay();
        }
        
        /// <summary>
        /// Updates the camera translations.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="ident"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        private void UpdateCameraTranslationVectors(ref uint offset, byte[] ident, float x, float y, float z)
        {
            byte[] sendIdent = ident;
            if(ident == null)
            {
                sendIdent = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            }
            else
            {
                bool allZero = true;
                foreach(byte bt in sendIdent)
                {
                    if (bt != 0)
                    {
                        allZero = false;
                        break;
                    }
                }
                if (allZero)
                {
                    sendIdent[0] = 0x01;
                }
            }

            this._memEditor.WriteMemory(this._arrayPatchAddress + offset + 0x00, sendIdent);
            this._memEditor.WriteFloat(this._arrayPatchAddress + offset + 0x10, x);
            this._memEditor.WriteFloat(this._arrayPatchAddress + offset + 0x14, y);
            this._memEditor.WriteFloat(this._arrayPatchAddress + offset + 0x18, z);

            offset += 0x20;
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
            float currentHeight = (float)(this._eyeHeight * 1.06f * this._playerScale);
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

            this._memEditor.WriteMemory(this._memEditor.MainModuleAddress + PATCH_JUMPIN_OFFSET1, originalCode, true);

            byte[] originalCode2 = new byte[]
            {
                0x0F, 0x28, 0x89, 0x90, 0x01, 0x00, 0x00, 0x0F, 0x29, 0x4B, 0x10, 0x0F, 0x28, 0x81, 0xA0, 0x01, 0x00, 0x00
            };
            this._memEditor.WriteMemory(this._memEditor.MainModuleAddress + PATCH_JUMPIN_OFFSET2, originalCode2, true);

            this._isPatched = false;
        }

        /// <summary>
        /// Starts the scaling process (or stops it) if possible.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnProcessStart_Click(object sender, EventArgs e)
        {
            float targetScale = 1.0f, targetTime = 0.0f;

            if(!float.TryParse(this.txtTargetScale.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out targetScale) 
               || targetScale <= 0.0f || targetScale == 1.0f)
            {
                MessageBox.Show("Scale factor is invalid: must be a positive number other than 1.0.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!float.TryParse(this.txtTargetTimeValue.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out targetTime)
               || targetTime <= 0.0f)
            {
                MessageBox.Show("Time span is invalid: must be a positive number other than 1.0.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.ReadPlayerScale();
            float currentScale = this._playerScale;

            float? endScale = null;
            if (this.chkEnableEndScale.Checked)
            {
                float endScaleValue = 1.0f;
                if (!float.TryParse(this.txtEndScale.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out endScaleValue)
                    || endScaleValue <= 0.0f)
                {
                    MessageBox.Show("End scale is invalid. It must be a positive value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if(targetScale < 1 && endScaleValue >= currentScale)
                {
                    MessageBox.Show("Your end scale (scale you reach) must be lower than your base scale if the scale factor is below 1.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (targetScale > 1 && endScaleValue <= currentScale)
                {
                    MessageBox.Show("Your end scale (scale you reach) must be above than your base scale if the scale factor is above 1.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                endScale = endScaleValue;
            }

            int countDownValue = 0;
            if (!int.TryParse(this.txtCountdown.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out countDownValue)
                    || countDownValue < 0)
            {
                MessageBox.Show("The countdown must be a positive integer (including zero).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (this._scaleCalculator.Enabled)
            {
                float newScale = this._scaleCalculator.StopProcess();
                MainForm.WriteTextboxThreadSafe(this.txtPlayerScale, newScale.ToString(CultureInfo.InvariantCulture));
                MainForm.WriteButtonLabelThreadSafe(this.btnProcessPause, "Pause");
                MainForm.WriteButtonLabelThreadSafe(this.btnProcessStart, "Start process");
                this.ResetFields(true);
                return;
            }

            double factor = 60.0;
            if(this.cbTargetTimeUnit.SelectedIndex == 1)
            {
                factor = 1.0;
            }

            this._jsonIni.WriteFloat("ProcessSpeed", targetScale);
            this._jsonIni.WriteFloat("ProcessTime", targetTime);
            this._jsonIni.WriteInt  ("ProcessTimeUnit", this.cbTargetTimeUnit.SelectedIndex);
            this._jsonIni.WriteBool ("ProcessHasEndScale", endScale.HasValue);
            if(endScale.HasValue)
            {
                this._jsonIni.WriteFloat("ProcessEndScale", endScale.Value);
            }            

            int seconds = (int)Math.Round(targetTime * factor);            

            this._scaleCalculator.StartProcess(currentScale, targetScale, seconds, countDownValue, endScale);
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
            if(InvokeRequired)
            {
                this.Invoke(new Action(() => ResetFields(enableDefaultFields)));
                return;
            }

            //this.txtPlayerHeight.Enabled = enableDefaultFields;
            this.txtPlayerScale.Enabled = enableDefaultFields;
            this.txtTargetTimeValue.Enabled = enableDefaultFields;
            this.cbTargetTimeUnit.Enabled = enableDefaultFields;
            this.txtTargetScale.Enabled = enableDefaultFields;
            this.chkEnableEndScale.Enabled = enableDefaultFields;
            this.txtEndScale.Enabled = enableDefaultFields && this.chkEnableEndScale.Checked;
            this.txtCountdown.Enabled = enableDefaultFields;
            this.btnProcessPause.Enabled = !enableDefaultFields;
            this.chkResetWorldScale.Enabled = enableDefaultFields && this._vrAvailable;
            this.chkResetScaleGradually.Enabled = enableDefaultFields && this._vrAvailable;
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

        /// <summary>
        /// Continues running scaling processes if required.
        /// </summary>
        private void ResumeProcessManager(bool invoked = false)
        {
            if(!this._paused && !invoked)
            {
                return;
            }
            this._paused = false;
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => ResumeProcessManager(true)));
                return;
            }

            if (this._scaleCalculator.Enabled)
            {
                this.btnProcessPause.Enabled = true;
                if (this._scalePaused && this._scaleCalculator.Paused)
                {
                    this._scaleCalculator.Resume();
                }
            }

            this._scalePaused = false;

            this.btnProcessStart.Enabled = true;            
        }

        /// <summary>
        /// Continues running scaling processes if required.
        /// </summary>
        private void PauseProcessManager(bool invoked = false)
        {
            if (this._paused && !invoked)
            {
                return;
            }
            this._paused = true;
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => PauseProcessManager(true)));
                return;
            }

            this._scalePaused = false;

            if(this._scaleCalculator.Enabled)
            {
                if(!this._scaleCalculator.Paused)
                {
                    this._scaleCalculator.Pause();
                    this._scalePaused = true;
                }
            }

            this.btnProcessStart.Enabled = false;
            this.btnProcessPause.Enabled = false;
        }

        /// <summary>
        /// Checkbox value has changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkEnableEndScale_CheckedChanged(object sender, EventArgs e)
        {
            txtEndScale.Enabled = chkEnableEndScale.Checked;
        }

        /// <summary>
        /// Scaling progress has been completed without cancellation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScaleProgessCompleted(object sender, EventArgs e)
        {
            float current = 1.0f;
            float? target = this._scaleCalculator.TargetScale;
            if(target != null)
            {
                current = target.Value;
            }

            MainForm.WriteTextboxThreadSafe(this.txtPlayerScale,
                current.ToString(CultureInfo.InvariantCulture)
            );
            MainForm.WriteButtonLabelThreadSafe(this.btnProcessStart, "Start process");

            this.ResetFields(true);
        }

        private void SaveCheckImmediate(object sender, EventArgs e)
        {
            this._jsonIni.WriteBool("ResetGradually", this.chkResetScaleGradually.Checked, null);
            this._jsonIni.WriteBool("ResetWorldScale", this.chkResetWorldScale.Checked, null);
            this._jsonIni.SaveData();
        }
    }
}
