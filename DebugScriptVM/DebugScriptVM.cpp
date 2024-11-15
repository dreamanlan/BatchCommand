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
        INCV,
        MOV,
        ARRGET,
        ARRSET,
        NEG,
        ADD,
        SUB,
        MUL,
        DIV,
        MOD,
        AND,
        OR,
        NOT,
        GT,
        GE,
        EQ,
        NE,
        LE,
        LT,
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
    static inline void DecodeOperand1(int32_t operand, bool& isGlobal, int32_t& index)
    {
        int32_t localOrGlobal = ((operand & 0x8000) >> 15);
        index = (operand & 0x1fff);
        isGlobal = localOrGlobal != 0;
    }
    static inline void DecodeOperand2(int32_t operand, bool& isGlobal, int32_t& index)
    {
        operand >>= 16;
        DecodeOperand1(operand, isGlobal, index);
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
            if (ty == TypeEnum::Int) {
                SetVarInt(isGlobal, index, val, intLocals, intGlobals);
            }
            else if (ty == TypeEnum::Float) {
                SetVarFloat(isGlobal, index, static_cast<double>(val), fltLocals, fltGlobals);
            }
        }break;
        case ApiEnum::Printf: {
            int64_t val = Api<ApiEnum::Printf>::Call(argNum, operand, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
            if (ty == TypeEnum::Int) {
                SetVarInt(isGlobal, index, val, intLocals, intGlobals);
            }
            else if (ty == TypeEnum::Float) {
                SetVarFloat(isGlobal, index, static_cast<double>(val), fltLocals, fltGlobals);
            }
        }break;
        case ApiEnum::Format: {
            std::string val = Api<ApiEnum::Format>::Call(argNum, operand, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
            if (ty == TypeEnum::String) {
                SetVarString(isGlobal, index, val, strLocals, strGlobals);
            }
        }break;
        case ApiEnum::Time: {
            int64_t val = Api<ApiEnum::Time>::Call(argNum, operand, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
            if (ty == TypeEnum::Int) {
                SetVarInt(isGlobal, index, val, intLocals, intGlobals);
            }
            else if (ty == TypeEnum::Float) {
                SetVarFloat(isGlobal, index, static_cast<double>(val), fltLocals, fltGlobals);
            }
        }break;
        case ApiEnum::FloatTime: {
            double val = Api<ApiEnum::FloatTime>::Call(argNum, operand, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
            if (ty == TypeEnum::Int) {
                SetVarInt(isGlobal, index, static_cast<int64_t>(val), intLocals, intGlobals);
            }
            else if (ty == TypeEnum::Float) {
                SetVarFloat(isGlobal, index, val, fltLocals, fltGlobals);
            }
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
        switch (ty) {
        case TypeEnum::Int: {
            int64_t val = GetVarInt(isGlobal, index, intLocals, intGlobals);
            SetVarInt(isGlobal, index, val + 1, intLocals, intGlobals);
        }break;
        case TypeEnum::Float: {
            double val = GetVarFloat(isGlobal, index, fltLocals, fltGlobals);
            SetVarFloat(isGlobal, index, val + 1, fltLocals, fltGlobals);
        }break;
        }
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
        switch (ty1) {
        case TypeEnum::Int: {
            int64_t inc = GetVarInt(isGlobal1, index1, intLocals, intGlobals);
            if (ty == TypeEnum::Int) {
                int64_t val = GetVarInt(isGlobal, index, intLocals, intGlobals);
                SetVarInt(isGlobal, index, val + inc, intLocals, intGlobals);
            }
            else if (ty == TypeEnum::Float) {
                double val = GetVarFloat(isGlobal, index, fltLocals, fltGlobals);
                SetVarFloat(isGlobal, index, val + static_cast<double>(inc), fltLocals, fltGlobals);
            }
        }break;
        case TypeEnum::Float: {
            double inc = GetVarFloat(isGlobal1, index1, fltLocals, fltGlobals);
            if (ty == TypeEnum::Int) {
                int64_t val = GetVarInt(isGlobal, index, intLocals, intGlobals);
                SetVarInt(isGlobal, index, val + static_cast<int64_t>(inc), intLocals, intGlobals);
            }
            else if (ty == TypeEnum::Float) {
                double val = GetVarFloat(isGlobal, index, fltLocals, fltGlobals);
                SetVarFloat(isGlobal, index, val + inc, fltLocals, fltGlobals);
            }
        }break;
        }
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
        switch (ty1) {
        case TypeEnum::Int: {
            int64_t val = GetVarInt(isGlobal1, index1, intLocals, intGlobals);
            if (ty == TypeEnum::Int)
                SetVarInt(isGlobal, index, val, intLocals, intGlobals);
            else if (ty == TypeEnum::Float)
                SetVarFloat(isGlobal, index, static_cast<double>(val), fltLocals, fltGlobals);
        }break;
        case TypeEnum::Float: {
            double val = GetVarFloat(isGlobal1, index1, fltLocals, fltGlobals);
            if (ty == TypeEnum::Int)
                SetVarInt(isGlobal, index, static_cast<int64_t>(val), intLocals, intGlobals);
            else if (ty == TypeEnum::Float)
                SetVarFloat(isGlobal, index, val, fltLocals, fltGlobals);
        }break;
        case TypeEnum::String: {
            if (ty == TypeEnum::String) {
                const std::string& val = GetVarString(isGlobal1, index1, strLocals, strGlobals);
                SetVarString(isGlobal, index, val, strLocals, strGlobals);
            }
        }break;
        }
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
        switch (ty1) {
        case TypeEnum::Int: {
            int32_t ix = static_cast<int32_t>(GetVarInt(isGlobal2, index2, intLocals, intGlobals));
            int64_t val = GetVarInt(isGlobal1, index1 + ix, intLocals, intGlobals);
            if (ty == TypeEnum::Int)
                SetVarInt(isGlobal, index, val, intLocals, intGlobals);
            else if (ty == TypeEnum::Float)
                SetVarFloat(isGlobal, index, static_cast<double>(val), fltLocals, fltGlobals);
        }break;
        case TypeEnum::Float: {
            int32_t ix = static_cast<int32_t>(GetVarInt(isGlobal2, index2, intLocals, intGlobals));
            double val = GetVarFloat(isGlobal1, index1 + ix, fltLocals, fltGlobals);
            if (ty == TypeEnum::Int)
                SetVarInt(isGlobal, index, static_cast<int64_t>(val), intLocals, intGlobals);
            else if (ty == TypeEnum::Float)
                SetVarFloat(isGlobal, index, val, fltLocals, fltGlobals);
        }break;
        case TypeEnum::String: {
            int32_t ix = static_cast<int32_t>(GetVarInt(isGlobal2, index2, intLocals, intGlobals));
            const std::string& val = GetVarString(isGlobal1, index1 + ix, strLocals, strGlobals);
            if (ty == TypeEnum::String)
                SetVarString(isGlobal, index, val, strLocals, strGlobals);
        }break;
        }
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
        switch (ty) {
        case TypeEnum::Int: {
            int32_t ix = static_cast<int32_t>(GetVarInt(isGlobal1, index1, intLocals, intGlobals));
            int64_t val = 0;
            if (ty2 == TypeEnum::Int)
                val = GetVarInt(isGlobal2, index2, intLocals, intGlobals);
            else if (ty2 == TypeEnum::Float)
                val = static_cast<int64_t>(GetVarFloat(isGlobal2, index2, fltLocals, fltGlobals));
            SetVarInt(isGlobal, index + ix, val, intLocals, intGlobals);
        }break;
        case TypeEnum::Float: {
            int32_t ix = static_cast<int32_t>(GetVarInt(isGlobal1, index1, intLocals, intGlobals));
            double val = 0;
            if (ty2 == TypeEnum::Int)
                val = static_cast<double>(GetVarInt(isGlobal2, index2, intLocals, intGlobals));
            else if (ty2 == TypeEnum::Float)
                val = GetVarFloat(isGlobal2, index2, fltLocals, fltGlobals);
            SetVarFloat(isGlobal, index + ix, val, fltLocals, fltGlobals);
        }break;
        case TypeEnum::String: {
            int32_t ix = static_cast<int32_t>(GetVarInt(isGlobal1, index1, intLocals, intGlobals));
            const std::string& val = GetVarString(isGlobal2, index2, strLocals, strGlobals);
            SetVarString(isGlobal, index + ix, val, strLocals, strGlobals);
        }break;
        }
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
        static inline int64_t CalcInt(int64_t val)
        {
            return -val;
        }
        static inline double CalcFloat(double val)
        {
            return -val;
        }
        static inline std::string CalcString(const std::string& val)
        {
            return std::string();
        }
    };
    struct AddOperation
    {
        static inline int64_t CalcInt(int64_t val1, int64_t val2)
        {
            return val1 + val2;
        }
        static inline double CalcFloat(double val1, double val2)
        {
            return val1 + val2;
        }
        static inline std::string CalcString(const std::string& val1, const std::string& val2)
        {
            return val1 + val2;
        }
    };
    struct SubOperation
    {
        static inline int64_t CalcInt(int64_t val1, int64_t val2)
        {
            return val1 - val2;
        }
        static inline double CalcFloat(double val1, double val2)
        {
            return val1 - val2;
        }
        static inline std::string CalcString(const std::string& val1, const std::string& val2)
        {
            return std::string();
        }
    };
    struct MulOperation
    {
        static inline int64_t CalcInt(int64_t val1, int64_t val2)
        {
            return val1 * val2;
        }
        static inline double CalcFloat(double val1, double val2)
        {
            return val1 * val2;
        }
        static inline std::string CalcString(const std::string& val1, const std::string& val2)
        {
            return std::string();
        }
    };
    struct DivOperation
    {
        static inline int64_t CalcInt(int64_t val1, int64_t val2)
        {
            return val1 / val2;
        }
        static inline double CalcFloat(double val1, double val2)
        {
            return val1 / val2;
        }
        static inline std::string CalcString(const std::string& val1, const std::string& val2)
        {
            return std::string();
        }
    };
    struct ModOperation
    {
        static inline int64_t CalcInt(int64_t val1, int64_t val2)
        {
            return val1 % val2;
        }
        static inline double CalcFloat(double val1, double val2)
        {
            return std::fmod(val1, val2);
        }
        static inline std::string CalcString(const std::string& val1, const std::string& val2)
        {
            return std::string();
        }
    };
    struct AndOperation
    {
        static inline int64_t CalcInt(int64_t val1, int64_t val2)
        {
            return val1 && val2 ? 1 : 0;
        }
        static inline int64_t CalcFloat(double val1, double val2)
        {
            return val1 && val2 ? 1 : 0;
        }
        static inline int64_t CalcString(const std::string& val1, const std::string& val2)
        {
            return !val1.empty() && !val2.empty() ? 1 : 0;
        }
    };
    struct OrOperation
    {
        static inline int64_t CalcInt(int64_t val1, int64_t val2)
        {
            return val1 || val2 ? 1 : 0;
        }
        static inline int64_t CalcFloat(double val1, double val2)
        {
            return val1 || val2 ? 1 : 0;
        }
        static inline int64_t CalcString(const std::string& val1, const std::string& val2)
        {
            return !val1.empty() || !val2.empty() ? 1 : 0;
        }
    };
    struct NotOperation
    {
        static inline int64_t CalcInt(int64_t val)
        {
            return val ? 1 : 0;
        }
        static inline int64_t CalcFloat(double val)
        {
            return val ? 1 : 0;
        }
        static inline int64_t CalcString(const std::string& val)
        {
            return !val.empty() ? 1 : 0;
        }
    };
    struct GTOperation
    {
        static inline int64_t CalcInt(int64_t val1, int64_t val2)
        {
            return val1 > val2 ? 1 : 0;
        }
        static inline int64_t CalcFloat(double val1, double val2)
        {
            return val1 > val2 ? 1 : 0;
        }
        static inline int64_t CalcString(const std::string& val1, const std::string& val2)
        {
            return val1 > val2 ? 1 : 0;
        }
    };
    struct GEOperation
    {
        static inline int64_t CalcInt(int64_t val1, int64_t val2)
        {
            return val1 >= val2 ? 1 : 0;
        }
        static inline int64_t CalcFloat(double val1, double val2)
        {
            return val1 >= val2 ? 1 : 0;
        }
        static inline int64_t CalcString(const std::string& val1, const std::string& val2)
        {
            return val1 >= val2 ? 1 : 0;
        }
    };
    struct EQOperation
    {
        static inline int64_t CalcInt(int64_t val1, int64_t val2)
        {
            return val1 == val2 ? 1 : 0;
        }
        static inline int64_t CalcFloat(double val1, double val2)
        {
            return val1 == val2 ? 1 : 0;
        }
        static inline int64_t CalcString(const std::string& val1, const std::string& val2)
        {
            return val1 == val2 ? 1 : 0;
        }
    };
    struct NEOperation
    {
        static inline int64_t CalcInt(int64_t val1, int64_t val2)
        {
            return val1 != val2 ? 1 : 0;
        }
        static inline int64_t CalcFloat(double val1, double val2)
        {
            return val1 != val2 ? 1 : 0;
        }
        static inline int64_t CalcString(const std::string& val1, const std::string& val2)
        {
            return val1 != val2 ? 1 : 0;
        }
    };
    struct LEOperation
    {
        static inline int64_t CalcInt(int64_t val1, int64_t val2)
        {
            return val1 <= val2 ? 1 : 0;
        }
        static inline int64_t CalcFloat(double val1, double val2)
        {
            return val1 <= val2 ? 1 : 0;
        }
        static inline int64_t CalcString(const std::string& val1, const std::string& val2)
        {
            return val1 <= val2 ? 1 : 0;
        }
    };
    struct LTOperation
    {
        static inline int64_t CalcInt(int64_t val1, int64_t val2)
        {
            return val1 < val2 ? 1 : 0;
        }
        static inline int64_t CalcFloat(double val1, double val2)
        {
            return val1 < val2 ? 1 : 0;
        }
        static inline int64_t CalcString(const std::string& val1, const std::string& val2)
        {
            return val1 < val2 ? 1 : 0;
        }
    };
    struct LShiftOperation
    {
        static inline int64_t CalcInt(int64_t val1, int64_t val2)
        {
            return val1 << static_cast<int32_t>(val2);
        }
        static inline int64_t CalcFloat(double val1, double val2)
        {
            return static_cast<int64_t>(val1) << static_cast<int32_t>(val2);
        }
        static inline int64_t CalcString(const std::string& val1, const std::string& val2)
        {
            return 0;
        }
    };
    struct RShiftOperation
    {
        static inline int64_t CalcInt(int64_t val1, int64_t val2)
        {
            return val1 >> static_cast<int32_t>(val2);
        }
        static inline int64_t CalcFloat(double val1, double val2)
        {
            return static_cast<int64_t>(val1) >> static_cast<int32_t>(val2);
        }
        static inline int64_t CalcString(const std::string& val1, const std::string& val2)
        {
            return 0;
        }
    };
    struct URShiftOperation
    {
        static inline int64_t CalcInt(int64_t val1, int64_t val2)
        {
            return static_cast<int64_t>(static_cast<uint64_t>(val1) >> static_cast<int32_t>(val2));
        }
        static inline int64_t CalcFloat(double val1, double val2)
        {
            return static_cast<int64_t>(static_cast<uint64_t>(val1) >> static_cast<int32_t>(val2));
        }
        static inline int64_t CalcString(const std::string& val1, const std::string& val2)
        {
            return 0;
        }
    };
    struct BitAndOperation
    {
        static inline int64_t CalcInt(int64_t val1, int64_t val2)
        {
            return val1 & val2;
        }
        static inline int64_t CalcFloat(double val1, double val2)
        {
            return static_cast<int64_t>(val1) & static_cast<int64_t>(val2);
        }
        static inline int64_t CalcString(const std::string& val1, const std::string& val2)
        {
            return 0;
        }
    };
    struct BitOrOperation
    {
        static inline int64_t CalcInt(int64_t val1, int64_t val2)
        {
            return val1 | val2;
        }
        static inline int64_t CalcFloat(double val1, double val2)
        {
            return static_cast<int64_t>(val1) | static_cast<int64_t>(val2);
        }
        static inline int64_t CalcString(const std::string& val1, const std::string& val2)
        {
            return 0;
        }
    };
    struct BitXorOperation
    {
        static inline int64_t CalcInt(int64_t val1, int64_t val2)
        {
            return val1 ^ val2;
        }
        static inline int64_t CalcFloat(double val1, double val2)
        {
            return static_cast<int64_t>(val1) ^ static_cast<int64_t>(val2);
        }
        static inline int64_t CalcString(const std::string& val1, const std::string& val2)
        {
            return 0;
        }
    };
    struct BitNotOperation
    {
        static inline int64_t CalcInt(int64_t val)
        {
            return ~val;
        }
        static inline int64_t CalcFloat(double val)
        {
            return ~static_cast<int64_t>(val);
        }
        static inline int64_t CalcString(const std::string& val)
        {
            return 0;
        }
    };

    struct ToFltOperation
    {
        static inline double CalcInt(int64_t val)
        {
            return static_cast<double>(val);
        }
        static inline double CalcFloat(double val)
        {
            return val;
        }
        static inline double CalcString(const std::string& val)
        {
            return std::atof(val.c_str());
        }
    };
    struct ToIntOperation
    {
        static inline int64_t CalcInt(int64_t val)
        {
            return val;
        }
        static inline int64_t CalcFloat(double val)
        {
            return static_cast<int64_t>(val);
        }
        static inline int64_t CalcString(const std::string& val)
        {
            return std::atoll(val.c_str());
        }
    };
    struct ToStrOperation
    {
        static inline std::string CalcInt(int64_t val)
        {
            return std::to_string(val);
        }
        static inline std::string CalcFloat(double val)
        {
            return std::to_string(val);
        }
        static inline std::string CalcString(const std::string& val)
        {
            return val;
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

        switch (ty1) {
        case TypeEnum::Int: {
            int64_t ival = GetVarInt(isGlobal1, index1, intLocals, intGlobals);
            using RT = RetType<decltype(OperationT::CalcInt)>;
            RT res = OperationT::CalcInt(ival);
            if (ty == TypeEnum::Int) {
                SetVarInt(isGlobal, index, Convert<int64_t>(res), intLocals, intGlobals);
            }
            else if (ty == TypeEnum::Float) {
                SetVarFloat(isGlobal, index, Convert<double>(res), fltLocals, fltGlobals);
            }
        }break;
        case TypeEnum::Float: {
            double val = GetVarFloat(isGlobal1, index1, fltLocals, fltGlobals);
            using RT = RetType<decltype(OperationT::CalcFloat)>;
            RT res = OperationT::CalcFloat(val);
            if (ty == TypeEnum::Int) {
                SetVarInt(isGlobal, index, Convert<int64_t>(res), intLocals, intGlobals);
            }
            else if (ty == TypeEnum::Float) {
                SetVarFloat(isGlobal, index, Convert<double>(res), fltLocals, fltGlobals);
            }
        }break;
        case TypeEnum::String: {
            const std::string& val = GetVarString(isGlobal1, index1, strLocals, strGlobals);
            using RT = RetType<decltype(OperationT::CalcString)>;
            RT res = OperationT::CalcString(val);
            if (ty == TypeEnum::Int) {
                SetVarInt(isGlobal, index, Convert<int64_t>(res), intLocals, intGlobals);
            }
            else if (ty == TypeEnum::Float) {
                SetVarFloat(isGlobal, index, Convert<double>(res), fltLocals, fltGlobals);
            }
            else if (ty == TypeEnum::String) {
                SetVarString(isGlobal, index, Convert<std::string>(res), strLocals, strGlobals);
            }
        }break;
        }
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

        if (ty1 == TypeEnum::Int && ty2 == TypeEnum::Int) {
            using Arg1T = Arg1Type<decltype(OperationT::CalcInt)>;
            using Arg2T = Arg2Type<decltype(OperationT::CalcInt)>;
            using RT = RetType<decltype(OperationT::CalcInt)>;

            int64_t val1 = GetVarInt(isGlobal1, index1, intLocals, intGlobals);
            int64_t val2 = GetVarInt(isGlobal2, index2, intLocals, intGlobals);
            RT val = OperationT::CalcInt(Convert<Arg1T>(val1), Convert<Arg2T>(val2));
            switch (ty) {
            case TypeEnum::Int:
                SetVarInt(isGlobal, index, Convert<int64_t>(val), intLocals, intGlobals);
                break;
            case TypeEnum::Float:
                SetVarFloat(isGlobal, index, Convert<double>(val), fltLocals, fltGlobals);
                break;
            case TypeEnum::String:
                SetVarString(isGlobal, index, Convert<std::string>(val), strLocals, strGlobals);
                break;
            }
        }
        else if (ty1 == TypeEnum::Float && ty2 == TypeEnum::Float) {
            using Arg1T = Arg1Type<decltype(OperationT::CalcFloat)>;
            using Arg2T = Arg2Type<decltype(OperationT::CalcFloat)>;
            using RT = RetType<decltype(OperationT::CalcFloat)>;

            double val1 = GetVarFloat(isGlobal1, index1, fltLocals, fltGlobals);
            double val2 = GetVarFloat(isGlobal2, index2, fltLocals, fltGlobals);
            RT val = OperationT::CalcFloat(Convert<Arg1T>(val1), Convert<Arg2T>(val2));
            switch (ty) {
            case TypeEnum::Int:
                SetVarInt(isGlobal, index, Convert<int64_t>(val), intLocals, intGlobals);
                break;
            case TypeEnum::Float:
                SetVarFloat(isGlobal, index, Convert<double>(val), fltLocals, fltGlobals);
                break;
            case TypeEnum::String:
                SetVarString(isGlobal, index, Convert<std::string>(val), strLocals, strGlobals);
                break;
            }
        }
        else if (ty1 == TypeEnum::Int && ty2 == TypeEnum::Float) {
            using Arg1T = Arg1Type<decltype(OperationT::CalcFloat)>;
            using Arg2T = Arg2Type<decltype(OperationT::CalcFloat)>;
            using RT = RetType<decltype(OperationT::CalcFloat)>;

            int64_t val1 = GetVarInt(isGlobal1, index1, intLocals, intGlobals);
            double val2 = GetVarFloat(isGlobal2, index2, fltLocals, fltGlobals);
            RT val = OperationT::CalcFloat(Convert<Arg1T>(val1), Convert<Arg2T>(val2));
            switch (ty) {
            case TypeEnum::Int:
                SetVarInt(isGlobal, index, Convert<int64_t>(val), intLocals, intGlobals);
                break;
            case TypeEnum::Float:
                SetVarFloat(isGlobal, index, Convert<double>(val), fltLocals, fltGlobals);
                break;
            case TypeEnum::String:
                SetVarString(isGlobal, index, Convert<std::string>(val), strLocals, strGlobals);
                break;
            }
        }
        else if (ty1 == TypeEnum::Float && ty2 == TypeEnum::Int) {
            using Arg1T = Arg1Type<decltype(OperationT::CalcFloat)>;
            using Arg2T = Arg2Type<decltype(OperationT::CalcFloat)>;
            using RT = RetType<decltype(OperationT::CalcFloat)>;

            double val1 = GetVarFloat(isGlobal1, index1, fltLocals, fltGlobals);
            int64_t val2 = GetVarInt(isGlobal2, index2, intLocals, intGlobals);
            RT val = OperationT::CalcFloat(Convert<Arg1T>(val1), Convert<Arg2T>(val2));
            switch (ty) {
            case TypeEnum::Int:
                SetVarInt(isGlobal, index, Convert<int64_t>(val), intLocals, intGlobals);
                break;
            case TypeEnum::Float:
                SetVarFloat(isGlobal, index, Convert<double>(val), fltLocals, fltGlobals);
                break;
            case TypeEnum::String:
                SetVarString(isGlobal, index, Convert<std::string>(val), strLocals, strGlobals);
                break;
            }
        }
        else {
            using Arg1T = Arg1Type<decltype(OperationT::CalcString)>;
            using Arg2T = Arg2Type<decltype(OperationT::CalcString)>;
            using RT = RetType<decltype(OperationT::CalcString)>;

            Arg1T val1{};
            Arg2T val2{};
            switch (ty1) {
            case TypeEnum::Int:
                val1 = Convert<Arg1T>(GetVarInt(isGlobal1, index1, intLocals, intGlobals));
                break;
            case TypeEnum::Float:
                val1 = Convert<Arg1T>(GetVarFloat(isGlobal1, index1, fltLocals, fltGlobals));
                break;
            case TypeEnum::String:
                val1 = Convert<Arg1T>(GetVarString(isGlobal1, index1, strLocals, strGlobals));
                break;
            }
            switch (ty2) {
            case TypeEnum::Int:
                val2 = Convert<Arg2T>(GetVarInt(isGlobal2, index2, intLocals, intGlobals));
                break;
            case TypeEnum::Float:
                val2 = Convert<Arg2T>(GetVarFloat(isGlobal2, index2, fltLocals, fltGlobals));
                break;
            case TypeEnum::String:
                val2 = Convert<Arg2T>(GetVarString(isGlobal2, index2, strLocals, strGlobals));
                break;
            }
            RT val = OperationT::CalcString(val1, val2);
            switch (ty) {
            case TypeEnum::Int:
                SetVarInt(isGlobal, index, Convert<int64_t>(val), intLocals, intGlobals);
                break;
            case TypeEnum::Float:
                SetVarFloat(isGlobal, index, Convert<double>(val), fltLocals, fltGlobals);
                break;
            case TypeEnum::String:
                SetVarString(isGlobal, index, Convert<std::string>(val), strLocals, strGlobals);
                break;
            }
        }
    }

#define DoUnaryNEG DoUnary<NegOperation>
#define DoUnaryNOT DoUnary<NotOperation>
#define DoUnaryBITNOT DoUnary<BitNotOperation>
#define DoUnaryINT2FLT DoUnary<ToFltOperation>
#define DoUnaryINT2STR DoUnary<ToStrOperation>
#define DoUnaryFLT2INT DoUnary<ToIntOperation>
#define DoUnaryFLT2STR DoUnary<ToStrOperation>
#define DoUnarySTR2INT DoUnary<ToIntOperation>
#define DoUnarySTR2FLT DoUnary<ToFltOperation>
#define DoBinaryADD DoBinary<AddOperation>
#define DoBinarySUB DoBinary<SubOperation>
#define DoBinaryMUL DoBinary<MulOperation>
#define DoBinaryDIV DoBinary<DivOperation>
#define DoBinaryMOD DoBinary<ModOperation>
#define DoBinaryAND DoBinary<AndOperation>
#define DoBinaryOR DoBinary<OrOperation>
#define DoBinaryGT DoBinary<GTOperation>
#define DoBinaryGE DoBinary<GEOperation>
#define DoBinaryEQ DoBinary<EQOperation>
#define DoBinaryNE DoBinary<NEOperation>
#define DoBinaryLE DoBinary<LEOperation>
#define DoBinaryLT DoBinary<LTOperation>
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
        int32_t AddHook(const char* name, int32_t* onEnter, int32_t onEnterSize, int32_t* onExit, int32_t onExitSize)
        {
            write_locker lock(m_Mutex);

            HookData newData{};
            if (onEnterSize > 0)
                newData.m_OnEnter.assign(onEnter, onEnter + onEnterSize);
            if (onExitSize > 0)
                newData.m_OnExit.assign(onExit, onExit + onExitSize);
            int32_t hookId = static_cast<int32_t>(m_HookDatas.size());
            m_HookMap.insert(std::make_pair(std::string(name), hookId));
            m_HookDatas.push_back(std::move(newData));
            return hookId;
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
                case InsEnum::INCV:
                    DoIncVal(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::MOV:
                    DoMov(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::ARRGET:
                    DoArrayGet(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::ARRSET:
                    DoArraySet(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::NEG:
                    DoUnaryNEG(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::ADD:
                    DoBinaryADD(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::SUB:
                    DoBinarySUB(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::MUL:
                    DoBinaryMUL(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::DIV:
                    DoBinaryDIV(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::MOD:
                    DoBinaryMOD(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
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
                case InsEnum::GE:
                    DoBinaryGE(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::EQ:
                    DoBinaryEQ(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::NE:
                    DoBinaryNE(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::LE:
                    DoBinaryLE(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
                    break;
                case InsEnum::LT:
                    DoBinaryLT(opcode, op, codes, pos, intLocals, fltLocals, strLocals, intGlobals, fltGlobals, strGlobals);
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

