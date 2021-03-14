using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GG2PlayerScale
{
    interface VRWrapper
    {
        bool ShouldCreateScreenshot();

        bool ShouldResetViewport();
    }
}
