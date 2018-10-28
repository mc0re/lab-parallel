using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks.Dataflow;


namespace TaskDataFlow
{
	internal class BatchBlockWithDbLab
	{
		#region Helper class

		// Describes an employee. Each property maps to a 
		// column in the Employees table in the Northwind database.
		// For brevity, the Employee class does not contain
		// all columns from the Employees table.
		class Employee
		{
			public int EmployeeID { get; set; }

			public string LastName { get; set; }

			public string FirstName { get; set; }


			// A random number generator that helps tp generate
			// Employee property values.
			static Random rand = new Random(42);

			// Possible random first names.
			static readonly string[] firstNames = { "Tom", "Mike", "Ruth", "Bob", "John" };
			
			// Possible random last names.
			static readonly string[] lastNames = { "Jones", "Smith", "Johnson", "Walker" };

			// Creates an Employee object that contains random 
			// property values.
			public static Employee Random()
			{
				return new Employee
				{
					EmployeeID = -1,
					LastName = lastNames[rand.Next() % lastNames.Length],
					FirstName = firstNames[rand.Next() % firstNames.Length]
				};
			}
		}

		#endregion


		#region Constants

		// The number of employees to add to the database.
		// TODO: Change this value to experiment with different numbers of 
		// employees to insert into the database.
		const int InsertCount = 256;

		// The size of a single batch of employees to add to the database.
		// TODO: Change this value to experiment with different batch sizes.
		const int InsertBatchSize = 96;

		// The source database file.
		// TODO: Change this value if Northwind.sdf is at a different location
		// on your computer.
		const string SourceDatabase =
		   @"C:\Projects\Experiments\Northwnd.mdf";

		// TODO: Change this value if you require a different temporary location.
		const string ScratchDatabase =
		   @"C:\Temp\Northwind.mdf";

		private const string ConnectString =
			@"Data Source=(localdb)\v14; AttachDbFilename={0}; Integrated Security=True; Connect Timeout=60";

		#endregion


		#region Database operations

		// Adds new employee records to the database.
		static void InsertEmployees(Employee[] employees, string connectionString)
		{
			using (var connection = new SqlConnection(connectionString))
			{
				try
				{
					// Create the SQL command.
					var command = new SqlCommand(
					   "INSERT INTO Employees ([LastName], [FirstName])" +
					   "VALUES (@lastName, @firstName)",
					   connection);

					connection.Open();
					for (int i = 0; i < employees.Length; i++)
					{
						// Set parameters.
						command.Parameters.Clear();
						command.Parameters.AddWithValue("@lastName", employees[i].LastName);
						command.Parameters.AddWithValue("@firstName", employees[i].FirstName);

						// Execute the command.
						command.ExecuteNonQuery();
					}
				}
				finally
				{
					connection.Close();
				}
			}
		}


		// Retrieves the number of entries in the Employees table in 
		// the Northwind database.
		static int GetEmployeeCount(string connectionString)
		{
			int result = 0;
			using (var sqlConnection = new SqlConnection(connectionString))
			{
				var sqlCommand = new SqlCommand(
				   "SELECT COUNT(*) FROM Employees", sqlConnection);

				sqlConnection.Open();
				try
				{
					result = (int) sqlCommand.ExecuteScalar();
				}
				finally
				{
					sqlConnection.Close();
				}
			}
			return result;
		}


		/// <summary>
		/// Retrieves the ID of the first employee that has the provided name.
		/// </summary>
		/// <exception cref="NullReferenceException">Employee not found</exception>
		static int GetEmployeeID(string lastName, string firstName, string connectionString)
		{
			using (var connection = new SqlConnection(connectionString))
			{
				var command = new SqlCommand(
				   string.Format(
					  "SELECT [EmployeeID] FROM Employees " +
					  "WHERE [LastName] = '{0}' AND [FirstName] = '{1}'",
					  lastName, firstName),
				   connection);

				connection.Open();
				try
				{
					return (int) command.ExecuteScalar();
				}
				finally
				{
					connection.Close();
				}
			}
		}

		#endregion


		#region Non-buffered operations

		// Adds random employee data to the database by using dataflow.
		static void AddEmployees(string connectionString, int count)
		{
			// Create an ActionBlock<Employee> object that adds a single
			// employee entry to the database.
			var insertEmployee = new ActionBlock<Employee>(e =>
			   InsertEmployees(new Employee[] { e }, connectionString));

			// Post several random Employee objects to the dataflow block.
			PostRandomEmployees(insertEmployee, count);

			// Set the dataflow block to the completed state and wait for 
			// all insert operations to complete.
			insertEmployee.Complete();
			insertEmployee.Completion.Wait();
		}

		#endregion


		#region Buffered operations

		// Adds random employee data to the database by using dataflow.
		// This method is similar to AddEmployees except that it uses batching
		// to add multiple employees to the database at a time.
		static void AddEmployeesBatched(string connectionString, int batchSize, int count)
		{
			// Create a BatchBlock<Employee> that holds several Employee objects and
			// then propagates them out as an array.
			var batchEmployees = new BatchBlock<Employee>(batchSize);

			// Create an ActionBlock<Employee[]> object that adds multiple
			// employee entries to the database.
			var insertEmployees = new ActionBlock<Employee[]>(a =>
			   InsertEmployees(a, connectionString));

			// Link the batch block to the action block.
			batchEmployees.LinkTo(insertEmployees, new DataflowLinkOptions { PropagateCompletion = true });

			// Post several random Employee objects to the batch block.
			PostRandomEmployees(batchEmployees, count);

			// Set the batch block to the completed state and wait for 
			// all insert operations to complete.
			batchEmployees.Complete();
			insertEmployees.Completion.Wait();
		}

