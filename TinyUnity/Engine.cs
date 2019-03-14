using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;

/*
 * string input = "1 + 1 - 1";
			AntlrInputStream inputStream = new AntlrInputStream(input);
			simplecalcLexer lex = new simplecalcLexer(inputStream);
			CommonTokenStream tokenStream = new CommonTokenStream(lex);
			simplecalcParser parser = new simplecalcParser(tokenStream);
			CalcBaseVisitor cvisitor = new CalcBaseVisitor();



			parser.RemoveErrorListeners();
			var ctx = parser.compileUnit();
			Console.WriteLine(cvisitor.visitCompileUnit(ctx));

			// The code provided will print ‘Hello World’ to the console.
			// Press Ctrl+F5 (or go to Debug > Start Without Debugging) to run your app.
			Console.WriteLine("Hello World!");
			Console.ReadKey();
 * */

namespace TinyUnity {
	class Engine {
		AntlrInputStream inputStream;
		TinyUnityLexer lexer;
		TinyUnityParser parser;

		public void init(AntlrInputStream istream) {
			inputStream = istream;
			lexer = new TinyUnityLexer(inputStream);
			parser = new TinyUnityParser(new CommonTokenStream(lexer));

			var ParseTree = parser.parse();

			Scope globalScope = new Scope();
			Dictionary<string, TinyFunction> functions = new Dictionary<string, TinyFunction>();

			SymbolVisitor symVisitor = new SymbolVisitor(functions);
			symVisitor.Visit(ParseTree);

			EvalVisitor evalVisitor = new EvalVisitor(globalScope, functions);
			evalVisitor.Visit(ParseTree);
		}
	}
}
