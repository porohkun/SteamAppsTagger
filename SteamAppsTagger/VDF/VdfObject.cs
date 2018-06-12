using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamAppsTagger.VDF
{
    public class VdfObject : Dictionary<string, VdfValue>
    {
        internal static VdfObject Parce(StreamReader reader)
        {
            var result = new VdfObject();

            reader.ReadBlockStart();
            reader.ReadEmpty();
            while (reader.CheckNext() != VdfReaderToken.BlockEnd)
            {
                if (!reader.ReadLimiter())
                    throw new Exception(reader.BaseStream.Position + ": Limiter expected");
                var key = reader.ReadText();
                if (key == null)
                    throw new Exception(reader.BaseStream.Position + ": key must be not null");
                if (!reader.ReadLimiter())
                    throw new Exception(reader.BaseStream.Position + ": Limiter expected");
                var value = VdfValue.Parce(reader);
                reader.ReadEmpty();

                result.Add(key, value);
            }
            reader.ReadBlockEnd();
            return result;
        }
    }
}
