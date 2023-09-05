using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlslRewriter
{
    public static class Calculator
    {
        public static bool CalcFunc(string func, IList<string> args, out string val)
        {
            bool succ = false;
            val = string.Empty;
            if (func == "bool") {
                if (TryParseBool(args[0], out var v)) {
                    val = v.ToString();
                    succ = true;
                }
            }
            else if (func == "float" || func == "double") {
                if (double.TryParse(args[0], out var v)) {
                    val = v.ToString();
                    succ = true;
                }
            }
            else if (func == "int") {
                if (long.TryParse(args[0], out var v)) {
                    val = v.ToString();
                    succ = true;
                }
            }
            else if (func == "uint") {
                if (ulong.TryParse(args[0], out var v)) {
                    val = v.ToString();
                    succ = true;
                }
            }
            else if (func == "bvec2") {
                if (args.Count == 1) {
                    if (TryParseBool(args[0], out var v)) {
                        Bool2 tmp = new Bool2();
                        tmp.x = tmp.y = v;
                        val = tmp.ToString();
                        succ = true;
                    }
                }
                else if (args.Count == 2) {
                    if (TryParseBool(args[0], out var v1) && TryParseBool(args[1], out var v2)) {
                        Bool2 tmp = new Bool2();
                        tmp.x = v1;
                        tmp.y = v2;
                        val = tmp.ToString();
                        succ = true;
                    }
                }
            }
            else if (func == "vec2") {
                if (args.Count == 1) {
                    if (float.TryParse(args[0], out var v)) {
                        Float2 tmp = new Float2();
                        tmp.x = tmp.y = v;
                        val = tmp.ToString();
                        succ = true;
                    }
                    else if (Int2.TryParse(args[0], out var i2)) {
                        Float2 tmp = new Float2();
                        tmp.x = (float)i2.x;
                        tmp.y = (float)i2.y;
                        val = tmp.ToString();
                        succ = true;
                    }
                    else if (Uint2.TryParse(args[0], out var u2)) {
                        Float2 tmp = new Float2();
                        tmp.x = (float)u2.x;
                        tmp.y = (float)u2.y;
                        val = tmp.ToString();
                        succ = true;
                    }
                }
                else if (args.Count == 2) {
                    if (float.TryParse(args[0], out var v1) && float.TryParse(args[1], out var v2)) {
                        Float2 tmp = new Float2();
                        tmp.x = v1;
                        tmp.y = v2;
                        val = tmp.ToString();
                        succ = true;
                    }
                }
            }
            else if (func == "ivec2") {
                if (args.Count == 1) {
                    if (int.TryParse(args[0], out var v)) {
                        Int2 tmp = new Int2();
                        tmp.x = tmp.y = v;
                        val = tmp.ToString();
                        succ = true;
                    }
                    else if (Uint2.TryParse(args[0], out var u2)) {
                        Int2 tmp = new Int2();
                        tmp.x = (int)u2.x;
                        tmp.y = (int)u2.y;
                        val = tmp.ToString();
                        succ = true;
                    }
                    else if (Float2.TryParse(args[0], out var f2)) {
                        Int2 tmp = new Int2();
                        tmp.x = (int)f2.x;
                        tmp.y = (int)f2.y;
                        val = tmp.ToString();
                        succ = true;
                    }
                }
                else if (args.Count == 2) {
                    if (int.TryParse(args[0], out var v1) && int.TryParse(args[1], out var v2)) {
                        Int2 tmp = new Int2();
                        tmp.x = v1;
                        tmp.y = v2;
                        val = tmp.ToString();
                        succ = true;
                    }
                }
            }
            else if (func == "uvec2") {
                if (args.Count == 1) {
                    if (uint.TryParse(args[0], out var v)) {
                        Uint2 tmp = new Uint2();
                        tmp.x = tmp.y = v;
                        val = tmp.ToString();
                        succ = true;
                    }
                    else if (Int2.TryParse(args[0], out var i2)) {
                        Uint2 tmp = new Uint2();
                        tmp.x = (uint)i2.x;
                        tmp.y = (uint)i2.y;
                        val = tmp.ToString();
                        succ = true;
                    }
                    else if (Float2.TryParse(args[0], out var f2)) {
                        Uint2 tmp = new Uint2();
                        tmp.x = (uint)f2.x;
                        tmp.y = (uint)f2.y;
                        val = tmp.ToString();
                        succ = true;
                    }
                }
                else if (args.Count == 2) {
                    if (uint.TryParse(args[0], out var v1) && uint.TryParse(args[1], out var v2)) {
                        Uint2 tmp = new Uint2();
                        tmp.x = v1;
                        tmp.y = v2;
                        val = tmp.ToString();
                        succ = true;
                    }
                }
            }
            else if (func == "bvec3") {
                if (args.Count == 1) {
                    if (TryParseBool(args[0], out var v)) {
                        Bool3 tmp = new Bool3();
                        tmp.x = tmp.y = tmp.z = v;
                        val = tmp.ToString();
                        succ = true;
                    }
                }
                else if (args.Count == 3) {
                    if (TryParseBool(args[0], out var v1) && TryParseBool(args[1], out var v2) && TryParseBool(args[2], out var v3)) {
                        Bool3 tmp = new Bool3();
                        tmp.x = v1;
                        tmp.y = v2;
                        tmp.z = v3;
                        val = tmp.ToString();
                        succ = true;
                    }
                }
            }
            else if (func == "vec3") {
                if (args.Count == 1) {
                    if (float.TryParse(args[0], out var v)) {
                        Float3 tmp = new Float3();
                        tmp.x = tmp.y = tmp.z = v;
                        val = tmp.ToString();
                        succ = true;
                    }
                }
                else if (args.Count == 3) {
                    if (float.TryParse(args[0], out var v1) && float.TryParse(args[1], out var v2) && float.TryParse(args[2], out var v3)) {
                        Float3 tmp = new Float3();
                        tmp.x = v1;
                        tmp.y = v2;
                        tmp.z = v3;
                        val = tmp.ToString();
                        succ = true;
                    }
                }
            }
            else if (func == "ivec3") {
                if (args.Count == 1) {
                    if (int.TryParse(args[0], out var v)) {
                        Int3 tmp = new Int3();
                        tmp.x = tmp.y = tmp.z = v;
                        val = tmp.ToString();
                        succ = true;
                    }
                }
                else if (args.Count == 3) {
                    if (int.TryParse(args[0], out var v1) && int.TryParse(args[1], out var v2) && int.TryParse(args[2], out var v3)) {
                        Int3 tmp = new Int3();
                        tmp.x = v1;
                        tmp.y = v2;
                        tmp.z = v3;
                        val = tmp.ToString();
                        succ = true;
                    }
                }
            }
            else if (func == "uvec3") {
                if (args.Count == 1) {
                    if (uint.TryParse(args[0], out var v)) {
                        Uint3 tmp = new Uint3();
                        tmp.x = tmp.y = tmp.z = v;
                        val = tmp.ToString();
                        succ = true;
                    }
                }
                else if (args.Count == 3) {
                    if (uint.TryParse(args[0], out var v1) && uint.TryParse(args[1], out var v2) && uint.TryParse(args[2], out var v3)) {
                        Uint3 tmp = new Uint3();
                        tmp.x = v1;
                        tmp.y = v2;
                        tmp.z = v3;
                        val = tmp.ToString();
                        succ = true;
                    }
                }
            }
            else if (func == "bcec4") {
                if (args.Count == 1) {
                    if (TryParseBool(args[0], out var v)) {
                        Bool4 tmp = new Bool4();
                        tmp.x = tmp.y = tmp.z = tmp.w = v;
                        val = tmp.ToString();
                        succ = true;
                    }
                }
                else if (args.Count == 2) {
                    if (TryParseBool(args[1], out var v2)) {
                        if (TryParseBool(args[0], out var v1)) {
                            Bool4 tmp = new Bool4();
                            tmp.x = v1;
                            tmp.y = v2;
                            tmp.z = false;
                            tmp.w = false;
                            val = tmp.ToString();
                            succ = true;
                        }
                        else if (Bool3.TryParse(args[0], out var f3)) {
                            Bool4 tmp = new Bool4();
                            tmp.x = f3.x;
                            tmp.y = f3.y;
                            tmp.z = f3.z;
                            tmp.w = v2;
                            val = tmp.ToString();
                            succ = true;
                        }
                    }
                }
                else if (args.Count == 3) {
                    if (TryParseBool(args[1], out var v2) && TryParseBool(args[2], out var v3)) {
                        if (TryParseBool(args[0], out var v1)) {
                            Bool4 tmp = new Bool4();
                            tmp.x = v1;
                            tmp.y = v2;
                            tmp.z = v3;
                            tmp.w = false;
                            val = tmp.ToString();
                            succ = true;
                        }
                        else if (Bool2.TryParse(args[0], out var f2)) {
                            Bool4 tmp = new Bool4();
                            tmp.x = f2.x;
                            tmp.y = f2.y;
                            tmp.z = v2;
                            tmp.w = v3;
                            val = tmp.ToString();
                            succ = true;
                        }
                    }
                }
                else if (args.Count == 4) {
                    if (TryParseBool(args[0], out var v1) && TryParseBool(args[1], out var v2) && TryParseBool(args[2], out var v3) && TryParseBool(args[3], out var v4)) {
                        Bool4 tmp = new Bool4();
                        tmp.x = v1;
                        tmp.y = v2;
                        tmp.z = v3;
                        tmp.w = v4;
                        val = tmp.ToString();
                        succ = true;
                    }
                }
            }
            else if (func == "vec4") {
                if (args.Count == 1) {
                    if (float.TryParse(args[0], out var v)) {
                        Float4 tmp = new Float4();
                        tmp.x = tmp.y = tmp.z = tmp.w = v;
                        val = tmp.ToString();
                        succ = true;
                    }
                }
                else if (args.Count == 2) {
                    if (float.TryParse(args[1], out var v2)) {
                        if (float.TryParse(args[0], out var v1)) {
                            Float4 tmp = new Float4();
                            tmp.x = v1;
                            tmp.y = v2;
                            tmp.z = 0;
                            tmp.w = 0;
                            val = tmp.ToString();
                            succ = true;
                        }
                        else if (Float3.TryParse(args[0], out var f3)) {
                            Float4 tmp = new Float4();
                            tmp.x = f3.x;
                            tmp.y = f3.y;
                            tmp.z = f3.z;
                            tmp.w = v2;
                            val = tmp.ToString();
                            succ = true;
                        }
                    }
                }
                else if (args.Count == 3) {
                    if (float.TryParse(args[1], out var v2) && float.TryParse(args[2], out var v3)) {
                        if (float.TryParse(args[0], out var v1)) {
                            Float4 tmp = new Float4();
                            tmp.x = v1;
                            tmp.y = v2;
                            tmp.z = v3;
                            tmp.w = 0;
                            val = tmp.ToString();
                            succ = true;
                        }
                        else if (Float2.TryParse(args[0], out var f2)) {
                            Float4 tmp = new Float4();
                            tmp.x = f2.x;
                            tmp.y = f2.y;
                            tmp.z = v2;
                            tmp.w = v3;
                            val = tmp.ToString();
                            succ = true;
                        }
                    }
                }
                else if (args.Count == 4) {
                    if (float.TryParse(args[0], out var v1) && float.TryParse(args[1], out var v2) && float.TryParse(args[2], out var v3) && float.TryParse(args[3], out var v4)) {
                        Float4 tmp = new Float4();
                        tmp.x = v1;
                        tmp.y = v2;
                        tmp.z = v3;
                        tmp.w = v4;
                        val = tmp.ToString();
                        succ = true;
                    }
                }
            }
            else if (func == "ivec4") {
                if (args.Count == 1) {
                    if (int.TryParse(args[0], out var v)) {
                        Int4 tmp = new Int4();
                        tmp.x = tmp.y = tmp.z = tmp.w = v;
                        val = tmp.ToString();
                        succ = true;
                    }
                }
                else if (args.Count == 2) {
                    if (int.TryParse(args[1], out var v2)) {
                        if (int.TryParse(args[0], out var v1)) {
                            Int4 tmp = new Int4();
                            tmp.x = v1;
                            tmp.y = v2;
                            tmp.z = 0;
                            tmp.w = 0;
                            val = tmp.ToString();
                            succ = true;
                        }
                        else if (Int3.TryParse(args[0], out var i3)) {
                            Int4 tmp = new Int4();
                            tmp.x = i3.x;
                            tmp.y = i3.y;
                            tmp.z = i3.z;
                            tmp.w = v2;
                            val = tmp.ToString();
                            succ = true;
                        }
                    }
                }
                else if (args.Count == 3) {
                    if (int.TryParse(args[1], out var v2) && int.TryParse(args[2], out var v3)) {
                        if (int.TryParse(args[0], out var v1)) {
                            Int4 tmp = new Int4();
                            tmp.x = v1;
                            tmp.y = v2;
                            tmp.z = v3;
                            tmp.w = 0;
                            val = tmp.ToString();
                            succ = true;
                        }
                        else if (Int2.TryParse(args[0], out var i2)) {
                            Int4 tmp = new Int4();
                            tmp.x = i2.x;
                            tmp.y = i2.y;
                            tmp.z = v2;
                            tmp.w = v3;
                            val = tmp.ToString();
                            succ = true;
                        }
                    }
                }
                else if (args.Count == 4) {
                    if (int.TryParse(args[0], out var v1) && int.TryParse(args[1], out var v2) && int.TryParse(args[2], out var v3) && int.TryParse(args[3], out var v4)) {
                        Int4 tmp = new Int4();
                        tmp.x = v1;
                        tmp.y = v2;
                        tmp.z = v3;
                        tmp.w = v4;
                        val = tmp.ToString();
                        succ = true;
                    }
                }
            }
            else if (func == "uvec4") {
                if (args.Count == 1) {
                    if (uint.TryParse(args[0], out var v)) {
                        Uint4 tmp = new Uint4();
                        tmp.x = tmp.y = tmp.z = tmp.w = v;
                        val = tmp.ToString();
                        succ = true;
                    }
                }
                else if (args.Count == 2) {
                    if (uint.TryParse(args[1], out var v2)) {
                        if (uint.TryParse(args[0], out var v1)) {
                            Uint4 tmp = new Uint4();
                            tmp.x = v1;
                            tmp.y = v2;
                            tmp.z = 0;
                            tmp.w = 0;
                            val = tmp.ToString();
                            succ = true;
                        }
                        else if (Uint3.TryParse(args[0], out var u3)) {
                            Uint4 tmp = new Uint4();
                            tmp.x = u3.x;
                            tmp.y = u3.y;
                            tmp.z = u3.z;
                            tmp.w = v2;
                            val = tmp.ToString();
                            succ = true;
                        }
                    }
                }
                else if (args.Count == 3) {
                    if (uint.TryParse(args[1], out var v2) && uint.TryParse(args[2], out var v3)) {
                        if (uint.TryParse(args[0], out var v1)) {
                            Uint4 tmp = new Uint4();
                            tmp.x = v1;
                            tmp.y = v2;
                            tmp.z = v3;
                            tmp.w = 0;
                            val = tmp.ToString();
                            succ = true;
                        }
                        else if (Uint2.TryParse(args[0], out var u2)) {
                            Uint4 tmp = new Uint4();
                            tmp.x = u2.x;
                            tmp.y = u2.y;
                            tmp.z = v2;
                            tmp.w = v3;
                            val = tmp.ToString();
                            succ = true;
                        }
                    }
                }
                else if (args.Count == 4) {
                    if (uint.TryParse(args[0], out var v1) && uint.TryParse(args[1], out var v2) && uint.TryParse(args[2], out var v3) && uint.TryParse(args[3], out var v4)) {
                        Uint4 tmp = new Uint4();
                        tmp.x = v1;
                        tmp.y = v2;
                        tmp.z = v3;
                        tmp.w = v4;
                        val = tmp.ToString();
                        succ = true;
                    }
                }
            }
            else if (func == "ftoi" || func == "floatBitsToInt") {
                if (float.TryParse(args[0], out var v)) {
                    unsafe {
                        int* p = (int*)&v;
                        val = (*p).ToString();
                    }
                    succ = true;
                }
            }
            else if (func == "ftou" || func == "floatBitsToUint") {
                if (float.TryParse(args[0], out var v)) {
                    unsafe {
                        uint* p = (uint*)&v;
                        val = (*p).ToString();
                    }
                    succ = true;
                }
            }
            else if (func == "itof" || func == "intBitsToFloat") {
                if (int.TryParse(args[0], out var v)) {
                    unsafe {
                        float* p = (float*)&v;
                        val = (*p).ToString();
                    }
                    succ = true;
                }
            }
            else if (func == "utof" || func == "uintBitsToFloat") {
                if (uint.TryParse(args[0], out var v)) {
                    unsafe {
                        float* p = (float*)&v;
                        val = (*p).ToString();
                    }
                    succ = true;
                }
            }
            else if (func == "isnan") {
                if (float.TryParse(args[0], out var v)) {
                    val = float.IsNaN(v).ToString();
                    succ = true;
                }
            }
            else if (func == "trunc") {
                if (float.TryParse(args[0], out var v)) {
                    val = MathF.Truncate(v).ToString();
                    succ = true;
                }
            }
            else if (func == "floor") {
                if (float.TryParse(args[0], out var v)) {
                    val = MathF.Floor(v).ToString();
                    succ = true;
                }
            }
            else if (func == "ceiling") {
                if (float.TryParse(args[0], out var v)) {
                    val = MathF.Ceiling(v).ToString();
                    succ = true;
                }
            }
            else if (func == "round") {
                if (float.TryParse(args[0], out var v)) {
                    val = MathF.Round(v).ToString();
                    succ = true;
                }
            }
            else if (func == "log2") {
                if (float.TryParse(args[0], out var v)) {
                    val = MathF.Log2(v).ToString();
                    succ = true;
                }
            }
            else if (func == "exp2") {
                if (float.TryParse(args[0], out var v)) {
                    val = MathF.Exp(v).ToString();
                    succ = true;
                }
            }
            else if (func == "sqrt") {
                if (float.TryParse(args[0], out var v)) {
                    val = MathF.Sqrt(v).ToString();
                    succ = true;
                }
            }
            else if (func == "inversesqrt") {
                if (float.TryParse(args[0], out var v)) {
                    float sv = MathF.Sqrt(v);
                    if (sv != 0) {
                        val = (1.0f / sv).ToString();
                        succ = true;
                    }
                }
            }
            else if (func == "fma") {
                if (double.TryParse(args[0], out var v1) && double.TryParse(args[1], out var v2) && double.TryParse(args[2], out var v3)) {
                    val = (v1 * v2 + v3).ToString();
                    succ = true;
                }
            }
            else if (func == "min") {
                if (TryParseNumeric(args[0], out var isFloat1, out var dval1, out var lval1) && TryParseNumeric(args[1], out var isFloat2, out var dval2, out var lval2)) {
                    if(isFloat1 || isFloat2) {
                        double v1 = isFloat1 ? dval1 : lval1;
                        double v2 = isFloat2 ? dval2 : lval2;
                        double v = v1 < v2 ? v1 : v2;
                        val = v.ToString();
                    }
                    else {
                        long v = lval1 < lval2 ? lval1 : lval2;
                        val = v.ToString();
                    }
                    succ = true;
                }
            }
            else if (func == "max") {
                if (TryParseNumeric(args[0], out var isFloat1, out var dval1, out var lval1) && TryParseNumeric(args[1], out var isFloat2, out var dval2, out var lval2)) {
                    if (isFloat1 || isFloat2) {
                        double v1 = isFloat1 ? dval1 : lval1;
                        double v2 = isFloat2 ? dval2 : lval2;
                        double v = v1 > v2 ? v1 : v2;
                        val = v.ToString();
                    }
                    else {
                        long v = lval1 > lval2 ? lval1 : lval2;
                        val = v.ToString();
                    }
                    succ = true;
                }
            }
            else {
                succ = Config.CalcFunc(func, args, out val);
            }
            return succ;
        }
        public static bool CalcCondExp(string cond, string opd1, string opd2, out string val)
        {
            bool succ = false;
            val = string.Empty;
            if(TryParseBool(cond, out var bval)) {
                if (TryParseNumeric(opd1, out var isFloat1, out var dval1, out var lval1) && TryParseNumeric(opd2, out var isFloat2, out var dval2, out var lval2)) {
                    if (bval) {
                        if (isFloat1)
                            val = dval1.ToString();
                        else
                            val = lval1.ToString();
                    }
                    else {
                        if (isFloat1)
                            val = dval2.ToString();
                        else
                            val = lval2.ToString();
                    }
                    succ = true;
                }
                else if(TryParseBool(opd1, out var bval1) && TryParseBool(opd2, out var bval2)) {
                    if (bval) {
                        val = bval1.ToString();
                    }
                    else {
                        val = bval2.ToString();
                    }
                    succ = true;
                }
            }
            return succ;
        }
        public static bool CalcBinary(string op, string opd1, string opd2, out string val)
        {
            bool succ = false;
            val = string.Empty;
            if (op == "+") {
                if (TryParseNumeric(opd1, out var isFloat1, out var dval1, out var lval1) && TryParseNumeric(opd2, out var isFloat2, out var dval2, out var lval2)) {
                    if(isFloat1 || isFloat2) {
                        double v1 = isFloat1 ? dval1 : lval1;
                        double v2 = isFloat2 ? dval2 : lval2;
                        val = (v1 + v2).ToString();
                    }
                    else {
                        val = (lval1 + lval2).ToString();
                    }
                    succ = true;
                }
            }
            else if (op == "-") {
                if (TryParseNumeric(opd1, out var isFloat1, out var dval1, out var lval1) && TryParseNumeric(opd2, out var isFloat2, out var dval2, out var lval2)) {
                    if (isFloat1 || isFloat2) {
                        double v1 = isFloat1 ? dval1 : lval1;
                        double v2 = isFloat2 ? dval2 : lval2;
                        val = (v1 - v2).ToString();
                    }
                    else {
                        val = (lval1 - lval2).ToString();
                    }
                    succ = true;
                }
            }
            else if (op == "*") {
                if (TryParseNumeric(opd1, out var isFloat1, out var dval1, out var lval1) && TryParseNumeric(opd2, out var isFloat2, out var dval2, out var lval2)) {
                    if (isFloat1 || isFloat2) {
                        double v1 = isFloat1 ? dval1 : lval1;
                        double v2 = isFloat2 ? dval2 : lval2;
                        val = (v1 * v2).ToString();
                    }
                    else {
                        val = (lval1 * lval2).ToString();
                    }
                    succ = true;
                }
            }
            else if (op == "/") {
                if (TryParseNumeric(opd1, out var isFloat1, out var dval1, out var lval1) && TryParseNumeric(opd2, out var isFloat2, out var dval2, out var lval2)) {
                    if (isFloat1 || isFloat2) {
                        double v1 = isFloat1 ? dval1 : lval1;
                        double v2 = isFloat2 ? dval2 : lval2;
                        if (v2 != 0) {
                            val = (v1 / v2).ToString();
                            succ = true;
                        }
                    }
                    else {
                        if (lval2 != 0) {
                            val = (lval1 / lval2).ToString();
                            succ = true;
                        }
                    }
                }
            }
            else if (op == "%") {
                if (TryParseNumeric(opd1, out var isFloat1, out var dval1, out var lval1) && TryParseNumeric(opd2, out var isFloat2, out var dval2, out var lval2)) {
                    if (isFloat1 || isFloat2) {
                        double v1 = isFloat1 ? dval1 : lval1;
                        double v2 = isFloat2 ? dval2 : lval2;
                        val = (v1 % v2).ToString();
                    }
                    else {
                        val = (lval1 % lval2).ToString();
                    }
                    succ = true;
                }
            }
            else if (op == "==") {
                if (TryParseNumeric(opd1, out var isFloat1, out var dval1, out var lval1) && TryParseNumeric(opd2, out var isFloat2, out var dval2, out var lval2)) {
                    if (isFloat1 || isFloat2) {
                        double v1 = isFloat1 ? dval1 : lval1;
                        double v2 = isFloat2 ? dval2 : lval2;
                        val = (v1 == v2).ToString();
                    }
                    else {
                        val = (lval1 == lval2).ToString();
                    }
                    succ = true;
                }
            }
            else if (op == "!=") {
                if (TryParseNumeric(opd1, out var isFloat1, out var dval1, out var lval1) && TryParseNumeric(opd2, out var isFloat2, out var dval2, out var lval2)) {
                    if (isFloat1 || isFloat2) {
                        double v1 = isFloat1 ? dval1 : lval1;
                        double v2 = isFloat2 ? dval2 : lval2;
                        val = (v1 != v2).ToString();
                    }
                    else {
                        val = (lval1 != lval2).ToString();
                    }
                    succ = true;
                }
            }
            else if (op == ">=") {
                if (TryParseNumeric(opd1, out var isFloat1, out var dval1, out var lval1) && TryParseNumeric(opd2, out var isFloat2, out var dval2, out var lval2)) {
                    if (isFloat1 || isFloat2) {
                        double v1 = isFloat1 ? dval1 : lval1;
                        double v2 = isFloat2 ? dval2 : lval2;
                        val = (v1 >= v2).ToString();
                    }
                    else {
                        val = (lval1 >= lval2).ToString();
                    }
                    succ = true;
                }
            }
            else if (op == "<=") {
                if (TryParseNumeric(opd1, out var isFloat1, out var dval1, out var lval1) && TryParseNumeric(opd2, out var isFloat2, out var dval2, out var lval2)) {
                    if (isFloat1 || isFloat2) {
                        double v1 = isFloat1 ? dval1 : lval1;
                        double v2 = isFloat2 ? dval2 : lval2;
                        val = (v1 <= v2).ToString();
                    }
                    else {
                        val = (lval1 <= lval2).ToString();
                    }
                    succ = true;
                }
            }
            else if (op == ">") {
                if (TryParseNumeric(opd1, out var isFloat1, out var dval1, out var lval1) && TryParseNumeric(opd2, out var isFloat2, out var dval2, out var lval2)) {
                    if (isFloat1 || isFloat2) {
                        double v1 = isFloat1 ? dval1 : lval1;
                        double v2 = isFloat2 ? dval2 : lval2;
                        val = (v1 > v2).ToString();
                    }
                    else {
                        val = (lval1 > lval2).ToString();
                    }
                    succ = true;
                }
            }
            else if (op == "<") {
                if (TryParseNumeric(opd1, out var isFloat1, out var dval1, out var lval1) && TryParseNumeric(opd2, out var isFloat2, out var dval2, out var lval2)) {
                    if (isFloat1 || isFloat2) {
                        double v1 = isFloat1 ? dval1 : lval1;
                        double v2 = isFloat2 ? dval2 : lval2;
                        val = (v1 < v2).ToString();
                    }
                    else {
                        val = (lval1 < lval2).ToString();
                    }
                    succ = true;
                }
            }
            else if (op == "&&") {
                if(TryParseBool(opd1, out var bval1) && TryParseBool(opd2, out var bval2)) {
                    val = (bval1 && bval2).ToString();
                    succ = true;
                }
            }
            else if (op == "||") {
                if (TryParseBool(opd1, out var bval1) && TryParseBool(opd2, out var bval2)) {
                    val = (bval1 || bval2) ? "true" : "false";
                    succ = true;
                }
            }
            else if (op == "&") {
                if (long.TryParse(opd1, out var lval1) && long.TryParse(opd2, out var lval2)) {
                    val = (lval1 & lval2).ToString();
                    succ = true;
                }
            }
            else if (op == "|") {
                if (long.TryParse(opd1, out var lval1) && long.TryParse(opd2, out var lval2)) {
                    val = (lval1 | lval2).ToString();
                    succ = true;
                }
            }
            return succ;
        }
        public static bool CalcUnary(string op, string opd, out string val)
        {
            bool succ = false;
            val = string.Empty;
            if (op == "-") {
                if(TryParseNumeric(opd, out var isFloat, out var dval, out var lval)) {
                    if (isFloat) {
                        val = (-dval).ToString();
                    }
                    else {
                        val = (-lval).ToString();
                    }
                    succ = true;
                }
            }
            else if (op == "~") {
                if (long.TryParse(opd, out var lval)) {
                    val = (~lval).ToString();
                    succ = true;
                }
            }
            else if (op == "!") {
                if(TryParseBool(opd, out var bval)) {
                    val = (!bval).ToString();
                    succ = true;
                }
            }
            else if (op == "+") {
                val = opd;
                succ = true;
            }
            return succ;
        }

        public static string ReStringNumeric(string val)
        {
            if (val.Length >= 2) {
                char c = val[val.Length - 1];
                if (c == 'u' || c == 'U') {
                    val = val.Substring(0, val.Length - 1);
                }
                else if (c == 'f' || c == 'F') {
                    val = val.Substring(0, val.Length - 1);
                    c = val[val.Length - 1];
                    if (c == 'l' || c == 'L') {
                        val = val.Substring(0, val.Length - 1);
                    }
                }
            }
            if (val.IndexOfAny(s_FloatExponent) > 0) {
                if (double.TryParse(val, NumberStyles.Float, NumberFormatInfo.CurrentInfo, out var v)) {
                    val = v.ToString();
                }
            }
            else if (val.Length > 2 && val[0] == '0' && val[1] == 'x') {
                if (ulong.TryParse(val, NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out var v)) {
                    val = v.ToString();
                }
            }
            else if (val.Length > 1 && val[0] == '0') {
                ulong v = Convert.ToUInt64(val, 8);
                val = v.ToString();
            }
            return val;
        }
        public static bool TryParseNumeric(string val, out bool isFloat, out double doubleVal, out long longVal)
        {
            isFloat = false;
            doubleVal = 0;
            longVal = 0;
            if (val.IndexOfAny(s_FloatExponent) >= 0 && double.TryParse(val, NumberStyles.Float, NumberFormatInfo.CurrentInfo, out doubleVal)) {
                isFloat = true;
                return true;
            }
            else if (val.Length > 2 && val[0] == '0' && val[1] == 'x') {
                if (long.TryParse(val, NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out longVal)) {
                    return true;
                }
            }
            else if (val.Length > 1 && val[0] == '0') {
                try {
                    longVal = Convert.ToInt64(val, 8);
                    return true;
                }
                catch {
                }
            }
            else if (long.TryParse(val, out longVal)) {
                return true;
            }
            return false;
        }
        public static bool TryParseBool(string v, out bool val)
        {
            if (bool.TryParse(v, out val)) {
                return true;
            }
            else if (int.TryParse(v, out var ival)) {
                val = ival != 0;
                return true;
            }
            else if (v == "true") {
                val = true;
                return true;
            }
            else if (v == "false") {
                val = false;
                return true;
            }
            return false;
        }

        public static char[] s_FloatExponent = new char[] { 'e', 'E', '.' };
    }
}
