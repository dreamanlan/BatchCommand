using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CefDotnetApp.AgentCore.Models;

namespace AgentCore.CodeAnalysis
{
    // Code metrics and analysis utilities
    public static class CodeMetricsAnalyzer
    {
        // Extract documentation comment from a syntax node
        public static string ExtractDocumentationComment(SyntaxNode node)
        {
            var trivia = node.GetLeadingTrivia();
            var docComments = new List<string>();

            foreach (var t in trivia)
            {
                if (t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                    t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
                {
                    docComments.Add(t.ToString().Trim());
                }
            }

            return docComments.Count > 0 ? string.Join("\n", docComments) : null;
        }

        // Calculate cyclomatic complexity of a method
        public static int CalculateCyclomaticComplexity(MethodDeclarationSyntax method)
        {
            int complexity = 1;  // Base complexity

            var body = method.Body;
            if (body == null) return complexity;

            // Count decision points
            var decisionNodes = body.DescendantNodes().Where(node =>
                node.IsKind(SyntaxKind.IfStatement) ||
                node.IsKind(SyntaxKind.WhileStatement) ||
                node.IsKind(SyntaxKind.ForStatement) ||
                node.IsKind(SyntaxKind.ForEachStatement) ||
                node.IsKind(SyntaxKind.CaseSwitchLabel) ||
                node.IsKind(SyntaxKind.CatchClause) ||
                node.IsKind(SyntaxKind.ConditionalExpression) ||
                node.IsKind(SyntaxKind.LogicalAndExpression) ||
                node.IsKind(SyntaxKind.LogicalOrExpression) ||
                node.IsKind(SyntaxKind.CoalesceExpression)
            );

            complexity += decisionNodes.Count();

            return complexity;
        }

        // Calculate line count for a syntax node
        public static int CalculateLineCount(SyntaxNode node)
        {
            var span = node.GetLocation().GetLineSpan();
            return span.EndLinePosition.Line - span.StartLinePosition.Line + 1;
        }

        // Count total lines, code lines, and comment lines in a file
        public static (int totalLines, int codeLines, int commentLines) AnalyzeFileLines(SyntaxTree tree)
        {
            var root = tree.GetRoot();
            var text = root.GetText();
            int totalLines = text.Lines.Count;

            // Count comment lines
            int commentLines = 0;
            var triviaList = root.DescendantTrivia();
            foreach (var trivia in triviaList)
            {
                if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                    trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) ||
                    trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                    trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
                {
                    var triviaSpan = trivia.GetLocation().GetLineSpan();
                    commentLines += triviaSpan.EndLinePosition.Line - triviaSpan.StartLinePosition.Line + 1;
                }
            }

            // Count blank lines
            int blankLines = 0;
            foreach (var line in text.Lines)
            {
                if (string.IsNullOrWhiteSpace(line.ToString()))
                {
                    blankLines++;
                }
            }

            int codeLines = totalLines - commentLines - blankLines;

            return (totalLines, codeLines, commentLines);
        }

        // Extract dependencies from a syntax tree
        public static List<DependencyInfo> ExtractDependencies(SyntaxTree tree)
        {
            var dependencies = new List<DependencyInfo>();
            var root = tree.GetRoot();

            // Extract using directives
            var usingDirectives = root.DescendantNodes().OfType<UsingDirectiveSyntax>();
            foreach (var usingDir in usingDirectives)
            {
                dependencies.Add(new DependencyInfo
                {
                    Name = usingDir.Name.ToString().Split('.').LastOrDefault(),
                    Type = "using",
                    FullName = usingDir.Name.ToString(),
                    Location = GetCodeLocation(tree, usingDir)
                });
            }

            // Extract type references (simplified - only from base lists)
            var typeDeclarations = root.DescendantNodes().OfType<TypeDeclarationSyntax>();
            foreach (var typeDecl in typeDeclarations)
            {
                if (typeDecl.BaseList != null)
                {
                    foreach (var baseType in typeDecl.BaseList.Types)
                    {
                        var typeName = baseType.Type.ToString();
                        dependencies.Add(new DependencyInfo
                        {
                            Name = typeName.Split('.').LastOrDefault(),
                            Type = "type_reference",
                            FullName = typeName,
                            Location = GetCodeLocation(tree, baseType)
                        });
                    }
                }
            }

            return dependencies;
        }

        // Check if a method is async
        public static bool IsAsync(MethodDeclarationSyntax method)
        {
            return method.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword));
        }

        // Check if a method is static
        public static bool IsStatic(MethodDeclarationSyntax method)
        {
            return method.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));
        }

        // Check if a method is public
        public static bool IsPublic(MethodDeclarationSyntax method)
        {
            return method.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
        }

        // Check if a property is static
        public static bool IsStatic(PropertyDeclarationSyntax property)
        {
            return property.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));
        }

        // Check if a property is public
        public static bool IsPublic(PropertyDeclarationSyntax property)
        {
            return property.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
        }

        // Check if a field is static
        public static bool IsStatic(FieldDeclarationSyntax field)
        {
            return field.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));
        }

        // Check if a field is public
        public static bool IsPublic(FieldDeclarationSyntax field)
        {
            return field.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
        }

        // Check if a constructor is public
        public static bool IsPublic(ConstructorDeclarationSyntax constructor)
        {
            return constructor.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
        }

        // Check if an event is static
        public static bool IsStatic(EventDeclarationSyntax evt)
        {
            return evt.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));
        }

        // Check if an event is public
        public static bool IsPublic(EventDeclarationSyntax evt)
        {
            return evt.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
        }

        // Check if an enum is public
        public static bool IsPublicType(EnumDeclarationSyntax enumDecl)
        {
            return enumDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
        }

        // Check if a type is abstract
        public static bool IsAbstract(TypeDeclarationSyntax type)
        {
            return type.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword));
        }

        // Check if a type is sealed
        public static bool IsSealed(TypeDeclarationSyntax type)
        {
            return type.Modifiers.Any(m => m.IsKind(SyntaxKind.SealedKeyword));
        }

        // Check if a type is public
        public static bool IsPublicType(TypeDeclarationSyntax type)
        {
            return type.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
        }

        // Count members in a type
        public static int CountMembers(TypeDeclarationSyntax type)
        {
            return type.Members.Count;
        }

        // Helper method to get CodeLocation from syntax node
        private static CodeLocation GetCodeLocation(SyntaxTree tree, SyntaxNode node)
        {
            var span = node.Span;
            var lineSpan = tree.GetLineSpan(span);

            return new CodeLocation(
                tree.FilePath,
                lineSpan.StartLinePosition.Line + 1,
                lineSpan.EndLinePosition.Line + 1,
                lineSpan.StartLinePosition.Character + 1,
                lineSpan.EndLinePosition.Character + 1
            );
        }
    }
}
