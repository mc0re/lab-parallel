using System;
using System.Linq;
using System.Threading;


public class MutexLockingTechnique : ILockingTechnique
{
	void ILockingTechnique.Lock(Action action, params object[] lockObjects)
	{
		if (lockObjects.Length == 1)
		{
			var mutex = (Mutex) lockObjects[0];
			if (mutex.WaitOne())
			{
				try
				{
					action.Invoke();
				}
				finally
				{
					mutex.ReleaseMutex();
				}
			}
		}
		else
		{
			var locks = lockObjects.Cast<Mutex>().ToArray();

			if (WaitHandle.WaitAll(locks))
			{
				try
				{
					action.Invoke();
				}
				finally
				{
					foreach (var mutex in locks)
					{
						mutex.ReleaseMutex();
					}
				}
			}
		}
	}
}
