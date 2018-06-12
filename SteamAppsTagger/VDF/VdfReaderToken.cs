using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamAppsTagger.VDF
{
    public enum VdfReaderToken
    {
        Empty,
        Limiter,
        BlockStart,
        BlockEnd,
        Other
    }
}
