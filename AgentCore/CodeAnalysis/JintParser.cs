using System;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Esprima;
using Esprima.Ast;
using CefDotnetApp.AgentCore.CodeAnalysis.JavaScript;

namespace CefDotnetApp.AgentCore.CodeAnalysis
{
    /// <summary>
    /// JavaScript parser using Jint engine and Esprima
    /// Provides parsing and analysis capabilities for JavaScript code
    /// </summary>
    public class JintParser
    {
        private readonly Engine _engine;
        private readonly JavaScriptParser _parser;

        public JintParser()
        {
            _engine = new Engine(options =>
            {
                options.AllowClr();
                options.CatchClrExceptions();
            });
            _parser = new JavaScriptParser();
        }

        /// <summary>
        /// Parse JavaScript code and return AST wrapper
        /// </summary>
        public JsTreeWrapper? Parse(string code, out string error)
        {
            error = string.Empty;
            try
            {
                var program = _parser.ParseScript(code);
                return new JsTreeWrapper(program, code);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Parse JavaScript code and return AST wrapper (without error info)
        /// </summary>
        public JsTreeWrapper? Parse(string code)
        {
            return Parse(code, out _);
        }

        /// <summary>
        /// Parse JavaScript code and return the AST
        /// </summary>
        public bool TryParse(string code, out string error)
        {
            error = string.Empty;
            try
            {
                _parser.ParseScript(code);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Verify if JavaScript code is syntactically correct
        /// </summary>
        public bool VerifyCode(string code)
        {
            return TryParse(code, out _);
        }

        /// <summary>
        /// Get detailed error information for invalid JavaScript code
        /// </summary>
        public string GetParseError(string code)
        {
            TryParse(code, out string error);
            return error;
        }

        /// <summary>
        /// Execute JavaScript code (for verification purposes)
        /// </summary>
        public bool TryExecute(string code, out string error)
        {
            error = string.Empty;
            try
            {
                _engine.Execute(code);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}
