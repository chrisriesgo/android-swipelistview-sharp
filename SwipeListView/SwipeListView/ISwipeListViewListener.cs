using System;

namespace FortySevenDeg.SwipeListView
{
	public interface ISwipeListViewListener {

		/**
	     * Called when open animation finishes
	     * @param position list item
	     * @param toRight Open to right
	     */
		Action<int, bool> OnOpened { get; set; }

		/**
	     * Called when close animation finishes
	     * @param position list item
	     * @param fromRight Close from right
	     */
		Action<int, bool> OnClosed { get; set; }

		/**
	     * Called when the list changed
	     */
		Action OnListChanged { get; set; }

		/**
	     * Called when user is moving an item
	     * @param position list item
	     * @param x Current position X
	     */
		Action<int, float> OnMove { get; set; }

		/**
	     * Start open item
	     * @param position list item
	     * @param action current action
	     * @param right to right
	     */
		Action<int, int, bool> OnStartOpen { get; set; }

		/**
	     * Start close item
	     * @param position list item
	     * @param right
	     */
		Action<int, bool> OnStartClose { get; set; }

		/**
	     * Called when user clicks on the front view
	     * @param position list item
	     */
		Action<int> OnClickFrontView { get; set; }

		/**
	     * Called when user clicks on the back view
	     * @param position list item
	     */
		Action<int> OnClickBackView { get; set; }

		/**
	     * Called when user dismisses items
	     * @param reverseSortedPositions Items dismissed
	     */
		Action<int[]> OnDismiss { get; set; }

		/**
	     * Used when user want to change swipe list mode on some rows. Return SWIPE_MODE_DEFAULT
	     * if you don't want to change swipe list mode
	     * @param position position that you want to change
	     * @return type
	     */
		Func<int, int> OnChangeSwipeMode { get; set; }

		/**
	     * Called when user choice item
	     * @param position position that choice
	     * @param selected if item is selected or not
	     */
		Action<int, bool> OnChoiceChanged { get; set; }

		/**
	     * User start choice items
	     */
		Action OnChoiceStarted { get; set; }

		/**
	     * User end choice items
	     */
		Action OnChoiceEnded { get; set; }

		/**
	     * User is in first item of list
	     */
		Action OnFirstListItem { get; set; }

		/**
	     * User is in last item of list
	     */
		Action OnLastListItem { get; set; }

	}
}

