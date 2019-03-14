using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyUnity {
	public class Scope {
		public Scope parent;
		private Dictionary<string, TinyValue> variables;

		public Scope() {
			parent = null;
			variables = new Dictionary<string, TinyValue>();
		}

		public Scope(Scope p) {
			parent = p;
			variables = new Dictionary<string, TinyValue>();
		}

		public void assignParam(string var, TinyValue val) {
			variables.Add(var, val);
		}

		public void assign(string var, TinyValue val) {
			if (resolve(var) != null)
			{
				reAssign(var, val);
			} else
			{
				variables.Add(var, val);
			}
		}

		public void reAssign(string var, TinyValue val) {
			if (variables.ContainsKey(var))
			{
				variables[var] = val;
			}
			else if (parent != null)
			{
				parent.reAssign(var, val);
			}
		}

		public bool isGlobalScope() {
			return parent == null;
		}

		public TinyValue resolve(string var) {
			TinyValue val;
			if (variables.TryGetValue(var, out val))
			{
				return val;
			}
			else if (!isGlobalScope())
			{
				return parent.resolve(var);
			}
			return null;
		}

		public override string ToString() {
			StringBuilder builder = new StringBuilder();
			foreach (KeyValuePair<string, TinyValue> items in variables)
			{
				builder.Append(items.Key + "->" + (items.Value).ToString());
			}
			return builder.ToString();
		}
	}
}
