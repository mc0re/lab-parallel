using System;
using System.Threading;


class Bank
{
	static readonly BankAccount[] sBankAccounts = new BankAccount[SimulationParameters.NumberOfAccounts];

	static volatile bool sSimulationOver = false;

	static Random sRandomGen = new Random((int) DateTime.Now.Ticks);


	public static void Main()
	{
		Console.WriteLine("[{0}] Starting program.  Total funds on deposit should always be {1:C0}",
						  Thread.CurrentThread.Name,
						  SimulationParameters.NumberOfAccounts * SimulationParameters.InitialDeposit);

		// An array of references to Account objects (bankAccounts) is
		// initialized, with each account starting out with the
		// same amount of money (INITIAL_DEPOSIT).
		//
		for (int n = 0; n < SimulationParameters.NumberOfAccounts; n++)
		{
			sBankAccounts[n] = new BankAccount(n, SimulationParameters.InitialDeposit);
		}

		// The array below will hold references to the threads
		// that are doing the funds transfers.
		//
		Thread[] transferThreads = new Thread[SimulationParameters.NumberOfTransferThreads];
		ThreadStart threadProc = new ThreadStart(TransferThreadProc);

		// Start the transfer threads.
		//
		for (int n = 0; n < SimulationParameters.NumberOfTransferThreads; n++)
		{
			transferThreads[n] = new Thread(threadProc)
			{
				Name = string.Format("TX-{0}", n)
			};
			transferThreads[n].Start();
		}

		// Let the simulation run the prescribed amount of time.
		//
		Thread.Sleep(SimulationParameters.SimulationDuration);

		// Signal to everyone that the simulation is over.
		//
		Console.WriteLine("[Main] Shutting down simulation.");
		sSimulationOver = true;

		// Wait for everyone to acknowledge the simulation is complete.
		//
		for (int n = 0; n < transferThreads.Length; n++)
		{
			transferThreads[n].Join();
		}

		// Perform consistency check.
		//
		Console.WriteLine("[Main] Simulation complete, verifying accounts.");
		VerifyAccounts();
	}


	// TransferThreadProc
	//
	// This method represents the code for the threads that
	// perform transfers from one account to another throughout
	// the life of the simulation.
	//
	static void TransferThreadProc()
	{
		string threadName = Thread.CurrentThread.Name;

		while (!sSimulationOver)
		{
			// Choose a random transfer amount.
			//
			double transferAmount = GetRandomTranferAmount();

			// Randomly choose two accounts to transfer funds between.
			//
			int debitAccount = GetRandomAccountIndex();
			int creditAccount = GetRandomAccountIndex();

			// Make sure we're actually transferring money
			// between different accounts.
			//
			while (creditAccount == debitAccount)
			{
				creditAccount = GetRandomAccountIndex();
			}

			// Transfer funds between the two chosen accounts.
			//
			sBankAccounts[creditAccount].TransferFrom(sBankAccounts[debitAccount], transferAmount);

			Thread.Sleep(SimulationParameters.TransferDuration);
		}
	}


	// VerifyAccounts
	//
	// This method verifies that the total amount of money "on deposit"
	// in our simulation is consistent.  In other words, we should find
	// that the total funds on deposit at the end of the simulation
	// is the same as it was when the program started (because we're
	// just shuffling funds between accounts in the same bank).
	// If we detect otherwise, we have a problem.
	//
	static void VerifyAccounts()
	{
		string threadName = Thread.CurrentThread.Name;
		double totalDepositsIfNoErrors = SimulationParameters.InitialDeposit * SimulationParameters.NumberOfAccounts;
		double totalDeposits = 0;

		// Iterate over accounts, adding each account's balance
		// to a running total.
		//
		for (int n = 0; n < SimulationParameters.NumberOfAccounts; n++)
		{
			totalDeposits += sBankAccounts[n].Balance;
		}

		// Display the results of this audit.
		//
		if (totalDeposits == totalDepositsIfNoErrors)
		{
			Console.WriteLine("[{0}] Audit result: bank accounts are consistent ({1:C0} on deposit)",
							  threadName, totalDeposits);
		}
		else
		{
			Console.WriteLine("[{0}] Audit result: *** inconsistencies detected ({1:C0} total deposits)",
							  threadName, totalDeposits);
		}
	}


	// GetRandomAccountIndex
	//
	// Returns a random number between 0 and (NUMBER_OF_ACCOUNTS - 1).  The returned
	// value will be used as an index into bankAccounts to determine which
	// account to modify.
	//
	static int GetRandomAccountIndex()
	{
		lock (sRandomGen)
		{
			return sRandomGen.Next(SimulationParameters.NumberOfAccounts - 1);
		}
	}


	// GetRandomTranferAmount
	//
	// Returns a random number representing between MIN_TRANSFER_AMOUNT and
	// MAX_TRANSFER_AMOUNT representing the amount of money to transfer
	// between any two accounts.
	//
	static double GetRandomTranferAmount()
	{
		lock (sRandomGen)
		{
			return sRandomGen.Next(SimulationParameters.MinTransferAmount,
									SimulationParameters.MaxTransferAmount);
		}
	}
}
