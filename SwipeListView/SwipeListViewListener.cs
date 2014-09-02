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

namespace FortySevenDeg.SwipeListView
{
	public class SwipeListViewListener : ISwipeListViewListener
	{
		public SwipeListViewListener()
		{
			OnOpened = (position, toRight) => {};
			OnClosed = (position, fromRight) => {};
			OnListChanged = () => {};
			OnMove = (position, x) => {};
			OnStartOpen = (position, action, right) => {};
			OnStartClose = (position, right) => {};
			OnClickFrontView = (position) => {};
			OnClickBackView = (position) => {};
			OnDismiss = (position) => {};
			OnChangeSwipeMode = (position) => (int)SwipeListView.SwipeMode.Default;
			OnChoiceChanged = (position, selected) => {};
			OnChoiceStarted = () => {};
			OnChoiceEnded = () => {};
			OnFirstListItem = () => {};
			OnLastListItem = () => {};
		}

		#region ISwipeListViewListener implementation
		public Action<int, bool> OnOpened { get; set; }

		public Action<int, bool> OnClosed { get; set; }

		public Action OnListChanged { get; set; }

		public Action<int, float> OnMove { get; set; }

		public Action<int, int, bool> OnStartOpen { get; set; }

		public Action<int, bool> OnStartClose { get; set; }

		public Action<int> OnClickFrontView { get; set; }

		public Action<int> OnClickBackView { get; set; }

		public Action<int[]> OnDismiss { get; set; }

		public Func<int, int> OnChangeSwipeMode { get; set; }

		public Action<int, bool> OnChoiceChanged { get; set; }

		public Action OnChoiceStarted { get; set; }

		public Action OnChoiceEnded { get; set; }

		public Action OnFirstListItem { get; set; }

		public Action OnLastListItem { get; set; }
		#endregion
	}
}

