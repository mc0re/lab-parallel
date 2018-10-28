using System;


public interface ILockingTechnique
{
	/// <summary>
	/// Execute the given action after aquiring a lock on all objects.
	/// </summary>
	void Lock(Action action, params object[] lockObjects);
}
