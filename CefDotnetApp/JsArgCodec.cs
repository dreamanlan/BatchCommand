using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using ScriptableFramework;
using AbstractAgent.Utils;

namespace DotNetLib
{
    // Managed counterpart of the native js_arg.h wire format. It replaces the
    // old "every argument stringified" convention so that int64/bigint/binary/
    // datetime and nested arrays/objects survive the C# <-> native <-> V8 round
    // trip without loss.
    //
    // A single self-contained byte blob is exchanged on every boundary. Layout
    // (little-endian, matching the native host on x64 Windows):
    //   [int32 count]
    //   per node: [int32 tag][int32 child_count][int64 i][double d]
    //             [int32 slen][slen raw bytes]
    // Containers use a pre-order flat sequence:
    //   Array : one node (child_count=N) followed by N child nodes.
    //   Object: one node (child_count=N) followed by 2*N nodes in
    //           key0,val0,key1,val1,... order (keys are String nodes).
    // The top level is a flat pre-order sequence consumed until exhausted.
    internal enum JsTag
    {
        Null = 0,
        Undefined = 1,
        Bool = 2,
        Int32 = 3,
        UInt32 = 4,
        Double = 5,
        BigInt = 6,    // Bytes = UTF-8 decimal string
        String = 7,    // Bytes = UTF-8 text
        Binary = 8,    // Bytes = raw bytes
        Array = 9,     // ChildCount = element count
        Object = 10,   // ChildCount = key/value pair count
        DateTime = 11  // Bytes = UTF-8 ISO 8601 string
    }

    internal sealed class JsArgNode
    {
        public JsTag Tag = JsTag.Null;
        public int ChildCount = 0;
        public long I = 0;      // Bool(0/1) / Int32 / UInt32
        public double D = 0.0;  // Double
        public byte[]? Bytes = null; // BigInt/String/Binary/DateTime payload
    }

