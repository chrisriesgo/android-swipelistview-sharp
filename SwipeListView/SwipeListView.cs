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
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Support.V4.View;
using Android.Database;

namespace FortySevenDeg.SwipeListView
{
	public class ObjectDataSetObserver : DataSetObserver
	{
		public Action Changed { get; set; }
		public override void OnChanged()
		{
			Changed();
		}
	}

	public class SwipeListView : ListView
	{
		/**
	     * Default ids for front view
	     */
		public static String SWIPE_DEFAULT_FRONT_VIEW = "swipelist_frontview";

		/**
	     * Default id for back view
	     */
		public static String SWIPE_DEFAULT_BACK_VIEW = "swipelist_backview";

		public enum SwipeMode
		{
			Default = -1,
			None = 0,
			Both = 1,
			Right = 2,
			Left = 3
		}

		public enum SwipeAction
		{
			None = 0,
			Reveal = 1,
			Dismiss = 2,
			Choice = 3
		}

		public enum TouchState
		{
			Rest = 0,
			ScrollingX = 1,
			ScrollingY = 2
		}
		private TouchState touchState = TouchState.Rest;

		private float lastMotionX;
		private float lastMotionY;
		private int touchSlop;

		int _swipeFrontView = 0;
		int _swipeBackView = 0;

		private SwipeListViewTouchListener _touchListener;

		public SwipeListView(IntPtr ptr, JniHandleOwnership handler) : base(ptr, handler) { }
		public SwipeListView(Context context, IAttributeSet attrs) : base(context, attrs) 
		{
			Init(attrs);
		}
		public SwipeListView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle) 
		{
			Init(attrs);
		}
		public SwipeListView(Context context, int swipeBackView, int swipeFrontView) : base(context) 
		{
			_swipeFrontView = swipeFrontView;
			_swipeBackView = swipeBackView;
			Init(null);
		}

		private void Init(IAttributeSet attrs) 
		{

			long swipeAnimationTime = 0;
			float swipeOffsetLeft = 0;
			float swipeOffsetRight = 0;
			var swipeMode = (int)SwipeMode.Both;
			var swipeOpenOnLongPress = true;
			var swipeCloseAllItemsWhenMoveList = true;
			var swipeDrawableChecked = 0;
			var swipeDrawableUnchecked = 0;

			var swipeActionLeft = (int)SwipeAction.Reveal;
			var swipeActionRight = (int)SwipeAction.Reveal;

			if (attrs != null) {
				var styled = Context.ObtainStyledAttributes(attrs, Resource.Styleable.SwipeListView);// getContext().obtainStyledAttributes(attrs, R.styleable.SwipeListView);

				swipeMode = styled.GetInt(Resource.Styleable.SwipeListView_swipeMode, ((int)SwipeMode.Both));
				swipeActionLeft = styled.GetInt(Resource.Styleable.SwipeListView_swipeActionLeft, (int)SwipeAction.Reveal);
				swipeActionRight = styled.GetInt(Resource.Styleable.SwipeListView_swipeActionRight, (int)SwipeAction.Reveal);
				swipeOffsetLeft = styled.GetDimension(Resource.Styleable.SwipeListView_swipeOffsetLeft, 0);
				swipeOffsetRight = styled.GetDimension(Resource.Styleable.SwipeListView_swipeOffsetRight, 0);
//				_choiceOffset = styled.GetDimension(Resource.Styleable.SwipeListView_choiceOffset, _choiceOffset);
//				_swipeRevealDismissThreshold = styled.GetFloat(Resource.Styleable.SwipeListView_swipeRevealDismissThreshold, _swipeRevealDismissThreshold);
				swipeOpenOnLongPress = styled.GetBoolean(Resource.Styleable.SwipeListView_swipeOpenOnLongPress, swipeOpenOnLongPress);
				swipeAnimationTime = styled.GetInt(Resource.Styleable.SwipeListView_swipeAnimationTime, 0);
				swipeCloseAllItemsWhenMoveList = styled.GetBoolean(Resource.Styleable.SwipeListView_swipeCloseAllItemsWhenMoveList, true);
				swipeDrawableChecked = styled.GetResourceId(Resource.Styleable.SwipeListView_swipeDrawableChecked, 0);
				swipeDrawableUnchecked = styled.GetResourceId(Resource.Styleable.SwipeListView_swipeDrawableUnchecked, 0);
//				swipeAllowFling = styled.GetBoolean(Resource.Styleable.SwipeListView_swipeAllowFling, swipeAllowFling);

				_swipeFrontView = styled.GetResourceId(Resource.Styleable.SwipeListView_swipeFrontView, 0);
				_swipeBackView = styled.GetResourceId(Resource.Styleable.SwipeListView_swipeBackView, 0);
//				_swipeRevealDismissView = styled.GetResourceId(Resource.Styleable.SwipeListView_swipeRevealDismissView, 0);
			}

			if (_swipeFrontView == 0 || _swipeBackView == 0) {
				_swipeFrontView = Context.Resources.GetIdentifier(SWIPE_DEFAULT_FRONT_VIEW, "id", Context.PackageName);
				_swipeBackView = Context.Resources.GetIdentifier(SWIPE_DEFAULT_BACK_VIEW, "id", Context.PackageName);

				if (_swipeFrontView == 0 || _swipeBackView == 0) {
					throw new Exception(String.Format("You forgot the attributes swipeFrontView or swipeBackView. You can add these attributes or use '{0}' and '{1}' identifiers", SWIPE_DEFAULT_FRONT_VIEW, SWIPE_DEFAULT_BACK_VIEW));
				}
			}

			ViewConfiguration configuration = ViewConfiguration.Get(Context);
			touchSlop = ViewConfigurationCompat.GetScaledPagingTouchSlop(configuration);
			_touchListener = new SwipeListViewTouchListener(this, _swipeFrontView, _swipeBackView);

			if (swipeAnimationTime > 0) {
				_touchListener.AnimationTime = swipeAnimationTime;
			}
			_touchListener.RightOffset = swipeOffsetRight;
			_touchListener.LeftOffset = swipeOffsetLeft;
//			_touchListener.ChoiceOffset = _choiceOffset;
//			_touchListener.RevealDismissThreshold = _swipeRevealDismissThreshold;
			_touchListener.SwipeActionLeft = swipeActionLeft;
			_touchListener.SwipeActionRight = swipeActionRight;
			_touchListener.SwipeMode = swipeMode;
			_touchListener.SwipeClosesAllItemsWhenListMoves = swipeCloseAllItemsWhenMoveList;
			_touchListener.SwipeOpenOnLongPress = swipeOpenOnLongPress;
			_touchListener.SwipeDrawableChecked = swipeDrawableChecked;
			_touchListener.SwipeDrawableUnchecked = swipeDrawableUnchecked;
//			_touchListener.SwipeAllowFling = swipeAllowFling;
			SetOnTouchListener(_touchListener);
			SetOnScrollListener(_touchListener);
		}

