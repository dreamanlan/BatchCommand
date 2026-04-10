using System;
using AgentPlugin.Abstractions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Security.Cryptography;
using DotnetStoryScript;
using DotnetStoryScript.DslExpression;
using ScriptableFramework;

namespace CefDotnetApp.AgentCore.ScriptApi
{
    using TupleValue1 = Tuple<BoxedValue>;
    using TupleValue2 = Tuple<BoxedValue, BoxedValue>;
    using TupleValue3 = Tuple<BoxedValue, BoxedValue, BoxedValue>;
    using TupleValue4 = Tuple<BoxedValue, BoxedValue, BoxedValue, BoxedValue>;
    using TupleValue5 = Tuple<BoxedValue, BoxedValue, BoxedValue, BoxedValue, BoxedValue>;
    using TupleValue6 = Tuple<BoxedValue, BoxedValue, BoxedValue, BoxedValue, BoxedValue, BoxedValue>;
    using TupleValue7 = Tuple<BoxedValue, BoxedValue, BoxedValue, BoxedValue, BoxedValue, BoxedValue, BoxedValue>;
    using TupleValue8 = Tuple<BoxedValue, BoxedValue, BoxedValue, BoxedValue, BoxedValue, BoxedValue, BoxedValue, Tuple<BoxedValue>>;

