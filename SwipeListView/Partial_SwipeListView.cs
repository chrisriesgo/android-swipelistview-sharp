using System;

namespace FortySevenDeg.SwipeListView
{
	public partial class SwipeListView
	{
		WeakReference dispatcher;
		SwipeListViewListenerDispatcher EventDispatcher
		{
			get
			{
				if(dispatcher == null || !dispatcher.IsAlive)
				{
					var d = new SwipeListViewListenerDispatcher(this);
					SetSwipeListViewListener(d);
					dispatcher = new WeakReference(d);
				}
				return(SwipeListViewListenerDispatcher)dispatcher.Target;
			}
		}

		public event EventHandler<SwipeListViewSwipeModeChangedEventArgs> SwipeModeChanged
		{
			add
			{
				EventDispatcher.SwipeModeChanged += value;
			}
			remove
			{
				EventDispatcher.SwipeModeChanged -= value;
			}
		}

		public event EventHandler<SwipeListViewChoiceChangedEventArgs> ChoiceToggled
		{
			add
			{
				EventDispatcher.ChoiceToggled += value;
			}
			remove
			{
				EventDispatcher.ChoiceToggled -= value;
			}
		}

		public event EventHandler ToggleChoiceEnded
		{
			add
			{
				EventDispatcher.ToggleChoiceEnded += value;
			}
			remove
			{
				EventDispatcher.ToggleChoiceEnded -= value;
			}
		}

		public event EventHandler ToggleChoiceStarted
		{
			add
			{
				EventDispatcher.ToggleChoiceStarted += value;
			}
			remove
			{
				EventDispatcher.ToggleChoiceStarted -= value;
			}
		}

		public event EventHandler<SwipeListViewClickedEventArgs> BackViewClicked
		{
			add
			{
				EventDispatcher.BackViewClicked += value;
			}
			remove
			{
				EventDispatcher.BackViewClicked -= value;
			}
		}

		public event EventHandler<SwipeListViewClickedEventArgs> FrontViewClicked
		{
			add
			{
				EventDispatcher.FrontViewClicked += value;
			}
			remove
			{
				EventDispatcher.FrontViewClicked -= value;
			}
		}

		public event EventHandler<SwipeListViewDismissedEventArgs> Dismissed
		{
			add
			{
				EventDispatcher.Dismissed += value;
			}
			remove
			{
				EventDispatcher.Dismissed -= value;
			}
		}

		public event EventHandler FirstItemListed
		{
			add
			{
				EventDispatcher.FirstItemListed += value;
			}
			remove
			{
				EventDispatcher.FirstItemListed -= value;
			}
		}

		public event EventHandler LastItemListed
		{
			add
			{
				EventDispatcher.LastItemListed += value;
			}
			remove
			{
				EventDispatcher.LastItemListed -= value;
			}
		}

		public event EventHandler ListDataSetChanged
		{
			add
			{
				EventDispatcher.ListDataSetChanged += value;
			}
			remove
			{
				EventDispatcher.ListDataSetChanged -= value;
			}
		}

		public event EventHandler<SwipeListViewMovedEventArgs> Moved
		{
			add
			{
				EventDispatcher.Moved += value;
			}
			remove
			{
				EventDispatcher.Moved -= value;
			}
		}

		public event EventHandler<SwipeListViewClosedEventArgs> ItemClosed
		{
			add
			{
				EventDispatcher.ItemClosed += value;
			}
			remove
			{
				EventDispatcher.ItemClosed -= value;
			}
		}

		public event EventHandler<SwipeListViewOpenedEventArgs> ItemOpened
		{
			add
			{
				EventDispatcher.ItemOpened += value;
			}
			remove
			{
				EventDispatcher.ItemOpened -= value;
			}
		}

		public event EventHandler<SwipeListViewOpenStartedEventArgs> ItemOpenStarted
		{
			add
			{
				EventDispatcher.ItemOpenStarted += value;
			}
			remove
			{
				EventDispatcher.ItemOpenStarted -= value;
			}
		}

		public event EventHandler<SwipeListViewCloseStartedEventArgs> ItemCloseStarted
		{
			add
			{
				EventDispatcher.ItemCloseStarted += value;
			}
			remove
			{
				EventDispatcher.ItemCloseStarted -= value;
			}
		}
	}
}

