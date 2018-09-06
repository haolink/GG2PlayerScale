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

using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Globalization;

namespace GGVREditor
{
    public class JSONINIFile
    {
        private string _path;

        /// <summary>
        /// Internal serialiser.
        /// </summary>
        private DataContractJsonSerializer _jsonSerializer;

        /// <summary>
        /// Internal storage.
        /// </summary>
        private Dictionary<string, Dictionary<string, string>> _internalStorage;

        /// <summary>
        /// Path of the file
        /// </summary>
        public string Path
        {
            get { return _path; }
        }

        /// <summary>
        /// Executable location
        /// </summary>
        private static string _EXE = Assembly.GetExecutingAssembly().GetName().Name;

        /// <summary>
        /// Initialises a JSON file for storage.
        /// </summary>
        /// <param name="JSonIniPath"></param>
        public JSONINIFile(string JSonIniPath = null)
        {
            _path = new FileInfo(JSonIniPath ?? _EXE + ".json").FullName.ToString();

            this._internalStorage = new Dictionary<string, Dictionary<string, string>>();
            this._jsonSerializer = new DataContractJsonSerializer(typeof(Dictionary<string, Dictionary<string, string>>), new DataContractJsonSerializerSettings()
            {
                UseSimpleDictionaryFormat = true,                
            });

            if(File.Exists(_path))
            {
                FileStream fs = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read);

                try
                {
                    this._internalStorage = (Dictionary<string, Dictionary<string, string>>)this._jsonSerializer.ReadObject(fs);
                }
                catch (Exception ex)
                {
                    this._internalStorage = new Dictionary<string, Dictionary<string, string>>();
                }

                fs.Close();
            }
        }

        ~JSONINIFile()
        {
            this.SaveData();
        }

        /// <summary>
        /// Saves data onto the disk.
        /// </summary>
        public void SaveData()
        {
            FileStream fs = new FileStream(_path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            var rwf = JsonReaderWriterFactory.CreateJsonWriter(fs, Encoding.UTF8, true, true, "  ");
            this._jsonSerializer.WriteObject(rwf, this._internalStorage);
            rwf.Flush();
            rwf.Close();
            fs.Close();
        }

        /// <summary>
        /// Reads a value.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Section"></param>
        /// <param name="DefaultValue"></param>
        /// <returns></returns>
        public string Read(string Key, string Section = null, string DefaultValue = "")
        {
            string sectInternal = Section ?? _EXE;
            if(!this._internalStorage.ContainsKey(sectInternal) || !this._internalStorage[sectInternal].ContainsKey(Key))
            {
                return DefaultValue;
            }
            return this._internalStorage[sectInternal][Key];
        }

        /// <summary>
        /// Reads all keys in a section.
        /// </summary>
        /// <param name="Section"></param>
        /// <returns></returns>
        public string[] ReadKeys(string Section = null)
        {
            string sectInternal = Section ?? _EXE;
            if (!this._internalStorage.ContainsKey(sectInternal))
            {
                return new string[0];
            }
            return this._internalStorage[sectInternal].Keys.ToArray();
        }

        /// <summary>
        /// Reads all sections.
        /// </summary>
        /// <returns></returns>
        public string[] ReadSections()
        {
            return this._internalStorage.Keys.ToArray();            
        }

        /// <summary>
        /// Writes to the internal storage file.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <param name="Section"></param>
        public void Write(string Key, string Value, string Section = null)
        {
            string sectInternal = Section ?? _EXE;
            if (!this._internalStorage.ContainsKey(sectInternal))
            {
                this._internalStorage.Add(sectInternal, new Dictionary<string, string>());
            }
            Dictionary<string, string> internalSection = this._internalStorage[sectInternal];

            if(internalSection.ContainsKey(Key))
            {
                internalSection[Key] = Value;
            }
            else
            {
                this._internalStorage[sectInternal].Add(Key, Value);
            }            
        }

        /// <summary>
        /// Deletes a key from the internal storage.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Section"></param>
        public void DeleteKey(string Key, string Section = null)
        {
            string sectInternal = Section ?? _EXE;
            if (!this._internalStorage.ContainsKey(sectInternal) || !this._internalStorage[sectInternal].ContainsKey(Key))
            {
                return;
            }
            this._internalStorage[sectInternal].Remove(Key);
        }

        /// <summary>
        /// Removes an entire section.
        /// </summary>
        /// <param name="Section"></param>
        public void DeleteSection(string Section = null)
        {
            string sectInternal = Section ?? _EXE;
            if (!this._internalStorage.ContainsKey(sectInternal))
            {
                return;
            }
            this._internalStorage.Remove(sectInternal);
        }

        /// <summary>
        /// Checks if a key exists.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Section"></param>
        /// <returns></returns>
        public bool KeyExists(string Key, string Section = null)
        {
            string sectInternal = Section ?? _EXE;
            if (!this._internalStorage.ContainsKey(sectInternal) || !this._internalStorage[sectInternal].ContainsKey(Key))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Writes a boolean value.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <param name="Section"></param>
        public void WriteBool(string Key, bool Value, string Section = null)
        {
            Write(Key, Value ? "1" : "0", Section);
        }

        /// <summary>
        /// Reads a boolean value.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Section"></param>
        /// <param name="Default"></param>
        /// <returns></returns>
        public bool ReadBool(string Key, string Section = null, bool Default = false)
        {
            return ((Read(Key, Section, Default ? "1" : "0")) == "1");
        }

        /// <summary>
        /// Writes an integer.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <param name="Section"></param>
        public void WriteInt(string Key, int Value, string Section = null)
        {
            this.Write(Key, Value.ToString(), Section);
        }

        /// <summary>
        /// Reads an integer.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Section"></param>
        /// <param name="Default"></param>
        /// <returns></returns>
        public int ReadInt(string Key, string Section = null, int Default = 0)
        {
            string v = this.Read(Key, Section, null);
            if(v == null)
            {
                return Default;
            }
            int result = Default;
            int.TryParse(v, out result);
            return result;
        }

        /// <summary>
        /// Writes an integer.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <param name="Section"></param>
        public void WriteFloat(string Key, double Value, string Section = null)
        {
            this.Write(Key, Value.ToString(CultureInfo.InvariantCulture), Section);
        }

        /// <summary>
        /// Reads an integer.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Section"></param>
        /// <param name="Default"></param>
        /// <returns></returns>
        public double ReadFloat(string Key, string Section = null, double Default = 0.0)
        {
            string v = this.Read(Key, Section, null);
            if (v == null)
            {
                return Default;
            }
            double result = Default;
            double.TryParse(v, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
            return result;
        }
    }
}