		/// <summary>
		/// Recycle the specified convertView and position.
		/// </summary>
		/// <param name="convertView">Convert view.</param>
		/// <param name="position">Position.</param>
		public void Recycle(View convertView, int position) {
			_touchListener.ReloadChoiceStateInView(convertView.FindViewById(_swipeFrontView), position);
			_touchListener.ReloadSwipeStateInView(convertView.FindViewById(_swipeFrontView), position);

			// Clean pressed state (if dismiss is fire from a cell, to this cell, with a press drawable, in a swipelistview
			// when this cell will be recycle it will still have his pressed state. This ensure the pressed state is
			// cleaned.
			for(int j = 0; j < ((ViewGroup)convertView).ChildCount; ++j) {
				View nextChild = ((ViewGroup)convertView).GetChildAt(j);
				nextChild.Pressed = false;
			}
		}

		/// <summary>
		/// Ises the checked.
		/// </summary>
		/// <returns>The checked.</returns>
		/// <param name="position">Position.</param>
		public bool IsChecked(int position) {
			return _touchListener.IsChecked(position);
		}

		#region "Properties"
		/// <summary>
		/// Gets the positions selected.
		/// </summary>
		/// <returns>The positions selected.</returns>
		public List<int> PositionsSelected 
		{
			get
			{
				return _touchListener.PositionsSelected;
			}
		}

		/// <summary>
		/// Gets the count selected.
		/// </summary>
		/// <returns>The count selected.</returns>
		public int CountSelected 
		{
			get
			{
				return _touchListener.CountSelected;
			}
		}

		/// <summary>
		/// Unselecteds the choice states.
		/// </summary>
		public void UnselectedChoiceStates() {
			_touchListener.UnselectedChoiceStates();
		}

		/// <summary>
		/// Gets or sets the adapter.
		/// </summary>
		/// <value>The adapter.</value>
		public override IListAdapter Adapter
		{
			get
			{
				return base.Adapter;
			}
			set
			{
				base.Adapter = value;
				_touchListener.ResetItems();

				if(base.Adapter != null)
				{
					var observer = new ObjectDataSetObserver();
					observer.Changed = () =>
					{
						OnListChanged();
						_touchListener.ResetItems();
					};

					base.Adapter.RegisterDataSetObserver(observer);
				}
			}
		}


