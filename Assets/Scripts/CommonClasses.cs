using System;

namespace IL.Common
{
	public class GenericEventArgs<T> : EventArgs
	{
		public T Value;

	    public GenericEventArgs(T value)
	    {
	        Value = value;
	    }
    }
}
