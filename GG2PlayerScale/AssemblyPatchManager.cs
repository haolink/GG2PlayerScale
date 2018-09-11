using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Resources;

namespace GG2PlayerScale
{
    public class AssemblyPatchManager
    {
        /// <summary>
        /// Assembly code.
        /// </summary>
        public byte[] AssemblyCode { get; private set; }

        /// <summary>
        /// Player scale to write.
        /// </summary>
        public int ScaleOffset { get; private set; }

        /// <summary>
        /// Current player scale.
        /// </summary>
        public int CurrentScaleOffset { get; private set; }

        /// <summary>
        /// Height of the current scene for the player.
        /// </summary>
        public int DefaultSceneHeightOffset { get; private set; }

        /// <summary>
        /// Camera offset to use.
        /// </summary>
        public int CurrentCameraOffset { get; private set; }

        /// <summary>
        /// Subtitle rendering offset.
        /// </summary>
        public int SubtitleOffset { get; private set; }

        /// <summary>
        /// The base address the patch code is added to needs to be stored here.
        /// </summary>
        public int[] BaseAddressOffsets { get; private set; }

        /// <summary>
        /// First code patch entry offset.
        /// </summary>
        public int Patch1EntryOffset { get; private set; }

        /// <summary>
        /// First code patch leaving offset.
        /// </summary>
        public int Patch1LeavingOffset { get; private set; }

        /// <summary>
        /// Second code patch entry offset.
        /// </summary>
        public int Patch2EntryOffset { get; private set; }

        /// <summary>
        /// Second code patch leaving offset.
        /// </summary>
        public int Patch2LeavingOffset { get; private set; }

        public AssemblyPatchManager()
        {
            byte[] patchCode = GG2PlayerScale.Properties.Resources.patch;
            this.AssemblyCode = patchCode;

            this.ScaleOffset = 0x00;
            this.CurrentScaleOffset = 0x28;
            this.DefaultSceneHeightOffset = 0x04;
            this.CurrentCameraOffset = 0x48;
            this.SubtitleOffset = 0x58;

            long[] offsets = patchCode.IndexesOf(Encoding.ASCII.GetBytes("RETURN01")).ToArray();
            if(offsets.Length != 1)
            {
                throw new Exception("Unable to find RETURN01");
            }
            this.Patch1LeavingOffset = (int)offsets[0];

            offsets = patchCode.IndexesOf(Encoding.ASCII.GetBytes("RETURN02")).ToArray();
            if (offsets.Length != 1)
            {
                throw new Exception("Unable to find RETURN02");
            }
            this.Patch2LeavingOffset = (int)offsets[0];

            offsets = patchCode.IndexesOf(BitConverter.GetBytes(0xDEADCAFEDEADCAFEL)).ToArray();
            if (offsets.Length != 2)
            {
                throw new Exception("Unable to find DEADCAFE");
            }
            this.BaseAddressOffsets = new int[] { (int)offsets[0], (int)offsets[1] };

            offsets = patchCode.IndexesOf(Encoding.ASCII.GetBytes("CAMUPDATE")).ToArray();
            if (offsets.Length != 1)
            {
                throw new Exception("Unable to find CAMUPDATE");
            }
            this.Patch1EntryOffset = (int)(offsets[0] + 0x20);

            offsets = patchCode.IndexesOf(Encoding.ASCII.GetBytes("LIVECAM")).ToArray();
            if (offsets.Length != 1)
            {
                throw new Exception("Unable to find LIVECAM");
            }
            this.Patch2EntryOffset = (int)(offsets[0] + 0x20);
        }
    }
}
