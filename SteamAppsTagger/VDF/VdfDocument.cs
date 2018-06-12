using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamAppsTagger.VDF
{
    public class VdfDocument
    {
        private string _key;
        private VdfValue _value;

        public string Key { get { return _key; } }
        public VdfValue Value { get { return _value; } }

        public VdfValue this[string key]
        {
            get
            {
                if (Value.Type != VdfValueType.Object) throw new Exception("VdfValue must be Object type");
                return _value[key];
            }
            set
            {
                if (Value.Type != VdfValueType.Object) throw new Exception("VdfValue must be Object type");
                _value[key] = value;
            }
        }

        public VdfDocument(string key, VdfValue value)
        {
            _key = key;
            _value = value;
        }

        public static VdfDocument ParceFile(string path)
        {
            using (var reader = File.OpenText(path))
            {
                reader.ReadEmpty();
                if (!reader.ReadLimiter())
                    throw new Exception(reader.BaseStream.Position + ": Limiter expected");
                var key = reader.ReadText();
                if (key == null)
                    throw new Exception(reader.BaseStream.Position + ": key must be not null");
                if (!reader.ReadLimiter())
                    throw new Exception(reader.BaseStream.Position + ": Limiter expected");
                var value = VdfValue.Parce(reader);
                return new VdfDocument(key, value);
            }
        }
    }
}
