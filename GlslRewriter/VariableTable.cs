using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GlslRewriter
{
    public class BoolVal
    {
        public bool Value = false;
        public bool IsValid = false;

        public BoolVal() { }
        public BoolVal(bool val) { Value = val; IsValid = true; }
        public BoolVal(bool val, bool isValid) {  Value = val; IsValid = isValid; }
        public void CopyFrom(BoolVal other)
        {
            Value = other.Value;
            IsValid = other.IsValid;
        }
        public BoolVal Clone()
        {
            return new BoolVal(Value, IsValid);
        }
        public static List<BoolVal> CloneList(List<BoolVal> listSrc)
        {
            var list = new List<BoolVal>(listSrc.Count);
            foreach(var v in listSrc) {
                list.Add(v.Clone());
            }
            return list;
        }
    }
    public class FloatVal
    {
        public float Value = 0;
        public bool IsValid = false;

        public FloatVal() { }
        public FloatVal(float val) { Value = val; IsValid = true; }
        public FloatVal(float val, bool isValid) { Value = val; IsValid = isValid; }
        public void CopyFrom(FloatVal other)
        {
            Value = other.Value;
            IsValid = other.IsValid;
        }
        public FloatVal Clone()
        {
            return new FloatVal(Value, IsValid);
        }
        public static List<FloatVal> CloneList(List<FloatVal> listSrc)
        {
            var list = new List<FloatVal>(listSrc.Count);
            foreach (var v in listSrc) {
                list.Add(v.Clone());
            }
            return list;
        }
    }
    public class IntVal
    {
        public int Value = 0;
        public bool IsValid = false;

        public IntVal() { }
        public IntVal(int val) { Value = val; IsValid = true; }
        public IntVal(int val, bool isValid) { Value = val; IsValid = isValid; }
        public void CopyFrom(IntVal other)
        {
            Value = other.Value;
            IsValid = other.IsValid;
        }
        public IntVal Clone()
        {
            return new IntVal(Value, IsValid);
        }
        public static List<IntVal> CloneList(List<IntVal> listSrc)
        {
            var list = new List<IntVal>(listSrc.Count);
            foreach (var v in listSrc) {
                list.Add(v.Clone());
            }
            return list;
        }
    }
    public class UintVal
    {
        public uint Value = 0;
        public bool IsValid = false;

        public UintVal() { }
        public UintVal(uint val) { Value = val; IsValid = true; }
        public UintVal(uint val, bool isValid) { Value = val; IsValid = isValid; }
        public void CopyFrom(UintVal other)
        {
            Value = other.Value;
            IsValid = other.IsValid;
        }
        public UintVal Clone()
        {
            return new UintVal(Value, IsValid);
        }
        public static List<UintVal> CloneList(List<UintVal> listSrc)
        {
            var list = new List<UintVal>(listSrc.Count);
            foreach (var v in listSrc) {
                list.Add(v.Clone());
            }
            return list;
        }
    }
    public class Bool4Val
    {
        public Bool4 Value = new Bool4();
        public bool IsValid = false;

        public Bool4Val() { }
        public Bool4Val(Bool4 val) { Value = val; IsValid = true; }
        public Bool4Val(Bool4 val, bool isValid) { Value = val; IsValid = isValid; }
        public void CopyFrom(Bool4Val other)
        {
            Value = other.Value;
            IsValid = other.IsValid;
        }
        public Bool4Val Clone()
        {
            return new Bool4Val(Value, IsValid);
        }
        public static List<Bool4Val> CloneList(List<Bool4Val> listSrc)
        {
            var list = new List<Bool4Val>(listSrc.Count);
            foreach (var v in listSrc) {
                list.Add(v.Clone());
            }
            return list;
        }
    }
    public class Float4Val
    {
        public Float4 Value = new Float4();
        public bool IsValid = false;

        public Float4Val() { }
        public Float4Val(Float4 val) { Value = val; IsValid = true; }
        public Float4Val(Float4 val, bool isValid) { Value = val; IsValid = isValid; }
        public void CopyFrom(Float4Val other)
        {
            Value = other.Value;
            IsValid = other.IsValid;
        }
        public Float4Val Clone()
        {
            return new Float4Val(Value, IsValid);
        }
        public static List<Float4Val> CloneList(List<Float4Val> listSrc)
        {
            var list = new List<Float4Val>(listSrc.Count);
            foreach (var v in listSrc) {
                list.Add(v.Clone());
            }
            return list;
        }
    }
    public class Int4Val
    {
        public Int4 Value = new Int4();
        public bool IsValid = false;

        public Int4Val() { }
        public Int4Val(Int4 val) { Value = val; IsValid = true; }
        public Int4Val(Int4 val, bool isValid) { Value = val; IsValid = isValid; }
        public void CopyFrom(Int4Val other)
        {
            Value = other.Value;
            IsValid = other.IsValid;
        }
        public Int4Val Clone()
        {
            return new Int4Val(Value, IsValid);
        }
        public static List<Int4Val> CloneList(List<Int4Val> listSrc)
        {
            var list = new List<Int4Val>(listSrc.Count);
            foreach (var v in listSrc) {
                list.Add(v.Clone());
            }
            return list;
        }
    }
    public class Uint4Val
    {
        public Uint4 Value = new Uint4();
        public bool IsValid = false;

        public Uint4Val() { }
        public Uint4Val(Uint4 val) { Value = val; IsValid = true; }
        public Uint4Val(Uint4 val, bool isValid) { Value = val; IsValid = isValid; }
        public void CopyFrom(Uint4Val other)
        {
            Value = other.Value;
            IsValid = other.IsValid;
        }
        public Uint4Val Clone()
        {
            return new Uint4Val(Value, IsValid);
        }
        public static List<Uint4Val> CloneList(List<Uint4Val> listSrc)
        {
            var list = new List<Uint4Val>(listSrc.Count);
            foreach (var v in listSrc) {
                list.Add(v.Clone());
            }
            return list;
        }
    }
    public struct Bool2
    {
        public bool x = false;
        public bool y = false;

        public Bool2() { }
        public bool Assign(IList<string> vs)
        {
            bool ret = true;
            bool val = false;
            if (vs.Count > 0) {
                if (Calculator.TryParseBool(vs[0], out val)) {
                    x = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                x = val;
            }
            if (vs.Count > 1) {
                if (Calculator.TryParseBool(vs[1], out val)) {
                    y = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                y = val;
            }
            return ret;
        }
        public bool SetMember(DslExpression.CalculatorValue mv, DslExpression.CalculatorValue v)
        {
            bool ret = false;
            if (Calculator.TryGetBool(v, out var val)) {
                ret = true;
                if (Calculator.TryGetString(mv, out var m)) {
                    if (m == "x")
                        x = val;
                    else if (m == "y")
                        y = val;
                }
                else if (Calculator.TryGetInt(mv, out var ix)) {
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
        public DslExpression.CalculatorValue GetMember(DslExpression.CalculatorValue mv)
        {
            if (Calculator.TryGetString(mv, out var m)) {
                if (m == "x")
                    return x;
                else if (m == "y")
                    return y;
            }
            else if (Calculator.TryGetInt(mv, out var ix)) {
                switch (ix) {
                    case 0:
                        return x;
                    case 1:
                        return y;
                }
            }
            return DslExpression.CalculatorValue.NullObject;
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
        public static bool TryParse(DslExpression.CalculatorValue v, out Bool2 val)
        {
            bool succ = false;
            if(v.IsObject && v.ObjectVal is Bool2) {
                val = (Bool2)v.ObjectVal;
                succ = true;
            }
            else if(v.IsString || v.IsObject && !v.IsNullObject) {
                succ = TryParse(v.ToString(), out val);
            }
            else {
                val = new Bool2();
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
            float val = 0;
            if (vs.Count > 0) {
                if (float.TryParse(vs[0], out val)) {
                    x = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                x = val;
            }
            if (vs.Count > 1) {
                if (float.TryParse(vs[1], out val)) {
                    y = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                y = val;
            }
            return ret;
        }
        public bool SetMember(DslExpression.CalculatorValue mv, DslExpression.CalculatorValue v)
        {
            bool ret = false;
            if (Calculator.TryGetFloat(v, out var val)) {
                ret = true;
                if (Calculator.TryGetString(mv, out var m)) {
                    if (m == "x")
                        x = val;
                    else if (m == "y")
                        y = val;
                }
                else if (Calculator.TryGetInt(mv, out var ix)) {
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
        public DslExpression.CalculatorValue GetMember(DslExpression.CalculatorValue mv)
        {
            if (Calculator.TryGetString(mv, out var m)) {
                if (m == "x")
                    return x;
                else if (m == "y")
                    return y;
            }
            else if (Calculator.TryGetInt(mv, out var ix)) {
                switch (ix) {
                    case 0:
                        return x;
                    case 1:
                        return y;
                }
            }
            return DslExpression.CalculatorValue.NullObject;
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
        public static bool TryParse(DslExpression.CalculatorValue v, out Float2 val)
        {
            bool succ = false;
            if (v.IsObject && v.ObjectVal is Float2) {
                val = (Float2)v.ObjectVal;
                succ = true;
            }
            else if (v.IsString || v.IsObject && !v.IsNullObject) {
                succ = TryParse(v.ToString(), out val);
            }
            else {
                val = new Float2();
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
            int val = 0;
            if (vs.Count > 0) {
                if (int.TryParse(vs[0], out val)) {
                    x = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                x = val;
            }
            if (vs.Count > 1) {
                if (int.TryParse(vs[1], out val)) {
                    y = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                y = val;
            }
            return ret;
        }
        public bool SetMember(DslExpression.CalculatorValue mv, DslExpression.CalculatorValue v)
        {
            bool ret = false;
            if (Calculator.TryGetInt(v, out var val)) {
                ret = true;
                if (Calculator.TryGetString(mv, out var m)) {
                    if (m == "x")
                        x = val;
                    else if (m == "y")
                        y = val;
                }
                else if (Calculator.TryGetInt(mv, out var ix)) {
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
        public DslExpression.CalculatorValue GetMember(DslExpression.CalculatorValue mv)
        {
            if (Calculator.TryGetString(mv, out var m)) {
                if (m == "x")
                    return x;
                else if (m == "y")
                    return y;
            }
            else if (Calculator.TryGetInt(mv, out var ix)) {
                switch (ix) {
                    case 0:
                        return x;
                    case 1:
                        return y;
                }
            }
            return DslExpression.CalculatorValue.NullObject;
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
        public static bool TryParse(DslExpression.CalculatorValue v, out Int2 val)
        {
            bool succ = false;
            if (v.IsObject && v.ObjectVal is Int2) {
                val = (Int2)v.ObjectVal;
                succ = true;
            }
            else if (v.IsString || v.IsObject && !v.IsNullObject) {
                succ = TryParse(v.ToString(), out val);
            }
            else {
                val = new Int2();
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
            uint val = 0;
            if (vs.Count > 0) {
                if (uint.TryParse(vs[0], out val)) {
                    x = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                x = val;
            }
            if (vs.Count > 1) {
                if (uint.TryParse(vs[1], out val)) {
                    y = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                y = val;
            }
            return ret;
        }
        public bool SetMember(DslExpression.CalculatorValue mv, DslExpression.CalculatorValue v)
        {
            bool ret = false;
            if (Calculator.TryGetUInt(v, out var val)) {
                ret = true;
                if (Calculator.TryGetString(mv, out var m)) {
                    if (m == "x")
                        x = val;
                    else if (m == "y")
                        y = val;
                }
                else if (Calculator.TryGetInt(mv, out var ix)) {
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
        public DslExpression.CalculatorValue GetMember(DslExpression.CalculatorValue mv)
        {
            if (Calculator.TryGetString(mv, out var m)) {
                if (m == "x")
                    return x;
                else if (m == "y")
                    return y;
            }
            else if (Calculator.TryGetInt(mv, out var ix)) {
                switch (ix) {
                    case 0:
                        return x;
                    case 1:
                        return y;
                }
            }
            return DslExpression.CalculatorValue.NullObject;
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
        public static bool TryParse(DslExpression.CalculatorValue v, out Uint2 val)
        {
            bool succ = false;
            if (v.IsObject && v.ObjectVal is Uint2) {
                val = (Uint2)v.ObjectVal;
                succ = true;
            }
            else if (v.IsString || v.IsObject && !v.IsNullObject) {
                succ = TryParse(v.ToString(), out val);
            }
            else {
                val = new Uint2();
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
            bool val = false;
            if (vs.Count > 0) {
                if (Calculator.TryParseBool(vs[0], out val)) {
                    x = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                x = val;
            }
            if (vs.Count > 1) {
                if (Calculator.TryParseBool(vs[1], out val)) {
                    y = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                y = val;
            }
            if (vs.Count > 2) {
                if (Calculator.TryParseBool(vs[2], out val)) {
                    z = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                z = val;
            }
            return ret;
        }
        public bool SetMember(DslExpression.CalculatorValue mv, DslExpression.CalculatorValue v)
        {
            bool ret = false;
            if (Calculator.TryGetBool(v, out var val)) {
                ret = true;
                if (Calculator.TryGetString(mv, out var m)) {
                    if (m == "x")
                        x = val;
                    else if (m == "y")
                        y = val;
                    else if (m == "z")
                        z = val;
                }
                else if (Calculator.TryGetInt(mv, out var ix)) {
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
        public DslExpression.CalculatorValue GetMember(DslExpression.CalculatorValue mv)
        {
            if (Calculator.TryGetString(mv, out var m)) {
                if (m == "x")
                    return x;
                else if (m == "y")
                    return y;
                else if (m == "z")
                    return z;
            }
            else if (Calculator.TryGetInt(mv, out var ix)) {
                switch (ix) {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    case 2:
                        return z;
                }
            }
            return DslExpression.CalculatorValue.NullObject;
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
        public static bool TryParse(DslExpression.CalculatorValue v, out Bool3 val)
        {
            bool succ = false;
            if (v.IsObject && v.ObjectVal is Bool3) {
                val = (Bool3)v.ObjectVal;
                succ = true;
            }
            else if (v.IsString || v.IsObject && !v.IsNullObject) {
                succ = TryParse(v.ToString(), out val);
            }
            else {
                val = new Bool3();
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
            float val = 0;
            if (vs.Count > 0) {
                if (float.TryParse(vs[0], out val)) {
                    x = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                x = val;
            }
            if (vs.Count > 1) {
                if (float.TryParse(vs[1], out val)) {
                    y = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                y = val;
            }
            if (vs.Count > 2) {
                if (float.TryParse(vs[2], out val)) {
                    z = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                z = val;
            }
            return ret;
        }
        public bool SetMember(DslExpression.CalculatorValue mv, DslExpression.CalculatorValue v)
        {
            bool ret = false;
            if (Calculator.TryGetFloat(v, out var val)) {
                ret = true;
                if (Calculator.TryGetString(mv, out var m)) {
                    if (m == "x")
                        x = val;
                    else if (m == "y")
                        y = val;
                    else if (m == "z")
                        z = val;
                }
                else if (Calculator.TryGetInt(mv, out var ix)) {
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
        public DslExpression.CalculatorValue GetMember(DslExpression.CalculatorValue mv)
        {
            if (Calculator.TryGetString(mv, out var m)) {
                if (m == "x")
                    return x;
                else if (m == "y")
                    return y;
                else if (m == "z")
                    return z;
            }
            else if (Calculator.TryGetInt(mv, out var ix)) {
                switch (ix) {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    case 2:
                        return z;
                }
            }
            return DslExpression.CalculatorValue.NullObject;
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
        public static bool TryParse(DslExpression.CalculatorValue v, out Float3 val)
        {
            bool succ = false;
            if (v.IsObject && v.ObjectVal is Float3) {
                val = (Float3)v.ObjectVal;
                succ = true;
            }
            else if (v.IsString || v.IsObject && !v.IsNullObject) {
                succ = TryParse(v.ToString(), out val);
            }
            else {
                val = new Float3();
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
            int val = 0;
            if (vs.Count > 0) {
                if (int.TryParse(vs[0], out val)) {
                    x = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                x = val;
            }
            if (vs.Count > 1) {
                if (int.TryParse(vs[1], out val)) {
                    y = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                y = val;
            }
            if (vs.Count > 2) {
                if (int.TryParse(vs[2], out val)) {
                    z = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                z = val;
            }
            return ret;
        }
        public bool SetMember(DslExpression.CalculatorValue mv, DslExpression.CalculatorValue v)
        {
            bool ret = false;
            if (Calculator.TryGetInt(v, out var val)) {
                ret = true;
                if (Calculator.TryGetString(mv, out var m)) {
                    if (m == "x")
                        x = val;
                    else if (m == "y")
                        y = val;
                    else if (m == "z")
                        z = val;
                }
                else if (Calculator.TryGetInt(mv, out var ix)) {
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
        public DslExpression.CalculatorValue GetMember(DslExpression.CalculatorValue mv)
        {
            if (Calculator.TryGetString(mv, out var m)) {
                if (m == "x")
                    return x;
                else if (m == "y")
                    return y;
                else if (m == "z")
                    return z;
            }
            else if (Calculator.TryGetInt(mv, out var ix)) {
                switch (ix) {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    case 2:
                        return z;
                }
            }
            return DslExpression.CalculatorValue.NullObject;
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
        public static bool TryParse(DslExpression.CalculatorValue v, out Int3 val)
        {
            bool succ = false;
            if (v.IsObject && v.ObjectVal is Int3) {
                val = (Int3)v.ObjectVal;
                succ = true;
            }
            else if (v.IsString || v.IsObject && !v.IsNullObject) {
                succ = TryParse(v.ToString(), out val);
            }
            else {
                val = new Int3();
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
            uint val = 0;
            if (vs.Count > 0) {
                if (uint.TryParse(vs[0], out val)) {
                    x = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                x = val;
            }
            if (vs.Count > 1) {
                if (uint.TryParse(vs[1], out val)) {
                    y = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                y = val;
            }
            if (vs.Count > 2) {
                if (uint.TryParse(vs[2], out val)) {
                    z = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                z = val;
            }
            return ret;
        }
        public bool SetMember(DslExpression.CalculatorValue mv, DslExpression.CalculatorValue v)
        {
            bool ret = false;
            if (Calculator.TryGetUInt(v, out var val)) {
                ret = true;
                if (Calculator.TryGetString(mv, out var m)) {
                    if (m == "x")
                        x = val;
                    else if (m == "y")
                        y = val;
                    else if (m == "z")
                        z = val;
                }
                else if (Calculator.TryGetInt(mv, out var ix)) {
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
        public DslExpression.CalculatorValue GetMember(DslExpression.CalculatorValue mv)
        {
            if (Calculator.TryGetString(mv, out var m)) {
                if (m == "x")
                    return x;
                else if (m == "y")
                    return y;
                else if (m == "z")
                    return z;
            }
            else if (Calculator.TryGetInt(mv, out var ix)) {
                switch (ix) {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    case 2:
                        return z;
                }
            }
            return DslExpression.CalculatorValue.NullObject;
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
        public static bool TryParse(DslExpression.CalculatorValue v, out Uint3 val)
        {
            bool succ = false;
            if (v.IsObject && v.ObjectVal is Uint3) {
                val = (Uint3)v.ObjectVal;
                succ = true;
            }
            else if (v.IsString || v.IsObject && !v.IsNullObject) {
                succ = TryParse(v.ToString(), out val);
            }
            else {
                val = new Uint3();
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
            bool val = false;
            if (vs.Count > 0) {
                if (Calculator.TryParseBool(vs[0], out val)) {
                    x = val;
                }
                else {
                    ret = false;
                }
            }
            if (vs.Count > 1) {
                if (Calculator.TryParseBool(vs[1], out val)) {
                    y = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                y = val;
            }
            if (vs.Count > 2) {
                if (Calculator.TryParseBool(vs[2], out val)) {
                    z = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                z = val;
            }
            if (vs.Count > 3) {
                if (Calculator.TryParseBool(vs[3], out val)) {
                    w = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                w = val;
            }
            return ret;
        }
        public bool SetMember(DslExpression.CalculatorValue mv, DslExpression.CalculatorValue v)
        {
            bool ret = false;
            if (Calculator.TryGetBool(v, out var val)) {
                ret = true;
                if (Calculator.TryGetString(mv, out var m)) {
                    if (m == "x")
                        x = val;
                    else if (m == "y")
                        y = val;
                    else if (m == "z")
                        z = val;
                    else if (m == "w")
                        w = val;
                }
                else if (Calculator.TryGetInt(mv, out var ix)) {
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
        public DslExpression.CalculatorValue GetMember(DslExpression.CalculatorValue mv)
        {
            if (Calculator.TryGetString(mv, out var m)) {
                if (m == "x")
                    return x;
                else if (m == "y")
                    return y;
                else if (m == "z")
                    return z;
                else if (m == "w")
                    return w;
            }
            else if (Calculator.TryGetInt(mv, out var ix)) {
                switch (ix) {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    case 2:
                        return z;
                    case 3:
                        return w;
                }
            }
            return DslExpression.CalculatorValue.NullObject;
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
        public static bool TryParse(DslExpression.CalculatorValue v, out Bool4 val)
        {
            bool succ = false;
            if (v.IsObject && v.ObjectVal is Bool4) {
                val = (Bool4)v.ObjectVal;
                succ = true;
            }
            else if (v.IsString || v.IsObject && !v.IsNullObject) {
                succ = TryParse(v.ToString(), out val);
            }
            else {
                val = new Bool4();
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
            float val = 0;
            if (vs.Count > 0) {
                if (float.TryParse(vs[0], out val)) {
                    x = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                x = val;
            }
            if (vs.Count > 1) {
                if (float.TryParse(vs[1], out val)) {
                    y = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                y = val;
            }
            if (vs.Count > 2) {
                if (float.TryParse(vs[2], out val)) {
                    z = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                z = val;
            }
            if (vs.Count > 3) {
                if (float.TryParse(vs[3], out val)) {
                    w = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                w = val;
            }
            return ret;
        }
        public bool SetMember(DslExpression.CalculatorValue mv, DslExpression.CalculatorValue v)
        {
            bool ret = false;
            if (Calculator.TryGetFloat(v, out var val)) {
                ret = true;
                if (Calculator.TryGetString(mv, out var m)) {
                    if (m == "x")
                        x = val;
                    else if (m == "y")
                        y = val;
                    else if (m == "z")
                        z = val;
                    else if (m == "w")
                        w = val;
                }
                else if (Calculator.TryGetInt(mv, out var ix)) {
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
        public DslExpression.CalculatorValue GetMember(DslExpression.CalculatorValue mv)
        {
            if (Calculator.TryGetString(mv, out var m)) {
                if (m == "x")
                    return x;
                else if (m == "y")
                    return y;
                else if (m == "z")
                    return z;
                else if (m == "w")
                    return w;
            }
            else if (Calculator.TryGetInt(mv, out var ix)) {
                switch (ix) {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    case 2:
                        return z;
                    case 3:
                        return w;
                }
            }
            return DslExpression.CalculatorValue.NullObject;
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
        public static bool TryParse(DslExpression.CalculatorValue v, out Float4 val)
        {
            bool succ = false;
            if (v.IsObject && v.ObjectVal is Float4) {
                val = (Float4)v.ObjectVal;
                succ = true;
            }
            else if (v.IsString || v.IsObject && !v.IsNullObject) {
                succ = TryParse(v.ToString(), out val);
            }
            else {
                val = new Float4();
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
            int val = 0;
            if (vs.Count > 0) {
                if (int.TryParse(vs[0], out val)) {
                    x = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                x = val;
            }
            if (vs.Count > 1) {
                if (int.TryParse(vs[1], out val)) {
                    y = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                y = val;
            }
            if (vs.Count > 2) {
                if (int.TryParse(vs[2], out val)) {
                    z = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                z = val;
            }
            if (vs.Count > 3) {
                if (int.TryParse(vs[3], out val)) {
                    w = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                w = val;
            }
            return ret;
        }
        public bool SetMember(DslExpression.CalculatorValue mv, DslExpression.CalculatorValue v)
        {
            bool ret = false;
            if (Calculator.TryGetInt(v, out var val)) {
                ret = true;
                if (Calculator.TryGetString(mv, out var m)) {
                    if (m == "x")
                        x = val;
                    else if (m == "y")
                        y = val;
                    else if (m == "z")
                        z = val;
                    else if (m == "w")
                        w = val;
                }
                else if (Calculator.TryGetInt(mv, out var ix)) {
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
        public DslExpression.CalculatorValue GetMember(DslExpression.CalculatorValue mv)
        {
            if (Calculator.TryGetString(mv, out var m)) {
                if (m == "x")
                    return x;
                else if (m == "y")
                    return y;
                else if (m == "z")
                    return z;
                else if (m == "w")
                    return w;
            }
            else if (Calculator.TryGetInt(mv, out var ix)) {
                switch (ix) {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    case 2:
                        return z;
                    case 3:
                        return w;
                }
            }
            return DslExpression.CalculatorValue.NullObject;
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
        public static bool TryParse(DslExpression.CalculatorValue v, out Int4 val)
        {
            bool succ = false;
            if (v.IsObject && v.ObjectVal is Int4) {
                val = (Int4)v.ObjectVal;
                succ = true;
            }
            else if (v.IsString || v.IsObject && !v.IsNullObject) {
                succ = TryParse(v.ToString(), out val);
            }
            else {
                val = new Int4();
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
            uint val = 0;
            if (vs.Count > 0) {
                if (uint.TryParse(vs[0], out val)) {
                    x = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                x = val;
            }
            if (vs.Count > 1) {
                if (uint.TryParse(vs[1], out val)) {
                    y = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                y = val;
            }
            if (vs.Count > 2) {
                if (uint.TryParse(vs[2], out val)) {
                    z = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                z = val;
            }
            if (vs.Count > 3) {
                if (uint.TryParse(vs[3], out val)) {
                    w = val;
                }
                else {
                    ret = false;
                }
            }
            else {
                w = val;
            }
            return ret;
        }
        public bool SetMember(DslExpression.CalculatorValue mv, DslExpression.CalculatorValue v)
        {
            bool ret = false;
            if (Calculator.TryGetUInt(v, out var val)) {
                ret = true;
                if (Calculator.TryGetString(mv, out var m)) {
                    if (m == "x")
                        x = val;
                    else if (m == "y")
                        y = val;
                    else if (m == "z")
                        z = val;
                    else if (m == "w")
                        w = val;
                }
                else if (Calculator.TryGetInt(mv, out var ix)) {
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
        public DslExpression.CalculatorValue GetMember(DslExpression.CalculatorValue mv)
        {
            if (Calculator.TryGetString(mv, out var m)) {
                if (m == "x")
                    return x;
                else if (m == "y")
                    return y;
                else if (m == "z")
                    return z;
                else if (m == "w")
                    return w;
            }
            else if (Calculator.TryGetInt(mv, out var ix)) {
                switch (ix) {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    case 2:
                        return z;
                    case 3:
                        return w;
                }
            }
            return DslExpression.CalculatorValue.NullObject;
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
        public static bool TryParse(DslExpression.CalculatorValue v, out Uint4 val)
        {
            bool succ = false;
            if (v.IsObject && v.ObjectVal is Uint4) {
                val = (Uint4)v.ObjectVal;
                succ = true;
            }
            else if (v.IsString || v.IsObject && !v.IsNullObject) {
                succ = TryParse(v.ToString(), out val);
            }
            else {
                val = new Uint4();
            }
            return succ;
        }
    }
    public static class VariableTable
    {
        public static void InvalidateAllValues()
        {
            foreach(var val in s_BoolVars.Values) {
                val.IsValid = false;
            }
            foreach (var val in s_FloatVars.Values) {
                val.IsValid = false;
            }
            foreach (var val in s_IntVars.Values) {
                val.IsValid = false;
            }
            foreach (var val in s_UintVars.Values) {
                val.IsValid = false;
            }
            foreach (var val in s_Bool4Vars.Values) {
                val.IsValid = false;
            }
            foreach (var val in s_Float4Vars.Values) {
                val.IsValid = false;
            }
            foreach (var val in s_Int4Vars.Values) {
                val.IsValid = false;
            }
            foreach (var val in s_Uint4Vars.Values) {
                val.IsValid = false;
            }
            foreach (var list in s_BoolArrayVars.Values) {
                foreach (var v in list) {
                    v.IsValid = false;
                }
            }
            foreach (var list in s_FloatArrayVars.Values) {
                foreach (var v in list) {
                    v.IsValid = false;
                }
            }
            foreach (var list in s_IntArrayVars.Values) {
                foreach (var v in list) {
                    v.IsValid = false;
                }
            }
            foreach (var list in s_UintArrayVars.Values) {
                foreach (var v in list) {
                    v.IsValid = false;
                }
            }
            foreach (var list in s_Bool4ArrayVars.Values) {
                foreach (var v in list) {
                    v.IsValid = false;
                }
            }
            foreach (var list in s_Float4ArrayVars.Values) {
                foreach (var v in list) {
                    v.IsValid = false;
                }
            }
            foreach (var list in s_Int4ArrayVars.Values) {
                foreach (var v in list) {
                    v.IsValid = false;
                }
            }
            foreach (var list in s_Uint4ArrayVars.Values) {
                foreach(var v in list) {
                    v.IsValid = false;
                }
            }
        }
        public static void AllocVar(string name, string type)
        {
            if(TryGetVarType(name, out var ty, out var isArray)) {
                return;
            }
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
            else if (suffix.Length > 0) {
                if (baseType == "vec") {
                    s_Float4Vars[name] = new Float4Val();
                }
                else if (baseType == "ivec") {
                    s_Int4Vars[name] = new Int4Val();
                }
                else if (baseType == "uvec") {
                    s_Uint4Vars[name] = new Uint4Val();
                }
                else if (baseType == "bvec") {
                    s_Bool4Vars[name] = new Bool4Val();
                }
            }
            else {
                if (baseType == "float") {
                    s_FloatVars[name] = new FloatVal();
                }
                else if (baseType == "int") {
                    s_IntVars[name] = new IntVal();
                }
                else if (baseType == "uint") {
                    s_UintVars[name] = new UintVal();
                }
                else if (baseType == "bool") {
                    s_BoolVars[name] = new BoolVal();
                }
            }
        }
        public static bool TryGetVarType(string name, out string type, out bool isArray)
        {
            bool exists = false;
            type = string.Empty;
            isArray = false;
            if (s_Float4ArrayVars.ContainsKey(name)) {
                isArray = true;
                exists = true;
                type = "vec4";
            }
            else if (s_Int4ArrayVars.ContainsKey(name)) {
                isArray = true;
                exists = true;
                type = "ivec4";
            }
            else if (s_Uint4ArrayVars.ContainsKey(name)) {
                isArray = true;
                exists = true;
                type = "uvec4";
            }
            else if (s_Bool4ArrayVars.ContainsKey(name)) {
                isArray = true;
                exists = true;
                type = "bvec4";
            }
            else if (s_FloatArrayVars.ContainsKey(name)) {
                isArray = true;
                exists = true;
                type = "float";
            }
            else if (s_IntArrayVars.ContainsKey(name)) {
                isArray = true;
                exists = true;
                type = "int";
            }
            else if (s_UintArrayVars.ContainsKey(name)) {
                isArray = true;
                exists = true;
                type = "uint";
            }
            else if (s_BoolArrayVars.ContainsKey(name)) {
                isArray = true;
                exists = true;
                type = "bool";
            }
            else if (s_Float4Vars.ContainsKey(name)) {
                exists = true;
                type = "vec4";
            }
            else if (s_Int4Vars.ContainsKey(name)) {
                exists = true;
                type = "ivec4";
            }
            else if (s_Uint4Vars.ContainsKey(name)) {
                exists = true;
                type = "uvec4";
            }
            else if (s_Bool4Vars.ContainsKey(name)) {
                exists = true;
                type = "bvec4";
            }
            else if (s_FloatVars.ContainsKey(name)) {
                exists = true;
                type = "float";
            }
            else if (s_IntVars.ContainsKey(name)) {
                exists = true;
                type = "int";
            }
            else if (s_UintVars.ContainsKey(name)) {
                exists = true;
                type = "uint";
            }
            else if (s_BoolVars.ContainsKey(name)) {
                exists = true;
                type = "bool";
            }
            return exists;
        }
        public static bool GetVarValue(string name, string type, out DslExpression.CalculatorValue varVal)
        {
            bool exists = false;
            varVal = string.Empty;
            string baseType = Program.GetTypeRemoveSuffix(type, out var suffix, out var arrNums);
            if (arrNums.Count > 0) {
                Debug.Assert(arrNums.Count == 1);
                int arrNum = arrNums[0];
                if (suffix.Length > 0) {
                    if (baseType == "vec") {
                        exists = s_Float4ArrayVars.TryGetValue(name, out var val);
                        if (exists) {
                            Debug.Assert(null != val);
                            varVal = DslExpression.CalculatorValue.FromObject(val);
                        }
                    }
                    else if (baseType == "ivec") {
                        exists = s_Int4ArrayVars.TryGetValue(name, out var val);
                        if (exists) {
                            Debug.Assert(null != val);
                            varVal = DslExpression.CalculatorValue.FromObject(val);
                        }
                    }
                    else if (baseType == "uvec") {
                        exists = s_Uint4ArrayVars.TryGetValue(name, out var val);
                        if (exists) {
                            Debug.Assert(null != val);
                            varVal = DslExpression.CalculatorValue.FromObject(val);
                        }
                    }
                    else if (baseType == "bvec") {
                        exists = s_Bool4ArrayVars.TryGetValue(name, out var val);
                        if (exists) {
                            Debug.Assert(null != val);
                            varVal = DslExpression.CalculatorValue.FromObject(val);
                        }
                    }
                }
                else {
                    if (baseType == "float") {
                        exists = s_FloatArrayVars.TryGetValue(name, out var val);
                        if (exists) {
                            Debug.Assert(null != val);
                            varVal = DslExpression.CalculatorValue.FromObject(val);
                        }
                    }
                    else if (baseType == "int") {
                        exists = s_IntArrayVars.TryGetValue(name, out var val);
                        if (exists) {
                            Debug.Assert(null != val);
                            varVal = DslExpression.CalculatorValue.FromObject(val);
                        }
                    }
                    else if (baseType == "uint") {
                        exists = s_UintArrayVars.TryGetValue(name, out var val);
                        if (exists) {
                            Debug.Assert(null != val);
                            varVal = DslExpression.CalculatorValue.FromObject(val);
                        }
                    }
                    else if (baseType == "bool") {
                        exists = s_BoolArrayVars.TryGetValue(name, out var val);
                        if (exists) {
                            Debug.Assert(null != val);
                            varVal = DslExpression.CalculatorValue.FromObject(val);
                        }
                    }
                }
            }
            else if (suffix.Length > 0) {
                if (baseType == "vec") {
                    if (s_Float4Vars.TryGetValue(name, out var val)) {
                        exists = val.IsValid;
                        varVal = DslExpression.CalculatorValue.FromObject(val.Value);
                    }
                }
                else if (baseType == "ivec") {
                    if (s_Int4Vars.TryGetValue(name, out var val)) {
                        exists = val.IsValid;
                        varVal = DslExpression.CalculatorValue.FromObject(val.Value);
                    }
                }
                else if (baseType == "uvec") {
                    if (s_Uint4Vars.TryGetValue(name, out var val)) {
                        exists = val.IsValid;
                        varVal = DslExpression.CalculatorValue.FromObject(val.Value);
                    }
                }
                else if (baseType == "bvec") {
                    if (s_Bool4Vars.TryGetValue(name, out var val)) {
                        exists = val.IsValid;
                        varVal = DslExpression.CalculatorValue.FromObject(val.Value);
                    }
                }
            }
            else {
                if (baseType == "float") {
                    if (s_FloatVars.TryGetValue(name, out var val)) {
                        exists = val.IsValid;
                        varVal = DslExpression.CalculatorValue.From(val.Value);
                    }
                }
                else if (baseType == "int") {
                    if (s_IntVars.TryGetValue(name, out var val)) {
                        exists = val.IsValid;
                        varVal = DslExpression.CalculatorValue.From(val.Value);
                    }
                }
                else if (baseType == "uint") {
                    if (s_UintVars.TryGetValue(name, out var val)) {
                        exists = val.IsValid;
                        varVal = DslExpression.CalculatorValue.From(val.Value);
                    }
                }
                else if (baseType == "bool") {
                    if (s_BoolVars.TryGetValue(name, out var val)) {
                        exists = val.IsValid;
                        varVal = DslExpression.CalculatorValue.From(val.Value);
                    }
                }
            }
            if(!exists && Config.ActiveConfig.SettingInfo.VariableAssignments.TryGetValue(name, out var vinfo)) {
                Debug.Assert(type == vinfo.Type || type == "vec4" && vinfo.Type == "Float4" || type == "uvec4" && vinfo.Type == "Uint4" || type == "uint" && vinfo.Type == "int" || type == "int" && vinfo.Type == "uint");
                varVal = vinfo.Value;
                AssignValue(name, type, varVal, Config.s_ArgTypeConversion, 0);
                exists = true;
            }
            return exists;
        }
        public static bool ObjectGetValue(ComputeGraphVarNode left, DslExpression.CalculatorValue m, out DslExpression.CalculatorValue varVal)
        {
            string name = left.VarName;
            string type = left.Type;
            return ObjectGetValue(name, type, m, out varVal);
        }
        public static bool ObjectGetValue(string name, string type, DslExpression.CalculatorValue m, out DslExpression.CalculatorValue varVal)
        {
            bool exists = false;
            varVal = string.Empty;
            string baseType = Program.GetTypeRemoveSuffix(type, out var suffix, out var arrNums);
            if (arrNums.Count == 0 && suffix.Length > 0) {
                if (baseType == "vec") {
                    if (s_Float4Vars.TryGetValue(name, out var val)) {
                        exists = val.IsValid;
                        varVal = val.Value.GetMember(m);
                    }
                }
                else if (baseType == "ivec") {
                    if (s_Int4Vars.TryGetValue(name, out var val)) {
                        exists = val.IsValid;
                        varVal = val.Value.GetMember(m);
                    }
                }
                else if (baseType == "uvec") {
                    if (s_Uint4Vars.TryGetValue(name, out var val)) {
                        exists = val.IsValid;
                        varVal = val.Value.GetMember(m);
                    }
                }
                else if (baseType == "bvec") {
                    if (s_Bool4Vars.TryGetValue(name, out var val)) {
                        exists = val.IsValid;
                        varVal = val.Value.GetMember(m);
                    }
                }
            }
            if (!exists && Config.ActiveConfig.SettingInfo.ObjectMemberAssignments.TryGetValue(name, out var objInfo) &&
                objInfo.TryGetValue(m, out var vinfo)) {
                Debug.Assert(type == vinfo.Type);
                varVal = vinfo.Value;
                ObjectAssignValue(name, type, m, varVal);
                exists = true;
            }
            return exists;
        }
        public static bool ArrayGetValue(ComputeGraphVarNode left, DslExpression.CalculatorValue ix, out DslExpression.CalculatorValue varVal)
        {
            string name = left.VarName;
            string type = left.Type;
            return ArrayGetValue(name, type, ix, out varVal);
        }
        public static bool ArrayGetValue(string name, string type, DslExpression.CalculatorValue ix, out DslExpression.CalculatorValue varVal)
        {
            bool exists = false;
            varVal = string.Empty;
            if (!Calculator.TryGetInt(ix, out var index) || index < 0) {
                return exists;
            }
            string baseType = Program.GetTypeRemoveSuffix(type, out var suffix, out var arrNums);
            if (arrNums.Count > 0) {
                Debug.Assert(arrNums.Count == 1);
                int arrNum = arrNums[0];
                if (suffix.Length > 0) {
                    if (baseType == "vec") {
                        if (s_Float4ArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            exists = vals[index].IsValid;
                            varVal = DslExpression.CalculatorValue.FromObject(vals[index].Value);
                        }
                    }
                    else if (baseType == "ivec") {
                        if (s_Int4ArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            exists = vals[index].IsValid;
                            varVal = DslExpression.CalculatorValue.FromObject(vals[index].Value);
                        }
                    }
                    else if (baseType == "uvec") {
                        if (s_Uint4ArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            exists = vals[index].IsValid;
                            varVal = DslExpression.CalculatorValue.FromObject(vals[index].Value);
                        }
                    }
                    else if (baseType == "bvec") {
                        if (s_Bool4ArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            exists = vals[index].IsValid;
                            varVal = DslExpression.CalculatorValue.FromObject(vals[index].Value);
                        }
                    }
                }
                else {
                    if (baseType == "float") {
                        if (s_FloatArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            exists = vals[index].IsValid;
                            varVal = DslExpression.CalculatorValue.From(vals[index].Value);
                        }
                    }
                    else if (baseType == "int") {
                        if (s_IntArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            exists = vals[index].IsValid;
                            varVal = DslExpression.CalculatorValue.From(vals[index].Value);
                        }
                    }
                    else if (baseType == "uint") {
                        if (s_UintArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            exists = vals[index].IsValid;
                            varVal = DslExpression.CalculatorValue.From(vals[index].Value);
                        }
                    }
                    else if (baseType == "bool") {
                        if (s_BoolArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            exists = vals[index].IsValid;
                            varVal = DslExpression.CalculatorValue.From(vals[index].Value);
                        }
                    }
                }
            }
            else if (suffix.Length > 0) {
                if (baseType == "vec") {
                    if (s_Float4Vars.TryGetValue(name, out var val)) {
                        exists = val.IsValid;
                        varVal = val.Value.GetMember(ix);
                    }
                }
                else if (baseType == "ivec") {
                    if (s_Int4Vars.TryGetValue(name, out var val)) {
                        exists = val.IsValid;
                        varVal = val.Value.GetMember(ix);
                    }
                }
                else if (baseType == "uvec") {
                    if (s_Uint4Vars.TryGetValue(name, out var val)) {
                        exists = val.IsValid;
                        varVal = val.Value.GetMember(ix);
                    }
                }
                else if (baseType == "bvec") {
                    if (s_Bool4Vars.TryGetValue(name, out var val)) {
                        exists = val.IsValid;
                        varVal = val.Value.GetMember(ix);
                    }
                }
            }
            if (!exists && Config.ActiveConfig.SettingInfo.ArrayElementAssignments.TryGetValue(name, out var arrInfo) &&
                arrInfo.TryGetValue(index, out var vinfo)) {
                Debug.Assert(type == vinfo.Type);
                varVal = vinfo.Value;
                ArrayAssignValue(name, type, ix, varVal);
                exists = true;
            }
            return exists;
        }
        public static bool ObjectArrayGetValue(ComputeGraphVarNode left, DslExpression.CalculatorValue ix, DslExpression.CalculatorValue m, out DslExpression.CalculatorValue varVal)
        {
            string name = left.VarName;
            string type = left.Type;
            return ObjectArrayGetValue(name, type, ix, m, out varVal);
        }
        public static bool ObjectArrayGetValue(string name, string type, DslExpression.CalculatorValue ix, DslExpression.CalculatorValue m, out DslExpression.CalculatorValue varVal)
        {
            bool exists = false;
            varVal = string.Empty;
            if (!Calculator.TryGetInt(ix, out var index) || index < 0) {
                return exists;
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
                ObjectArrayAssignValue(name, type, ix, m, varVal);
                exists = true;
            }
            return exists;
        }
        public static void AssignValue(ComputeGraphVarNode left, DslExpression.CalculatorValue right, Dictionary<int, int> argTypeConversion, int argIx)
        {
            string name = left.VarName;
            string type = left.Type;
            AssignValue(name, type, right, argTypeConversion, argIx);
        }
        public static void AssignValue(string name, string type, DslExpression.CalculatorValue right, Dictionary<int, int> argTypeConversion, int argIx)
        {
            if (Config.ActiveConfig.SettingInfo.UnassignableVariables.Contains(name))
                return;
            string baseType = Program.GetTypeRemoveSuffix(type, out var suffix, out var arrNums);
            if (arrNums.Count > 0) {
                Debug.Assert(arrNums.Count == 1);
                int arrNum = arrNums[0];
                if (suffix.Length > 0) {
                    if (baseType == "vec") {
                        if(s_Float4ArrayVars.TryGetValue(name, out var val)) {
                            foreach(var v in val) {
                                v.IsValid = false;
                            }
                        }
                    }
                    else if (baseType == "ivec") {
                        if (s_Int4ArrayVars.TryGetValue(name, out var val)) {
                            foreach (var v in val) {
                                v.IsValid = false;
                            }
                        }
                    }
                    else if (baseType == "uvec") {
                        if (s_Uint4ArrayVars.TryGetValue(name, out var val)) {
                            foreach (var v in val) {
                                v.IsValid = false;
                            }
                        }
                    }
                    else if (baseType == "bvec") {
                        if (s_Bool4ArrayVars.TryGetValue(name, out var val)) {
                            foreach (var v in val) {
                                v.IsValid = false;
                            }
                        }
                    }
                }
                else {
                    if (baseType == "float") {
                        if (s_FloatArrayVars.TryGetValue(name, out var val)) {
                            foreach (var v in val) {
                                v.IsValid = false;
                            }
                        }
                    }
                    else if (baseType == "int") {
                        if (s_IntArrayVars.TryGetValue(name, out var val)) {
                            foreach (var v in val) {
                                v.IsValid = false;
                            }
                        }
                    }
                    else if (baseType == "uint") {
                        if (s_UintArrayVars.TryGetValue(name, out var val)) {
                            foreach (var v in val) {
                                v.IsValid = false;
                            }
                        }
                    }
                    else if (baseType == "bool") {
                        if (s_BoolArrayVars.TryGetValue(name, out var val)) {
                            foreach (var v in val) {
                                v.IsValid = false;
                            }
                        }
                    }
                }
            }
            else{
                if (suffix.Length > 0) {
                    if (baseType == "vec") {
                        if (s_Float4Vars.TryGetValue(name, out var val)) {
                            val.IsValid = false;
                        }
                    }
                    else if (baseType == "ivec") {
                        if (s_Int4Vars.TryGetValue(name, out var val)) {
                            val.IsValid = false;
                        }
                    }
                    else if (baseType == "uvec") {
                        if (s_Uint4Vars.TryGetValue(name, out var val)) {
                            val.IsValid = false;
                        }
                    }
                    else if (baseType == "bvec") {
                        if (s_Bool4Vars.TryGetValue(name, out var val)) {
                            val.IsValid = false;
                        }
                    }
                }
                else {
                    if (baseType == "float" && right.IsSignedInteger) {
                        float val = Calculator.itof(right.GetInt());
                        argTypeConversion[argIx] = ComputeGraphCalcNode.c_action_add_itof;
                        if (s_FloatVars.TryGetValue(name, out var v)) {
                            v.Value = val;
                            v.IsValid = true;
                        }
                        else {
                            s_FloatVars.Add(name, new FloatVal(val));
                        }
                    }
                    else if (baseType == "float" && right.IsUnsignedInteger) {
                        float val = Calculator.utof(right.GetUInt());
                        argTypeConversion[argIx] = ComputeGraphCalcNode.c_action_add_utof;
                        if (s_FloatVars.TryGetValue(name, out var v)) {
                            v.Value = val;
                            v.IsValid = true;
                        }
                        else {
                            s_FloatVars.Add(name, new FloatVal(val));
                        }
                    }
                    else if (baseType == "int" && right.IsNumber) {
                        int val = Calculator.ftoi(right.GetFloat());
                        argTypeConversion[argIx] = ComputeGraphCalcNode.c_action_add_ftoi;
                        if (s_IntVars.TryGetValue(name, out var v)) {
                            v.Value = val;
                            v.IsValid = true;
                        }
                        else {
                            s_IntVars.Add(name, new IntVal(val));
                        }
                    }
                    else if (baseType == "uint" && right.IsNumber) {
                        uint val = Calculator.ftou(right.GetFloat());
                        argTypeConversion[argIx] = ComputeGraphCalcNode.c_action_add_ftou;
                        if (s_UintVars.TryGetValue(name, out var v)) {
                            v.Value = val;
                            v.IsValid = true;
                        }
                        else {
                            s_UintVars.Add(name, new UintVal(val));
                        }
                    }
                    else if (baseType == "float") {
                        if (Calculator.TryGetFloat(right, out var val)) {
                            if(s_FloatVars.TryGetValue(name, out var v)){
                                v.Value = val;
                                v.IsValid = true;
                            }
                            else {
                                s_FloatVars.Add(name, new FloatVal(val));
                            }
                        }
                        else {
                            if (s_FloatVars.TryGetValue(name, out var v)) {
                                v.IsValid = false;
                            }
                            else {
                                s_FloatVars.Add(name, new FloatVal());
                            }
                        }
                    }
                    else if (baseType == "int") {
                        if (Calculator.TryGetInt(right, out var val)) {
                            if (s_IntVars.TryGetValue(name, out var v)) {
                                v.Value = val;
                                v.IsValid = true;
                            }
                            else {
                                s_IntVars.Add(name, new IntVal(val));
                            }
                        }
                        else {
                            if (s_IntVars.TryGetValue(name, out var v)) {
                                v.IsValid = false;
                            }
                            else {
                                s_IntVars.Add(name, new IntVal());
                            }
                        }
                    }
                    else if (baseType == "uint") {
                        if (Calculator.TryGetUInt(right, out var val)) {
                            if (s_UintVars.TryGetValue(name, out var v)) {
                                v.Value = val;
                                v.IsValid = true;
                            }
                            else {
                                s_UintVars.Add(name, new UintVal(val));
                            }
                        }
                        else {
                            if (s_UintVars.TryGetValue(name, out var v)) {
                                v.IsValid = false;
                            }
                            else {
                                s_UintVars.Add(name, new UintVal());
                            }
                        }
                    }
                    else if (baseType == "bool") {
                        if (Calculator.TryGetBool(right, out var val)) {
                            if (s_BoolVars.TryGetValue(name, out var v)) {
                                v.Value = val;
                                v.IsValid = true;
                            }
                            else {
                                s_BoolVars.Add(name, new BoolVal(val));
                            }
                        }
                        else {
                            if (s_BoolVars.TryGetValue(name, out var v)) {
                                v.IsValid = false;
                            }
                            else {
                                s_BoolVars.Add(name, new BoolVal());
                            }
                        }
                    }
                }
            }
        }
        public static void AssignValue(ComputeGraphVarNode left, ComputeGraphVarNode right, Dictionary<int, int> argTypeConversion, int argIx)
        {
            string name = left.VarName;
            string type = left.Type;
            string otherName = right.VarName;
            string otherType = right.Type;
            if (Config.ActiveConfig.SettingInfo.UnassignableVariables.Contains(name))
                return;
            if (type == otherType) {
                string baseType = Program.GetTypeRemoveSuffix(type, out var suffix, out var arrNums);
                if (arrNums.Count > 0) {
                    Debug.Assert(arrNums.Count == 1);
                    int arrNum = arrNums[0];
                    if (suffix.Length > 0) {
                        if (baseType == "vec") {
                            if (s_Float4ArrayVars.TryGetValue(otherName, out var vals)) {
                                s_Float4ArrayVars[name] = Float4Val.CloneList(vals);
                            }
                            else if(s_Float4ArrayVars.TryGetValue(name, out var vs)) {
                                foreach(var v  in vs) {
                                    v.IsValid = false;
                                }
                            }
                        }
                        else if (baseType == "ivec") {
                            if (s_Int4ArrayVars.TryGetValue(otherName, out var vals)) {
                                s_Int4ArrayVars[name] = Int4Val.CloneList(vals);
                            }
                            else if (s_Int4ArrayVars.TryGetValue(name, out var vs)) {
                                foreach (var v in vs) {
                                    v.IsValid = false;
                                }
                            }
                        }
                        else if (baseType == "uvec") {
                            if (s_Uint4ArrayVars.TryGetValue(otherName, out var vals)) {
                                s_Uint4ArrayVars[name] = Uint4Val.CloneList(vals);
                            }
                            else if (s_Uint4ArrayVars.TryGetValue(name, out var vs)) {
                                foreach (var v in vs) {
                                    v.IsValid = false;
                                }
                            }
                        }
                        else if (baseType == "bvec") {
                            if (s_Bool4ArrayVars.TryGetValue(otherName, out var vals)) {
                                s_Bool4ArrayVars[name] = Bool4Val.CloneList(vals);
                            }
                            else if (s_Bool4ArrayVars.TryGetValue(name, out var vs)) {
                                foreach (var v in vs) {
                                    v.IsValid = false;
                                }
                            }
                        }
                    }
                    else {
                        if (baseType == "float") {
                            if (s_FloatArrayVars.TryGetValue(otherName, out var vals)) {
                                s_FloatArrayVars[name] = FloatVal.CloneList(vals);
                            }
                            else if (s_FloatArrayVars.TryGetValue(name, out var vs)) {
                                foreach (var v in vs) {
                                    v.IsValid = false;
                                }
                            }
                        }
                        else if (baseType == "int") {
                            if (s_IntArrayVars.TryGetValue(otherName, out var vals)) {
                                s_IntArrayVars[name] = IntVal.CloneList(vals);
                            }
                            else if (s_IntArrayVars.TryGetValue(name, out var vs)) {
                                foreach (var v in vs) {
                                    v.IsValid = false;
                                }
                            }
                        }
                        else if (baseType == "uint") {
                            if (s_UintArrayVars.TryGetValue(otherName, out var vals)) {
                                s_UintArrayVars[name] = UintVal.CloneList(vals);
                            }
                            else if (s_UintArrayVars.TryGetValue(name, out var vs)) {
                                foreach (var v in vs) {
                                    v.IsValid = false;
                                }
                            }
                        }
                        else if (baseType == "bool") {
                            if (s_BoolArrayVars.TryGetValue(otherName, out var vals)) {
                                s_BoolArrayVars[name] = BoolVal.CloneList(vals);
                            }
                            else if (s_BoolArrayVars.TryGetValue(name, out var vs)) {
                                foreach (var v in vs) {
                                    v.IsValid = false;
                                }
                            }
                        }
                    }
                }
                else if (suffix.Length > 0) {
                    if (baseType == "vec") {
                        if (s_Float4Vars.TryGetValue(otherName, out var val) && val.IsValid) {
                            if (s_Float4Vars.TryGetValue(name, out var v)) {
                                v.Value = val.Value;
                                v.IsValid = true;
                            }
                            else {
                                s_Float4Vars[name] = new Float4Val(val.Value);
                            }
                        }
                        else if(GetVarValue(otherName, otherType, out var cv) && Float4.TryParse(cv, out var cval)) {
                            if (s_Float4Vars.TryGetValue(name, out var v)) {
                                v.Value = cval;
                                v.IsValid = true;
                            }
                            else {
                                s_Float4Vars[name] = new Float4Val(cval);
                            }
                        }
                        else if(s_Float4Vars.TryGetValue(name, out var v)){
                            v.IsValid = false;
                        }
                        else {
                            s_Float4Vars[name] = new Float4Val();
                        }
                    }
                    else if (baseType == "ivec") {
                        if (s_Int4Vars.TryGetValue(otherName, out var val) && val.IsValid) {
                            if (s_Int4Vars.TryGetValue(name, out var v)) {
                                v.Value = val.Value;
                                v.IsValid = true;
                            }
                            else {
                                s_Int4Vars[name] = new Int4Val(val.Value);
                            }
                        }
                        else if (GetVarValue(otherName, otherType, out var cv) && Int4.TryParse(cv, out var cval)) {
                            if (s_Int4Vars.TryGetValue(name, out var v)) {
                                v.Value = cval;
                                v.IsValid = true;
                            }
                            else {
                                s_Int4Vars[name] = new Int4Val(cval);
                            }
                        }
                        else if (s_Int4Vars.TryGetValue(name, out var v)) {
                            v.IsValid = false;
                        }
                        else {
                            s_Int4Vars[name] = new Int4Val();
                        }
                    }
                    else if (baseType == "uvec") {
                        if (s_Uint4Vars.TryGetValue(otherName, out var val) && val.IsValid) {
                            if (s_Uint4Vars.TryGetValue(name, out var v)) {
                                v.Value = val.Value;
                                v.IsValid = true;
                            }
                            else {
                                s_Uint4Vars[name] = new Uint4Val(val.Value);
                            }
                        }
                        else if (GetVarValue(otherName, otherType, out var cv) && Uint4.TryParse(cv, out var cval)) {
                            if (s_Uint4Vars.TryGetValue(name, out var v)) {
                                v.Value = cval;
                                v.IsValid = true;
                            }
                            else {
                                s_Uint4Vars[name] = new Uint4Val(cval);
                            }
                        }
                        else if (s_Uint4Vars.TryGetValue(name, out var v)) {
                            v.IsValid = false;
                        }
                        else {
                            s_Uint4Vars[name] = new Uint4Val();
                        }
                    }
                    else if (baseType == "bvec") {
                        if (s_Bool4Vars.TryGetValue(otherName, out var val) && val.IsValid) {
                            if (s_Bool4Vars.TryGetValue(name, out var v)) {
                                v.Value = val.Value;
                                v.IsValid = true;
                            }
                            else {
                                s_Bool4Vars[name] = new Bool4Val(val.Value);
                            }
                        }
                        else if (GetVarValue(otherName, otherType, out var cv) && Bool4.TryParse(cv, out var cval)) {
                            if (s_Bool4Vars.TryGetValue(name, out var v)) {
                                v.Value = cval;
                                v.IsValid = true;
                            }
                            else {
                                s_Bool4Vars[name] = new Bool4Val(cval);
                            }
                        }
                        else if (s_Bool4Vars.TryGetValue(name, out var v)) {
                            v.IsValid = false;
                        }
                        else {
                            s_Bool4Vars[name] = new Bool4Val();
                        }
                    }
                }
                else {
                    if (baseType == "float") {
                        if (s_FloatVars.TryGetValue(otherName, out var val) && val.IsValid) {
                            if (s_FloatVars.TryGetValue(name, out var v)) {
                                v.Value = val.Value;
                                v.IsValid = true;
                            }
                            else {
                                s_FloatVars[name] = new FloatVal(val.Value);
                            }
                        }
                        else if (GetVarValue(otherName, otherType, out var cv) && Calculator.TryGetFloat(cv, out var cval)) {
                            if (s_FloatVars.TryGetValue(name, out var v)) {
                                v.Value = cval;
                                v.IsValid = true;
                            }
                            else {
                                s_FloatVars[name] = new FloatVal(cval);
                            }
                        }
                        else if (s_FloatVars.TryGetValue(name, out var v)) {
                            v.IsValid = false;
                        }
                        else {
                            s_FloatVars[name] = new FloatVal();
                        }
                    }
                    else if (baseType == "int") {
                        if (s_IntVars.TryGetValue(otherName, out var val) && val.IsValid) {
                            if (s_IntVars.TryGetValue(name, out var v)) {
                                v.Value = val.Value;
                                v.IsValid = true;
                            }
                            else {
                                s_IntVars[name] = new IntVal(val.Value);
                            }
                        }
                        else if (GetVarValue(otherName, otherType, out var cv) && Calculator.TryGetInt(cv, out var cval)) {
                            if (s_IntVars.TryGetValue(name, out var v)) {
                                v.Value = cval;
                                v.IsValid = true;
                            }
                            else {
                                s_IntVars[name] = new IntVal(cval);
                            }
                        }
                        else if (s_IntVars.TryGetValue(name, out var v)) {
                            v.IsValid = false;
                        }
                        else {
                            s_IntVars[name] = new IntVal();
                        }
                    }
                    else if (baseType == "uint") {
                        if (s_UintVars.TryGetValue(otherName, out var val) && val.IsValid) {
                            if (s_UintVars.TryGetValue(name, out var v)) {
                                v.Value = val.Value;
                                v.IsValid = true;
                            }
                            else {
                                s_UintVars[name] = new UintVal(val.Value);
                            }
                        }
                        else if (GetVarValue(otherName, otherType, out var cv) && Calculator.TryGetUInt(cv, out var cval)) {
                            if (s_UintVars.TryGetValue(name, out var v)) {
                                v.Value = cval;
                                v.IsValid = true;
                            }
                            else {
                                s_UintVars[name] = new UintVal(cval);
                            }
                        }
                        else if (s_UintVars.TryGetValue(name, out var v)) {
                            v.IsValid = false;
                        }
                        else {
                            s_UintVars[name] = new UintVal();
                        }
                    }
                    else if (baseType == "bool") {
                        if (s_BoolVars.TryGetValue(otherName, out var val) && val.IsValid) {
                            if (s_BoolVars.TryGetValue(name, out var v)) {
                                v.Value = val.Value;
                                v.IsValid = true;
                            }
                            else {
                                s_BoolVars[name] = new BoolVal(val.Value);
                            }
                        }
                        else if (GetVarValue(otherName, otherType, out var cv) && Calculator.TryGetBool(cv, out var cval)) {
                            if (s_BoolVars.TryGetValue(name, out var v)) {
                                v.Value = cval;
                                v.IsValid = true;
                            }
                            else {
                                s_BoolVars[name] = new BoolVal(cval);
                            }
                        }
                        else if (s_BoolVars.TryGetValue(name, out var v)) {
                            v.IsValid = false;
                        }
                        else {
                            s_BoolVars[name] = new BoolVal();
                        }
                    }
                }
            }
            else {
                string baseType = Program.GetTypeRemoveSuffix(type, out var suffix, out var arrNums);
                if(arrNums.Count==0 && suffix.Length == 0) {
                    if (baseType == "float" && otherType == "int") {
                        argTypeConversion[argIx] = ComputeGraphCalcNode.c_action_add_itof;

                        if (s_IntVars.TryGetValue(otherName, out var val) && val.IsValid) {
                            float fv = Calculator.itof(val.Value);
                            if (s_FloatVars.TryGetValue(name, out var v)) {
                                v.Value = fv;
                                v.IsValid = true;
                            }
                            else {
                                s_FloatVars[name] = new FloatVal(fv);
                            }
                        }
                        else if (s_FloatVars.TryGetValue(name, out var v)) {
                            v.IsValid = false;
                        }
                        else {
                            s_FloatVars[name] = new FloatVal();
                        }
                    }
                    else if (baseType == "float" && otherType == "uint") {
                        argTypeConversion[argIx] = ComputeGraphCalcNode.c_action_add_utof;

                        if (s_UintVars.TryGetValue(otherName, out var val) && val.IsValid) {
                            float fv = Calculator.utof(val.Value);
                            if (s_FloatVars.TryGetValue(name, out var v)) {
                                v.Value = fv;
                                v.IsValid = true;
                            }
                            else {
                                s_FloatVars[name] = new FloatVal(fv);
                            }
                        }
                        else if (s_FloatVars.TryGetValue(name, out var v)) {
                            v.IsValid = false;
                        }
                        else {
                            s_FloatVars[name] = new FloatVal();
                        }
                    }
                    else if (baseType == "int" && otherType == "float") {
                        argTypeConversion[argIx] = ComputeGraphCalcNode.c_action_add_ftoi;

                        if (s_FloatVars.TryGetValue(otherName, out var val) && val.IsValid) {
                            int iv = Calculator.ftoi(val.Value);
                            if (s_IntVars.TryGetValue(name, out var v)) {
                                v.Value = iv;
                                v.IsValid = true;
                            }
                            else {
                                s_IntVars[name] = new IntVal(iv);
                            }
                        }
                        else if (s_IntVars.TryGetValue(name, out var v)) {
                            v.IsValid = false;
                        }
                        else {
                            s_IntVars[name] = new IntVal();
                        }
                    }
                    else if (baseType == "uint" && otherType == "float") {
                        argTypeConversion[argIx] = ComputeGraphCalcNode.c_action_add_ftou;

                        if (s_FloatVars.TryGetValue(otherName, out var val) && val.IsValid) {
                            uint uv = Calculator.ftou(val.Value);
                            if (s_UintVars.TryGetValue(name, out var v)) {
                                v.Value = uv;
                                v.IsValid = true;
                            }
                            else {
                                s_UintVars[name] = new UintVal(uv);
                            }
                        }
                        else if (s_UintVars.TryGetValue(name, out var v)) {
                            v.IsValid = false;
                        }
                        else {
                            s_UintVars[name] = new UintVal();
                        }
                    }
                }
            }
        }
        public static void ObjectAssignValue(ComputeGraphVarNode left, DslExpression.CalculatorValue m, DslExpression.CalculatorValue right)
        {
            string name = left.VarName;
            string type = left.Type;
            ObjectAssignValue(name, type, m, right);
        }
        public static void ObjectAssignValue(string name, string type, DslExpression.CalculatorValue m, DslExpression.CalculatorValue right)
        {
            if (Config.ActiveConfig.SettingInfo.UnassignableVariables.Contains(name))
                return;
            if (Config.ActiveConfig.SettingInfo.UnassignableObjectMembers.TryGetValue(name, out var members) && members.Contains(m)) {
                return;
            }
            string baseType = Program.GetTypeRemoveSuffix(type, out var suffix, out var arrNums);
            if (arrNums.Count == 0 && suffix.Length > 0) {
                if (baseType == "vec") {
                    if (s_Float4Vars.TryGetValue(name, out var val)) {
                        val.IsValid = val.Value.SetMember(m, right);
                    }
                    else {
                        var v = new Float4();
                        if (v.SetMember(m, right))
                            s_Float4Vars.Add(name, new Float4Val(v));
                    }
                }
                else if (baseType == "ivec") {
                    if (s_Int4Vars.TryGetValue(name, out var val)) {
                        val.IsValid = val.Value.SetMember(m, right);
                    }
                    else {
                        var v = new Int4();
                        if (v.SetMember(m, right))
                            s_Int4Vars.Add(name, new Int4Val(v));
                    }
                }
                else if (baseType == "uvec") {
                    if (s_Uint4Vars.TryGetValue(name, out var val)) {
                        val.IsValid = val.Value.SetMember(m, right);
                    }
                    else {
                        var v = new Uint4();
                        if (v.SetMember(m, right))
                            s_Uint4Vars.Add(name, new Uint4Val(v));
                    }
                }
                else if (baseType == "bvec") {
                    if (s_Bool4Vars.TryGetValue(name, out var val)) {
                        val.IsValid = val.Value.SetMember(m, right);
                    }
                    else {
                        var v = new Bool4();
                        if (v.SetMember(m, right))
                            s_Bool4Vars.Add(name, new Bool4Val(v));
                    }
                }
            }
        }
        public static void ArrayAssignValue(ComputeGraphVarNode left, DslExpression.CalculatorValue ix, DslExpression.CalculatorValue right)
        {
            string name = left.VarName;
            string type = left.Type;
            ArrayAssignValue(name, type, ix, right);
        }
        public static void ArrayAssignValue(string name, string type, DslExpression.CalculatorValue ix, DslExpression.CalculatorValue right)
        {
            if (!Calculator.TryGetInt(ix, out var index) || index < 0) {
                return;
            }
            if (Config.ActiveConfig.SettingInfo.UnassignableVariables.Contains(name))
                return;
            if (Config.ActiveConfig.SettingInfo.UnassignableArrayElements.TryGetValue(name, out var skipIndexes) && skipIndexes.Contains(index)) {
                return;
            }
            string baseType = Program.GetTypeRemoveSuffix(type, out var suffix, out var arrNums);
            if (arrNums.Count > 0) {
                Debug.Assert(arrNums.Count == 1);
                int arrNum = arrNums[0];
                if (suffix.Length > 0) {
                    if (baseType == "vec") {
                        if (s_Float4ArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            if (Float4.TryParse(right, out var val)) {
                                vals[index].Value = val;
                                vals[index].IsValid = true;
                            }
                            else {
                                vals[index].IsValid = false;
                            }
                        }
                    }
                    else if (baseType == "ivec") {
                        if (s_Int4ArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            if (Int4.TryParse(right, out var val)) {
                                vals[index].Value = val;
                                vals[index].IsValid = true;
                            }
                            else {
                                vals[index].IsValid = false;
                            }
                        }
                    }
                    else if (baseType == "uvec") {
                        if (s_Uint4ArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            if (Uint4.TryParse(right, out var val)) {
                                vals[index].Value = val;
                                vals[index].IsValid = true;
                            }
                            else {
                                vals[index].IsValid = false;
                            }
                        }
                    }
                    else if (baseType == "bvec") {
                        if (s_Bool4ArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            if (Bool4.TryParse(right, out var val)) {
                                vals[index].Value = val;
                                vals[index].IsValid = true;
                            }
                            else {
                                vals[index].IsValid = false;
                            }
                        }
                    }
                }
                else {
                    if (baseType == "float") {
                        if (s_FloatArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            if (Calculator.TryGetFloat(right, out var val)) {
                                vals[index].Value = val;
                                vals[index].IsValid = true;
                            }
                            else {
                                vals[index].IsValid = false;
                            }
                        }
                    }
                    else if (baseType == "int") {
                        if (s_IntArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            if (Calculator.TryGetInt(right, out var val)) {
                                vals[index].Value = val;
                                vals[index].IsValid = true;
                            }
                            else {
                                vals[index].IsValid = false;
                            }
                        }
                    }
                    else if (baseType == "uint") {
                        if (s_UintArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            if (Calculator.TryGetUInt(right, out var val)) {
                                vals[index].Value = val;
                                vals[index].IsValid = true;
                            }
                            else {
                                vals[index].IsValid = false;
                            }
                        }
                    }
                    else if (baseType == "bool") {
                        if (s_BoolArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            if (Calculator.TryGetBool(right, out var val)) {
                                vals[index].Value = val;
                                vals[index].IsValid = true;
                            }
                            else {
                                vals[index].IsValid = false;
                            }
                        }
                    }
                }
            }
            else if (suffix.Length > 0) {
                if (baseType == "vec") {
                    if (s_Float4Vars.TryGetValue(name, out var val)) {
                        val.IsValid = val.Value.SetMember(ix, right);
                    }
                    else {
                        var v = new Float4();
                        if (v.SetMember(ix, right))
                            s_Float4Vars.Add(name, new Float4Val(v));
                    }
                }
                else if (baseType == "ivec") {
                    if (s_Int4Vars.TryGetValue(name, out var val)) {
                        val.IsValid = val.Value.SetMember(ix, right);
                    }
                    else {
                        var v = new Int4();
                        if (v.SetMember(ix, right))
                            s_Int4Vars.Add(name, new Int4Val(v));
                    }
                }
                else if (baseType == "uvec") {
                    if (s_Uint4Vars.TryGetValue(name, out var val)) {
                        val.IsValid = val.Value.SetMember(ix, right);
                    }
                    else {
                        var v = new Uint4();
                        if (v.SetMember(ix, right))
                            s_Uint4Vars.Add(name, new Uint4Val(v));
                    }
                }
                else if (baseType == "bvec") {
                    if (s_Bool4Vars.TryGetValue(name, out var val)) {
                        val.IsValid = val.Value.SetMember(ix, right);
                    }
                    else {
                        var v = new Bool4();
                        if (v.SetMember(ix, right))
                            s_Bool4Vars.Add(name, new Bool4Val(v));
                    }
                }
            }
        }
        public static void ObjectArrayAssignValue(ComputeGraphVarNode left, DslExpression.CalculatorValue ix, DslExpression.CalculatorValue m, DslExpression.CalculatorValue right)
        {
            string name = left.VarName;
            string type = left.Type;
            ObjectArrayAssignValue(name, type, ix, m, right);
        }
        public static void ObjectArrayAssignValue(string name, string type, DslExpression.CalculatorValue ix, DslExpression.CalculatorValue m, DslExpression.CalculatorValue right)
        {
            if (!Calculator.TryGetInt(ix, out var index) || index < 0) {
                return;
            }
            if (Config.ActiveConfig.SettingInfo.UnassignableVariables.Contains(name))
                return;
            if (Config.ActiveConfig.SettingInfo.UnassignableArrayElements.TryGetValue(name, out var skipIndexes) && skipIndexes.Contains(index)) {
                return;
            }
            if (Config.ActiveConfig.SettingInfo.UnassignableObjectMembers.TryGetValue(name, out var members) && members.Contains(m)) {
                return;
            }
            if (Config.ActiveConfig.SettingInfo.UnassignableObjectArrayMembers.TryGetValue(name, out var list) && list.TryGetValue(index, out var hashSet) && hashSet.Contains(m)) {
                return;
            }
            string baseType = Program.GetTypeRemoveSuffix(type, out var suffix, out var arrNums);
            if (arrNums.Count > 0) {
                Debug.Assert(arrNums.Count == 1);
                int arrNum = arrNums[0];
                if (suffix.Length > 0) {
                    if (baseType == "vec") {
                        if (s_Float4ArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            var val = vals[index].Value;
                            if (val.SetMember(m, right)) {
                                vals[index].Value = val;
                                vals[index].IsValid = true;
                            }
                            else {
                                vals[index].IsValid = false;
                            }
                        }
                    }
                    else if (baseType == "ivec") {
                        if (s_Int4ArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            var val = vals[index].Value;
                            if (val.SetMember(m, right)) {
                                vals[index].Value = val;
                                vals[index].IsValid = true;
                            }
                            else {
                                vals[index].IsValid = false;
                            }
                        }
                    }
                    else if (baseType == "uvec") {
                        if (s_Uint4ArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            var val = vals[index].Value;
                            if (val.SetMember(m, right)) {
                                vals[index].Value = val;
                                vals[index].IsValid = true;
                            }
                            else {
                                vals[index].IsValid = false;
                            }
                        }
                    }
                    else if (baseType == "bvec") {
                        if (s_Bool4ArrayVars.TryGetValue(name, out var vals) && index < vals.Count) {
                            var val = vals[index].Value;
                            if (val.SetMember(m, right)) {
                                vals[index].Value = val;
                                vals[index].IsValid = true;
                            }
                            else {
                                vals[index].IsValid = false;
                            }
                        }
                    }
                }
            }
        }

        public static bool TryGetVariable(string v, out DslExpression.CalculatorValue result)
        {
            bool ret = false;
            result = DslExpression.CalculatorValue.NullObject;
            if (TryGetVarType(v, out var type, out var isArray)) {
                if (isArray) {
                    if (type == "vec4") {
                        if (s_Float4ArrayVars.TryGetValue(v, out var list)) {
                            result = DslExpression.CalculatorValue.FromObject(list);
                            ret = true;
                        }
                    }
                    else if (type == "ivec4") {
                        if (s_Int4ArrayVars.TryGetValue(v, out var list)) {
                            result = DslExpression.CalculatorValue.FromObject(list);
                            ret = true;
                        }
                    }
                    else if (type == "uvec4") {
                        if (s_Uint4ArrayVars.TryGetValue(v, out var list)) {
                            result = DslExpression.CalculatorValue.FromObject(list);
                            ret = true;
                        }
                    }
                    else if (type == "bvec4") {
                        if (s_Bool4ArrayVars.TryGetValue(v, out var list)) {
                            result = DslExpression.CalculatorValue.FromObject(list);
                            ret = true;
                        }
                    }
                    else if (type == "float") {
                        if (s_FloatArrayVars.TryGetValue(v, out var list)) {
                            result = DslExpression.CalculatorValue.FromObject(list);
                            ret = true;
                        }
                    }
                    else if (type == "int") {
                        if (s_IntArrayVars.TryGetValue(v, out var list)) {
                            result = DslExpression.CalculatorValue.FromObject(list);
                            ret = true;
                        }
                    }
                    else if (type == "uint") {
                        if (s_UintArrayVars.TryGetValue(v, out var list)) {
                            result = DslExpression.CalculatorValue.FromObject(list);
                            ret = true;
                        }
                    }
                    else if (type == "bool") {
                        if (s_BoolArrayVars.TryGetValue(v, out var list)) {
                            result = DslExpression.CalculatorValue.FromObject(list);
                            ret = true;
                        }
                    }
                }
                else {
                    if (type == "vec4") {
                        if (s_Float4Vars.TryGetValue(v, out var val)) {
                            result = DslExpression.CalculatorValue.FromObject(val.Value);
                            ret = true;
                        }
                    }
                    else if (type == "ivec4") {
                        if (s_Int4Vars.TryGetValue(v, out var val)) {
                            result = DslExpression.CalculatorValue.FromObject(val.Value);
                            ret = true;
                        }
                    }
                    else if (type == "uvec4") {
                        if (s_Uint4Vars.TryGetValue(v, out var val)) {
                            result = DslExpression.CalculatorValue.FromObject(val.Value);
                            ret = true;
                        }
                    }
                    else if (type == "bvec4") {
                        if (s_Bool4Vars.TryGetValue(v, out var val)) {
                            result = DslExpression.CalculatorValue.FromObject(val.Value);
                            ret = true;
                        }
                    }
                    else if (type == "float") {
                        if (s_FloatVars.TryGetValue(v, out var val)) {
                            result = DslExpression.CalculatorValue.From(val.Value);
                            ret = true;
                        }
                    }
                    else if (type == "int") {
                        if (s_IntVars.TryGetValue(v, out var val)) {
                            result = DslExpression.CalculatorValue.From(val.Value);
                            ret = true;
                        }
                    }
                    else if (type == "uint") {
                        if (s_UintVars.TryGetValue(v, out var val)) {
                            result = DslExpression.CalculatorValue.From(val.Value);
                            ret = true;
                        }
                    }
                    else if (type == "bool") {
                        if (s_BoolVars.TryGetValue(v, out var val)) {
                            result = DslExpression.CalculatorValue.From(val.Value);
                            ret = true;
                        }
                    }
                }
            }
            return ret;
        }
        public static bool TrySetVariable(string v, ref DslExpression.CalculatorValue result)
        {
            bool ret = false;
            if (TryGetVarType(v, out var type, out var isArray)) {
                if (isArray) {
                    if (type == "vec4") {
                        if (s_Float4ArrayVars.TryGetValue(v, out var list) && result.ObjectVal is List<Float4Val> newList) {
                            s_Float4ArrayVars[v] = Float4Val.CloneList(newList);
                            ret = true;
                        }
                    }
                    else if (type == "ivec4") {
                        if (s_Int4ArrayVars.TryGetValue(v, out var list) && result.ObjectVal is List<Int4Val> newList) {
                            s_Int4ArrayVars[v] = Int4Val.CloneList(newList);
                            ret = true;
                        }
                    }
                    else if (type == "uvec4") {
                        if (s_Uint4ArrayVars.TryGetValue(v, out var list) && result.ObjectVal is List<Uint4Val> newList) {
                            s_Uint4ArrayVars[v] = Uint4Val.CloneList(newList);
                            ret = true;
                        }
                    }
                    else if (type == "bvec4") {
                        if (s_Bool4ArrayVars.TryGetValue(v, out var list) && result.ObjectVal is List<Bool4Val> newList) {
                            s_Bool4ArrayVars[v] = Bool4Val.CloneList(newList);
                            ret = true;
                        }
                    }
                    else if (type == "float") {
                        if (s_FloatArrayVars.TryGetValue(v, out var list) && result.ObjectVal is List<FloatVal> newList) {
                            s_FloatArrayVars[v] = FloatVal.CloneList(newList);
                            ret = true;
                        }
                    }
                    else if (type == "int") {
                        if (s_IntArrayVars.TryGetValue(v, out var list) && result.ObjectVal is List<IntVal> newList) {
                            s_IntArrayVars[v] = IntVal.CloneList(newList);
                            ret = true;
                        }
                    }
                    else if (type == "uint") {
                        if (s_UintArrayVars.TryGetValue(v, out var list) && result.ObjectVal is List<UintVal> newList) {
                            s_UintArrayVars[v] = UintVal.CloneList(newList);
                            ret = true;
                        }
                    }
                    else if (type == "bool") {
                        if (s_BoolArrayVars.TryGetValue(v, out var list) && result.ObjectVal is List<BoolVal> newList) {
                            s_BoolArrayVars[v] = BoolVal.CloneList(newList);
                            ret = true;
                        }
                    }
                }
                else {
                    if (type == "vec4") {
                        if (result.ObjectVal is Float4 newVal) {
                            if (s_Float4Vars.TryGetValue(v, out var val)) {
                                val.Value = newVal;
                                val.IsValid = true;
                            }
                            else {
                                s_Float4Vars.Add(v, new Float4Val(newVal));
                            }
                            ret = true;
                        }
                    }
                    else if (type == "ivec4") {
                        if (result.ObjectVal is Int4 newVal) {
                            if (s_Int4Vars.TryGetValue(v, out var val)) {
                                val.Value = newVal;
                                val.IsValid = true;
                            }
                            else {
                                s_Int4Vars.Add(v, new Int4Val(newVal));
                            }
                            ret = true;
                        }
                    }
                    else if (type == "uvec4") {
                        if (result.ObjectVal is Uint4 newVal) {
                            if (s_Uint4Vars.TryGetValue(v, out var val)) {
                                val.Value = newVal;
                                val.IsValid = true;
                            }
                            else {
                                s_Uint4Vars.Add(v, new Uint4Val(newVal));
                            }
                            ret = true;
                        }
                    }
                    else if (type == "bvec4") {
                        if (result.ObjectVal is Bool4 newVal) {
                            if (s_Bool4Vars.TryGetValue(v, out var val)) {
                                val.Value = newVal;
                                val.IsValid = true;
                            }
                            else {
                                s_Bool4Vars.Add(v, new Bool4Val(newVal));
                            }
                            ret = true;
                        }
                    }
                    else if (type == "float") {
                        if (Calculator.TryGetFloat(result, out var newVal)) {
                            if (s_FloatVars.TryGetValue(v, out var val)) {
                                val.Value = newVal;
                                val.IsValid = true;
                            }
                            else {
                                s_FloatVars.Add(v, new FloatVal(newVal));
                            }
                            ret = true;
                        }
                    }
                    else if (type == "int") {
                        if (Calculator.TryGetInt(result, out var newVal)) {
                            if (s_IntVars.TryGetValue(v, out var val)) {
                                val.Value = newVal;
                                val.IsValid = true;
                            }
                            else {
                                s_IntVars.Add(v, new IntVal(newVal));
                            }
                            ret = true;
                        }
                    }
                    else if (type == "uint") {
                        if (Calculator.TryGetUInt(result, out var newVal)) {
                            if (s_UintVars.TryGetValue(v, out var val)) {
                                val.Value = newVal;
                                val.IsValid = true;
                            }
                            else {
                                s_UintVars.Add(v, new UintVal(newVal));
                            }
                            ret = true;
                        }
                    }
                    else if (type == "bool") {
                        if (Calculator.TryGetBool(result, out var newVal)) {
                            if (s_BoolVars.TryGetValue(v, out var val)) {
                                val.Value = newVal;
                                val.IsValid = true;
                            }
                            else {
                                s_BoolVars.Add(v, new BoolVal(newVal));
                            }
                            ret = true;
                        }
                    }
                }
            }
            else {
                if (result.ObjectVal is List<Float4Val> f4List) {
                    s_Float4ArrayVars.Add(v, Float4Val.CloneList(f4List));
                    ret = true;
                }
                else if (result.ObjectVal is List<Int4Val> i4List) {
                    s_Int4ArrayVars.Add(v, Int4Val.CloneList(i4List));
                    ret = true;
                }
                else if (result.ObjectVal is List<Uint4Val> u4List) {
                    s_Uint4ArrayVars.Add(v, Uint4Val.CloneList(u4List));
                    ret = true;
                }
                else if (result.ObjectVal is List<Bool4Val> b4List) {
                    s_Bool4ArrayVars.Add(v, Bool4Val.CloneList(b4List));
                    ret = true;
                }
                else if (result.ObjectVal is List<FloatVal> fList) {
                    s_FloatArrayVars.Add(v, FloatVal.CloneList(fList));
                    ret = true;
                }
                else if (result.ObjectVal is List<IntVal> iList) {
                    s_IntArrayVars.Add(v, IntVal.CloneList(iList));
                    ret = true;
                }
                else if (result.ObjectVal is List<UintVal> uList) {
                    s_UintArrayVars.Add(v, UintVal.CloneList(uList));
                    ret = true;
                }
                else if (result.ObjectVal is List<BoolVal> bList) {
                    s_BoolArrayVars.Add(v, BoolVal.CloneList(bList));
                    ret = true;
                }
                else if (result.ObjectVal is Float4 f4) {
                    s_Float4Vars.Add(v, new Float4Val(f4));
                    ret = true;
                }
                else if (result.ObjectVal is Int4 i4) {
                    s_Int4Vars.Add(v, new Int4Val(i4));
                    ret = true;
                }
                else if (result.ObjectVal is Uint4 u4) {
                    s_Uint4Vars.Add(v, new Uint4Val(u4));
                    ret = true;
                }
                else if (result.ObjectVal is Bool4 b4) {
                    s_Bool4Vars.Add(v, new Bool4Val(b4));
                    ret = true;
                }
                else if (result.IsNumber) {
                    s_FloatVars.Add(v, new FloatVal(result.GetFloat()));
                    ret = true;
                }
                else if (result.IsSignedInteger) {
                    s_IntVars.Add(v, new IntVal(result.GetInt()));
                    ret = true;
                }
                else if (result.IsUnsignedInteger) {
                    s_UintVars.Add(v, new UintVal(result.GetUInt()));
                    ret = true;
                }
                else if (result.IsBoolean) {
                    s_BoolVars.Add(v, new BoolVal(result.GetBool()));
                    ret = true;
                }
            }
            if(ret)
                Program.ResetValueDependsVar(v);
            return ret;
        }
        public static bool TrySetObject(string v, string m, ref DslExpression.CalculatorValue result)
        {
            bool ret = false;
            if (TryGetVarType(v, out var type, out var isArray)) {
                if (!isArray) {
                    if (type == "vec4") {
                        if (Calculator.TryGetFloat(result, out var newVal)) {
                            if (s_Float4Vars.TryGetValue(v, out var val)) {
                                val.Value.SetMember(m, newVal);
                                val.IsValid = true;
                            }
                            else {
                                var nv = new Float4();
                                nv.SetMember(m, newVal);
                                s_Float4Vars.Add(v, new Float4Val(nv));
                            }
                            ret = true;
                        }
                    }
                    else if (type == "ivec4") {
                        if (Calculator.TryGetInt(result, out var newVal)) {
                            if (s_Int4Vars.TryGetValue(v, out var val)) {
                                val.Value.SetMember(m, newVal);
                                val.IsValid = true;
                            }
                            else {
                                var nv = new Int4();
                                nv.SetMember(m, newVal);
                                s_Int4Vars.Add(v, new Int4Val(nv));
                            }
                            ret = true;
                        }
                    }
                    else if (type == "uvec4") {
                        if (Calculator.TryGetUInt(result, out var newVal)) {
                            if (s_Uint4Vars.TryGetValue(v, out var val)) {
                                val.Value.SetMember(m, newVal);
                                val.IsValid = true;
                            }
                            else {
                                var nv = new Uint4();
                                nv.SetMember(m, newVal);
                                s_Uint4Vars.Add(v, new Uint4Val(nv));
                            }
                            ret = true;
                        }
                    }
                    else if (type == "bvec4") {
                        if (result.IsBoolean) {
                            if (s_Bool4Vars.TryGetValue(v, out var val)) {
                                val.Value.SetMember(m, result);
                                val.IsValid = true;
                            }
                            else {
                                var nv = new Bool4();
                                nv.SetMember(m, result);
                                s_Bool4Vars.Add(v, new Bool4Val(nv));
                            }
                            ret = true;
                        }
                    }
                }
            }
            else {
                if (Calculator.TryGetFloat(result, out var newVal)) {
                    var nv = new Float4();
                    nv.SetMember(m, result.GetFloat());
                    s_Float4Vars.Add(v, new Float4Val(nv));
                    ret = true;
                }
                else if (result.IsSignedInteger) {
                    var nv = new Int4();
                    nv.SetMember(m, newVal);
                    s_Int4Vars.Add(v, new Int4Val(nv));
                    ret = true;
                }
                else if (result.IsUnsignedInteger) {
                    var nv = new Uint4();
                    nv.SetMember(m, newVal);
                    s_Uint4Vars.Add(v, new Uint4Val(nv));
                    ret = true;
                }
                else if (result.IsBoolean) {
                    var nv = new Bool4();
                    nv.SetMember(m, result);
                    s_Bool4Vars.Add(v, new Bool4Val(nv));
                    ret = true;
                }
            }
            if (ret)
                Program.ResetValueDependsVar(v);
            return ret;
        }
        public static bool TrySetArray(string v, int ix, ref DslExpression.CalculatorValue result)
        {
            bool ret = false;
            if (TryGetVarType(v, out var type, out var isArray)) {
                if (isArray) {
                    if (type == "vec4") {
                        if (s_Float4ArrayVars.TryGetValue(v, out var list) && ix >= 0 && ix < list.Count && result.ObjectVal is Float4 newVal) {
                            list[ix].Value = newVal;
                            list[ix].IsValid = true;
                            ret = true;
                        }
                    }
                    else if (type == "ivec4") {
                        if (s_Int4ArrayVars.TryGetValue(v, out var list) && ix >= 0 && ix < list.Count && result.ObjectVal is Int4 newVal) {
                            list[ix].Value = newVal;
                            list[ix].IsValid = true;
                            ret = true;
                        }
                    }
                    else if (type == "uvec4") {
                        if (s_Uint4ArrayVars.TryGetValue(v, out var list) && ix >= 0 && ix < list.Count && result.ObjectVal is Uint4 newVal) {
                            list[ix].Value = newVal;
                            list[ix].IsValid = true;
                            ret = true;
                        }
                    }
                    else if (type == "bvec4") {
                        if (s_Bool4ArrayVars.TryGetValue(v, out var list) && ix >= 0 && ix < list.Count && result.ObjectVal is Bool4 newVal) {
                            list[ix].Value = newVal;
                            list[ix].IsValid = true;
                            ret = true;
                        }
                    }
                    else if (type == "float") {
                        if (s_FloatArrayVars.TryGetValue(v, out var list) && ix >= 0 && ix < list.Count && Calculator.TryGetFloat(result, out var newVal)) {
                            list[ix].Value = result.GetFloat();
                            list[ix].IsValid = true;
                            ret = true;
                        }
                    }
                    else if (type == "int") {
                        if (s_IntArrayVars.TryGetValue(v, out var list) && ix >= 0 && ix < list.Count && Calculator.TryGetInt(result, out var newVal)) {
                            list[ix].Value = result.GetInt();
                            list[ix].IsValid = true;
                            ret = true;
                        }
                    }
                    else if (type == "uint") {
                        if (s_UintArrayVars.TryGetValue(v, out var list) && ix >= 0 && ix < list.Count && Calculator.TryGetInt(result, out var newVal)) {
                            list[ix].Value = result.GetUInt();
                            list[ix].IsValid = true;
                            ret = true;
                        }
                    }
                    else if (type == "bool") {
                        if (s_BoolArrayVars.TryGetValue(v, out var list) && ix >= 0 && ix < list.Count && Calculator.TryGetBool(result, out var newVal)) {
                            list[ix].Value = result.GetBool();
                            list[ix].IsValid = true;
                            ret = true;
                        }
                    }
                }
                else {
                    if (type == "vec4") {
                        if (Calculator.TryGetFloat(result, out var newVal)) {
                            if (s_Float4Vars.TryGetValue(v, out var val)) {
                                val.Value.SetMember(ix, result.GetFloat());
                                val.IsValid = true;
                            }
                            else {
                                var nv = new Float4();
                                nv.SetMember(ix, result.GetFloat());
                                s_Float4Vars.Add(v, new Float4Val(nv));
                            }
                            ret = true;
                        }
                    }
                    else if (type == "ivec4") {
                        if (Calculator.TryGetInt(result, out var newVal)) {
                            if (s_Int4Vars.TryGetValue(v, out var val)) {
                                val.Value.SetMember(ix, newVal);
                                val.IsValid = true;
                            }
                            else {
                                var nv = new Int4();
                                nv.SetMember(ix, newVal);
                                s_Int4Vars.Add(v, new Int4Val(nv));
                            }
                            ret = true;
                        }
                    }
                    else if (type == "uvec4") {
                        if (Calculator.TryGetInt(result, out var newVal)) {
                            if (s_Uint4Vars.TryGetValue(v, out var val)) {
                                val.Value.SetMember(ix, newVal);
                                val.IsValid = true;
                            }
                            else {
                                var nv = new Uint4();
                                nv.SetMember(ix, newVal);
                                s_Uint4Vars.Add(v, new Uint4Val(nv));
                            }
                            ret = true;
                        }
                    }
                    else if (type == "bvec4") {
                        if (Calculator.TryGetBool(result, out var newVal)) {
                            if (s_Bool4Vars.TryGetValue(v, out var val)) {
                                val.Value.SetMember(ix, newVal);
                                val.IsValid = true;
                            }
                            else {
                                var nv = new Bool4();
                                nv.SetMember(ix, newVal);
                                s_Bool4Vars.Add(v, new Bool4Val(nv));
                            }
                            ret = true;
                        }
                    }
                }
            }
            if (ret)
                Program.ResetValueDependsVar(v);
            return ret;
        }
        public static bool TrySetObjArray(string v, int ix, string m, ref DslExpression.CalculatorValue result)
        {
            bool ret = false;
            if (TryGetVarType(v, out var type, out var isArray)) {
                if (isArray) {
                    if (type == "vec4") {
                        if (s_Float4ArrayVars.TryGetValue(v, out var list) && ix >= 0 && ix < list.Count && Calculator.TryGetFloat(result, out var newVal)) {
                            var nv = new Float4();
                            nv.SetMember(m, newVal);
                            list[ix].Value = nv;
                            list[ix].IsValid = true;
                            ret = true;
                        }
                    }
                    else if (type == "ivec4") {
                        if (s_Int4ArrayVars.TryGetValue(v, out var list) && ix >= 0 && ix < list.Count && Calculator.TryGetInt(result, out var newVal)) {
                            var nv = new Int4();
                            nv.SetMember(m, newVal);
                            list[ix].Value = nv;
                            list[ix].IsValid = true;
                            ret = true;
                        }
                    }
                    else if (type == "uvec4") {
                        if (s_Uint4ArrayVars.TryGetValue(v, out var list) && ix >= 0 && ix < list.Count && Calculator.TryGetInt(result, out var newVal)) {
                            var nv = new Uint4();
                            nv.SetMember(m, newVal);
                            list[ix].Value = nv;
                            list[ix].IsValid = true;
                            ret = true;
                        }
                    }
                    else if (type == "bvec4") {
                        if (s_Bool4ArrayVars.TryGetValue(v, out var list) && ix >= 0 && ix < list.Count && Calculator.TryGetBool(result, out var newVal)) {
                            var nv = new Bool4();
                            nv.SetMember(m, newVal);
                            list[ix].Value = nv;
                            list[ix].IsValid = true;
                            ret = true;
                        }
                    }
                }
            }
            if (ret)
                Program.ResetValueDependsVar(v);
            return ret;
        }

        public static Dictionary<string, BoolVal> s_BoolVars = new Dictionary<string, BoolVal>();
        public static Dictionary<string, FloatVal> s_FloatVars = new Dictionary<string, FloatVal>();
        public static Dictionary<string, IntVal> s_IntVars = new Dictionary<string, IntVal>();
        public static Dictionary<string, UintVal> s_UintVars = new Dictionary<string, UintVal>();
        public static Dictionary<string, Bool4Val> s_Bool4Vars = new Dictionary<string, Bool4Val>();
        public static Dictionary<string, Float4Val> s_Float4Vars = new Dictionary<string, Float4Val>();
        public static Dictionary<string, Int4Val> s_Int4Vars = new Dictionary<string, Int4Val>();
        public static Dictionary<string, Uint4Val> s_Uint4Vars = new Dictionary<string, Uint4Val>();
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
