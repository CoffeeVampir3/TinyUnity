using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;

namespace TinyUnity {
	public class EvalException : Exception {
		public EvalException(ParserRuleContext context) {
			throw new Exception("Illegal expression: " + context.GetText() + " starting " + context.start.Line + " to " + context.stop.Line);
		}

		public EvalException(string msg, ParserRuleContext context) {
			throw new Exception(msg + "Illegal expression: " + context.GetText() + " starting " + context.start.Line + " to " + context.stop.Line);
		}
	}
}