    public class FrameworkApiAlias
    {
        public static void RegisterApis()
        {
            //AgentFrameworkService.Instance.DslEngine!.Register("clone", "clone(v)", new ExpressionFactoryHelper<CloneExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("clone_array", "clone_array(v)", false, new ExpressionFactoryHelper<CloneExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("clone_list", "clone_list(v)", false, new ExpressionFactoryHelper<CloneExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("clone_hashtable", "clone_hashtable(v)", false, new ExpressionFactoryHelper<CloneExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("clone_queue", "clone_queue(v)", false, new ExpressionFactoryHelper<CloneExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("clone_stack", "clone_stack(v)", false, new ExpressionFactoryHelper<CloneExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("clone_tuple", "clone_tuple(v)", false, new ExpressionFactoryHelper<CloneExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("copy", "copy(v)", new ExpressionFactoryHelper<CloneExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("copy_array", "copy_array(v)", new ExpressionFactoryHelper<CloneExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("copy_list", "copy_list(v)", new ExpressionFactoryHelper<CloneExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("copy_hashtable", "copy_hashtable(v)", new ExpressionFactoryHelper<CloneExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("copy_queue", "copy_queue(v)", new ExpressionFactoryHelper<CloneExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("copy_stack", "copy_stack(v)", new ExpressionFactoryHelper<CloneExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("copy_tuple", "copy_tuple(v)", new ExpressionFactoryHelper<CloneExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("add", "add(a,b)", new ExpressionFactoryHelper<AddExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("sub", "sub(a,b)", new ExpressionFactoryHelper<SubExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("mul", "mul(a,b)", new ExpressionFactoryHelper<MulExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("div", "div(a,b)", new ExpressionFactoryHelper<DivExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("mod", "mod(a,b)", new ExpressionFactoryHelper<ModExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("bit_and", "bitand(a,b)", new ExpressionFactoryHelper<BitAndExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("bit_or", "bitor(a,b)", new ExpressionFactoryHelper<BitOrExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("bit_xor", "bitxor(a,b)", new ExpressionFactoryHelper<BitXorExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("bit_not", "bitnot(a)", new ExpressionFactoryHelper<BitNotExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("left_shift", "left_shift(a,b)", new ExpressionFactoryHelper<LShiftExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("right_shift", "right_shift(a,b)", new ExpressionFactoryHelper<RShiftExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("max", "max(v1,v2)", new ExpressionFactoryHelper<MaxExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("min", "min(v1,v2)", new ExpressionFactoryHelper<MinExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("abs", "abs(v)", new ExpressionFactoryHelper<AbsExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("sin", "sin(v)", new ExpressionFactoryHelper<SinExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("cos", "cos(v)", new ExpressionFactoryHelper<CosExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("tan", "tan(v)", new ExpressionFactoryHelper<TanExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("asin", "asin(v)", new ExpressionFactoryHelper<AsinExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("acos", "acos(v)", new ExpressionFactoryHelper<AcosExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("atan", "atan(v)", new ExpressionFactoryHelper<AtanExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("atan2", "atan2(v1,v2)", new ExpressionFactoryHelper<Atan2Exp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("sinh", "sinh(v)", new ExpressionFactoryHelper<SinhExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("cosh", "cosh(v)", new ExpressionFactoryHelper<CoshExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("tanh", "tanh(v)", new ExpressionFactoryHelper<TanhExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("rnd_int", "rnd_int(min,max)", new ExpressionFactoryHelper<RndIntExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("rnd_float", "rnd_float(min,max)", new ExpressionFactoryHelper<RndFloatExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("pow", "pow(v1,v2)", new ExpressionFactoryHelper<PowExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("sqrt", "sqrt(v)", new ExpressionFactoryHelper<SqrtExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("exp", "exp(v)", new ExpressionFactoryHelper<ExpExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("exp2", "exp2(v)", new ExpressionFactoryHelper<Exp2Exp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("log", "log(v)", new ExpressionFactoryHelper<LogExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("log2", "log2(v)", new ExpressionFactoryHelper<Log2Exp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("log10", "log10(v)", new ExpressionFactoryHelper<Log10Exp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("floor", "floor(v)", new ExpressionFactoryHelper<FloorExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("ceiling", "ceiling(v)", new ExpressionFactoryHelper<CeilingExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("round", "round(v)", new ExpressionFactoryHelper<RoundExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("floor_to_int", "floor_to_int(v)", new ExpressionFactoryHelper<FloorToIntExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("ceiling_to_int", "ceiling_to_int(v)", new ExpressionFactoryHelper<CeilingToIntExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("round_to_int", "round_to_int(v)", new ExpressionFactoryHelper<RoundToIntExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("bool", "bool(v)", new ExpressionFactoryHelper<BoolExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("sbyte", "sbyte(v)", new ExpressionFactoryHelper<SByteExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("byte", "byte(v)", new ExpressionFactoryHelper<ByteExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("char", "char(v)", new ExpressionFactoryHelper<CharExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("short", "short(v)", new ExpressionFactoryHelper<ShortExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("ushort", "ushort(v)", new ExpressionFactoryHelper<UShortExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("int", "int(v)", new ExpressionFactoryHelper<IntExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("uint", "uint(v)", new ExpressionFactoryHelper<UIntExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("long", "long(v)", new ExpressionFactoryHelper<LongExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("ulong", "ulong(v)", new ExpressionFactoryHelper<ULongExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("float", "float(v)", new ExpressionFactoryHelper<FloatExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("double", "double(v)", new ExpressionFactoryHelper<DoubleExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("decimal", "decimal(v)", new ExpressionFactoryHelper<DecimalExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("f_to_i", "f_to_i(v)", new ExpressionFactoryHelper<FtoiExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("i_to_f", "i_to_f(v)", new ExpressionFactoryHelper<ItofExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("f_to_u", "f_to_u(v)", new ExpressionFactoryHelper<FtouExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("u_to_f", "u_to_f(v)", new ExpressionFactoryHelper<UtofExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("d_to_l", "d_to_l(v)", new ExpressionFactoryHelper<DtolExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("l_to_d", "l_to_d(v)", new ExpressionFactoryHelper<LtodExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("d_to_u", "d_to_u(v)", new ExpressionFactoryHelper<DtouExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("u_to_d", "u_to_d(v)", new ExpressionFactoryHelper<UtodExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("lerp", "lerp(a,b,t)", new ExpressionFactoryHelper<LerpExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("lerp_unclamped", "lerp_unclamped(a,b,t)", new ExpressionFactoryHelper<LerpUnclampedExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("lerp_angle", "lerp_angle(a,b,t)", new ExpressionFactoryHelper<LerpAngleExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("smooth_step", "smooth_step(from,to,t)", new ExpressionFactoryHelper<SmoothStepExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("clamp01", "clamp01(v)", new ExpressionFactoryHelper<Clamp01Exp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("clamp", "clamp(v,v1,v2)", new ExpressionFactoryHelper<ClampExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("approximately", "approximately(v1,v2)", new ExpressionFactoryHelper<ApproximatelyExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("is_power_of_two", "is_power_of_two(v)", new ExpressionFactoryHelper<IsPowerOfTwoExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("closest_power_of_two", "closest_power_of_two(v)", new ExpressionFactoryHelper<ClosestPowerOfTwoExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("next_power_of_two", "next_power_of_two(v)", new ExpressionFactoryHelper<NextPowerOfTwoExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("distance", "distance(x1,y1,x2,y2)", new ExpressionFactoryHelper<DistExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("distance_square", "distance_square(x1,y1,x2,y2)", new ExpressionFactoryHelper<DistSqrExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("great", "great(a,b)", new ExpressionFactoryHelper<GreatExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("great_equal", "great_equal(a,b)", new ExpressionFactoryHelper<GreatEqualExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("less", "less(a,b)", new ExpressionFactoryHelper<LessExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("less_equal", "less_equal(a,b)", new ExpressionFactoryHelper<LessEqualExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("equal", "equal(a,b)", new ExpressionFactoryHelper<EqualExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("not_equal", "not_equal(a,b)", new ExpressionFactoryHelper<NotEqualExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("and", "and(a,b)", new ExpressionFactoryHelper<AndExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("or", "or(a,b)", new ExpressionFactoryHelper<OrExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("not", "not(a)", new ExpressionFactoryHelper<NotExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("cond_expr", "cond_expr(expr,a,b)", new ExpressionFactoryHelper<CondExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("format", "format(fmt,arg1,arg2,...)", new ExpressionFactoryHelper<FormatExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_type_assembly_name", "get_type_assembly_name(obj)", new ExpressionFactoryHelper<GetTypeAssemblyNameExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_type_full_name", "get_type_full_name(obj)", new ExpressionFactoryHelper<GetTypeFullNameExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_type_name", "get_type_name(obj)", new ExpressionFactoryHelper<GetTypeNameExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_type", "get_type(type_str)", new ExpressionFactoryHelper<GetTypeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("change_type", "change_type(obj,type_str)", new ExpressionFactoryHelper<ChangeTypeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("parse_enum", "parse_enum(type_str,val_str)", new ExpressionFactoryHelper<ParseEnumExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("is_null", "is_null(obj)", new ExpressionFactoryHelper<IsNullExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("null", "null()", new ExpressionFactoryHelper<NullExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("equals_null", "equals_null(obj)", new ExpressionFactoryHelper<EqualsNullExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("dotnet_load", "dotnet_load(dll_path)", new ExpressionFactoryHelper<DotnetLoadExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("dotnet_new", "dotnet_new(assembly,type_name,arg1,arg2,...)", new ExpressionFactoryHelper<DotnetNewExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_substring", "string_substring(str[,start,len]) function", new ExpressionFactoryHelper<SubstringExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_substr", "string_substr(str[,start,len]) function", new ExpressionFactoryHelper<SubstringExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("substr", "substr(str[, start, count])", new ExpressionFactoryHelper<SubstringExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("new_string_builder", "new_string_builder()", new ExpressionFactoryHelper<NewStringBuilderExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("append_format", "append_format(sb,fmt,arg1,arg2,...)", new ExpressionFactoryHelper<AppendFormatExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("append_format_line", "append_format_line(sb,fmt,arg1,arg2,...)", new ExpressionFactoryHelper<AppendFormatLineExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_builder_to_string", "string_builder_to_string(sb)", new ExpressionFactoryHelper<StringBuilderToStringExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_join", "string_join(sep,list)", new ExpressionFactoryHelper<StringJoinExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("join_string", "join_string(sep,list)", new ExpressionFactoryHelper<StringJoinExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_split", "string_split(str, sep) or string_split(str,sep_list)", new ExpressionFactoryHelper<StringSplitExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("split_string", "split_string(str, sep) or split_string(str,sep_list)", new ExpressionFactoryHelper<StringSplitExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("join", "join(str, list)", new ExpressionFactoryHelper<StringJoinExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("split", "split(str, sep) or split(str, sep_char_list)", new ExpressionFactoryHelper<StringSplitExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_trim", "string_trim(str)", new ExpressionFactoryHelper<StringTrimExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("trim_string", "trim_string(str)", new ExpressionFactoryHelper<StringTrimExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("trim", "trim(str)", new ExpressionFactoryHelper<StringTrimExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_trim_start", "string_trim_start(str)", new ExpressionFactoryHelper<StringTrimStartExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("trim_start_string", "trim_start_string(str)", new ExpressionFactoryHelper<StringTrimStartExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("trim_start", "trim_start(str)", new ExpressionFactoryHelper<StringTrimStartExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_trim_end", "string_trim_end(str)", new ExpressionFactoryHelper<StringTrimEndExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("trim_end_string", "trim_end_string(str)", new ExpressionFactoryHelper<StringTrimEndExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("trim_end", "trim_end(str)", new ExpressionFactoryHelper<StringTrimEndExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_to_lower", "string_to_lower(str)", new ExpressionFactoryHelper<StringToLowerExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_to_upper", "string_to_upper(str)", new ExpressionFactoryHelper<StringToUpperExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_replace", "string_replace(str, substr, replace[, exactMatch])", new ExpressionFactoryHelper<StringReplaceExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("replace_string", "replace_string(str, substr, replace[, exactMatch])", new ExpressionFactoryHelper<StringReplaceExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("replace", "replace(str, substr, replace[, exactMatch])", new ExpressionFactoryHelper<StringReplaceExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_replace_char", "string_replace_char(str,key,char_as_str)", new ExpressionFactoryHelper<StringReplaceCharExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("make_string", "make_string(char1_as_str_or_int,char2_as_str_or_int,...)", new ExpressionFactoryHelper<MakeStringExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_contains", "string_contains(str,str_or_list_1,str_or_list_2,...)", new ExpressionFactoryHelper<StringContainsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_not_contains", "string_not_contains(str,str_or_list_1,str_or_list_2,...)", new ExpressionFactoryHelper<StringNotContainsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_contains_any", "string_contains_any(str,str_or_list_1,str_or_list_2,...)", new ExpressionFactoryHelper<StringContainsAnyExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_not_contains_any", "string_not_contains_any(str,str_or_list_1,str_or_list_2,...)", new ExpressionFactoryHelper<StringNotContainsAnyExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("contains", "contains(str, substr1, ...)", new ExpressionFactoryHelper<StringContainsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("contains_any", "contains_any(str, substr1, ...)", new ExpressionFactoryHelper<StringContainsAnyExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("not_contains", "not_contains(str, substr1, ...)", new ExpressionFactoryHelper<StringNotContainsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("not_contains_any", "not_contains_any(str, substr1, ...)", new ExpressionFactoryHelper<StringNotContainsAnyExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_to_int", "string_to_int(str)", new ExpressionFactoryHelper<Str2IntExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("str_to_int", "str_to_int(str)", new ExpressionFactoryHelper<Str2IntExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_to_uint", "string_to_uint(str)", new ExpressionFactoryHelper<Str2UintExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("str_to_uint", "str_to_uint(str)", new ExpressionFactoryHelper<Str2UintExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_to_long", "string_to_long(str)", new ExpressionFactoryHelper<Str2LongExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("str_to_long", "str_to_long(str)", new ExpressionFactoryHelper<Str2LongExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_to_ulong", "string_to_ulong(str)", new ExpressionFactoryHelper<Str2UlongExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("str_to_ulong", "str_to_ulong(str)", new ExpressionFactoryHelper<Str2UlongExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("string_to_float", "string_to_float(str)", new ExpressionFactoryHelper<Str2FloatExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("str_to_float", "str_to_float(str)", new ExpressionFactoryHelper<Str2FloatExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("strint_to_double", "strint_to_double(str)", new ExpressionFactoryHelper<Str2DoubleExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("str_to_double", "str_to_double(str)", new ExpressionFactoryHelper<Str2DoubleExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("hex_string_to_int", "hex_string_to_int(str)", new ExpressionFactoryHelper<Hex2IntExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("hex_to_int", "hex_to_int(str)", new ExpressionFactoryHelper<Hex2IntExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("hex_string_to_uint", "hex_string_to_uint(str)", new ExpressionFactoryHelper<Hex2UintExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("hex_to_uint", "hex_to_uint(str)", new ExpressionFactoryHelper<Hex2UintExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("hex_string_to_long", "hex_string_to_long(str)", new ExpressionFactoryHelper<Hex2LongExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("hex_to_long", "hex_to_long(str)", new ExpressionFactoryHelper<Hex2LongExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("hex_string_to_ulong", "hex_string_to_ulong(str)", new ExpressionFactoryHelper<Hex2UlongExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("hex_to_ulong", "hex_to_ulong(str)", new ExpressionFactoryHelper<Hex2UlongExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("date_time_string", "date_time_string([fmt])", new ExpressionFactoryHelper<DatetimeStrExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("date_time_str", "date_time_str([fmt])", new ExpressionFactoryHelper<DatetimeStrExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("long_date_string", "long_date_string()", new ExpressionFactoryHelper<LongDateStrExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("long_date_str", "long_date_str()", new ExpressionFactoryHelper<LongDateStrExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("long_time_string", "long_time_string()", new ExpressionFactoryHelper<LongTimeStrExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("long_time_str", "long_time_str()", new ExpressionFactoryHelper<LongTimeStrExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("short_date_string", "short_date_string()", new ExpressionFactoryHelper<ShortDateStrExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("short_date_str", "short_date_str()", new ExpressionFactoryHelper<ShortDateStrExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("short_time_string", "short_time_string()", new ExpressionFactoryHelper<ShortTimeStrExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("short_time_str", "short_time_str()", new ExpressionFactoryHelper<ShortTimeStrExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("is_null_or_empty", "is_null_or_empty(str)", new ExpressionFactoryHelper<IsNullOrEmptyExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("tuple", "(v1,v2,...) or tuple(v1,v2,...) object", new ExpressionFactoryHelper<TupleExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("new_tuple", "(v1,v2,...) or new_tuple(v1,v2,...) object", new ExpressionFactoryHelper<TupleExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("tuple_set", "(var1,var2,...) = (v1,v2,...) or tuple_set((var1,var2,...), (v1,v2,...))", new ExpressionFactoryHelper<TupleSetExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("array", "[v1,v2,...] or array(v1,v2,...) object", new ExpressionFactoryHelper<ArrayExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("new_array", "[v1,v2,...] or new_array(v1,v2,...) object", new ExpressionFactoryHelper<ArrayExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("newarray", "[v1,v2,...] or newarray(v1,v2,...) object", false, new ExpressionFactoryHelper<ArrayExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("to_array", "to_array(list)", new ExpressionFactoryHelper<ToArrayExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("list_count", "list_count(list)", new ExpressionFactoryHelper<ListSizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("list_get_count", "list_get_count(list)", false, new ExpressionFactoryHelper<ListSizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_list_count", "get_list_count(list)", false, new ExpressionFactoryHelper<ListSizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("list_size", "list_size(list)", false, new ExpressionFactoryHelper<ListSizeExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("list", "list(v1,v2,...) object", new ExpressionFactoryHelper<ListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("new_list", "new_list(v1,v2,...) object", new ExpressionFactoryHelper<ListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("newlist", "newlist(v1,v2,...) object", false, new ExpressionFactoryHelper<ListExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("list_get", "list_get(list,index[,defval])", new ExpressionFactoryHelper<ListGetExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("list_set", "list_set(list,index,val)", new ExpressionFactoryHelper<ListSetExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("list_index_of", "list_index_of(list,val)", new ExpressionFactoryHelper<ListIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("list_indexof", "list_indexof(list,val)", false, new ExpressionFactoryHelper<ListIndexOfExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("list_add", "list_add(list,val)", new ExpressionFactoryHelper<ListAddExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("list_remove", "list_remove(list,val)", new ExpressionFactoryHelper<ListRemoveExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("list_insert", "list_insert(list,index,val)", new ExpressionFactoryHelper<ListInsertExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("list_remove_at", "list_remove_at(list,index)", new ExpressionFactoryHelper<ListRemoveAtExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("list_clear", "list_clear(list)", new ExpressionFactoryHelper<ListClearExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("list_split", "list_split(list,ct) api, return list of list", new ExpressionFactoryHelper<ListSplitExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("hashtable_count", "hashtable_count(hash)", new ExpressionFactoryHelper<HashtableSizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("hashtable_get_count", "hashtable_get_count(hash)", false, new ExpressionFactoryHelper<HashtableSizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_hashtable_count", "get_hashtable_count(hash)", false, new ExpressionFactoryHelper<HashtableSizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("hashtable_size", "hashtable_size(hash)", false, new ExpressionFactoryHelper<HashtableSizeExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("hashtable", "{k1=>v1,k2=>v2,...} or {k1:v1,k2:v2,...} or hashtable(k1=>v1,k2=>v2,...) or hashtable(k1:v1,k2:v2,...) object", new ExpressionFactoryHelper<HashtableExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("new_hashtable", "{k1=>v1,k2=>v2,...} or {k1:v1,k2:v2,...} or new_hashtable(k1=>v1,k2=>v2,...) or new_hashtable(k1:v1,k2:v2,...) object", new ExpressionFactoryHelper<HashtableExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("newhashtable", "{k1=>v1,k2=>v2,...} or {k1:v1,k2:v2,...} or newhashtable(k1=>v1,k2=>v2,...) or newhashtable(k1:v1,k2:v2,...) object", false, new ExpressionFactoryHelper<HashtableExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("hashtable_get", "hashtable_get(hash,key[,defval])", new ExpressionFactoryHelper<HashtableGetExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("hashtable_set", "hashtable_set(hash,key,val)", new ExpressionFactoryHelper<HashtableSetExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("hashtable_add", "hashtable_add(hash,key,val)", new ExpressionFactoryHelper<HashtableAddExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("hashtable_remove", "hashtable_remove(hash,key)", new ExpressionFactoryHelper<HashtableRemoveExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("hashtable_clear", "hashtable_clear(hash)", new ExpressionFactoryHelper<HashtableClearExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("hashtable_keys", "hashtable_keys(hash)", new ExpressionFactoryHelper<HashtableKeysExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("hashtable_values", "hashtable_values(hash)", new ExpressionFactoryHelper<HashtableValuesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("list_hashtable", "list_hashtable(hash) api, return list of pair", new ExpressionFactoryHelper<ListHashtableExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("hashtable_split", "hashtable_split(hash,ct) api, return list of hashtable", new ExpressionFactoryHelper<HashtableSplitExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("peek", "peek(queue_or_stack)", new ExpressionFactoryHelper<PeekExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("stack_count", "stack_count(stack)", new ExpressionFactoryHelper<StackSizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("stack_get_count", "stack_get_count(stack)", false, new ExpressionFactoryHelper<StackSizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_stack_count", "get_stack_count(stack)", false, new ExpressionFactoryHelper<StackSizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("stack_size", "stack_size(stack)", false, new ExpressionFactoryHelper<StackSizeExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("stack", "stack(v1,v2,...) object", new ExpressionFactoryHelper<StackExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("new_stack", "new_stack(v1,v2,...) object", new ExpressionFactoryHelper<StackExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("newstack", "newstack(v1,v2,...) object", false, new ExpressionFactoryHelper<StackExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("push", "push(stack,v)", new ExpressionFactoryHelper<PushExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("pop", "pop(stack)", new ExpressionFactoryHelper<PopExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("stack_clear", "stack_clear(stack)", new ExpressionFactoryHelper<StackClearExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("queue_count", "queue_count(queue)", new ExpressionFactoryHelper<QueueSizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("queue_get_count", "queue_get_count(queue)", false, new ExpressionFactoryHelper<QueueSizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_queue_count", "get_queue_count(queue)", false, new ExpressionFactoryHelper<QueueSizeExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("queue_size", "queue_size(queue)", false, new ExpressionFactoryHelper<QueueSizeExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("queue", "queue(v1,v2,...) object", new ExpressionFactoryHelper<QueueExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("new_queue", "new_queue(v1,v2,...) object", new ExpressionFactoryHelper<QueueExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("newqueue", "newqueue(v1,v2,...) object", false, new ExpressionFactoryHelper<QueueExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("enqueue", "enqueue(queue,v)", new ExpressionFactoryHelper<EnqueueExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("dequeue", "dequeue(queue)", new ExpressionFactoryHelper<DequeueExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("queue_clear", "queue_clear(queue)", new ExpressionFactoryHelper<QueueClearExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("set_env", "set_env(k,v)", new ExpressionFactoryHelper<SetEnvironmentExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_env", "get_env(k)", new ExpressionFactoryHelper<GetEnvironmentExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("expand", "expand(str)", new ExpressionFactoryHelper<ExpandEnvironmentsExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("envs", "envs()", new ExpressionFactoryHelper<EnvironmentsExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("cd", "cd(path)", new ExpressionFactoryHelper<SetCurrentDirectoryExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("pwd", "pwd()", new ExpressionFactoryHelper<GetCurrentDirectoryExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("cmd_line", "cmd_line()", new ExpressionFactoryHelper<CommandLineExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("cmd_line_args", "cmd_line_args(prev_arg) or cmdlineargs() api, first return next arg, second return array of arg", new ExpressionFactoryHelper<CommandLineArgsExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("os", "os()", new ExpressionFactoryHelper<OsExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("os_platform", "os_platform()", new ExpressionFactoryHelper<OsPlatformExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("os_version", "os_version()", new ExpressionFactoryHelper<OsVersionExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_full_path", "get_full_path(path)", new ExpressionFactoryHelper<GetFullPathExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_path_root", "get_path_root(path)", new ExpressionFactoryHelper<GetPathRootExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_random_file_name", "get_random_file_name()", new ExpressionFactoryHelper<GetRandomFileNameExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_temp_file_name", "get_temp_file_name()", new ExpressionFactoryHelper<GetTempFileNameExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_temp_path", "get_temp_path()", new ExpressionFactoryHelper<GetTempPathExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("has_extension", "has_extension(path)", new ExpressionFactoryHelper<HasExtensionExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("is_path_rooted", "is_path_rooted(path)", new ExpressionFactoryHelper<IsPathRootedExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_file_name", "get_file_name(path)", new ExpressionFactoryHelper<GetFileNameExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_file_name_without_extension", "get_file_name_without_extension(path)", new ExpressionFactoryHelper<GetFileNameWithoutExtensionExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_extension", "get_extension(path)", new ExpressionFactoryHelper<GetExtensionExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_directory_name", "get_directory_name(path)", new ExpressionFactoryHelper<GetDirectoryNameExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("combine_path", "combine_path(path1,path2,...)", new ExpressionFactoryHelper<CombinePathExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("change_extension", "change_extension(path,ext)", new ExpressionFactoryHelper<ChangeExtensionExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("quote_path", "quote_path(path[,only_needed,single_quote])", new ExpressionFactoryHelper<QuotePathExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("echo", "echo(fmt,arg1,arg2,...) api, Console.WriteLine", new ExpressionFactoryHelper<EchoExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("call_stack", "call_stack()", new ExpressionFactoryHelper<CallStackExp>());

            AgentFrameworkService.Instance.DslEngine!.Register("file_echo", "file_echo(bool) or file_echo()", new ExpressionFactoryHelper<FileEchoExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("dir_exists", "dir_exists(dir)", new ExpressionFactoryHelper<DirectoryExistExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("direxists", "direxists(dir)", false, new ExpressionFactoryHelper<DirectoryExistExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("file_exists", "file_exists(file)", new ExpressionFactoryHelper<FileExistExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("fileexists", "fileexists(file)", false, new ExpressionFactoryHelper<FileExistExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("list_dirs", "list_dirs(dir,glob_pattern_list_or_str_1,glob_pattern_list_or_str_2,...)", new ExpressionFactoryHelper<ListDirectoriesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("list_files", "list_files(dir,glob_pattern_list_or_str_1,glob_pattern_list_or_str_2,...)", new ExpressionFactoryHelper<ListFilesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("list_all_dirs", "list_all_dirs(dir,glob_pattern_list_or_str_1,glob_pattern_list_or_str_2,...)", new ExpressionFactoryHelper<ListAllDirectoriesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("list_all_files", "list_all_files(dir,glob_pattern_list_or_str_1,glob_pattern_list_or_str_2,...)", new ExpressionFactoryHelper<ListAllFilesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("create_dir", "create_dir(dir)", new ExpressionFactoryHelper<CreateDirectoryExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("make_dir", "make_dir(path)", new ExpressionFactoryHelper<CreateDirectoryExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("copy_dir", "copy_dir(dir1,dir2,glob_pattern_list_or_str_1,glob_pattern_list_or_str_2,...) api, include subdir", new ExpressionFactoryHelper<CopyDirectoryExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("move_dir", "move_dir(dir1,dir2)", new ExpressionFactoryHelper<MoveDirectoryExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("delete_dir", "delete_dir(dir)", new ExpressionFactoryHelper<DeleteDirectoryExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("copy_file", "copy_file(file1,file2)", new ExpressionFactoryHelper<CopyFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("copy_files", "copy_files(dir1,dir2,glob_pattern_list_or_str_1,glob_pattern_list_or_str_2,...) api, dont include subdir", new ExpressionFactoryHelper<CopyFilesExp>());
            //AgentFrameworkService.Instance.DslEngine!.Register("move_file", "move_file(file1,file2)", new ExpressionFactoryHelper<MoveFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("delete_file", "delete_file(file)", new ExpressionFactoryHelper<DeleteFileExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("delete_files", "delete_files(dir,glob_pattern_list_or_str_1,glob_pattern_list_or_str_2,...) api, dont include subdir", new ExpressionFactoryHelper<DeleteFilesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("delete_all_files", "delete_all_files(dir,glob_pattern_list_or_str_1,glob_pattern_list_or_str_2,...) api, include subdir", new ExpressionFactoryHelper<DeleteAllFilesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_file_info", "get_file_info(file)", new ExpressionFactoryHelper<GetFileInfoExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_dir_info", "get_dir_info(dir)", new ExpressionFactoryHelper<GetDirectoryInfoExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_drive_info", "get_drive_info(drive)", new ExpressionFactoryHelper<GetDriveInfoExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("get_drives_info", "get_drives_info()", new ExpressionFactoryHelper<GetDrivesInfoExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("read_all_lines", "read_all_lines(file[,encoding])", new ExpressionFactoryHelper<ReadAllLinesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("write_all_lines", "write_all_lines(file,lines[,encoding])", new ExpressionFactoryHelper<WriteAllLinesExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("read_all_text", "read_all_text(file[,encoding])", new ExpressionFactoryHelper<ReadAllTextExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("write_all_text", "write_all_text(file,txt[,encoding])", new ExpressionFactoryHelper<WriteAllTextExp>());
            AgentFrameworkService.Instance.DslEngine!.Register("calc_md5", "calc_md5(file)", new ExpressionFactoryHelper<CalcMd5Exp>());
        }

        internal sealed class CloneExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: clone(v), aliased as clone_array|clone_list|clone_hashtable|clone_queue|clone_stack|clone_tuple|copy|copy_array|copy_list|copy_hashtable|copy_queue|copy_stack|copy_tuple");
                    return BoxedValue.NullObject;
                }
                if (operands.Count >= 1) {
                    var val = operands[0];
                    return Clone(val);
                }
                return BoxedValue.NullObject;
            }
            private BoxedValue Clone(BoxedValue val)
            {
                if (val.IsNullObject) {
                    return BoxedValue.NullObject;
                }
                else if (val.IsString) {
                    var str = val.GetString();
                    return string.Concat(str);
                }
                else if (val.IsObject) {
                    var obj = val.GetObject();
                    if (obj is ICloneable cloneable) {
                        return BoxedValue.FromObject(cloneable.Clone());
                    }
                    else if (obj is IList list) {
                        var newList = new List<BoxedValue>();
                        foreach (var item in list) {
                            newList.Add(BoxedValue.FromObject(item));
                        }
                        return BoxedValue.FromObject(newList);
                    }
                    else if (obj is IDictionary dict) {
                        var newDict = new Dictionary<BoxedValue, BoxedValue>();
                        foreach (var key in dict.Keys) {
                            var v = dict[key];
                            newDict.Add(BoxedValue.FromObject(key), BoxedValue.FromObject(v));
                        }
                        return BoxedValue.FromObject(newDict);
                    }
                    else if (obj is Queue<BoxedValue> queue) {
                        var newQueue = new Queue<BoxedValue>(queue);
                        return BoxedValue.FromObject(newQueue);
                    }
                    else if (obj is Stack<BoxedValue> stack) {
                        var newStack = new Stack<BoxedValue>(stack.ToArray());
                        return BoxedValue.FromObject(newStack);
                    }
                    else if (obj is Tuple<BoxedValue> t1) {
                        return BoxedValue.From(Tuple.Create(t1.Item1));
                    }
                    else if (obj is Tuple<BoxedValue, BoxedValue> t2) {
                        return BoxedValue.From(Tuple.Create(t2.Item1, t2.Item2));
                    }
                    else if (obj is Tuple<BoxedValue, BoxedValue, BoxedValue> t3) {
                        return BoxedValue.From(Tuple.Create(t3.Item1, t3.Item2, t3.Item3));
                    }
                    else if (obj is Tuple<BoxedValue, BoxedValue, BoxedValue, BoxedValue> t4) {
                        return BoxedValue.From(Tuple.Create(t4.Item1, t4.Item2, t4.Item3, t4.Item4));
                    }
                    else if (obj is Tuple<BoxedValue, BoxedValue, BoxedValue, BoxedValue, BoxedValue> t5) {
                        return BoxedValue.From(Tuple.Create(t5.Item1, t5.Item2, t5.Item3, t5.Item4, t5.Item5));
                    }
                    else if (obj is Tuple<BoxedValue, BoxedValue, BoxedValue, BoxedValue, BoxedValue, BoxedValue> t6) {
                        return BoxedValue.From(Tuple.Create(t6.Item1, t6.Item2, t6.Item3, t6.Item4, t6.Item5, t6.Item6));
                    }
                    else if (obj is Tuple<BoxedValue, BoxedValue, BoxedValue, BoxedValue, BoxedValue, BoxedValue, BoxedValue> t7) {
                        return BoxedValue.From(Tuple.Create(t7.Item1, t7.Item2, t7.Item3, t7.Item4, t7.Item5, t7.Item6, t7.Item7));
                    }
                    else if (obj is Tuple<BoxedValue, BoxedValue, BoxedValue, BoxedValue, BoxedValue, BoxedValue, BoxedValue, Tuple<BoxedValue>> t8) {
                        return BoxedValue.From(Tuple.Create(t8.Item1, t8.Item2, t8.Item3, t8.Item4, t8.Item5, t8.Item6, t8.Item7, Tuple.Create(Clone(t8.Rest.Item1))));
                    }
                }
                return val;
            }
        }
        internal sealed class AddExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                var v1 = m_Op1.Calc();
                var v2 = m_Op2.Calc();
                BoxedValue v;
                if (v1.IsString || v2.IsString) {
                    v = v1.ToString() + v2.ToString();
                }
                else if (v1.IsInteger && v2.IsInteger) {
                    if (v1.IsUnsignedInteger && v2.IsUnsignedInteger) {
                        v = v1.GetULong() + v2.GetULong();
                    }
                    else {
                        v = v1.GetLong() + v2.GetLong();
                    }
                }
                else {
                    v = v1.GetDouble() + v2.GetDouble();
                }
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                m_Op2 = exps[1];
                return true;
            }

            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;
        }
        internal sealed class SubExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                var v1 = m_Op1.Calc();
                var v2 = m_Op2.Calc();
                BoxedValue v;
                if (v1.IsInteger && v2.IsInteger) {
                    if (v1.IsUnsignedInteger && v2.IsUnsignedInteger) {
                        v = v1.GetULong() - v2.GetULong();
                    }
                    else {
                        v = v1.GetLong() - v2.GetLong();
                    }
                }
                else {
                    v = v1.GetDouble() - v2.GetDouble();
                }
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                m_Op2 = exps[1];
                return true;
            }

            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;
        }
        internal sealed class MulExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                var v1 = m_Op1.Calc();
                var v2 = m_Op2.Calc();
                BoxedValue v;
                if (v1.IsInteger && v2.IsInteger) {
                    if (v1.IsUnsignedInteger && v2.IsUnsignedInteger) {
                        v = v1.GetULong() * v2.GetULong();
                    }
                    else {
                        v = v1.GetLong() * v2.GetLong();
                    }
                }
                else {
                    v = v1.GetDouble() * v2.GetDouble();
                }
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                m_Op2 = exps[1];
                return true;
            }

            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;
        }
        internal sealed class DivExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                var v1 = m_Op1.Calc();
                var v2 = m_Op2.Calc();
                BoxedValue v;
                if (v1.IsInteger && v2.IsInteger) {
                    if (v1.IsUnsignedInteger && v2.IsUnsignedInteger) {
                        v = v1.GetULong() / v2.GetULong();
                    }
                    else {
                        v = v1.GetLong() / v2.GetLong();
                    }
                }
                else {
                    v = v1.GetDouble() / v2.GetDouble();
                }
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                m_Op2 = exps[1];
                return true;
            }

            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;
        }
        internal sealed class ModExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                var v1 = m_Op1.Calc();
                var v2 = m_Op2.Calc();
                BoxedValue v;
                if (v1.IsInteger && v2.IsInteger) {
                    if (v1.IsUnsignedInteger && v2.IsUnsignedInteger) {
                        v = v1.GetULong() % v2.GetULong();
                    }
                    else {
                        v = v1.GetLong() % v2.GetLong();
                    }
                }
                else {
                    v = v1.GetDouble() % v2.GetDouble();
                }
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                m_Op2 = exps[1];
                return true;
            }

            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;
        }
        internal sealed class BitAndExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                var v1 = m_Op1.Calc();
                var v2 = m_Op2.Calc();
                BoxedValue v;
                if (v1.IsUnsignedInteger && v2.IsUnsignedInteger) {
                    v = v1.GetULong() & v2.GetULong();
                }
                else {
                    v = v1.GetLong() & v2.GetLong();
                }
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                m_Op2 = exps[1];
                return true;
            }

            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;
        }
        internal sealed class BitOrExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                var v1 = m_Op1.Calc();
                var v2 = m_Op2.Calc();
                BoxedValue v;
                if (v1.IsUnsignedInteger && v2.IsUnsignedInteger) {
                    v = v1.GetULong() | v2.GetULong();
                }
                else {
                    v = v1.GetLong() | v2.GetLong();
                }
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                m_Op2 = exps[1];
                return true;
            }

            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;
        }
        internal sealed class BitXorExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                var v1 = m_Op1.Calc();
                var v2 = m_Op2.Calc();
                BoxedValue v;
                if (v1.IsUnsignedInteger && v2.IsUnsignedInteger) {
                    v = v1.GetULong() ^ v2.GetULong();
                }
                else {
                    v = v1.GetLong() ^ v2.GetLong();
                }
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                m_Op2 = exps[1];
                return true;
            }

            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;
        }
        internal sealed class BitNotExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                var v1 = m_Op1.Calc();
                BoxedValue v;
                if (v1.IsUnsignedInteger) {
                    v = ~v1.GetULong();
                }
                else {
                    v = ~v1.GetLong();
                }
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;
        }
        internal sealed class LShiftExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                var v1 = m_Op1.Calc();
                var v2 = m_Op2.Calc();
                BoxedValue v;
                if (v1.IsUnsignedInteger) {
                    v = v1.GetULong() << v2.GetInt();
                }
                else {
                    v = v1.GetLong() << v2.GetInt();
                }
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                m_Op2 = exps[1];
                return true;
            }

            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;
        }
        internal sealed class RShiftExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                var v1 = m_Op1.Calc();
                var v2 = m_Op2.Calc();
                BoxedValue v;
                if (v1.IsUnsignedInteger) {
                    v = v1.GetULong() >> v2.GetInt();
                }
                else {
                    v = v1.GetLong() >> v2.GetInt();
                }
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                m_Op2 = exps[1];
                return true;
            }

            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;
        }
        internal sealed class MaxExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                var opd1 = m_Op1.Calc();
                var opd2 = m_Op2.Calc();
                BoxedValue v;
                if (opd1.IsInteger && opd2.IsInteger) {
                    if (opd1.IsUnsignedInteger && opd2.IsUnsignedInteger) {
                        var v1 = opd1.GetULong();
                        var v2 = opd2.GetULong();
                        v = v1 >= v2 ? v1 : v2;
                    }
                    else {
                        var v1 = opd1.GetLong();
                        var v2 = opd2.GetLong();
                        v = v1 >= v2 ? v1 : v2;
                    }
                }
                else {
                    var v1 = opd1.GetDouble();
                    var v2 = opd2.GetDouble();
                    v = v1 >= v2 ? v1 : v2;
                }
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                m_Op2 = exps[1];
                return true;
            }

            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;
        }
        internal sealed class MinExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                var opd1 = m_Op1.Calc();
                var opd2 = m_Op2.Calc();
                BoxedValue v;
                if (opd1.IsInteger && opd2.IsInteger) {
                    if (opd1.IsUnsignedInteger && opd2.IsUnsignedInteger) {
                        var v1 = opd1.GetULong();
                        var v2 = opd2.GetULong();
                        v = v1 <= v2 ? v1 : v2;
                    }
                    else {
                        var v1 = opd1.GetLong();
                        var v2 = opd2.GetLong();
                        v = v1 <= v2 ? v1 : v2;
                    }
                }
                else {
                    var v1 = opd1.GetDouble();
                    var v2 = opd2.GetDouble();
                    v = v1 <= v2 ? v1 : v2;
                }
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                m_Op2 = exps[1];
                return true;
            }

            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;
        }
        internal sealed class AbsExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                var opd = m_Op.Calc();
                BoxedValue v;
                if (opd.IsInteger) {
                    long v1 = opd.GetLong();
                    v = v1 >= 0 ? v1 : -v1;
                }
                else {
                    double v1 = opd.GetDouble();
                    v = v1 >= 0 ? v1 : -v1;
                }
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op = exps[0];
                return true;
            }

            private IExpression m_Op = null!;
        }
        internal sealed class SinExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double v1 = m_Op.Calc().GetDouble();
                BoxedValue v = Math.Sin(v1);
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op = exps[0];
                return true;
            }

            private IExpression m_Op = null!;
        }
        internal sealed class CosExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double v1 = m_Op.Calc().GetDouble();
                BoxedValue v = Math.Cos(v1);
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op = exps[0];
                return true;
            }

            private IExpression m_Op = null!;
        }
        internal sealed class TanExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double v1 = m_Op.Calc().GetDouble();
                BoxedValue v = Math.Tan(v1);
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op = exps[0];
                return true;
            }

            private IExpression m_Op = null!;
        }
        internal sealed class AsinExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double v1 = m_Op.Calc().GetDouble();
                BoxedValue v = Math.Asin(v1);
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op = exps[0];
                return true;
            }

            private IExpression m_Op = null!;
        }
        internal sealed class AcosExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double v1 = m_Op.Calc().GetDouble();
                BoxedValue v = Math.Acos(v1);
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op = exps[0];
                return true;
            }

            private IExpression m_Op = null!;
        }
        internal sealed class AtanExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double v1 = m_Op.Calc().GetDouble();
                BoxedValue v = Math.Atan(v1);
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op = exps[0];
                return true;
            }

            private IExpression m_Op = null!;
        }
        internal sealed class Atan2Exp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double v1 = m_Op1.Calc().GetDouble();
                double v2 = m_Op2.Calc().GetDouble();
                BoxedValue v = Math.Atan2(v1, v2);
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                m_Op2 = exps[1];
                return true;
            }

            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;
        }
        internal sealed class SinhExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double v1 = m_Op.Calc().GetDouble();
                BoxedValue v = Math.Sinh(v1);
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op = exps[0];
                return true;
            }

            private IExpression m_Op = null!;
        }
        internal sealed class CoshExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double v1 = m_Op.Calc().GetDouble();
                BoxedValue v = Math.Cosh(v1);
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op = exps[0];
                return true;
            }

            private IExpression m_Op = null!;
        }
        internal sealed class TanhExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double v1 = m_Op.Calc().GetDouble();
                BoxedValue v = Math.Tanh(v1);
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op = exps[0];
                return true;
            }

            private IExpression m_Op = null!;
        }
        internal sealed class RndIntExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                long v1 = m_Op1.Calc().GetLong();
                long v2 = m_Op2.Calc().GetLong();
                BoxedValue v = (long)s_Random.Next((int)v1, (int)v2);
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                m_Op2 = exps[1];
                return true;
            }

            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;

            private static Random s_Random = new Random();
        }
        internal sealed class RndFloatExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double v1 = m_Op1.Calc().GetDouble();
                double v2 = m_Op2.Calc().GetDouble();
                BoxedValue v = s_Random.NextDouble() * (v2 - v1) + v1;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                m_Op2 = exps[1];
                return true;
            }

            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;

            private static Random s_Random = new Random();
        }
        internal sealed class PowExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double v1 = m_Op1.Calc().GetDouble();
                double v2 = m_Op2.Calc().GetDouble();
                BoxedValue v = Math.Pow(v1, v2);
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                m_Op2 = exps[1];
                return true;
            }

            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;
        }
        internal sealed class SqrtExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double v1 = m_Op1.Calc().GetDouble();
                BoxedValue v = Math.Sqrt(v1);
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;
        }
        internal sealed class ExpExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double v1 = m_Op1.Calc().GetDouble();
                BoxedValue v = Math.Exp(v1);
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;
        }
        internal sealed class Exp2Exp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double v1 = m_Op1.Calc().GetDouble();
                BoxedValue v = Math.Pow(2, v1);
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;
        }
        internal sealed class LogExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double v1 = m_Op1.Calc().GetDouble();
                if (m_ArgNum == 1) {
                    BoxedValue v = Math.Log(v1);
                    return v;
                }
                else {
                    double v2 = m_Op2.Calc().GetDouble();
                    BoxedValue v = Math.Log(v1, v2);
                    return v;
                }
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_ArgNum = exps.Count;
                m_Op1 = exps[0];
                if (m_ArgNum > 1) {
                    m_Op2 = exps[1];
                }
                return true;
            }

            private int m_ArgNum;
            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;
        }
        internal sealed class Log2Exp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double v1 = m_Op1.Calc().GetDouble();
                BoxedValue v = Math.Log(v1) / Math.Log(2);
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;
        }
        internal sealed class Log10Exp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double v1 = m_Op1.Calc().GetDouble();
                BoxedValue v = Math.Log10(v1);
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;
        }
        internal sealed class FloorExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double v1 = m_Op1.Calc().GetDouble();
                BoxedValue v = Math.Floor(v1);
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;
        }
        internal sealed class CeilingExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double v1 = m_Op1.Calc().GetDouble();
                BoxedValue v = Math.Ceiling(v1);
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;
        }
        internal sealed class RoundExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double v1 = m_Op1.Calc().GetDouble();
                BoxedValue v = Math.Round(v1);
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;
        }
        internal sealed class FloorToIntExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double v1 = m_Op1.Calc().GetDouble();
                BoxedValue v = (int)Math.Floor(v1);
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;
        }
        internal sealed class CeilingToIntExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double v1 = m_Op1.Calc().GetDouble();
                BoxedValue v = (int)Math.Ceiling(v1);
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;
        }
        internal sealed class RoundToIntExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double v1 = m_Op1.Calc().GetDouble();
                BoxedValue v = (int)Math.Round(v1);
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;
        }
        internal sealed class BoolExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                bool v1 = m_Op1.Calc().GetBool();
                BoxedValue v = v1;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;
        }
        internal sealed class SByteExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                sbyte v1 = m_Op1.Calc().GetSByte();
                BoxedValue v = v1;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;
        }
        internal sealed class ByteExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                byte v1 = m_Op1.Calc().GetByte();
                BoxedValue v = v1;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;
        }
        internal sealed class CharExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                char v1 = m_Op1.Calc().GetChar();
                BoxedValue v = v1;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;
        }
        internal sealed class ShortExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                short v1 = m_Op1.Calc().GetShort();
                BoxedValue v = v1;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;
        }
        internal sealed class UShortExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                ushort v1 = m_Op1.Calc().GetUShort();
                BoxedValue v = v1;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;
        }
        internal sealed class IntExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                int v1 = m_Op1.Calc().GetInt();
                BoxedValue v = v1;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;
        }
        internal sealed class UIntExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                uint v1 = m_Op1.Calc().GetUInt();
                BoxedValue v = v1;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;
        }
        internal sealed class LongExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                long v1 = m_Op1.Calc().GetLong();
                BoxedValue v = v1;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;
        }
        internal sealed class ULongExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                ulong v1 = m_Op1.Calc().GetULong();
                BoxedValue v = v1;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;
        }
        internal sealed class FloatExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                float v1 = m_Op1.Calc().GetFloat();
                BoxedValue v = v1;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;
        }
        internal sealed class DoubleExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double v1 = m_Op1.Calc().GetDouble();
                BoxedValue v = v1;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;
        }
        internal sealed class DecimalExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                decimal v1 = m_Op1.Calc().GetDecimal();
                BoxedValue v = v1;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;
        }
        internal sealed class ItofExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                int v1 = m_Op1.Calc().GetInt();
                float v2 = 0;
                unsafe {
                    v2 = *(float*)&v1;
                }
                BoxedValue v = v2;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;
        }
        internal sealed class FtoiExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                float v1 = m_Op1.Calc().GetFloat();
                int v2 = 0;
                unsafe {
                    v2 = *(int*)&v1;
                }
                BoxedValue v = v2;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;
        }
        internal sealed class UtofExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                uint v1 = m_Op1.Calc().GetUInt();
                float v2 = 0;
                unsafe {
                    v2 = *(float*)&v1;
                }
                BoxedValue v = v2;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;
        }
        internal sealed class FtouExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                float v1 = m_Op1.Calc().GetFloat();
                uint v2 = 0;
                unsafe {
                    v2 = *(uint*)&v1;
                }
                BoxedValue v = v2;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;
        }
        internal sealed class LtodExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                long v1 = m_Op1.Calc().GetLong();
                double v2 = 0;
                unsafe {
                    v2 = *(double*)&v1;
                }
                BoxedValue v = v2;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;
        }
        internal sealed class DtolExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double v1 = m_Op1.Calc().GetDouble();
                long v2 = 0;
                unsafe {
                    v2 = *(long*)&v1;
                }
                BoxedValue v = v2;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;
        }
        internal sealed class UtodExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                ulong v1 = m_Op1.Calc().GetULong();
                double v2 = 0;
                unsafe {
                    v2 = *(double*)&v1;
                }
                BoxedValue v = v2;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;
        }
        internal sealed class DtouExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double v1 = m_Op1.Calc().GetDouble();
                ulong v2 = 0;
                unsafe {
                    v2 = *(ulong*)&v1;
                }
                BoxedValue v = v2;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;
        }
        internal sealed class LerpExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double a = m_Op1.Calc().GetDouble();
                double b = m_Op2.Calc().GetDouble();
                double t = m_Op3.Calc().GetDouble();
                BoxedValue v;
                v = a + (b - a) * ClampExp.Clamp01(t);
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                m_Op2 = exps[1];
                m_Op3 = exps[2];
                return true;
            }

            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;
            private IExpression m_Op3 = null!;
        }
        internal sealed class LerpUnclampedExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double a = m_Op1.Calc().GetDouble();
                double b = m_Op2.Calc().GetDouble();
                double t = m_Op3.Calc().GetDouble();
                BoxedValue v = a + (b - a) * t;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                m_Op2 = exps[1];
                m_Op3 = exps[2];
                return true;
            }

            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;
            private IExpression m_Op3 = null!;
        }
        internal sealed class LerpAngleExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double a = m_Op1.Calc().GetDouble();
                double b = m_Op2.Calc().GetDouble();
                double t = m_Op3.Calc().GetDouble();
                double num = Repeat(b - a, 360.0);
                if (num > 180f) {
                    num -= 360f;
                }
                BoxedValue v = a + num * ClampExp.Clamp01(t);
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                m_Op2 = exps[1];
                m_Op3 = exps[2];
                return true;
            }

            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;
            private IExpression m_Op3 = null!;

            public static double Repeat(double t, double length)
            {
                return ClampExp.Clamp(t - Math.Floor(t / length) * length, 0f, length);
            }
        }
        internal sealed class SmoothStepExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double from = m_Op1.Calc().GetDouble();
                double to = m_Op2.Calc().GetDouble();
                double t = m_Op3.Calc().GetDouble();
                t = ClampExp.Clamp01(t);
                t = -2.0 * t * t * t + 3.0 * t * t;
                BoxedValue v = to * t + from * (1.0 - t);
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                m_Op2 = exps[1];
                m_Op3 = exps[2];
                return true;
            }

            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;
            private IExpression m_Op3 = null!;
        }
        internal sealed class Clamp01Exp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double v1 = m_Op.Calc().GetDouble();
                BoxedValue v = ClampExp.Clamp01(v1);
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op = exps[0];
                return true;
            }

            private IExpression m_Op = null!;
        }
        internal sealed class ClampExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double v1 = m_Op1.Calc().GetDouble();
                double v2 = m_Op2.Calc().GetDouble();
                double v3 = m_Op3.Calc().GetDouble();
                BoxedValue v;
                if (v2 <= v3) {
                    if (v1 < v2)
                        v = v2;
                    else if (v1 > v3)
                        v = v3;
                    else
                        v = v1;
                }
                else {
                    if (v1 > v2)
                        v = v2;
                    else if (v1 < v3)
                        v = v3;
                    else
                        v = v1;
                }
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                m_Op2 = exps[1];
                m_Op3 = exps[2];
                return true;
            }

            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;
            private IExpression m_Op3 = null!;

            public static double Clamp(double value, double min, double max)
            {
                if (value < min) {
                    value = min;
                }
                else if (value > max) {
                    value = max;
                }
                return value;
            }
            public static double Clamp01(double value)
            {
                if (value < 0f) {
                    return 0f;
                }
                if (value > 1f) {
                    return 1f;
                }
                return value;
            }
        }
        internal sealed class ApproximatelyExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                float v1 = m_Op1.Calc().GetFloat();
                float v2 = m_Op2.Calc().GetFloat();
                BoxedValue v = Approximately(v1, v2) ? 1 : 0;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                m_Op2 = exps[1];
                return true;
            }

            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;

            public static bool Approximately(double a, double b)
            {
                return Math.Abs(b - a) < Math.Max(1E-06 * Math.Max(Math.Abs(a), Math.Abs(b)), double.Epsilon * 8.0);
            }
        }
        internal sealed class IsPowerOfTwoExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                int v1 = m_Op1.Calc().GetInt();
                int v = IsPowerOfTwo(v1) ? 1 : 0;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;

            public bool IsPowerOfTwo(int v)
            {
                int n = (int)Math.Round(Math.Log(v) / Math.Log(2));
                return (int)Math.Round(Math.Pow(2, n)) == v;
            }
        }
        internal sealed class ClosestPowerOfTwoExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                int v1 = m_Op1.Calc().GetInt();
                int v = ClosestPowerOfTwo(v1);
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;

            public int ClosestPowerOfTwo(int v)
            {
                int n = (int)Math.Round(Math.Log(v) / Math.Log(2));
                return (int)Math.Round(Math.Pow(2, n));
            }
        }
        internal sealed class NextPowerOfTwoExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                int v1 = m_Op1.Calc().GetInt();
                int v = NextPowerOfTwo(v1);
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                return true;
            }

            private IExpression m_Op1 = null!;

            public int NextPowerOfTwo(int v)
            {
                int n = (int)Math.Round(Math.Log(v) / Math.Log(2));
                return (int)Math.Round(Math.Pow(2, n + 1));
            }
        }
        internal sealed class DistExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                float x1 = (float)m_Op1.Calc().GetDouble();
                float y1 = (float)m_Op2.Calc().GetDouble();
                float x2 = (float)m_Op3.Calc().GetDouble();
                float y2 = (float)m_Op4.Calc().GetDouble();
                BoxedValue v = Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                m_Op2 = exps[1];
                m_Op3 = exps[2];
                m_Op4 = exps[3];
                return true;
            }

            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;
            private IExpression m_Op3 = null!;
            private IExpression m_Op4 = null!;
        }
        internal sealed class DistSqrExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                float x1 = (float)m_Op1.Calc().GetDouble();
                float y1 = (float)m_Op2.Calc().GetDouble();
                float x2 = (float)m_Op3.Calc().GetDouble();
                float y2 = (float)m_Op4.Calc().GetDouble();
                BoxedValue v = (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1);
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                m_Op2 = exps[1];
                m_Op3 = exps[2];
                m_Op4 = exps[3];
                return true;
            }

            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;
            private IExpression m_Op3 = null!;
            private IExpression m_Op4 = null!;
        }
        internal sealed class GreatExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double v1 = m_Op1.Calc().GetDouble();
                double v2 = m_Op2.Calc().GetDouble();
                BoxedValue v = v1 > v2 ? 1 : 0;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                m_Op2 = exps[1];
                return true;
            }

            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;
        }
        internal sealed class GreatEqualExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double v1 = m_Op1.Calc().GetDouble();
                double v2 = m_Op2.Calc().GetDouble();
                BoxedValue v = v1 >= v2 ? 1 : 0;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                m_Op2 = exps[1];
                return true;
            }

            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;
        }
        internal sealed class LessExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double v1 = m_Op1.Calc().GetDouble();
                double v2 = m_Op2.Calc().GetDouble();
                BoxedValue v = v1 < v2 ? 1 : 0;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                m_Op2 = exps[1];
                return true;
            }

            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;
        }
        internal sealed class LessEqualExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                double v1 = m_Op1.Calc().GetDouble();
                double v2 = m_Op2.Calc().GetDouble();
                BoxedValue v = v1 <= v2 ? 1 : 0;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                m_Op2 = exps[1];
                return true;
            }

            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;
        }
        internal sealed class EqualExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                var v1 = m_Op1.Calc();
                var v2 = m_Op2.Calc();
                BoxedValue v = v1.ToString() == v2.ToString() ? 1 : 0;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                m_Op2 = exps[1];
                return true;
            }

            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;
        }
        internal sealed class NotEqualExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                var v1 = m_Op1.Calc();
                var v2 = m_Op2.Calc();
                BoxedValue v = v1.ToString() != v2.ToString() ? 1 : 0;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                m_Op2 = exps[1];
                return true;
            }

            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;
        }
        internal sealed class AndExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                long v1 = m_Op1.Calc().GetLong();
                long v2 = 0;
                BoxedValue v = v1 != 0 && (v2 = m_Op2.Calc().GetLong()) != 0 ? 1 : 0;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                m_Op2 = exps[1];
                return true;
            }

            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;
        }
        internal sealed class OrExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                long v1 = m_Op1.Calc().GetLong();
                long v2 = 0;
                BoxedValue v = v1 != 0 || (v2 = m_Op2.Calc().GetLong()) != 0 ? 1 : 0;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op1 = exps[0];
                m_Op2 = exps[1];
                return true;
            }

            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;
        }
        internal sealed class NotExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                long val = m_Op.Calc().GetLong();
                BoxedValue v = val == 0 ? 1 : 0;
                return v;
            }
            protected override bool Load(IList<IExpression> exps)
            {
                m_Op = exps[0];
                return true;
            }

            private IExpression m_Op = null!;
        }
        internal sealed class CondExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                var v1 = m_Op1.Calc();
                var v2 = BoxedValue.NullObject;
                BoxedValue v3 = BoxedValue.NullObject;
                BoxedValue v = v1.GetLong() != 0 ? v2 = m_Op2.Calc() : v3 = m_Op3.Calc();
                return v;
            }
            protected override bool Load(Dsl.StatementData statementData)
            {
                Dsl.FunctionData funcData1 = statementData.First.AsFunction;
                Dsl.FunctionData funcData2 = statementData.Second.AsFunction;
                if (funcData1.IsHighOrder && funcData1.HaveLowerOrderParam() && funcData2.GetId() == ":" && funcData2.HaveParamOrStatement()) {
                    Dsl.ISyntaxComponent cond = funcData1.LowerOrderFunction.GetParam(0);
                    Dsl.ISyntaxComponent op1 = funcData1.GetParam(0);
                    Dsl.ISyntaxComponent op2 = funcData2.GetParam(0);
                    m_Op1 = Calculator.Load(cond);
                    m_Op2 = Calculator.Load(op1);
                    m_Op3 = Calculator.Load(op2);
                }
                else {
                    //error
                    Calculator.Log("DslCalculator error, {0} line {1}", statementData.ToScriptString(false, Dsl.DelimiterInfo.Default), statementData.GetLine());
                }
                return true;
            }

            private IExpression m_Op1 = null!;
            private IExpression m_Op2 = null!;
            private IExpression m_Op3 = null!;
        }
        internal sealed class TupleExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                BoxedValue v;
                int num = m_Expressions.Count;
                if (num == 0) {
                    v = BoxedValue.NullObject;
                }
                else {
                    v = PackValues(0);
                }
                return v;
            }
            protected override bool Load(Dsl.FunctionData callData)
            {
                int num = callData.GetParamNum();
                for (int i = 0; i < num; ++i) {
                    Dsl.ISyntaxComponent param = callData.GetParam(i);
                    m_Expressions.Add(Calculator.Load(param));
                }
                return true;
            }
            private BoxedValue PackValues(int start)
            {
                const int c_MaxTupleElementNum = 8;
                BoxedValue v1 = BoxedValue.NullObject, v2 = BoxedValue.NullObject, v3 = BoxedValue.NullObject, v4 = BoxedValue.NullObject, v5 = BoxedValue.NullObject, v6 = BoxedValue.NullObject, v7 = BoxedValue.NullObject, v8 = BoxedValue.NullObject;
                int totalNum = m_Expressions.Count;
                int num = totalNum - start;
                for (int ix = 0; ix < num && ix < c_MaxTupleElementNum; ++ix) {
                    var exp = m_Expressions[start + ix];
                    switch (ix) {
                        case 0:
                            v1 = exp.Calc();
                            if (num == 1) {
                                return new TupleValue1(v1);
                            }
                            break;
                        case 1:
                            v2 = exp.Calc();
                            if (num == 2) {
                                return new TupleValue2(v1, v2);
                            }
                            break;
                        case 2:
                            v3 = exp.Calc();
                            if (num == 3) {
                                return new TupleValue3(v1, v2, v3);
                            }
                            break;
                        case 3:
                            v4 = exp.Calc();
                            if (num == 4) {
                                return new TupleValue4(v1, v2, v3, v4);
                            }
                            break;
                        case 4:
                            v5 = exp.Calc();
                            if (num == 5) {
                                return new TupleValue5(v1, v2, v3, v4, v5);
                            }
                            break;
                        case 5:
                            v6 = exp.Calc();
                            if (num == 6) {
                                return new TupleValue6(v1, v2, v3, v4, v5, v6);
                            }
                            break;
                        case 6:
                            v7 = exp.Calc();
                            if (num == 7) {
                                return new TupleValue7(v1, v2, v3, v4, v5, v6, v7);
                            }
                            break;
                        case 7:
                            if (num == 8) {
                                v8 = exp.Calc();
                                return new TupleValue8(v1, v2, v3, v4, v5, v6, v7, Tuple.Create(v8));
                            }
                            else {
                                var tuple = PackValues(start + 7);
                                return new TupleValue8(v1, v2, v3, v4, v5, v6, v7, Tuple.Create(tuple));
                            }
                    }
                }
                return BoxedValue.NullObject;
            }

            private List<IExpression> m_Expressions = new List<IExpression>();
        }
        internal sealed class TupleSetExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                var val = m_Op.Calc();
                bool success = true;
                var setVars = new Dictionary<string, BoxedValue>();
                MatchRecursively(ref success, setVars, val, m_VarIds, 0);
                if (success) {
                    foreach (var pair in setVars) {
                        Calculator.SetVariable(pair.Key, pair.Value);
                    }
                }
                return BoxedValue.FromBool(success);
            }
            protected override bool Load(Dsl.FunctionData callData)
            {
                Dsl.ISyntaxComponent param1 = callData.GetParam(0);
                var vars = param1 as Dsl.FunctionData;
                if (null != vars) {
                    LoadRecursively(vars, m_VarIds);
                }
                Dsl.ISyntaxComponent param2 = callData.GetParam(1);
                m_Op = Calculator.Load(param2);
                return true;
            }
            private void LoadRecursively(Dsl.FunctionData vars, List<ValueTuple<string, int>> varIds)
            {
                int num = vars.GetParamNum();
                for (int i = 0; i < num; ++i) {
                    var p = vars.GetParam(i);
                    var pvd = p as Dsl.ValueData;
                    var pfd = p as Dsl.FunctionData;
                    if (null != pvd) {
                        varIds.Add(ValueTuple.Create(pvd.GetId(), -1));
                    }
                    else if (null != pfd && !pfd.HaveId()) {
                        m_EmbeddedVars.Add(new List<ValueTuple<string, int>>());
                        int index = m_EmbeddedVars.Count - 1;
                        varIds.Add(ValueTuple.Create(string.Empty, index));
                        LoadRecursively(pfd, m_EmbeddedVars[index]);
                    }
                    else {
                        Calculator.Log("invalid tuple member {0}. code:{1} line:{2}", i, p.ToScriptString(false, Dsl.DelimiterInfo.Default), p.GetLine());
                    }
                }
            }
            private void MatchRecursively(ref bool success, Dictionary<string, BoxedValue> setVars, BoxedValue val, List<ValueTuple<string, int>> varIds, int start)
            {
                int num = varIds.Count - start;
                if (num == 1) {
                    //tuple1 may be a single value, in order to use it both for tuple1 and for normal parentheses usage
                    if (val.IsTuple) {
                        var tuple1 = val.GetTuple1();
                        if (null != tuple1) {
                            val = tuple1.Item1;
                        }
                    }
                    MatchItem(ref success, setVars, varIds[start], val);
                }
                else {
                    switch (val.Type) {
                        case BoxedValue.c_Tuple2Type:
                            if (num == 2) {
                                var tuple = val.GetTuple2();
                                MatchItem(ref success, setVars, varIds[start + 0], tuple.Item1);
                                MatchItem(ref success, setVars, varIds[start + 1], tuple.Item2);
                            }
                            else {
                                success = false;
                            }
                            break;
                        case BoxedValue.c_Tuple3Type:
                            if (num == 3) {
                                var tuple = val.GetTuple3();
                                MatchItem(ref success, setVars, varIds[start + 0], tuple.Item1);
                                MatchItem(ref success, setVars, varIds[start + 1], tuple.Item2);
                                MatchItem(ref success, setVars, varIds[start + 2], tuple.Item3);
                            }
                            else {
                                success = false;
                            }
                            break;
                        case BoxedValue.c_Tuple4Type:
                            if (num == 4) {
                                var tuple = val.GetTuple4();
                                MatchItem(ref success, setVars, varIds[start + 0], tuple.Item1);
                                MatchItem(ref success, setVars, varIds[start + 1], tuple.Item2);
                                MatchItem(ref success, setVars, varIds[start + 2], tuple.Item3);
                                MatchItem(ref success, setVars, varIds[start + 3], tuple.Item4);
                            }
                            else {
                                success = false;
                            }
                            break;
                        case BoxedValue.c_Tuple5Type:
                            if (num == 5) {
                                var tuple = val.GetTuple5();
                                MatchItem(ref success, setVars, varIds[start + 0], tuple.Item1);
                                MatchItem(ref success, setVars, varIds[start + 1], tuple.Item2);
                                MatchItem(ref success, setVars, varIds[start + 2], tuple.Item3);
                                MatchItem(ref success, setVars, varIds[start + 3], tuple.Item4);
                                MatchItem(ref success, setVars, varIds[start + 4], tuple.Item5);
                            }
                            else {
                                success = false;
                            }
                            break;
                        case BoxedValue.c_Tuple6Type:
                            if (num == 6) {
                                var tuple = val.GetTuple6();
                                MatchItem(ref success, setVars, varIds[start + 0], tuple.Item1);
                                MatchItem(ref success, setVars, varIds[start + 1], tuple.Item2);
                                MatchItem(ref success, setVars, varIds[start + 2], tuple.Item3);
                                MatchItem(ref success, setVars, varIds[start + 3], tuple.Item4);
                                MatchItem(ref success, setVars, varIds[start + 4], tuple.Item5);
                                MatchItem(ref success, setVars, varIds[start + 5], tuple.Item6);
                            }
                            else {
                                success = false;
                            }
                            break;
                        case BoxedValue.c_Tuple7Type:
                            if (num == 7) {
                                var tuple = val.GetTuple7();
                                MatchItem(ref success, setVars, varIds[start + 0], tuple.Item1);
                                MatchItem(ref success, setVars, varIds[start + 1], tuple.Item2);
                                MatchItem(ref success, setVars, varIds[start + 2], tuple.Item3);
                                MatchItem(ref success, setVars, varIds[start + 3], tuple.Item4);
                                MatchItem(ref success, setVars, varIds[start + 4], tuple.Item5);
                                MatchItem(ref success, setVars, varIds[start + 5], tuple.Item6);
                                MatchItem(ref success, setVars, varIds[start + 6], tuple.Item7);
                            }
                            else {
                                success = false;
                            }
                            break;
                        case BoxedValue.c_Tuple8Type:
                            if (num >= 8) {
                                var tuple = val.GetTuple8();
                                MatchItem(ref success, setVars, varIds[start + 0], tuple.Item1);
                                MatchItem(ref success, setVars, varIds[start + 1], tuple.Item2);
                                MatchItem(ref success, setVars, varIds[start + 2], tuple.Item3);
                                MatchItem(ref success, setVars, varIds[start + 3], tuple.Item4);
                                MatchItem(ref success, setVars, varIds[start + 4], tuple.Item5);
                                MatchItem(ref success, setVars, varIds[start + 5], tuple.Item6);
                                MatchItem(ref success, setVars, varIds[start + 6], tuple.Item7);
                                MatchRecursively(ref success, setVars, tuple.Rest.Item1, varIds, start + 7);
                            }
                            else {
                                success = false;
                            }
                            break;
                    }
                }
            }
            private void MatchItem(ref bool success, Dictionary<string, BoxedValue> setVars, ValueTuple<string, int> var, BoxedValue val)
            {
                string varId = var.Item1;
                if (string.IsNullOrEmpty(varId)) {
                    int index = var.Item2;
                    if (index >= 0 && index < m_EmbeddedVars.Count) {
                        var newVarIds = m_EmbeddedVars[index];
                        MatchRecursively(ref success, setVars, val, newVarIds, 0);
                    }
                    else {
                        success = false;
                    }
                }
                else {
                    setVars[varId] = val;
                }
            }

            private List<ValueTuple<string, int>> m_VarIds = new List<ValueTuple<string, int>>();
            private List<List<ValueTuple<string, int>>> m_EmbeddedVars = new List<List<ValueTuple<string, int>>>();
            private IExpression m_Op = null!;
        }
        internal sealed class FormatExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                BoxedValue v = 0;
                string fmt = string.Empty;
                ArrayList al = new ArrayList();
                for (int ix = 0; ix < m_Expressions.Count; ++ix) {
                    var exp = m_Expressions[ix];
                    v = exp.Calc();
                    if (ix == 0)
                        fmt = v.AsString;
                    else
                        al.Add(v.GetObject());
                }
                v = string.Format(fmt, al.ToArray());
                return v;
            }
            protected override bool Load(Dsl.FunctionData callData)
            {
                for (int i = 0; i < callData.GetParamNum(); ++i) {
                    Dsl.ISyntaxComponent param = callData.GetParam(i);
                    m_Expressions.Add(Calculator.Load(param));
                }
                return true;
            }

            private List<IExpression> m_Expressions = new List<IExpression>();
        }
        internal sealed class GetTypeAssemblyNameExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                var ret = BoxedValue.NullObject;
                if (m_Expressions.Count >= 1) {
                    var obj = m_Expressions[0].Calc().GetObject();
                    try {
                        if (null != obj) {
                            ret = obj.GetType().AssemblyQualifiedName;
                        }
                        else {
                            ret = "(null)";
                        }
                    }
                    catch (Exception ex) {
                        Calculator.Log("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
                    }
                }
                return ret;
            }
            protected override bool Load(Dsl.FunctionData callData)
            {
                for (int i = 0; i < callData.GetParamNum(); ++i) {
                    Dsl.ISyntaxComponent param = callData.GetParam(i);
                    m_Expressions.Add(Calculator.Load(param));
                }
                return true;
            }

            private List<IExpression> m_Expressions = new List<IExpression>();
        }
        internal sealed class GetTypeFullNameExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                var ret = BoxedValue.NullObject;
                if (m_Expressions.Count >= 1) {
                    var obj = m_Expressions[0].Calc().GetObject();
                    try {
                        if (null != obj) {
                            ret = obj.GetType().FullName;
                        }
                        else {
                            ret = "(null)";
                        }
                    }
                    catch (Exception ex) {
                        Calculator.Log("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
                    }
                }
                return ret;
            }
            protected override bool Load(Dsl.FunctionData callData)
            {
                for (int i = 0; i < callData.GetParamNum(); ++i) {
                    Dsl.ISyntaxComponent param = callData.GetParam(i);
                    m_Expressions.Add(Calculator.Load(param));
                }
                return true;
            }

            private List<IExpression> m_Expressions = new List<IExpression>();
        }
        internal sealed class GetTypeNameExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                var ret = BoxedValue.NullObject;
                if (m_Expressions.Count >= 1) {
                    var obj = m_Expressions[0].Calc().GetObject();
                    try {
                        if (null != obj) {
                            ret = obj.GetType().Name;
                        }
                        else {
                            ret = "(null)";
                        }
                    }
                    catch (Exception ex) {
                        Calculator.Log("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
                    }
                }
                return ret;
            }
            protected override bool Load(Dsl.FunctionData callData)
            {
                for (int i = 0; i < callData.GetParamNum(); ++i) {
                    Dsl.ISyntaxComponent param = callData.GetParam(i);
                    m_Expressions.Add(Calculator.Load(param));
                }
                return true;
            }

            private List<IExpression> m_Expressions = new List<IExpression>();
        }
        internal sealed class GetTypeExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                var ret = BoxedValue.NullObject;
                if (m_Expressions.Count >= 1) {
                    string type = m_Expressions[0].Calc().AsString;
                    try {
                        var r = Type.GetType(type);
                        if (null == r) {
                            Calculator.Log("null == Type.GetType({0})", type);
                        }
                        else {
                            ret = BoxedValue.FromObject(r);
                        }
                    }
                    catch (Exception ex) {
                        Calculator.Log("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
                    }
                }
                return ret;
            }
            protected override bool Load(Dsl.FunctionData callData)
            {
                for (int i = 0; i < callData.GetParamNum(); ++i) {
                    Dsl.ISyntaxComponent param = callData.GetParam(i);
                    m_Expressions.Add(Calculator.Load(param));
                }
                return true;
            }

            private List<IExpression> m_Expressions = new List<IExpression>();
        }
        internal sealed class ChangeTypeExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                var ret = BoxedValue.NullObject;
                if (m_Expressions.Count >= 2) {
                    var obj = m_Expressions[0].Calc();
                    string type = m_Expressions[1].Calc().AsString;
                    try {
                        string str = obj.AsString;
                        if (obj.IsString) {
                            if (0 == type.CompareTo("sbyte")) {
                                ret = CastTo<sbyte>(str);
                            }
                            else if (0 == type.CompareTo("byte")) {
                                ret = CastTo<byte>(str);
                            }
                            else if (0 == type.CompareTo("short")) {
                                ret = CastTo<short>(str);
                            }
                            else if (0 == type.CompareTo("ushort")) {
                                ret = CastTo<ushort>(str);
                            }
                            else if (0 == type.CompareTo("int")) {
                                ret = CastTo<int>(str);
                            }
                            else if (0 == type.CompareTo("uint")) {
                                ret = CastTo<uint>(str);
                            }
                            else if (0 == type.CompareTo("long")) {
                                ret = CastTo<long>(str);
                            }
                            else if (0 == type.CompareTo("ulong")) {
                                ret = CastTo<ulong>(str);
                            }
                            else if (0 == type.CompareTo("float")) {
                                ret = CastTo<float>(str);
                            }
                            else if (0 == type.CompareTo("double")) {
                                ret = CastTo<double>(str);
                            }
                            else if (0 == type.CompareTo("string")) {
                                ret = str;
                            }
                            else if (0 == type.CompareTo("bool")) {
                                ret = CastTo<bool>(str);
                            }
                            else {
                                Type? t = Type.GetType(type);
                                if (null != t) {
                                    ret = BoxedValue.FromObject(CastTo(t, str));
                                }
                                else {
                                    Calculator.Log("null == Type.GetType({0})", type);
                                }
                            }
                        }
                        else {
                            if (0 == type.CompareTo("sbyte")) {
                                ret = obj.GetSByte();
                            }
                            else if (0 == type.CompareTo("byte")) {
                                ret = obj.GetByte();
                            }
                            else if (0 == type.CompareTo("short")) {
                                ret = obj.GetShort();
                            }
                            else if (0 == type.CompareTo("ushort")) {
                                ret = obj.GetUShort();
                            }
                            else if (0 == type.CompareTo("int")) {
                                ret = obj.GetInt();
                            }
                            else if (0 == type.CompareTo("uint")) {
                                ret = obj.GetUInt();
                            }
                            else if (0 == type.CompareTo("long")) {
                                ret = obj.GetLong();
                            }
                            else if (0 == type.CompareTo("ulong")) {
                                ret = obj.GetULong();
                            }
                            else if (0 == type.CompareTo("float")) {
                                ret = obj.GetFloat();
                            }
                            else if (0 == type.CompareTo("double")) {
                                ret = obj.GetDouble();
                            }
                            else if (0 == type.CompareTo("string")) {
                                ret = obj.GetString();
                            }
                            else if (0 == type.CompareTo("bool")) {
                                ret = obj.GetBool();
                            }
                            else {
                                Type? t = Type.GetType(type);
                                if (null != t) {
                                    ret = BoxedValue.FromObject(obj.CastTo(t));
                                }
                                else {
                                    Calculator.Log("null == Type.GetType({0})", type);
                                }
                            }
                        }
                    }
                    catch (Exception ex) {
                        Calculator.Log("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
                    }
                }
                return ret;
            }
            protected override bool Load(Dsl.FunctionData callData)
            {
                for (int i = 0; i < callData.GetParamNum(); ++i) {
                    Dsl.ISyntaxComponent param = callData.GetParam(i);
                    m_Expressions.Add(Calculator.Load(param));
                }
                return true;
            }

            private List<IExpression> m_Expressions = new List<IExpression>();
        }
        internal sealed class ParseEnumExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                var ret = BoxedValue.NullObject;
                if (m_Expressions.Count >= 2) {
                    string type = m_Expressions[0].Calc().AsString;
                    string val = m_Expressions[1].Calc().AsString;
                    try {
                        Type? t = Type.GetType(type);
                        if (null != t) {
                            ret = BoxedValue.FromObject(Enum.Parse(t, val, true));
                        }
                        else {
                            Calculator.Log("null == Type.GetType({0})", type);
                        }
                    }
                    catch (Exception ex) {
                        Calculator.Log("Exception:{0}\n{1}", ex.Message, ex.StackTrace);
                    }
                }
                return ret;
            }
            protected override bool Load(Dsl.FunctionData callData)
            {
                for (int i = 0; i < callData.GetParamNum(); ++i) {
                    Dsl.ISyntaxComponent param = callData.GetParam(i);
                    m_Expressions.Add(Calculator.Load(param));
                }
                return true;
            }

            private List<IExpression> m_Expressions = new List<IExpression>();
        }
        internal sealed class IsNullExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                var ret = BoxedValue.NullObject;
                if (m_Expressions.Count >= 1) {
                    var obj = m_Expressions[0].Calc();
                    ret = obj.IsNullObject;
                }
                return ret;
            }
            protected override bool Load(Dsl.FunctionData callData)
            {
                for (int i = 0; i < callData.GetParamNum(); ++i) {
                    Dsl.ISyntaxComponent param = callData.GetParam(i);
                    m_Expressions.Add(Calculator.Load(param));
                }
                return true;
            }

            private List<IExpression> m_Expressions = new List<IExpression>();
        }
        internal sealed class NullExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                var ret = BoxedValue.NullObject;
                return ret;
            }
            protected override bool Load(Dsl.FunctionData callData)
            {
                return true;
            }
        }
        internal sealed class EqualsNullExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                var ret = BoxedValue.NullObject;
                if (m_Expressions.Count >= 1) {
                    var obj = m_Expressions[0].Calc();
                    ret = object.Equals(null, obj.ObjectVal);
                }
                return ret;
            }
            protected override bool Load(Dsl.FunctionData callData)
            {
                for (int i = 0; i < callData.GetParamNum(); ++i) {
                    Dsl.ISyntaxComponent param = callData.GetParam(i);
                    m_Expressions.Add(Calculator.Load(param));
                }
                return true;
            }

            private List<IExpression> m_Expressions = new List<IExpression>();
        }
        internal sealed class DotnetLoadExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: dotnet_load(dll_path)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    string path = operands[0].AsString;
                    if (!string.IsNullOrEmpty(path) && File.Exists(path)) {
                        r = BoxedValue.FromObject(Assembly.LoadFile(path));
                    }
                }
                return r;
            }
        }
        internal sealed class DotnetNewExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: dotnet_new(assembly,type_name,arg1,arg2,...)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 2) {
                    var assem = operands[0].As<Assembly>();
                    string typeName = operands[1].AsString;
                    if (null != assem && !string.IsNullOrEmpty(typeName)) {
                        var al = new ArrayList();
                        for (int i = 2; i < operands.Count; ++i) {
                            al.Add(operands[i].GetObject());
                        }
                        r = BoxedValue.FromObject(assem.CreateInstance(typeName, false, BindingFlags.CreateInstance, null, al.ToArray()!, System.Globalization.CultureInfo.CurrentCulture, null));
                    }
                }
                return r;
            }
        }
        internal sealed class SubstringExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 1 || operands.Count > 3) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: string_substring(str[,start,len]) function, aliased as string_substr|substr");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    string str = operands[0].GetString();
                    if (null != str) {
                        int start = 0;
                        int len = str.Length;
                        if (operands.Count >= 2) {
                            start = operands[1].GetInt();
                            len -= start;
                        }
                        if (operands.Count >= 3) {
                            len = operands[2].GetInt();
                        }
                        r = str.Substring(start, len);
                    }
                }
                return r;
            }
        }
        internal sealed class NewStringBuilderExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 0) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: new_string_builder()");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 0) {
                    r = BoxedValue.FromObject(new StringBuilder());
                }
                return r;
            }
        }
        internal sealed class AppendFormatExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: append_format(sb,fmt,arg1,arg2,...)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 2) {
                    var sb = operands[0].As<StringBuilder>();
                    string fmt = string.Empty;
                    var al = new ArrayList();
                    for (int i = 1; i < operands.Count; ++i) {
                        if (i == 1)
                            fmt = operands[i].AsString;
                        else
                            al.Add(operands[i].GetObject());
                    }
                    if (null != sb && !string.IsNullOrEmpty(fmt)) {
                        sb.AppendFormat(fmt, al.ToArray());
                        r = BoxedValue.FromObject(sb);
                    }
                }
                return r;
            }
        }
        internal sealed class AppendFormatLineExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: append_format_line(sb,fmt,arg1,arg2,...)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var sb = operands[0].As<StringBuilder>();
                    string fmt = string.Empty;
                    var al = new ArrayList();
                    for (int i = 1; i < operands.Count; ++i) {
                        if (i == 1)
                            fmt = operands[i].AsString;
                        else
                            al.Add(operands[i].GetObject());
                    }
                    if (null != sb) {
                        if (string.IsNullOrEmpty(fmt)) {
                            sb.AppendLine();
                        }
                        else {
                            sb.AppendFormat(fmt, al.ToArray());
                            sb.AppendLine();
                        }
                        r = BoxedValue.FromObject(sb);
                    }
                }
                return r;
            }
        }
        internal sealed class StringBuilderToStringExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: string_builder_to_string(sb)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var sb = operands[0].As<StringBuilder>();
                    if (null != sb) {
                        r = sb.ToString();
                    }
                }
                return r;
            }
        }
        internal sealed class StringJoinExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 2) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: string_join(sep,list), aliased as join_string|join");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 2) {
                    string sep = operands[0].AsString;
                    IList list = operands[1].As<IList>();
                    if (sep != null && list != null) {
                        string[] strs = new string[list.Count];
                        for (int i = 0; i < list.Count; i++) {
                            var elem = list[i];
                            if (elem != null) {
                                var s = elem.ToString();
                                if (s == null) {
                                    strs[i] = string.Empty;
                                }
                                else {
                                    strs[i] = s;
                                }
                            }
                        }
                        r = string.Join(sep, strs);
                    }
                }
                return r;
            }
        }
        internal sealed class StringSplitExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 2) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: string_split(str, sep) or string_split(str,sep_list), aliased as split_string|split");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 2) {
                    string str = operands[0].AsString;
                    string sepStr = operands[1].AsString;
                    if (!string.IsNullOrEmpty(sepStr)) {
                        r = BoxedValue.FromObject(str.Split(sepStr, StringSplitOptions.RemoveEmptyEntries));
                    }
                    else {
                        IList seps = operands[1].As<IList>();
                        if (str != null && seps != null) {
                            char[] cs = new char[seps.Count];
                            for (int i = 0; i < seps.Count; i++) {
                                var _sep = seps[i];
                                if (null != _sep) {
                                    string? sep = _sep.ToString();
                                    if (sep?.Length > 0) {
                                        cs[i] = sep[0];
                                    }
                                    else {
                                        cs[i] = '\0';
                                    }
                                }
                            }
                            r = BoxedValue.FromObject(str.Split(cs, StringSplitOptions.RemoveEmptyEntries));
                        }
                    }
                }
                return r;
            }
        }
        internal sealed class StringTrimExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: string_trim(str), aliased as trim_string|trim");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var str = operands[0].AsString;
                    if (str != null) {
                        r = str.Trim();
                    }
                }
                return r;
            }
        }
        internal sealed class StringTrimStartExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: string_trim_start(str), aliased as trim_start_string|trim_start");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var str = operands[0].AsString;
                    if (str != null) {
                        r = str.TrimStart();
                    }
                }
                return r;
            }
        }
        internal sealed class StringTrimEndExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: string_trim_end(str), aliased as trim_end_string|trim_end");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var str = operands[0].AsString;
                    if (str != null) {
                        r = str.TrimEnd();
                    }
                }
                return r;
            }
        }
        internal sealed class StringToLowerExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: string_to_lower(str)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var str = operands[0].AsString;
                    if (str != null) {
                        r = str.ToLower();
                    }
                }
                return r;
            }
        }
        internal sealed class StringToUpperExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: string_to_upper(str)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var str = operands[0].AsString;
                    if (str != null) {
                        r = str.ToUpper();
                    }
                }
                return r;
            }
        }
        internal sealed class StringReplaceExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count < 3 || operands.Count > 4) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: string_replace(str, substr, replace[, exactMatch]), aliased as replace_string|replace");
                    return BoxedValue.NullObject;
                }
                var str = operands[0].AsString;
                var key = operands[1].AsString;
                var val = operands[2].AsString;
                bool exactMatch = operands.Count > 3 ? operands[3].GetBool() : false;
                if (null != str && null != key && null != val) {
                    if (string.IsNullOrEmpty(key)) {
                        AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("The substr cannot be empty !!!");
                        return BoxedValue.From(str);
                    }
                    // Level 1: Exact match (no modification to search string)
                    if (str.Contains(key)) {
                        r = str.Replace(key, val);
                    }
                    else if (exactMatch) {
                        if (File.Exists(str.Trim())) {
                            AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("string_replace: used for strings, not files.");
                        }
                        else {
                            AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("string_replace: substr not found (exact match)");
                        }
                        return BoxedValue.From(str);
                    }
                    else {
                        // Level 2: Trimmed match
                        var trimedKey = key.Trim();
                        var trimedVal = val.Trim();
                        if (str.Contains(trimedKey)) {
                            r = str.Replace(trimedKey, trimedVal);
                        }
                        else {
                            // Level 3: Normalized whitespace matching (DiffOps fallback)
                            var normResult = CefDotnetApp.AgentCore.Core.DiffOperations.ReplaceFullLinesText(str, key, val, true);
                            if (normResult.Success) {
                                return BoxedValue.From(normResult.ResultContent);
                            }
                            if (File.Exists(str.Trim())) {
                                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("string_replace: used for strings, not files.");
                            }
                            else {
                                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("string_replace: substr not found");
                            }
                            return BoxedValue.From(str);
                        }
                    }
                }
                else {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("string_replace: str or substr or replace is null !!!");
                    r = str;
                }
                return r;
            }
        }
        internal sealed class StringReplaceCharExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 3) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: string_replace_char(str,key,char_as_str)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 3) {
                    var str = operands[0].AsString;
                    var key = operands[1].AsString;
                    var val = operands[2].AsString;
                    if (null != str && !string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(val)) {
                        if (str.IndexOf(key[0]) >= 0) {
                            r = str.Replace(key[0], val[0]);
                        }
                        else if (File.Exists(str.Trim())) {
                            AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("string_replace_char: used for strings, not files.");
                        }
                        else {
                            AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("string_replace_char: char not found");
                        }
                    }
                }
                return r;
            }
        }
        internal sealed class MakeStringExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                List<char> chars = new List<char>();
                for (int i = 0; i < operands.Count; ++i) {
                    var v = operands[i];
                    var str = v.AsString;
                    if (null != str) {
                        char c = '\0';
                        if (str.Length > 0) {
                            c = str[0];
                        }
                        chars.Add(c);
                    }
                    else {
                        char c = operands[i].GetChar();
                        chars.Add(c);
                    }
                }
                return new String(chars.ToArray());
            }
        }
        internal sealed class StringContainsExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: string_contains(str,str_or_list_1,str_or_list_2,...), aliased as contains");
                    return BoxedValue.NullObject;
                }
                bool r = false;
                if (operands.Count >= 2) {
                    string str = operands[0].AsString;
                    if (null != str) {
                        r = true;
                        for (int i = 1; i < operands.Count; ++i) {
                            var list = operands[i].As<IList>();
                            if (null != list) {
                                foreach (var o in list) {
                                    var key = o as string;
                                    if (!string.IsNullOrEmpty(key) && !str.Contains(key)) {
                                        return false;
                                    }
                                }
                            }
                            else {
                                var key = operands[i].AsString;
                                if (!string.IsNullOrEmpty(key) && !str.Contains(key)) {
                                    return false;
                                }
                            }
                        }
                    }
                }
                return r;
            }
        }
        internal sealed class StringNotContainsExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: string_not_contains(str,str_or_list_1,str_or_list_2,...), aliased as not_contains");
                    return BoxedValue.NullObject;
                }
                bool r = false;
                if (operands.Count >= 2) {
                    string str = operands[0].AsString;
                    if (null != str) {
                        r = true;
                        for (int i = 1; i < operands.Count; ++i) {
                            var list = operands[i].As<IList>();
                            if (null != list) {
                                foreach (var o in list) {
                                    var key = o as string;
                                    if (!string.IsNullOrEmpty(key) && str.Contains(key)) {
                                        return false;
                                    }
                                }
                            }
                            else {
                                var key = operands[i].AsString;
                                if (!string.IsNullOrEmpty(key) && str.Contains(key)) {
                                    return false;
                                }
                            }
                        }
                    }
                }
                return r;
            }
        }
        internal sealed class StringContainsAnyExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: string_contains_any(str,str_or_list_1,str_or_list_2,...), aliased as contains_any");
                    return BoxedValue.NullObject;
                }
                bool r = false;
                if (operands.Count >= 2) {
                    string str = operands[0].AsString;
                    if (null != str) {
                        r = true;
                        for (int i = 1; i < operands.Count; ++i) {
                            var list = operands[i].As<IList>();
                            if (null != list) {
                                foreach (var o in list) {
                                    var key = o as string;
                                    if (!string.IsNullOrEmpty(key)) {
                                        if (str.Contains(key)) {
                                            return true;
                                        }
                                        else {
                                            r = false;
                                        }
                                    }
                                }
                            }
                            else {
                                var key = operands[i].AsString;
                                if (!string.IsNullOrEmpty(key)) {
                                    if (str.Contains(key)) {
                                        return true;
                                    }
                                    else {
                                        r = false;
                                    }
                                }
                            }
                        }
                    }
                }
                return r;
            }
        }
        internal sealed class StringNotContainsAnyExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: string_not_contains_any(str,str_or_list_1,str_or_list_2,...), aliased as not_contains_any");
                    return BoxedValue.NullObject;
                }
                bool r = false;
                if (operands.Count >= 2) {
                    string str = operands[0].AsString;
                    if (null != str) {
                        r = true;
                        for (int i = 1; i < operands.Count; ++i) {
                            var list = operands[i].As<IList>();
                            if (null != list) {
                                foreach (var o in list) {
                                    var key = o as string;
                                    if (!string.IsNullOrEmpty(key)) {
                                        if (!str.Contains(key)) {
                                            return true;
                                        }
                                        else {
                                            r = false;
                                        }
                                    }
                                }
                            }
                            else {
                                var key = operands[i].AsString;
                                if (!string.IsNullOrEmpty(key)) {
                                    if (!str.Contains(key)) {
                                        return true;
                                    }
                                    else {
                                        r = false;
                                    }
                                }
                            }
                        }
                    }
                }
                return r;
            }
        }
        internal sealed class Str2IntExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: string_to_int(str), aliased as str_to_int");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var str = operands[0].AsString;
                    if (null != str) {
                        int v;
                        if (int.TryParse(str, System.Globalization.NumberStyles.Number, null, out v)) {
                            r = v;
                        }
                    }
                }
                return r;
            }
        }
        internal sealed class Str2UintExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: string_to_uint(str), aliased as str_to_uint");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var str = operands[0].AsString;
                    if (null != str) {
                        uint v;
                        if (uint.TryParse(str, System.Globalization.NumberStyles.Number, null, out v)) {
                            r = v;
                        }
                    }
                }
                return r;
            }
        }
        internal sealed class Str2LongExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: string_to_long(str), aliased as str_to_long");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var str = operands[0].AsString;
                    if (null != str) {
                        long v;
                        if (long.TryParse(str, System.Globalization.NumberStyles.Number, null, out v)) {
                            r = v;
                        }
                    }
                }
                return r;
            }
        }
        internal sealed class Str2UlongExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: string_to_ulong(str), aliased as str_to_ulong");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var str = operands[0].AsString;
                    if (null != str) {
                        ulong v;
                        if (ulong.TryParse(str, System.Globalization.NumberStyles.Number, null, out v)) {
                            r = v;
                        }
                    }
                }
                return r;
            }
        }
        internal sealed class Str2FloatExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: string_to_float(str), aliased as str_to_float");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var str = operands[0].AsString;
                    if (null != str) {
                        float v;
                        if (float.TryParse(str, System.Globalization.NumberStyles.Float, null, out v)) {
                            r = v;
                        }
                    }
                }
                return r;
            }
        }
        internal sealed class Str2DoubleExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: strint_to_double(str), aliased as str_to_double");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var str = operands[0].AsString;
                    if (null != str) {
                        double v;
                        if (double.TryParse(str, System.Globalization.NumberStyles.Float, null, out v)) {
                            r = v;
                        }
                    }
                }
                return r;
            }
        }
        internal sealed class Hex2IntExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: hex_string_to_int(str), aliased as hex_to_int");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var str = operands[0].AsString;
                    if (null != str) {
                        int v;
                        if (int.TryParse(str, System.Globalization.NumberStyles.HexNumber, null, out v)) {
                            r = v;
                        }
                    }
                }
                return r;
            }
        }
        internal sealed class Hex2UintExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: hex_string_to_uint(str), aliased as hex_to_uint");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var str = operands[0].AsString;
                    if (null != str) {
                        uint v;
                        if (uint.TryParse(str, System.Globalization.NumberStyles.HexNumber, null, out v)) {
                            r = v;
                        }
                    }
                }
                return r;
            }
        }
        internal sealed class Hex2LongExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: hex_string_to_long(str), aliased as hex_to_long");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var str = operands[0].AsString;
                    if (null != str) {
                        long v;
                        if (long.TryParse(str, System.Globalization.NumberStyles.HexNumber, null, out v)) {
                            r = v;
                        }
                    }
                }
                return r;
            }
        }
        internal sealed class Hex2UlongExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: hex_string_to_ulong(str), aliased as hex_to_ulong");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var str = operands[0].AsString;
                    if (null != str) {
                        ulong v;
                        if (ulong.TryParse(str, System.Globalization.NumberStyles.HexNumber, null, out v)) {
                            r = v;
                        }
                    }
                }
                return r;
            }
        }
        internal sealed class DatetimeStrExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count > 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: date_time_string([fmt]), aliased as date_time_str");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var fmt = operands[0].AsString;
                    if (null != fmt) {
                        r = DateTime.Now.ToString(fmt);
                    }
                }
                else {
                    r = DateTime.Now.ToString();
                }
                return r;
            }
        }
        internal sealed class LongDateStrExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 0) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: long_date_string(), aliased as long_date_str");
                    return BoxedValue.NullObject;
                }
                var r = BoxedValue.FromObject(DateTime.Now.ToLongDateString());
                return r;
            }
        }
        internal sealed class LongTimeStrExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 0) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: long_time_string(), aliased as long_time_str");
                    return BoxedValue.NullObject;
                }
                var r = BoxedValue.FromObject(DateTime.Now.ToShortDateString());
                return r;
            }
        }
        internal sealed class ShortDateStrExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 0) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: short_date_string(), aliased as short_date_str");
                    return BoxedValue.NullObject;
                }
                var r = BoxedValue.FromObject(DateTime.Now.ToShortDateString());
                return r;
            }
        }
        internal sealed class ShortTimeStrExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 0) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: short_time_string(), aliased as short_time_str");
                    return BoxedValue.NullObject;
                }
                var r = BoxedValue.FromObject(DateTime.Now.ToShortTimeString());
                return r;
            }
        }
        internal sealed class IsNullOrEmptyExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: is_null_or_empty(str)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var str = operands[0].AsString;
                    r = string.IsNullOrEmpty(str);
                }
                return r;
            }
        }
        internal sealed class ArrayExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                BoxedValue[] r = new BoxedValue[operands.Count];
                for (int i = 0; i < operands.Count; ++i) {
                    r[i] = operands[i];
                }
                return BoxedValue.FromObject(r);
            }
        }
        internal sealed class ToArrayExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: to_array(list)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var list = operands[0];
                    IEnumerable obj = list.As<IEnumerable>();
                    if (null != obj) {
                        List<BoxedValue> al = new List<BoxedValue>();
                        IEnumerator enumer = obj.GetEnumerator();
                        while (enumer.MoveNext()) {
                            var val = BoxedValue.FromObject(enumer.Current);
                            al.Add(val);
                        }
                        r = BoxedValue.FromObject(al.ToArray());
                    }
                }
                return r;
            }
        }
        internal sealed class ListSizeExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: list_count(list), aliased as list_get_count|get_list_count|list_size");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var list = operands[0].As<IList>();
                    if (null != list) {
                        r = list.Count;
                    }
                }
                return r;
            }
        }
        internal sealed class ListExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                BoxedValue r = BoxedValue.NullObject;
                List<BoxedValue> al = new List<BoxedValue>();
                for (int i = 0; i < operands.Count; ++i) {
                    al.Add(operands[i]);
                }
                r = BoxedValue.FromObject(al);
                return r;
            }
        }
        internal sealed class ListGetExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2 || operands.Count > 3) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: list_get(list,index[,defval])");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 2) {
                    var list = operands[0].As<IList>();
                    var index = operands[1].GetInt();
                    var defVal = BoxedValue.NullObject;
                    if (operands.Count >= 3) {
                        defVal = operands[2];
                    }
                    if (null != list) {
                        if (index >= 0 && index < list.Count) {
                            r = BoxedValue.FromObject(list[index]);
                        }
                        else {
                            r = defVal;
                        }
                    }
                }
                return r;
            }
        }
        internal sealed class ListSetExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 3) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: list_set(list,index,val)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 3) {
                    var list = operands[0].As<IList>();
                    var index = operands[1].GetInt();
                    var val = operands[2];
                    if (null != list && list is List<BoxedValue> bvList) {
                        if (index >= 0 && index < list.Count) {
                            bvList[index] = val;
                        }
                    }
                    else if (null != list) {
                        if (index >= 0 && index < list.Count) {
                            list[index] = val.GetObject();
                        }
                    }
                }
                return r;
            }
        }
        internal sealed class ListIndexOfExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 2) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: list_index_of(list,val), aliased as list_indexof");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 2) {
                    var list = operands[0].As<IList>();
                    var val = operands[1];
                    if (null != list && list is List<BoxedValue> bvList) {
                        r = bvList.IndexOf(val);
                    }
                    else if (null != list) {
                        r = list.IndexOf(val.GetObject());
                    }
                }
                return r;
            }
        }
        internal sealed class ListAddExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 2) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: list_add(list,val)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 2) {
                    var list = operands[0].As<IList>();
                    var val = operands[1];
                    if (null != list && list is List<BoxedValue> bvList) {
                        bvList.Add(val);
                    }
                    else if (null != list) {
                        list.Add(val.GetObject());
                    }
                }
                return r;
            }
        }
        internal sealed class ListRemoveExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 2) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: list_remove(list,val)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 2) {
                    var list = operands[0].As<IList>();
                    var val = operands[1];
                    if (null != list && list is List<BoxedValue> bvList) {
                        bvList.Remove(val);
                    }
                    else if (null != list) {
                        list.Remove(val.GetObject());
                    }
                }
                return r;
            }
        }
        internal sealed class ListInsertExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 3) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: list_insert(list,index,val)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 3) {
                    var list = operands[0].As<IList>();
                    var index = operands[1].GetInt();
                    var val = operands[2];
                    if (null != list && list is List<BoxedValue> bvList) {
                        bvList.Insert(index, val);
                    }
                    else if (null != list) {
                        list.Insert(index, val.GetObject());
                    }
                }
                return r;
            }
        }
        internal sealed class ListRemoveAtExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 2) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: list_remove_at(list,index)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 2) {
                    var list = operands[0].As<IList>();
                    var index = operands[1].GetInt();
                    if (null != list) {
                        list.RemoveAt(index);
                    }
                }
                return r;
            }
        }
        internal sealed class ListClearExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: list_clear(list)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var list = operands[0].As<IList>();
                    if (null != list) {
                        list.Clear();
                    }
                }
                return r;
            }
        }
        internal sealed class ListSplitExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 2) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: list_split(list,ct) api, return list of list");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 2) {
                    var enumer = operands[0].As<IEnumerable>();
                    var ct = operands[1].GetInt();
                    if (null != enumer && enumer is List<BoxedValue> bvList) {
                        var e = bvList.GetEnumerator();
                        List<List<BoxedValue>> al = new List<List<BoxedValue>>();
                        List<BoxedValue> arr = new List<BoxedValue>();
                        int ix = 0;
                        while (e.MoveNext()) {
                            if (ix < ct) {
                                arr.Add(e.Current);
                                ++ix;
                            }
                            if (ix >= ct) {
                                al.Add(arr);
                                arr = new List<BoxedValue>();
                                ix = 0;
                            }
                        }
                        if (arr.Count > 0) {
                            al.Add(arr);
                        }
                        r = BoxedValue.FromObject(al);
                    }
                    else if (null != enumer) {
                        var e = enumer.GetEnumerator();
                        if (null != e) {
                            List<List<BoxedValue>> al = new List<List<BoxedValue>>();
                            List<BoxedValue> arr = new List<BoxedValue>();
                            int ix = 0;
                            while (e.MoveNext()) {
                                if (ix < ct) {
                                    arr.Add(BoxedValue.FromObject(e.Current));
                                    ++ix;
                                }
                                if (ix >= ct) {
                                    al.Add(arr);
                                    arr = new List<BoxedValue>();
                                    ix = 0;
                                }
                            }
                            if (arr.Count > 0) {
                                al.Add(arr);
                            }
                            r = BoxedValue.FromObject(al);
                        }
                    }
                }
                return r;
            }
        }
        internal sealed class HashtableSizeExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: hashtable_count(hash), aliased as hashtable_get_count|get_hashtable_count|hashtable_size");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var dict = operands[0].As<IDictionary>();
                    if (null != dict) {
                        r = dict.Count;
                    }
                }
                return r;
            }
        }
        internal sealed class HashtableExp : AbstractExpression
        {
            protected override BoxedValue DoCalc()
            {
                BoxedValue r = BoxedValue.NullObject;
                var dict = new Dictionary<BoxedValue, BoxedValue>();
                for (int i = 0; i < m_Expressions.Count - 1; i += 2) {
                    var key = m_Expressions[i].Calc();
                    var val = m_Expressions[i + 1].Calc();
                    dict.Add(key, val);
                }
                r = BoxedValue.FromObject(dict);
                return r;
            }
            protected override bool Load(Dsl.FunctionData funcData)
            {
                for (int i = 0; i < funcData.GetParamNum(); ++i) {
                    Dsl.FunctionData? callData = funcData.GetParam(i) as Dsl.FunctionData;
                    if (null != callData && callData.GetParamNum() == 2) {
                        var expKey = Calculator.Load(callData.GetParam(0));
                        m_Expressions.Add(expKey);
                        var expVal = Calculator.Load(callData.GetParam(1));
                        m_Expressions.Add(expVal);
                    }
                }
                return true;
            }

            private List<IExpression> m_Expressions = new List<IExpression>();
        }
        internal sealed class HashtableGetExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2 || operands.Count > 3) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: hashtable_get(hash,key[,defval])");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 2) {
                    var dict = operands[0].As<IDictionary>();
                    var index = operands[1];
                    var defVal = BoxedValue.NullObject;
                    if (operands.Count >= 3) {
                        defVal = operands[2];
                    }
                    if (null != dict && dict is Dictionary<BoxedValue, BoxedValue> bvDict) {
                        r = bvDict.TryGetValue(index, out var val) ? val : defVal;
                    }
                    else {
                        var indexObj = index.GetObject();
                        if (null != dict && dict.Contains(indexObj)) {
                            r = BoxedValue.FromObject(dict[indexObj]);
                        }
                        else {
                            r = defVal;
                        }
                    }
                }
                return r;
            }
        }
        internal sealed class HashtableSetExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 3) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: hashtable_set(hash,key,val)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 3) {
                    var dict = operands[0].As<IDictionary>();
                    var index = operands[1];
                    var val = operands[2];
                    if (null != dict && dict is Dictionary<BoxedValue, BoxedValue> bvDict) {
                        bvDict[index] = val;
                    }
                    else {
                        var indexObj = index.GetObject();
                        var valObj = val.GetObject();
                        if (null != dict) {
                            dict[indexObj] = valObj;
                        }
                    }
                }
                return r;
            }
        }
        internal sealed class HashtableAddExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 3) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: hashtable_add(hash,key,val)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 3) {
                    var dict = operands[0].As<IDictionary>();
                    var key = operands[1];
                    var val = operands[2];
                    if (null != dict && dict is Dictionary<BoxedValue, BoxedValue> bvDict) {
                        bvDict.Add(key, val);
                    }
                    else {
                        var keyObj = key.GetObject();
                        var valObj = val.GetObject();
                        if (null != dict && null != keyObj) {
                            dict.Add(keyObj, valObj);
                        }
                    }
                }
                return r;
            }
        }
        internal sealed class HashtableRemoveExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 2) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: hashtable_remove(hash,key)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 2) {
                    var dict = operands[0].As<IDictionary>();
                    var key = operands[1];
                    if (null != dict && dict is Dictionary<BoxedValue, BoxedValue> bvDict) {
                        bvDict.Remove(key);
                    }
                    else {
                        var keyObj = key.GetObject();
                        if (null != dict && null != keyObj) {
                            dict.Remove(keyObj);
                        }
                    }
                }
                return r;
            }
        }
        internal sealed class HashtableClearExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: hashtable_clear(hash)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var dict = operands[0].As<IDictionary>();
                    if (null != dict) {
                        dict.Clear();
                    }
                }
                return r;
            }
        }
        internal sealed class HashtableKeysExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: hashtable_keys(hash)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var dict = operands[0].As<IDictionary>();
                    if (null != dict && dict is Dictionary<BoxedValue, BoxedValue> bvDict) {
                        var list = new List<BoxedValue>();
                        list.AddRange(bvDict.Keys);
                        r = BoxedValue.FromObject(list);
                    }
                    else {
                        if (null != dict) {
                            var list = new List<BoxedValue>();
                            foreach (var key in dict.Keys) {
                                list.Add(BoxedValue.FromObject(key));
                            }
                            r = BoxedValue.FromObject(list);
                        }
                    }
                }
                return r;
            }
        }
        internal sealed class HashtableValuesExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: hashtable_values(hash)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var dict = operands[0].As<IDictionary>();
                    if (null != dict && dict is Dictionary<BoxedValue, BoxedValue> bvDict) {
                        var list = new List<BoxedValue>();
                        list.AddRange(bvDict.Values);
                        r = BoxedValue.FromObject(list);
                    }
                    else {
                        if (null != dict) {
                            var list = new List<BoxedValue>();
                            foreach (var val in dict.Values) {
                                list.Add(BoxedValue.FromObject(val));
                            }
                            r = BoxedValue.FromObject(list);
                        }
                    }
                }
                return r;
            }
        }
        internal sealed class ListHashtableExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: list_hashtable(hash) api, return list of pair");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var dict = operands[0].As<IDictionary>();
                    if (null != dict) {
                        var list = new ArrayList();
                        foreach (var pair in dict) {
                            list.Add(pair);
                        }
                        r = list;
                    }
                }
                return r;
            }
        }
        internal sealed class HashtableSplitExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 2) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: hashtable_split(hash,ct) api, return list of hashtable");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 2) {
                    var dict = operands[0].As<IDictionary>();
                    var ct = operands[1].GetInt();
                    if (null != dict && dict is Dictionary<BoxedValue, BoxedValue> bvDict) {
                        var e = bvDict.GetEnumerator();
                        var al = new List<Dictionary<BoxedValue, BoxedValue>>();
                        var ht = new Dictionary<BoxedValue, BoxedValue>();
                        int ix = 0;
                        while (e.MoveNext()) {
                            if (ix < ct) {
                                ht.Add(e.Current.Key, e.Current.Value);
                                ++ix;
                            }
                            if (ix >= ct) {
                                al.Add(ht);
                                ht = new Dictionary<BoxedValue, BoxedValue>();
                                ix = 0;
                            }
                        }
                        if (ht.Count > 0) {
                            al.Add(ht);
                        }
                        r = BoxedValue.FromObject(al);
                    }
                    else if (null != dict) {
                        var e = dict.GetEnumerator();
                        if (null != e) {
                            var al = new List<Dictionary<BoxedValue, BoxedValue>>();
                            var ht = new Dictionary<BoxedValue, BoxedValue>();
                            int ix = 0;
                            while (e.MoveNext()) {
                                if (ix < ct) {
                                    ht.Add(BoxedValue.FromObject(e.Key), BoxedValue.FromObject(e.Value));
                                    ++ix;
                                }
                                if (ix >= ct) {
                                    al.Add(ht);
                                    ht = new Dictionary<BoxedValue, BoxedValue>();
                                    ix = 0;
                                }
                            }
                            if (ht.Count > 0) {
                                al.Add(ht);
                            }
                            r = BoxedValue.FromObject(al);
                        }
                    }
                }
                return r;
            }
        }
        //The stack and queue share the same peek function.
        internal sealed class PeekExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: peek(queue_or_stack)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var stack = operands[0].As<Stack<BoxedValue>>();
                    var queue = operands[0].As<Queue<BoxedValue>>();
                    if (null != stack) {
                        r = stack.Peek();
                    }
                    else if (null != queue) {
                        r = queue.Peek();
                    }
                }
                return r;
            }
        }
        internal sealed class StackSizeExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: stack_count(stack), aliased as stack_get_count|get_stack_count|stack_size");
                    return BoxedValue.NullObject;
                }
                int r = 0;
                if (operands.Count >= 1) {
                    var stack = operands[0].As<Stack<BoxedValue>>();
                    if (null != stack) {
                        r = stack.Count;
                    }
                }
                return r;
            }
        }
        internal sealed class StackExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                BoxedValue r = BoxedValue.NullObject;
                var stack = new Stack<BoxedValue>();
                for (int i = 0; i < operands.Count; ++i) {
                    stack.Push(operands[i]);
                }
                r = BoxedValue.FromObject(stack);
                return r;
            }
        }
        internal sealed class PushExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 2) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: push(stack,v)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 2) {
                    var stack = operands[0].As<Stack<BoxedValue>>();
                    var val = operands[1];
                    if (null != stack) {
                        stack.Push(val);
                    }
                }
                return r;
            }
        }
        internal sealed class PopExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: pop(stack)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var stack = operands[0].As<Stack<BoxedValue>>();
                    if (null != stack) {
                        r = stack.Pop();
                    }
                }
                return r;
            }
        }
        internal sealed class StackClearExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: stack_clear(stack)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var stack = operands[0].As<Stack<BoxedValue>>();
                    if (null != stack) {
                        stack.Clear();
                    }
                }
                return r;
            }
        }
        internal sealed class QueueSizeExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: queue_count(queue), aliased as queue_get_count|get_queue_count|queue_size");
                    return BoxedValue.NullObject;
                }
                int r = 0;
                if (operands.Count >= 1) {
                    var queue = operands[0].As<Queue<BoxedValue>>();
                    if (null != queue) {
                        r = queue.Count;
                    }
                }
                return r;
            }
        }
        internal sealed class QueueExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                BoxedValue r = BoxedValue.NullObject;
                var queue = new Queue<BoxedValue>();
                for (int i = 0; i < operands.Count; ++i) {
                    queue.Enqueue(operands[i]);
                }
                r = BoxedValue.FromObject(queue);
                return r;
            }
        }
        internal sealed class EnqueueExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 2) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: enqueue(queue,v)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 2) {
                    var queue = operands[0].As<Queue<BoxedValue>>();
                    var val = operands[1];
                    if (null != queue) {
                        queue.Enqueue(val);
                    }
                }
                return r;
            }
        }
        internal sealed class DequeueExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: dequeue(queue)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var queue = operands[0].As<Queue<BoxedValue>>();
                    if (null != queue) {
                        r = queue.Dequeue();
                    }
                }
                return r;
            }
        }
        internal sealed class QueueClearExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: queue_clear(queue)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var queue = operands[0].As<Queue<BoxedValue>>();
                    if (null != queue) {
                        queue.Clear();
                    }
                }
                return r;
            }
        }
        internal sealed class SetEnvironmentExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 2) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: set_env(k,v)");
                    return BoxedValue.NullObject;
                }
                var ret = BoxedValue.NullObject;
                if (operands.Count >= 2) {
                    var key = operands[0].AsString;
                    var val = operands[1].AsString;
                    val = Environment.ExpandEnvironmentVariables(val);
                    Environment.SetEnvironmentVariable(key, val);
                }
                return ret;
            }
        }
        internal sealed class GetEnvironmentExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: get_env(k)");
                    return BoxedValue.NullObject;
                }
                string ret = string.Empty;
                if (operands.Count >= 1) {
                    var key = operands[0].AsString;
                    return Environment.GetEnvironmentVariable(key);
                }
                return ret;
            }
        }
        internal sealed class ExpandEnvironmentsExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: expand(str)");
                    return BoxedValue.NullObject;
                }
                string ret = string.Empty;
                if (operands.Count >= 1) {
                    var key = operands[0].AsString;
                    return Environment.ExpandEnvironmentVariables(key);
                }
                return ret;
            }
        }
        internal sealed class EnvironmentsExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                return BoxedValue.FromObject(Environment.GetEnvironmentVariables());
            }
        }
        internal sealed class SetCurrentDirectoryExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: cd(path)");
                    return BoxedValue.NullObject;
                }
                string ret = string.Empty;
                if (operands.Count >= 1) {
                    var dir = operands[0].AsString;
                    string path = Environment.ExpandEnvironmentVariables(dir);
                    if (Path.IsPathRooted(path)) {
                        Environment.CurrentDirectory = path;
                    }
                    else {
                        Environment.CurrentDirectory = Path.Combine(Environment.CurrentDirectory, path);
                    }
                    ret = dir;
                }
                return ret;
            }
        }
        internal sealed class GetCurrentDirectoryExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                return Environment.CurrentDirectory;
            }
        }
        internal sealed class CommandLineExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 0) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: cmd_line()");
                    return BoxedValue.NullObject;
                }
                return Environment.CommandLine;
            }
        }
        internal sealed class CommandLineArgsExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: cmd_line_args(prev_arg) or cmdlineargs() api, first return next arg, second return array of arg");
                    return BoxedValue.NullObject;
                }
                if (operands.Count >= 1) {
                    string name = operands[0].AsString;
                    if (!string.IsNullOrEmpty(name)) {
                        string[] args = System.Environment.GetCommandLineArgs();
                        int suffixIndex = Array.FindIndex(args, item => item == name);
                        if (suffixIndex != -1 && suffixIndex < args.Length - 1) {
                            return args[suffixIndex + 1];
                        }
                    }
                    return string.Empty;
                }
                else {
                    return BoxedValue.FromObject(Environment.GetCommandLineArgs());
                }
            }
        }
        internal sealed class OsExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                return Environment.OSVersion.VersionString;
            }
        }
        internal sealed class OsPlatformExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 0) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: os_platform()");
                    return BoxedValue.NullObject;
                }
                return Environment.OSVersion.Platform.ToString();
            }
        }
        internal sealed class OsVersionExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 0) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: os_version()");
                    return BoxedValue.NullObject;
                }
                return Environment.OSVersion.Version.ToString();
            }
        }
        internal sealed class GetFullPathExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: get_full_path(path)");
                    return BoxedValue.NullObject;
                }
                string ret = string.Empty;
                if (operands.Count >= 1) {
                    var path = operands[0].AsString;
                    if (null != path) {
                        path = Environment.ExpandEnvironmentVariables(path);
                        return Path.GetFullPath(path);
                    }
                }
                return ret;
            }
        }
        internal sealed class GetPathRootExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: get_path_root(path)");
                    return BoxedValue.NullObject;
                }
                string ret = string.Empty;
                if (operands.Count >= 1) {
                    var path = operands[0].AsString;
                    if (null != path) {
                        path = Environment.ExpandEnvironmentVariables(path);
                        return Path.GetPathRoot(path);
                    }
                }
                return ret;
            }
        }
        internal sealed class GetRandomFileNameExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 0) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: get_random_file_name()");
                    return BoxedValue.NullObject;
                }
                return Path.GetRandomFileName();
            }
        }
        internal sealed class GetTempFileNameExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 0) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: get_temp_file_name()");
                    return BoxedValue.NullObject;
                }
                return Path.GetTempFileName();
            }
        }
        internal sealed class GetTempPathExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 0) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: get_temp_path()");
                    return BoxedValue.NullObject;
                }
                return Path.GetTempPath();
            }
        }
        internal sealed class HasExtensionExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: has_extension(path)");
                    return BoxedValue.NullObject;
                }
                bool ret = false;
                if (operands.Count >= 1) {
                    var path = operands[0].AsString;
                    if (null != path) {
                        path = Environment.ExpandEnvironmentVariables(path);
                        return Path.HasExtension(path);
                    }
                }
                return ret;
            }
        }
        internal sealed class IsPathRootedExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: is_path_rooted(path)");
                    return BoxedValue.NullObject;
                }
                bool ret = false;
                if (operands.Count >= 1) {
                    var path = operands[0].AsString;
                    if (null != path) {
                        path = Environment.ExpandEnvironmentVariables(path);
                        return Path.IsPathRooted(path);
                    }
                }
                return ret;
            }
        }
        internal sealed class GetFileNameExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: get_file_name(path)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var path = operands[0].AsString;
                    if (null != path) {
                        path = Environment.ExpandEnvironmentVariables(path);
                        r = Path.GetFileName(path);
                    }
                }
                return r;
            }
        }
        internal sealed class GetFileNameWithoutExtensionExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: get_file_name_without_extension(path)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var path = operands[0].AsString;
                    if (null != path) {
                        path = Environment.ExpandEnvironmentVariables(path);
                        r = Path.GetFileNameWithoutExtension(path);
                    }
                }
                return r;
            }
        }
        internal sealed class GetExtensionExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: get_extension(path)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var path = operands[0].AsString;
                    if (null != path) {
                        path = Environment.ExpandEnvironmentVariables(path);
                        r = Path.GetExtension(path);
                    }
                }
                return r;
            }
        }
        internal sealed class GetDirectoryNameExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: get_directory_name(path)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var path = operands[0].AsString;
                    if (null != path) {
                        path = Environment.ExpandEnvironmentVariables(path);
                        r = Path.GetDirectoryName(path);
                    }
                }
                return r;
            }
        }
        internal sealed class CombinePathExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: combine_path(path1,path2,...)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 2) {
                    List<string> list = new List<string>();
                    for (int ix = 0; ix < operands.Count; ++ix) {
                        var v = operands[ix];
                        var str = v.AsString;
                        if (string.IsNullOrEmpty(str)) {
                            AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine(string.Format("Path {0} is null or empty", ix));
                            return BoxedValue.NullObject;
                        }
                        list.Add(str);
                    }
                    var path = Path.Combine(list.ToArray());
                    path = Environment.ExpandEnvironmentVariables(path);
                    r = path;
                }
                return r;
            }
        }
        internal sealed class ChangeExtensionExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 2) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: change_extension(path,ext)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 2) {
                    var path = operands[0].AsString;
                    var ext = operands[1].AsString;
                    if (null != path && null != ext) {
                        path = Environment.ExpandEnvironmentVariables(path);
                        r = Path.ChangeExtension(path, ext);
                    }
                }
                return r;
            }
        }
        internal sealed class QuotePathExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 1 || operands.Count > 3) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: quote_path(path[,only_needed,single_quote])");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var path = operands[0].AsString;
                    bool onlyNeeded = operands.Count >= 2 ? operands[1].GetBool() : true;
                    bool singleQuotes = operands.Count >= 3 ? operands[2].GetBool() : false;
                    if (null != path && path.Length > 0) {
                        path = Environment.ExpandEnvironmentVariables(path).Trim();
                        if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
                            //On Windows, file names can contain single quotes, but paths must be quoted using double quotes.
                            string delim = "\"";
                            if (onlyNeeded) {
                                char first = path[0];
                                char last = path[path.Length - 1];
                                int ix = path.IndexOf(' ');
                                if (ix > 0 && !CharIsQuote(first) && !CharIsQuote(last)) {
                                    path = delim + path + delim;
                                }
                            }
                            else {
                                char first = path[0];
                                char last = path[path.Length - 1];
                                if (!CharIsQuote(first) && !CharIsQuote(last)) {
                                    path = delim + path + delim;
                                }
                            }
                        }
                        else {
                            string delim = singleQuotes ? "'" : "\"";
                            if (onlyNeeded) {
                                char first = path[0];
                                char last = path[path.Length - 1];
                                int ix = path.IndexOf(' ');
                                if (ix > 0 && !CharIsQuote(first) && !CharIsQuote(last)) {
                                    path = delim + path + delim;
                                }
                            }
                            else {
                                char first = path[0];
                                char last = path[path.Length - 1];
                                if (!CharIsQuote(first) && !CharIsQuote(last)) {
                                    path = delim + path + delim;
                                }
                            }
                        }
                        r = path;
                    }
                }
                return r;
            }
            private static bool CharIsQuote(char c)
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
                    return c == '"';
                }
                else {
                    return c == '"' || c == '\'';
                }
            }
        }
        internal sealed class EchoExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: echo(fmt,arg1,arg2,...) api, Console.WriteLine");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var obj = operands[0];
                    if (obj.IsString) {
                        var fmt = obj.StringVal;
                        if (operands.Count > 1 && null != fmt) {
                            ArrayList arrayList = new ArrayList();
                            for (int i = 1; i < operands.Count; ++i) {
                                arrayList.Add(operands[i].GetObject());
                            }
                            Console.WriteLine(fmt, arrayList.ToArray());
                        }
                        else {
                            Console.WriteLine(obj.GetObject());
                        }
                    }
                    else {
                        Console.WriteLine(obj.GetObject());
                    }
                }
                else {
                    Console.WriteLine();
                }
                return r;
            }
        }
        internal sealed class CallStackExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 0) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: call_stack()");
                    return BoxedValue.NullObject;
                }
                var r = System.Environment.StackTrace;
                return BoxedValue.FromObject(r);
            }
        }
        internal sealed class FileEchoExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count > 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: file_echo(bool) or file_echo()");
                    return BoxedValue.NullObject;
                }
                if (operands.Count >= 1) {
                    DslCalculator.FileEchoOn = operands[0].GetBool();
                }
                return DslCalculator.FileEchoOn;
            }
        }
        internal sealed class DirectoryExistExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: dir_exists(dir), aliased as direxists");
                    return BoxedValue.NullObject;
                }
                var ret = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var dir = operands[0].AsString;
                    dir = Environment.ExpandEnvironmentVariables(dir);
                    ret = Directory.Exists(dir);
                }
                return ret;
            }
        }
        internal sealed class FileExistExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: file_exists(file), aliased as fileexists");
                    return BoxedValue.NullObject;
                }
                var ret = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var file = operands[0].AsString;
                    file = Environment.ExpandEnvironmentVariables(file);
                    ret = File.Exists(file);
                }
                return ret;
            }
        }
        internal sealed class ListDirectoriesExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: list_dirs(dir,glob_pattern_list_or_str_1,glob_pattern_list_or_str_2,...)");
                    return BoxedValue.NullObject;
                }
                var ret = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var baseDir = operands[0].AsString;
                    baseDir = Environment.ExpandEnvironmentVariables(baseDir);
                    IList<string> filterList = new string[] { "*" };
                    if (operands.Count >= 2) {
                        var list = new List<string>();
                        for (int i = 1; i < operands.Count; ++i) {
                            var str = operands[i].AsString;
                            if (null != str) {
                                list.Add(str);
                            }
                            else {
                                var strList = operands[i].As<IList>();
                                if (null != strList) {
                                    foreach (var strObj in strList) {
                                        var tempStr = strObj as string;
                                        if (null != tempStr)
                                            list.Add(tempStr);
                                    }
                                }
                            }
                        }
                        filterList = list;
                    }
                    if (null != baseDir && Directory.Exists(baseDir)) {
                        var fullList = new List<string>();
                        foreach (var filter in filterList) {
                            var list = Directory.GetDirectories(baseDir, filter, SearchOption.TopDirectoryOnly);
                            fullList.AddRange(list);
                        }
                        ret = BoxedValue.FromObject(fullList);
                    }
                }
                return ret;
            }
        }
        internal sealed class ListFilesExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: list_files(dir,glob_pattern_list_or_str_1,glob_pattern_list_or_str_2,...)");
                    return BoxedValue.NullObject;
                }
                var ret = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var baseDir = operands[0].AsString;
                    baseDir = Environment.ExpandEnvironmentVariables(baseDir);
                    IList<string> filterList = new string[] { "*" };
                    if (operands.Count >= 2) {
                        var list = new List<string>();
                        for (int i = 1; i < operands.Count; ++i) {
                            var str = operands[i].AsString;
                            if (null != str) {
                                list.Add(str);
                            }
                            else {
                                var strList = operands[i].As<IList>();
                                if (null != strList) {
                                    foreach (var strObj in strList) {
                                        var tempStr = strObj as string;
                                        if (null != tempStr)
                                            list.Add(tempStr);
                                    }
                                }
                            }
                        }
                        filterList = list;
                    }
                    if (null != baseDir && Directory.Exists(baseDir)) {
                        var fullList = new List<string>();
                        foreach (var filter in filterList) {
                            var list = Directory.GetFiles(baseDir, filter, SearchOption.TopDirectoryOnly);
                            fullList.AddRange(list);
                        }
                        ret = BoxedValue.FromObject(fullList);
                    }
                }
                return ret;
            }
        }
        internal sealed class ListAllDirectoriesExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: list_all_dirs(dir,glob_pattern_list_or_str_1,glob_pattern_list_or_str_2,...)");
                    return BoxedValue.NullObject;
                }
                var ret = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var baseDir = operands[0].AsString;
                    baseDir = Environment.ExpandEnvironmentVariables(baseDir);
                    IList<string> filterList = new string[] { "*" };
                    if (operands.Count >= 2) {
                        var list = new List<string>();
                        for (int i = 1; i < operands.Count; ++i) {
                            var str = operands[i].AsString;
                            if (null != str) {
                                list.Add(str);
                            }
                            else {
                                var strList = operands[i].As<IList>();
                                if (null != strList) {
                                    foreach (var strObj in strList) {
                                        var tempStr = strObj as string;
                                        if (null != tempStr)
                                            list.Add(tempStr);
                                    }
                                }
                            }
                        }
                        filterList = list;
                    }
                    if (null != baseDir && Directory.Exists(baseDir)) {
                        var fullList = new List<string>();
                        foreach (var filter in filterList) {
                            var list = Directory.GetDirectories(baseDir, filter, SearchOption.AllDirectories);
                            fullList.AddRange(list);
                        }
                        ret = BoxedValue.FromObject(fullList);
                    }
                }
                return ret;
            }
        }
        internal sealed class ListAllFilesExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: list_all_files(dir,glob_pattern_list_or_str_1,glob_pattern_list_or_str_2,...)");
                    return BoxedValue.NullObject;
                }
                var ret = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var baseDir = operands[0].AsString;
                    baseDir = Environment.ExpandEnvironmentVariables(baseDir);
                    IList<string> filterList = new string[] { "*" };
                    if (operands.Count >= 2) {
                        var list = new List<string>();
                        for (int i = 1; i < operands.Count; ++i) {
                            var str = operands[i].AsString;
                            if (null != str) {
                                list.Add(str);
                            }
                            else {
                                var strList = operands[i].As<IList>();
                                if (null != strList) {
                                    foreach (var strObj in strList) {
                                        var tempStr = strObj as string;
                                        if (null != tempStr)
                                            list.Add(tempStr);
                                    }
                                }
                            }
                        }
                        filterList = list;
                    }
                    if (null != baseDir && Directory.Exists(baseDir)) {
                        var fullList = new List<string>();
                        foreach (var filter in filterList) {
                            var list = Directory.GetFiles(baseDir, filter, SearchOption.AllDirectories);
                            fullList.AddRange(list);
                        }
                        ret = BoxedValue.FromObject(fullList);
                    }
                }
                return ret;
            }
        }
        internal sealed class CreateDirectoryExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: create_dir(dir), aliased as make_dir");
                    return BoxedValue.NullObject;
                }
                bool ret = false;
                if (operands.Count >= 1) {
                    var dir = operands[0].AsString;
                    dir = Environment.ExpandEnvironmentVariables(dir);
                    if (!Directory.Exists(dir)) {
                        Directory.CreateDirectory(dir);
                        ret = true;

                        if (DslCalculator.FileEchoOn) {
                            Console.WriteLine("create directory {0}", dir);
                        }
                    }
                }
                return ret;
            }
        }
        internal sealed class CopyDirectoryExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: copy_dir(dir1,dir2,glob_pattern_list_or_str_1,glob_pattern_list_or_str_2,...) api, include subdir");
                    return BoxedValue.NullObject;
                }
                int ct = 0;
                if (operands.Count >= 2) {
                    var dir1 = operands[0].AsString;
                    var dir2 = operands[1].AsString;
                    dir1 = Environment.ExpandEnvironmentVariables(dir1);
                    dir2 = Environment.ExpandEnvironmentVariables(dir2);
                    List<string> filterAndNewExts = new List<string>();
                    for (int i = 2; i < operands.Count; ++i) {
                        var str = operands[i].AsString;
                        if (null != str) {
                            filterAndNewExts.Add(str);
                        }
                        else {
                            var strList = operands[i].As<IList>();
                            if (null != strList) {
                                foreach (var strObj in strList) {
                                    var tempStr = strObj as string;
                                    if (null != tempStr)
                                        filterAndNewExts.Add(tempStr);
                                }
                            }
                        }
                    }
                    if (filterAndNewExts.Count <= 0) {
                        filterAndNewExts.Add("*");
                    }
                    var targetRoot = Path.GetFullPath(dir2);
                    if (Directory.Exists(dir1)) {
                        CopyFolder(targetRoot, dir1, dir2, filterAndNewExts, ref ct);
                    }
                }
                return ct;
            }
            private static void CopyFolder(string targetRoot, string from, string to, IList<string> filterAndNewExts, ref int ct)
            {
                if (!string.IsNullOrEmpty(to) && !Directory.Exists(to))
                    Directory.CreateDirectory(to);
                // sub directories
                foreach (string sub in Directory.GetDirectories(from)) {
                    var srcPath = Path.GetFullPath(sub);
                    if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX) {
                        if (srcPath.IndexOf(targetRoot) == 0)
                            continue;
                    }
                    else {
                        if (srcPath.IndexOf(targetRoot, StringComparison.CurrentCultureIgnoreCase) == 0)
                            continue;
                    }
                    var sName = Path.GetFileName(sub);
                    CopyFolder(targetRoot, sub, Path.Combine(to, sName), filterAndNewExts, ref ct);
                }
                // file
                for (int i = 0; i < filterAndNewExts.Count; i += 2) {
                    string filter = filterAndNewExts[i];
                    string newExt = string.Empty;
                    if (i + 1 < filterAndNewExts.Count) {
                        newExt = filterAndNewExts[i + 1];
                    }
                    foreach (string file in Directory.GetFiles(from, filter, SearchOption.TopDirectoryOnly)) {
                        string targetFile;
                        if (string.IsNullOrEmpty(newExt))
                            targetFile = Path.Combine(to, Path.GetFileName(file));
                        else
                            targetFile = Path.Combine(to, Path.ChangeExtension(Path.GetFileName(file), newExt));
                        File.Copy(file, targetFile, true);
                        ++ct;

                        if (DslCalculator.FileEchoOn) {
                            Console.WriteLine("copy file {0} => {1}", file, targetFile);
                        }
                    }
                }
            }
        }
        internal sealed class MoveDirectoryExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 2) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: move_dir(dir1,dir2)");
                    return BoxedValue.NullObject;
                }
                bool ret = false;
                if (operands.Count >= 2) {
                    var dir1 = operands[0].AsString;
                    var dir2 = operands[1].AsString;
                    dir1 = Environment.ExpandEnvironmentVariables(dir1);
                    dir2 = Environment.ExpandEnvironmentVariables(dir2);
                    if (Directory.Exists(dir1)) {
                        if (Directory.Exists(dir2)) {
                            Directory.Delete(dir2);
                        }
                        Directory.Move(dir1, dir2);
                        ret = true;

                        if (DslCalculator.FileEchoOn) {
                            Console.WriteLine("move directory {0} => {1}", dir1, dir2);
                        }
                    }
                }
                return ret;
            }
        }
        internal sealed class DeleteDirectoryExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: delete_dir(dir)");
                    return BoxedValue.NullObject;
                }
                bool ret = false;
                if (operands.Count >= 1) {
                    var dir = operands[0].AsString;
                    dir = Environment.ExpandEnvironmentVariables(dir);
                    if (Directory.Exists(dir)) {
                        Directory.Delete(dir, true);
                        ret = true;

                        if (DslCalculator.FileEchoOn) {
                            Console.WriteLine("delete directory {0}", dir);
                        }
                    }
                }
                return ret;
            }
        }
        internal sealed class CopyFileExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 2) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: copy_file(file1,file2)");
                    return BoxedValue.NullObject;
                }
                bool ret = false;
                if (operands.Count >= 2) {
                    var file1 = operands[0].AsString;
                    var file2 = operands[1].AsString;
                    file1 = Environment.ExpandEnvironmentVariables(file1);
                    file2 = Environment.ExpandEnvironmentVariables(file2);
                    if (File.Exists(file1)) {
                        var dir = Path.GetDirectoryName(file2);
                        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) {
                            Directory.CreateDirectory(dir);
                        }
                        File.Copy(file1, file2, true);
                        ret = true;

                        if (DslCalculator.FileEchoOn) {
                            Console.WriteLine("copy file {0} => {1}", file1, file2);
                        }
                    }
                }
                return ret;
            }
        }
        internal sealed class CopyFilesExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: copy_files(dir1,dir2,glob_pattern_list_or_str_1,glob_pattern_list_or_str_2,...) api, dont include subdir");
                    return BoxedValue.NullObject;
                }
                int ct = 0;
                if (operands.Count >= 2) {
                    var dir1 = operands[0].AsString;
                    var dir2 = operands[1].AsString;
                    dir1 = Environment.ExpandEnvironmentVariables(dir1);
                    dir2 = Environment.ExpandEnvironmentVariables(dir2);
                    List<string> filterAndNewExts = new List<string>();
                    for (int i = 2; i < operands.Count; ++i) {
                        var str = operands[i].AsString;
                        if (null != str) {
                            filterAndNewExts.Add(str);
                        }
                        else {
                            var strList = operands[i].As<IList>();
                            if (null != strList) {
                                foreach (var strObj in strList) {
                                    var tempStr = strObj as string;
                                    if (null != tempStr)
                                        filterAndNewExts.Add(tempStr);
                                }
                            }
                        }
                    }
                    if (filterAndNewExts.Count <= 0) {
                        filterAndNewExts.Add("*");
                    }
                    if (Directory.Exists(dir1)) {
                        CopyFolder(dir1, dir2, filterAndNewExts, ref ct);
                    }
                }
                return ct;
            }
            private static void CopyFolder(string from, string to, IList<string> filterAndNewExts, ref int ct)
            {
                if (!string.IsNullOrEmpty(to) && !Directory.Exists(to))
                    Directory.CreateDirectory(to);
                // file
                for (int i = 0; i < filterAndNewExts.Count; i += 2) {
                    string filter = filterAndNewExts[i];
                    string newExt = string.Empty;
                    if (i + 1 < filterAndNewExts.Count) {
                        newExt = filterAndNewExts[i + 1];
                    }
                    foreach (string file in Directory.GetFiles(from, filter, SearchOption.TopDirectoryOnly)) {
                        string targetFile;
                        if (string.IsNullOrEmpty(newExt))
                            targetFile = Path.Combine(to, Path.GetFileName(file));
                        else
                            targetFile = Path.Combine(to, Path.ChangeExtension(Path.GetFileName(file), newExt));
                        File.Copy(file, targetFile, true);
                        ++ct;

                        if (DslCalculator.FileEchoOn) {
                            Console.WriteLine("copy file {0} => {1}", file, targetFile);
                        }
                    }
                }
            }
        }
        internal sealed class MoveFileExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 2) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: move_file(file1,file2)");
                    return BoxedValue.NullObject;
                }
                bool ret = false;
                if (operands.Count >= 2) {
                    var file1 = operands[0].AsString;
                    var file2 = operands[1].AsString;
                    file1 = Environment.ExpandEnvironmentVariables(file1);
                    file2 = Environment.ExpandEnvironmentVariables(file2);
                    if (File.Exists(file1)) {
                        var dir = Path.GetDirectoryName(file2);
                        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) {
                            Directory.CreateDirectory(dir);
                        }
                        if (File.Exists(file2)) {
                            File.Delete(file2);
                        }
                        File.Move(file1, file2);
                        ret = true;

                        if (DslCalculator.FileEchoOn) {
                            Console.WriteLine("move file {0} => {1}", file1, file2);
                        }
                    }
                }
                return ret;
            }
        }
        internal sealed class DeleteFileExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: delete_file(file)");
                    return BoxedValue.NullObject;
                }
                bool ret = false;
                if (operands.Count >= 1) {
                    var file = operands[0].AsString;
                    file = Environment.ExpandEnvironmentVariables(file);
                    if (File.Exists(file)) {
                        File.Delete(file);
                        ret = true;

                        if (DslCalculator.FileEchoOn) {
                            Console.WriteLine("delete file {0}", file);
                        }
                    }
                }
                return ret;
            }
        }
        internal sealed class DeleteFilesExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: delete_files(dir,glob_pattern_list_or_str_1,glob_pattern_list_or_str_2,...) api, dont include subdir");
                    return BoxedValue.NullObject;
                }
                int ct = 0;
                if (operands.Count >= 1) {
                    var dir = operands[0].AsString;
                    List<string> filters = new List<string>();
                    for (int i = 1; i < operands.Count; ++i) {
                        var str = operands[i].AsString;
                        if (null != str) {
                            filters.Add(str);
                        }
                        else {
                            var strList = operands[i].As<IList>();
                            if (null != strList) {
                                foreach (var strObj in strList) {
                                    var tempStr = strObj as string;
                                    if (null != tempStr)
                                        filters.Add(tempStr);
                                }
                            }
                        }
                    }
                    if (filters.Count <= 0) {
                        filters.Add("*");
                    }
                    dir = Environment.ExpandEnvironmentVariables(dir);
                    if (Directory.Exists(dir)) {
                        foreach (var filter in filters) {
                            foreach (string file in Directory.GetFiles(dir, filter, SearchOption.TopDirectoryOnly)) {
                                File.Delete(file);
                                ++ct;

                                if (DslCalculator.FileEchoOn) {
                                    Console.WriteLine("delete file {0}", file);
                                }
                            }
                        }
                    }
                }
                return ct;
            }
        }
        internal sealed class DeleteAllFilesExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: delete_all_files(dir,glob_pattern_list_or_str_1,glob_pattern_list_or_str_2,...) api, include subdir");
                    return BoxedValue.NullObject;
                }
                int ct = 0;
                if (operands.Count >= 1) {
                    var dir = operands[0].AsString;
                    List<string> filters = new List<string>();
                    for (int i = 1; i < operands.Count; ++i) {
                        var str = operands[i].AsString;
                        if (null != str) {
                            filters.Add(str);
                        }
                        else {
                            var strList = operands[i].As<IList>();
                            if (null != strList) {
                                foreach (var strObj in strList) {
                                    var tempStr = strObj as string;
                                    if (null != tempStr)
                                        filters.Add(tempStr);
                                }
                            }
                        }
                    }
                    if (filters.Count <= 0) {
                        filters.Add("*");
                    }
                    dir = Environment.ExpandEnvironmentVariables(dir);
                    if (Directory.Exists(dir)) {
                        foreach (var filter in filters) {
                            foreach (string file in Directory.GetFiles(dir, filter, SearchOption.AllDirectories)) {
                                File.Delete(file);
                                ++ct;

                                if (DslCalculator.FileEchoOn) {
                                    Console.WriteLine("delete file {0}", file);
                                }
                            }
                        }
                    }
                }
                return ct;
            }
        }
        internal sealed class GetFileInfoExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: get_file_info(file)");
                    return BoxedValue.NullObject;
                }
                var ret = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var file = operands[0].AsString;
                    file = Environment.ExpandEnvironmentVariables(file);
                    if (File.Exists(file)) {
                        ret = BoxedValue.FromObject(new FileInfo(file));
                    }
                }
                return ret;
            }
        }
        internal sealed class GetDirectoryInfoExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: get_dir_info(dir)");
                    return BoxedValue.NullObject;
                }
                var ret = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var file = operands[0].AsString;
                    file = Environment.ExpandEnvironmentVariables(file);
                    if (Directory.Exists(file)) {
                        ret = BoxedValue.FromObject(new DirectoryInfo(file));
                    }
                }
                return ret;
            }
        }
        internal sealed class GetDriveInfoExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: get_drive_info(drive)");
                    return BoxedValue.NullObject;
                }
                var ret = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var drive = operands[0].AsString;
                    ret = BoxedValue.FromObject(new DriveInfo(drive));
                }
                return ret;
            }
        }
        internal sealed class GetDrivesInfoExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 0) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: get_drives_info()");
                    return BoxedValue.NullObject;
                }
                var ret = DriveInfo.GetDrives();
                return BoxedValue.FromObject(ret);
            }
        }
        internal sealed class ReadAllLinesExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 1 || operands.Count > 2) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: read_all_lines(file[,encoding])");
                    return BoxedValue.NullObject;
                }
                if (operands.Count >= 1) {
                    string path = operands[0].AsString;
                    if (!string.IsNullOrEmpty(path)) {
                        path = Environment.ExpandEnvironmentVariables(path);
                        Encoding encoding = Encoding.UTF8;
                        if (operands.Count >= 2) {
                            var v = operands[1];
                            encoding = GetEncoding(v);
                        }
                        return BoxedValue.FromObject(File.ReadAllLines(path, encoding));
                    }
                }
                return BoxedValue.FromObject(new string[0]);
            }
        }
        internal sealed class WriteAllLinesExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2 || operands.Count > 3) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: write_all_lines(file,lines[,encoding])");
                    return BoxedValue.NullObject;
                }
                if (operands.Count >= 2) {
                    string path = operands[0].AsString;
                    var lines = operands[1].As<IList>();
                    if (!string.IsNullOrEmpty(path) && null != lines) {
                        path = Environment.ExpandEnvironmentVariables(path);
                        Encoding encoding = Encoding.UTF8;
                        if (operands.Count >= 3) {
                            var v = operands[2];
                            encoding = GetEncoding(v);
                        }
                        var strs = new List<string>();
                        foreach (var line in lines) {
                            strs.Add(line.ToString() ?? string.Empty);
                        }
                        if (strs.Count == 0) {
                            AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("You cannot write empty values 鈥嬧€媡o a file !!! To delete certain lines, use the 'delete_lines' function.");
                            return BoxedValue.From(false);
                        }
                        string ext = Path.GetExtension(path).ToLower();
                        if (File.Exists(path) && ext != ".txt" && ext != ".md") {
                            var lineCount = File.ReadAllLines(path).Length;
                            var newLineCount = path.Split('\n').Length;
                            if (lineCount > newLineCount + Core.AgentCore.Instance.MaxLinesDeletedByWriteFile) {
                                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("You cannot significantly reduce code using 'write_file' !!! To delete certain lines, use the 'delete_lines' function.");
                                return BoxedValue.From(false);
                            }
                        }
                        File.WriteAllLines(path, strs, encoding);
                        return true;
                    }
                }
                return false;
            }
        }
        internal sealed class ReadAllTextExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 1 || operands.Count > 2) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: read_all_text(file[,encoding])");
                    return BoxedValue.NullObject;
                }
                if (operands.Count >= 1) {
                    string path = operands[0].AsString;
                    if (!string.IsNullOrEmpty(path)) {
                        path = Environment.ExpandEnvironmentVariables(path);
                        Encoding encoding = Encoding.UTF8;
                        if (operands.Count >= 2) {
                            var v = operands[1];
                            encoding = GetEncoding(v);
                        }
                        return File.ReadAllText(path, encoding);
                    }
                }
                return BoxedValue.NullObject;
            }
        }
        internal sealed class WriteAllTextExp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count < 2 || operands.Count > 3) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: write_all_text(file,txt[,encoding])");
                    return BoxedValue.NullObject;
                }
                if (operands.Count >= 2) {
                    string path = operands[0].AsString;
                    var text = operands[1].AsString;
                    if (!string.IsNullOrEmpty(path) && null != text) {
                        path = Environment.ExpandEnvironmentVariables(path);
                        Encoding encoding = Encoding.UTF8;
                        if (operands.Count >= 3) {
                            var v = operands[2];
                            encoding = GetEncoding(v);
                        }
                        if (string.IsNullOrEmpty(text)) {
                            AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("You cannot write empty values 鈥嬧€媡o a file !!! To delete certain lines, use the 'delete_lines' function.");
                            return BoxedValue.From(false);
                        }
                        string ext = Path.GetExtension(path).ToLower();
                        if (File.Exists(path) && ext != ".txt" && ext != ".md") {
                            var lineCount = File.ReadAllLines(path).Length;
                            var newLineCount = path.Split('\n').Length;
                            if (lineCount > newLineCount + Core.AgentCore.Instance.MaxLinesDeletedByWriteFile) {
                                AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("You cannot significantly reduce code using 'write_file' !!! To delete certain lines, use the 'delete_lines' function.");
                                return BoxedValue.From(false);
                            }
                        }
                        File.WriteAllText(path, text, encoding);
                        return true;
                    }
                }
                return false;
            }
        }
        internal sealed class CalcMd5Exp : SimpleExpressionBase
        {
            protected override BoxedValue OnCalc(IList<BoxedValue> operands)
            {
                if (operands.Count != 1) {
                    AgentFrameworkService.Instance.ErrorReporter!.AppendApiErrorInfoLine("Expected: calc_md5(file)");
                    return BoxedValue.NullObject;
                }
                BoxedValue r = BoxedValue.NullObject;
                if (operands.Count >= 1) {
                    var file = operands[0].AsString;
                    if (null != file) {
                        r = CalcMD5(file);
                    }
                }
                return r;
            }
            public string CalcMD5(string file)
            {
                byte[]? array = null;
                using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                    MD5 md5 = MD5.Create();
                    array = md5.ComputeHash(stream);
                    stream.Close();
                }
                if (null != array) {
                    StringBuilder stringBuilder = new StringBuilder();
                    for (int i = 0; i < array.Length; i++) {
                        stringBuilder.Append(array[i].ToString("x2"));
                    }
                    return stringBuilder.ToString();
                }
                else {
                    return string.Empty;
                }
            }
        }
    }
}
