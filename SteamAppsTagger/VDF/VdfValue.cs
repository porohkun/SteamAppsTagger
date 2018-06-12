using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamAppsTagger.VDF
{
    [DebuggerDisplay("{DebugString}")]
    public class VdfValue
    {
        private VdfValueType _type;
        private VdfObject _object;
        private string _text;

        public VdfValueType Type { get { return _type; } }
        public VdfObject Object { get { return _object; } }
        public string Text { get { return _text; } }

        public VdfValue this[string key]
        {
            get
            {
                if (_type != VdfValueType.Object) throw new Exception("VdfValue must be Object type");
                return Object[key];
            }
            set
            {
                if (_type != VdfValueType.Object) throw new Exception("VdfValue must be Object type");
                Object[key] = value;
            }
        }

        public VdfValue()
        {
            _type = VdfValueType.Null;
        }

        public VdfValue(VdfObject obj)
        {
            _type = VdfValueType.Object;
            _object = obj;
        }

        public VdfValue(string text)
        {
            _type = VdfValueType.Text;
            _text = text;
        }

        internal static VdfValue Parce(StreamReader reader)
        {
            reader.ReadEmpty();
            var next = reader.CheckNext();
            switch (next)
            {
                case VdfReaderToken.BlockStart:
                    return new VdfValue(VdfObject.Parce(reader));
                case VdfReaderToken.Limiter:
                    reader.ReadLimiter();
                    var text = reader.ReadText();
                    reader.ReadLimiter();
                    if (text == null)
                        return new VdfValue();
                    return new VdfValue(text);
                default: throw new Exception(reader.BaseStream.Position + ": Limiter or BlockStart expected");
            }
        }

        public string DebugString
        {
            get
            {
                switch (_type)
                {
                    case VdfValueType.Null: return "NULL";
                    case VdfValueType.Text: return Text;
                    case VdfValueType.Object: return "OBJECT";
                    default: return "UNKNOWN";
                }
            }
        }

        public static implicit operator string(VdfValue value)
        {
            if (value.Type != VdfValueType.Text) throw new Exception("VdfValue must be Text type");
            return value.Text;
        }
    }
}
