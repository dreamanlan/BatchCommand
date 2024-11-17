#include "DebugScriptVM.h"
#include "string.h"
#include <array>
#include <vector>
#include <unordered_map>
#include <string>
#include <atomic>
#include <mutex>
#include <shared_mutex>
#include <type_traits>
#include <chrono>
#include <ctime>
#include <cmath>
#include <cstdio>
#include <cstdlib>
#include <cstdarg>
#include <sstream>
#include <fstream>

uint32_t g_DebugScriptSerialNum = 0;
bool g_DebugScriptStarted = false;

namespace
{
    struct DebugScriptGlobalImpl;
    struct DebugScriptVMImpl;
    static DebugScriptGlobalImpl* g_pDebugScriptGlobal = nullptr;
    static thread_local DebugScriptVMImpl* g_pDebugScriptVM = nullptr;
    static std::chrono::time_point<std::chrono::high_resolution_clock> g_start_time{};

    static const int c_max_variable_table_size = 8192;

    using IntGlobals = std::array<int64_t, c_max_variable_table_size>;
    using FloatGlobals = std::array<double, c_max_variable_table_size>;
    using StringGlobals = std::array<std::string, c_max_variable_table_size>;

    using IntLocals = std::array<int64_t, c_max_variable_table_size>;
    using FloatLocals = std::array<double, c_max_variable_table_size>;
    using StringLocals = std::array<std::string, c_max_variable_table_size>;

    //this enum must sync with InsEnum in DebugScriptCompiler.cs
    enum class InsEnum
    {
        CALL = 0,
        RET,
        JMP,
        JMPIF,
        JMPIFNOT,
        INC,
        INCFLT,
        INCV,
        INCVFLT,
        MOV,
        MOVFLT,
        MOVSTR,
        ARRGET,
        ARRGETFLT,
        ARRGETSTR,
        ARRSET,
        ARRSETFLT,
        ARRSETSTR,
        NEG,
        NEGFLT,
        ADD,
        ADDFLT,
        ADDSTR,
        SUB,
        SUBFLT,
        MUL,
        MULFLT,
        DIV,
        DIVFLT,
        MOD,
        MODFLT,
        AND,
        OR,
        NOT,
        GT,
        GTFLT,
        GTSTR,
        GE,
        GEFLT,
        GESTR,
        EQ,
        EQFLT,
        EQSTR,
        NE,
        NEFLT,
        NESTR,
        LE,
        LEFLT,
        LESTR,
        LT,
        LTFLT,
        LTSTR,
        LSHIFT,
        RSHIFT,
        URSHIFT,
        BITAND,
        BITOR,
        BITXOR,
        BITNOT,
        INT2FLT,
        INT2STR,
        FLT2INT,
        FLT2STR,
        STR2INT,
        STR2FLT,
        ITOF,
        FTOI,
        ARGC,
        ARGV,
        ADDR,
        PTRGET,
        PTRSET,
        JPTR,
        NUM
    };
    enum class TypeEnum
    {
        NotUse = 0,
        Int,
        Float,
        String
    };
    static const int32_t c_abs_offset_mask = 0x7fffffff;
    static const int32_t c_offset_backward_flag = static_cast<int32_t>(0x80000000);

    static inline int32_t DecodeOffset(int32_t offset)
    {
        bool back = (offset & c_offset_backward_flag) != 0;
        offset = ((offset & c_abs_offset_mask) >> 8);
        if (back)
            offset = -offset;
        return offset;
    }
    static inline InsEnum DecodeInsEnum(int32_t opcode)
    {
        return (InsEnum)(opcode & 0xff);
    }
    static inline void DecodeOperand1(int32_t operand, int32_t& apiIndex)
    {
        apiIndex = (operand & 0xffff);
    }
    static inline void DecodeOperand1(int32_t operand, bool& isGlobal, TypeEnum& type, int32_t& index)
    {
        int32_t localOrGlobal = ((operand & 0x8000) >> 15);
        int32_t ty = ((operand & 0x6000) >> 13);
        index = (operand & 0x1fff);
        isGlobal = localOrGlobal != 0;
        type = static_cast<TypeEnum>(ty);
    }
    static inline void DecodeOperand2(int32_t operand, bool& isGlobal, TypeEnum& type, int32_t& index)
    {
        operand >>= 16;
        DecodeOperand1(operand, isGlobal, type, index);
    }
    static inline void DecodeOpcode(int32_t opcode, InsEnum& ins, int32_t& offset)
    {
        ins = (InsEnum)(opcode & 0xff);
        offset = DecodeOffset(opcode);
    }
    static inline void DecodeOpcode(int32_t opcode, InsEnum& ins, int32_t& argNum, bool& isGlobal, TypeEnum& type, int32_t& index)
    {
        ins = (InsEnum)(opcode & 0xff);
        argNum = ((opcode & 0xff00) >> 8);
        int32_t operand = opcode >> 16;
        DecodeOperand1(operand, isGlobal, type, index);
    }

    static inline int64_t GetVarInt(bool isGlobal, int32_t index, IntLocals& intLocals, IntGlobals& intGlobals)
    {
        return isGlobal ? intGlobals[index] : intLocals[index];
    }
    static inline double GetVarFloat(bool isGlobal, int32_t index, FloatLocals& fltLocals, FloatGlobals& fltGlobals)
    {
        return isGlobal ? fltGlobals[index] : fltLocals[index];
    }
    static inline const std::string& GetVarString(bool isGlobal, int32_t index, StringLocals& strLocals, StringGlobals& strGlobals)
    {
        return isGlobal ? strGlobals[index] : strLocals[index];
    }
    static inline void SetVarInt(bool isGlobal, int32_t index, int64_t val, IntLocals& intLocals, IntGlobals& intGlobals)
    {
        (isGlobal ? intGlobals[index] : intLocals[index]) = val;
    }
    static inline void SetVarFloat(bool isGlobal, int32_t index, double val, FloatLocals& fltLocals, FloatGlobals& fltGlobals)
    {
        (isGlobal ? fltGlobals[index] : fltLocals[index]) = val;
    }
    static inline void SetVarString(bool isGlobal, int32_t index, const std::string& val, StringLocals& strLocals, StringGlobals& strGlobals)
    {
        (isGlobal ? strGlobals[index] : strLocals[index]) = val;
    }

    static inline uint8_t ReadByte(std::ifstream& ifs)
    {
        uint8_t v;
        ifs.read(reinterpret_cast<char*>(&v), sizeof(v));
        return v;
    }
    static inline int Read7BitEncodedInt(std::ifstream& ifs)
    {
        uint32_t num = 0u;
        uint8_t b;
        for (int i = 0; i < 28; i += 7)
        {
            b = ReadByte(ifs);
            num |= static_cast<uint32_t>((b & 0x7F) << i);
            if (static_cast<uint32_t>(b) <= 127u)
            {
                return static_cast<int>(num);
            }
        }
        b = ReadByte(ifs);
        if (static_cast<uint32_t>(b) > 15u)
        {
            return -1;
        }
        return static_cast<int>(num) | (b << 28);
    }
    static inline int32_t ReadInt32(std::ifstream& ifs)
    {
        int32_t v;
        ifs.read(reinterpret_cast<char*>(&v), sizeof(v));
        return v;
    }
    static inline int64_t ReadInt64(std::ifstream& ifs)
    {
        int64_t v;
        ifs.read(reinterpret_cast<char*>(&v), sizeof(v));
        return v;
    }
    static inline double ReadDouble(std::ifstream& ifs)
    {
        double v;
        ifs.read(reinterpret_cast<char*>(&v), sizeof(v));
        return v;
    }
    static inline std::string ReadString(std::ifstream& ifs)
    {
        int charNum = Read7BitEncodedInt(ifs);
        std::vector<char> strBuf(charNum + 1);
        ifs.read(strBuf.data(), charNum);
        std::string str{};
        str.reserve(charNum + 1);
        str.assign(strBuf.data());
        return str;
    }
    static int myprintf(const char* fmt, int64_t args[]) {
        const char* p = fmt;
        int i = 0;
        int ct = 0;

        while (*p) {
            if (*p == '%') {
                char tmpFmt[64] = { *p };
                int nextTmpIx = 1;
                bool cont = true;
                while (*p && cont) {
                    ++p;
                    tmpFmt[nextTmpIx++] = *p;
                    tmpFmt[nextTmpIx] = 0;
                    switch (*p) {
                    case 'd':
                    case 'i':
                        ct += printf(tmpFmt, args[i++]);
                        cont = false;
                        break;
                    case 'u':
                        ct += printf(tmpFmt, args[i++]);
                        cont = false;
                        break;
                    case 'o':
                        ct += printf(tmpFmt, args[i++]);
                        cont = false;
                        break;
                    case 'x':
                        ct += printf(tmpFmt, args[i++]);
                        cont = false;
                        break;
                    case 'X':
                        ct += printf(tmpFmt, args[i++]);
                        cont = false;
                        break;
                    case 'f':
                    case 'F':
                        ct += printf(tmpFmt, args[i++]);
                        cont = false;
                        break;
                    case 'e':
                    case 'E':
                        ct += printf(tmpFmt, args[i++]);
                        break;
                    case 'g':
                    case 'G':
                        ct += printf(tmpFmt, args[i++]);
                        cont = false;
                        break;
                    case 'a':
                    case 'A':
                        ct += printf(tmpFmt, args[i++]);
                        cont = false;
                        break;
                    case 'c':
                        ct += printf(tmpFmt, args[i++]);
                        cont = false;
                        break;
                    case 's':
                        ct += printf(tmpFmt, reinterpret_cast<const char*>(args[i++]));
                        cont = false;
                        break;
                    case 'p':
                        ct += printf(tmpFmt, reinterpret_cast<const void*>(args[i++]));
                        cont = false;
                        break;
                    case 'n':
                        ct += printf("%d", ct);
                        cont = false;
                        break;
                    case '%':
                        ct += printf(tmpFmt);
                        cont = false;
                        break;
                    }
                }
            }
            else {
                putchar(*p);
                ++ct;
            }
            ++p;
        }
        return ct;
    }
    static int mysnprintf(char* buffer, std::size_t buf_size, const char* fmt, int64_t args[]) {
        const char* p = fmt;
        int i = 0;
        int ct = 0;

        while (*p) {
            if (*p == '%') {
                char tmpFmt[64] = { *p };
                int nextTmpIx = 1;
                bool cont = true;
                while (*p && cont) {
                    ++p;
                    tmpFmt[nextTmpIx++] = *p;
                    tmpFmt[nextTmpIx] = 0;
                    switch (*p) {
                    case 'd':
                    case 'i':
                        ct += snprintf(buffer + ct, buf_size - ct, tmpFmt, args[i++]);
                        cont = false;
                        break;
                    case 'u':
                        ct += snprintf(buffer + ct, buf_size - ct, tmpFmt, args[i++]);
                        cont = false;
                        break;
                    case 'o':
                        ct += snprintf(buffer + ct, buf_size - ct, tmpFmt, args[i++]);
                        cont = false;
                        break;
                    case 'x':
                        ct += snprintf(buffer + ct, buf_size - ct, tmpFmt, args[i++]);
                        cont = false;
                        break;
                    case 'X':
                        ct += snprintf(buffer + ct, buf_size - ct, tmpFmt, args[i++]);
                        cont = false;
                        break;
                    case 'f':
                    case 'F':
                        ct += snprintf(buffer + ct, buf_size - ct, tmpFmt, args[i++]);
                        cont = false;
                        break;
                    case 'e':
                    case 'E':
                        ct += snprintf(buffer + ct, buf_size - ct, tmpFmt, args[i++]);
                        break;
                    case 'g':
                    case 'G':
                        ct += snprintf(buffer + ct, buf_size - ct, tmpFmt, args[i++]);
                        cont = false;
                        break;
                    case 'a':
                    case 'A':
                        ct += snprintf(buffer + ct, buf_size - ct, tmpFmt, args[i++]);
                        cont = false;
                        break;
                    case 'c':
                        ct += snprintf(buffer + ct, buf_size - ct, tmpFmt, args[i++]);
                        cont = false;
                        break;
                    case 's':
                        ct += snprintf(buffer + ct, buf_size - ct, tmpFmt, reinterpret_cast<const char*>(args[i++]));
                        cont = false;
                        break;
                    case 'p':
                        ct += snprintf(buffer + ct, buf_size - ct, tmpFmt, reinterpret_cast<const void*>(args[i++]));
                        cont = false;
                        break;
                    case 'n':
                        ct += snprintf(buffer + ct, buf_size - ct, "%d", ct);
                        cont = false;
                        break;
                    case '%':
                        ct += snprintf(buffer + ct, buf_size - ct, tmpFmt);
                        cont = false;
                        break;
                    }
                }
            }
            else {
                buffer[ct++] = *p;
            }
            ++p;
        }
        return ct;
    }

