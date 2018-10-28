using System;
using System.Threading;

public class BankAccount
{
	public readonly int AccountNumber;

	double mBalance;

	Mutex mLock = new Mutex();

	public static ILockingTechnique Locking;


	public BankAccount(int acctNum, double initDeposit)
	{
		AccountNumber = acctNum;
		mBalance = initDeposit;
	}


	public void Credit(double amt)
	{
		Locking.Lock(() =>
			{
				double temp = mBalance;
				temp += amt;
				Thread.Sleep(1);
				mBalance = temp;
			},
			mLock
		);
	}


	public void Debit(double amt)
	{
		Credit(-amt);
	}


	public double Balance
	{
		get
		{
			double b = 0;

			Locking.Lock(() => b = mBalance, mLock);

			return (b);
		}
	}


	public void TransferFrom(BankAccount otherAcct, double amt)
	{
		Console.WriteLine("[{0}] Transfering {2} -> {3}: {1:C0}",
						  Thread.CurrentThread.Name, amt,
						  otherAcct.AccountNumber, this.AccountNumber);

		Locking.Lock(() =>
			{
				otherAcct.Debit(amt);
				this.Credit(amt);
			},
		this.mLock, otherAcct.mLock);
	}
}
