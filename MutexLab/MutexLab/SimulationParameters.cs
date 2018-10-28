
// The following constants control the simulation performed
// in this activity.
//
using System;

internal class SimulationParameters
{
	// Dollar amount to initialize each account with.
	public const double InitialDeposit = 1000;

	// Minimum dollar amount to transfer between accounts.
	public const int MinTransferAmount = 25;

	// Maximum dollar amount to transfer between accounts.
	public const int MaxTransferAmount = 250;

	// Number of accounts in the "bank".
	public const int NumberOfAccounts = 10;

	// Number of threads transfering funds around between accounts.
	public static readonly int NumberOfTransferThreads = Environment.ProcessorCount;

	// Amount of time (in MS) each transfer thread sleeps between transfers.
	public const int TransferDuration = 200;

	// Duration (in MS) of entire simulation.
	public const int SimulationDuration = 10000;
}