		/// <summary>
		/// Dismiss the specified position.
		/// </summary>
		/// <param name="position">Position.</param>
		public void Dismiss(int position) {
			int height = _touchListener.Dismiss(position);
			if (height > 0) {
				_touchListener.HandlerPendingDismisses(height);
			} else {
				int[] dismissPositions = new int[1];
				dismissPositions[0] = position;
				OnDismiss(dismissPositions);
				_touchListener.ResetPendingDismisses();
			}
		}

		/// <summary>
		/// Dismisses items selected.
		/// </summary>
		public void DismissSelected() {
			List<int> list = _touchListener.PositionsSelected;
			int[] dismissPositions = new int[list.Count];
			int height = 0;
			for (int i = 0; i < list.Count; i++) {
				int position = list[i];
				dismissPositions[i] = position;
				int auxHeight = _touchListener.Dismiss(position);
				if (auxHeight > 0) {
					height = auxHeight;
				}
			}
			if (height > 0) {
				_touchListener.HandlerPendingDismisses(height);
			} else {
				OnDismiss(dismissPositions);
				_touchListener.ResetPendingDismisses();
			}
			_touchListener.ReturnOldActions();
		}

		/// <summary>
		/// Opens the animate.
		/// </summary>
		/// <param name="position">Position.</param>
		public void OpenAnimate(int position) {
			_touchListener.OpenAnimate(position);
		}

		/// <summary>
		/// Closes the animate.
		/// </summary>
		/// <param name="position">Position.</param>
		public void CloseAnimate(int position) {
			_touchListener.CloseAnimate(position);
		}

		#endregion

		/// Notifies OnDismiss
		public void OnDismiss(int[] reverseSortedPositions) {
			if (SwipeListViewListener != null) {
				SwipeListViewListener.OnDismiss(reverseSortedPositions);
			}
		}
			
		public void OnStartOpen(int position, int action, bool right) {
			if (SwipeListViewListener != null && position != ListView.InvalidPosition) {
				SwipeListViewListener.OnStartOpen(position, action, right);
			}
		}

		public void OnStartClose(int position, bool right) {
			if (SwipeListViewListener != null && position != ListView.InvalidPosition) {
				SwipeListViewListener.OnStartClose(position, right);
			}
		}

		public void OnClickFrontView(int position) {
			if (SwipeListViewListener != null && position != ListView.InvalidPosition) {
				SwipeListViewListener.OnClickFrontView(position);
			}
		}

		public void OnClickBackView(int position) {
			if (SwipeListViewListener != null && position != ListView.InvalidPosition) {
				SwipeListViewListener.OnClickBackView(position);
			}
		}

		public void OnOpened(int position, bool toRight) {
			if (SwipeListViewListener != null && position != ListView.InvalidPosition) {
				SwipeListViewListener.OnOpened(position, toRight);
			}
		}

		public void OnClosed(int position, bool fromRight) {
			if (SwipeListViewListener != null && position != ListView.InvalidPosition) {
				SwipeListViewListener.OnClosed(position, fromRight);
			}
		}

		public void OnChoiceChanged(int position, bool selected) {
			if (SwipeListViewListener != null && position != ListView.InvalidPosition) {
				SwipeListViewListener.OnChoiceChanged(position, selected);
			}
		}

		public void OnChoiceStarted() {
			if (SwipeListViewListener != null) {
				SwipeListViewListener.OnChoiceStarted();
			}
		}

		public void OnChoiceEnded() {
			if (SwipeListViewListener != null) {
				SwipeListViewListener.OnChoiceEnded();
			}
		}

		public void OnFirstListItem() {
			if (SwipeListViewListener != null) {
				SwipeListViewListener.OnFirstListItem();
			}
		}

		public void OnLastListItem() {
			if (SwipeListViewListener != null) {
				SwipeListViewListener.OnLastListItem();
			}
		}

		public void OnListChanged() {
			if (SwipeListViewListener != null) {
				SwipeListViewListener.OnListChanged();
			}
		}

		public void OnMove(int position, float x) {
			if (SwipeListViewListener != null && position != ListView.InvalidPosition) {
				SwipeListViewListener.OnMove(position, x);
			}
		}

		public int ChangeSwipeMode(int position) {
			if (SwipeListViewListener != null && position != ListView.InvalidPosition) {
				return SwipeListViewListener.OnChangeSwipeMode(position);
			}
			return (int)SwipeMode.Default;
		}

		public ISwipeListViewListener SwipeListViewListener { get; set; }

		public void ResetScrolling() {
			touchState = TouchState.Rest;
		}

		/// <summary>
		/// Sets the offset right.
		/// </summary>
		/// <param name="offsetRight">Offset right.</param>
		public void SetOffsetRight(float offsetRight) {
			_touchListener.RightOffset = offsetRight;
		}