    template<typename RetT, typename... ArgsT>
    struct RetTypeT
    {};
    template<typename RetT, typename... ArgsT>
    struct RetTypeT<RetT(ArgsT...)>
    {
        using Type = typename RetT;
    };
    template<typename T>
    using RetType = typename RetTypeT<T>::Type;

    template<typename... ArgsT>
    struct VariadicArgs
    {
    };
    template<typename Arg1T, typename... ArgsT>
    struct VariadicArgs<Arg1T, ArgsT...>
    {
        using Type1 = typename std::remove_cv_t<std::remove_reference_t<Arg1T>>;
    };
    template<typename Arg1T, typename Arg2T, typename... ArgsT>
    struct VariadicArgs<Arg1T, Arg2T, ArgsT...>
    {
        using Type1 = typename std::remove_cv_t<std::remove_reference_t<Arg1T>>;
        using Type2 = typename std::remove_cv_t<std::remove_reference_t<Arg2T>>;
    };

    template<typename RetT, typename... ArgsT>
    struct Arg1TypeT
    {};
    template<typename RetT, typename... ArgsT>
    struct Arg1TypeT<RetT(ArgsT...)>
    {
        using Type = typename VariadicArgs<ArgsT...>::Type1;
    };
    template<typename T>
    using Arg1Type = typename Arg1TypeT<T>::Type;

    template<typename RetT, typename... ArgsT>
    struct Arg2TypeT
    {};
    template<typename RetT, typename... ArgsT>
    struct Arg2TypeT<RetT(ArgsT...)>
    {
        using Type = typename VariadicArgs<ArgsT...>::Type2;
    };
    template<typename T>
    using Arg2Type = typename Arg2TypeT<T>::Type;

    template<typename R, typename T>
    static inline R Convert(const T& v) {}

    template<>
    static inline int64_t Convert<int64_t, int64_t>(const int64_t& v)
    {
        return v;
    }
    template<>
    static inline int64_t Convert<int64_t, double>(const double& v)
    {
        return static_cast<int64_t>(v);
    }
    template<>
    static inline int64_t Convert<int64_t, std::string>(const std::string& v)
    {
        return std::atoll(v.c_str());
    }

    template<>
    static inline double Convert<double, int64_t>(const int64_t& v)
    {
        return static_cast<double>(v);
    }
    template<>
    static inline double Convert<double, double>(const double& v)
    {
        return v;
    }
    template<>
    static inline double Convert<double, std::string>(const std::string& v)
    {
        return std::atof(v.c_str());
    }

    template<>
    static inline std::string Convert<std::string, int64_t>(const int64_t& v)
    {
        return std::to_string(v);
    }
    template<>
    static inline std::string Convert<std::string, double>(const double& v)
    {
        return std::to_string(v);
    }
    template<>
    static inline std::string Convert<std::string, std::string>(const std::string& v)
    {
        return v;
    }