    internal static class JsArgCodec
    {
        // --- Blob <-> nodes ---------------------------------------------------
        public static byte[] SerializeNodes(IReadOnlyList<JsArgNode> nodes)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms, Encoding.UTF8, true);
            bw.Write(nodes.Count);
            foreach (var n in nodes) {
                int slen = n.Bytes != null ? n.Bytes.Length : 0;
                bw.Write((int)n.Tag);
                bw.Write(n.ChildCount);
                bw.Write(n.I);
                bw.Write(n.D);
                bw.Write(slen);
                if (slen > 0) {
                    bw.Write(n.Bytes!, 0, slen);
                }
            }
            bw.Flush();
            return ms.ToArray();
        }

        public static List<JsArgNode> DeserializeNodes(byte[]? data)
        {
            var nodes = new List<JsArgNode>();
            if (data == null || data.Length < sizeof(int)) {
                return nodes;
            }
            try {
                using var ms = new MemoryStream(data, false);
                using var br = new BinaryReader(ms, Encoding.UTF8, true);
                int count = br.ReadInt32();
                if (count < 0) {
                    return nodes;
                }
                for (int k = 0; k < count; k++) {
                    var n = new JsArgNode();
                    n.Tag = (JsTag)br.ReadInt32();
                    n.ChildCount = br.ReadInt32();
                    n.I = br.ReadInt64();
                    n.D = br.ReadDouble();
                    int slen = br.ReadInt32();
                    if (slen > 0) {
                        n.Bytes = br.ReadBytes(slen);
                        if (n.Bytes.Length != slen) {
                            nodes.Clear();
                            return nodes;
                        }
                    }
                    nodes.Add(n);
                }
            }
            catch (Exception) {
                nodes.Clear();
            }
            return nodes;
        }

        // --- Public helpers ---------------------------------------------------
        // Serializes a DSL argument list (type preserving).
        public static byte[] SerializeBoxedValues(IList<BoxedValue> args)
        {
            var nodes = new List<JsArgNode>();
            for (int i = 0; i < args.Count; i++) {
                AppendBoxedValue(args[i], nodes);
            }
            return SerializeNodes(nodes);
        }
        // Serializes a plain string argument list (each becomes a String node).
        public static byte[] SerializeStrings(string[] args)
        {
            var nodes = new List<JsArgNode>();
            for (int i = 0; i < args.Length; i++) {
                AppendString(args[i], nodes);
            }
            return SerializeNodes(nodes);
        }
        // Serializes a single value as a one-element top-level sequence.
        public static byte[] SerializeSingle(BoxedValue value)
        {
            var nodes = new List<JsArgNode>();
            AppendBoxedValue(value, nodes);
            return SerializeNodes(nodes);
        }
        // Rebuilds the full top-level argument list from a blob.
        public static List<BoxedValue> DeserializeToBoxedList(byte[]? data)
        {
            var nodes = DeserializeNodes(data);
            var result = new List<BoxedValue>();
            int cur = 0;
            while (cur < nodes.Count) {
                result.Add(ReadValue(nodes, ref cur));
            }
            return result;
        }
        // Rebuilds a single value (first top-level node) from a blob.
        public static BoxedValue DeserializeSingle(byte[]? data)
        {
            var nodes = DeserializeNodes(data);
            if (nodes.Count == 0) {
                return BoxedValue.NullObject;
            }
            int cur = 0;
            return ReadValue(nodes, ref cur);
        }

        // --- BoxedValue -> nodes ---------------------------------------------
        private static void AppendString(string? s, List<JsArgNode> outNodes)
        {
            outNodes.Add(new JsArgNode {
                Tag = JsTag.String,
                Bytes = Encoding.UTF8.GetBytes(s ?? string.Empty)
            });
        }

        private static void AppendBoxedValue(BoxedValue v, List<JsArgNode> outNodes)
        {
            if (v.IsNullObject) {
                outNodes.Add(new JsArgNode { Tag = JsTag.Null });
                return;
            }
            if (v.IsBoolean) {
                outNodes.Add(new JsArgNode { Tag = JsTag.Bool, I = v.GetBool() ? 1 : 0 });
                return;
            }
            if (v.IsString) {
                AppendString(v.AsString, outNodes);
                return;
            }
            if (v.IsDateTime) {
                outNodes.Add(new JsArgNode {
                    Tag = JsTag.DateTime,
                    Bytes = Encoding.UTF8.GetBytes(v.GetDateTime().ToString("o", CultureInfo.InvariantCulture))
                });
                return;
            }
            if (v.IsInteger) {
                switch (v.Type) {
                    case BoxedValue.c_LongType:
                        outNodes.Add(new JsArgNode {
                            Tag = JsTag.BigInt,
                            Bytes = Encoding.UTF8.GetBytes(v.GetLong().ToString(CultureInfo.InvariantCulture))
                        });
                        return;
                    case BoxedValue.c_ULongType:
                        outNodes.Add(new JsArgNode {
                            Tag = JsTag.BigInt,
                            Bytes = Encoding.UTF8.GetBytes(v.GetULong().ToString(CultureInfo.InvariantCulture))
                        });
                        return;
                    case BoxedValue.c_ByteType:
                    case BoxedValue.c_UShortType:
                    case BoxedValue.c_UIntType:
                        outNodes.Add(new JsArgNode { Tag = JsTag.UInt32, I = v.GetUInt() });
                        return;
                    default:
                        outNodes.Add(new JsArgNode { Tag = JsTag.Int32, I = v.GetInt() });
                        return;
                }
            }
            if (v.IsNumber) {
                outNodes.Add(new JsArgNode { Tag = JsTag.Double, D = v.GetDouble() });
                return;
            }
            if (v.IsChar) {
                AppendString(v.GetChar().ToString(), outNodes);
                return;
            }
            if (v.IsObject) {
                object? obj = v.GetObject();
                if (obj == null) {
                    outNodes.Add(new JsArgNode { Tag = JsTag.Null });
                    return;
                }
                if (obj is byte[] rawBytes) {
                    outNodes.Add(new JsArgNode { Tag = JsTag.Binary, Bytes = rawBytes });
                    return;
                }
                if (obj is IList<BoxedValue> bvlist) {
                    outNodes.Add(new JsArgNode { Tag = JsTag.Array, ChildCount = bvlist.Count });
                    for (int i = 0; i < bvlist.Count; i++) {
                        AppendBoxedValue(bvlist[i], outNodes);
                    }
                    return;
                }
                if (obj is IDictionary<BoxedValue, BoxedValue> bvdict) {
                    outNodes.Add(new JsArgNode { Tag = JsTag.Object, ChildCount = bvdict.Count });
                    foreach (var pair in bvdict) {
                        AppendString(KeyToString(pair.Key), outNodes);
                        AppendBoxedValue(pair.Value, outNodes);
                    }
                    return;
                }
                if (obj is IDictionary<string, BoxedValue> svdict) {
                    outNodes.Add(new JsArgNode { Tag = JsTag.Object, ChildCount = svdict.Count });
                    foreach (var pair in svdict) {
                        AppendString(pair.Key, outNodes);
                        AppendBoxedValue(pair.Value, outNodes);
                    }
                    return;
                }
                // Other container/object forms (IList<object?>, IDictionary<string,
                // object?>, IList<string?>, LitJson.JsonData, JsonElement, ...) are
                // normalized to BoxedValue containers by DslHelper, then recursed.
                var normalized = DslHelper.GetBoxedValueFromValue(obj);
                if (normalized.IsObject && ReferenceEquals(normalized.GetObject(), obj)) {
                    // No normalization happened: fall back to a text form.
                    AppendString(obj.ToString(), outNodes);
                    return;
                }
                AppendBoxedValue(normalized, outNodes);
                return;
            }
            // Any remaining type (tuple, decimal edge cases, ...) degrades to text.
            AppendString(v.ToString(), outNodes);
        }

        private static string KeyToString(BoxedValue key)
        {
            if (key.IsString) {
                return key.AsString ?? string.Empty;
            }
            return key.ToString() ?? string.Empty;
        }

        // --- nodes -> BoxedValue ---------------------------------------------
        private static BoxedValue ReadValue(List<JsArgNode> nodes, ref int cur)
        {
            if (cur >= nodes.Count) {
                return BoxedValue.NullObject;
            }
            var n = nodes[cur++];
            switch (n.Tag) {
                case JsTag.Null:
                case JsTag.Undefined:
                    return BoxedValue.NullObject;
                case JsTag.Bool:
                    return BoxedValue.From(n.I != 0);
                case JsTag.Int32:
                    return BoxedValue.From((int)n.I);
                case JsTag.UInt32:
                    return BoxedValue.From((uint)n.I);
                case JsTag.Double:
                    return BoxedValue.From(n.D);
                case JsTag.BigInt: {
                        string s = BytesToString(n.Bytes);
                        if (long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var lv)) {
                            return BoxedValue.From(lv);
                        }
                        if (ulong.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ulv)) {
                            return BoxedValue.From(ulv);
                        }
                        return BoxedValue.FromString(s);
                    }
                case JsTag.String:
                    return BoxedValue.FromString(BytesToString(n.Bytes));
                case JsTag.DateTime: {
                        string s = BytesToString(n.Bytes);
                        if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt)) {
                            return BoxedValue.From(dt);
                        }
                        return BoxedValue.FromString(s);
                    }
                case JsTag.Binary:
                    return BoxedValue.FromObject(n.Bytes ?? Array.Empty<byte>());
                case JsTag.Array: {
                        int count = n.ChildCount;
                        var list = new List<BoxedValue>(count > 0 ? count : 0);
                        for (int k = 0; k < count; k++) {
                            list.Add(ReadValue(nodes, ref cur));
                        }
                        return BoxedValue.FromObject(list);
                    }
                case JsTag.Object: {
                        int count = n.ChildCount;
                        var dict = new Dictionary<BoxedValue, BoxedValue>();
                        for (int k = 0; k < count; k++) {
                            BoxedValue key = ReadValue(nodes, ref cur);
                            BoxedValue val = ReadValue(nodes, ref cur);
                            dict[key] = val;
                        }
                        return BoxedValue.FromObject(dict);
                    }
                default:
                    return BoxedValue.NullObject;
            }
        }

        private static string BytesToString(byte[]? bytes)
        {
            if (bytes == null || bytes.Length == 0) {
                return string.Empty;
            }
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
