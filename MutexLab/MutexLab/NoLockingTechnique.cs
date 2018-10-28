using System;


class NoLockingTechnique : ILockingTechnique
{
	void ILockingTechnique.Lock(Action action, params object[] lockObjects)
	{
		action.Invoke();
	}
}
