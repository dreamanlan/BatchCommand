using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GlslRewriter
{
    public struct BoolVal
    {
        public bool Value = false;
        public bool IsValid = false;

        public BoolVal() { }
        public BoolVal(bool val) { Value = val; IsValid = true; }
        public BoolVal(bool val, bool isValid) {  Value = val; IsValid = isValid; }
    }
    public struct FloatVal
    {
        public float Value = 0;
        public bool IsValid = false;

        public FloatVal() { }
        public FloatVal(float val) { Value = val; IsValid = true; }
        public FloatVal(float val, bool isValid) { Value = val; IsValid = isValid; }
    }
    public struct IntVal
    {
        public int Value = 0;
        public bool IsValid = false;

        public IntVal() { }
        public IntVal(int val) { Value = val; IsValid = true; }
        public IntVal(int val, bool isValid) { Value = val; IsValid = isValid; }
    }
    public struct UintVal
    {
        public uint Value = 0;
        public bool IsValid = false;

        public UintVal() { }
        public UintVal(uint val) { Value = val; IsValid = true; }
        public UintVal(uint val, bool isValid) { Value = val; IsValid = isValid; }
    }
    public struct Bool4Val
    {
        public Bool4 Value = new Bool4();
        public bool IsValid = false;

        public Bool4Val() { }
        public Bool4Val(Bool4 val) { Value = val; IsValid = true; }
        public Bool4Val(Bool4 val, bool isValid) { Value = val; IsValid = isValid; }
    }
    public struct Float4Val
    {
        public Float4 Value = new Float4();
        public bool IsValid = false;

        public Float4Val() { }
        public Float4Val(Float4 val) { Value = val; IsValid = true; }
        public Float4Val(Float4 val, bool isValid) { Value = val; IsValid = isValid; }
    }
    public struct Int4Val
    {
        public Int4 Value = new Int4();
        public bool IsValid = false;

        public Int4Val() { }
        public Int4Val(Int4 val) { Value = val; IsValid = true; }
        public Int4Val(Int4 val, bool isValid) { Value = val; IsValid = isValid; }
    }
    public struct Uint4Val
    {
        public Uint4 Value = new Uint4();
        public bool IsValid = false;

        public Uint4Val() { }
        public Uint4Val(Uint4 val) { Value = val; IsValid = true; }
        public Uint4Val(Uint4 val, bool isValid) { Value = val; IsValid = isValid; }
    }
    public struct Bool2
    {
        public bool x = false;
        public bool y = false;

        public Bool2() { }
        public bool Assign(IList<string> vs)
        {
            bool ret = true;
            bool lv = false;
            if (vs.Count > 0) {
                if (Calculator.TryParseBool(vs[0], out lv)) {
                    x = lv;
                }
                else {
                    ret = false;
                }
            }
            if (vs.Count > 1) {
                if (Calculator.TryParseBool(vs[1], out lv)) {
                    y = lv;
                }
                else {
                    ret = false;
                }
            }
            else {
                y = lv;
            }
            return ret;
        }
        public bool SetMember(string m, string v)
        {
            bool ret = false;
            if (Calculator.TryParseBool(v, out var val)) {
                ret = true;
                if (m == "x")
                    x = val;
                else if (m == "y")
                    y = val;
                else if (int.TryParse(m, out var ix)) {
                    switch (ix) {
                        case 0:
                            x = val;
                            break;
                        case 1:
                            y = val;
                            break;
                        default:
                            ret = false;
                            break;
                    }
                }
                else {
                    ret = false;
                }
            }
            return ret;
        }
        public string GetMember(string m)
        {
            if (m == "x")
                return x.ToString();
            else if (m == "y")
                return y.ToString();
            else if (int.TryParse(m, out var ix)) {
                switch (ix) {
                    case 0:
                        return x.ToString();
                    case 1:
                        return y.ToString();
                }
            }
            return string.Empty;
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("bvec2(");
            sb.Append(x);
            sb.Append(",");
            sb.Append(y);
            sb.Append(")");
            return sb.ToString();
        }

        public static bool TryParse(string str, out Bool2 val)
        {
            const string c_key = "bvec2";

            bool succ = false;
            val = new Bool2();
            str = str.Trim();
            if (str.StartsWith(c_key) && str[str.Length - 1] == ')') {
                str = str.Substring(c_key.Length, str.Length - c_key.Length - 1).Trim();
                if (str.Length > 0 && str[0] == '(') {
                    str = str.Substring(1).TrimStart();
                    var vals = str.Split(",");
                    succ = val.Assign(vals);
                }
            }
            return succ;
        }
    }
    public struct Float2
    {
        public float x = 0;
        public float y = 0;

        public Float2() { }
        public bool Assign(IList<string> vs)
        {
            bool ret = true;
            float lv = 0;
            if (vs.Count > 0) {
                if (float.TryParse(vs[0], out lv)) {
                    x = lv;
                }
                else {
                    ret = false;
                }
            }
            if (vs.Count > 1) {
                if (float.TryParse(vs[1], out lv)) {
                    y = lv;
                }
                else {
                    ret = false;
                }
            }
            else {
                y = lv;
            }
            return ret;
        }
        public bool SetMember(string m, string v)
        {
            bool ret = false;
            if (float.TryParse(v, out var val)) {
                ret = true;
                if (m == "x")
                    x = val;
                else if (m == "y")
                    y = val;
                else if (int.TryParse(m, out var ix)) {
                    switch (ix) {
                        case 0:
                            x = val;
                            break;
                        case 1:
                            y = val;
                            break;
                        default:
                            ret = false;
                            break;
                    }
                }
                else {
                    ret = false;
                }
            }
            return ret;
        }
        public string GetMember(string m)
        {
            if (m == "x")
                return Calculator.FloatToString(x);
            else if (m == "y")
                return Calculator.FloatToString(y);
            else if (int.TryParse(m, out var ix)) {
                switch (ix) {
                    case 0:
                        return Calculator.FloatToString(x);
                    case 1:
                        return Calculator.FloatToString(y);
                }
            }
            return string.Empty;
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("vec2(");
            sb.Append(Calculator.FloatToString(x));
            sb.Append(",");
            sb.Append(Calculator.FloatToString(y));
            sb.Append(")");
            return sb.ToString();
        }

        public static bool TryParse(string str, out Float2 val)
        {
            const string c_key = "vec2";

            bool succ = false;
            val = new Float2();
            str = str.Trim();
            if (str.StartsWith(c_key) && str[str.Length - 1] == ')') {
                str = str.Substring(c_key.Length, str.Length - c_key.Length - 1).Trim();
                if (str.Length > 0 && str[0] == '(') {
                    str = str.Substring(1).TrimStart();
                    var vals = str.Split(",");
                    succ = val.Assign(vals);
                }
            }
            return succ;
        }
    }
    public struct Int2
    {
        public int x = 0;
        public int y = 0;

        public Int2() { }
        public bool Assign(IList<string> vs)
        {
            bool ret = true;
            int lv = 0;
            if (vs.Count > 0) {
                if (int.TryParse(vs[0], out lv)) {
                    x = lv;
                }
                else {
                    ret = false;
                }
            }
            if (vs.Count > 1) {
                if (int.TryParse(vs[1], out lv)) {
                    y = lv;
                }
                else {
                    ret = false;
                }
            }
            else {
                y = lv;
            }
            return ret;
        }
        public bool SetMember(string m, string v)
        {
            bool ret = false;
            if (int.TryParse(v, out var val)) {
                ret = true;
                if (m == "x")
                    x = val;
                else if (m == "y")
                    y = val;
                else if (int.TryParse(m, out var ix)) {
                    switch (ix) {
                        case 0:
                            x = val;
                            break;
                        case 1:
                            y = val;
                            break;
                        default:
                            ret = false;
                            break;
                    }
                }
                else {
                    ret = false;
                }
            }
            return ret;
        }
        public string GetMember(string m)
        {
            if (m == "x")
                return x.ToString();
            else if (m == "y")
                return y.ToString();
            else if (int.TryParse(m, out var ix)) {
                switch (ix) {
                    case 0:
                        return x.ToString();
                    case 1:
                        return y.ToString();
                }
            }
            return string.Empty;
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("ivec2(");
            sb.Append(x);
            sb.Append(",");
            sb.Append(y);
            sb.Append(")");
            return sb.ToString();
        }

        public static bool TryParse(string str, out Int2 val)
        {
            const string c_key = "ivec2";

            bool succ = false;
            val = new Int2();
            str = str.Trim();
            if (str.StartsWith(c_key) && str[str.Length - 1] == ')') {
                str = str.Substring(c_key.Length, str.Length - c_key.Length - 1).Trim();
                if (str.Length > 0 && str[0] == '(') {
                    str = str.Substring(1).TrimStart();
                    var vals = str.Split(",");
                    succ = val.Assign(vals);
                }
            }
            return succ;
        }
    }
    public struct Uint2
    {
        public uint x = 0;
        public uint y = 0;

        public Uint2() { }
        public bool Assign(IList<string> vs)
        {
            bool ret = true;
            uint lv = 0;
            if (vs.Count > 0) {
                if (uint.TryParse(vs[0], out lv)) {
                    x = lv;
                }
                else {
                    ret = false;
                }
            }
            if (vs.Count > 1) {
                if (uint.TryParse(vs[1], out lv)) {
                    y = lv;
                }
                else {
                    ret = false;
                }
            }
            else {
                y = lv;
            }
            return ret;
        }
        public bool SetMember(string m, string v)
        {
            bool ret = false;
            if (uint.TryParse(v, out var val)) {
                ret = true;
                if (m == "x")
                    x = val;
                else if (m == "y")
                    y = val;
                else if (int.TryParse(m, out var ix)) {
                    switch (ix) {
                        case 0:
                            x = val;
                            break;
                        case 1:
                            y = val;
                            break;
                        default:
                            ret = false;
                            break;
                    }
                }
                else {
                    ret = false;
                }
            }
            return ret;
        }
        public string GetMember(string m)
        {
            if (m == "x")
                return x.ToString();
            else if (m == "y")
                return y.ToString();
            else if (int.TryParse(m, out var ix)) {
                switch (ix) {
                    case 0:
                        return x.ToString();
                    case 1:
                        return y.ToString();
                }
            }
            return string.Empty;
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("uvec2(");
            sb.Append(x);
            sb.Append(",");
            sb.Append(y);
            sb.Append(")");
            return sb.ToString();
        }

        public static bool TryParse(string str, out Uint2 val)
        {
            const string c_key = "uvec2";

            bool succ = false;
            val = new Uint2();
            str = str.Trim();
            if (str.StartsWith(c_key) && str[str.Length - 1] == ')') {
                str = str.Substring(c_key.Length, str.Length - c_key.Length - 1).Trim();
                if (str.Length > 0 && str[0] == '(') {
                    str = str.Substring(1).TrimStart();
                    var vals = str.Split(",");
                    succ = val.Assign(vals);
                }
            }
            return succ;
        }
    }
    public struct Bool3
    {
        public bool x = false;
        public bool y = false;
        public bool z = false;

        public Bool3() { }
        public bool Assign(IList<string> vs)
        {
            bool ret = true;
            bool lv = false;
            if (vs.Count > 0) {
                if (Calculator.TryParseBool(vs[0], out lv)) {
                    x = lv;
                }
                else {
                    ret = false;
                }
            }
            if (vs.Count > 1) {
                if (Calculator.TryParseBool(vs[1], out lv)) {
                    y = lv;
                }
                else {
                    ret = false;
                }
            }
            else {
                y = lv;
            }
            if (vs.Count > 2) {
                if (Calculator.TryParseBool(vs[2], out lv)) {
                    z = lv;
                }
                else {
                    ret = false;
                }
            }
            else {
                z = lv;
            }
            return ret;
        }
        public bool SetMember(string m, string v)
        {
            bool ret = false;
            if (Calculator.TryParseBool(v, out var val)) {
                ret = true;
                if (m == "x")
                    x = val;
                else if (m == "y")
                    y = val;
                else if (m == "z")
                    z = val;
                else if (int.TryParse(m, out var ix)) {
                    switch (ix) {
                        case 0:
                            x = val;
                            break;
                        case 1:
                            y = val;
                            break;
                        case 2:
                            z = val;
                            break;
                        default:
                            ret = false;
                            break;
                    }
                }
                else {
                    ret = false;
                }
            }
            return ret;
        }
        public string GetMember(string m)
        {
            if (m == "x")
                return x.ToString();
            else if (m == "y")
                return y.ToString();
            else if (m == "z")
                return z.ToString();
            else if (int.TryParse(m, out var ix)) {
                switch (ix) {
                    case 0:
                        return x.ToString();
                    case 1:
                        return y.ToString();
                    case 2:
                        return z.ToString();
                }
            }
            return string.Empty;
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("bvec3(");
            sb.Append(x);
            sb.Append(",");
            sb.Append(y);
            sb.Append(",");
            sb.Append(z);
            sb.Append(")");
            return sb.ToString();
        }

        public static bool TryParse(string str, out Bool3 val)
        {
            const string c_key = "bvec3";

            bool succ = false;
            val = new Bool3();
            str = str.Trim();
            if (str.StartsWith(c_key) && str[str.Length - 1] == ')') {
                str = str.Substring(c_key.Length, str.Length - c_key.Length - 1).Trim();
                if (str.Length > 0 && str[0] == '(') {
                    str = str.Substring(1).TrimStart();
                    var vals = str.Split(",");
                    succ = val.Assign(vals);
                }
            }
            return succ;
        }
    }
    public struct Float3
    {
        public float x = 0;
        public float y = 0;
        public float z = 0;

        public Float3() { }
        public bool Assign(IList<string> vs)
        {
            bool ret = true;
            float lv = 0;
            if (vs.Count > 0) {
                if (float.TryParse(vs[0], out lv)) {
                    x = lv;
                }
                else {
                    ret = false;
                }
            }
            if (vs.Count > 1) {
                if (float.TryParse(vs[1], out lv)) {
                    y = lv;
                }
                else {
                    ret = false;
                }
            }
            else {
                y = lv;
            }
            if (vs.Count > 2) {
                if (float.TryParse(vs[2], out lv)) {
                    z = lv;
                }
                else {
                    ret = false;
                }
            }
            else {
                z = lv;
            }
            return ret;
        }
        public bool SetMember(string m, string v)
        {
            bool ret = false;
            if (float.TryParse(v, out var val)) {
                ret = true;
                if (m == "x")
                    x = val;
                else if (m == "y")
                    y = val;
                else if (m == "z")
                    z = val;
                else if (int.TryParse(m, out var ix)) {
                    switch (ix) {
                        case 0:
                            x = val;
                            break;
                        case 1:
                            y = val;
                            break;
                        case 2:
                            z = val;
                            break;
                        default:
                            ret = false;
                            break;
                    }
                }
                else {
                    ret = false;
                }
            }
            return ret;
        }
        public string GetMember(string m)
        {
            if (m == "x")
                return Calculator.FloatToString(x);
            else if (m == "y")
                return Calculator.FloatToString(y);
            else if (m == "z")
                return Calculator.FloatToString(z);
            else if (int.TryParse(m, out var ix)) {
                switch (ix) {
                    case 0:
                        return Calculator.FloatToString(x);
                    case 1:
                        return Calculator.FloatToString(y);
                    case 2:
                        return Calculator.FloatToString(z);
                }
            }
            return string.Empty;
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("vec3(");
            sb.Append(Calculator.FloatToString(x));
            sb.Append(",");
            sb.Append(Calculator.FloatToString(y));
            sb.Append(",");
            sb.Append(Calculator.FloatToString(z));
            sb.Append(")");
            return sb.ToString();
        }

        public static bool TryParse(string str, out Float3 val)
        {
            const string c_key = "vec3";

            bool succ = false;
            val = new Float3();
            str = str.Trim();
            if (str.StartsWith(c_key) && str[str.Length - 1] == ')') {
                str = str.Substring(c_key.Length, str.Length - c_key.Length - 1).Trim();
                if (str.Length > 0 && str[0] == '(') {
                    str = str.Substring(1).TrimStart();
                    var vals = str.Split(",");
                    succ = val.Assign(vals);
                }
            }
            return succ;
        }
    }
    public struct Int3
    {
        public int x = 0;
        public int y = 0;
        public int z = 0;

        public Int3() { }
        public bool Assign(IList<string> vs)
        {
            bool ret = true;
            int lv = 0;
            if (vs.Count > 0) {
                if (int.TryParse(vs[0], out lv)) {
                    x = lv;
                }
                else {
                    ret = false;
                }
            }
            if (vs.Count > 1) {
                if (int.TryParse(vs[1], out lv)) {
                    y = lv;
                }
                else {
                    ret = false;
                }
            }
            else {
                y = lv;
            }
            if (vs.Count > 2) {
                if (int.TryParse(vs[2], out lv)) {
                    z = lv;
                }
                else {
                    ret = false;
                }
            }
            else {
                z = lv;
            }
            return ret;
        }
        public bool SetMember(string m, string v)
        {
            bool ret = false;
            if (int.TryParse(v, out var val)) {
                ret = true;
                if (m == "x")
                    x = val;
                else if (m == "y")
                    y = val;
                else if (m == "z")
                    z = val;
                else if (int.TryParse(m, out var ix)) {
                    switch (ix) {
                        case 0:
                            x = val;
                            break;
                        case 1:
                            y = val;
                            break;
                        case 2:
                            z = val;
                            break;
                        default:
                            ret = false;
                            break;
                    }
                }
                else {
                    ret = false;
                }
            }
            return ret;
        }
        public string GetMember(string m)
        {
            if (m == "x")
                return x.ToString();
            else if (m == "y")
                return y.ToString();
            else if (m == "z")
                return z.ToString();
            else if (int.TryParse(m, out var ix)) {
                switch (ix) {
                    case 0:
                        return x.ToString();
                    case 1:
                        return y.ToString();
                    case 2:
                        return z.ToString();
                }
            }
            return string.Empty;
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("ivec3(");
            sb.Append(x);
            sb.Append(",");
            sb.Append(y);
            sb.Append(",");
            sb.Append(z);
            sb.Append(")");
            return sb.ToString();
        }

        public static bool TryParse(string str, out Int3 val)
        {
            const string c_key = "ivec3";

            bool succ = false;
            val = new Int3();
            str = str.Trim();
            if (str.StartsWith(c_key) && str[str.Length - 1] == ')') {
                str = str.Substring(c_key.Length, str.Length - c_key.Length - 1).Trim();
                if (str.Length > 0 && str[0] == '(') {
                    str = str.Substring(1).TrimStart();
                    var vals = str.Split(",");
                    succ = val.Assign(vals);
                }
            }
            return succ;
        }
    }
    public struct Uint3
    {
        public uint x = 0;
        public uint y = 0;
        public uint z = 0;

        public Uint3() { }
        public bool Assign(IList<string> vs)
        {
            bool ret = true;
            uint lv = 0;
            if (vs.Count > 0) {
                if (uint.TryParse(vs[0], out lv)) {
                    x = lv;
                }
                else {
                    ret = false;
                }
            }
            if (vs.Count > 1) {
                if (uint.TryParse(vs[1], out lv)) {
                    y = lv;
                }
                else {
                    ret = false;
                }
            }
            else {
                y = lv;
            }
            if (vs.Count > 2) {
                if (uint.TryParse(vs[2], out lv)) {
                    z = lv;
                }
                else {
                    ret = false;
                }
            }
            else {
                z = lv;
            }
            return ret;
        }
        public bool SetMember(string m, string v)
        {
            bool ret = false;
            if (uint.TryParse(v, out var val)) {
                ret = true;
                if (m == "x")
                    x = val;
                else if (m == "y")
                    y = val;
                else if (m == "z")
                    z = val;
                else if (int.TryParse(m, out var ix)) {
                    switch (ix) {
                        case 0:
                            x = val;
                            break;
                        case 1:
                            y = val;
                            break;
                        case 2:
                            z = val;
                            break;
                        default:
                            ret = false;
                            break;
                    }
                }
                else {
                    ret = false;
                }
            }
            return ret;
        }
        public string GetMember(string m)
        {
            if (m == "x")
                return x.ToString();
            else if (m == "y")
                return y.ToString();
            else if (m == "z")
                return z.ToString();
            else if (int.TryParse(m, out var ix)) {
                switch (ix) {
                    case 0:
                        return x.ToString();
                    case 1:
                        return y.ToString();
                    case 2:
                        return z.ToString();
                }
            }
            return string.Empty;
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("uvec3(");
            sb.Append(x);
            sb.Append(",");
            sb.Append(y);
            sb.Append(",");
            sb.Append(z);
            sb.Append(")");
            return sb.ToString();
        }

        public static bool TryParse(string str, out Uint3 val)
        {
            const string c_key = "uvec3";

            bool succ = false;
            val = new Uint3();
            str = str.Trim();
            if (str.StartsWith(c_key) && str[str.Length - 1] == ')') {
                str = str.Substring(c_key.Length, str.Length - c_key.Length - 1).Trim();
                if (str.Length > 0 && str[0] == '(') {
                    str = str.Substring(1).TrimStart();
                    var vals = str.Split(",");
                    succ = val.Assign(vals);
                }
            }
            return succ;
        }
    }
    public struct Bool4
    {
        public bool x = false;
        public bool y = false;
        public bool z = false;
        public bool w = false;

        public Bool4() { }
        public bool Assign(IList<string> vs)
        {
            bool ret = true;
            bool lv = false;
            if (vs.Count > 0) {
                if (Calculator.TryParseBool(vs[0], out lv)) {
                    x = lv;
                }
                else {
                    ret = false;
                }
            }
            if (vs.Count > 1) {
                if (Calculator.TryParseBool(vs[1], out lv)) {
                    y = lv;
                }
                else {
                    ret = false;
                }
            }
            else {
                y = lv;
            }
            if (vs.Count > 2) {
                if (Calculator.TryParseBool(vs[2], out lv)) {
                    z = lv;
                }
                else {
                    ret = false;
                }
            }
            else {
                z = lv;
            }
            if (vs.Count > 3) {
                if (Calculator.TryParseBool(vs[3], out lv)) {
                    w = lv;
                }
                else {
                    ret = false;
                }
            }
            else {
                w = lv;
            }
            return ret;
        }
        public bool SetMember(string m, string v)
        {
            bool ret = false;
            if (Calculator.TryParseBool(v, out var val)) {
                ret = true;
                if (m == "x")
                    x = val;
                else if (m == "y")
                    y = val;
                else if (m == "z")
                    z = val;
                else if (m == "w")
                    w = val;
                else if (int.TryParse(m, out var ix)) {
                    switch (ix) {
                        case 0:
                            x = val;
                            break;
                        case 1:
                            y = val;
                            break;
                        case 2:
                            z = val;
                            break;
                        case 3:
                            w = val;
                            break;
                        default:
                            ret = false;
                            break;
                    }
                }
                else {
                    ret = false;
                }
            }
            return ret;
        }
        public string GetMember(string m)
        {
            if (m == "x")
                return x.ToString();
            else if (m == "y")
                return y.ToString();
            else if (m == "z")
                return z.ToString();
            else if (m == "w")
                return w.ToString();
            else if (int.TryParse(m, out var ix)) {
                switch (ix) {
                    case 0:
                        return x.ToString();
                    case 1:
                        return y.ToString();
                    case 2:
                        return z.ToString();
                    case 3:
                        return w.ToString();
                }
            }
            return string.Empty;
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("bvec4(");
            sb.Append(x);
            sb.Append(",");
            sb.Append(y);
            sb.Append(",");
            sb.Append(z);
            sb.Append(",");
            sb.Append(w);
            sb.Append(")");
            return sb.ToString();
        }

        public static bool TryParse(string str, out Bool4 val)
        {
            const string c_key = "bvec4";

            bool succ = false;
            val = new Bool4();
            str = str.Trim();
            if (str.StartsWith(c_key) && str[str.Length - 1] == ')') {
                str = str.Substring(c_key.Length, str.Length - c_key.Length - 1).Trim();
                if (str.Length > 0 && str[0] == '(') {
                    str = str.Substring(1).TrimStart();
                    var vals = str.Split(",");
                    succ = val.Assign(vals);
                }
            }
            return succ;
        }
    }
    public struct Float4
    {
        public float x = 0;
        public float y = 0;
        public float z = 0;
        public float w = 0;

        public Float4() { }
        public bool Assign(IList<string> vs)
        {
            bool ret = true;
            float lv = 0;
            if (vs.Count > 0) {
                if (float.TryParse(vs[0], out lv)) {
                    x = lv;
                }
                else {
                    ret = false;
                }
            }
            if (vs.Count > 1) {
                if (float.TryParse(vs[1], out lv)) {
                    y = lv;
                }
                else {
                    ret = false;
                }
            }
            else {
                y = lv;
            }
            if (vs.Count > 2) {
                if (float.TryParse(vs[2], out lv)) {
                    z = lv;
                }
                else {
                    ret = false;
                }
            }
            else {
                z = lv;
            }
            if (vs.Count > 3) {
                if (float.TryParse(vs[3], out lv)) {
                    w = lv;
                }
                else {
                    ret = false;
                }
            }
            else {
                w = lv;
            }
            return ret;
        }
        public bool SetMember(string m, string v)
        {
            bool ret = false;
            if (float.TryParse(v, out var val)) {
                ret = true;
                if (m == "x")
                    x = val;
                else if (m == "y")
                    y = val;
                else if (m == "z")
                    z = val;
                else if (m == "w")
                    w = val;
                else if (int.TryParse(m, out var ix)) {
                    switch (ix) {
                        case 0:
                            x = val;
                            break;
                        case 1:
                            y = val;
                            break;
                        case 2:
                            z = val;
                            break;
                        case 3:
                            w = val;
                            break;
                        default:
                            ret = false;
                            break;
                    }
                }
                else {
                    ret = false;
                }
            }
            return ret;
        }
        public string GetMember(string m)
        {
            if (m == "x")
                return Calculator.FloatToString(x);
            else if (m == "y")
                return Calculator.FloatToString(y);
            else if (m == "z")
                return Calculator.FloatToString(z);
            else if (m == "w")
                return Calculator.FloatToString(w);
            else if (int.TryParse(m, out var ix)) {
                switch (ix) {
                    case 0:
                        return Calculator.FloatToString(x);
                    case 1:
                        return Calculator.FloatToString(y);
                    case 2:
                        return Calculator.FloatToString(z);
                    case 3:
                        return Calculator.FloatToString(w);
                }
            }
            return string.Empty;
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("vec4(");
            sb.Append(Calculator.FloatToString(x));
            sb.Append(",");
            sb.Append(Calculator.FloatToString(y));
            sb.Append(",");
            sb.Append(Calculator.FloatToString(z));
            sb.Append(",");
            sb.Append(Calculator.FloatToString(w));
            sb.Append(")");
            return sb.ToString();
        }

        public static bool TryParse(string str, out Float4 val)
        {
            const string c_key = "vec4";

            bool succ = false;
            val = new Float4();
            str = str.Trim();
            if (str.StartsWith(c_key) && str[str.Length - 1] == ')') {
                str = str.Substring(c_key.Length, str.Length - c_key.Length - 1).Trim();
                if (str.Length > 0 && str[0] == '(') {
                    str = str.Substring(1).TrimStart();
                    var vals = str.Split(",");
                    succ = val.Assign(vals);
                }
            }
            return succ;
        }
    }
    public struct Int4
    {
        public int x = 0;
        public int y = 0;
        public int z = 0;
        public int w = 0;

        public Int4() { }
        public bool Assign(IList<string> vs)
        {
            bool ret = true;
            int lv = 0;
            if (vs.Count > 0) {
                if (int.TryParse(vs[0], out lv)) {
                    x = lv;
                }
                else {
                    ret = false;
                }
            }
            if (vs.Count > 1) {
                if (int.TryParse(vs[1], out lv)) {
                    y = lv;
                }
                else {
                    ret = false;
                }
            }
            else {
                y = lv;
            }
            if (vs.Count > 2) {
                if (int.TryParse(vs[2], out lv)) {
                    z = lv;
                }
                else {
                    ret = false;
                }
            }
            else {
                z = lv;
            }
            if (vs.Count > 3) {
                if (int.TryParse(vs[3], out lv)) {
                    w = lv;
                }
                else {
                    ret = false;
                }
            }
            else {
                w = lv;
            }
            return ret;
        }
        public bool SetMember(string m, string v)
        {
            bool ret = false;
            if (int.TryParse(v, out var val)) {
                ret = true;
                if (m == "x")
                    x = val;
                else if (m == "y")
                    y = val;
                else if (m == "z")
                    z = val;
                else if (m == "w")
                    w = val;
                else if (int.TryParse(m, out var ix)) {
                    switch (ix) {
                        case 0:
                            x = val;
                            break;
                        case 1:
                            y = val;
                            break;
                        case 2:
                            z = val;
                            break;
                        case 3:
                            w = val;
                            break;
                        default:
                            ret = false;
                            break;
                    }
                }
                else {
                    ret = false;
                }
            }
            return ret;
        }
        public string GetMember(string m)
        {
            if (m == "x")
                return x.ToString();
            else if (m == "y")
                return y.ToString();
            else if (m == "z")
                return z.ToString();
            else if (m == "w")
                return w.ToString();
            else if (int.TryParse(m, out var ix)) {
                switch (ix) {
                    case 0:
                        return x.ToString();
                    case 1:
                        return y.ToString();
                    case 2:
                        return z.ToString();
                    case 3:
                        return w.ToString();
                }
            }
            return string.Empty;
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("ivec4(");
            sb.Append(x);
            sb.Append(",");
            sb.Append(y);
            sb.Append(",");
            sb.Append(z);
            sb.Append(",");
            sb.Append(w);
            sb.Append(")");
            return sb.ToString();
        }

        public static bool TryParse(string str, out Int4 val)
        {
            const string c_key = "ivec4";

            bool succ = false;
            val = new Int4();
            str = str.Trim();
            if (str.StartsWith(c_key) && str[str.Length - 1] == ')') {
                str = str.Substring(c_key.Length, str.Length - c_key.Length - 1).Trim();
                if (str.Length > 0 && str[0] == '(') {
                    str = str.Substring(1).TrimStart();
                    var vals = str.Split(",");
                    succ = val.Assign(vals);
                }
            }
            return succ;
        }
    }
    public struct Uint4
    {
        public uint x = 0;
        public uint y = 0;
        public uint z = 0;
        public uint w = 0;

        public Uint4() { }
        public bool Assign(IList<string> vs)
        {
            bool ret = true;
            uint lv = 0;
            if (vs.Count > 0) {
                if (uint.TryParse(vs[0], out lv)) {
                    x = lv;
                }
                else {
                    ret = false;
                }
            }
            if (vs.Count > 1) {
                if (uint.TryParse(vs[1], out lv)) {
                    y = lv;
                }
                else {
                    ret = false;
                }
            }
            else {
                y = lv;
            }
            if (vs.Count > 2) {
                if (uint.TryParse(vs[2], out lv)) {
                    z = lv;
                }
                else {
                    ret = false;
                }
            }
            else {
                z = lv;
            }
            if (vs.Count > 3) {
                if (uint.TryParse(vs[3], out lv)) {
                    w = lv;
                }
                else {
                    ret = false;
                }
            }
            else {
                w = lv;
            }
            return ret;
        }
        public bool SetMember(string m, string v)
        {
            bool ret = false;
            if (uint.TryParse(v, out var val)) {
                ret = true;
                if (m == "x")
                    x = val;
                else if (m == "y")
                    y = val;
                else if (m == "z")
                    z = val;
                else if (m == "w")
                    w = val;
                else if (int.TryParse(m, out var ix)) {
                    switch (ix) {
                        case 0:
                            x = val;
                            break;
                        case 1:
                            y = val;
                            break;
                        case 2:
                            z = val;
                            break;
                        case 3:
                            w = val;
                            break;
                        default:
                            ret = false;
                            break;
                    }
                }
                else {
                    ret = false;
                }
            }
            return ret;
        }
        public string GetMember(string m)
        {
            if (m == "x")
                return x.ToString();
            else if (m == "y")
                return y.ToString();
            else if (m == "z")
                return z.ToString();
            else if (m == "w")
                return w.ToString();
            else if(int.TryParse(m, out var ix)) {
                switch(ix) {
                    case 0:
                        return x.ToString();
                    case 1:
                        return y.ToString();
                    case 2:
                        return z.ToString();
                    case 3:
                        return w.ToString();
                }
            }
            return string.Empty;
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("uvec4(");
            sb.Append(x);
            sb.Append(",");
            sb.Append(y);
            sb.Append(",");
            sb.Append(z);
            sb.Append(",");
            sb.Append(w);
            sb.Append(")");
            return sb.ToString();
        }

        public static bool TryParse(string str, out Uint4 val)
        {
            const string c_key = "uvec4";

            bool succ = false;
            val = new Uint4();
            str = str.Trim();
            if (str.StartsWith(c_key) && str[str.Length - 1] == ')') {
                str = str.Substring(c_key.Length, str.Length - c_key.Length - 1).Trim();
                if (str.Length > 0 && str[0] == '(') {
                    str = str.Substring(1).TrimStart();
                    var vals = str.Split(",");
                    succ = val.Assign(vals);
                }
            }
            return succ;
        }
    }
    public static class VariableTable
    {
        public static void AllocVar(string name, string type)
        {
            string baseType = Program.GetTypeRemoveSuffix(type, out var suffix, out var arrNums);
            if (arrNums.Count > 0) {
                Debug.Assert(arrNums.Count == 1);
                int arrNum = arrNums[0];
                if (suffix.Length > 0) {
                    if (baseType == "vec") {
                        var fvals = new List<Float4Val>(arrNum);
                        for (int ix = 0; ix < arrNum; ++ix) {
                            fvals.Add(new Float4Val());
                        }
                        s_Float4ArrayVars[name] = fvals;
                    }
                    else if (baseType == "ivec") {
                        var ivals = new List<Int4Val>(arrNum);
                        for (int ix = 0; ix < arrNum; ++ix) {
                            ivals.Add(new Int4Val());
                        }
                        s_Int4ArrayVars[name] = ivals;
                    }
                    else if (baseType == "uvec") {
                        var uvals = new List<Uint4Val>(arrNum);
                        for (int ix = 0; ix < arrNum; ++ix) {
                            uvals.Add(new Uint4Val());
                        }
                        s_Uint4ArrayVars[name] = uvals;
                    }
                    else if (baseType == "bvec") {
                        var bvals = new List<Bool4Val>(arrNum);
                        for (int ix = 0; ix < arrNum; ++ix) {
                            bvals.Add(new Bool4Val());
                        }
                        s_Bool4ArrayVars[name] = bvals;
                    }
                }
                else {
                    if (baseType == "float") {
                        var fvals = new List<FloatVal>(arrNum);
                        for (int ix = 0; ix < arrNum; ++ix) {
                            fvals.Add(new FloatVal());
                        }
                        s_FloatArrayVars[name] = fvals;
                    }
                    else if (baseType == "int") {
                        var ivals = new List<IntVal>(arrNum);
                        for (int ix = 0; ix < arrNum; ++ix) {
                            ivals.Add(new IntVal());
                        }
                        s_IntArrayVars[name] = ivals;
                    }
                    else if (baseType == "uint") {
                        var uvals = new List<UintVal>(arrNum);
                        for (int ix = 0; ix < arrNum; ++ix) {
                            uvals.Add(new UintVal());
                        }
                        s_UintArrayVars[name] = uvals;
                    }
                    else if (baseType == "bool") {
                        var bvals = new List<BoolVal>(arrNum);
                        for (int ix = 0; ix < arrNum; ++ix) {
                            bvals.Add(new BoolVal());
                        }
                        s_BoolArrayVars[name] = bvals;
                    }
                }
            }
            /*
            else if (suffix.Length > 0) {
                if (baseType == "vec") {
                    s_Float4Vars[name] = new Float4();
                }
                else if (baseType == "ivec") {
                    s_Int4Vars[name] = new Int4();
                }
                else if (baseType == "uvec") {
                    s_Uint4Vars[name] = new Uint4();
                }
                else if (baseType == "bvec") {
                    s_Bool4Vars[name] = new Bool4();
                }
            }
            else {
                if (baseType == "float") {
                    s_FloatVars[name] = 0;
                }
                else if (baseType == "int") {
                    s_IntVars[name] = 0;
                }
                else if (baseType == "uint") {
                    s_UintVars[name] = 0;
                }
                else if (baseType == "bool") {
                    s_BoolVars[name] = false;
                }
            }
            */
        }
        public static bool GetVarValue(string name, string type, out string varVal)
        {
            bool exists = false;
            varVal = string.Empty;
            if (Config.ActiveConfig.SettingInfo.InvalidatedVariables.Contains(name))
                return false;
            string baseType = Program.GetTypeRemoveSuffix(type, out var suffix, out var arrNums);
            if (arrNums.Count > 0) {
                Debug.Assert(arrNums.Count == 1);
                int arrNum = arrNums[0];
                if (suffix.Length > 0) {
                    if (baseType == "vec") {
                        exists = s_Float4ArrayVars.ContainsKey(name);
                    }
                    else if (baseType == "ivec") {
                        exists = s_Int4ArrayVars.ContainsKey(name);
                    }
                    else if (baseType == "uvec") {
                        exists = s_Uint4ArrayVars.ContainsKey(name);
                    }
                    else if (baseType == "bvec") {
                        exists = s_Bool4ArrayVars.ContainsKey(name);
                    }
                }
                else {
                    if (baseType == "float") {
                        exists = s_FloatArrayVars.ContainsKey(name);
                    }
                    else if (baseType == "int") {
                        exists = s_IntArrayVars.ContainsKey(name);
                    }
                    else if (baseType == "uint") {
                        exists = s_UintArrayVars.ContainsKey(name);
                    }
                    else if (baseType == "bool") {
                        exists = s_BoolArrayVars.ContainsKey(name);
                    }
                }
            }
            else if (suffix.Length > 0) {
                if (baseType == "vec") {
                    if (s_Float4Vars.TryGetValue(name, out var val)) {
                        exists = true;
                        varVal = val.ToString();
                    }
                }
                else if (baseType == "ivec") {
                    if (s_Int4Vars.TryGetValue(name, out var val)) {
                        exists = true;
                        varVal = val.ToString();
                    }
                }
                else if (baseType == "uvec") {
                    if (s_Uint4Vars.TryGetValue(name, out var val)) {
                        exists = true;
                        varVal = val.ToString();
                    }
                }
                else if (baseType == "bvec") {
                    if (s_Bool4Vars.TryGetValue(name, out var val)) {
                        exists = true;
                        varVal = val.ToString();
                    }
                }
            }
            else {
                if (baseType == "float") {
                    if (s_FloatVars.TryGetValue(name, out var val)) {
                        exists = true;
                        varVal = Calculator.FloatToString(val);
                    }
                }
                else if (baseType == "int") {
                    if (s_IntVars.TryGetValue(name, out var val)) {
                        exists = true;
                        varVal = val.ToString();
                    }
                }
                else if (baseType == "uint") {
                    if (s_UintVars.TryGetValue(name, out var val)) {
                        exists = true;
                        varVal = val.ToString();
                    }
                }
                else if (baseType == "bool") {
                    if (s_BoolVars.TryGetValue(name, out var val)) {
                        exists = true;
                        varVal = val.ToString();
                    }
                }
            }
            if(!exists && Config.ActiveConfig.SettingInfo.VariableAssignments.TryGetValue(name, out var vinfo)) {
                Debug.Assert(type == vinfo.Type || type == "uint" && vinfo.Type == "int" || type == "int" && vinfo.Type == "uint");
                varVal = vinfo.Value;
                exists = true;
            }
            return exists;
        }
        public static bool ObjectGetValue(ComputeGraphVarNode left, string m, out string varVal)
        {
            bool exists = false;
            varVal = string.Empty;
            string name = left.VarName;
            string type = left.Type;
            if (Config.ActiveConfig.SettingInfo.InvalidatedVariables.Contains(name))
                return false;
            if(Config.ActiveConfig.SettingInfo.InvalidatedObjectMembers.TryGetValue(name, out var members) && members.Contains(m)) {
                return false;
            }
            string baseType = Program.GetTypeRemoveSuffix(type, out var suffix, out var arrNums);
            if (arrNums.Count == 0 && suffix.Length > 0) {
                if (baseType == "vec") {
                    if (s_Float4Vars.TryGetValue(name, out var val)) {
                        exists = true;
                        varVal = val.GetMember(m);
                    }
                }
                else if (baseType == "ivec") {
                    if (s_Int4Vars.TryGetValue(name, out var val)) {
                        exists = true;
                        varVal = val.GetMember(m);
                    }
                }
                else if (baseType == "uvec") {
                    if (s_Uint4Vars.TryGetValue(name, out var val)) {
                        exists = true;
                        varVal = val.GetMember(m);
                    }
                }
                else if (baseType == "bvec") {
                    if (s_Bool4Vars.TryGetValue(name, out var val)) {
                        exists = true;
                        varVal = val.GetMember(m);
                    }
                }
            }
            if (!exists && Config.ActiveConfig.SettingInfo.ObjectMemberAssignments.TryGetValue(name, out var objInfo) &&
                objInfo.TryGetValue(m, out var vinfo)) {
                Debug.Assert(type == vinfo.Type);
                varVal = vinfo.Value;
                exists = true;
            }
            return exists;
        }
        public static bool ArrayGetValue(ComputeGraphVarNode left, string ix, out string varVal)
        {
            bool exists = false;
            varVal = string.Empty;
            if (!int.TryParse(ix, out var index) || index < 0) {
                return exists;
            }
            string name = left.VarName;
            string type = left.Type;
            if (Config.ActiveConfig.SettingInfo.InvalidatedVariables.Contains(name))
                return false;
            if (Config.ActiveConfig.SettingInfo.InvalidatedArrayElements.TryGetValue(name, out var skipIndexes) && skipIndexes.Contains(index)) {
                return false;
            }
            string baseType = Program.GetTypeRemoveSuffix(type, out var suffix, out var arrNums);
            if (arrNums.Count > 0) {
                Debug.Assert(arrNums.Count == 1);
                int arrNum = arrNums[0];
                if (suffix.Length > 0) {
                    if (baseType == "vec") {
                        if (s_Float4ArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            exists = vals[index].IsValid;
                            varVal = vals[index].Value.ToString();
                        }
                    }
                    else if (baseType == "ivec") {
                        if (s_Int4ArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            exists = vals[index].IsValid;
                            varVal = vals[index].Value.ToString();
                        }
                    }
                    else if (baseType == "uvec") {
                        if (s_Uint4ArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            exists = vals[index].IsValid;
                            varVal = vals[index].Value.ToString();
                        }
                    }
                    else if (baseType == "bvec") {
                        if (s_Bool4ArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            exists = vals[index].IsValid;
                            varVal = vals[index].Value.ToString();
                        }
                    }
                }
                else {
                    if (baseType == "float") {
                        if (s_FloatArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            exists = vals[index].IsValid;
                            varVal = Calculator.FloatToString(vals[index].Value);
                        }
                    }
                    else if (baseType == "int") {
                        if (s_IntArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            exists = vals[index].IsValid;
                            varVal = vals[index].Value.ToString();
                        }
                    }
                    else if (baseType == "uint") {
                        if (s_UintArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            exists = vals[index].IsValid;
                            varVal = vals[index].Value.ToString();
                        }
                    }
                    else if (baseType == "bool") {
                        if (s_BoolArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            exists = vals[index].IsValid;
                            varVal = vals[index].Value.ToString();
                        }
                    }
                }
            }
            else if (suffix.Length > 0) {
                if (baseType == "vec") {
                    if (s_Float4Vars.TryGetValue(name, out var val)) {
                        exists = true;
                        varVal = val.GetMember(ix);
                    }
                }
                else if (baseType == "ivec") {
                    if (s_Int4Vars.TryGetValue(name, out var val)) {
                        exists = true;
                        varVal = val.GetMember(ix);
                    }
                }
                else if (baseType == "uvec") {
                    if (s_Uint4Vars.TryGetValue(name, out var val)) {
                        exists = true;
                        varVal = val.GetMember(ix);
                    }
                }
                else if (baseType == "bvec") {
                    if (s_Bool4Vars.TryGetValue(name, out var val)) {
                        exists = true;
                        varVal = val.GetMember(ix);
                    }
                }
            }
            if (!exists && Config.ActiveConfig.SettingInfo.ArrayElementAssignments.TryGetValue(name, out var arrInfo) &&
                arrInfo.TryGetValue(index, out var vinfo)) {
                Debug.Assert(type == vinfo.Type);
                varVal = vinfo.Value;
                exists = true;
            }
            return exists;
        }
        public static bool ObjectArrayGetValue(ComputeGraphVarNode left, string ix, string m, out string varVal)
        {
            bool exists = false;
            varVal = string.Empty;
            if (!int.TryParse(ix, out var index) || index < 0) {
                return exists;
            }
            string name = left.VarName;
            string type = left.Type;
            if (Config.ActiveConfig.SettingInfo.InvalidatedVariables.Contains(name))
                return false;
            if (Config.ActiveConfig.SettingInfo.InvalidatedArrayElements.TryGetValue(name, out var skipIndexes) && skipIndexes.Contains(index)) {
                return false;
            }
            if (Config.ActiveConfig.SettingInfo.InvalidatedObjectMembers.TryGetValue(name, out var members) && members.Contains(m)) {
                return false;
            }
            if (Config.ActiveConfig.SettingInfo.InvalidatedObjectArrayMembers.TryGetValue(name, out var list) && list.TryGetValue(index, out var hashSet) && hashSet.Contains(m)) {
                return false;
            }
            string baseType = Program.GetTypeRemoveSuffix(type, out var suffix, out var arrNums);
            if (arrNums.Count > 0) {
                Debug.Assert(arrNums.Count == 1);
                int arrNum = arrNums[0];
                if (suffix.Length > 0) {
                    if (baseType == "vec") {
                        if (s_Float4ArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            exists = vals[index].IsValid;
                            varVal = vals[index].Value.GetMember(m);
                        }
                    }
                    else if (baseType == "ivec") {
                        if (s_Int4ArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            exists = vals[index].IsValid;
                            varVal = vals[index].Value.GetMember(m);
                        }
                    }
                    else if (baseType == "uvec") {
                        if (s_Uint4ArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            exists = vals[index].IsValid;
                            varVal = vals[index].Value.GetMember(m);
                        }
                    }
                    else if (baseType == "bvec") {
                        if (s_Bool4ArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            exists = vals[index].IsValid;
                            varVal = vals[index].Value.GetMember(m);
                        }
                    }
                }
            }
            if (!exists && Config.ActiveConfig.SettingInfo.ObjectArrayMemberAssignments.TryGetValue(name, out var arrInfo) &&
                arrInfo.TryGetValue(index, out var objInfo) &&
                objInfo.TryGetValue(m, out var vinfo)) {
                Debug.Assert(type == vinfo.Type);
                varVal = vinfo.Value;
                exists = true;
            }
            return exists;
        }
        public static void AssignValue(ComputeGraphVarNode left, string right)
        {
            string name = left.VarName;
            string type = left.Type;
            string baseType = Program.GetTypeRemoveSuffix(type, out var suffix, out var arrNums);
            if (arrNums.Count > 0) {
                Debug.Assert(arrNums.Count == 1);
                int arrNum = arrNums[0];
                if (suffix.Length > 0) {
                    if (baseType == "vec") {
                        s_Float4ArrayVars.Remove(name);
                    }
                    else if (baseType == "ivec") {
                        s_Int4ArrayVars.Remove(name);
                    }
                    else if (baseType == "uvec") {
                        s_Uint4ArrayVars.Remove(name);
                    }
                    else if (baseType == "bvec") {
                        s_Bool4ArrayVars.Remove(name);
                    }
                }
                else {
                    if (baseType == "float") {
                        s_FloatArrayVars.Remove(name);
                    }
                    else if (baseType == "int") {
                        s_IntArrayVars.Remove(name);
                    }
                    else if (baseType == "uint") {
                        s_UintArrayVars.Remove(name);
                    }
                    else if (baseType == "bool") {
                        s_BoolArrayVars.Remove(name);
                    }
                }
            }
            else{
                if (suffix.Length > 0) {
                    if (baseType == "vec") {
                        s_Float4Vars.Remove(name);
                    }
                    else if (baseType == "ivec") {
                        s_Int4Vars.Remove(name);
                    }
                    else if (baseType == "uvec") {
                        s_Uint4Vars.Remove(name);
                    }
                    else if (baseType == "bvec") {
                        s_Bool4Vars.Remove(name);
                    }
                }
                else {
                    if (baseType == "float") {
                        if (float.TryParse(right, out var val))
                            s_FloatVars[name] = val;
                        else
                            s_FloatVars.Remove(name);
                    }
                    else if (baseType == "int") {
                        if (int.TryParse(right, out var val))
                            s_IntVars[name] = val;
                        else
                            s_IntVars.Remove(name);
                    }
                    else if (baseType == "uint") {
                        if (uint.TryParse(right, out var val))
                            s_UintVars[name] = val;
                        else
                            s_UintVars.Remove(name);
                    }
                    else if (baseType == "bool") {
                        if (Calculator.TryParseBool(right, out var val))
                            s_BoolVars[name] = val;
                        else
                            s_BoolVars.Remove(name);
                    }
                }
            }
        }
        public static void AssignValue(ComputeGraphVarNode left, ComputeGraphVarNode right)
        {
            string name = left.VarName;
            string type = left.Type;
            string otherName = right.VarName;
            string otherType = right.Type;
            if (type == otherType) {
                string baseType = Program.GetTypeRemoveSuffix(type, out var suffix, out var arrNums);
                if (arrNums.Count > 0) {
                    Debug.Assert(arrNums.Count == 1);
                    int arrNum = arrNums[0];
                    if (suffix.Length > 0) {
                        if (baseType == "vec") {
                            if (s_Float4ArrayVars.TryGetValue(otherName, out var vals)) {
                                s_Float4ArrayVars[name] = new List<Float4Val>(vals);
                            }
                            else {
                                s_Float4ArrayVars.Remove(name);
                            }
                        }
                        else if (baseType == "ivec") {
                            if (s_Int4ArrayVars.TryGetValue(otherName, out var vals)) {
                                s_Int4ArrayVars[name] = new List<Int4Val>(vals);
                            }
                            else {
                                s_Int4ArrayVars.Remove(name);
                            }
                        }
                        else if (baseType == "uvec") {
                            if (s_Uint4ArrayVars.TryGetValue(otherName, out var vals)) {
                                s_Uint4ArrayVars[name] = new List<Uint4Val>(vals);
                            }
                            else {
                                s_Uint4ArrayVars.Remove(name);
                            }
                        }
                        else if (baseType == "bvec") {
                            if (s_Bool4ArrayVars.TryGetValue(otherName, out var vals)) {
                                s_Bool4ArrayVars[name] = new List<Bool4Val>(vals);
                            }
                            else {
                                s_Bool4ArrayVars.Remove(name);
                            }
                        }
                    }
                    else {
                        if (baseType == "float") {
                            if (s_FloatArrayVars.TryGetValue(otherName, out var vals)) {
                                s_FloatArrayVars[name] = new List<FloatVal>(vals);
                            }
                            else {
                                s_FloatArrayVars.Remove(name);
                            }
                        }
                        else if (baseType == "int") {
                            if (s_IntArrayVars.TryGetValue(otherName, out var vals)) {
                                s_IntArrayVars[name] = new List<IntVal>(vals);
                            }
                            else {
                                s_IntArrayVars.Remove(name);
                            }
                        }
                        else if (baseType == "uint") {
                            if (s_UintArrayVars.TryGetValue(otherName, out var vals)) {
                                s_UintArrayVars[name] = new List<UintVal>(vals);
                            }
                            else {
                                s_UintArrayVars.Remove(name);
                            }
                        }
                        else if (baseType == "bool") {
                            if (s_BoolArrayVars.TryGetValue(otherName, out var vals)) {
                                s_BoolArrayVars[name] = new List<BoolVal>(vals);
                            }
                            else {
                                s_BoolArrayVars.Remove(name);
                            }
                        }
                    }
                }
                else if (suffix.Length > 0) {
                    if (baseType == "vec") {
                        if (s_Float4Vars.TryGetValue(otherName, out var val)) {
                            s_Float4Vars[name] = val;
                        }
                        else {
                            s_Float4Vars.Remove(name);
                        }
                    }
                    else if (baseType == "ivec") {
                        if (s_Int4Vars.TryGetValue(otherName, out var val)) {
                            s_Int4Vars[name] = val;
                        }
                        else {
                            s_Int4Vars.Remove(name);
                        }
                    }
                    else if (baseType == "uvec") {
                        if (s_Uint4Vars.TryGetValue(otherName, out var val)) {
                            s_Uint4Vars[name] = val;
                        }
                        else {
                            s_Uint4Vars.Remove(name);
                        }
                    }
                    else if (baseType == "bvec") {
                        if (s_Bool4Vars.TryGetValue(otherName, out var val)) {
                            s_Bool4Vars[name] = val;
                        }
                        else {
                            s_Bool4Vars.Remove(name);
                        }
                    }
                }
                else {
                    if (baseType == "float") {
                        if (s_FloatVars.TryGetValue(otherName, out var val)) {
                            s_FloatVars[name] = val;
                        }
                        else {
                            s_Float4Vars.Remove(name);
                        }
                    }
                    else if (baseType == "int") {
                        if (s_IntVars.TryGetValue(otherName, out var val)) {
                            s_IntVars[name] = val;
                        }
                        else {
                            s_Int4Vars.Remove(name);
                        }
                    }
                    else if (baseType == "uint") {
                        if (s_UintVars.TryGetValue(otherName, out var val)) {
                            s_UintVars[name] = val;
                        }
                        else {
                            s_Uint4Vars.Remove(name);
                        }
                    }
                    else if (baseType == "bool") {
                        if (s_BoolVars.TryGetValue(otherName, out var val)) {
                            s_BoolVars[name] = val;
                        }
                        else {
                            s_BoolVars.Remove(name);
                        }
                    }
                }
            }
        }
        public static void AssignValue(ComputeGraphVarNode left, ComputeGraphCalcNode right, HashSet<ComputeGraphNode> visits)
        {
            bool isConst = true;
            List<string> consts = new List<string>();
            foreach(var n in right.PrevNodes) {
                if(n is ComputeGraphConstNode cn) {
                    consts.Add(cn.Value);
                }
                else {
                    isConst = false;
                    break;
                }
            }
            if (!isConst) {
                AssignValue(left, right.CalcValue(visits));
                return;
            }
            string name = left.VarName;
            string type = left.Type;
            string baseType = Program.GetTypeRemoveSuffix(type, out var suffix, out var arrNums);
            if (arrNums.Count > 0) {
                Debug.Assert(arrNums.Count == 1);
                int arrNum = arrNums[0];
                if (suffix.Length > 0) {
                    if (baseType == "vec") {
                        s_Float4ArrayVars.Remove(name);
                    }
                    else if (baseType == "ivec") {
                        s_Int4ArrayVars.Remove(name);
                    }
                    else if (baseType == "uvec") {
                        s_Uint4ArrayVars.Remove(name);
                    }
                    else if (baseType == "bvec") {
                        s_Bool4ArrayVars.Remove(name);
                    }
                }
                else {
                    if (baseType == "float") {
                        s_FloatArrayVars.Remove(name);
                    }
                    else if (baseType == "int") {
                        s_IntArrayVars.Remove(name);
                    }
                    else if (baseType == "uint") {
                        s_UintArrayVars.Remove(name);
                    }
                    else if (baseType == "bool") {
                        s_BoolArrayVars.Remove(name);
                    }
                }
            }
            else {
                if (suffix.Length > 0) {
                    if (baseType == "vec") {
                        var val = new Float4();
                        if (val.Assign(consts))
                            s_Float4Vars[name] = val;
                        else
                            s_Float4Vars.Remove(name);
                    }
                    else if (baseType == "ivec") {
                        var val = new Int4();
                        if (val.Assign(consts))
                            s_Int4Vars[name] = val;
                        else
                            s_Int4Vars.Remove(name);
                    }
                    else if (baseType == "uvec") {
                        var val = new Uint4();
                        if (val.Assign(consts))
                            s_Uint4Vars[name] = val;
                        else
                            s_Uint4Vars.Remove(name);
                    }
                    else if (baseType == "bvec") {
                        var val = new Bool4();
                        if (val.Assign(consts))
                            s_Bool4Vars[name] = val;
                        else
                            s_Bool4Vars.Remove(name);
                    }
                }
                else {
                    if (baseType == "float") {
                        if (float.TryParse(consts[0], out var val))
                            s_FloatVars[name] = val;
                        else
                            s_FloatVars.Remove(name);
                    }
                    else if (baseType == "int") {
                        if (int.TryParse(consts[0], out var val))
                            s_IntVars[name] = val;
                        else
                            s_Int4Vars.Remove(name);
                    }
                    else if (baseType == "uint") {
                        if (uint.TryParse(consts[0], out var val))
                            s_UintVars[name] = val;
                        else
                            s_UintVars.Remove(name);
                    }
                    else if (baseType == "bool") {
                        if (Calculator.TryParseBool(consts[0], out var val))
                            s_BoolVars[name] = val;
                        else
                            s_BoolVars.Remove(name);
                    }
                }
            }
        }
        public static void ObjectAssignValue(ComputeGraphVarNode left, string m, string right)
        {
            string name = left.VarName;
            string type = left.Type;
            string baseType = Program.GetTypeRemoveSuffix(type, out var suffix, out var arrNums);
            if (arrNums.Count == 0 && suffix.Length > 0) {
                if (baseType == "vec") {
                    if (s_Float4Vars.TryGetValue(name, out var val)) {
                        if (val.SetMember(m, right))
                            s_Float4Vars[name] = val;
                        else
                            s_Float4Vars.Remove(name);
                    }
                    else {
                        val = new Float4();
                        if (val.SetMember(m, right))
                            s_Float4Vars[name] = val;
                    }
                }
                else if (baseType == "ivec") {
                    if (s_Int4Vars.TryGetValue(name, out var val)) {
                        if (val.SetMember(m, right))
                            s_Int4Vars[name] = val;
                        else
                            s_Int4Vars.Remove(name);
                    }
                    else {
                        val = new Int4();
                        if (val.SetMember(m, right))
                            s_Int4Vars[name] = val;
                    }
                }
                else if (baseType == "uvec") {
                    if (s_Uint4Vars.TryGetValue(name, out var val)) {
                        if (val.SetMember(m, right))
                            s_Uint4Vars[name] = val;
                        else
                            s_Uint4Vars.Remove(name);
                    }
                    else {
                        val = new Uint4();
                        if (val.SetMember(m, right))
                            s_Uint4Vars[name] = val;
                    }
                }
                else if (baseType == "bvec") {
                    if (s_Bool4Vars.TryGetValue(name, out var val)) {
                        if (val.SetMember(m, right))
                            s_Bool4Vars[name] = val;
                        else
                            s_Bool4Vars.Remove(name);
                    }
                    else {
                        val = new Bool4();
                        if (val.SetMember(m, right))
                            s_Bool4Vars[name] = val;
                    }
                }
            }
        }
        public static void ArrayAssignValue(ComputeGraphVarNode left, string ix, string right)
        {
            if (!int.TryParse(ix, out var index) || index < 0) {
                return;
            }
            string name = left.VarName;
            string type = left.Type;
            string baseType = Program.GetTypeRemoveSuffix(type, out var suffix, out var arrNums);
            if (arrNums.Count > 0) {
                Debug.Assert(arrNums.Count == 1);
                int arrNum = arrNums[0];
                if (suffix.Length == 0) {
                    if (baseType == "float") {
                        if (s_FloatArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            if (float.TryParse(right, out var val)) {
                                vals[index] = new FloatVal(val);
                            }
                            else {
                                vals[index] = new FloatVal(val, false);
                            }
                        }
                    }
                    else if (baseType == "int") {
                        if (s_IntArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            if (int.TryParse(right, out var val)) {
                                vals[index] = new IntVal(val);
                            }
                            else {
                                vals[index] = new IntVal(val, false);
                            }
                        }
                    }
                    else if (baseType == "uint") {
                        if (s_UintArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            if (uint.TryParse(right, out var val)) {
                                vals[index] = new UintVal(val);
                            }
                            else {
                                vals[index] = new UintVal(val, false);
                            }
                        }
                    }
                    else if (baseType == "bool") {
                        if (s_BoolArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            if (Calculator.TryParseBool(right, out var val)) {
                                vals[index] = new BoolVal(val);
                            }
                            else {
                                vals[index] = new BoolVal(val, false);
                            }
                        }
                    }
                }
            }
            else if (suffix.Length > 0) {
                if (baseType == "vec") {
                    if (s_Float4Vars.TryGetValue(name, out var val)) {
                        if (val.SetMember(ix, right))
                            s_Float4Vars[name] = val;
                        else
                            s_Float4Vars.Remove(name);
                    }
                }
                else if (baseType == "ivec") {
                    if (s_Int4Vars.TryGetValue(name, out var val)) {
                        if (val.SetMember(ix, right))
                            s_Int4Vars[name] = val;
                        else
                            s_Int4Vars.Remove(name);
                    }
                }
                else if (baseType == "uvec") {
                    if (s_Uint4Vars.TryGetValue(name, out var val)) {
                        if (val.SetMember(ix, right))
                            s_Uint4Vars[name] = val;
                        else
                            s_Uint4Vars.Remove(name);
                    }
                }
                else if (baseType == "bvec") {
                    if (s_Bool4Vars.TryGetValue(name, out var val)) {
                        if (val.SetMember(ix, right))
                            s_Bool4Vars[name] = val;
                        else
                            s_Bool4Vars.Remove(name);
                    }
                }
            }
        }
        public static void ArrayAssignValue(ComputeGraphVarNode left, string ix, ComputeGraphVarNode right)
        {
            if (!int.TryParse(ix, out var index) || index < 0) {
                return;
            }
            string name = left.VarName;
            string type = left.Type;
            string otherName = right.VarName;
            string otherType = right.Type;
            string baseType = Program.GetTypeRemoveSuffix(type, out var suffix, out var arrNums);
            string baseType2 = Program.GetTypeRemoveSuffix(otherType, out var suffix2, out var arrNums2);
            if (baseType == baseType2 && suffix == suffix2) {
                if (arrNums.Count > 0) {
                    Debug.Assert(arrNums.Count == 1);
                    int arrNum = arrNums[0];
                    if (suffix.Length > 0) {
                        if (baseType == "vec") {
                            if (s_Float4ArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                                if (s_Float4Vars.TryGetValue(otherName, out var val)) {
                                    vals[index] = new Float4Val(val);
                                }
                                else {
                                    vals[index] = new Float4Val(val, false);
                                }
                            }
                        }
                        else if (baseType == "ivec") {
                            if (s_Int4ArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                                if (s_Int4Vars.TryGetValue(otherName, out var val)) {
                                    vals[index] = new Int4Val(val);
                                }
                                else {
                                    vals[index] = new Int4Val(val, false);
                                }
                            }
                        }
                        else if (baseType == "uvec") {
                            if (s_Uint4ArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                                if (s_Uint4Vars.TryGetValue(otherName, out var val)) {
                                    vals[index] = new Uint4Val(val);
                                }
                                else {
                                    vals[index] = new Uint4Val(val, false);
                                }
                            }
                        }
                        else if (baseType == "bvec") {
                            if (s_Bool4ArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                                if (s_Bool4Vars.TryGetValue(otherName, out var val)) {
                                    vals[index] = new Bool4Val(val);
                                }
                                else {
                                    vals[index] = new Bool4Val(val, false);
                                }
                            }
                        }
                    }
                    else {
                        if (baseType == "float") {
                            if (s_FloatArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                                if (s_FloatVars.TryGetValue(otherName, out var val)) {
                                    vals[index] = new FloatVal(val);
                                }
                                else {
                                    vals[index] = new FloatVal(val, false);
                                }
                            }
                        }
                        else if (baseType == "int") {
                            if (s_IntArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                                if (s_IntVars.TryGetValue(otherName, out var val)) {
                                    vals[index] = new IntVal(val);
                                }
                                else {
                                    vals[index] = new IntVal(val, false);
                                }
                            }
                        }
                        else if (baseType == "uint") {
                            if (s_UintArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                                if (s_UintVars.TryGetValue(otherName, out var val)) {
                                    vals[index] = new UintVal(val);
                                }
                                else {
                                    vals[index] = new UintVal(val, false);
                                }
                            }
                        }
                        else if (baseType == "bool") {
                            if (s_BoolArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                                if (s_BoolVars.TryGetValue(otherName, out var val)) {
                                    vals[index] = new BoolVal(val);
                                }
                                else {
                                    vals[index] = new BoolVal(val, false);
                                }
                            }
                        }
                    }
                }
            }
            else if (suffix.Length > 0) {
                if (baseType == "vec") {
                    if (s_Float4Vars.TryGetValue(name, out var val)) {
                        if (s_FloatVars.TryGetValue(otherName, out var val2) && val.SetMember(ix, val2.ToString()))
                            s_Float4Vars[name] = val;
                        else
                            s_Float4Vars.Remove(name);
                    }
                }
                else if (baseType == "ivec") {
                    if (s_Int4Vars.TryGetValue(name, out var val)) {
                        if (s_IntVars.TryGetValue(otherName, out var val2) && val.SetMember(ix, val2.ToString()))
                            s_Int4Vars[name] = val;
                        else
                            s_Int4Vars.Remove(name);
                    }
                }
                else if (baseType == "uvec") {
                    if (s_Uint4Vars.TryGetValue(name, out var val)) {
                        if (s_UintVars.TryGetValue(otherName, out var val2) && val.SetMember(ix, val2.ToString()))
                            s_Uint4Vars[name] = val;
                        else
                            s_Uint4Vars.Remove(name);
                    }
                }
                else if (baseType == "bvec") {
                    if (s_Bool4Vars.TryGetValue(name, out var val)) {
                        if (s_BoolVars.TryGetValue(otherName, out var val2) && val.SetMember(ix, val2.ToString()))
                            s_Bool4Vars[name] = val;
                        else
                            s_Bool4Vars.Remove(name);
                    }
                }
            }
        }
        public static void ArrayAssignValue(ComputeGraphVarNode left, string ix, ComputeGraphCalcNode right, HashSet<ComputeGraphNode> visits)
        {
            bool isConst = true;
            List<string> consts = new List<string>();
            foreach (var n in right.PrevNodes) {
                if (n is ComputeGraphConstNode cn) {
                    consts.Add(cn.Value);
                }
                else {
                    isConst = false;
                    break;
                }
            }
            if (!isConst) {
                ArrayAssignValue(left, ix, right.CalcValue(visits));
                return;
            }
            if (!int.TryParse(ix, out var index) || index < 0) {
                return;
            }
            string name = left.VarName;
            string type = left.Type;
            string baseType = Program.GetTypeRemoveSuffix(type, out var suffix, out var arrNums);
            if (arrNums.Count > 0) {
                Debug.Assert(arrNums.Count == 1);
                int arrNum = arrNums[0];
                if (suffix.Length > 0) {
                    if (baseType == "vec") {
                        if (s_Float4ArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            var val = new Float4();
                            if (val.Assign(consts))
                                vals[index] = new Float4Val(val);
                            else
                                vals[index] = new Float4Val(val, false);
                        }
                    }
                    else if (baseType == "ivec") {
                        if (s_Int4ArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            var val = new Int4();
                            if (val.Assign(consts))
                                vals[index] = new Int4Val(val);
                            else
                                vals[index] = new Int4Val(val, false);
                        }
                    }
                    else if (baseType == "uvec") {
                        if (s_Uint4ArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            var val = new Uint4();
                            if (val.Assign(consts))
                                vals[index] = new Uint4Val(val);
                            else
                                vals[index] = new Uint4Val(val, false);
                        }
                    }
                    else if (baseType == "bvec") {
                        if (s_Bool4ArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            var val = new Bool4();
                            if (val.Assign(consts))
                                vals[index] = new Bool4Val(val);
                            else
                                vals[index] = new Bool4Val(val, false);
                        }
                    }
                }
                else {
                    if (baseType == "float") {
                        if (s_FloatArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            if (float.TryParse(consts[0], out var val)) {
                                vals[index] = new FloatVal(val);
                            }
                            else {
                                vals[index] = new FloatVal(val, false);
                            }
                        }
                    }
                    else if (baseType == "int") {
                        if (s_IntArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            if (int.TryParse(consts[0], out var val)) {
                                vals[index] = new IntVal(val);
                            }
                            else {
                                vals[index] = new IntVal(val, false);
                            }
                        }
                    }
                    else if (baseType == "uint") {
                        if (s_UintArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            if (uint.TryParse(consts[0], out var val)) {
                                vals[index] = new UintVal(val);
                            }
                            else {
                                vals[index] = new UintVal(val, false);
                            }
                        }
                    }
                }
            }
            else if (suffix.Length > 0) {
                if (baseType == "vec") {
                    if (s_Float4Vars.TryGetValue(name, out var val)) {
                        if (val.SetMember(ix, consts[0]))
                            s_Float4Vars[name] = val;
                        else
                            s_Float4Vars.Remove(name);
                    }
                }
                else if (baseType == "ivec") {
                    if (s_Int4Vars.TryGetValue(name, out var val)) {
                        if (val.SetMember(ix, consts[0]))
                            s_Int4Vars[name] = val;
                        else
                            s_Int4Vars.Remove(name);
                    }
                }
                else if (baseType == "uvec") {
                    if (s_Uint4Vars.TryGetValue(name, out var val)) {
                        if (val.SetMember(ix, consts[0]))
                            s_Uint4Vars[name] = val;
                        else
                            s_Uint4Vars.Remove(name);
                    }
                }
                else if (baseType == "bvec") {
                    if (s_Bool4Vars.TryGetValue(name, out var val)) {
                        if (val.SetMember(ix, consts[0]))
                            s_Bool4Vars[name] = val;
                        else
                            s_Bool4Vars.Remove(name);
                    }
                }
            }
        }

        public static Dictionary<string, bool> s_BoolVars = new Dictionary<string, bool>();
        public static Dictionary<string, float> s_FloatVars = new Dictionary<string, float>();
        public static Dictionary<string, int> s_IntVars = new Dictionary<string, int>();
        public static Dictionary<string, uint> s_UintVars = new Dictionary<string, uint>();
        public static Dictionary<string, Bool4> s_Bool4Vars = new Dictionary<string, Bool4>();
        public static Dictionary<string, Float4> s_Float4Vars = new Dictionary<string, Float4>();
        public static Dictionary<string, Int4> s_Int4Vars = new Dictionary<string, Int4>();
        public static Dictionary<string, Uint4> s_Uint4Vars = new Dictionary<string, Uint4>();
        public static Dictionary<string, List<BoolVal>> s_BoolArrayVars = new Dictionary<string, List<BoolVal>>();
        public static Dictionary<string, List<FloatVal>> s_FloatArrayVars = new Dictionary<string, List<FloatVal>>();
        public static Dictionary<string, List<IntVal>> s_IntArrayVars = new Dictionary<string, List<IntVal>>();
        public static Dictionary<string, List<UintVal>> s_UintArrayVars = new Dictionary<string, List<UintVal>>();
        public static Dictionary<string, List<Bool4Val>> s_Bool4ArrayVars = new Dictionary<string, List<Bool4Val>>();
        public static Dictionary<string, List<Float4Val>> s_Float4ArrayVars = new Dictionary<string, List<Float4Val>>();
        public static Dictionary<string, List<Int4Val>> s_Int4ArrayVars = new Dictionary<string, List<Int4Val>>();
        public static Dictionary<string, List<Uint4Val>> s_Uint4ArrayVars = new Dictionary<string, List<Uint4Val>>();
    }
}
