using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace TinyUnity {
	class SymbolVisitor : TinyUnityBaseVisitor<TinyValue> {

		private Dictionary<string, TinyFunction> functions;

		public SymbolVisitor(Dictionary<string, TinyFunction> functions) {
			this.functions = functions;
		}
		//		public delegate TinyValue TinyFunction(object[] args);
		public override TinyValue VisitFunctionDecl([NotNull] TinyUnityParser.FunctionDeclContext context) {
			if(context.idList() == null)
			{
				IParseTree block = context.parenBlock();
				string id = context.Identifier().GetText();

				functions.Add(id, new TinyFunction(null, block));
				return null;
			} else
			{
				var parameters = context.idList().Identifier().ToList();
				IParseTree block = context.parenBlock();
				string id = context.Identifier().GetText() + parameters.Count();

				functions.Add(id, new TinyFunction(parameters, block));
				return null;
			}
		}
	}
}
