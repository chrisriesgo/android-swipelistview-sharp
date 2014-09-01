using System;
using System.Collections.Generic;

namespace SwipeListViewSample
{
	public static class Dogs
	{
		/// <summary>
		/// Helper method to populate our dog data, 
		/// </summary>
		public static IEnumerable<Dog> GetDogData()
		{
			return new[] {
				new Dog { Name = "Alaskan Malamute", ImageUrl = "images/dogs/alaskan_malamute.jpg" },
				new Dog { Name = "Australian Cattle Dog", ImageUrl = "images/dogs/australian_cattle_dog.jpg" },
				new Dog { Name = "Canaan Dog", ImageUrl = "images/dogs/canaan_dog.jpg" },
				new Dog { Name = "Dalmatian", ImageUrl = "images/dogs/dalmatian.jpg" },
				new Dog { Name = "English Foxhound", ImageUrl = "images/dogs/english_foxhound.jpg" },
				new Dog { Name = "German Shepherd", ImageUrl = "images/dogs/german_shepherd.jpg" },
				new Dog { Name = "Golden Retriever", ImageUrl = "images/dogs/golden_retriever.jpg" },
				new Dog { Name = "Pomeranian", ImageUrl = "images/dogs/pomeranian.jpg" },
				new Dog { Name = "Rhodesian Ridgeback", ImageUrl = "images/dogs/rhodesian_ridgeback.jpg" },
				new Dog { Name = "Rottweiler", ImageUrl = "images/dogs/rottweiler.jpg" },
				new Dog { Name = "Russell Terrier", ImageUrl = "images/dogs/russell_terrier.jpg" },
				new Dog { Name = "Saint Bernard", ImageUrl = "images/dogs/saint_bernard.jpg" },
				new Dog { Name = "Scottish Deerhound", ImageUrl = "images/dogs/scottish_deerhound.jpg" },
				new Dog { Name = "Shiba Inu", ImageUrl = "images/dogs/shiba_inu.jpg" },
				new Dog { Name = "Siberian Husky", ImageUrl = "images/dogs/siberian_husky.jpg" },
				new Dog { Name = "Weimaraner", ImageUrl = "images/dogs/weimaraner.jpg" }
			};
		}
	}
}

