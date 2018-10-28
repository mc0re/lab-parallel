using System.Globalization;
using System.Threading;

namespace MutexLab
{
	class Program
	{
		static void Main(string[] args)
		{
			Thread.CurrentThread.Name = "Main";
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(1033);

			BankAccount.Locking = new MutexLockingTechnique(); // NoLockingTechnique to show the error
			Bank.Main();
		}
	}
}
