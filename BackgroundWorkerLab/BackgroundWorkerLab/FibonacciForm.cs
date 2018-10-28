using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BackgroundWorkerLab
{
	public partial class FibonacciForm : Form
	{
		private int numberToCompute = 0;
		private int highestPercentageReached = 0;


		public FibonacciForm()
		{
			InitializeComponent();
			InitializeBackgroundWorker();
		}


		// Set up the BackgroundWorker object by 
		// attaching event handlers. 
		private void InitializeBackgroundWorker()
		{
			backgroundWorker1.DoWork += new DoWorkEventHandler(Bg_DoWork);
			backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Bg_RunWorkerCompleted);
			backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(Bg_ProgressChanged);
		}


		#region Button handlers

		private void StartAsyncButton_Click(object sender, EventArgs args)
		{
			// Reset the text in the result label.
			resultLabel.Text = string.Empty;

			// Disable the UpDown control until 
			// the asynchronous operation is done.
			this.numericUpDown1.Enabled = false;

			// Disable the Start button until 
			// the asynchronous operation is done.
			this.startAsyncButton.Enabled = false;

			// Enable the Cancel button while 
			// the asynchronous operation runs.
			this.cancelAsyncButton.Enabled = true;

			// Get the value from the UpDown control.
			numberToCompute = (int) numericUpDown1.Value;

			// Reset the variable for percentage tracking.
			highestPercentageReached = 0;

			// Start the asynchronous operation.
			backgroundWorker1.RunWorkerAsync(numberToCompute);
		}


		private void CancelAsyncButton_Click(object sender, EventArgs args)
		{
			// Cancel the asynchronous operation.
			this.backgroundWorker1.CancelAsync();

			// Disable the Cancel button.
			cancelAsyncButton.Enabled = false;
		}

		#endregion


		#region BackgroundWorker handlers

		// This event handler is where the actual,
		// potentially time-consuming work is done.
		private void Bg_DoWork(object sender, DoWorkEventArgs args)
		{
			// Get the BackgroundWorker that raised this event.
			var worker = sender as BackgroundWorker;

			// Assign the result of the computation
			// to the Result property of the DoWorkEventArgs
			// object. This is will be available to the 
			// RunWorkerCompleted eventhandler.
			args.Result = ComputeFibonacci((int) args.Argument, worker, args);
		}


		// This event handler deals with the results of the
		// background operation.
		private void Bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs args)
		{
			// First, handle the case where an exception was thrown.
			if (args.Error != null)
			{
				MessageBox.Show(args.Error.Message, "BackgroundWorker lab");
			}
			else if (args.Cancelled)
			{
				// Next, handle the case where the user canceled 
				// the operation.
				// Note that due to a race condition in 
				// the DoWork event handler, the Cancelled
				// flag may not have been set, even though
				// CancelAsync was called.
				resultLabel.Text = "Canceled";
			}
			else
			{
				// Finally, handle the case where the operation 
				// succeeded.
				resultLabel.Text = args.Result.ToString();
			}

			// Enable the UpDown control.
			this.numericUpDown1.Enabled = true;

			// Enable the Start button.
			startAsyncButton.Enabled = true;

			// Disable the Cancel button.
			cancelAsyncButton.Enabled = false;
		}


		// This event handler updates the progress bar.
		private void Bg_ProgressChanged(object sender, ProgressChangedEventArgs args)
		{
			this.progressBar1.Value = args.ProgressPercentage;
		}

		#endregion


		#region Calculation

		// This is the method that does the actual work. For this
		// example, it computes a Fibonacci number and
		// reports progress as it does its work.
		long ComputeFibonacci(int n, BackgroundWorker worker, DoWorkEventArgs args)
		{
			// The parameter n must be >= 0 and <= 91.
			// Fib(n), with n > 91, overflows a long.
			if ((n < 0) || (n > 91))
			{
				throw new ArgumentException("value must be >= 0 and <= 91", "n");
			}

			long result = 0;

			// Abort the operation if the user has canceled.
			// Note that a call to CancelAsync may have set 
			// CancellationPending to true just after the
			// last invocation of this method exits, so this 
			// code will not have the opportunity to set the 
			// DoWorkEventArgs.Cancel flag to true. This means
			// that RunWorkerCompletedEventArgs.Cancelled will
			// not be set to true in your RunWorkerCompleted
			// event handler. This is a race condition.

			if (worker.CancellationPending)
			{
				args.Cancel = true;
			}
			else
			{
				if (n < 2)
				{
					result = 1;
				}
				else
				{
					result = ComputeFibonacci(n - 1, worker, args) +
							 ComputeFibonacci(n - 2, worker, args);
				}

				// Report progress as a percentage of the total task.
				int percentComplete = (int) ((float) n / numberToCompute * 100);
				if (percentComplete > highestPercentageReached)
				{
					highestPercentageReached = percentComplete;
					worker.ReportProgress(percentComplete);
				}
			}

			return result;
		}

		#endregion
	}
}
