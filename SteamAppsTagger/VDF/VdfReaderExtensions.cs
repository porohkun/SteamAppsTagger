using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamAppsTagger.VDF
{
    public static class VdfReaderExtensions
    {
        public static VdfReaderToken CheckNext(this StreamReader reader)
        {
            var next = reader.Peek();
            if (IsEmpty(next)) return VdfReaderToken.Empty;
            if (IsLimiter(next)) return VdfReaderToken.Limiter;
            if (IsBlockStart(next)) return VdfReaderToken.BlockStart;
            if (IsBlockEnd(next)) return VdfReaderToken.BlockEnd;
            return VdfReaderToken.Other;
        }

        public static void ReadEmpty(this StreamReader reader)
        {
            while (IsEmpty(reader.Peek()))
                reader.Read();
        }

        public static bool ReadLimiter(this StreamReader reader)
        {
            return IsLimiter(reader.Read());
        }

        public static bool ReadBlockStart(this StreamReader reader)
        {
            return IsBlockStart(reader.Read());
        }

        public static bool ReadBlockEnd(this StreamReader reader)
        {
            return IsBlockEnd(reader.Read());
        }

        public static string ReadText(this StreamReader reader)
        {
            var sb = new StringBuilder();

            while (!IsLimiter(reader.Peek()))
                sb.Append((char)reader.Read());

            if (sb.Length == 0)
                return null;
            return sb.ToString();
        }

        private static bool IsEmpty(int value)
        {
            return value == 9 || value == 10 || value == 13 || value == 32;
        }

        private static bool IsLimiter(int value)
        {
            return value == 34;
        }

        private static bool IsBlockStart(int value)
        {
            return value == 123;
        }

        private static bool IsBlockEnd(int value)
        {
            return value == 125;
        }
    }
}
