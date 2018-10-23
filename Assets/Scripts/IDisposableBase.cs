using System;

public abstract class Disposable : IDisposable
{
	private bool disposed = false;

	//Implement IDisposable.
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected void Dispose(bool disposing)
	{
		if (!disposed)
		{
			disposed = true;
			InternalDispose();
		}
	}

	protected virtual void InternalDispose()
	{
		
	}

	// Use C# destructor syntax for finalization code.
	~Disposable()
	{
		// Simply call Dispose(false).
		Dispose(false);
	}
}
