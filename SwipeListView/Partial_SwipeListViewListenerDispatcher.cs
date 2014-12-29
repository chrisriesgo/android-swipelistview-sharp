using System;

namespace FortySevenDeg.SwipeListView
{
	internal partial class SwipeListViewListenerDispatcher : Java.Lang.Object, ISwipeListViewListener
	{
		SwipeListView sender;

		public SwipeListViewListenerDispatcher(SwipeListView sender)
		{
			this.sender = sender;
		}

		internal EventHandler<SwipeListViewSwipeModeChangedEventArgs> SwipeModeChanged;
		public int OnChangeSwipeMode(int position)
		{
			var h = SwipeModeChanged;
			var args = new SwipeListViewSwipeModeChangedEventArgs() { Position = position, SwipeMode = -1 };
			if(h != null)
				h(sender, args);

			return args.SwipeMode;
		}

		internal EventHandler<SwipeListViewChoiceChangedEventArgs> ChoiceToggled;
		public void OnChoiceChanged(int position, bool selected)
		{
			var h = ChoiceToggled;
			var args = new SwipeListViewChoiceChangedEventArgs() { Position = position, Selected = selected };
			if(h != null)
				h(sender, args);
		}

		internal EventHandler ToggleChoiceEnded;
		public void OnChoiceEnded()
		{
			var h = ToggleChoiceEnded;
			if(h != null)
				h(sender, new EventArgs());
		}

		internal EventHandler ToggleChoiceStarted;
		public void OnChoiceStarted()
		{
			var h = ToggleChoiceStarted;
			if(h != null)
				h(sender, new EventArgs());
		}

		internal EventHandler<SwipeListViewClickedEventArgs> BackViewClicked;
		public void OnClickBackView(int position)
		{
			var h = BackViewClicked;
			var args = new SwipeListViewClickedEventArgs() { Position = position };
			if(h != null)
				h(sender, args);
		}

		internal EventHandler<SwipeListViewClickedEventArgs> FrontViewClicked;
		public void OnClickFrontView(int position)
		{
			var h = FrontViewClicked;
			var args = new SwipeListViewClickedEventArgs() { Position = position };
			if(h != null)
				h(sender, args);
		}

		internal EventHandler<SwipeListViewDismissedEventArgs> Dismissed;
		public void OnDismiss(int[] reverseSortedPositions)
		{
			var h = Dismissed;
			var args = new SwipeListViewDismissedEventArgs() { ReverseSortedPositions = reverseSortedPositions };
			if(h != null)
				h(sender, args);
		}

		internal EventHandler FirstItemListed;
		public void OnFirstListItem()
		{
			var h = FirstItemListed;
			if(h != null)
				h(sender, new EventArgs());
		}

		internal EventHandler LastItemListed;
		public void OnLastListItem()
		{
			var h = LastItemListed;
			if(h != null)
				h(sender, new EventArgs());
		}

		internal EventHandler ListDataSetChanged;
		public void OnListChanged()
		{
			var h = ListDataSetChanged;
			if(h != null)
				h(sender, new EventArgs());
		}

		internal EventHandler<SwipeListViewMovedEventArgs> Moved;
		public void OnMove(int position, float x)
		{
			var h = Moved;
			var args = new SwipeListViewMovedEventArgs() { Position = position, X = x };
			if(h != null)
				h(sender, args);
		}

		internal EventHandler<SwipeListViewClosedEventArgs> ItemClosed;
		public void OnClosed(int position, bool fromRight)
		{
			var h = ItemClosed;
			var args = new SwipeListViewClosedEventArgs() { Position = position, FromRight = fromRight };
			if(h != null)
				h(sender, args);
		}

		internal EventHandler<SwipeListViewOpenedEventArgs> ItemOpened;
		public void OnOpened(int position, bool toRight)
		{
			var h = ItemOpened;
			var args = new SwipeListViewOpenedEventArgs() { Position = position, ToRight = toRight };
			if(h != null)
				h(sender, args);
		}

		internal EventHandler<SwipeListViewOpenStartedEventArgs> ItemOpenStarted;
		public void OnStartOpen(int position, int action, bool right)
		{
			var h = ItemOpenStarted;
			var args = new SwipeListViewOpenStartedEventArgs() { Position = position, Right = right };
			if(h != null)
				h(sender, args);
		}

		internal EventHandler<SwipeListViewCloseStartedEventArgs> ItemCloseStarted;
		public void OnStartClose(int position, bool right)
		{
			var h = ItemCloseStarted;
			var args = new SwipeListViewCloseStartedEventArgs() { Position = position, Right = right };
			if(h != null)
				h(sender, args);
		}
	}

	public class SwipeListViewSwipeModeChangedEventArgs : EventArgs
	{
		public int Position { get; internal set; }
		public int SwipeMode { get; set; }
	}

	public class SwipeListViewChoiceChangedEventArgs : EventArgs
	{
		public int Position { get; internal set; }
		public bool Selected { get; internal set; }
	}

	public class SwipeListViewClickedEventArgs : EventArgs
	{
		public int Position { get; internal set; }
	}

	public class SwipeListViewDismissedEventArgs : EventArgs
	{
		public int[] ReverseSortedPositions { get; internal set; }
	}

	public class SwipeListViewMovedEventArgs : EventArgs
	{
		public int Position { get; internal set; }
		public float X { get; internal set; }
	}

	public class SwipeListViewClosedEventArgs : EventArgs
	{
		public int Position { get; internal set; }
		public bool FromRight { get; internal set; }
	}

	public class SwipeListViewOpenedEventArgs : EventArgs
	{
		public int Position { get; internal set; }
		public bool ToRight { get; internal set; }
	}

	public class SwipeListViewCloseStartedEventArgs : EventArgs
	{
		public int Position { get; internal set; }
		public bool Right { get; internal set; }
	}

	public class SwipeListViewOpenStartedEventArgs : EventArgs
	{
		public int Position { get; internal set; }
		public bool Right { get; internal set; }
	}
}