    //this enum must sync with ApiInfo::ApiId from DebugScriptCompiler.cs
    enum class ApiEnum
    {
        Assert = 0,
        Printf,
        Format,
        Time,
        FloatTime,
        Num
    };
    template<ApiEnum>
    struct Api
    {
        static inline int64_t Call(int argNum, int32_t firstOperand, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            static_assert(false, "Unimplemented api !");
            return 0;
        }
    };
    template<>
    struct Api<ApiEnum::Assert>
    {
        static inline int64_t Call(int argNum, int32_t firstOperand, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            bool isGlobal;
            TypeEnum ty;
            int32_t index;
            DecodeOperand2(firstOperand, isGlobal, ty, index);
            int64_t cond = GetVarInt(isGlobal, index, intLocals, intGlobals);
            if (argNum == 1) {
                _ASSERT(cond != 0);
            }
            else if (argNum == 2) {
                ++pos;
                int operand = codes[pos];
                bool isGlobal2;
                TypeEnum ty2;
                int32_t index2;
                DecodeOperand1(operand, isGlobal2, ty2, index2);
                std::string msg = GetVarString(isGlobal2, index2, strLocals, strGlobals);
                //_ASSERTEX(cond != 0, msg);
                printf("%s", msg.c_str());
                _ASSERT(cond != 0);
            }
            return 0;
        }
    };
    template<>
    struct Api<ApiEnum::Printf>
    {
        static inline int64_t Call(int argNum, int32_t firstOperand, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            bool isGlobal;
            TypeEnum ty;
            int32_t index;
            DecodeOperand2(firstOperand, isGlobal, ty, index);
            const std::string& fmt = GetVarString(isGlobal, index, strLocals, strGlobals);

            std::vector<int64_t> args{};
            args.reserve(argNum - 1);

            for (int i = 1; i < argNum; i += 2) {
                ++pos;
                int operand = codes[pos];

                if (i < argNum) {
                    int64_t val = 0;

                    bool isGlobal1;
                    TypeEnum ty1;
                    int32_t index1;
                    DecodeOperand1(operand, isGlobal1, ty1, index1);
                    switch (ty1) {
                    case TypeEnum::Int: {
                        val = GetVarInt(isGlobal1, index1, intLocals, intGlobals);
                    }break;
                    case TypeEnum::Float: {
                        double v = GetVarFloat(isGlobal1, index1, fltLocals, fltGlobals);
                        val = *reinterpret_cast<int64_t*>(&v);
                    }break;
                    case TypeEnum::String: {
                        const std::string& v = GetVarString(isGlobal1, index1, strLocals, strGlobals);
                        val = reinterpret_cast<int64_t>(v.c_str());
                    }break;
                    }

                    args.push_back(val);
                }
                if (i + 1 < argNum) {
                    int64_t val = 0;

                    bool isGlobal2;
                    TypeEnum ty2;
                    int32_t index2;
                    DecodeOperand2(operand, isGlobal2, ty2, index2);
                    switch (ty2) {
                    case TypeEnum::Int: {
                        val = GetVarInt(isGlobal2, index2, intLocals, intGlobals);
                    }break;
                    case TypeEnum::Float: {
                        double v = GetVarFloat(isGlobal2, index2, fltLocals, fltGlobals);
                        val = *reinterpret_cast<int64_t*>(&v);
                    }break;
                    case TypeEnum::String: {
                        const std::string& v = GetVarString(isGlobal2, index2, strLocals, strGlobals);
                        val = reinterpret_cast<int64_t>(v.c_str());
                    }break;
                    }

                    args.push_back(val);
                }
            }

            int64_t ct = myprintf(fmt.c_str(), args.data());
            return ct;
        }
    };
    template<>
    struct Api<ApiEnum::Format>
    {
        static inline std::string Call(int argNum, int32_t firstOperand, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            bool isGlobal;
            TypeEnum ty;
            int32_t index;
            DecodeOperand2(firstOperand, isGlobal, ty, index);
            const std::string& fmt = GetVarString(isGlobal, index, strLocals, strGlobals);

            std::vector<int64_t> args{};
            args.reserve(argNum - 1);
            const int c_strbuf_size = 4096;
            std::vector<int8_t> strbuf(c_strbuf_size + 1);
            char* pbuf = reinterpret_cast<char*>(strbuf.data());

            for (int i = 1; i < argNum; i += 2) {
                ++pos;
                int operand = codes[pos];

                if (i < argNum) {
                    int64_t val = 0;

                    bool isGlobal1;
                    TypeEnum ty1;
                    int32_t index1;
                    DecodeOperand1(operand, isGlobal1, ty1, index1);
                    switch (ty1) {
                    case TypeEnum::Int: {
                        val = GetVarInt(isGlobal1, index1, intLocals, intGlobals);
                    }break;
                    case TypeEnum::Float: {
                        double v = GetVarFloat(isGlobal1, index1, fltLocals, fltGlobals);
                        val = *reinterpret_cast<int64_t*>(&v);
                    }break;
                    case TypeEnum::String: {
                        const std::string& v = GetVarString(isGlobal1, index1, strLocals, strGlobals);
                        val = reinterpret_cast<int64_t>(v.c_str());
                    }break;
                    }

                    args.push_back(val);
                }
                if (i + 1 < argNum) {
                    int64_t val = 0;

                    bool isGlobal2;
                    TypeEnum ty2;
                    int32_t index2;
                    DecodeOperand2(operand, isGlobal2, ty2, index2);
                    switch (ty2) {
                    case TypeEnum::Int: {
                        val = GetVarInt(isGlobal2, index2, intLocals, intGlobals);
                    }break;
                    case TypeEnum::Float: {
                        double v = GetVarFloat(isGlobal2, index2, fltLocals, fltGlobals);
                        val = *reinterpret_cast<int64_t*>(&v);
                    }break;
                    case TypeEnum::String: {
                        const std::string& v = GetVarString(isGlobal2, index2, strLocals, strGlobals);
                        val = reinterpret_cast<int64_t>(v.c_str());
                    }break;
                    }

                    args.push_back(val);
                }
            }

            int64_t ct = mysnprintf(pbuf, c_strbuf_size + 1, fmt.c_str(), args.data());
            pbuf[c_strbuf_size] = 0;
            return std::string(pbuf);
        }
    };
    template<>
    struct Api<ApiEnum::Time>
    {
        static inline int64_t Call(int argNum, int32_t firstOperand, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            auto&& duration = std::chrono::high_resolution_clock::now() - g_start_time;
            auto&& ms = std::chrono::duration_cast<std::chrono::microseconds>(duration);
            return ms.count();
        }
    };
    template<>
    struct Api<ApiEnum::FloatTime>
    {
        static inline double Call(int argNum, int32_t firstOperand, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            using double_microseconds = std::chrono::duration<double, std::micro>;
            auto&& duration = std::chrono::high_resolution_clock::now() - g_start_time;
            auto&& ms = std::chrono::duration_cast<double_microseconds>(duration);
            return ms.count();
        }
    };
    static inline void DoCall(int32_t opcode, InsEnum op, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
    {
        ++pos;
        int32_t operand = codes[pos];
        int32_t argNum;
        bool isGlobal;
        TypeEnum ty;
        int32_t index;
        DecodeOpcode(opcode, op, argNum, isGlobal, ty, index);
        int32_t apiIndex;
        DecodeOperand1(operand, apiIndex);
        ApiEnum api = static_cast<ApiEnum>(apiIndex);
        switch (api) {
        case ApiEnum::Assert: {
            int64_t val = Api<ApiEnum::Assert>::Call(argNum, operand, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
            _ASSERTE(ty == TypeEnum::Int);
            SetVarInt(isGlobal, index, val, intLocals, intGlobals);
        }break;
        case ApiEnum::Printf: {
            int64_t val = Api<ApiEnum::Printf>::Call(argNum, operand, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
            _ASSERTE(ty == TypeEnum::Int);
            SetVarInt(isGlobal, index, val, intLocals, intGlobals);;
        }break;
        case ApiEnum::Format: {
            std::string val = Api<ApiEnum::Format>::Call(argNum, operand, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
            _ASSERTE(ty == TypeEnum::String);
            SetVarString(isGlobal, index, val, strLocals, strGlobals);
        }break;
        case ApiEnum::Time: {
            int64_t val = Api<ApiEnum::Time>::Call(argNum, operand, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
            _ASSERTE(ty == TypeEnum::Int);
            SetVarInt(isGlobal, index, val, intLocals, intGlobals);
        }break;
        case ApiEnum::FloatTime: {
            double val = Api<ApiEnum::FloatTime>::Call(argNum, operand, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
            _ASSERT(ty == TypeEnum::Float);
            SetVarFloat(isGlobal, index, val, fltLocals, fltGlobals);
        }break;
        }
    }
    static inline void DoRet(int32_t opcode, InsEnum op, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals, bool& retVal)
    {
        int32_t argNum;
        bool isGlobal;
        TypeEnum ty;
        int32_t index;
        DecodeOpcode(opcode, op, argNum, isGlobal, ty, index);
        switch (ty) {
        case TypeEnum::Int: {
            int64_t val = GetVarInt(isGlobal, index, intLocals, intGlobals);
            retVal = val != 0;
        }break;
        case TypeEnum::Float: {
            double val = GetVarFloat(isGlobal, index, fltLocals, fltGlobals);
            retVal = val != 0;
        }break;
        }
    }
    static inline void DoJmp(int32_t opcode, InsEnum op, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
    {
        int32_t enterPos = pos;
        int32_t offset;
        DecodeOpcode(opcode, op, offset);
        pos = enterPos - 1 + offset;
    }
    static inline void DoJmpIf(int32_t opcode, InsEnum op, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
    {
        int32_t enterPos = pos;
        ++pos;
        int32_t operand = codes[pos];

        int32_t offset;
        DecodeOpcode(opcode, op, offset);
        bool isGlobal1;
        TypeEnum ty1;
        int32_t index1;
        DecodeOperand1(operand, isGlobal1, ty1, index1);
        switch (ty1) {
        case TypeEnum::Int: {
            int64_t val = GetVarInt(isGlobal1, index1, intLocals, intGlobals);
            if (val != 0) {
                pos = enterPos - 1 + offset;
            }
        }break;
        case TypeEnum::Float: {
            double val = GetVarFloat(isGlobal1, index1, fltLocals, fltGlobals);
            if (val != 0) {
                pos = enterPos - 1 + offset;
            }
        }break;
        case TypeEnum::String: {
            const std::string& val = GetVarString(isGlobal1, index1, strLocals, strGlobals);
            if (val.length() > 0) {
                pos = enterPos - 1 + offset;
            }
        }break;
        }
    }
    static inline void DoJmpIfNot(int32_t opcode, InsEnum op, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
    {
        int32_t enterPos = pos;
        ++pos;
        int32_t operand = codes[pos];

        int32_t offset;
        DecodeOpcode(opcode, op, offset);
        bool isGlobal1;
        TypeEnum ty1;
        int32_t index1;
        DecodeOperand1(operand, isGlobal1, ty1, index1);
        switch (ty1) {
        case TypeEnum::Int: {
            int64_t val = GetVarInt(isGlobal1, index1, intLocals, intGlobals);
            if (val == 0) {
                pos = enterPos - 1 + offset;
            }
        }break;
        case TypeEnum::Float: {
            double val = GetVarFloat(isGlobal1, index1, fltLocals, fltGlobals);
            if (val == 0) {
                pos = enterPos - 1 + offset;
            }
        }break;
        case TypeEnum::String: {
            const std::string& val = GetVarString(isGlobal1, index1, strLocals, strGlobals);
            if (val.length() == 0) {
                pos = enterPos - 1 + offset;
            }
        }break;
        }
    }
    static inline void DoInc(int32_t opcode, InsEnum op, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
    {
        int32_t argNum;
        bool isGlobal;
        TypeEnum ty;
        int32_t index;
        DecodeOpcode(opcode, op, argNum, isGlobal, ty, index);
        _ASSERT(ty == TypeEnum::Int);
        int64_t val = GetVarInt(isGlobal, index, intLocals, intGlobals);
        SetVarInt(isGlobal, index, val + 1, intLocals, intGlobals);
    }
    static inline void DoIncFlt(int32_t opcode, InsEnum op, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
    {
        int32_t argNum;
        bool isGlobal;
        TypeEnum ty;
        int32_t index;
        DecodeOpcode(opcode, op, argNum, isGlobal, ty, index);
        _ASSERT(ty == TypeEnum::Float);
        double val = GetVarFloat(isGlobal, index, fltLocals, fltGlobals);
        SetVarFloat(isGlobal, index, val + 1, fltLocals, fltGlobals);
    }
    static inline void DoIncVal(int32_t opcode, InsEnum op, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
    {
        ++pos;
        int32_t operand = codes[pos];

        int32_t argNum;
        bool isGlobal;
        TypeEnum ty;
        int32_t index;
        DecodeOpcode(opcode, op, argNum, isGlobal, ty, index);
        bool isGlobal1;
        TypeEnum ty1;
        int32_t index1;
        DecodeOperand1(operand, isGlobal1, ty1, index1);
        _ASSERT(ty1 == TypeEnum::Int && ty == TypeEnum::Int);
        int64_t inc = GetVarInt(isGlobal1, index1, intLocals, intGlobals);
        int64_t val = GetVarInt(isGlobal, index, intLocals, intGlobals);
        SetVarInt(isGlobal, index, val + inc, intLocals, intGlobals);
    }
    static inline void DoIncValFlt(int32_t opcode, InsEnum op, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
    {
        ++pos;
        int32_t operand = codes[pos];

        int32_t argNum;
        bool isGlobal;
        TypeEnum ty;
        int32_t index;
        DecodeOpcode(opcode, op, argNum, isGlobal, ty, index);
        bool isGlobal1;
        TypeEnum ty1;
        int32_t index1;
        DecodeOperand1(operand, isGlobal1, ty1, index1);
        _ASSERT(ty1 == TypeEnum::Float && ty == TypeEnum::Float);
        double inc = GetVarFloat(isGlobal1, index1, fltLocals, fltGlobals);
        double val = GetVarFloat(isGlobal, index, fltLocals, fltGlobals);
        SetVarFloat(isGlobal, index, val + inc, fltLocals, fltGlobals);
    }
    static inline void DoMov(int32_t opcode, InsEnum op, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
    {
        ++pos;
        int32_t operand = codes[pos];

        int32_t argNum;
        bool isGlobal;
        TypeEnum ty;
        int32_t index;
        DecodeOpcode(opcode, op, argNum, isGlobal, ty, index);
        bool isGlobal1;
        TypeEnum ty1;
        int32_t index1;
        DecodeOperand1(operand, isGlobal1, ty1, index1);
        _ASSERT(ty1 == TypeEnum::Int && ty == TypeEnum::Int);
        int64_t val = GetVarInt(isGlobal1, index1, intLocals, intGlobals);
        SetVarInt(isGlobal, index, val, intLocals, intGlobals);
    }
    static inline void DoMovFlt(int32_t opcode, InsEnum op, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
    {
        ++pos;
        int32_t operand = codes[pos];

        int32_t argNum;
        bool isGlobal;
        TypeEnum ty;
        int32_t index;
        DecodeOpcode(opcode, op, argNum, isGlobal, ty, index);
        bool isGlobal1;
        TypeEnum ty1;
        int32_t index1;
        DecodeOperand1(operand, isGlobal1, ty1, index1);
        _ASSERT(ty1 == TypeEnum::Float && ty == TypeEnum::Float);
        double val = GetVarFloat(isGlobal1, index1, fltLocals, fltGlobals);
        SetVarFloat(isGlobal, index, val, fltLocals, fltGlobals);
    }
    static inline void DoMovStr(int32_t opcode, InsEnum op, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
    {
        ++pos;
        int32_t operand = codes[pos];

        int32_t argNum;
        bool isGlobal;
        TypeEnum ty;
        int32_t index;
        DecodeOpcode(opcode, op, argNum, isGlobal, ty, index);
        bool isGlobal1;
        TypeEnum ty1;
        int32_t index1;
        DecodeOperand1(operand, isGlobal1, ty1, index1);
        _ASSERT(ty1 == TypeEnum::String && ty == TypeEnum::String);
        const std::string& val = GetVarString(isGlobal1, index1, strLocals, strGlobals);
        SetVarString(isGlobal, index, val, strLocals, strGlobals);
    }
    static inline void DoArrayGet(int32_t opcode, InsEnum op, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
    {
        ++pos;
        int32_t operand = codes[pos];

        int32_t argNum;
        bool isGlobal;
        TypeEnum ty;
        int32_t index;
        DecodeOpcode(opcode, op, argNum, isGlobal, ty, index);
        bool isGlobal1;
        TypeEnum ty1;
        int32_t index1;
        DecodeOperand1(operand, isGlobal1, ty1, index1);
        bool isGlobal2;
        TypeEnum ty2;
        int32_t index2;
        DecodeOperand2(operand, isGlobal2, ty2, index2);
        _ASSERT(ty1 == TypeEnum::Int && ty2 == TypeEnum::Int && ty == TypeEnum::Int);
        int32_t ix = static_cast<int32_t>(GetVarInt(isGlobal2, index2, intLocals, intGlobals));
        int64_t val = GetVarInt(isGlobal1, index1 + ix, intLocals, intGlobals);
        SetVarInt(isGlobal, index, val, intLocals, intGlobals);
    }
    static inline void DoArrayGetFlt(int32_t opcode, InsEnum op, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
    {
        ++pos;
        int32_t operand = codes[pos];

        int32_t argNum;
        bool isGlobal;
        TypeEnum ty;
        int32_t index;
        DecodeOpcode(opcode, op, argNum, isGlobal, ty, index);
        bool isGlobal1;
        TypeEnum ty1;
        int32_t index1;
        DecodeOperand1(operand, isGlobal1, ty1, index1);
        bool isGlobal2;
        TypeEnum ty2;
        int32_t index2;
        DecodeOperand2(operand, isGlobal2, ty2, index2);
        _ASSERT(ty1 == TypeEnum::Float && ty2 == TypeEnum::Int && ty == TypeEnum::Float);
        int32_t ix = static_cast<int32_t>(GetVarInt(isGlobal2, index2, intLocals, intGlobals));
        double val = GetVarFloat(isGlobal1, index1 + ix, fltLocals, fltGlobals);
        SetVarFloat(isGlobal, index, val, fltLocals, fltGlobals);
    }
    static inline void DoArrayGetStr(int32_t opcode, InsEnum op, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
    {
        ++pos;
        int32_t operand = codes[pos];

        int32_t argNum;
        bool isGlobal;
        TypeEnum ty;
        int32_t index;
        DecodeOpcode(opcode, op, argNum, isGlobal, ty, index);
        bool isGlobal1;
        TypeEnum ty1;
        int32_t index1;
        DecodeOperand1(operand, isGlobal1, ty1, index1);
        bool isGlobal2;
        TypeEnum ty2;
        int32_t index2;
        DecodeOperand2(operand, isGlobal2, ty2, index2);
        _ASSERT(ty1 == TypeEnum::String && ty2 == TypeEnum::Int && ty == TypeEnum::String);
        int32_t ix = static_cast<int32_t>(GetVarInt(isGlobal2, index2, intLocals, intGlobals));
        const std::string& val = GetVarString(isGlobal1, index1 + ix, strLocals, strGlobals);
        SetVarString(isGlobal, index, val, strLocals, strGlobals);
    }
    static inline void DoArraySet(int32_t opcode, InsEnum op, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
    {
        ++pos;
        int32_t operand = codes[pos];

        int32_t argNum;
        bool isGlobal;
        TypeEnum ty;
        int32_t index;
        DecodeOpcode(opcode, op, argNum, isGlobal, ty, index);
        bool isGlobal1;
        TypeEnum ty1;
        int32_t index1;
        DecodeOperand1(operand, isGlobal1, ty1, index1);
        bool isGlobal2;
        TypeEnum ty2;
        int32_t index2;
        DecodeOperand2(operand, isGlobal2, ty2, index2);
        _ASSERT(ty1 == TypeEnum::Int && ty2 == TypeEnum::Int && ty == TypeEnum::Int);
        int32_t ix = static_cast<int32_t>(GetVarInt(isGlobal1, index1, intLocals, intGlobals));
        int64_t val = GetVarInt(isGlobal2, index2, intLocals, intGlobals);
        SetVarInt(isGlobal, index + ix, val, intLocals, intGlobals);
    }
    static inline void DoArraySetFlt(int32_t opcode, InsEnum op, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
    {
        ++pos;
        int32_t operand = codes[pos];

        int32_t argNum;
        bool isGlobal;
        TypeEnum ty;
        int32_t index;
        DecodeOpcode(opcode, op, argNum, isGlobal, ty, index);
        bool isGlobal1;
        TypeEnum ty1;
        int32_t index1;
        DecodeOperand1(operand, isGlobal1, ty1, index1);
        bool isGlobal2;
        TypeEnum ty2;
        int32_t index2;
        DecodeOperand2(operand, isGlobal2, ty2, index2);
        _ASSERT(ty1 == TypeEnum::Int && ty2 == TypeEnum::Float && ty == TypeEnum::Float);
        int32_t ix = static_cast<int32_t>(GetVarInt(isGlobal1, index1, intLocals, intGlobals));
        double val = GetVarFloat(isGlobal2, index2, fltLocals, fltGlobals);
        SetVarFloat(isGlobal, index + ix, val, fltLocals, fltGlobals);
    }
    static inline void DoArraySetStr(int32_t opcode, InsEnum op, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
    {
        ++pos;
        int32_t operand = codes[pos];

        int32_t argNum;
        bool isGlobal;
        TypeEnum ty;
        int32_t index;
        DecodeOpcode(opcode, op, argNum, isGlobal, ty, index);
        bool isGlobal1;
        TypeEnum ty1;
        int32_t index1;
        DecodeOperand1(operand, isGlobal1, ty1, index1);
        bool isGlobal2;
        TypeEnum ty2;
        int32_t index2;
        DecodeOperand2(operand, isGlobal2, ty2, index2);
        _ASSERT(ty1 == TypeEnum::Int && ty2 == TypeEnum::String && ty == TypeEnum::String);
        int32_t ix = static_cast<int32_t>(GetVarInt(isGlobal1, index1, intLocals, intGlobals));
        const std::string& val = GetVarString(isGlobal2, index2, strLocals, strGlobals);
        SetVarString(isGlobal, index + ix, val, strLocals, strGlobals);
    }
    static inline void DoIToF(int32_t opcode, InsEnum op, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, FloatLocals& fltLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals)
    {
        ++pos;
        int32_t operand = codes[pos];

        int32_t argNum;
        bool isGlobal;
        TypeEnum ty;
        int32_t index;
        DecodeOpcode(opcode, op, argNum, isGlobal, ty, index);
        bool isGlobal1;
        TypeEnum ty1;
        int32_t index1;
        DecodeOperand1(operand, isGlobal1, ty1, index1);

        _ASSERTE(ty == TypeEnum::Float);
        _ASSERTE(ty1 == TypeEnum::Int);
        int64_t val = GetVarInt(isGlobal1, index1, intLocals, intGlobals);
        SetVarFloat(isGlobal, index, *reinterpret_cast<double*>(&val), fltLocals, fltGlobals);
    }
    static inline void DoFToI(int32_t opcode, InsEnum op, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, FloatLocals& fltLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals)
    {
        ++pos;
        int32_t operand = codes[pos];

        int32_t argNum;
        bool isGlobal;
        TypeEnum ty;
        int32_t index;
        DecodeOpcode(opcode, op, argNum, isGlobal, ty, index);
        bool isGlobal1;
        TypeEnum ty1;
        int32_t index1;
        DecodeOperand1(operand, isGlobal1, ty1, index1);

        _ASSERTE(ty == TypeEnum::Int);
        _ASSERTE(ty1 == TypeEnum::Float);
        double val = GetVarFloat(isGlobal1, index1, fltLocals, fltGlobals);
        SetVarInt(isGlobal, index, *reinterpret_cast<int64_t*>(&val), intLocals, intGlobals);
    }
    static inline void DoArgc(int32_t opcode, InsEnum op, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, IntGlobals& intGlobals, int32_t argc)
    {
        ++pos;
        int32_t operand = codes[pos];

        int32_t argNum;
        bool isGlobal;
        TypeEnum ty;
        int32_t index;
        DecodeOpcode(opcode, op, argNum, isGlobal, ty, index);

        SetVarInt(isGlobal, index, argc, intLocals, intGlobals);
    }
    static inline void DoArgv(int32_t opcode, InsEnum op, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, IntGlobals& intGlobals, int32_t argc, int64_t argv[])
    {
        ++pos;
        int32_t operand = codes[pos];

        int32_t argNum;
        bool isGlobal;
        TypeEnum ty;
        int32_t index;
        DecodeOpcode(opcode, op, argNum, isGlobal, ty, index);
        bool isGlobal1;
        TypeEnum ty1;
        int32_t index1;
        DecodeOperand1(operand, isGlobal1, ty1, index1);

        int64_t ix = GetVarInt(isGlobal1, index1, intLocals, intGlobals);
        if (ix >= 0 && ix < argc) {
            int64_t val = argv[ix];
            SetVarInt(isGlobal, index, val, intLocals, intGlobals);
        }
        else {
            SetVarInt(isGlobal, index, 0, intLocals, intGlobals);
        }
    }
    static inline void DoAddr(int32_t opcode, InsEnum op, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
    {
        ++pos;
        int32_t operand = codes[pos];

        int32_t argNum;
        bool isGlobal;
        TypeEnum ty;
        int32_t index;
        DecodeOpcode(opcode, op, argNum, isGlobal, ty, index);
        bool isGlobal1;
        TypeEnum ty1;
        int32_t index1;
        DecodeOperand1(operand, isGlobal1, ty1, index1);

        int64_t val = 0;
        switch (ty1) {
        case TypeEnum::Int:
            if (isGlobal1) {
                val = reinterpret_cast<int64_t>(intGlobals.data() + index1);
            }
            else {
                val = reinterpret_cast<int64_t>(intLocals.data() + index1);
            }
            break;
        case TypeEnum::Float:
            if (isGlobal1) {
                val = reinterpret_cast<int64_t>(fltGlobals.data() + index1);
            }
            else {
                val = reinterpret_cast<int64_t>(fltLocals.data() + index1);
            }
            break;
        case TypeEnum::String:
            if (isGlobal1) {
                val = reinterpret_cast<int64_t>(strGlobals.data() + index1);
            }
            else {
                val = reinterpret_cast<int64_t>(strLocals.data() + index1);
            }
            break;
        }
        SetVarInt(isGlobal, index, val, intLocals, intGlobals);
    }
    static inline void DoPtrGet(int32_t opcode, InsEnum op, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
    {
        ++pos;
        int32_t operand = codes[pos];

        int32_t argNum;
        bool isGlobal;
        TypeEnum ty;
        int32_t index;
        DecodeOpcode(opcode, op, argNum, isGlobal, ty, index);
        bool isGlobal1;
        TypeEnum ty1;
        int32_t index1;
        DecodeOperand1(operand, isGlobal1, ty1, index1);
        bool isGlobal2;
        TypeEnum ty2;
        int32_t index2;
        DecodeOperand2(operand, isGlobal2, ty2, index2);

        int64_t addr = GetVarInt(isGlobal1, index1, intLocals, intGlobals);
        int64_t size = GetVarInt(isGlobal2, index2, intLocals, intGlobals);
        switch (ty) {
        case TypeEnum::Int: {
            int64_t val = 0;
            switch (size) {
            case 1:
                val = *reinterpret_cast<int8_t*>(addr);
                break;
            case 2:
                val = *reinterpret_cast<int16_t*>(addr);
                break;
            case 4:
                val = *reinterpret_cast<int32_t*>(addr);
                break;
            case 8:
                val = *reinterpret_cast<int64_t*>(addr);
                break;
            }
            SetVarInt(isGlobal, index, val, intLocals, intGlobals);
        }break;
        case TypeEnum::Float: {
            double val = 0;
            switch (size) {
            case 4:
                val = *reinterpret_cast<float*>(addr);
                break;
            case 8:
                val = *reinterpret_cast<double*>(addr);
                break;
            }
            SetVarFloat(isGlobal, index, val, fltLocals, fltGlobals);
        }break;
        case TypeEnum::String: {
            //dangerous!!!
            std::string val{};
            if (size == 8) {
                val = reinterpret_cast<const char*>(addr);
            }
            SetVarString(isGlobal, index, val, strLocals, strGlobals);
        }break;
        }
    }
    static inline void DoPtrSet(int32_t opcode, InsEnum op, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
    {
        ++pos;
        int32_t operand = codes[pos];

        int32_t argNum;
        bool isGlobal;
        TypeEnum ty;
        int32_t index;
        DecodeOpcode(opcode, op, argNum, isGlobal, ty, index);
        bool isGlobal1;
        TypeEnum ty1;
        int32_t index1;
        DecodeOperand1(operand, isGlobal1, ty1, index1);
        bool isGlobal2;
        TypeEnum ty2;
        int32_t index2;
        DecodeOperand2(operand, isGlobal2, ty2, index2);

        int64_t addr = GetVarInt(isGlobal, index, intLocals, intGlobals);
        int64_t size = GetVarInt(isGlobal1, index1, intLocals, intGlobals);
        switch (ty) {
        case TypeEnum::Int: {
            int64_t val = GetVarInt(isGlobal2, index2, intLocals, intGlobals);
            switch (size) {
            case 1:
                *reinterpret_cast<int8_t*>(addr) = static_cast<int8_t>(val);
                break;
            case 2:
                *reinterpret_cast<int16_t*>(addr) = static_cast<int16_t>(val);
                break;
            case 4:
                *reinterpret_cast<int32_t*>(addr) = static_cast<int32_t>(val);
                break;
            case 8:
                *reinterpret_cast<int64_t*>(addr) = val;
                break;
            }
        }break;
        case TypeEnum::Float: {
            double val = GetVarFloat(isGlobal2, index2, fltLocals, fltGlobals);
            switch (size) {
            case 4:
                *reinterpret_cast<float*>(addr) = static_cast<float>(val);
                break;
            case 8:
                *reinterpret_cast<double*>(addr) = val;
                break;
            }
        }break;
        case TypeEnum::String: {
            //dangerous!!!
            if (size == 8) {
                const std::string& val = GetVarString(isGlobal2, index2, strLocals, strGlobals);
                char* tstr = reinterpret_cast<char*>(addr);
                strcpy_s(tstr, std::strlen(tstr), val.c_str());
            }
        }break;
        }
    }
    static inline void DoJaggedPtr(int32_t opcode, InsEnum op, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
    {
        int32_t argNum;
        bool isGlobal;
        TypeEnum ty;
        int32_t index;
        DecodeOpcode(opcode, op, argNum, isGlobal, ty, index);

        int64_t addr = 0;
        for (int i = 0; i < argNum; i += 2) {
            ++pos;
            int32_t operand = codes[pos];
            if (i < argNum) {
                bool isGlobal1;
                TypeEnum ty1;
                int32_t index1;
                DecodeOperand1(operand, isGlobal1, ty1, index1);
                if (i == 0) {
                    addr = GetVarInt(isGlobal1, index1, intLocals, intGlobals);
                }
                else {
                    int64_t offset = GetVarInt(isGlobal1, index1, intLocals, intGlobals);
                    addr = *reinterpret_cast<int64_t*>(addr + offset);
                }
            }
            if (i + 1 < argNum) {
                bool isGlobal2;
                TypeEnum ty2;
                int32_t index2;
                DecodeOperand2(operand, isGlobal2, ty2, index2);
                int64_t offset = GetVarInt(isGlobal2, index2, intLocals, intGlobals);
                addr = *reinterpret_cast<int64_t*>(addr + offset);
            }
        }
        SetVarInt(isGlobal, index, addr, intLocals, intGlobals);
    }

    //unary or binary operation
    struct NegOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool argIsGlobal, TypeEnum argTy, int argIndex, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && argTy == TypeEnum::Int);
            int64_t val = GetVarInt(argIsGlobal, argIndex, intLocals, intGlobals);
            SetVarInt(isGlobal, index, -val, intLocals, intGlobals);
        }
    };
    struct NegFltOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool argIsGlobal, TypeEnum argTy, int argIndex, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Float && argTy == TypeEnum::Float);
            double val = GetVarFloat(argIsGlobal, argIndex, fltLocals, fltGlobals);
            SetVarFloat(isGlobal, index, -val, fltLocals, fltGlobals);
        }
    };
    struct AddOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && arg1Ty == TypeEnum::Int && arg2Ty == TypeEnum::Int);
            int64_t val1 = GetVarInt(arg1IsGlobal, arg1Index, intLocals, intGlobals);
            int64_t val2 = GetVarInt(arg2IsGlobal, arg2Index, intLocals, intGlobals);
            SetVarInt(isGlobal, index, val1 + val2, intLocals, intGlobals);
        }
    };
    struct AddFltOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Float && arg1Ty == TypeEnum::Float && arg2Ty == TypeEnum::Float);
            double val1 = GetVarFloat(arg1IsGlobal, arg1Index, fltLocals, fltGlobals);
            double val2 = GetVarFloat(arg2IsGlobal, arg2Index, fltLocals, fltGlobals);
            SetVarFloat(isGlobal, index, val1 + val2, fltLocals, fltGlobals);
        }
    };
    struct AddStrOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::String && arg1Ty == TypeEnum::String && arg2Ty == TypeEnum::String);
            const std::string& val1 = GetVarString(arg1IsGlobal, arg1Index, strLocals, strGlobals);
            const std::string& val2 = GetVarString(arg2IsGlobal, arg2Index, strLocals, strGlobals);
            SetVarString(isGlobal, index, val1 + val2, strLocals, strGlobals);
        }
    };
    struct SubOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && arg1Ty == TypeEnum::Int && arg2Ty == TypeEnum::Int);
            int64_t val1 = GetVarInt(arg1IsGlobal, arg1Index, intLocals, intGlobals);
            int64_t val2 = GetVarInt(arg2IsGlobal, arg2Index, intLocals, intGlobals);
            SetVarInt(isGlobal, index, val1 - val2, intLocals, intGlobals);
        }
    };
    struct SubFltOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Float && arg1Ty == TypeEnum::Float && arg2Ty == TypeEnum::Float);
            double val1 = GetVarFloat(arg1IsGlobal, arg1Index, fltLocals, fltGlobals);
            double val2 = GetVarFloat(arg2IsGlobal, arg2Index, fltLocals, fltGlobals);
            SetVarFloat(isGlobal, index, val1 - val2, fltLocals, fltGlobals);
        }
    };
    struct MulOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && arg1Ty == TypeEnum::Int && arg2Ty == TypeEnum::Int);
            int64_t val1 = GetVarInt(arg1IsGlobal, arg1Index, intLocals, intGlobals);
            int64_t val2 = GetVarInt(arg2IsGlobal, arg2Index, intLocals, intGlobals);
            SetVarInt(isGlobal, index, val1 * val2, intLocals, intGlobals);
        }
    };
    struct MulFltOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Float && arg1Ty == TypeEnum::Float && arg2Ty == TypeEnum::Float);
            double val1 = GetVarFloat(arg1IsGlobal, arg1Index, fltLocals, fltGlobals);
            double val2 = GetVarFloat(arg2IsGlobal, arg2Index, fltLocals, fltGlobals);
            SetVarFloat(isGlobal, index, val1 * val2, fltLocals, fltGlobals);
        }
    };
    struct DivOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && arg1Ty == TypeEnum::Int && arg2Ty == TypeEnum::Int);
            int64_t val1 = GetVarInt(arg1IsGlobal, arg1Index, intLocals, intGlobals);
            int64_t val2 = GetVarInt(arg2IsGlobal, arg2Index, intLocals, intGlobals);
            SetVarInt(isGlobal, index, val1 / val2, intLocals, intGlobals);
        }
    };
    struct DivFltOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Float && arg1Ty == TypeEnum::Float && arg2Ty == TypeEnum::Float);
            double val1 = GetVarFloat(arg1IsGlobal, arg1Index, fltLocals, fltGlobals);
            double val2 = GetVarFloat(arg2IsGlobal, arg2Index, fltLocals, fltGlobals);
            SetVarFloat(isGlobal, index, val1 / val2, fltLocals, fltGlobals);
        }
    };
    struct ModOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && arg1Ty == TypeEnum::Int && arg2Ty == TypeEnum::Int);
            int64_t val1 = GetVarInt(arg1IsGlobal, arg1Index, intLocals, intGlobals);
            int64_t val2 = GetVarInt(arg2IsGlobal, arg2Index, intLocals, intGlobals);
            SetVarInt(isGlobal, index, val1 % val2, intLocals, intGlobals);
        }
    };
    struct ModFltOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Float && arg1Ty == TypeEnum::Float && arg2Ty == TypeEnum::Float);
            double val1 = GetVarFloat(arg1IsGlobal, arg1Index, fltLocals, fltGlobals);
            double val2 = GetVarFloat(arg2IsGlobal, arg2Index, fltLocals, fltGlobals);
            SetVarFloat(isGlobal, index, std::fmod(val1, val2), fltLocals, fltGlobals);
        }
    };
    struct AndOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && arg1Ty == TypeEnum::Int && arg2Ty == TypeEnum::Int);
            int64_t val1 = GetVarInt(arg1IsGlobal, arg1Index, intLocals, intGlobals);
            int64_t val2 = GetVarInt(arg2IsGlobal, arg2Index, intLocals, intGlobals);
            SetVarInt(isGlobal, index, val1 && val2 ? 1 : 0, intLocals, intGlobals);
        }
    };
    struct OrOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && arg1Ty == TypeEnum::Int && arg2Ty == TypeEnum::Int);
            int64_t val1 = GetVarInt(arg1IsGlobal, arg1Index, intLocals, intGlobals);
            int64_t val2 = GetVarInt(arg2IsGlobal, arg2Index, intLocals, intGlobals);
            SetVarInt(isGlobal, index, val1 || val2 ? 1 : 0, intLocals, intGlobals);
        }
    };
    struct NotOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool argIsGlobal, TypeEnum argTy, int argIndex, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && argTy == TypeEnum::Int);
            int64_t val = GetVarInt(argIsGlobal, argIndex, intLocals, intGlobals);
            SetVarInt(isGlobal, index, val ? 1 : 0, intLocals, intGlobals);
        }
    };
    struct GTOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && arg1Ty == TypeEnum::Int && arg2Ty == TypeEnum::Int);
            int64_t val1 = GetVarInt(arg1IsGlobal, arg1Index, intLocals, intGlobals);
            int64_t val2 = GetVarInt(arg2IsGlobal, arg2Index, intLocals, intGlobals);
            SetVarInt(isGlobal, index, val1 > val2 ? 1 : 0, intLocals, intGlobals);
        }
    };
    struct GTFltOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && arg1Ty == TypeEnum::Float && arg2Ty == TypeEnum::Float);
            double val1 = GetVarFloat(arg1IsGlobal, arg1Index, fltLocals, fltGlobals);
            double val2 = GetVarFloat(arg2IsGlobal, arg2Index, fltLocals, fltGlobals);
            SetVarInt(isGlobal, index, val1 > val2 ? 1 : 0, intLocals, intGlobals);
        }
    };
    struct GTStrOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && arg1Ty == TypeEnum::String && arg2Ty == TypeEnum::String);
            const std::string& val1 = GetVarString(arg1IsGlobal, arg1Index, strLocals, strGlobals);
            const std::string& val2 = GetVarString(arg2IsGlobal, arg2Index, strLocals, strGlobals);
            SetVarInt(isGlobal, index, val1 > val2 ? 1 : 0, intLocals, intGlobals);
        }
    };
    struct GEOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && arg1Ty == TypeEnum::Int && arg2Ty == TypeEnum::Int);
            int64_t val1 = GetVarInt(arg1IsGlobal, arg1Index, intLocals, intGlobals);
            int64_t val2 = GetVarInt(arg2IsGlobal, arg2Index, intLocals, intGlobals);
            SetVarInt(isGlobal, index, val1 >= val2 ? 1 : 0, intLocals, intGlobals);
        }
    };
    struct GEFltOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && arg1Ty == TypeEnum::Float && arg2Ty == TypeEnum::Float);
            double val1 = GetVarFloat(arg1IsGlobal, arg1Index, fltLocals, fltGlobals);
            double val2 = GetVarFloat(arg2IsGlobal, arg2Index, fltLocals, fltGlobals);
            SetVarInt(isGlobal, index, val1 >= val2 ? 1 : 0, intLocals, intGlobals);
        }
    };
    struct GEStrOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && arg1Ty == TypeEnum::String && arg2Ty == TypeEnum::String);
            const std::string& val1 = GetVarString(arg1IsGlobal, arg1Index, strLocals, strGlobals);
            const std::string& val2 = GetVarString(arg2IsGlobal, arg2Index, strLocals, strGlobals);
            SetVarInt(isGlobal, index, val1 >= val2 ? 1 : 0, intLocals, intGlobals);
        }
    };
    struct EQOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && arg1Ty == TypeEnum::Int && arg2Ty == TypeEnum::Int);
            int64_t val1 = GetVarInt(arg1IsGlobal, arg1Index, intLocals, intGlobals);
            int64_t val2 = GetVarInt(arg2IsGlobal, arg2Index, intLocals, intGlobals);
            SetVarInt(isGlobal, index, val1 == val2 ? 1 : 0, intLocals, intGlobals);
        }
    };
    struct EQFltOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && arg1Ty == TypeEnum::Float && arg2Ty == TypeEnum::Float);
            double val1 = GetVarFloat(arg1IsGlobal, arg1Index, fltLocals, fltGlobals);
            double val2 = GetVarFloat(arg2IsGlobal, arg2Index, fltLocals, fltGlobals);
            SetVarInt(isGlobal, index, val1 == val2 ? 1 : 0, intLocals, intGlobals);
        }
    };
    struct EQStrOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && arg1Ty == TypeEnum::String && arg2Ty == TypeEnum::String);
            const std::string& val1 = GetVarString(arg1IsGlobal, arg1Index, strLocals, strGlobals);
            const std::string& val2 = GetVarString(arg2IsGlobal, arg2Index, strLocals, strGlobals);
            SetVarInt(isGlobal, index, val1 == val2 ? 1 : 0, intLocals, intGlobals);
        }
    };
    struct NEOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && arg1Ty == TypeEnum::Int && arg2Ty == TypeEnum::Int);
            int64_t val1 = GetVarInt(arg1IsGlobal, arg1Index, intLocals, intGlobals);
            int64_t val2 = GetVarInt(arg2IsGlobal, arg2Index, intLocals, intGlobals);
            SetVarInt(isGlobal, index, val1 != val2 ? 1 : 0, intLocals, intGlobals);
        }
    };
    struct NEFltOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && arg1Ty == TypeEnum::Float && arg2Ty == TypeEnum::Float);
            double val1 = GetVarFloat(arg1IsGlobal, arg1Index, fltLocals, fltGlobals);
            double val2 = GetVarFloat(arg2IsGlobal, arg2Index, fltLocals, fltGlobals);
            SetVarInt(isGlobal, index, val1 != val2 ? 1 : 0, intLocals, intGlobals);
        }
    };
    struct NEStrOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && arg1Ty == TypeEnum::String && arg2Ty == TypeEnum::String);
            const std::string& val1 = GetVarString(arg1IsGlobal, arg1Index, strLocals, strGlobals);
            const std::string& val2 = GetVarString(arg2IsGlobal, arg2Index, strLocals, strGlobals);
            SetVarInt(isGlobal, index, val1 != val2 ? 1 : 0, intLocals, intGlobals);
        }
    };
    struct LEOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && arg1Ty == TypeEnum::Int && arg2Ty == TypeEnum::Int);
            int64_t val1 = GetVarInt(arg1IsGlobal, arg1Index, intLocals, intGlobals);
            int64_t val2 = GetVarInt(arg2IsGlobal, arg2Index, intLocals, intGlobals);
            SetVarInt(isGlobal, index, val1 <= val2 ? 1 : 0, intLocals, intGlobals);
        }
    };
    struct LEFltOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && arg1Ty == TypeEnum::Float && arg2Ty == TypeEnum::Float);
            double val1 = GetVarFloat(arg1IsGlobal, arg1Index, fltLocals, fltGlobals);
            double val2 = GetVarFloat(arg2IsGlobal, arg2Index, fltLocals, fltGlobals);
            SetVarInt(isGlobal, index, val1 <= val2 ? 1 : 0, intLocals, intGlobals);
        }
    };
    struct LEStrOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && arg1Ty == TypeEnum::String && arg2Ty == TypeEnum::String);
            const std::string& val1 = GetVarString(arg1IsGlobal, arg1Index, strLocals, strGlobals);
            const std::string& val2 = GetVarString(arg2IsGlobal, arg2Index, strLocals, strGlobals);
            SetVarInt(isGlobal, index, val1 <= val2 ? 1 : 0, intLocals, intGlobals);
        }
    };
    struct LTOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && arg1Ty == TypeEnum::Int && arg2Ty == TypeEnum::Int);
            int64_t val1 = GetVarInt(arg1IsGlobal, arg1Index, intLocals, intGlobals);
            int64_t val2 = GetVarInt(arg2IsGlobal, arg2Index, intLocals, intGlobals);
            SetVarInt(isGlobal, index, val1 < val2 ? 1 : 0, intLocals, intGlobals);
        }
    };
    struct LTFltOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && arg1Ty == TypeEnum::Float && arg2Ty == TypeEnum::Float);
            double val1 = GetVarFloat(arg1IsGlobal, arg1Index, fltLocals, fltGlobals);
            double val2 = GetVarFloat(arg2IsGlobal, arg2Index, fltLocals, fltGlobals);
            SetVarInt(isGlobal, index, val1 < val2 ? 1 : 0, intLocals, intGlobals);
        }
    };
    struct LTStrOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && arg1Ty == TypeEnum::String && arg2Ty == TypeEnum::String);
            const std::string& val1 = GetVarString(arg1IsGlobal, arg1Index, strLocals, strGlobals);
            const std::string& val2 = GetVarString(arg2IsGlobal, arg2Index, strLocals, strGlobals);
            SetVarInt(isGlobal, index, val1 < val2 ? 1 : 0, intLocals, intGlobals);
        }
    };
    struct LShiftOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && arg1Ty == TypeEnum::Int && arg2Ty == TypeEnum::Int);
            int64_t val1 = GetVarInt(arg1IsGlobal, arg1Index, intLocals, intGlobals);
            int64_t val2 = GetVarInt(arg2IsGlobal, arg2Index, intLocals, intGlobals);
            SetVarInt(isGlobal, index, val1 << static_cast<int>(val2), intLocals, intGlobals);
        }
    };
    struct RShiftOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && arg1Ty == TypeEnum::Int && arg2Ty == TypeEnum::Int);
            int64_t val1 = GetVarInt(arg1IsGlobal, arg1Index, intLocals, intGlobals);
            int64_t val2 = GetVarInt(arg2IsGlobal, arg2Index, intLocals, intGlobals);
            SetVarInt(isGlobal, index, val1 >> static_cast<int>(val2), intLocals, intGlobals);
        }
    };
    struct URShiftOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && arg1Ty == TypeEnum::Int && arg2Ty == TypeEnum::Int);
            int64_t val1 = GetVarInt(arg1IsGlobal, arg1Index, intLocals, intGlobals);
            int64_t val2 = GetVarInt(arg2IsGlobal, arg2Index, intLocals, intGlobals);
            SetVarInt(isGlobal, index, static_cast<int64_t>(static_cast<uint64_t>(val1) >> static_cast<int>(val2)), intLocals, intGlobals);
        }
    };
    struct BitAndOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && arg1Ty == TypeEnum::Int && arg2Ty == TypeEnum::Int);
            int64_t val1 = GetVarInt(arg1IsGlobal, arg1Index, intLocals, intGlobals);
            int64_t val2 = GetVarInt(arg2IsGlobal, arg2Index, intLocals, intGlobals);
            SetVarInt(isGlobal, index, val1 & val2, intLocals, intGlobals);
        }
    };
    struct BitOrOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && arg1Ty == TypeEnum::Int && arg2Ty == TypeEnum::Int);
            int64_t val1 = GetVarInt(arg1IsGlobal, arg1Index, intLocals, intGlobals);
            int64_t val2 = GetVarInt(arg2IsGlobal, arg2Index, intLocals, intGlobals);
            SetVarInt(isGlobal, index, val1 | val2, intLocals, intGlobals);
        }
    };
    struct BitXorOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool arg1IsGlobal, TypeEnum arg1Ty, int arg1Index, bool arg2IsGlobal, TypeEnum arg2Ty, int arg2Index, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && arg1Ty == TypeEnum::Int && arg2Ty == TypeEnum::Int);
            int64_t val1 = GetVarInt(arg1IsGlobal, arg1Index, intLocals, intGlobals);
            int64_t val2 = GetVarInt(arg2IsGlobal, arg2Index, intLocals, intGlobals);
            SetVarInt(isGlobal, index, val1 ^ val2, intLocals, intGlobals);
        }
    };
    struct BitNotOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool argIsGlobal, TypeEnum argTy, int argIndex, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty == TypeEnum::Int && argTy == TypeEnum::Int);
            int64_t val = GetVarInt(argIsGlobal, argIndex, intLocals, intGlobals);
            SetVarInt(isGlobal, index, ~val, intLocals, intGlobals);
        }
    };

    struct Int2FltOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool isGlobal1, TypeEnum ty1, int index1, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty1 == TypeEnum::Int && ty == TypeEnum::Float);
            int64_t ival = GetVarInt(isGlobal1, index1, intLocals, intGlobals);
            double res = Convert<double>(ival);
            SetVarFloat(isGlobal, index, res, fltLocals, fltGlobals);
        }
    };
    struct Int2StrOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool isGlobal1, TypeEnum ty1, int index1, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty1 == TypeEnum::Int && ty == TypeEnum::String);
            int64_t ival = GetVarInt(isGlobal1, index1, intLocals, intGlobals);
            std::string res = Convert<std::string>(ival);
            SetVarString(isGlobal, index, res, strLocals, strGlobals);
        }
    };
    struct Flt2IntOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool isGlobal1, TypeEnum ty1, int index1, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty1 == TypeEnum::Float && ty == TypeEnum::Int);
            double fval = GetVarFloat(isGlobal1, index1, fltLocals, fltGlobals);
            int64_t res = Convert<int64_t>(fval);
            SetVarInt(isGlobal, index, res, intLocals, intGlobals);
        }
    };
    struct Flt2StrOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool isGlobal1, TypeEnum ty1, int index1, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty1 == TypeEnum::Float && ty == TypeEnum::String);
            double fval = GetVarFloat(isGlobal1, index1, fltLocals, fltGlobals);
            std::string res = Convert<std::string>(fval);
            SetVarString(isGlobal, index, res, strLocals, strGlobals);
        }
    };
    struct Str2IntOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool isGlobal1, TypeEnum ty1, int index1, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty1 == TypeEnum::String && ty == TypeEnum::Int);
            const std::string& sval = GetVarString(isGlobal1, index1, strLocals, strGlobals);
            int64_t res = Convert<int64_t>(sval);
            SetVarInt(isGlobal, index, res, intLocals, intGlobals);
        }
    };
    struct Str2FltOperation
    {
        static inline void Calc(bool isGlobal, TypeEnum ty, int index, bool isGlobal1, TypeEnum ty1, int index1, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
        {
            _ASSERT(ty1 == TypeEnum::String && ty == TypeEnum::Float);
            const std::string& sval = GetVarString(isGlobal1, index1, strLocals, strGlobals);
            double res = Convert<double>(sval);
            SetVarFloat(isGlobal, index, res, fltLocals, fltGlobals);
        }
    };

    template<typename OperationT>
    static inline void DoUnary(int32_t opcode, InsEnum op, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
    {
        ++pos;
        int32_t operand = codes[pos];

        int32_t argNum;
        bool isGlobal;
        TypeEnum ty;
        int32_t index;
        DecodeOpcode(opcode, op, argNum, isGlobal, ty, index);
        bool isGlobal1;
        TypeEnum ty1;
        int32_t index1;
        DecodeOperand1(operand, isGlobal1, ty1, index1);

        OperationT::Calc(isGlobal, ty, index, isGlobal1, ty1, index1, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
    }
    template<typename OperationT>
    static inline void DoBinary(int32_t opcode, InsEnum op, const std::vector<int32_t>& codes, int32_t& pos, IntLocals& intLocals, FloatLocals& fltLocals, StringLocals& strLocals, IntGlobals& intGlobals, FloatGlobals& fltGlobals, StringGlobals& strGlobals)
    {
        ++pos;
        int32_t operand = codes[pos];

        int32_t argNum;
        bool isGlobal;
        TypeEnum ty;
        int32_t index;
        DecodeOpcode(opcode, op, argNum, isGlobal, ty, index);
        bool isGlobal1;
        TypeEnum ty1;
        int32_t index1;
        DecodeOperand1(operand, isGlobal1, ty1, index1);
        bool isGlobal2;
        TypeEnum ty2;
        int32_t index2;
        DecodeOperand2(operand, isGlobal2, ty2, index2);

        OperationT::Calc(isGlobal, ty, index, isGlobal1, ty1, index1, isGlobal2, ty2, index2, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
    }

#define DoUnaryNEG DoUnary<NegOperation>
#define DoUnaryNEGFLT DoUnary<NegFltOperation>
#define DoUnaryNOT DoUnary<NotOperation>
#define DoUnaryBITNOT DoUnary<BitNotOperation>
#define DoUnaryINT2FLT DoUnary<Int2FltOperation>
#define DoUnaryINT2STR DoUnary<Int2StrOperation>
#define DoUnaryFLT2INT DoUnary<Flt2IntOperation>
#define DoUnaryFLT2STR DoUnary<Flt2StrOperation>
#define DoUnarySTR2INT DoUnary<Str2IntOperation>
#define DoUnarySTR2FLT DoUnary<Str2FltOperation>
#define DoBinaryADD DoBinary<AddOperation>
#define DoBinaryADDFLT DoBinary<AddFltOperation>
#define DoBinaryADDSTR DoBinary<AddStrOperation>
#define DoBinarySUB DoBinary<SubOperation>
#define DoBinarySUBFLT DoBinary<SubFltOperation>
#define DoBinaryMUL DoBinary<MulOperation>
#define DoBinaryMULFLT DoBinary<MulFltOperation>
#define DoBinaryDIV DoBinary<DivOperation>
#define DoBinaryDIVFLT DoBinary<DivFltOperation>
#define DoBinaryMOD DoBinary<ModOperation>
#define DoBinaryMODFLT DoBinary<ModFltOperation>
#define DoBinaryAND DoBinary<AndOperation>
#define DoBinaryOR DoBinary<OrOperation>
#define DoBinaryGT DoBinary<GTOperation>
#define DoBinaryGTFLT DoBinary<GTFltOperation>
#define DoBinaryGTSTR DoBinary<GTStrOperation>
#define DoBinaryGE DoBinary<GEOperation>
#define DoBinaryGEFLT DoBinary<GEFltOperation>
#define DoBinaryGESTR DoBinary<GEStrOperation>
#define DoBinaryEQ DoBinary<EQOperation>
#define DoBinaryEQFLT DoBinary<EQFltOperation>
#define DoBinaryEQSTR DoBinary<EQStrOperation>
#define DoBinaryNE DoBinary<NEOperation>
#define DoBinaryNEFLT DoBinary<NEFltOperation>
#define DoBinaryNESTR DoBinary<NEStrOperation>
#define DoBinaryLE DoBinary<LEOperation>
#define DoBinaryLEFLT DoBinary<LEFltOperation>
#define DoBinaryLESTR DoBinary<LEStrOperation>
#define DoBinaryLT DoBinary<LTOperation>
#define DoBinaryLTFLT DoBinary<LTFltOperation>
#define DoBinaryLTSTR DoBinary<LTStrOperation>
#define DoBinaryLSHIFT DoBinary<LShiftOperation>
#define DoBinaryRSHIFT DoBinary<RShiftOperation>
#define DoBinaryURSHIFT DoBinary<URShiftOperation>
#define DoBinaryBITAND DoBinary<BitAndOperation>
#define DoBinaryBITOR DoBinary<BitOrOperation>
#define DoBinaryBITXOR DoBinary<BitXorOperation>

    struct HookData
    {
        std::vector<int32_t> m_OnEnter{};
        std::vector<int32_t> m_OnExit{};
    };

    struct DebugScriptGlobalImpl
    {
        int32_t m_NextIntConstIndex{ c_max_variable_table_size - 1 };
        int32_t m_NextFloatConstIndex{ c_max_variable_table_size - 1 };
        int32_t m_NextStringConstIndex{ c_max_variable_table_size - 1 };

        IntGlobals m_IntGlobals{};
        FloatGlobals m_FloatGlobals{};
        StringGlobals m_StringGlobals{};
        int32_t m_NextIntGlobalIndex{};
        int32_t m_NextFloatGlobalIndex{};
        int32_t m_NextStringGlobalIndex{};

        std::unordered_map<std::string, int32_t> m_HookMap{};
        std::vector<HookData> m_HookDatas{};

        std::atomic_uint32_t m_RuntimeVersion{};
        std::shared_mutex m_Mutex{};
        using read_locker = std::shared_lock<std::shared_mutex>;
        using write_locker = std::lock_guard<std::shared_mutex>;

        void Reset()
        {
            write_locker lock(m_Mutex);

            g_DebugScriptStarted = false;
            g_DebugScriptSerialNum = ++m_RuntimeVersion;

            g_start_time = std::chrono::high_resolution_clock::now();

            m_NextIntConstIndex = c_max_variable_table_size - 1;
            m_NextFloatConstIndex = c_max_variable_table_size - 1;
            m_NextStringConstIndex = c_max_variable_table_size - 1;
            m_IntGlobals.fill(0);
            m_FloatGlobals.fill(0);
            m_StringGlobals.fill(std::string());
            m_NextIntGlobalIndex = 0;
            m_NextFloatGlobalIndex = 0;
            m_NextStringGlobalIndex = 0;

            m_HookMap.clear();
            m_HookDatas.clear();
        }
        void AllocConstInt(int64_t val)
        {
            write_locker lock(m_Mutex);

            if (m_NextIntConstIndex > 0) {
                m_IntGlobals[m_NextIntConstIndex--] = val;
            }
        }
        void AllocConstFloat(double val)
        {
            write_locker lock(m_Mutex);

            if (m_NextFloatConstIndex > 0) {
                m_FloatGlobals[m_NextFloatConstIndex--] = val;
            }
        }
        void AllocConstString(const char* val)
        {
            write_locker lock(m_Mutex);

            if (m_NextStringConstIndex > 0) {
                m_StringGlobals[m_NextStringConstIndex--] = val;
            }
        }
        void AllocGlobalInt(int64_t val)
        {
            write_locker lock(m_Mutex);

            if (m_NextIntGlobalIndex < m_IntGlobals.size()) {
                m_IntGlobals[m_NextIntGlobalIndex++] = val;
            }
        }
        void AllocGlobalFloat(double val)
        {
            write_locker lock(m_Mutex);

            if (m_NextFloatGlobalIndex < m_FloatGlobals.size()) {
                m_FloatGlobals[m_NextFloatGlobalIndex++] = val;
            }
        }
        void AllocGlobalString(const char* val)
        {
            write_locker lock(m_Mutex);

            if (m_NextStringGlobalIndex < m_StringGlobals.size()) {
                m_StringGlobals[m_NextStringGlobalIndex++] = val;
            }
        }
        int32_t AddHook(const char* name, std::vector<int32_t>&& onEnter, std::vector<int32_t>&& onExit)
        {
            write_locker lock(m_Mutex);

            HookData newData{};
            newData.m_OnEnter.swap(onEnter);
            newData.m_OnExit.swap(onExit);
            int32_t hookId = static_cast<int32_t>(m_HookDatas.size());
            m_HookMap.insert(std::make_pair(std::string(name), hookId));
            m_HookDatas.push_back(std::move(newData));
            return hookId;
        }
        int32_t AddHook(const char* name, int32_t* onEnter, int32_t onEnterSize, int32_t* onExit, int32_t onExitSize)
        {
            std::vector<int32_t> onEnterVec{};
            std::vector<int32_t> onExitVec{};
            onEnterVec.assign(onEnter, onEnter + onEnterSize);
            onExitVec.assign(onExit, onExit + onExitSize);
            return AddHook(name, std::move(onEnterVec), std::move(onExitVec));
        }
        int32_t ShareWith(int32_t hookId, const char* other)
        {
            if (hookId >= 0 && hookId < static_cast<int32_t>(m_HookDatas.size())) {
                m_HookMap.insert(std::make_pair(std::string(other), hookId));
                return hookId;
            }
            return -1;
        }
        void Start()
        {
            g_DebugScriptStarted = true;
        }
        void Load(const char* file)
        {
            std::ifstream ifs(file, std::ios::in | std::ios::binary);
            if (ifs.fail())
                return;

            //tag:DSBC 0x43425344
            int tag = ReadInt32(ifs);
            if (tag != 0x43425344)
                return;

            //str table offset
            int offset = ReadInt32(ifs);

            auto&& pos = ifs.tellg();
            ifs.seekg(offset, std::ios::beg);

            //string count
            int strNum = ReadInt32(ifs);
            std::vector<std::string> strTable{};
            for (int i = 0; i < strNum; ++i) {
                std::string str = ReadString(ifs);
                strTable.push_back(std::move(str));
            }

            ifs.seekg(pos, std::ios::beg);

            //int consts
            int intConstNum = ReadInt32(ifs);
            for (int i = 0; i < intConstNum; ++i) {
                int64_t v = ReadInt64(ifs);
                AllocConstInt(v);
            }

            //float consts
            int floatConstNum = ReadInt32(ifs);
            for (int i = 0; i < floatConstNum; ++i) {
                double v = ReadDouble(ifs);
                AllocConstFloat(v);
            }

            //string consts
            int strConstNum = ReadInt32(ifs);
            for (int i = 0; i < strConstNum; ++i) {
                int ix = ReadInt32(ifs);
                if (ix >= 0 && ix < static_cast<int>(strTable.size())) {
                    AllocConstString(strTable[ix].c_str());
                }
                else {
                    AllocConstString("");
                }
            }

            //int globals
            int intGlobalNum = ReadInt32(ifs);
            for (int i = 0; i < intGlobalNum; ++i) {
                int64_t v = ReadInt64(ifs);
                AllocGlobalInt(v);
            }

            //float globals
            int floatGlobalNum = ReadInt32(ifs);
            for (int i = 0; i < floatGlobalNum; ++i) {
                double v = ReadDouble(ifs);
                AllocGlobalFloat(v);
            }

            //string globals
            int strGlobalNum = ReadInt32(ifs);
            for (int i = 0; i < strGlobalNum; ++i) {
                int ix = ReadInt32(ifs);
                if (ix >= 0 && ix < static_cast<int>(strTable.size())) {
                    AllocGlobalString(strTable[ix].c_str());
                }
                else {
                    AllocGlobalString("");
                }
            }

            //hook count
            int hookNum = ReadInt32(ifs);
            for (int i = 0; i < hookNum; ++i) {
                //hook name
                int nameIx = ReadInt32(ifs);
                const char* name = nameIx >= 0 && nameIx < static_cast<int>(strTable.size()) ? strTable[nameIx].c_str() : "";
                //onEnter count
                int onEnterNum = ReadInt32(ifs);
                std::vector<int32_t> onEnter(onEnterNum);
                ifs.read(reinterpret_cast<char*>(onEnter.data()), onEnterNum * sizeof(int32_t));
                int onExitNum = ReadInt32(ifs);
                std::vector<int32_t> onExit(onExitNum);
                ifs.read(reinterpret_cast<char*>(onExit.data()), onExitNum * sizeof(int32_t));

                int32_t hookId = AddHook(name, std::move(onEnter), std::move(onExit));
                printf("hook:%s id:%d\n", name, hookId);

                //shader name count
                int shareNameNum = ReadInt32(ifs);
                for (int ix = 0; ix < shareNameNum; ++ix) {
                    int shareNameIx = ReadInt32(ifs);
                    const char* other = shareNameIx >= 0 && shareNameIx < static_cast<int>(strTable.size()) ? strTable[shareNameIx].c_str() : "";
                    ShareWith(hookId, other);
                    printf("share with:{0} id:{1}\n", other, hookId);
                }
            }

            ifs.close();
        }
        std::shared_mutex& GetSharedMutex()
        {
            return m_Mutex;
        }

        //these methods must be protected by read_locker
        int32_t FindHook(const std::string& name)const
        {
            auto&& it = m_HookMap.find(name);
            if (it != m_HookMap.end())
                return it->second;
            return -1;
        }
    };

    struct DebugScriptVMImpl
    {
        IntLocals m_IntLocals{};
        FloatLocals m_FloatLocals{};
        StringLocals m_StringLocals{};

        int32_t FindHook(const std::string& name)const
        {
            DebugScriptGlobalImpl::read_locker lock(g_pDebugScriptGlobal->GetSharedMutex());

            int32_t id = g_pDebugScriptGlobal->FindHook(name);
            return id;
        }
        void Reset()
        {
            m_IntLocals.fill(0);
            m_FloatLocals.fill(0);
            m_StringLocals.fill(std::string());
        }
        bool CanRun()const
        {
            return g_DebugScriptStarted;
        }
        bool RunOnEnter(int32_t id, int32_t argc, int64_t argv[])
        {
            if (CanRun())
            {
                DebugScriptGlobalImpl::read_locker lock(g_pDebugScriptGlobal->GetSharedMutex());

                if (id < 0 || id >= g_pDebugScriptGlobal->m_HookDatas.size())
                    return false;
                const HookData& data = g_pDebugScriptGlobal->m_HookDatas[id];
                return Run(data.m_OnEnter, argc, argv);
            }
            return false;
        }
        bool RunOnExit(int32_t id, int32_t argc, int64_t argv[])
        {
            if (CanRun())
            {
                DebugScriptGlobalImpl::read_locker lock(g_pDebugScriptGlobal->GetSharedMutex());

                if (id < 0 || id >= g_pDebugScriptGlobal->m_HookDatas.size())
                    return false;
                const HookData& data = g_pDebugScriptGlobal->m_HookDatas[id];
                return Run(data.m_OnExit, argc, argv);
            }
            return false;
        }

        bool Run(const std::vector<int32_t>& codes, int32_t argc, int64_t argv[])
        {
            auto&& intGlobals = g_pDebugScriptGlobal->m_IntGlobals;
            auto&& fltGlobals = g_pDebugScriptGlobal->m_FloatGlobals;
            auto&& strGlobals = g_pDebugScriptGlobal->m_StringGlobals;

            auto&& intLocals = m_IntLocals;
            auto&& fltLocals = m_FloatLocals;
            auto&& strLocals = m_StringLocals;

            bool ret = false;
            for (int32_t pos = 0; pos < codes.size(); ++pos) {
                int32_t opcode = codes[pos];
                InsEnum op = DecodeInsEnum(opcode);
                switch (op) {
                case InsEnum::CALL:
                    DoCall(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::RET:
                    DoRet(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals, ret);
                    return ret;
                case InsEnum::JMP:
                    DoJmp(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::JMPIF:
                    DoJmpIf(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::JMPIFNOT:
                    DoJmpIfNot(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::INC:
                    DoInc(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::INCFLT:
                    DoIncFlt(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::INCV:
                    DoIncVal(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::INCVFLT:
                    DoIncValFlt(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::MOV:
                    DoMov(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::MOVFLT:
                    DoMovFlt(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::MOVSTR:
                    DoMovStr(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::ARRGET:
                    DoArrayGet(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::ARRGETFLT:
                    DoArrayGetFlt(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::ARRGETSTR:
                    DoArrayGetStr(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::ARRSET:
                    DoArraySet(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::ARRSETFLT:
                    DoArraySetFlt(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::ARRSETSTR:
                    DoArraySetStr(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::NEG:
                    DoUnaryNEG(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::NEGFLT:
                    DoUnaryNEGFLT(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::ADD:
                    DoBinaryADD(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::ADDFLT:
                    DoBinaryADDFLT(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::ADDSTR:
                    DoBinaryADDSTR(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::SUB:
                    DoBinarySUB(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::SUBFLT:
                    DoBinarySUBFLT(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::MUL:
                    DoBinaryMUL(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::MULFLT:
                    DoBinaryMULFLT(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::DIV:
                    DoBinaryDIV(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::DIVFLT:
                    DoBinaryDIVFLT(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::MOD:
                    DoBinaryMOD(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::MODFLT:
                    DoBinaryMODFLT(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::AND:
                    DoBinaryAND(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::OR:
                    DoBinaryOR(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::NOT:
                    DoUnaryNOT(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::GT:
                    DoBinaryGT(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::GTFLT:
                    DoBinaryGTFLT(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::GTSTR:
                    DoBinaryGTSTR(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::GE:
                    DoBinaryGE(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::GEFLT:
                    DoBinaryGEFLT(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::GESTR:
                    DoBinaryGESTR(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::EQ:
                    DoBinaryEQ(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::EQFLT:
                    DoBinaryEQFLT(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::EQSTR:
                    DoBinaryEQSTR(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::NE:
                    DoBinaryNE(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::NEFLT:
                    DoBinaryNEFLT(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::NESTR:
                    DoBinaryNESTR(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::LE:
                    DoBinaryLE(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::LEFLT:
                    DoBinaryLEFLT(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::LESTR:
                    DoBinaryLESTR(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::LT:
                    DoBinaryLT(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::LTFLT:
                    DoBinaryLTFLT(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::LTSTR:
                    DoBinaryLTSTR(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::LSHIFT:
                    DoBinaryLSHIFT(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::RSHIFT:
                    DoBinaryRSHIFT(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::URSHIFT:
                    DoBinaryURSHIFT(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::BITAND:
                    DoBinaryBITAND(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::BITOR:
                    DoBinaryBITOR(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::BITXOR:
                    DoBinaryBITXOR(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::BITNOT:
                    DoUnaryBITNOT(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::INT2FLT:
                    DoUnaryINT2FLT(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::INT2STR:
                    DoUnaryINT2STR(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::FLT2INT:
                    DoUnaryFLT2INT(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::FLT2STR:
                    DoUnaryFLT2STR(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::STR2INT:
                    DoUnarySTR2INT(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::STR2FLT:
                    DoUnarySTR2FLT(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::ITOF:
                    DoIToF(opcode, op, codes, pos, intLocals, fltLocals, intGlobals, fltGlobals);
                    break;
                case InsEnum::FTOI:
                    DoFToI(opcode, op, codes, pos, intLocals, fltLocals, intGlobals, fltGlobals);
                    break;
                case InsEnum::ARGC:
                    DoArgc(opcode, op, codes, pos, intLocals, intGlobals, argc);
                    break;
                case InsEnum::ARGV:
                    DoArgv(opcode, op, codes, pos, intLocals, intGlobals, argc, argv);
                    break;
                case InsEnum::ADDR:
                    DoAddr(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::PTRGET:
                    DoPtrGet(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::PTRSET:
                    DoPtrSet(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::JPTR:
                    DoJaggedPtr(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                }
            }
            return ret;
        }
    };

    DebugScriptGlobalImpl* GetDebugScriptGlobal()
    {
        if (nullptr == g_pDebugScriptGlobal) {
            g_pDebugScriptGlobal = new DebugScriptGlobalImpl();
        }
        return g_pDebugScriptGlobal;
    }

    DebugScriptVMImpl* GetDebugScriptVM()
    {
        if (nullptr == g_pDebugScriptVM) {
            g_pDebugScriptVM = new DebugScriptVMImpl();
        }
        return g_pDebugScriptVM;
    }
}

void DebugScriptGlobal::Reset()
{
    GetDebugScriptGlobal()->Reset();
}
void DebugScriptGlobal::AllocConstInt(int64_t val)
{
    GetDebugScriptGlobal()->AllocConstInt(val);
}
void DebugScriptGlobal::AllocConstFloat(double val)
{
    GetDebugScriptGlobal()->AllocConstFloat(val);
}
void DebugScriptGlobal::AllocConstString(const char* val)
{
    GetDebugScriptGlobal()->AllocConstString(val);
}
void DebugScriptGlobal::AllocGlobalInt(int64_t val)
{
    GetDebugScriptGlobal()->AllocGlobalInt(val);
}
void DebugScriptGlobal::AllocGlobalFloat(double val)
{
    GetDebugScriptGlobal()->AllocGlobalFloat(val);
}
void DebugScriptGlobal::AllocGlobalString(const char* val)
{
    GetDebugScriptGlobal()->AllocGlobalString(val);
}
int32_t DebugScriptGlobal::AddHook(const char* name, int32_t* onEnter, int32_t onEnterSize, int32_t* onExit, int32_t onExitSize)
{
    return GetDebugScriptGlobal()->AddHook(name, onEnter, onEnterSize, onExit, onExitSize);
}
int32_t DebugScriptGlobal::ShareWith(int32_t hookId, const char* other)
{
    return GetDebugScriptGlobal()->ShareWith(hookId, other);
}
void DebugScriptGlobal::Start()
{
    GetDebugScriptGlobal()->Start();
}
void DebugScriptGlobal::Load(const char* file)
{
    GetDebugScriptGlobal()->Load(file);
}

int32_t DebugScriptVM::FindHook(const char* name)
{
    return GetDebugScriptVM()->FindHook(name);
}
void DebugScriptVM::Reset()
{
    GetDebugScriptVM()->Reset();
}
bool DebugScriptVM::CanRun()
{
    return GetDebugScriptVM()->CanRun();
}
bool DebugScriptVM::RunHookOnEnter(int32_t id, int32_t argc, int64_t argv[])
{
    return GetDebugScriptVM()->RunOnEnter(id, argc, argv);
}
bool DebugScriptVM::RunHookOnExit(int32_t id, int32_t argc, int64_t argv[])
{
    return GetDebugScriptVM()->RunOnExit(id, argc, argv);
}

