using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Forms;


namespace ChangeTaskScheduler
{
	public partial class Form1 : Form
	{
		#region Fields

		// Broadcasts values to an ActionBlock<int> object that is associated
		// with each check box.
		BroadcastBlock<int> mBroadcaster = new BroadcastBlock<int>(null);

		#endregion

		public Form1()
		{
			InitializeComponent();

			// Create an ActionBlock<CheckBox> object that toggles the state
			// of CheckBox objects.
			// Specifying the current synchronization context enables the 
			// action to run on the user-interface thread.
			var toggleCheckBox = new ActionBlock<CheckBox>(checkBox =>
			{
				checkBox.Checked = !checkBox.Checked;
			},
			new ExecutionDataflowBlockOptions
			{
				TaskScheduler = TaskScheduler.FromCurrentSynchronizationContext()
			});

			// Create a ConcurrentExclusiveSchedulerPair object.
			// Readers will run on the concurrent part of the scheduler pair.
			// The writer will run on the exclusive part of the scheduler pair.
			var taskSchedulerPair = new ConcurrentExclusiveSchedulerPair();

			// Create an ActionBlock<int> object for each reader CheckBox object.
			// Each ActionBlock<int> object represents an action that can read 
			// from a resource in parallel to other readers.
			// Specifying the concurrent part of the scheduler pair enables the 
			// reader to run in parallel to other actions that are managed by 
			// that scheduler.
			var readerActions =
				from checkBox in new[] { checkBox1, checkBox2, checkBox3 }
				select new ActionBlock<int>(milliseconds =>
				{
				   // Toggle the check box to the checked state.
				   toggleCheckBox.Post(checkBox);

				   // Perform the read action. For demonstration, suspend the current
				   // thread to simulate a lengthy read operation.
				   Thread.Sleep(milliseconds);

				   // Toggle the check box to the unchecked state.
				   toggleCheckBox.Post(checkBox);
				},
				new ExecutionDataflowBlockOptions
				{
					TaskScheduler = taskSchedulerPair.ConcurrentScheduler
				});

			// Create an ActionBlock<int> object for the writer CheckBox object.
			// This ActionBlock<int> object represents an action that writes to 
			// a resource, but cannot run in parallel to readers.
			// Specifying the exclusive part of the scheduler pair enables the 
			// writer to run in exclusively with respect to other actions that are 
			// managed by the scheduler pair.
			var writerAction = new ActionBlock<int>(milliseconds =>
			{
				// Toggle the check box to the checked state.
				toggleCheckBox.Post(checkBox4);

				// Perform the write action. For demonstration, suspend the current
				// thread to simulate a lengthy write operation.
				Thread.Sleep(milliseconds);

				// Toggle the check box to the unchecked state.
				toggleCheckBox.Post(checkBox4);
			},
			new ExecutionDataflowBlockOptions
			{
				TaskScheduler = taskSchedulerPair.ExclusiveScheduler
			});

			// Link the broadcaster to each reader and writer block.
			// The BroadcastBlock<T> class propagates values that it 
			// receives to all connected targets.
			foreach (var readerAction in readerActions)
			{
				mBroadcaster.LinkTo(readerAction);
			}
			mBroadcaster.LinkTo(writerAction);

			// Start the timer.
			timer1.Start();
		}


		// Event handler for the timer.
		private void Timer1_Tick(object sender, EventArgs e)
		{
			// Post a value to the broadcaster. The broadcaster
			// sends this message to each target. 
			mBroadcaster.Post(1000);
		}
	}
}