		/// <summary>
		/// Sets the offset left.
		/// </summary>
		/// <param name="offsetLeft">Offset left.</param>
		public void SetOffsetLeft(float offsetLeft) {
			_touchListener.LeftOffset = offsetLeft;
		}

		/// <summary>
		/// Sets the swipe close all items when move list.
		/// </summary>
		/// <param name="swipeCloseAllItemsWhenMoveList">Swipe close all items when move list.</param>
		public void SetSwipeCloseAllItemsWhenMoveList(bool swipeCloseAllItemsWhenMoveList) {
			_touchListener.SwipeClosesAllItemsWhenListMoves = swipeCloseAllItemsWhenMoveList;
		}

		/// <summary>
		/// Sets the swipe open on long press.
		/// </summary>
		/// <param name="swipeOpenOnLongPress">Swipe open on long press.</param>
		public void SetSwipeOpenOnLongPress(bool swipeOpenOnLongPress) {
			_touchListener.SwipeOpenOnLongPress = swipeOpenOnLongPress;
		}

		/// <summary>
		/// Sets the swipe mode.
		/// </summary>
		/// <param name="swipeMode">Swipe mode.</param>
		public void SetSwipeMode(int swipeMode) {
			_touchListener.SwipeMode = swipeMode;
		}

		/// <summary>
		/// Gets the swipe action left.
		/// </summary>
		/// <returns>The swipe action left.</returns>
		public int GetSwipeActionLeft() {
			return _touchListener.SwipeActionLeft;
		}

		/// <summary>
		/// Sets the swipe action left.
		/// </summary>
		/// <param name="swipeActionLeft">Swipe action left.</param>
		public void SetSwipeActionLeft(int swipeActionLeft) {
			_touchListener.SwipeActionLeft = swipeActionLeft;
		}

		/// <summary>
		/// Gets the swipe action right.
		/// </summary>
		/// <returns>The swipe action right.</returns>
		public int GetSwipeActionRight() {
			return _touchListener.SwipeActionRight;
		}

		/// <summary>
		/// Sets the swipe action right.
		/// </summary>
		/// <param name="swipeActionRight">Swipe action right.</param>
		public void SetSwipeActionRight(int swipeActionRight) {
			_touchListener.SwipeActionRight = swipeActionRight;
		}

		/// <summary>
		/// Sets the animation time.
		/// </summary>
		/// <param name="animationTime">Animation time.</param>
		public void SetAnimationTime(long animationTime) {
			_touchListener.AnimationTime = animationTime;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SwipeListView.SwipeListView"/> class.
		/// </summary>
		/// <param name="ev">Ev.</param>
		public override bool OnInterceptTouchEvent(MotionEvent ev)
		{
			int action = MotionEventCompat.GetActionMasked(ev);
			float x = ev.GetX();
			float y = ev.GetY();

			if (this.Enabled && _touchListener.IsSwipeEnabled) 
			{

				if (touchState == TouchState.ScrollingX) 
				{
					return _touchListener.OnTouch(this, ev);
				}

				switch (action) {
					case (int)MotionEventActions.Move:
						CheckInMoving(x, y);
						return touchState == TouchState.ScrollingY;
					case (int)MotionEventActions.Down:
						base.OnInterceptTouchEvent(ev);
						_touchListener.OnTouch(this, ev);
						touchState = TouchState.Rest;
						lastMotionX = x;
						lastMotionY = y;
						return false;
					case (int)MotionEventActions.Cancel:
						touchState = TouchState.Rest;
						break;
					case (int)MotionEventActions.Up:
						_touchListener.OnTouch(this, ev);
						return touchState == TouchState.ScrollingY;
					default:
						break;
				}
			}

			return base.OnInterceptTouchEvent(ev);
		}

		/// <summary>
		/// Checks the in moving.
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		private void CheckInMoving(float x, float y) 
		{
			int xDiff = (int) Math.Abs(x - lastMotionX);
			int yDiff = (int) Math.Abs(y - lastMotionY);

			int touchSlop = this.touchSlop;
			bool xMoved = xDiff > touchSlop;
			bool yMoved = yDiff > touchSlop;

			if (xMoved) {
				touchState = TouchState.ScrollingX;
				lastMotionX = x;
				lastMotionY = y;
			}

			if (yMoved) {
				touchState = TouchState.ScrollingY;
				lastMotionX = x;
				lastMotionY = y;
			}
		}

		/// <summary>
		/// Closes the opened items.
		/// </summary>
		public void CloseOpenedItems() 
		{
			_touchListener.CloseOpenedItems();
		}

		public void Close(int position)
		{
			_touchListener.CloseAnimate(position);
			return;
		}

		public void Open(int position)
		{
			_touchListener.OpenAnimate(position);
			return;
		}
	}
}

