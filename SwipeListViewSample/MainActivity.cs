using System;

using Android.App;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using System.Linq;
using Android.Support.V4.App;
using Android.Graphics.Drawables;
using FortySevenDeg.SwipeListView;

namespace SwipeListViewSample
{
	[Activity(Label = "Swipe ListView", MainLauncher = true)]
	public class MainActivity : FragmentActivity
	{
		SwipeListView _swipeListView;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);
			ActionBar.SetIcon(new ColorDrawable(Android.Graphics.Color.Transparent));

			// Get our button from the layout resource,
			// and attach an event to it
			_swipeListView = FindViewById<SwipeListView>(Resource.Id.example_lv_list);

			var adapter = new DogsAdapter(this, Dogs.GetDogData());

			_swipeListView.SwipeListViewListener = SetupListener(adapter);
			_swipeListView.Adapter = adapter;	
		}

		private ISwipeListViewListener SetupListener(DogsAdapter adapter)
		{
			var listener = new SwipeListViewListener();
			listener.OnDismiss = (reverseSortedPosition) =>
			{
				foreach (var i in reverseSortedPosition)
				{
					adapter.RemoveView(i);
				}
			};
			listener.OnClickFrontView = (position) => RunOnUiThread(() => _swipeListView.Open(position));
			listener.OnClickBackView = (position) => RunOnUiThread(() => _swipeListView.Close(position));

			return listener;
		}
	}


	public class DogsAdapter: BaseAdapter<Dog>
	{
		private readonly List<Dog> data;
		private readonly Activity context;

		public DogsAdapter(Activity activity, IEnumerable<Dog> speakers)
		{
			data = speakers.OrderBy(s => s.Name).ToList();
			context = activity;
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override Dog this [int index] {
			get { return data[index]; }
		}

		public override int Count {
			get { return data.Count; }
		}

		public void RemoveView(int position)
		{
			data.RemoveAt(position);
			NotifyDataSetChanged();
		}
			
		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			var view = convertView;
			if(view == null)
			{
				// inflate the custom AXML layout
				view = context.LayoutInflater.Inflate(Resource.Layout.package_row, null);
			}

			((SwipeListView)parent).Recycle(view, position);

			var dog = data[position];

			var ivImage = view.FindViewById<ImageView>(Resource.Id.example_row_iv_image);
			var tvTitle = view.FindViewById<TextView>(Resource.Id.example_row_tv_title);

			var image = GetHeadShot(dog.ImageUrl);
			ivImage.SetImageDrawable(image);
			tvTitle.Text = dog.Name;

			view.Click += (sender, e) => 
			{
				((SwipeListView)parent).OnClickBackView(position);
			};

			return view;
		}

		private Drawable GetHeadShot(string url)
		{
			Drawable headshotDrawable = null;
			try {
				headshotDrawable = Drawable.CreateFromStream(context.Assets.Open(url), null);
			} catch (Exception ex) {
				Android.Util.Log.Debug(GetType().FullName, "Error getting headshot for " + url + ", " + ex.ToString());
				headshotDrawable = null;
			}
			return headshotDrawable;
		}
	}
}


