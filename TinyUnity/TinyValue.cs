using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * This is a C# fork of Tiny, credit to the original creator bkiers 
 * */

#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()

namespace TinyUnity {
	public class TinyValue : System.Collections.IComparer {
		public static TinyValue NULL = new TinyValue();

		public Object value;
		public TinyValue() {
			value = new object();
		}

		public TinyValue(object obj) {
			if (obj == null)
			{
				throw new ArgumentNullException("Attempted to create an object which was null");
			}
			//All ints are floats
			if(obj is int)
			{
				value = (float)(int)obj;
				return;
			}
			if (obj is double)
			{
				double d = (double)obj;
				if (d > float.MaxValue)
				{
					value = float.MaxValue;
					return;
				}
				if (d < float.MinValue)
				{
					value = float.MinValue;
					return;
				}
				value = (float)d;
				return;
			}
			value = obj;
			if (!(value is bool || isList() || value is float || isString()))
			{
				throw new InvalidCastException("Invalid data type attempted to be instantiated: " + obj + "(" + obj.GetType() + ")");
			}
		}

		public bool asBoolean() {
			return (bool)value;
		}

		public float asFloat() {
			return (float)(value);
		}

		public List<TinyValue> asList() {
			return (List<TinyValue>)(value);
		}

		public string asString() {
			return (string)value;
		}

		public int Compare(object x, object y) {
			if (x == null || y == null)
				return 1;

			if (ReferenceEquals(x, y))
				return 0;

			TinyValue obj1 = (TinyValue)x;
			TinyValue obj2 = (TinyValue)y;

			if (obj1.value is float && obj2.value is float)
			{
				if (obj1.Equals(obj2))
					return 0;
				return this.asFloat().CompareTo(obj2.asFloat());

			}
			else if (obj1.isString() && obj2.isString())
			{
				return obj1.asString().CompareTo(obj2.asString());
			}
			throw new InvalidOperationException("Could not compare expression: '" + obj1 + "' against '" + obj2 + "'");
		}

		public override bool Equals(object comparitor) {
			if (this == NULL || comparitor == NULL)
			{
				throw new InvalidOperationException("Attempted to compare void expressions: '" + this + "' and '" + comparitor + "'");
			}
			if (this == comparitor)
			{
				return true;
			}
			if (comparitor == null || this.GetType() != comparitor.GetType())
			{
				return false;
			}
			TinyValue cmp = (TinyValue)comparitor;
			return this.value.Equals(cmp.value);
		}

		public int hashCode() {
			return value.GetHashCode();
		}

		public bool isList() {
			return value is List<TinyValue>;
		}

		public bool isNull() {
			return this == NULL;
		}

		public bool isString() {
			return value.GetType() == typeof(string);
		}

		public override string ToString() {
			return isNull() ? "NULL" : value.ToString();
		}

	}
}

#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
