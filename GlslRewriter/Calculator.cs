using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GlslRewriter
{
    /// <summary>
    /// Although we have introduced the BatchCommand DSL script in GlslRewriter, this script may have a different syntax and API signature compared to GLSL.
    /// Therefore, we cannot directly use the DSL script interpreter to execute GLSL computations.
    /// However, the DSL script can be used in Config to provide alternative implementations of GLSL functions, because Config is parsed and interpreted based on DSL.
    /// We can choose execution rules that are consistent with the DSL script.
    /// It might be worth considering using DslExpression.CalculatorValue to represent values on the computation graph.
    /// This class is similar to a union in C++, capable of holding data types required by GLSL.
    /// </summary>
    public static class Calculator
    {
        public static bool CalcFunc(string func, IList<DslExpression.CalculatorValue> args, string resultType, Dictionary<int, int> argTypeConversion, out DslExpression.CalculatorValue val, out bool supported)
        {
            bool succ = false;
            val = DslExpression.CalculatorValue.NullObject;
            supported = true;
            if (func == "bool") {
                if (IsNumeric(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(v.GetBool());
                    succ = true;
                }
            }
            else if (func == "float") {
                if (IsNumeric(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(v.GetFloat());
                    succ = true;
                }
            }
            else if (func == "double") {
                if (IsNumeric(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(v.GetDouble());
                    succ = true;
                }
            }
            else if (func == "int") {
                if (IsNumeric(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(v.GetInt());
                    succ = true;
                }
            }
            else if (func == "uint") {
                if (IsNumeric(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(v.GetUInt());
                    succ = true;
                }
            }
            else if (func == "bvec2") {
                if (args.Count == 1) {
                    if (TryGetBool(args[0], out var v)) {
                        Bool2 tmp = new Bool2();
                        tmp.x = tmp.y = v;
                        val = DslExpression.CalculatorValue.FromObject(tmp);
                        succ = true;
                    }
                }
                else if (args.Count == 2) {
                    if (TryGetBool(args[0], out var v1) && TryGetBool(args[1], out var v2)) {
                        Bool2 tmp = new Bool2();
                        tmp.x = v1;
                        tmp.y = v2;
                        val = DslExpression.CalculatorValue.FromObject(tmp);
                        succ = true;
                    }
                }
            }
            else if (func == "vec2") {
                if (args.Count == 1) {
                    if (TryGetFloat(args[0], out var v)) {
                        Float2 tmp = new Float2();
                        tmp.x = tmp.y = v;
                        val = DslExpression.CalculatorValue.FromObject(tmp);
                        succ = true;
                    }
                    else if (Int2.TryParse(args[0], out var i2)) {
                        Float2 tmp = new Float2();
                        tmp.x = (float)i2.x;
                        tmp.y = (float)i2.y;
                        val = DslExpression.CalculatorValue.FromObject(tmp);
                        succ = true;
                    }
                    else if (Uint2.TryParse(args[0], out var u2)) {
                        Float2 tmp = new Float2();
                        tmp.x = (float)u2.x;
                        tmp.y = (float)u2.y;
                        val = DslExpression.CalculatorValue.FromObject(tmp);
                        succ = true;
                    }
                }
                else if (args.Count == 2) {
                    if (TryGetFloat(args[0], out var v1) && TryGetFloat(args[1], out var v2)) {
                        Float2 tmp = new Float2();
                        tmp.x = v1;
                        tmp.y = v2;
                        val = DslExpression.CalculatorValue.FromObject(tmp);
                        succ = true;
                    }
                }
            }
            else if (func == "ivec2") {
                if (args.Count == 1) {
                    if (TryGetInt(args[0], out var v)) {
                        Int2 tmp = new Int2();
                        tmp.x = tmp.y = v;
                        val = DslExpression.CalculatorValue.FromObject(tmp);
                        succ = true;
                    }
                    else if (Uint2.TryParse(args[0], out var u2)) {
                        Int2 tmp = new Int2();
                        tmp.x = (int)u2.x;
                        tmp.y = (int)u2.y;
                        val = DslExpression.CalculatorValue.FromObject(tmp);
                        succ = true;
                    }
                    else if (Float2.TryParse(args[0], out var f2)) {
                        Int2 tmp = new Int2();
                        tmp.x = (int)f2.x;
                        tmp.y = (int)f2.y;
                        val = DslExpression.CalculatorValue.FromObject(tmp);
                        succ = true;
                    }
                }
                else if (args.Count == 2) {
                    if (TryGetInt(args[0], out var v1) && TryGetInt(args[1], out var v2)) {
                        Int2 tmp = new Int2();
                        tmp.x = v1;
                        tmp.y = v2;
                        val = DslExpression.CalculatorValue.FromObject(tmp);
                        succ = true;
                    }
                }
            }
            else if (func == "uvec2") {
                if (args.Count == 1) {
                    if (TryGetUInt(args[0], out var v)) {
                        Uint2 tmp = new Uint2();
                        tmp.x = tmp.y = v;
                        val = DslExpression.CalculatorValue.FromObject(tmp);
                        succ = true;
                    }
                    else if (Int2.TryParse(args[0], out var i2)) {
                        Uint2 tmp = new Uint2();
                        tmp.x = (uint)i2.x;
                        tmp.y = (uint)i2.y;
                        val = DslExpression.CalculatorValue.FromObject(tmp);
                        succ = true;
                    }
                    else if (Float2.TryParse(args[0], out var f2)) {
                        Uint2 tmp = new Uint2();
                        tmp.x = (uint)f2.x;
                        tmp.y = (uint)f2.y;
                        val = DslExpression.CalculatorValue.FromObject(tmp);
                        succ = true;
                    }
                }
                else if (args.Count == 2) {
                    if (TryGetUInt(args[0], out var v1) && TryGetUInt(args[1], out var v2)) {
                        Uint2 tmp = new Uint2();
                        tmp.x = v1;
                        tmp.y = v2;
                        val = DslExpression.CalculatorValue.FromObject(tmp);
                        succ = true;
                    }
                }
            }
            else if (func == "bvec3") {
                if (args.Count == 1) {
                    if (TryGetBool(args[0], out var v)) {
                        Bool3 tmp = new Bool3();
                        tmp.x = tmp.y = tmp.z = v;
                        val = DslExpression.CalculatorValue.FromObject(tmp);
                        succ = true;
                    }
                }
                else if (args.Count == 3) {
                    if (TryGetBool(args[0], out var v1) && TryGetBool(args[1], out var v2) && TryGetBool(args[2], out var v3)) {
                        Bool3 tmp = new Bool3();
                        tmp.x = v1;
                        tmp.y = v2;
                        tmp.z = v3;
                        val = DslExpression.CalculatorValue.FromObject(tmp);
                        succ = true;
                    }
                }
            }
            else if (func == "vec3") {
                if (args.Count == 1) {
                    if (TryGetFloat(args[0], out var v)) {
                        Float3 tmp = new Float3();
                        tmp.x = tmp.y = tmp.z = v;
                        val = DslExpression.CalculatorValue.FromObject(tmp);
                        succ = true;
                    }
                }
                else if (args.Count == 3) {
                    if (TryGetFloat(args[0], out var v1) && TryGetFloat(args[1], out var v2) && TryGetFloat(args[2], out var v3)) {
                        Float3 tmp = new Float3();
                        tmp.x = v1;
                        tmp.y = v2;
                        tmp.z = v3;
                        val = DslExpression.CalculatorValue.FromObject(tmp);
                        succ = true;
                    }
                }
            }
            else if (func == "ivec3") {
                if (args.Count == 1) {
                    if (TryGetInt(args[0], out var v)) {
                        Int3 tmp = new Int3();
                        tmp.x = tmp.y = tmp.z = v;
                        val = DslExpression.CalculatorValue.FromObject(tmp);
                        succ = true;
                    }
                }
                else if (args.Count == 3) {
                    if (TryGetInt(args[0], out var v1) && TryGetInt(args[1], out var v2) && TryGetInt(args[2], out var v3)) {
                        Int3 tmp = new Int3();
                        tmp.x = v1;
                        tmp.y = v2;
                        tmp.z = v3;
                        val = DslExpression.CalculatorValue.FromObject(tmp);
                        succ = true;
                    }
                }
            }
            else if (func == "uvec3") {
                if (args.Count == 1) {
                    if (TryGetUInt(args[0], out var v)) {
                        Uint3 tmp = new Uint3();
                        tmp.x = tmp.y = tmp.z = v;
                        val = DslExpression.CalculatorValue.FromObject(tmp);
                        succ = true;
                    }
                }
                else if (args.Count == 3) {
                    if (TryGetUInt(args[0], out var v1) && TryGetUInt(args[1], out var v2) && TryGetUInt(args[2], out var v3)) {
                        Uint3 tmp = new Uint3();
                        tmp.x = v1;
                        tmp.y = v2;
                        tmp.z = v3;
                        val = DslExpression.CalculatorValue.FromObject(tmp);
                        succ = true;
                    }
                }
            }
            else if (func == "bcec4") {
                if (args.Count == 1) {
                    if (TryGetBool(args[0], out var v)) {
                        Bool4 tmp = new Bool4();
                        tmp.x = tmp.y = tmp.z = tmp.w = v;
                        val = DslExpression.CalculatorValue.FromObject(tmp);
                        succ = true;
                    }
                }
                else if (args.Count == 2) {
                    if (TryGetBool(args[1], out var v2)) {
                        if (TryGetBool(args[0], out var v1)) {
                            Bool4 tmp = new Bool4();
                            tmp.x = v1;
                            tmp.y = v2;
                            tmp.z = false;
                            tmp.w = false;
                            val = DslExpression.CalculatorValue.FromObject(tmp);
                            succ = true;
                        }
                        else if (Bool3.TryParse(args[0], out var f3)) {
                            Bool4 tmp = new Bool4();
                            tmp.x = f3.x;
                            tmp.y = f3.y;
                            tmp.z = f3.z;
                            tmp.w = v2;
                            val = DslExpression.CalculatorValue.FromObject(tmp);
                            succ = true;
                        }
                    }
                }
                else if (args.Count == 3) {
                    if (TryGetBool(args[1], out var v2) && TryGetBool(args[2], out var v3)) {
                        if (TryGetBool(args[0], out var v1)) {
                            Bool4 tmp = new Bool4();
                            tmp.x = v1;
                            tmp.y = v2;
                            tmp.z = v3;
                            tmp.w = false;
                            val = DslExpression.CalculatorValue.FromObject(tmp);
                            succ = true;
                        }
                        else if (Bool2.TryParse(args[0], out var f2)) {
                            Bool4 tmp = new Bool4();
                            tmp.x = f2.x;
                            tmp.y = f2.y;
                            tmp.z = v2;
                            tmp.w = v3;
                            val = DslExpression.CalculatorValue.FromObject(tmp);
                            succ = true;
                        }
                    }
                }
                else if (args.Count == 4) {
                    if (TryGetBool(args[0], out var v1) && TryGetBool(args[1], out var v2) && TryGetBool(args[2], out var v3) && TryGetBool(args[3], out var v4)) {
                        Bool4 tmp = new Bool4();
                        tmp.x = v1;
                        tmp.y = v2;
                        tmp.z = v3;
                        tmp.w = v4;
                        val = DslExpression.CalculatorValue.FromObject(tmp);
                        succ = true;
                    }
                }
            }
            else if (func == "vec4") {
                if (args.Count == 1) {
                    if (TryGetFloat(args[0], out var v)) {
                        Float4 tmp = new Float4();
                        tmp.x = tmp.y = tmp.z = tmp.w = v;
                        val = DslExpression.CalculatorValue.FromObject(tmp);
                        succ = true;
                    }
                }
                else if (args.Count == 2) {
                    if (TryGetFloat(args[1], out var v2)) {
                        if (TryGetFloat(args[0], out var v1)) {
                            Float4 tmp = new Float4();
                            tmp.x = v1;
                            tmp.y = v2;
                            tmp.z = 0;
                            tmp.w = 0;
                            val = DslExpression.CalculatorValue.FromObject(tmp);
                            succ = true;
                        }
                        else if (Float3.TryParse(args[0], out var f3)) {
                            Float4 tmp = new Float4();
                            tmp.x = f3.x;
                            tmp.y = f3.y;
                            tmp.z = f3.z;
                            tmp.w = v2;
                            val = DslExpression.CalculatorValue.FromObject(tmp);
                            succ = true;
                        }
                    }
                }
                else if (args.Count == 3) {
                    if (TryGetFloat(args[1], out var v2) && TryGetFloat(args[2], out var v3)) {
                        if (TryGetFloat(args[0], out var v1)) {
                            Float4 tmp = new Float4();
                            tmp.x = v1;
                            tmp.y = v2;
                            tmp.z = v3;
                            tmp.w = 0;
                            val = DslExpression.CalculatorValue.FromObject(tmp);
                            succ = true;
                        }
                        else if (Float2.TryParse(args[0], out var f2)) {
                            Float4 tmp = new Float4();
                            tmp.x = f2.x;
                            tmp.y = f2.y;
                            tmp.z = v2;
                            tmp.w = v3;
                            val = DslExpression.CalculatorValue.FromObject(tmp);
                            succ = true;
                        }
                    }
                }
                else if (args.Count == 4) {
                    if (TryGetFloat(args[0], out var v1) && TryGetFloat(args[1], out var v2) && TryGetFloat(args[2], out var v3) && TryGetFloat(args[3], out var v4)) {
                        Float4 tmp = new Float4();
                        tmp.x = v1;
                        tmp.y = v2;
                        tmp.z = v3;
                        tmp.w = v4;
                        val = DslExpression.CalculatorValue.FromObject(tmp);
                        succ = true;
                    }
                }
            }
            else if (func == "ivec4") {
                if (args.Count == 1) {
                    if (TryGetInt(args[0], out var v)) {
                        Int4 tmp = new Int4();
                        tmp.x = tmp.y = tmp.z = tmp.w = v;
                        val = DslExpression.CalculatorValue.FromObject(tmp);
                        succ = true;
                    }
                }
                else if (args.Count == 2) {
                    if (TryGetInt(args[1], out var v2)) {
                        if (TryGetInt(args[0], out var v1)) {
                            Int4 tmp = new Int4();
                            tmp.x = v1;
                            tmp.y = v2;
                            tmp.z = 0;
                            tmp.w = 0;
                            val = DslExpression.CalculatorValue.FromObject(tmp);
                            succ = true;
                        }
                        else if (Int3.TryParse(args[0], out var i3)) {
                            Int4 tmp = new Int4();
                            tmp.x = i3.x;
                            tmp.y = i3.y;
                            tmp.z = i3.z;
                            tmp.w = v2;
                            val = DslExpression.CalculatorValue.FromObject(tmp);
                            succ = true;
                        }
                    }
                }
                else if (args.Count == 3) {
                    if (TryGetInt(args[1], out var v2) && TryGetInt(args[2], out var v3)) {
                        if (TryGetInt(args[0], out var v1)) {
                            Int4 tmp = new Int4();
                            tmp.x = v1;
                            tmp.y = v2;
                            tmp.z = v3;
                            tmp.w = 0;
                            val = DslExpression.CalculatorValue.FromObject(tmp);
                            succ = true;
                        }
                        else if (Int2.TryParse(args[0], out var i2)) {
                            Int4 tmp = new Int4();
                            tmp.x = i2.x;
                            tmp.y = i2.y;
                            tmp.z = v2;
                            tmp.w = v3;
                            val = DslExpression.CalculatorValue.FromObject(tmp);
                            succ = true;
                        }
                    }
                }
                else if (args.Count == 4) {
                    if (TryGetInt(args[0], out var v1) && TryGetInt(args[1], out var v2) && TryGetInt(args[2], out var v3) && TryGetInt(args[3], out var v4)) {
                        Int4 tmp = new Int4();
                        tmp.x = v1;
                        tmp.y = v2;
                        tmp.z = v3;
                        tmp.w = v4;
                        val = DslExpression.CalculatorValue.FromObject(tmp);
                        succ = true;
                    }
                }
            }
            else if (func == "uvec4") {
                if (args.Count == 1) {
                    if (TryGetUInt(args[0], out var v)) {
                        Uint4 tmp = new Uint4();
                        tmp.x = tmp.y = tmp.z = tmp.w = v;
                        val = DslExpression.CalculatorValue.FromObject(tmp);
                        succ = true;
                    }
                }
                else if (args.Count == 2) {
                    if (TryGetUInt(args[1], out var v2)) {
                        if (TryGetUInt(args[0], out var v1)) {
                            Uint4 tmp = new Uint4();
                            tmp.x = v1;
                            tmp.y = v2;
                            tmp.z = 0;
                            tmp.w = 0;
                            val = DslExpression.CalculatorValue.FromObject(tmp);
                            succ = true;
                        }
                        else if (Uint3.TryParse(args[0], out var u3)) {
                            Uint4 tmp = new Uint4();
                            tmp.x = u3.x;
                            tmp.y = u3.y;
                            tmp.z = u3.z;
                            tmp.w = v2;
                            val = DslExpression.CalculatorValue.FromObject(tmp);
                            succ = true;
                        }
                    }
                }
                else if (args.Count == 3) {
                    if (TryGetUInt(args[1], out var v2) && TryGetUInt(args[2], out var v3)) {
                        if (TryGetUInt(args[0], out var v1)) {
                            Uint4 tmp = new Uint4();
                            tmp.x = v1;
                            tmp.y = v2;
                            tmp.z = v3;
                            tmp.w = 0;
                            val = DslExpression.CalculatorValue.FromObject(tmp);
                            succ = true;
                        }
                        else if (Uint2.TryParse(args[0], out var u2)) {
                            Uint4 tmp = new Uint4();
                            tmp.x = u2.x;
                            tmp.y = u2.y;
                            tmp.z = v2;
                            tmp.w = v3;
                            val = DslExpression.CalculatorValue.FromObject(tmp);
                            succ = true;
                        }
                    }
                }
                else if (args.Count == 4) {
                    if (TryGetUInt(args[0], out var v1) && TryGetUInt(args[1], out var v2) && TryGetUInt(args[2], out var v3) && TryGetUInt(args[3], out var v4)) {
                        Uint4 tmp = new Uint4();
                        tmp.x = v1;
                        tmp.y = v2;
                        tmp.z = v3;
                        tmp.w = v4;
                        val = DslExpression.CalculatorValue.FromObject(tmp);
                        succ = true;
                    }
                }
            }
            else if (func == "ftoi" || func == "floatBitsToInt") {
                if (args[0].IsSignedInteger) {
                    val = args[0];
                    succ = true;

                    argTypeConversion[0] = ComputeGraphCalcNode.c_action_remove_func;
                }
                else if (TryGetFloat(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(ftoi(v));
                    succ = true;
                }
            }
            else if (func == "ftou" || func == "floatBitsToUint") {
                if (args[0].IsUnsignedInteger) {
                    val = args[0];
                    succ = true;

                    argTypeConversion[0] = ComputeGraphCalcNode.c_action_remove_func;
                }
                else if (TryGetFloat(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(ftou(v));
                    succ = true;
                }
            }
            else if (func == "itof" || func == "intBitsToFloat") {
                if (args[0].IsNumber) {
                    val = args[0];
                    succ = true;

                    argTypeConversion[0] = ComputeGraphCalcNode.c_action_remove_func;
                }
                else if (TryGetInt(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(itof(v));
                    succ = true;
                }
            }
            else if (func == "utof" || func == "uintBitsToFloat") {
                if (args[0].IsNumber) {
                    val = args[0];
                    succ = true;

                    argTypeConversion[0] = ComputeGraphCalcNode.c_action_remove_func;
                }
                else if (TryGetUInt(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(utof(v));
                    succ = true;
                }
            }
            else if (func == "bitfieldExtract") {
                if (args[0].IsUnsignedInteger && TryGetUInt(args[0], out var uval) && TryGetInt(args[1], out var uoffset) && TryGetInt(args[2], out var ubits)) {
                    if (uoffset >= 0 && ubits >= 0 && uoffset + ubits <= sizeof(uint) * 8) {
                        if (ubits == 0) {
                            val = DslExpression.CalculatorValue.From((uint)0);
                        }
                        else if (uoffset == 0 && ubits == sizeof(uint) * 8) {
                            val = uval;
                        }
                        else {
                            //unsigned
                            uint mask = (1u << ubits) - 1u;
                            uint v = (uval >> uoffset) & mask;
                            val = DslExpression.CalculatorValue.From(v);
                        }
                        succ = true;
                    }
                }
                else if (args[0].IsSignedInteger && TryGetInt(args[0], out var ival) && TryGetInt(args[1], out var ioffset) && TryGetInt(args[2], out var ibits)) {
                    if (ioffset >= 0 && ibits >= 0 && ioffset + ibits <= sizeof(uint) * 8) {
                        if (ibits == 0) {
                            val = DslExpression.CalculatorValue.From((uint)0);
                        }
                        else if (ioffset == 0 && ibits == sizeof(uint) * 8) {
                            val = ival;
                        }
                        else {
                            //signed
                            int shifted = ival >> ioffset;
                            int signBit = shifted & (int)(1u << (ibits - 1));
                            int mask = (int)((1u << ibits) - 1u);

                            int v = -signBit | (shifted & mask);
                            val = DslExpression.CalculatorValue.From(v);
                        }
                        succ = true;
                    }
                }
            }
            else if(func== "bitfieldInsert") {
                if (TryGetUInt(args[0], out var uval) && TryGetUInt(args[1], out var uinsert) && TryGetInt(args[2], out var uoffset) && TryGetInt(args[3], out var ubits)) {
                    if (uoffset >= 0 && ubits>=0 && uoffset + ubits <= sizeof(uint) * 8) {
                        if (ubits == 0) {
                            val = DslExpression.CalculatorValue.From(uval);
                        }
                        else if (uoffset == 0 && ubits == sizeof(uint) * 8) {
                            val = uinsert;
                        }
                        else {
                            uint maskBits = (1u << ubits) - 1u;
                            uint mask = maskBits << uoffset;
                            uint src = uinsert << uoffset;
                            uint v = (src & mask) | (uval & ~mask);
                            val = DslExpression.CalculatorValue.From(v);
                        }
                        succ = true;
                    }
                }
            }
            else if (func == "isinf") {
                if (TryGetFloat(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(float.IsInfinity(v));
                    succ = true;
                }
            }
            else if (func == "isnan") {
                if (TryGetFloat(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(float.IsNaN(v));
                    succ = true;
                }
            }
            else if (func == "trunc") {
                if (TryGetFloat(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(MathF.Truncate(v));
                    succ = true;
                }
            }
            else if (func == "floor") {
                if (TryGetFloat(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(MathF.Floor(v));
                    succ = true;
                }
            }
            else if (func == "ceiling") {
                if (TryGetFloat(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(MathF.Ceiling(v));
                    succ = true;
                }
            }
            else if (func == "round") {
                if (TryGetFloat(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(MathF.Round(v));
                    succ = true;
                }
            }
            else if(func=="roundEven") {
                if (TryGetFloat(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(MathF.Round(v, MidpointRounding.ToEven));
                    succ = true;
                }
            }
            else if (func == "abs") {
                if (args[0].IsUnsignedInteger) {
                    uint v = args[0].GetUInt();
                    val = DslExpression.CalculatorValue.From(v);
                    succ = true;
                }
                else if (args[0].IsSignedInteger) {
                    int v = args[0].GetInt();
                    if (v < 0)
                        v = -v;
                    val = DslExpression.CalculatorValue.From(v);
                    succ = true;
                }
                else if (TryGetFloat(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(MathF.Abs(v));
                    succ = true;
                }
            }
            else if (func == "log") {
                if (TryGetFloat(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(MathF.Log(v));
                    succ = true;
                }
            }
            else if (func == "exp") {
                if (TryGetFloat(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(MathF.Exp(v));
                    succ = true;
                }
            }
            else if (func == "log2") {
                if (TryGetFloat(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(MathF.Log2(v));
                    succ = true;
                }
            }
            else if (func == "exp2") {
                if (TryGetFloat(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(MathF.Pow(2, v));
                    succ = true;
                }
            }
            else if (func == "sqrt") {
                if (TryGetFloat(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(MathF.Sqrt(v));
                    succ = true;
                }
            }
            else if (func == "inversesqrt") {
                if (TryGetFloat(args[0], out var v)) {
                    float sv = MathF.Sqrt(v);
                    if (sv != 0) {
                        val = DslExpression.CalculatorValue.From(1.0f / sv);
                        succ = true;
                    }
                }
            }
            else if (func == "sin") {
                if (TryGetFloat(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(MathF.Sin(v));
                    succ = true;
                }
            }
            else if (func == "cos") {
                if (TryGetFloat(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(MathF.Cos(v));
                    succ = true;
                }
            }
            else if (func == "tan") {
                if (TryGetFloat(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(MathF.Tan(v));
                    succ = true;
                }
            }
            else if (func == "sinh") {
                if (TryGetFloat(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(MathF.Sinh(v));
                    succ = true;
                }
            }
            else if (func == "cosh") {
                if (TryGetFloat(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(MathF.Cosh(v));
                    succ = true;
                }
            }
            else if (func == "tanh") {
                if (TryGetFloat(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(MathF.Tanh(v));
                    succ = true;
                }
            }
            else if (func == "asin") {
                if (TryGetFloat(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(MathF.Asin(v));
                    succ = true;
                }
            }
            else if (func == "acos") {
                if (TryGetFloat(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(MathF.Acos(v));
                    succ = true;
                }
            }
            else if (func == "atan") {
                if (TryGetFloat(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(MathF.Atan(v));
                    succ = true;
                }
            }
            else if (func == "asinh") {
                if (TryGetFloat(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(MathF.Asinh(v));
                    succ = true;
                }
            }
            else if (func == "acosh") {
                if (TryGetFloat(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(MathF.Acosh(v));
                    succ = true;
                }
            }
            else if (func == "atanh") {
                if (TryGetFloat(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(MathF.Atanh(v));
                    succ = true;
                }
            }
            else if (func == "degrees") {
                if (TryGetFloat(args[0], out var v)) {
                    val = DslExpression.CalculatorValue.From(v * 180.0f / MathF.PI);
                    succ = true;
                }
            }
            else if (func == "radians") {
                if (TryGetFloat(args[0], out var v)) {
                    val = FloatToString(v * MathF.PI / 180.0f);
                    succ = true;
                }
            }
            else if (func == "pow") {
                if (TryGetFloat(args[0], out var v1) && TryGetFloat(args[1], out var v2)) {
                    val = DslExpression.CalculatorValue.From(MathF.Pow(v1, v2));
                    succ = true;
                }
            }
            else if (func == "fma") {
                if (TryGetDouble(args[0], out var v1) && TryGetDouble(args[1], out var v2) && TryGetDouble(args[2], out var v3)) {
                    val = DslExpression.CalculatorValue.From((float)(v1 * v2 + v3));
                    succ = true;
                }
            }
            else if (func == "min") {
                if (IsNumeric(args[0], out var val1) && IsNumeric(args[1], out var val2)) {
                    if(val1.IsNumber || val2.IsNumber) {
                        double v1 = val1.GetDouble();
                        double v2 = val2.GetDouble();
                        double v = v1 < v2 ? v1 : v2;
                        val = DslExpression.CalculatorValue.From((float)v);
                    }
                    else {
                        long v1 = val1.GetLong();
                        long v2 = val2.GetLong();
                        long v = v1 < v2 ? v1 : v2;
                        val = DslExpression.CalculatorValue.From((int)v);
                    }
                    succ = true;
                }
            }
            else if (func == "max") {
                if (IsNumeric(args[0], out var val1) && IsNumeric(args[1], out var val2)) {
                    if (val1.IsNumber || val2.IsNumber) {
                        double v1 = val1.GetDouble();
                        double v2 = val2.GetDouble();
                        double v = v1 > v2 ? v1 : v2;
                        val = DslExpression.CalculatorValue.From((float)v);
                    }
                    else {
                        long v1 = val1.GetLong();
                        long v2 = val2.GetLong();
                        long v = v1 > v2 ? v1 : v2;
                        val = DslExpression.CalculatorValue.From((int)v);
                    }
                    succ = true;
                }
            }
            else if (func == "clamp") {
                if (IsNumeric(args[0], out var val1) && IsNumeric(args[1], out var val2) && IsNumeric(args[2], out var val3)) {
                    if (val1.IsNumber || val2.IsNumber || val3.IsNumber) {
                        double v1 = val1.GetDouble();
                        double v2 = val2.GetDouble();
                        double v3 = val3.GetDouble();
                        double v = v1 < v2 ? v2 : (v1 > v3 ? v3 : v1);
                        val = DslExpression.CalculatorValue.From((float)v);
                    }
                    else {
                        long v1 = val1.GetLong();
                        long v2 = val2.GetLong();
                        long v3 = val3.GetLong();
                        long v = v1 < v2 ? v2 : (v1 > v3 ? v3 : v1);
                        val = DslExpression.CalculatorValue.From((int)v);
                    }
                    succ = true;
                }
            }
            else if(func=="atomicAdd" || func== "atomicAnd" || func == "atomicOr" || func == "atomicXor"
                || func == "atomicMin" || func == "atomicMax" || func == "atomicExchange" || func == "atomicCompSwap") {
                //The atomic series functions require separate processing (the first argument needs to pass in both the memory address and index information),
                //so for now, let's leave them here to allow the translation to proceed normally.
                val = args[0];
                succ = true;
            }
            else {
                supported = false;
            }
            return succ;
        }
        public static bool CalcCondExp(DslExpression.CalculatorValue cond, DslExpression.CalculatorValue opd1, DslExpression.CalculatorValue opd2, string resultType, Dictionary<int, int> argTypeConversion, out DslExpression.CalculatorValue val, out bool supported)
        {
            bool succ = false;
            val = DslExpression.CalculatorValue.NullObject;
            supported = true;
            if(TryGetBool(cond, out var bval)) {
                if (IsNumeric(opd1, out var val1) && IsNumeric(opd2, out var val2)) {
                    if (bval) {
                        val = val1;
                    }
                    else {
                        val = val2;
                    }
                    succ = true;
                }
            }
            return succ;
        }
        public static bool CalcBinary(string op, DslExpression.CalculatorValue opd1, DslExpression.CalculatorValue opd2, string resultType, Dictionary<int, int> argTypeConversion, out DslExpression.CalculatorValue val, out bool supported)
        {
            bool succ = false;
            val = DslExpression.CalculatorValue.NullObject;
            supported = true;
            if (op == "+") {
                if (IsNumeric(opd1, out var val1) && IsNumeric(opd2, out var val2)) {
                    if ((resultType == "uint" || resultType == "int") && (val1.IsNumber || val2.IsNumber)) {
                        if (val1.IsNumber) {
                            val1 = ftou(val1.GetFloat());
                            argTypeConversion[0] = ComputeGraphCalcNode.c_action_add_ftou;
                        }
                        if (val2.IsNumber) {
                            val2 = ftou(val2.GetFloat());
                            argTypeConversion[1] = ComputeGraphCalcNode.c_action_add_ftou;
                        }
                    }
                    if (val1.IsNumber || val2.IsNumber) {
                        float v1 = val1.GetFloat();
                        float v2 = val2.GetFloat();
                        val = DslExpression.CalculatorValue.From(v1 + v2);
                    }
                    else if (val1.IsSignedInteger || val2.IsSignedInteger) {
                        int v1 = val1.GetInt();
                        int v2 = val2.GetInt();
                        val = DslExpression.CalculatorValue.From(v1 + v2);
                    }
                    else {
                        uint v1 = val1.GetUInt();
                        uint v2 = val2.GetUInt();
                        val = DslExpression.CalculatorValue.From(v1 + v2);
                    }
                    succ = true;
                }
            }
            else if (op == "-") {
                if (IsNumeric(opd1, out var val1) && IsNumeric(opd2, out var val2)) {
                    if ((resultType == "uint" || resultType == "int") && (val1.IsNumber || val2.IsNumber)) {
                        if (val1.IsNumber) {
                            val1 = ftou(val1.GetFloat());
                            argTypeConversion[0] = ComputeGraphCalcNode.c_action_add_ftou;
                        }
                        if (val2.IsNumber) {
                            val2 = ftou(val2.GetFloat());
                            argTypeConversion[1] = ComputeGraphCalcNode.c_action_add_ftou;
                        }
                    }
                    if (val1.IsNumber || val2.IsNumber) {
                        float v1 = val1.GetFloat();
                        float v2 = val2.GetFloat();
                        val = DslExpression.CalculatorValue.From(v1 - v2);
                    }
                    else if (val1.IsSignedInteger || val2.IsSignedInteger) {
                        int v1 = val1.GetInt();
                        int v2 = val2.GetInt();
                        val = DslExpression.CalculatorValue.From(v1 - v2);
                    }
                    else {
                        uint v1 = val1.GetUInt();
                        uint v2 = val2.GetUInt();
                        val = DslExpression.CalculatorValue.From(v1 - v2);
                    }
                    succ = true;
                }
            }
            else if (op == "*") {
                if (IsNumeric(opd1, out var val1) && IsNumeric(opd2, out var val2)) {
                    if ((resultType == "uint" || resultType == "int") && (val1.IsNumber || val2.IsNumber)) {
                        if (val1.IsNumber) {
                            val1 = ftou(val1.GetFloat());
                            argTypeConversion[0] = ComputeGraphCalcNode.c_action_add_ftou;
                        }
                        if (val2.IsNumber) {
                            val2 = ftou(val2.GetFloat());
                            argTypeConversion[1] = ComputeGraphCalcNode.c_action_add_ftou;
                        }
                    }
                    if (val1.IsNumber || val2.IsNumber) {
                        float v1 = val1.GetFloat();
                        float v2 = val2.GetFloat();
                        val = DslExpression.CalculatorValue.From(v1 * v2);
                    }
                    else if (val1.IsSignedInteger || val2.IsSignedInteger) {
                        int v1 = val1.GetInt();
                        int v2 = val2.GetInt();
                        val = DslExpression.CalculatorValue.From(v1 * v2);
                    }
                    else {
                        uint v1 = val1.GetUInt();
                        uint v2 = val2.GetUInt();
                        val = DslExpression.CalculatorValue.From(v1 * v2);
                    }
                    succ = true;
                }
            }
            else if (op == "/") {
                if (IsNumeric(opd1, out var val1) && IsNumeric(opd2, out var val2)) {
                    if ((resultType == "uint" || resultType == "int") && (val1.IsNumber || val2.IsNumber)) {
                        if (val1.IsNumber) {
                            val1 = ftou(val1.GetFloat());
                            argTypeConversion[0] = ComputeGraphCalcNode.c_action_add_ftou;
                        }
                        if (val2.IsNumber) {
                            val2 = ftou(val2.GetFloat());
                            argTypeConversion[1] = ComputeGraphCalcNode.c_action_add_ftou;
                        }
                    }
                    if (val1.IsNumber || val2.IsNumber) {
                        float v1 = val1.GetFloat();
                        float v2 = val2.GetFloat();
                        val = DslExpression.CalculatorValue.From(v1 / v2);
                    }
                    else if (val1.IsSignedInteger || val2.IsSignedInteger) {
                        int v1 = val1.GetInt();
                        int v2 = val2.GetInt();
                        val = DslExpression.CalculatorValue.From(v1 / v2);
                    }
                    else {
                        uint v1 = val1.GetUInt();
                        uint v2 = val2.GetUInt();
                        val = DslExpression.CalculatorValue.From(v1 / v2);
                    }
                    succ = true;
                }
            }
            else if (op == "%") {
                if (IsNumeric(opd1, out var val1) && IsNumeric(opd2, out var val2)) {
                    if ((resultType == "uint" || resultType == "int") && (val1.IsNumber || val2.IsNumber)) {
                        if (val1.IsNumber) {
                            val1 = ftou(val1.GetFloat());
                            argTypeConversion[0] = ComputeGraphCalcNode.c_action_add_ftou;
                        }
                        if (val2.IsNumber) {
                            val2 = ftou(val2.GetFloat());
                            argTypeConversion[1] = ComputeGraphCalcNode.c_action_add_ftou;
                        }
                    }
                    if (val1.IsSignedInteger || val2.IsSignedInteger) {
                        int v1 = val1.GetInt();
                        int v2 = val2.GetInt();
                        val = DslExpression.CalculatorValue.From(v1 % v2);
                    }
                    else {
                        uint v1 = val1.GetUInt();
                        uint v2 = val2.GetUInt();
                        val = DslExpression.CalculatorValue.From(v1 % v2);
                    }
                    succ = true;
                }
            }
            else if (op == "==") {
                if (IsNumeric(opd1, out var val1) && IsNumeric(opd2, out var val2)) {
                    if (val1.IsNumber || val2.IsNumber) {
                        float v1 = val1.GetFloat();
                        float v2 = val2.GetFloat();
                        val = DslExpression.CalculatorValue.From(v1 == v2);
                    }
                    else if (val1.IsSignedInteger || val2.IsSignedInteger) {
                        int v1 = val1.GetInt();
                        int v2 = val2.GetInt();
                        val = DslExpression.CalculatorValue.From(v1 == v2);
                    }
                    else {
                        uint v1 = val1.GetUInt();
                        uint v2 = val2.GetUInt();
                        val = DslExpression.CalculatorValue.From(v1 == v2);
                    }
                    succ = true;
                }
            }
            else if (op == "!=") {
                if (IsNumeric(opd1, out var val1) && IsNumeric(opd2, out var val2)) {
                    if (val1.IsNumber || val2.IsNumber) {
                        float v1 = val1.GetFloat();
                        float v2 = val2.GetFloat();
                        val = DslExpression.CalculatorValue.From(v1 != v2);
                    }
                    else if (val1.IsSignedInteger || val2.IsSignedInteger) {
                        int v1 = val1.GetInt();
                        int v2 = val2.GetInt();
                        val = DslExpression.CalculatorValue.From(v1 != v2);
                    }
                    else {
                        uint v1 = val1.GetUInt();
                        uint v2 = val2.GetUInt();
                        val = DslExpression.CalculatorValue.From(v1 != v2);
                    }
                    succ = true;
                }
            }
            else if (op == ">=") {
                if (IsNumeric(opd1, out var val1) && IsNumeric(opd2, out var val2)) {
                    if (val1.IsNumber || val2.IsNumber) {
                        float v1 = val1.GetFloat();
                        float v2 = val2.GetFloat();
                        val = DslExpression.CalculatorValue.From(v1 >= v2);
                    }
                    else if (val1.IsSignedInteger || val2.IsSignedInteger) {
                        int v1 = val1.GetInt();
                        int v2 = val2.GetInt();
                        val = DslExpression.CalculatorValue.From(v1 >= v2);
                    }
                    else {
                        uint v1 = val1.GetUInt();
                        uint v2 = val2.GetUInt();
                        val = DslExpression.CalculatorValue.From(v1 >= v2);
                    }
                    succ = true;
                }
            }
            else if (op == "<=") {
                if (IsNumeric(opd1, out var val1) && IsNumeric(opd2, out var val2)) {
                    if (val1.IsNumber || val2.IsNumber) {
                        float v1 = val1.GetFloat();
                        float v2 = val2.GetFloat();
                        val = DslExpression.CalculatorValue.From(v1 <= v2);
                    }
                    else if (val1.IsSignedInteger || val2.IsSignedInteger) {
                        int v1 = val1.GetInt();
                        int v2 = val2.GetInt();
                        val = DslExpression.CalculatorValue.From(v1 <= v2);
                    }
                    else {
                        uint v1 = val1.GetUInt();
                        uint v2 = val2.GetUInt();
                        val = DslExpression.CalculatorValue.From(v1 <= v2);
                    }
                    succ = true;
                }
            }
            else if (op == ">") {
                if (IsNumeric(opd1, out var val1) && IsNumeric(opd2, out var val2)) {
                    if (val1.IsNumber || val2.IsNumber) {
                        float v1 = val1.GetFloat();
                        float v2 = val2.GetFloat();
                        val = DslExpression.CalculatorValue.From(v1 > v2);
                    }
                    else if (val1.IsSignedInteger || val2.IsSignedInteger) {
                        int v1 = val1.GetInt();
                        int v2 = val2.GetInt();
                        val = DslExpression.CalculatorValue.From(v1 > v2);
                    }
                    else {
                        uint v1 = val1.GetUInt();
                        uint v2 = val2.GetUInt();
                        val = DslExpression.CalculatorValue.From(v1 > v2);
                    }
                    succ = true;
                }
            }
            else if (op == "<") {
                if (IsNumeric(opd1, out var val1) && IsNumeric(opd2, out var val2)) {
                    if (val1.IsNumber || val2.IsNumber) {
                        float v1 = val1.GetFloat();
                        float v2 = val2.GetFloat();
                        val = DslExpression.CalculatorValue.From(v1 < v2);
                    }
                    else if (val1.IsSignedInteger || val2.IsSignedInteger) {
                        int v1 = val1.GetInt();
                        int v2 = val2.GetInt();
                        val = DslExpression.CalculatorValue.From(v1 < v2);
                    }
                    else {
                        uint v1 = val1.GetUInt();
                        uint v2 = val2.GetUInt();
                        val = DslExpression.CalculatorValue.From(v1 < v2);
                    }
                    succ = true;
                }
            }
            else if (op == "&&") {
                if (TryGetBool(opd1, out var val1) && TryGetBool(opd2, out var val2)) {
                    val = DslExpression.CalculatorValue.From(val1 && val2);
                    succ = true;
                }
            }
            else if (op == "||") {
                if (TryGetBool(opd1, out var val1) && TryGetBool(opd2, out var val2)) {
                    val = DslExpression.CalculatorValue.From(val1 || val2);
                    succ = true;
                }
            }
            else if (op == "^^") {
                if (TryGetBool(opd1, out var val1) && TryGetBool(opd2, out var val2)) {
                    val = DslExpression.CalculatorValue.From(val1 != val2);
                    succ = true;
                }
            }
            else if (op == "&") {
                if (IsNumeric(opd1, out var val1) && IsNumeric(opd2, out var val2)) {
                    if ((resultType == "uint" || resultType == "int") && (val1.IsNumber || val2.IsNumber)) {
                        if (val1.IsNumber) {
                            val1 = ftou(val1.GetFloat());
                            argTypeConversion[0] = ComputeGraphCalcNode.c_action_add_ftou;
                        }
                        if (val2.IsNumber) {
                            val2 = ftou(val2.GetFloat());
                            argTypeConversion[1] = ComputeGraphCalcNode.c_action_add_ftou;
                        }
                    }
                    if(val1.IsSignedInteger || val2.IsSignedInteger) {
                        int v1 = val1.GetInt();
                        int v2 = val2.GetInt();
                        val = DslExpression.CalculatorValue.From(v1 & v2);
                        succ = true;
                    }
                    else {
                        uint v1 = val1.GetUInt();
                        uint v2 = val2.GetUInt();
                        val = DslExpression.CalculatorValue.From(v1 & v2);
                        succ = true;
                    }
                }
            }
            else if (op == "|") {
                if (IsNumeric(opd1, out var val1) && IsNumeric(opd2, out var val2)) {
                    if ((resultType == "uint" || resultType == "int") && (val1.IsNumber || val2.IsNumber)) {
                        if (val1.IsNumber) {
                            val1 = ftou(val1.GetFloat());
                            argTypeConversion[0] = ComputeGraphCalcNode.c_action_add_ftou;
                        }
                        if (val2.IsNumber) {
                            val2 = ftou(val2.GetFloat());
                            argTypeConversion[1] = ComputeGraphCalcNode.c_action_add_ftou;
                        }
                    }
                    if(val1.IsSignedInteger || val2.IsSignedInteger) {
                        int v1 = val1.GetInt();
                        int v2 = val2.GetInt();
                        val = DslExpression.CalculatorValue.From(v1 | v2);
                        succ = true;
                    }
                    else {
                        uint v1 = val1.GetUInt();
                        uint v2 = val2.GetUInt();
                        val = DslExpression.CalculatorValue.From(v1 | v2);
                        succ = true;
                    }
                }
            }
            else if (op == "^") {
                if (IsNumeric(opd1, out var val1) && IsNumeric(opd2, out var val2)) {
                    if ((resultType == "uint" || resultType == "int") && (val1.IsNumber || val2.IsNumber)) {
                        if (val1.IsNumber) {
                            val1 = ftou(val1.GetFloat());
                            argTypeConversion[0] = ComputeGraphCalcNode.c_action_add_ftou;
                        }
                        if (val2.IsNumber) {
                            val2 = ftou(val2.GetFloat());
                            argTypeConversion[1] = ComputeGraphCalcNode.c_action_add_ftou;
                        }
                    }
                    if (val1.IsSignedInteger || val2.IsSignedInteger) {
                        int v1 = val1.GetInt();
                        int v2 = val2.GetInt();
                        val = DslExpression.CalculatorValue.From(v1 ^ v2);
                        succ = true;
                    }
                    else {
                        uint v1 = val1.GetUInt();
                        uint v2 = val2.GetUInt();
                        val = DslExpression.CalculatorValue.From(v1 ^ v2);
                        succ = true;
                    }
                }
            }
            else if (op == "<<") {
                if (IsNumeric(opd1, out var val1) && IsNumeric(opd2, out var val2)) {
                    if ((resultType == "uint" || resultType == "int") && (val1.IsNumber || val2.IsNumber)) {
                        if (val1.IsNumber) {
                            val1 = ftou(val1.GetFloat());
                            argTypeConversion[0] = ComputeGraphCalcNode.c_action_add_ftou;
                        }
                        if (val2.IsNumber) {
                            val2 = ftoi(val2.GetFloat());
                            argTypeConversion[1] = ComputeGraphCalcNode.c_action_add_ftoi;
                        }
                    }
                    if (val1.IsSignedInteger || val2.IsSignedInteger) {
                        int v1 = val1.GetInt();
                        int v2 = val2.GetInt();
                        val = DslExpression.CalculatorValue.From(v1 << v2);
                        succ = true;
                    }
                    else {
                        uint v1 = val1.GetUInt();
                        int v2 = val2.GetInt();
                        val = DslExpression.CalculatorValue.From(v1 << v2);
                        succ = true;
                    }
                }
            }
            else if (op == ">>") {
                if (IsNumeric(opd1, out var val1) && IsNumeric(opd2, out var val2)) {
                    if ((resultType == "uint" || resultType == "int") && (val1.IsNumber || val2.IsNumber)) {
                        if (val1.IsNumber) {
                            val1 = ftou(val1.GetFloat());
                            argTypeConversion[0] = ComputeGraphCalcNode.c_action_add_ftou;
                        }
                        if (val2.IsNumber) {
                            val2 = ftoi(val2.GetFloat());
                            argTypeConversion[1] = ComputeGraphCalcNode.c_action_add_ftoi;
                        }
                    }
                    if (val1.IsSignedInteger || val2.IsSignedInteger) {
                        int v1 = val1.GetInt();
                        int v2 = val2.GetInt();
                        val = DslExpression.CalculatorValue.From(v1 >> v2);
                        succ = true;
                    }
                    else {
                        uint v1 = val1.GetUInt();
                        int v2 = val2.GetInt();
                        val = DslExpression.CalculatorValue.From(v1 >> v2);
                        succ = true;
                    }
                }
            }
            else {
                supported = false;
            }
            if (succ) {
                if (resultType == "uint" && val.IsSignedInteger) {
                    val = val.GetUInt();
                }
                else if (resultType == "int" && val.IsUnsignedInteger) {
                    val = val.GetInt();
                }
            }
            return succ;
        }
        public static bool CalcUnary(string op, DslExpression.CalculatorValue opd, string resultType, Dictionary<int, int> argTypeConversion, out DslExpression.CalculatorValue val, out bool supported)
        {
            bool succ = false;
            val = DslExpression.CalculatorValue.NullObject;
            supported = true;
            if (op == "-") {
                if (opd.IsNumber) {
                    val.Set(-opd.GetFloat());
                    succ = true;
                }
                else if (opd.IsInteger) {
                    val.Set(-opd.GetInt());
                    succ = true;
                }
            }
            else if (op == "~") {
                if (opd.IsInteger) {
                    val.Set(~opd.GetInt());
                    succ = true;
                }
            }
            else if (op == "!") {
                if (opd.IsBoolean) {
                    val.Set(!opd.GetBool());
                    succ = true;
                }
            }
            else if (op == "+") {
                val = opd;
                succ = true;
            }
            else {
                supported = false;
            }
            if (succ) {
                if (resultType == "uint" && val.IsSignedInteger) {
                    val = val.GetUInt();
                }
                else if (resultType == "int" && val.IsUnsignedInteger) {
                    val = val.GetInt();
                }
            }
            return succ;
        }
        public static bool CalcMember(DslExpression.CalculatorValue objVal, DslExpression.CalculatorValue m, string resultType, Dictionary<int, int> argTypeConversion, out DslExpression.CalculatorValue oval, out bool supported)
        {
            bool succ = false;
            oval = DslExpression.CalculatorValue.NullObject;
            supported = true;
            if (objVal.IsNullObject) {
                // succ=false && supported=true
            }
            else if (Float4.TryParse(objVal, out var f4val)) {
                oval = f4val.GetMember(m);
                succ = true;
            }
            else if (Int4.TryParse(objVal, out var i4val)) {
                oval = i4val.GetMember(m);
                succ = true;
            }
            else if (Uint4.TryParse(objVal, out var u4val)) {
                oval = u4val.GetMember(m);
                succ = true;
            }
            else if (Bool4.TryParse(objVal, out var b4val)) {
                oval = b4val.GetMember(m);
                succ = true;
            }
            else if (Float3.TryParse(objVal, out var f3val)) {
                oval = f3val.GetMember(m);
                succ = true;
            }
            else if (Int3.TryParse(objVal, out var i3val)) {
                oval = i3val.GetMember(m);
                succ = true;
            }
            else if (Uint3.TryParse(objVal, out var u3val)) {
                oval = u3val.GetMember(m);
                succ = true;
            }
            else if (Bool3.TryParse(objVal, out var b3val)) {
                oval = b3val.GetMember(m);
                succ = true;
            }
            else if (Float2.TryParse(objVal, out var f2val)) {
                oval = f2val.GetMember(m);
                succ = true;
            }
            else
                if (Int2.TryParse(objVal, out var i2val)) {
                oval = i2val.GetMember(m);
                succ = true;
            }
            else if (Uint2.TryParse(objVal, out var u2val)) {
                oval = u2val.GetMember(m);
                succ = true;
            }
            else if (Bool2.TryParse(objVal, out var b2val)) {
                oval = b2val.GetMember(m);
                succ = true;
            }
            else if (TryGetInt(m, out int ix)) {
                if (ix < 0) {
                }
                else if (TryGetList(objVal, out IList<Bool4Val>? b4list)) {
                    if (null != b4list && ix < b4list.Count) {
                        oval = DslExpression.CalculatorValue.FromObject(b4list[ix].Value);
                        succ = b4list[ix].IsValid;
                    }
                }
                else if (TryGetList(objVal, out IList<Float4Val>? f4list)) {
                    if (null != f4list && ix < f4list.Count) {
                        oval = DslExpression.CalculatorValue.FromObject(f4list[ix].Value);
                        succ = f4list[ix].IsValid;
                    }
                }
                else if (TryGetList(objVal, out IList<Int4Val>? i4list)) {
                    if (null != i4list && ix < i4list.Count) {
                        oval = DslExpression.CalculatorValue.FromObject(i4list[ix].Value);
                        succ = i4list[ix].IsValid;
                    }
                }
                else if (TryGetList(objVal, out IList<Uint4Val>? u4list)) {
                    if (null != u4list && ix < u4list.Count) {
                        oval = DslExpression.CalculatorValue.FromObject(u4list[ix].Value);
                        succ = u4list[ix].IsValid;
                    }
                }
                else if (TryGetList(objVal, out IList<BoolVal>? blist)) {
                    if (null != blist && ix < blist.Count) {
                        oval = DslExpression.CalculatorValue.From(blist[ix].Value);
                        succ = blist[ix].IsValid;
                    }
                }
                else if (TryGetList(objVal, out IList<FloatVal>? flist)) {
                    if (null != flist && ix < flist.Count) {
                        oval = DslExpression.CalculatorValue.From(flist[ix].Value);
                        succ = flist[ix].IsValid;
                    }
                }
                else if (TryGetList(objVal, out IList<IntVal>? ilist)) {
                    if (null != ilist && ix < ilist.Count) {
                        oval = DslExpression.CalculatorValue.From(ilist[ix].Value);
                        succ = ilist[ix].IsValid;
                    }
                }
                else if (TryGetList(objVal, out IList<UintVal>? ulist)) {
                    if (null != ulist && ix < ulist.Count) {
                        oval = DslExpression.CalculatorValue.From(ulist[ix].Value);
                        succ = ulist[ix].IsValid;
                    }
                }
                else {
                    supported = false;
                }
            }
            else {
                supported = false;
            }
            if (succ) {
                if (resultType == "uint" && oval.IsSignedInteger) {
                    oval = oval.GetUInt();
                }
                else if (resultType == "int" && oval.IsUnsignedInteger) {
                    oval = oval.GetInt();
                }
            }
            return succ;
        }

        public static string FloatToString(float v)
        {
            if (v > -1e28 && v < 1e28)
                return v.ToString(s_FloatFormat);
            else
                return string.Format("{0}", v);
        }
        public static string DoubleToString(double v)
        {
            if (v > -1e28 && v < 1e28)
                return v.ToString(s_DoubleFormat);
            else
                return string.Format("{0}", v);
        }
        public static unsafe int ftoi(float v)
        {
            int* p = (int*)&v;
            var val = *p;
            return val;
        }
        public static unsafe float itof(int v)
        {
            float* p = (float*)&v;
            var val = *p;
            return val;
        }
        public static unsafe uint ftou(float v)
        {
            uint* p = (uint*)&v;
            var val = *p;
            return val;
        }
        public static unsafe float utof(uint v)
        {
            float* p = (float*)&v;
            var val = *p;
            return val;
        }
        public static bool TryParseNumeric(string str, out DslExpression.CalculatorValue val)
        {
            string type = string.Empty;
            return TryParseNumeric(str, ref type, out val);
        }
        public static bool TryParseNumeric(string str, ref string type, out DslExpression.CalculatorValue val)
        {
            bool ret = false;
            val = DslExpression.CalculatorValue.NullObject;
            if (str.Length > 2 && str[0] == '0' && str[1] == 'x') {
                char c = str[str.Length - 1];
                if (c == 'u' || c == 'U') {
                    str = str.Substring(0, str.Length - 1);
                }
                if (ulong.TryParse(str.Substring(2), NumberStyles.HexNumber, NumberFormatInfo.CurrentInfo, out var v)) {
                    str = v.ToString();
                }
                type = "uint";
            }
            else if (str.Length >= 2) {
                char c = str[str.Length - 1];
                if (c == 'u' || c == 'U') {
                    str = str.Substring(0, str.Length - 1);
                    type = "uint";
                }
                else if (c == 'f' || c == 'F') {
                    str = str.Substring(0, str.Length - 1);
                    c = str[str.Length - 1];
                    if (c == 'l' || c == 'L') {
                        str = str.Substring(0, str.Length - 1);
                    }
                    type = "float";
                }
            }
            if (type == "float" || str.IndexOfAny(s_FloatExponent) > 0) {
                if (double.TryParse(str, NumberStyles.Float, NumberFormatInfo.CurrentInfo, out var v)) {
                    val.Set((float)v);
                    type = "float";
                    ret = true;
                }
            }
            else if (str.Length > 1 && str[0] == '0') {
                ulong v = Convert.ToUInt64(str, 8);
                val.Set((uint)v);
                type = "uint";
                ret = true;
            }
            if (long.TryParse(str, out var lv)) {
                if (lv > int.MaxValue || type == "uint") {
                    val.Set((uint)lv);
                    type = "uint";
                }
                else {
                    val.Set((int)lv);
                    type = "int";
                }
                ret = true;
            }
            else if(TryParseBool(str, out var bv)) {
                val.Set(bv);
                type = "bool";
                ret = true;
            }
            return ret;
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

        public static string GetValueType(DslExpression.CalculatorValue val)
        {
            string valType;
            if (val.IsNumber)
                valType = "float";
            else if (val.IsSignedInteger)
                valType = "int";
            else if (val.IsUnsignedInteger)
                valType = "uint";
            else if (val.IsBoolean)
                valType = "bool";
            else if (val.IsNullObject)
                valType = "null";
            else if (val.IsObject)
                valType = val.ObjectVal.GetType().Name;
            else
                valType = "string";
            return valType;
        }
        public static bool IsNumeric(DslExpression.CalculatorValue val, out DslExpression.CalculatorValue oval)
        {
            bool ret = val.IsNumber || val.IsInteger || val.IsChar || val.IsBoolean;
            if (ret) {
                oval = val;
            }
            else {
                oval = DslExpression.CalculatorValue.NullObject;
            }
            return ret;
        }
        public static bool TryGetBool(DslExpression.CalculatorValue v, out bool val)
        {
            val = false;
            if(v.IsBoolean || v.IsInteger) {
                val = v.GetBool();
                return true;
            }
            return false;
        }
        public static bool TryGetFloat(DslExpression.CalculatorValue v, out float val)
        {
            val = 0;
            if (v.IsNumber || v.IsInteger) {
                val = v.GetFloat();
                return true;
            }
            return false;
        }
        public static bool TryGetDouble(DslExpression.CalculatorValue v, out double val)
        {
            val = 0;
            if (v.IsNumber || v.IsInteger) {
                val = v.GetDouble();
                return true;
            }
            return false;
        }
        public static bool TryGetInt(DslExpression.CalculatorValue v, out int val)
        {
            val = 0;
            if (v.IsInteger || v.IsChar) {
                val = v.GetInt();
                return true;
            }
            return false;
        }
        public static bool TryGetLong(DslExpression.CalculatorValue v, out long val)
        {
            val = 0;
            if (v.IsInteger || v.IsChar) {
                val = v.GetLong();
                return true;
            }
            return false;
        }
        public static bool TryGetUInt(DslExpression.CalculatorValue v, out uint val)
        {
            val = 0;
            if (v.IsInteger || v.IsChar) {
                val = v.GetUInt();
                return true;
            }
            return false;
        }
        public static bool TryGetULong(DslExpression.CalculatorValue v, out ulong val)
        {
            val = 0;
            if (v.IsInteger || v.IsChar) {
                val = v.GetULong();
                return true;
            }
            return false;
        }
        public static bool TryGetString(DslExpression.CalculatorValue v, out string val)
        {
            val = string.Empty;
            if (v.IsString) {
                val = v.GetString();
                return true;
            }
            return false;
        }
        public static bool TryGetList<T>(DslExpression.CalculatorValue v, out IList<T>? list)
        {
            list = null;
            if (v.IsObject) {
                list = v.ObjectVal as IList<T>;
                return null != list;
            }
            return false;
        }

        public static char[] s_FloatExponent = new char[] { 'e', 'E', '.' };
        private static string s_FloatFormat = "###########################0.00#####";
        private static string s_DoubleFormat = "###########################0.00##############";
    }
}
