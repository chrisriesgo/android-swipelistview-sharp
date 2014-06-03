SwipeListView | C#
===========================

This is a C# port of the [47 Degrees Swipe List View](https://github.com/47deg/android-swipelistview). 

An Android List View implementation with support for drawable cells and many other swipe related features.

#XML Usage

If you decide to use SwipeListView as a view, you can define it in your xml layout like this:

```xml
	<fortysevendeg.swipelistview.SwipeListView
            xmlns:swipe="http://schemas.android.com/apk/res-auto"
            android:id="@+id/example_lv_list"
            android:listSelector="#00000000"
            android:layout_width="fill_parent"
            android:layout_height="wrap_content"
            swipe:swipeFrontView="@+id/front"
            swipe:swipeBackView="@+id/back"
            swipe:swipeActionLeft="[reveal | dismiss]"
            swipe:swipeActionRight="[reveal | dismiss]"
            swipe:swipeMode="[none | both | right | left]"
            swipe:swipeCloseAllItemsWhenMoveList="[true | false]"
            swipe:swipeOpenOnLongPress="[true | false]"
            swipe:swipeAnimationTime="[miliseconds]"
            swipe:swipeOffsetLeft="[dimension]"
            swipe:swipeOffsetRight="[dimension]"
            swipe:choiceOffset="[dimension]"
            />
```

* `swipeFrontView` - **Required** - front view id.
* `swipeBackView` - **Required** - back view id.
* `swipeActionLeft` - Optional - left swipe action Default: 'reveal'
* `swipeActionRight` - Optional - right swipe action Default: 'reveal'
* `swipeMode` - Gestures to enable or 'none'. Default: 'both'
* `swipeCloseAllItemsWhenMoveList` - Close revealed items on list motion. Default: 'true'
* `swipeOpenOnLongPress` - Reveal on long press Default: 'true'
* `swipeAnimationTime` - item drop animation time. Default: android configuration
* `swipeOffsetLeft` - left offset
* `swipeOffsetRight` - right offset
* `choiceOffset` - choice toggle offset

# License

Copyright (C) 2012 47 Degrees, LLC
http://47deg.com
hello@47deg.com

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.