		#endregion


		#region Utility

		// Posts random Employee data to the provided target block.
		static void PostRandomEmployees(ITargetBlock<Employee> target, int count)
		{
			Console.WriteLine("Adding {0} entries to Employee table...", count);

			for (int i = 0; i < count; i++)
			{
				target.Post(Employee.Random());
			}
		}

		// Displays information about several random employees to the console.
		static void GetRandomEmployees(string connectionString, int batchSize, int count)
		{
			// Create a BatchedJoinBlock<Employee, Exception> object that holds
			// both employee and exception data.
			var selectEmployees = new BatchedJoinBlock<Employee, Exception>(batchSize);

			// Holds the total number of exceptions that occurred.
			int totalErrors = 0;

			// Create an action block that prints employee and error information
			// to the console.
			var printEmployees =
				new ActionBlock<Tuple<IList<Employee>, IList<Exception>>>(data =>
				{
					// Print information about the employees in this batch.
					Console.WriteLine("Received a batch...");

					foreach (var e in data.Item1)
					{
						Console.WriteLine("Last={0} First={1} ID={2}",
							e.FirstName, e.LastName, e.EmployeeID);
					}

					// Print the error count for this batch.
					Console.WriteLine("There were {0} errors in this batch...", data.Item2.Count);

					// Update total error count.
					totalErrors += data.Item2.Count;
				});

			// Link the batched join block to the action block.
			selectEmployees.LinkTo(printEmployees, new DataflowLinkOptions { PropagateCompletion = true });

			// Try to retrieve the ID for several random employees.
			Console.WriteLine("Selecting random entries from Employees table...");

			for (int i = 0; i < count; i++)
			{
				try
				{
					// Create a random employee.
					var e = Employee.Random();

					// Try to retrieve the ID for the employee from the database.
					e.EmployeeID = GetEmployeeID(e.LastName, e.FirstName, connectionString);

					// Post the Employee object to the Employee target of 
					// the batched join block.
					selectEmployees.Target1.Post(e);
				}
				catch (NullReferenceException e)
				{
					// GetEmployeeID throws NullReferenceException when there is 
					// no such employee with the given name. When this happens,
					// post the Exception object to the Exception target of
					// the batched join block.
					selectEmployees.Target2.Post(e);
				}
			}

			// Set the batched join block to the completed state and wait for 
			// all retrieval operations to complete.
			selectEmployees.Complete();
			printEmployees.Completion.Wait();

			// Print the total error count.
			Console.WriteLine("Finished. There were {0} total errors.", totalErrors);
		}

		#endregion


		internal static void Run()
		{
			// Create a connection string for accessing the database.
			// The connection string refers to the temporary database location.
			string connectionString = string.Format(
				ConnectString, ScratchDatabase);

			// Create a Stopwatch object to time database insert operations.
			var stopwatch = new Stopwatch();

			RestoreDatabase();

			// Demonstrate multiple insert operations without batching.
			Console.WriteLine("Demonstrating non-batched database insert operations...");
			Console.WriteLine("Original size of Employee table: {0}.",
			   GetEmployeeCount(connectionString));
			stopwatch.Start();
			AddEmployees(connectionString, InsertCount);
			stopwatch.Stop();
			Console.WriteLine("New size of Employee table: {0}; elapsed insert time: {1} ms.",
			   GetEmployeeCount(connectionString), stopwatch.ElapsedMilliseconds);

			Console.WriteLine();

			// Start again with a clean database file.
			RestoreDatabase();

			// Demonstrate multiple insert operations, this time with batching.
			Console.WriteLine("Demonstrating batched database insert operations...");
			Console.WriteLine("Original size of Employee table: {0}.",
			   GetEmployeeCount(connectionString));
			stopwatch.Restart();
			AddEmployeesBatched(connectionString, InsertBatchSize, InsertCount);
			stopwatch.Stop();
			Console.WriteLine("New size of Employee table: {0}; elapsed insert time: {1} ms.",
			   GetEmployeeCount(connectionString), stopwatch.ElapsedMilliseconds);

			Console.WriteLine();

			// Start again with a clean database file.
			RestoreDatabase();

			// Demonstrate multiple retrieval operations with error reporting.
			Console.WriteLine("Demonstrating batched join database select operations...");
			// Add a small number of employees to the database.
			AddEmployeesBatched(connectionString, InsertBatchSize, 16);
			// Query for random employees.
			GetRandomEmployees(connectionString, InsertBatchSize, 10);
		}

		private static void RestoreDatabase()
		{
			// Start with a clean database file by copying the source database to 
			// the temporary location.
			var p = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "sqllocaldb",
					Arguments = "stop v14",
					ErrorDialog = true,
					WindowStyle = ProcessWindowStyle.Hidden,
					CreateNoWindow = true,
					Verb = "runas"
				}
			};
			p.Start();
			p.WaitForExit();

			var path = Path.GetDirectoryName(ScratchDatabase);
			var mask = Path.GetFileNameWithoutExtension(ScratchDatabase) + "*.*";
			Directory.EnumerateFiles(path, mask).AsParallel().ForAll(f => File.Delete(f));

			File.Copy(SourceDatabase, ScratchDatabase, true);
		}
	}
}