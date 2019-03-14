#define __DEBUGMODE__

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace TinyUnity {
	class EvalVisitor : TinyUnityBaseVisitor<TinyValue> {
		private static ReturnValue returnValue = new ReturnValue();
		private Scope currentScope;
		private Dictionary<string, TinyFunction> functions;

		public EvalVisitor(Scope scope, Dictionary<string, TinyFunction> functions) {
			this.currentScope = scope;
			this.functions = functions;
		}

		public override TinyValue VisitUnaryMinusExpression([NotNull] TinyUnityParser.UnaryMinusExpressionContext context) {
			TinyValue val = this.Visit(context.expression());

			if (val.value is float)
			{
				return new TinyValue(-1.0f * val.asFloat());
			}

			throw new EvalException(context);
		}

		public override TinyValue VisitNotExpression([NotNull] TinyUnityParser.NotExpressionContext context) {
			TinyValue val = this.Visit(context.expression());
			if (val.value is bool)
			{
				return new TinyValue(!(bool)val.value);
			}

			throw new EvalException(context);
		}

		#region Math

		public override TinyValue VisitPowerExpression([NotNull] TinyUnityParser.PowerExpressionContext context) {
			TinyValue lhs = this.Visit(context.expression(0));
			TinyValue rhs = this.Visit(context.expression(1));

			if (lhs.value is float && rhs.value is float)
			{
				return new TinyValue(Math.Pow(lhs.asFloat(), rhs.asFloat()));
			}

			throw new EvalException(context);
		}

		#region MultiplyDivideModulus

		public override TinyValue VisitMultExpression([NotNull] TinyUnityParser.MultExpressionContext context) {
			switch (context.op.Type)
			{
				case TinyUnityLexer.Multiply:
					return Multiply(context);
				case TinyUnityLexer.Divide:
					return Divide(context);
				case TinyUnityLexer.Modulus:
					return Modulus(context);
				default:
					throw new EvalException("unknown operator type: " + context.op.Type, context);
			}
		}

		private TinyValue Multiply(TinyUnityParser.MultExpressionContext context) {
			TinyValue lhs = this.Visit(context.expression(0));
			TinyValue rhs = this.Visit(context.expression(1));

			if(lhs == null || rhs == null)
				throw new EvalException("unknown operator type: " + context.op.Type, context);

			if(lhs.value is float && rhs.value is float)
			{
				return new TinyValue((float)lhs.value * (float)rhs.value);
			}

			throw new EvalException(context);
		}

		private TinyValue Divide(TinyUnityParser.MultExpressionContext context) {
			TinyValue lhs = this.Visit(context.expression(0));
			TinyValue rhs = this.Visit(context.expression(1));

			if (lhs == null || rhs == null)
				throw new EvalException("unknown operator type: " + context.op.Type, context);

			if (lhs.value is float && rhs.value is float)
			{
				return new TinyValue((float)lhs.value / (float)rhs.value);
			}

			throw new EvalException(context);
		}

		private TinyValue Modulus(TinyUnityParser.MultExpressionContext context) {
			TinyValue lhs = this.Visit(context.expression(0));
			TinyValue rhs = this.Visit(context.expression(1));

			if (lhs == null || rhs == null)
				throw new EvalException("unknown operator type: " + context.op.Type, context);

			if (lhs.value is float && rhs.value is float)
			{
				return new TinyValue((float)lhs.value % (float)rhs.value);
			}

			throw new EvalException(context);
		}

		#endregion

		#region AddSubtract

		public override TinyValue VisitAddExpression([NotNull] TinyUnityParser.AddExpressionContext context) {
			switch (context.op.Type)
			{
				case TinyUnityLexer.Add:
					return Add(context);
				case TinyUnityLexer.Subtract:
					return Subtract(context);
				default:
					throw new EvalException("Bad operators: " + context.op.Type, context);
			}
		}

		private TinyValue Add(TinyUnityParser.AddExpressionContext context) {
			TinyValue lhs = this.Visit(context.expression(0));
			TinyValue rhs = this.Visit(context.expression(1));

			if (lhs == null || rhs == null)
				throw new EvalException("Null operators: " + context.op.Type, context);

			if (lhs.value is float && rhs.value is float)
			{
				return new TinyValue((float)lhs.value + (float)rhs.value);
			}

			if (lhs.isList())
			{
				List<TinyValue> nlst = lhs.asList();
				nlst.Add(rhs);
				return new TinyValue(nlst);
			}

			if(lhs.isString())
			{
				return new TinyValue(lhs.asString() + rhs.value.ToString());
			}

			if (rhs.isString())
			{
				return new TinyValue(lhs.value.ToString() + rhs.asString());
			}

			return new TinyValue(lhs.value.ToString() + rhs.value.ToString());
		}

		private TinyValue Subtract(TinyUnityParser.AddExpressionContext context) {
			TinyValue lhs = this.Visit(context.expression(0));
			TinyValue rhs = this.Visit(context.expression(1));

			if (lhs == null || rhs == null)
				throw new EvalException("Null operators: " + context.op.Type, context);

			if (lhs.value is float && rhs.value is float)
			{
				return new TinyValue((float)lhs.value - (float)rhs.value);
			}

			if (lhs.isList())
			{
				List<TinyValue> nlst = lhs.asList();
				nlst.Remove(rhs);
				return new TinyValue(nlst);
			}

			throw new EvalException("Bad operators: " + context.op.Type, context);
		}

		#endregion

		#region Comparisons

		public override TinyValue VisitCompExpression([NotNull] TinyUnityParser.CompExpressionContext context) {
			switch (context.op.Type)
			{
				case TinyUnityLexer.LT:
					return lessThan(context);
				case TinyUnityLexer.LTEquals:
					return lessThanEq(context);
				case TinyUnityLexer.GT:
					return greaterThan(context);
				case TinyUnityLexer.GTEquals:
					return greaterThanEq(context);
				default:
					throw new EvalException("unknown operator type: " + context.op.Type, context);
			}
		}

		private TinyValue lessThan(TinyUnityParser.CompExpressionContext context) {
			TinyValue lhs = this.Visit(context.expression(0));
			TinyValue rhs = this.Visit(context.expression(1));

			if (lhs == null || rhs == null)
				throw new EvalException("Null operators: " + context.op.Type, context);

			if (lhs.value is float && rhs.value is float)
			{
				return new TinyValue((float)lhs.value < (float)rhs.value);
			}

			if (lhs.isString() && rhs.isString())
			{
				return new TinyValue(lhs.asString().CompareTo(rhs.asString()) < 0);
			}

			throw new EvalException("Bad operators: " + context.op.Type, context);
		}

		private TinyValue lessThanEq(TinyUnityParser.CompExpressionContext context) {
			TinyValue lhs = this.Visit(context.expression(0));
			TinyValue rhs = this.Visit(context.expression(1));

			if (lhs == null || rhs == null)
				throw new EvalException("Null operators: " + context.op.Type, context);

			if (lhs.value is float && rhs.value is float)
			{
				return new TinyValue((float)lhs.value <= (float)rhs.value);
			}

			if (lhs.isString() && rhs.isString())
			{
				return new TinyValue(lhs.asString().CompareTo(rhs.asString()) <= 0);
			}

			throw new EvalException("Bad operators: " + context.op.Type, context);
		}

		private TinyValue greaterThan(TinyUnityParser.CompExpressionContext context) {
			TinyValue lhs = this.Visit(context.expression(0));
			TinyValue rhs = this.Visit(context.expression(1));

			if (lhs == null || rhs == null)
				throw new EvalException("Null operators: " + context.op.Type, context);

			if (lhs.value is float && rhs.value is float)
			{
				return new TinyValue((float)lhs.value > (float)rhs.value);
			}

			if (lhs.isString() && rhs.isString())
			{
				return new TinyValue(lhs.asString().CompareTo(rhs.asString()) > 0);
			}

			throw new EvalException("Bad operators: " + context.op.Type, context);
		}

		private TinyValue greaterThanEq(TinyUnityParser.CompExpressionContext context) {
			TinyValue lhs = this.Visit(context.expression(0));
			TinyValue rhs = this.Visit(context.expression(1));

			if (lhs == null || rhs == null)
				throw new EvalException("Null operators: " + context.op.Type, context);

			if (lhs.value is float && rhs.value is float)
			{
				return new TinyValue((float)lhs.value >= (float)rhs.value);
			}

			if (lhs.isString() && rhs.isString())
			{
				return new TinyValue(lhs.asString().CompareTo(rhs.asString()) >= 0);
			}

			throw new EvalException("Bad operators: " + context.op.Type, context);
		}

		#endregion

		#region equality

		public override TinyValue VisitEqExpression([NotNull] TinyUnityParser.EqExpressionContext context) {
			switch (context.op.Type)
			{
				case TinyUnityLexer.Equals:
					return equals(context);
				case TinyUnityLexer.NEquals:
					return notEquals(context);
				default:
					throw new EvalException("unknown operator type: " + context.op.Type, context);
			}
		}

		private TinyValue equals(TinyUnityParser.EqExpressionContext context) {
			TinyValue lhs = this.Visit(context.expression(0));
			TinyValue rhs = this.Visit(context.expression(1));

			if (lhs == null)
				throw new EvalException("Null operator: " + context.op.Type, context);

			return new TinyValue(lhs.Equals(rhs));
		}

		private TinyValue notEquals(TinyUnityParser.EqExpressionContext context) {
			TinyValue lhs = this.Visit(context.expression(0));
			TinyValue rhs = this.Visit(context.expression(1));

			if (lhs == null)
				throw new EvalException("Null operator: " + context.op.Type, context);

			return new TinyValue(!lhs.Equals(rhs));
		}

		#endregion

		#endregion

		#region BinaryOps

		public override TinyValue VisitAndExpression([NotNull] TinyUnityParser.AndExpressionContext context) {
			TinyValue lhs = this.Visit(context.expression(0));
			TinyValue rhs = this.Visit(context.expression(1));

			if(lhs.value is bool && rhs.value is bool)
			{
				return new TinyValue(lhs.asBoolean() && rhs.asBoolean());
			}

			throw new EvalException(context);
		}

		public override TinyValue VisitOrExpression([NotNull] TinyUnityParser.OrExpressionContext context) {
			TinyValue lhs = this.Visit(context.expression(0));
			TinyValue rhs = this.Visit(context.expression(1));

			if (lhs.value is bool && rhs.value is bool)
			{
				return new TinyValue(lhs.asBoolean() || rhs.asBoolean());
			}

			throw new EvalException(context);
		}

		public override TinyValue VisitTernaryExpression([NotNull] TinyUnityParser.TernaryExpressionContext context) {
			TinyValue condition = this.Visit(context.expression(0));

			if (condition.asBoolean())
			{
				return new TinyValue(this.Visit(context.expression(1)));
			}

			return new TinyValue(this.Visit(context.expression(2)));
		}

		public override TinyValue VisitInExpression([NotNull] TinyUnityParser.InExpressionContext context) {
			TinyValue lhs = this.Visit(context.expression(0));
			TinyValue rhs = this.Visit(context.expression(1));

			if (rhs.isList())
			{
				foreach(TinyValue val in (List<TinyValue>)rhs.value)
				{
					if(val.Equals(lhs))
					{
						return new TinyValue(true);
					}
				}
				return new TinyValue(false);
			}
			throw new EvalException(context);
		}

		#endregion

		#region Primitives

		public override TinyValue VisitBoolExpression([NotNull] TinyUnityParser.BoolExpressionContext context) {
			return new TinyValue(bool.Parse(context.GetText()));
		}

		public override TinyValue VisitFloatExpression([NotNull] TinyUnityParser.FloatExpressionContext context) {
			return new TinyValue(float.Parse(context.GetText()));
		}

		public override TinyValue VisitNullExpression([NotNull] TinyUnityParser.NullExpressionContext context) {
			return TinyValue.NULL;
		}

		#endregion

		#region IndexHelpers

		private TinyValue resolveIndexes(TinyValue subject, List<TinyUnityParser.ExpressionContext> indexes) {
			foreach (TinyUnityParser.ExpressionContext context in indexes)
			{
				TinyValue indice = this.Visit(context);

				if (!(indice.value is float))
				{
					throw new EvalException("Could not resolve indexes of " + subject.value, context);
				}

				if (!(subject.isList()) && !(subject.isString()))
				{
					throw new EvalException("Not a list " + subject.value, context);
				}

				int i = (int)(float)(indice.value);
				if (subject.isString())
				{
					subject = new TinyValue(subject.asString().Substring(i, i + 1));
				}
				else
				{
					subject = subject.asList()[i];
				}
			}

			return subject;
		}

		private TinyValue setAtIndex(TinyValue subject, TinyValue newValue,
			List<TinyUnityParser.ExpressionContext> indexes, ParserRuleContext context) {

			if (!subject.isList())
				throw new EvalException(context);

			for (int i = 0; i < indexes.Count; i++)
			{
				TinyValue indice = this.Visit(indexes[i]);
				if (!(indice.value is float))
					throw new EvalException(context);
				int next = (int)indice.value;
				subject = subject.asList()[next];
			}
			TinyValue id = this.Visit(indexes[indexes.Count]);
			if (!(id.value is float))
				throw new EvalException(context);

			subject.asList()[(int)id.value] = newValue;
			return subject;
		}

		#endregion

		#region List

		//List constructor
		public override TinyValue VisitList([NotNull] TinyUnityParser.ListContext context) {
			List<TinyValue> list = null;
			if (context.exprList() != null)
			{
				list = new List<TinyValue>();
				foreach (TinyUnityParser.ExpressionContext expr in context.exprList().expression())
				{
					list.Add(this.Visit(expr));
				}
			}

			#if __DEBUGMODE__
			StringBuilder builder = new StringBuilder();
			foreach (TinyValue item in list)
			{
				if (item != null)
					builder.Append(item.value.ToString());
			}
			Console.WriteLine("Generated list: " + builder.ToString());
			#endif


			return new TinyValue(list);
		}

		public override TinyValue VisitListExpression([NotNull] TinyUnityParser.ListExpressionContext context) {
			TinyValue val = this.Visit(context.list());

			if(context.indexes() != null)
			{
				List<TinyUnityParser.ExpressionContext> exprs = context.indexes().expression().ToList();
				val = resolveIndexes(val, exprs);
			}

			return val;
		}

		#endregion

		#region TinyFunctions

		//Handled by symbol visitor
		public override TinyValue VisitFunctionDecl([NotNull] TinyUnityParser.FunctionDeclContext context) {
			return TinyValue.NULL;
		}

		public override TinyValue VisitFunctionCallExpression([NotNull] TinyUnityParser.FunctionCallExpressionContext context) {
			TinyValue val = this.Visit(context.functionCall());
			if (context.indexes() != null)
			{
				List<TinyUnityParser.ExpressionContext> exprs = context.indexes().expression().ToList();
				val = resolveIndexes(val, exprs);
			}
			return val;
		}

		public override TinyValue VisitIdentifierFunctionCall([NotNull] TinyUnityParser.IdentifierFunctionCallContext context) {
			if(context.exprList() == null)
			{
				string id = context.Identifier().GetText();
				TinyFunction function;

				if (functions.TryGetValue(id, out function))
				{
					return function.invoke(null, functions, currentScope);
				}
			} else
			{
				var parameters = context.exprList().expression().ToList();
				string id = context.Identifier().GetText() + parameters.Count();
				TinyFunction function;

				if (functions.TryGetValue(id, out function))
				{
					return function.invoke(parameters, functions, currentScope);
				}
			}
			throw new EvalException(context);
		}

		#endregion

		#region variables

		public override TinyValue VisitAssignment([NotNull] TinyUnityParser.AssignmentContext context) {
			TinyValue assignedVal = this.Visit(context.expression());

			if (context.indexes() != null)
			{
				TinyValue assigner = currentScope.resolve(context.Identifier().GetText());
				List<TinyUnityParser.ExpressionContext> exprs = context.indexes().expression().ToList();
				setAtIndex(assigner, assignedVal, exprs, context);
			}
			else
			{
				string id = context.Identifier().GetText();
				currentScope.assign(id, assignedVal);
			}

			return TinyValue.NULL;
		}


		public override TinyValue VisitIdentifierExpression([NotNull] TinyUnityParser.IdentifierExpressionContext context) {
			string id = context.Identifier().GetText();
			TinyValue val = currentScope.resolve(id);

			if(context.indexes() != null)
			{
				List<TinyUnityParser.ExpressionContext> exprs = context.indexes().expression().ToList();
				val = resolveIndexes(val, exprs);
			}
			return val;
		}

		public override TinyValue VisitStringExpression([NotNull] TinyUnityParser.StringExpressionContext context) {
			string s = context.GetText();
			s = s.Substring(1, s.Length - 2).Replace("\\\\( .)", "$1");
			TinyValue val = new TinyValue(s);

			if (context.indexes() != null)
			{
				List<TinyUnityParser.ExpressionContext> exprs = context.indexes().expression().ToList();
				val = resolveIndexes(val, exprs);
			}
			return val;
		}

		public override TinyValue VisitExpressionExpression([NotNull] TinyUnityParser.ExpressionExpressionContext context) {
			TinyValue val = this.Visit(context.expression());

			if (context.indexes() != null)
			{
				List<TinyUnityParser.ExpressionContext> exprs = context.indexes().expression().ToList();
				val = resolveIndexes(val, exprs);
			}
			return val;
		}

		#endregion

		#region Native Functions

		/// <summary>
		///  print(stuff);
		/// </summary>
		/// <param name="context"></param>
		/// <returns>NULL</returns>
		public override TinyValue VisitPrintFunctionCall([NotNull] TinyUnityParser.PrintFunctionCallContext context) {
			Console.Write(this.Visit(context.expression()));
			return TinyValue.NULL;
		}

		/// <summary>
		/// println(stuff);
		/// </summary>
		/// <param name="context"></param>
		/// <returns>NULL</returns>
		public override TinyValue VisitPrintlnFunctionCall([NotNull] TinyUnityParser.PrintlnFunctionCallContext context) {
			Console.WriteLine(this.Visit(context.expression()));
			return TinyValue.NULL;
		}

		/// <summary>
		/// assert(bool);
		/// </summary>
		/// <param name="context"></param>
		/// <returns>NULL</returns>
		public override TinyValue VisitAssertFunctionCall([NotNull] TinyUnityParser.AssertFunctionCallContext context) {
			TinyValue val = this.Visit(context.expression());

			if (!(val.value is bool))
				throw new EvalException(context);

			if(!val.asBoolean())
			{
				throw new EvalException("Assertion failed " + context.expression().GetText(), context);
			}

			return TinyValue.NULL;
		}

		/// <summary>
		/// size(list or string)
		/// </summary>
		/// <param name="context"></param>
		/// <returns>Length of list or string as TinyValue(float)</returns>
		public override TinyValue VisitSizeFunctionCall([NotNull] TinyUnityParser.SizeFunctionCallContext context) {
			TinyValue val = this.Visit(context.expression());

			if(val.isString())
			{
				return new TinyValue((float)val.asString().Length);
			}

			if (val.isList())
			{
				return new TinyValue((float)val.asList().Count);
			}

			throw new EvalException("Type does not have size " + context.expression().GetText(), context);
		}

		public override TinyValue VisitIfStatement([NotNull] TinyUnityParser.IfStatementContext context) {
			TinyValue val = this.Visit(context.ifStat().expression());

			if (val.asBoolean())
				return this.Visit(context.ifStat().parenBlock());

			for (int i = 0; i < context.elseIfStat().Length; i++)
			{
				if (this.Visit(context.elseIfStat(i).expression()).asBoolean())
				{
					return this.Visit(context.elseIfStat(i).parenBlock());
				}
			}

			if (context.elseStat() != null)
			{
				return this.Visit(context.elseStat().parenBlock());
			}

			return TinyValue.NULL;
		}

		public override TinyValue VisitBlock([NotNull] TinyUnityParser.BlockContext context) {
			currentScope = new Scope(currentScope);

			foreach(TinyUnityParser.StatementContext stctx in context.statement())
			{
				this.Visit(stctx);
			}
			TinyUnityParser.ExpressionContext exprctx = context.expression();
			//This indicates we are returning from our expression
			if(exprctx != null)
			{
				returnValue.val = this.Visit(exprctx);
				currentScope = currentScope.parent;
				throw returnValue;
			}
			currentScope = currentScope.parent;
			return TinyValue.NULL;
		}

		public override TinyValue VisitParenBlock([NotNull] TinyUnityParser.ParenBlockContext context) {
			return VisitBlock(context.block());
		}

		public override TinyValue VisitForStatement([NotNull] TinyUnityParser.ForStatementContext context) {
			int start = (int)(float)(this.Visit(context.expression(0)).value);
			int end = (int)(float)(this.Visit(context.expression(1)).value);

			for(int i = start; i <= end; i++)
			{
				currentScope.assign(context.Identifier().GetText(), new TinyValue(i));
				TinyValue ret = this.Visit(context.parenBlock());
				if(ret != TinyValue.NULL)
				{
					return ret;
				}
			}
			return TinyValue.NULL;
		}

		public override TinyValue VisitWhileStatement([NotNull] TinyUnityParser.WhileStatementContext context) {
			while( this.Visit(context.expression()).asBoolean() )
			{
				TinyValue ret = this.Visit(context.parenBlock());
				if (ret != TinyValue.NULL)
				{
					return ret;
				}
			}
			return TinyValue.NULL;
		}

		#endregion
	}
}
