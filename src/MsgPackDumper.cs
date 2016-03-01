//
// Copyright 2016 HANAI Tohru aka pokehanai <hanai@pokelabo.co.jp>
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MsgPack
{
	public class MsgPackDumper
	{
		const string typeFormat = "{0,-12:s}";
		StringBuilder _sb;
		int _indentLevel;
		List<string> indentSpaces;

		byte[] _buf = new byte[64];

		uint tabWidth = 4;
		public uint TabWidth
		{
			get { return tabWidth; }
			set { tabWidth = value; }
		}

		uint maxDumpArrayCount = 256;
		public uint MaxDumpArrayCount {
			get { return maxDumpArrayCount; }
			set { maxDumpArrayCount = value; }
		}

		public string Dump (byte[] buf)
		{
			return Dump (buf, 0, buf.Length);
		}

		public string Dump (byte[] buf, int offset, int size)
		{
			using (MemoryStream ms = new MemoryStream (buf, offset, size)) {
				return Dump (ms);
			}
		}

		public string Dump (Stream stream)
		{
			indentSpaces = new List<string>(6);
			indentSpaces.Add ("");
			_indentLevel = 0;
			indentUp ();

			_sb = new StringBuilder (1024);

			MsgPackReader reader = new MsgPackReader (stream);
			while (buildDump (reader, append)) {
			}

			return _sb.ToString ();
		}

		bool buildDump (MsgPackReader reader, Action<string, string> appender)
		{
			if (!reader.Read ()) {
				return false;
			}

			if (reader.IsArray ()) {
				buildArrayDump (reader, appender);
				return true;
			}

			if (reader.IsMap ()) {
				buildMapDump (reader, appender);
				return true;
			}

			if (reader.IsRaw ()) {
				buildRawDump (reader, appender);
				return true;
			}

			switch (reader.Type) {
			case TypePrefixes.PositiveFixNum:
			case TypePrefixes.NegativeFixNum:
			case TypePrefixes.Int8:
			case TypePrefixes.Int16:
			case TypePrefixes.Int32:
				appender (getTypeName (reader), reader.ValueSigned.ToString ());
				return true;
			case TypePrefixes.Int64:
				appender (getTypeName (reader), reader.ValueSigned64.ToString ());
				return true;
			case TypePrefixes.UInt8:
			case TypePrefixes.UInt16:
			case TypePrefixes.UInt32:
				appender (getTypeName (reader), reader.ValueUnsigned.ToString ());
				return true;
			case TypePrefixes.UInt64:
				appender (getTypeName (reader), reader.ValueUnsigned64.ToString ());
				return true;
			case TypePrefixes.Float:
				appender (getTypeName (reader), reader.ValueFloat.ToString ());
				return true;
			case TypePrefixes.Double:
				appender (getTypeName (reader), reader.ValueDouble.ToString ());
				return true;
			case TypePrefixes.FixExt:
				// TODO
				return true;
			case TypePrefixes.Nil:
				appender (getTypeName (reader), "null");
				return true;
			case TypePrefixes.False:
				appender (getTypeName (reader), "false");
				return true;
			case TypePrefixes.True:
				appender (getTypeName (reader), "true");
				return true;
			case TypePrefixes.Bin8:
			case TypePrefixes.Bin16:
			case TypePrefixes.Bin32:
				// TODO
				appender (getTypeName (reader), "{bin}");
				CheckBufferSize ((int)reader.Length);
				reader.ReadValueRaw (_buf, 0, (int)reader.Length);
				return true;
			case TypePrefixes.Ext8:
			case TypePrefixes.Ext16:
			case TypePrefixes.Ext32:
				// TODO
				appender (getTypeName (reader), "{ext}");
				CheckBufferSize ((int)reader.Length);
				reader.ReadValueRaw (_buf, 0, (int)reader.Length);
				return true;
			default:
				appender (getTypeName (reader), "{???}");
				return true;
			}
		}

		void buildArrayDump (MsgPackReader reader, Action<string, string> appender)
		{
			appender ("array", "");
			appendLine ("[");
			indentUp ();
			for (uint i = 0, end = Math.Min(maxDumpArrayCount, reader.Length); i < end; ++i) {
				buildDump(reader, append);
			}
			if (maxDumpArrayCount < reader.Length) {
				appendLine ("...");
			}
			indentDown ();
			appendLine ("]");
		}

		void buildMapDump (MsgPackReader reader, Action<string, string> appender)
		{
			bool isKey = true;
			Action<string, string> mapAppender = (typeName, value) => {
				if (isKey) {
					appendKey(typeName, value);
				} else {
					appendValue(typeName, value);
				}
				isKey = !isKey;
			};

			appender ("map", "");
			appendLine ("{");
			indentUp ();
			for (uint i = 0, end = reader.Length; i < end; ++i) {
				buildDump(reader, mapAppender);
				buildDump(reader, mapAppender);
			}
			indentDown ();
			appendLine ("}");
		}

		void buildRawDump (MsgPackReader reader, Action<string, string> appender)
		{
			// TODO raw may not be a string...

			CheckBufferSize ((int)reader.Length);
			reader.ReadValueRaw (_buf, 0, (int)reader.Length);
			var value = "\"" + Encoding.UTF8.GetString (_buf, 0, (int)reader.Length) + "\"";
			appender ("string", value);
		}

		void indentUp()
		{
			++_indentLevel;
			if (indentSpaces.Count <= _indentLevel) {
				indentSpaces.Add("".PadLeft((int)tabWidth * _indentLevel));
			}
		}

		void indentDown()
		{
			--_indentLevel;
		}

		void append(string type, string value)
		{
			_sb.Append (indentSpaces [_indentLevel]);
			_sb.AppendFormat (typeFormat, type);
			_sb.AppendLine (value);
		}

		void appendLine(string s)
		{
			_sb.Append (indentSpaces [_indentLevel]);
			_sb.AppendLine (s);
		}

		void appendKey(string type, string value)
		{
			_sb.Append (indentSpaces [_indentLevel]);
			_sb.AppendFormat (typeFormat, type);
			_sb.Append (value);
			_sb.Append (" => ");
		}

		void appendValue(string type, string value)
		{
			_sb.AppendFormat (typeFormat, type);
			_sb.AppendLine (value);
		}

		string getTypeName(MsgPackReader reader)
		{
			switch (reader.Type) {
			case TypePrefixes.PositiveFixNum:
				return "int";
			case TypePrefixes.NegativeFixNum:
				return "uint";
			case TypePrefixes.FixArray:
			case TypePrefixes.Array16:
			case TypePrefixes.Array32:
				return string.Format ("array[{0}]", reader.Length);
			case TypePrefixes.FixRaw:
			case TypePrefixes.Raw8:
			case TypePrefixes.Raw16:
			case TypePrefixes.Raw32:
				// string or raw data, interface
				return "raw";
			case TypePrefixes.Nil:
				return "null";
			case TypePrefixes.False:
			case TypePrefixes.True:
				return "bool";
			case TypePrefixes.Float:
				return "float";
			case TypePrefixes.Double:
				return "double";
			case TypePrefixes.UInt8:
				return "UInt8";
			case TypePrefixes.UInt16:
				return "UInt16";
			case TypePrefixes.UInt32:
				return "UInt32";
			case TypePrefixes.UInt64:
				return "UInt64";
			case TypePrefixes.Int8:
				return "Int8";
			case TypePrefixes.Int16:
				return "Int16";
			case TypePrefixes.Int32:
				return "Int32";
			case TypePrefixes.Int64:
				return "Int64";
			case TypePrefixes.Map16:
			case TypePrefixes.Map32:
				return "map";
			case TypePrefixes.Bin8:
			case TypePrefixes.Bin16:
			case TypePrefixes.Bin32:
				return "bin";
			case TypePrefixes.FixExt:
			case TypePrefixes.Ext8:
			case TypePrefixes.Ext16:
			case TypePrefixes.Ext32:
				return "ext";
			default:
				return Enum.GetName (typeof(TypePrefixes), reader.Type);
			}
		}

		void CheckBufferSize (int size)
		{
			if (_buf.Length < size) {
				Array.Resize<byte> (ref _buf, size);
			}
		}
	}
}
