using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Tree;

namespace TinyUnity {
	public class TinyFunction {
		private List<ITerminalNode> parameters;
		private IParseTree functionBlock;

		public TinyFunction(List<ITerminalNode> args, IParseTree block) {
			this.parameters = args;
			this.functionBlock = block;
		}

		public TinyValue invokeEmpty(Dictionary<string, TinyFunction> functions, Scope scope) {
			Scope functionScope = new Scope(null);
			EvalVisitor evisitorNext = new EvalVisitor(functionScope, functions);

			//Do stuff
			TinyValue ret = TinyValue.NULL;
			try
			{
				evisitorNext.Visit(this.functionBlock);
			}
			catch (ReturnValue returnValue)
			{
				ret = returnValue.val;
			}
			return ret;
		}

		public TinyValue invoke(List<TinyUnityParser.ExpressionContext> args, Dictionary<string, TinyFunction> functions, Scope scope) {
			if(args == null)
			{
				return invokeEmpty(functions, scope);
			}

			if (args.Count() != this.parameters.Count())
			{
				throw new ArgumentException("Attempted to call function with invalid arguments");
			}
			Scope functionScope = new Scope(null);

			EvalVisitor evisitor = new EvalVisitor(scope, functions);
			for(int i = 0; i < this.parameters.Count(); i++)
			{
				TinyValue val = evisitor.Visit(args[i]);
				functionScope.assignParam(this.parameters[i].GetText(), val);
			}
			EvalVisitor evisitorNext = new EvalVisitor(functionScope, functions);

			//Do stuff
			TinyValue ret = TinyValue.NULL;
			try
			{
				evisitorNext.Visit(this.functionBlock);
			}
			catch (ReturnValue returnValue)
			{
				ret = returnValue.val;
			}
			return ret;
		}
	}
}
