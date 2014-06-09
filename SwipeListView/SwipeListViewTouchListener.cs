/*
 * Copyright (C) 2013 47 Degrees, LLC
 *  http://47deg.com
 *  hello@47deg.com
 *
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;

using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V4.View;
using Android.Animation;
using System.Threading.Tasks;

namespace FortySevenDeg.SwipeListView
{
	public class SwipeListViewTouchListener : Java.Lang.Object, View.IOnTouchListener, AbsListView.IOnScrollListener
	{
		private int _swipeFrontView = 0;
		private int swipeBackView = 0;
		private int _swipeRevealDismissView = 0;

		private Rect rect = new Rect();

		// Cached ViewConfiguration and system-wide constant values
		private int slop;
		private int minFlingVelocity;
		private int maxFlingVelocity;
		private long _configShortAnimationTime;
		private long _animationTime;

		// Fixed properties
		private SwipeListView _swipeListView;
		private int viewWidth = 1; // 1 and not 0 to prevent dividing by zero

		private List<PendingDismissData> pendingDismisses = new List<PendingDismissData>();
		private int dismissAnimationRefCount = 0;

		private float downX;
		private bool swiping;
		private bool swipingRight;
		private VelocityTracker velocityTracker;
		private int downPosition;
		private View _frontView;
		private View _backView;
		private bool paused;

		private int swipeCurrentAction = (int)SwipeListView.SwipeAction.None;

		private Dictionary<int, bool> _opened = new Dictionary<int, bool>();
		private Dictionary<int, bool> _openedRight = new Dictionary<int, bool>();
		private Dictionary<int, bool> _checked = new Dictionary<int, bool>();
		private int oldSwipeActionRight;
		private int oldSwipeActionLeft;

		private bool _isFirstItem = false;
		private bool _isLastItem = false;

		public SwipeListViewTouchListener(SwipeListView swipeListView, int swipeFrontView, int swipeBackView, int swipeRevealDismissView) {
			this._swipeFrontView = swipeFrontView;
			this.swipeBackView = swipeBackView;
			_swipeRevealDismissView = swipeRevealDismissView;
			ViewConfiguration vc = ViewConfiguration.Get(swipeListView.Context);
			slop = vc.ScaledTouchSlop;
			SwipeClosesAllItemsWhenListMoves = true;
			minFlingVelocity = vc.ScaledMinimumFlingVelocity;
			maxFlingVelocity = vc.ScaledMaximumFlingVelocity;
			_configShortAnimationTime = swipeListView.Context.Resources.GetInteger(Android.Resource.Integer.ConfigShortAnimTime);
			_animationTime = _configShortAnimationTime;
			_swipeListView = swipeListView;
			SwipeOpenOnLongPress = true;
			SwipeMode = (int)SwipeListView.SwipeMode.Both;
			SwipeActionLeft = (int)SwipeListView.SwipeAction.Reveal;
			SwipeActionRight = (int)SwipeListView.SwipeAction.Reveal;
			LeftOffset = 0;
			RightOffset = 0;
			ChoiceOffset = 0;
			RevealDismissThreshold = 0;
			SwipeDrawableChecked = 0;
			SwipeDrawableUnchecked = 0;
		}

		#region "Properties"
		public View ParentView { get; set; }
		public View FrontView 
		{
			get
			{
				return _frontView;
			}
			set
			{
				_frontView = value;
				_frontView.Click += (sender, e) => _swipeListView.OnClickFrontView(downPosition);
				if(SwipeOpenOnLongPress)
				{
					_frontView.LongClick += (sender, e) => OpenAnimate(_frontView, downPosition);
				}
			}
		}
			
		private View BackView 
		{
			get { return this._backView; }
			set {
				this._backView = value;
				this._backView.Click += (sender, e) => _swipeListView.OnClickBackView(downPosition);
			}
		}

		public View RevealDismissView { get; set; }

		public bool IsListViewMoving { get; set; }

		/// Sets animation time when the user drops the cell
		public long AnimationTime 
		{
			get { return _animationTime; }
			set {
				if(value > 0)
				{
					_animationTime = value;
				}
				else
				{
					_animationTime = _configShortAnimationTime;
				}
			}
		}

		public float RightOffset { get; set; }
		public float LeftOffset { get; set; }
		public float ChoiceOffset { get; set; }
		public float RevealDismissThreshold { get; set; }

		/// Set if all items opened will be closed when the user moves ListView
		public bool SwipeClosesAllItemsWhenListMoves { get; set; }

		/// Set if the user can open an item with long press on cell
		public bool SwipeOpenOnLongPress { get; set ;}

		public int SwipeMode { get; set; }

		public bool IsSwipeEnabled {
			get {
				return SwipeMode != (int)SwipeListView.SwipeMode.None;
			}
		}
			
		public int SwipeActionLeft { get; set; }
		public int SwipeActionRight { get; set; }
		public int SwipeDrawableChecked { get; set; }

		/// Set drawable unchecked (only SWIPE_ACTION_CHOICE)
		public int SwipeDrawableUnchecked { get; set; }

		public int CountSelected {
			get
			{
				int count = 0;
				for (int i = 0; i < _checked.Count; i++) {
					if (IsChecked(i)) {
						count++;
					}
				}
				Android.Util.Log.Debug("SwipeListView", "selected: " + count);
				return count;
			}
		}

		/// <summary>
		/// Gets the positions selected.
		/// </summary>
		/// <returns>The positions selected.</returns>
		public List<int> PositionsSelected {
			get
			{
				List<int> list = new List<int>();
				for(int i = 0; i < _checked.Count; i++)
				{
					if(IsChecked(i))
					{
						list.Add(i);
					}
				}
				return list;
			}
		}

		public bool SwipeAllowFling { get; set; }
		#endregion 

		public bool OpenedRight(int position)
		{
			return _openedRight.ContainsKey(position) && _openedRight.FirstOrDefault(o => o.Key == position).Value;
		}

		public bool Opened(int position)
		{
			return _opened.ContainsKey(position) && _opened.FirstOrDefault(o => o.Key == position).Value;
		}



		public void ResetItems() {
			if (_swipeListView.Adapter != null) {
				int count = _swipeListView.Adapter.Count;
				for (int i = _opened.Count; i <= count; i++) {
					_opened[i] = false;
					_openedRight[i] = false;
					_checked[i] = false;
				}
			}
		}

		/// <summary>
		/// Slide open item
		/// </summary>
		/// <param name="position">Position.</param>
		public void OpenAnimate(int position) {
			OpenAnimate(_swipeListView.GetChildAt(position - _swipeListView.FirstVisiblePosition).FindViewById(_swipeFrontView), position);
		}

		public void CloseAnimate(int position) {
			CloseAnimate(_swipeListView.GetChildAt(position - _swipeListView.FirstVisiblePosition).FindViewById(_swipeFrontView), position);
		}

		/// <summary>
		/// Swaps the state of the choice.
		/// </summary>
		/// <param name="position">Position.</param>
		private void SwapChoiceState(int position) {
			int lastCount = CountSelected;
			bool lastChecked = IsChecked(position);
			_checked[position] = !lastChecked;
			int count = lastChecked ? lastCount - 1 : lastCount + 1;
			if (lastCount == 0 && count == 1) {
				_swipeListView.OnChoiceStarted();
				CloseOpenedItems();
				SetActionsTo((int)SwipeListView.SwipeAction.Choice);
			}
			if (lastCount == 1 && count == 0) {
				_swipeListView.OnChoiceEnded();
				ReturnOldActions();
			}
			if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Honeycomb) {
				_swipeListView.SetItemChecked(position, !lastChecked);
			}
			_swipeListView.OnChoiceChanged(position, !lastChecked);
			ReloadChoiceStateInView(FrontView, position);
		}

		/// <summary>
		/// Unselecteds the choice states.
		/// </summary>
		public void UnselectedChoiceStates() {
			int start = _swipeListView.FirstVisiblePosition;
			int end = _swipeListView.LastVisiblePosition;
			for (int i = 0; i < _checked.Count; i++) {
				if (IsChecked(i) && i >= start && i <= end) {
					ReloadChoiceStateInView(_swipeListView.GetChildAt(i - start).FindViewById(_swipeFrontView), i);
				}
				_checked[i] = false;
			}
			_swipeListView.OnChoiceEnded();
			ReturnOldActions();
		}

		/// <summary>
		/// Dismiss the specified position.
		/// </summary>
		/// <param name="position">Position.</param>
		protected int Dismiss(int position) {
			int start = _swipeListView.FirstVisiblePosition;
			int end = _swipeListView.LastVisiblePosition;
			View view = _swipeListView.GetChildAt(position - start);
			++dismissAnimationRefCount;
			if (position >= start && position <= end) {
				PerformDismiss(view, position, false);
				return view.Height;
			} else {
				pendingDismisses.Add(new PendingDismissData(position, null));
				return 0;
			}
		}

		/// <summary>
		/// Reloads the choice state in view.
		/// </summary>
		/// <param name="frontView">Front view.</param>
		/// <param name="position">Position.</param>
		public void ReloadChoiceStateInView(View frontView, int position) {
			if (IsChecked(position)) {
				if (SwipeDrawableChecked > 0) frontView.SetBackgroundResource(SwipeDrawableChecked);
			} else {
				if (SwipeDrawableUnchecked > 0) frontView.SetBackgroundResource(SwipeDrawableUnchecked);
			}
		}

		/// <summary>
		/// Reloads the swipe state in view.
		/// </summary>
		/// <param name="frontView">Front view.</param>
		public void ReloadSwipeStateInView(View frontView) {
			if(SwipeClosesAllItemsWhenListMoves){
				frontView.TranslationX = 0f;
			}
		}

		/// <summary>
		/// Determines whether this instance is checked the specified position.
		/// </summary>
		/// <returns><c>true</c> if this instance is checked the specified position; otherwise, <c>false</c>.</returns>
		/// <param name="position">Position.</param>
		public bool IsChecked(int position) {
			return _checked.ContainsKey(position) && _checked.FirstOrDefault(o => o.Key == position).Value;
		}

		private void OpenAnimate(View view, int position) {
			if (!Opened(position)) {
				GenerateRevealAnimate(view, true, false, position);
			}
		}

		/// <summary>
		/// Closes the animate.
		/// </summary>
		/// <param name="view">View.</param>
		/// <param name="position">Position.</param>
		private void CloseAnimate(View view, int position) {
			if (Opened(position)) {
				GenerateRevealAnimate(view, true, false, position);
			}
		}

		/// <summary>
		/// Generates the animate.
		/// </summary>
		/// <param name="view">View.</param>
		/// <param name="swap">If set to <c>true</c> swap.</param>
		/// <param name="swapRight">If set to <c>true</c> swap right.</param>
		/// <param name="position">Position.</param>
		private void GenerateAnimate(View view, bool swap, bool swapRight, int position) {
			Android.Util.Log.Debug("SwipeListView", "swap: " + swap + " - swapRight: " + swapRight + " - position: " + position);
			if (swipeCurrentAction == (int)SwipeListView.SwipeAction.Reveal) {
				GenerateRevealAnimate(view, swap, swapRight, position);
			}
			if (swipeCurrentAction == (int)SwipeListView.SwipeAction.Dismiss) {
				GenerateDismissAnimate(ParentView, swap, swapRight, position);
			}
			if (swipeCurrentAction == (int)SwipeListView.SwipeAction.Choice) {
				GenerateChoiceAnimate(view, position);
			}
			if (swipeCurrentAction == (int)SwipeListView.SwipeAction.RevealDismiss) {
				GenerateRevealDismissAnimate(view, swap, swapRight, position);
			}
		}

		/// <summary>
		/// Generates the choice animate.
		/// </summary>
		/// <param name="view">View.</param>
		/// <param name="position">Position.</param>
		private void GenerateChoiceAnimate(View view, int position) 
		{
			var listener = new ObjectAnimatorListenerAdapter();
			listener.AnimationEnd = (animation) =>
			{
				_swipeListView.ResetScrolling();
				ResetCell();
			};

			view.Animate()
				.TranslationX(0)
				.SetDuration(_animationTime)
				.SetListener(listener);
		}

		/// <summary>
		/// Generates the dismiss animate.
		/// </summary>
		/// <param name="view">View.</param>
		/// <param name="swap">If set to <c>true</c> swap.</param>
		/// <param name="swapRight">If set to <c>true</c> swap right.</param>
		/// <param name="position">Position.</param>
		private void GenerateDismissAnimate(View view, bool swap, bool swapRight, int position) {
			int moveTo = 0;
			if (Opened(position)) {
				if (!swap) {
					moveTo = OpenedRight(position) ? (int) (viewWidth - RightOffset) : (int) (-viewWidth + LeftOffset);
				}
			} else {
				if (swap) {
					moveTo = swapRight ? (int) (viewWidth - RightOffset) : (int) (-viewWidth + LeftOffset);
				}
			}

			int alpha = 1;
			if (swap) {
				++dismissAnimationRefCount;
				alpha = 0;
			}

			var listener = new ObjectAnimatorListenerAdapter();
			listener.AnimationEnd = (animation) =>
			{
				if(swap) {
					CloseOpenedItems();
					PerformDismiss(view, position, true);
				}
				ResetCell();
			};

			view.Animate()
				.TranslationX(moveTo)
				.Alpha(alpha)
				.SetDuration(_animationTime)
				.SetListener(listener);
		}

		/// <summary>
		/// Generates the reveal animate.
		/// </summary>
		/// <param name="view">View.</param>
		/// <param name="swap">If set to <c>true</c> swap.</param>
		/// <param name="swapRight">If set to <c>true</c> swap right.</param>
		/// <param name="position">Position.</param>
		private void GenerateRevealAnimate(View view, bool swap, bool swapRight, int position) {
			int moveTo = 0;
			if (Opened(position)) {
				if (!swap) {
					moveTo = OpenedRight(position) ? (int) (viewWidth - RightOffset) : (int) (-viewWidth + LeftOffset);
				}
			} else {
				if (swap) {
					moveTo = swapRight ? (int) (viewWidth - RightOffset) : (int) (-viewWidth + LeftOffset);
				}
			}

			var listener = new ObjectAnimatorListenerAdapter();
			listener.AnimationEnd = (animation) =>
			{
				_swipeListView.ResetScrolling();
				if (swap) {
					var aux = !Opened(position);
					_opened[position] = aux;
					if (aux) {
						_swipeListView.OnOpened(position, swapRight);
						_openedRight[position] = swapRight;
					} else {
						_swipeListView.OnClosed(position, OpenedRight(position));
					}
				}
				ResetCell();
			};

			view.Animate()
				.TranslationX(moveTo)
				.SetDuration(_animationTime)
				.SetListener(listener);
		}

		/// <summary>
		/// Generates the reveal + dismiss animation.
		/// </summary>
		/// <param name="view">View.</param>
		/// <param name="swap">If set to <c>true</c> swap.</param>
		/// <param name="swapRight">If set to <c>true</c> swap right.</param>
		/// <param name="position">Position.</param>
		private void GenerateRevealDismissAnimate(View view, bool swap, bool swapRight, int position) {
			int moveTo = 0;
			if (Opened(position)) {
				if (!swap) {
					moveTo = OpenedRight(position) ? (int) (viewWidth - RightOffset) : (int) (-viewWidth + LeftOffset);
				}
			} else {
				if (swap) {
					moveTo = swapRight ? (int) (viewWidth - RightOffset) : (int) (-viewWidth + LeftOffset);
				}
			}

			var listener = new ObjectAnimatorListenerAdapter();

			int alpha = 1;
			if (swap) {
				++dismissAnimationRefCount;
				alpha = 0;
			}

			listener.AnimationEnd = async (animation) =>
			{
				if(swap) {
					CloseOpenedItems();
					await Task.Delay(Convert.ToInt32(AnimationTime));
					PerformDismiss(view, position, true);
				}
				ResetCell();
			};

			view.Animate()
				.TranslationX(moveTo)
				.Alpha(alpha)
				.SetDuration(_animationTime)
				.SetListener(listener);
		}

		private void ResetCell() {
			if (downPosition != ListView.InvalidPosition) {
				if (swipeCurrentAction == (int)SwipeListView.SwipeAction.Choice) {
					BackView.Visibility = ViewStates.Visible;
				}
				FrontView.Clickable = Opened(downPosition);
				FrontView.LongClickable = Opened(downPosition);
				//FrontView = null;
				_backView = null;
				downPosition = ListView.InvalidPosition;
			}
		}

		/// <summary>
		/// Sets the enabled.
		/// </summary>
		/// <param name="enabled">Enabled.</param>
		public void SetEnabled(bool enabled) {
			paused = !enabled;
		}

		/// <summary>
		/// Closes the opened items.
		/// </summary>
		public void CloseOpenedItems() {
			if (_opened != null) {
				int start = _swipeListView.FirstVisiblePosition;
				int end = _swipeListView.LastVisiblePosition;
				for (int i = start; i <= end; i++) {
					if (Opened(i)) {
						CloseAnimate(_swipeListView.GetChildAt(i - start).FindViewById(_swipeFrontView), i);
					}
				}
			}
		}

		private void SetActionsTo(int action) {
			oldSwipeActionRight = SwipeActionRight;
			oldSwipeActionLeft = SwipeActionLeft;
			SwipeActionRight = action;
			SwipeActionLeft = action;
		}

		protected void ReturnOldActions() {
			SwipeActionRight = oldSwipeActionRight;
			SwipeActionLeft = oldSwipeActionLeft;
		}

		/// <summary>
		/// Move the specified deltaX.
		/// </summary>
		/// <param name="deltaX">Delta x.</param>
		public void Move(float deltaX) {
			_swipeListView.OnMove(downPosition, deltaX);
			float posX = FrontView.GetX();
			if (Opened(downPosition)) {
				posX += OpenedRight(downPosition) ? -viewWidth + RightOffset : viewWidth - LeftOffset;
			}
			if (posX > 0 && !swipingRight) {
				Android.Util.Log.Debug("SwipeListView", "change to right");
				swipingRight = !swipingRight;
				swipeCurrentAction = SwipeActionRight;
				if (swipeCurrentAction == (int)SwipeListView.SwipeAction.Choice) {
					BackView.Visibility = ViewStates.Gone;
				} else {
					BackView.Visibility = ViewStates.Visible;
				}
			}
			if (posX < 0 && swipingRight) {
				Android.Util.Log.Debug("SwipeListView", "change to left");
				swipingRight = !swipingRight;
				swipeCurrentAction = SwipeActionLeft;
				if (swipeCurrentAction == (int)SwipeListView.SwipeAction.Choice) {
					BackView.Visibility = ViewStates.Gone;
				} else {
					BackView.Visibility = ViewStates.Visible;
				}
			}
			if (swipeCurrentAction == (int)SwipeListView.SwipeAction.Dismiss) {
				ParentView.TranslationX = deltaX;
				ParentView.Alpha = (float)Math.Max(0f, Math.Min(1f, 1f - 2f * Math.Abs(deltaX) / viewWidth));
			} else if (swipeCurrentAction == (int)SwipeListView.SwipeAction.Choice) {
				if ((swipingRight && deltaX > 0 && posX < ChoiceOffset)
					|| (!swipingRight && deltaX < 0 && posX > -ChoiceOffset)
					|| (swipingRight && deltaX < ChoiceOffset)
					|| (!swipingRight && deltaX > -ChoiceOffset)) {
					FrontView.TranslationX = deltaX;
				}
			} else if (swipeCurrentAction == (int)SwipeListView.SwipeAction.RevealDismiss) {
				var threshold = Math.Abs(deltaX) / viewWidth;
				if(threshold > RevealDismissThreshold) 
				{
					BackView.Visibility = ViewStates.Gone;
					RevealDismissView.Visibility = ViewStates.Visible;
				}
				else {
					BackView.Visibility = ViewStates.Visible;
					RevealDismissView.Visibility = ViewStates.Gone;
				}
				FrontView.TranslationX = deltaX;
			} 
			else {
				FrontView.TranslationX = deltaX;
			}
		}

		/// <summary>
		/// Performs the dismiss.
		/// </summary>
		/// <param name="dismissView">Dismiss view.</param>
		/// <param name="dismissPosition">Dismiss position.</param>
		/// <param name="doPendingDismiss">If set to <c>true</c> do pending dismiss.</param>
		protected void PerformDismiss(View dismissView, int dismissPosition, bool doPendingDismiss) {
			ViewGroup.LayoutParams lp = dismissView.LayoutParameters;
			int originalHeight = dismissView.Height;

			ValueAnimator animator = (ValueAnimator)ValueAnimator.OfInt(originalHeight, 1).SetDuration(_animationTime);

			if (doPendingDismiss) {
				var listener = new ObjectAnimatorListenerAdapter();
				listener.AnimationEnd = (valueAnimator) =>
				{
					--dismissAnimationRefCount;
					if(dismissAnimationRefCount == 0) {
						RemovePendingDismisses(originalHeight);
					}
				};

				animator.AddListener(listener);
			}

			var updateListener = new ObjectAnimatorUpdateListener();
			updateListener.AnimationUpdate = (valueAnimator) =>
			{
				lp.Height = (int) valueAnimator.AnimatedValue;
				dismissView.LayoutParameters = lp;
			};


			pendingDismisses.Add(new PendingDismissData(dismissPosition, dismissView));
			animator.Start();
		}

		protected void ResetPendingDismisses() {
			pendingDismisses.Clear();
		}

		protected async void HandlerPendingDismisses(int originalHeight) {
			await Task.Delay(Convert.ToInt32(_animationTime) + 100);
			await Task.Run(() =>
			{
				RemovePendingDismisses(originalHeight);
			});
		}

		private void RemovePendingDismisses(int originalHeight) 
		{
			// No active animations, process all pending dismisses.
			// Sort by descending position
			pendingDismisses.Sort();

			int[] dismissPositions = new int[pendingDismisses.Count];
			for (int i = pendingDismisses.Count - 1; i >= 0; i--) {
				dismissPositions[i] = pendingDismisses[i].Position;
			}
			_swipeListView.OnDismiss(dismissPositions);

			ViewGroup.LayoutParams lp;
			foreach (var pendingDismiss in pendingDismisses) {
				// Reset view presentation
				if (pendingDismiss.View != null) {
					pendingDismiss.View.Alpha = 1f;
					pendingDismiss.View.TranslationX = 0;
					lp = pendingDismiss.View.LayoutParameters;
					lp.Height = originalHeight;
					pendingDismiss.View.LayoutParameters = lp;
				}
			}

			ResetPendingDismisses();
		}

		#region IOnTouchListener implementation
		public bool OnTouch(View v, MotionEvent e)
		{
			float velocityX, velocityY, deltaX;
			int localSwipeMode = SwipeMode;

			if (!IsSwipeEnabled) {
				return false;
			}

			if (viewWidth < 2) {
				viewWidth = _swipeListView.Width;
			}

			switch(MotionEventCompat.GetActionMasked(e))
			{
				case (int)MotionEventActions.Down: 
					if(paused && downPosition != ListView.InvalidPosition)
					{
						return false;
					}
					swipeCurrentAction = (int)SwipeListView.SwipeAction.None;

					int childCount = _swipeListView.ChildCount;
					int[] listViewCoords = new int[2];
					_swipeListView.GetLocationOnScreen(listViewCoords);
					int x = (int)e.RawX - listViewCoords[0];
					int y = (int)e.RawY - listViewCoords[1];
					View child;
					for(int i = 0; i < childCount; i++)
					{
						child = _swipeListView.GetChildAt(i);
						child.GetHitRect(rect);

						int childPosition = _swipeListView.GetPositionForView(child);

						// dont allow swiping if this is on the header or footer or IGNORE_ITEM_VIEW_TYPE or enabled is false on the adapter
						bool allowSwipe = _swipeListView.Adapter.IsEnabled(childPosition) && _swipeListView.Adapter.GetItemViewType(childPosition) >= 0;

						if(allowSwipe && rect.Contains(x, y))
						{
							ParentView = child;
							var viewHolder = child.FindViewById(_swipeFrontView);
							FrontView = viewHolder;

							downX = e.RawX;
							downPosition = childPosition;

							FrontView.Clickable = !Opened(downPosition);
							FrontView.LongClickable = !Opened(downPosition);

							velocityTracker = VelocityTracker.Obtain();
							velocityTracker.AddMovement(e);
							if(swipeBackView > 0)
							{
								BackView = child.FindViewById(swipeBackView);
							}
							if(_swipeRevealDismissView > 0)
							{
								RevealDismissView = child.FindViewById(_swipeRevealDismissView);
							}
							break;
						}
					}
					v.OnTouchEvent(e);
					return true;

				case (int)MotionEventActions.Up: 
					if(velocityTracker == null || !swiping || downPosition == ListView.InvalidPosition)
					{
						break;
					}

					deltaX = e.RawX - downX;
					velocityTracker.AddMovement(e);
					velocityTracker.ComputeCurrentVelocity(1000);
					velocityX = Math.Abs(velocityTracker.XVelocity);
					if(Opened(downPosition))
					{
						if(localSwipeMode == (int)SwipeListView.SwipeMode.Left && velocityTracker.XVelocity > 0)
						{
							velocityX = 0;
						}
						if(localSwipeMode == (int)SwipeListView.SwipeMode.Right && velocityTracker.XVelocity < 0)
						{
							velocityX = 0;
						}
					}
					velocityY = Math.Abs(velocityTracker.YVelocity);
					bool swap = false;
					bool swapRight = false;
					if(minFlingVelocity <= velocityX && velocityX <= maxFlingVelocity && velocityY * 2 < velocityX)
					{
						swapRight = velocityTracker.XVelocity > 0;
						Android.Util.Log.Debug("SwipeListView", "swapRight: " + swapRight + " - swipingRight: " + swipingRight);
						if(swapRight != swipingRight && SwipeActionLeft != SwipeActionRight)
						{
							swap = false;
						}
						else if(Opened(downPosition) && OpenedRight(downPosition) && swapRight)
						{
							swap = false;
						}
						else if(Opened(downPosition) && !OpenedRight(downPosition) && !swapRight)
						{
							swap = false;
						}
						else if(SwipeAllowFling)
						{
							swap = true;
						}
					}
					else if(velocityX > maxFlingVelocity)
					{
						swap = false;
					}
					else if((swipeCurrentAction == (int)SwipeListView.SwipeAction.RevealDismiss && Math.Abs(deltaX) > (RevealDismissThreshold * viewWidth)) || (swipeCurrentAction != (int)SwipeListView.SwipeAction.RevealDismiss && Math.Abs(deltaX) > viewWidth / 2))
					{
						swap = true;
						swapRight = deltaX > 0;
					}

					GenerateAnimate(FrontView, swap, swapRight, downPosition);

					if(swipeCurrentAction == (int)SwipeListView.SwipeAction.Choice)
					{
						SwapChoiceState(downPosition);
					}

					velocityTracker.Recycle();
					velocityTracker = null;
					downX = 0;
				
					// change clickable front view
					if(swap)
					{
						FrontView.Clickable = Opened(downPosition);
						FrontView.LongClickable = Opened(downPosition);
					}

					swiping = false;
					break;

				case (int)MotionEventActions.Move: 
					if(velocityTracker == null || paused || downPosition == ListView.InvalidPosition)
					{
						break;
					}

					velocityTracker.AddMovement(e);
					velocityTracker.ComputeCurrentVelocity(1000);

					velocityX = Math.Abs(velocityTracker.XVelocity);
					velocityY = Math.Abs(velocityTracker.YVelocity);

					deltaX = e.RawX - downX;
					float deltaMode = Math.Abs(deltaX);

					int changeSwipeMode = _swipeListView.ChangeSwipeMode(downPosition);
					if(changeSwipeMode >= 0)
					{
						localSwipeMode = changeSwipeMode;
					}

					if(localSwipeMode == (int)SwipeListView.SwipeMode.None)
					{
						deltaMode = 0;
					}
					else if(localSwipeMode != (int)SwipeListView.SwipeMode.Both)
					{
						if(Opened(downPosition))
						{
							if(localSwipeMode == (int)SwipeListView.SwipeMode.Left && deltaX < 0)
							{
								deltaMode = 0;
							}
							else if(localSwipeMode == (int)SwipeListView.SwipeMode.Right && deltaX > 0)
							{
								deltaMode = 0;
							}
						}
						else
						{
							if(localSwipeMode == (int)SwipeListView.SwipeMode.Left && deltaX > 0)
							{
								deltaMode = 0;
							}
							else if(localSwipeMode == (int)SwipeListView.SwipeMode.Right && deltaX < 0)
							{
								deltaMode = 0;
							}
						}
					}

					if(deltaMode > slop && swipeCurrentAction == (int)SwipeListView.SwipeAction.None && velocityY < velocityX)
					{
						swiping = true;
						swipingRight = (deltaX > 0);
						Android.Util.Log.Debug("SwipeListView", "deltaX: " + deltaX + " - swipingRight: " + swipingRight);
						if(Opened(downPosition))
						{
							_swipeListView.OnStartClose(downPosition, swipingRight);
							swipeCurrentAction = (int)SwipeListView.SwipeAction.Reveal;
						}
						else
						{
							if (swipingRight && SwipeActionRight == (int)SwipeListView.SwipeAction.Dismiss) 
							{
								swipeCurrentAction = (int)SwipeListView.SwipeAction.Dismiss;
							} 
							else if (!swipingRight && SwipeActionLeft == (int)SwipeListView.SwipeAction.Dismiss) 
							{
								swipeCurrentAction = (int)SwipeListView.SwipeAction.Dismiss;
							} 	
							else if (swipingRight && SwipeActionRight == (int)SwipeListView.SwipeAction.Choice) 
							{
								swipeCurrentAction = (int)SwipeListView.SwipeAction.Choice;
							} 
							else if (!swipingRight && SwipeActionLeft == (int)SwipeListView.SwipeAction.Choice) 
							{
								swipeCurrentAction = (int)SwipeListView.SwipeAction.Choice;
							} 
							else if (swipingRight && SwipeActionRight == (int)SwipeListView.SwipeAction.RevealDismiss) 
							{
								swipeCurrentAction = (int)SwipeListView.SwipeAction.RevealDismiss;
							} 
							else if (!swipingRight && SwipeActionLeft == (int)SwipeListView.SwipeAction.RevealDismiss) 
							{
								swipeCurrentAction = (int)SwipeListView.SwipeAction.RevealDismiss;
							} 
							else 
							{
								swipeCurrentAction = (int)SwipeListView.SwipeAction.Reveal;
							}
						
							_swipeListView.OnStartOpen(downPosition, swipeCurrentAction, swipingRight);
						}

						_swipeListView.RequestDisallowInterceptTouchEvent(true);
						MotionEvent cancelEvent = MotionEvent.Obtain(e);
						cancelEvent.Action = (MotionEventActions)((int)MotionEventActions.Cancel | (MotionEventCompat.GetActionIndex(e) << MotionEventCompat.ActionPointerIndexShift));
						_swipeListView.OnTouchEvent(cancelEvent);
						if (swipeCurrentAction == (int)SwipeListView.SwipeAction.Choice) {
							BackView.Visibility = ViewStates.Gone;
						}
					}

					if(swiping && downPosition != ListView.InvalidPosition)
					{		
						if(Opened(downPosition))
						{
							deltaX += OpenedRight(downPosition) ? viewWidth - RightOffset : -viewWidth + LeftOffset;
						}
						if(deltaMode != 0f)
						{
							Move(deltaX);
						}
						return true;
					}
					break;
				
			}
			return false;
		}
		#endregion

		#region "AbsListView.IOnScrollListener"
		public void OnScroll(AbsListView view, int firstVisibleItem, int visibleItemCount, int totalItemCount)
		{
			if (_isFirstItem) 
			{
				bool onSecondItemList = firstVisibleItem == 1;
				if (onSecondItemList) {
					_isFirstItem = false;
				}
			} 
			else 
			{
				bool onFirstItemList = firstVisibleItem == 0;
				if (onFirstItemList) {
					_isFirstItem = true;
					_swipeListView.OnFirstListItem();
				}
			}

			if (_isLastItem) {
				bool onBeforeLastItemList = firstVisibleItem + visibleItemCount == totalItemCount - 1;
				if (onBeforeLastItemList) {
					_isLastItem = false;
				}
			} else {
				bool onLastItemList = firstVisibleItem + visibleItemCount >= totalItemCount;
				if (onLastItemList) {
					_isLastItem = true;
					_swipeListView.OnLastListItem();
				}
			}
		}

		public async void OnScrollStateChanged(AbsListView view, ScrollState scrollState)
		{
			SetEnabled(scrollState != ScrollState.TouchScroll);
			if (SwipeClosesAllItemsWhenListMoves && scrollState == ScrollState.TouchScroll) 
			{
				CloseOpenedItems();
			}
			if (scrollState == ScrollState.TouchScroll) 
			{
				IsListViewMoving = true;
				SetEnabled(false);
			}

			if (scrollState != ScrollState.Fling && scrollState != ScrollState.TouchScroll) 
			{
				IsListViewMoving = false;
				downPosition = ListView.InvalidPosition;
				_swipeListView.ResetScrolling();

				await Task.Delay(500);
				await Task.Run(() => SetEnabled(true));
			}
		}
		#endregion
	}

	public class PendingDismissData : IComparable
	{
		public int Position { get; set; }
		public View View { get; set; }

		public PendingDismissData(int position, View view) 
		{
			Position = position;
			View = view;
		} 

		public int CompareTo(object obj)
		{
			var data = (PendingDismissData)obj;
			// Sort by descending position
			return data.Position - Position;
		}
	}

	public class ObjectAnimatorListenerAdapter : AnimatorListenerAdapter
	{
		public ObjectAnimatorListenerAdapter ()
		{
			this.AnimationEnd = (animator) => {};
		}

		public Action<Animator> AnimationEnd { get; set; }

		public override void OnAnimationEnd (Animator animator) 
		{
			AnimationEnd(animator);
		}
	}

	public class ObjectAnimatorUpdateListener : Java.Lang.Object, Android.Animation.ValueAnimator.IAnimatorUpdateListener
	{
		public ObjectAnimatorUpdateListener()
		{
			AnimationUpdate = (valueAnimator) => {};
		}

		public Action<ValueAnimator> AnimationUpdate { get; set; }

		#region IAnimatorUpdateListener implementation
		public void OnAnimationUpdate(ValueAnimator valueAnimator)
		{
			AnimationUpdate(valueAnimator);
		}
		#endregion
	}
}

