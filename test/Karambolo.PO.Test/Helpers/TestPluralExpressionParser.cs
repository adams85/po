/*
 * WARNING: this file has been generated by
 * Hime Parser Generator 3.5.1.0
 */
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Hime.Redist;
using Hime.Redist.Parsers;

namespace Karambolo.Po.Test.Helpers
{
	/// <summary>
	/// Represents a parser
	/// </summary>
	[GeneratedCodeAttribute("Hime.SDK", "3.5.1.0")]
	internal class TestPluralExpressionParser : LRkParser
	{
		/// <summary>
		/// The automaton for this parser
		/// </summary>
		private static readonly LRkAutomaton commonAutomaton = LRkAutomaton.Find(typeof(TestPluralExpressionParser), "TestPluralExpressionParser.bin");
		/// <summary>
		/// Contains the constant IDs for the variables and virtuals in this parser
		/// </summary>
		[GeneratedCodeAttribute("Hime.SDK", "3.5.1.0")]
		public class ID
		{
			/// <summary>
			/// The unique identifier for variable expression
			/// </summary>
			public const int VariableExpression = 0x0007;
			/// <summary>
			/// The unique identifier for variable logical_or_expression
			/// </summary>
			public const int VariableLogicalOrExpression = 0x0008;
			/// <summary>
			/// The unique identifier for variable logical_and_expression
			/// </summary>
			public const int VariableLogicalAndExpression = 0x0009;
			/// <summary>
			/// The unique identifier for variable equality_expression
			/// </summary>
			public const int VariableEqualityExpression = 0x000A;
			/// <summary>
			/// The unique identifier for variable relational_expression
			/// </summary>
			public const int VariableRelationalExpression = 0x000B;
			/// <summary>
			/// The unique identifier for variable additive_expression
			/// </summary>
			public const int VariableAdditiveExpression = 0x000C;
			/// <summary>
			/// The unique identifier for variable multiplicative_expression
			/// </summary>
			public const int VariableMultiplicativeExpression = 0x000D;
			/// <summary>
			/// The unique identifier for variable factor
			/// </summary>
			public const int VariableFactor = 0x000E;
		}
		/// <summary>
		/// The collection of variables matched by this parser
		/// </summary>
		/// <remarks>
		/// The variables are in an order consistent with the automaton,
		/// so that variable indices in the automaton can be used to retrieve the variables in this table
		/// </remarks>
		private static readonly Symbol[] variables = {
			new Symbol(0x0007, "expression"), 
			new Symbol(0x0008, "logical_or_expression"), 
			new Symbol(0x0009, "logical_and_expression"), 
			new Symbol(0x000A, "equality_expression"), 
			new Symbol(0x000B, "relational_expression"), 
			new Symbol(0x000C, "additive_expression"), 
			new Symbol(0x000D, "multiplicative_expression"), 
			new Symbol(0x000E, "factor"), 
			new Symbol(0x0020, "__VAxiom") };
		/// <summary>
		/// The collection of virtuals matched by this parser
		/// </summary>
		/// <remarks>
		/// The virtuals are in an order consistent with the automaton,
		/// so that virtual indices in the automaton can be used to retrieve the virtuals in this table
		/// </remarks>
		private static readonly Symbol[] virtuals = {
 };
		/// <summary>
		/// Initializes a new instance of the parser
		/// </summary>
		/// <param name="lexer">The input lexer</param>
		public TestPluralExpressionParser(TestPluralExpressionLexer lexer) : base (commonAutomaton, variables, virtuals, null, lexer) { }

		/// <summary>
		/// Visitor interface
		/// </summary>
		[GeneratedCodeAttribute("Hime.SDK", "3.5.1.0")]
		public class Visitor
		{
			public virtual void OnTerminalWhiteSpace(ASTNode node) {}
			public virtual void OnTerminalSeparator(ASTNode node) {}
			public virtual void OnTerminalInteger(ASTNode node) {}
			public virtual void OnTerminalVariable(ASTNode node) {}
			public virtual void OnVariableExpression(ASTNode node) {}
			public virtual void OnVariableLogicalOrExpression(ASTNode node) {}
			public virtual void OnVariableLogicalAndExpression(ASTNode node) {}
			public virtual void OnVariableEqualityExpression(ASTNode node) {}
			public virtual void OnVariableRelationalExpression(ASTNode node) {}
			public virtual void OnVariableAdditiveExpression(ASTNode node) {}
			public virtual void OnVariableMultiplicativeExpression(ASTNode node) {}
			public virtual void OnVariableFactor(ASTNode node) {}
		}

		/// <summary>
		/// Walk the AST of a result using a visitor
		/// <param name="result">The parse result</param>
		/// <param name="visitor">The visitor to use</param>
		/// </summary>
		public static void Visit(ParseResult result, Visitor visitor)
		{
			VisitASTNode(result.Root, visitor);
		}

		/// <summary>
		/// Walk the sub-AST from the specified node using a visitor
		/// </summary>
		/// <param name="node">The AST node to start from</param>
		/// <param name="visitor">The visitor to use</param>
		public static void VisitASTNode(ASTNode node, Visitor visitor)
		{
			for (int i = 0; i < node.Children.Count; i++)
				VisitASTNode(node.Children[i], visitor);
			switch(node.Symbol.ID)
			{
				case 0x0003: visitor.OnTerminalWhiteSpace(node); break;
				case 0x0004: visitor.OnTerminalSeparator(node); break;
				case 0x0005: visitor.OnTerminalInteger(node); break;
				case 0x0006: visitor.OnTerminalVariable(node); break;
				case 0x0007: visitor.OnVariableExpression(node); break;
				case 0x0008: visitor.OnVariableLogicalOrExpression(node); break;
				case 0x0009: visitor.OnVariableLogicalAndExpression(node); break;
				case 0x000A: visitor.OnVariableEqualityExpression(node); break;
				case 0x000B: visitor.OnVariableRelationalExpression(node); break;
				case 0x000C: visitor.OnVariableAdditiveExpression(node); break;
				case 0x000D: visitor.OnVariableMultiplicativeExpression(node); break;
				case 0x000E: visitor.OnVariableFactor(node); break;
			}
		}
	}
}
