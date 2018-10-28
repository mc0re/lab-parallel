using System;
using System.Threading;
using System.Windows;


namespace SyncContext
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window,
		IEventListener<ItemStartedEvent>, IEventListener<ItemCompletedEvent>
    {
		#region Fields

		private Random mRandom = new Random();

		private EventCentral mEventCentral = new EventCentral();

		private TaskRunner mTaskRunner;

		private Thread mThread;

		#endregion


		#region QueuedList dependency property

		private static readonly DependencyPropertyKey QueuedListPropertyKey = DependencyProperty.RegisterReadOnly(
			nameof(QueuedList), typeof(TaskQueue), typeof(MainWindow),
			new PropertyMetadata(new TaskQueue()));


		public static readonly DependencyProperty QueuedListProperty = QueuedListPropertyKey.DependencyProperty;


		public TaskQueue QueuedList
		{
			get => (TaskQueue)GetValue(QueuedListProperty);
		}

		#endregion


		#region RunningList dependency property

		private static readonly DependencyPropertyKey RunningListPropertyKey = DependencyProperty.RegisterReadOnly(
			nameof(RunningList), typeof(TaskList), typeof(MainWindow),
			new PropertyMetadata(new TaskList()));


		public static readonly DependencyProperty RunningListProperty = RunningListPropertyKey.DependencyProperty;


		public TaskList RunningList
		{
			get => (TaskList) GetValue(RunningListProperty);
		}

		#endregion


		#region CompletedList dependency property

		private static readonly DependencyPropertyKey CompletedListPropertyKey = DependencyProperty.RegisterReadOnly(
			nameof(CompletedList), typeof(TaskList), typeof(MainWindow),
			new PropertyMetadata(new TaskList()));


		public static readonly DependencyProperty CompletedListProperty = CompletedListPropertyKey.DependencyProperty;


		public TaskList CompletedList
		{
			get => (TaskList) GetValue(CompletedListProperty);
		}

		#endregion


		#region Init and clean-up

		public MainWindow()
        {
            InitializeComponent();

			mEventCentral.AddListener(this);
			mEventCentral.AddQueuePublisher(QueuedList);

			mTaskRunner = new TaskRunner(mEventCentral);
			mThread = new Thread(new ThreadStart(() => mTaskRunner.Run()));
			mThread.Start();
		}


		private void OnClose(object sender, EventArgs args)
		{
			mThread.Abort();
		}

		#endregion


		#region Event handlers

		private void AddButton_Click(object sender, RoutedEventArgs e)
		{
			QueuedList.Add("Task " + mRandom.Next(1000));
		}


		void IEventListener<ItemStartedEvent>.OnEvent(ItemStartedEvent ev)
		{
			RunningList.Add(ev.Item);
		}

		void IEventListener<ItemCompletedEvent>.OnEvent(ItemCompletedEvent ev)
		{
			RunningList.Remove(ev.Item);
			CompletedList.Add(ev.Item);
		}

		#endregion
	}
}
