namespace Mapbox.Unity.Location
{
	using System;
	using Mapbox.Utils;

	public class FixedLocationData : IFixedLocation
	{
		int _locationId;
		public int LocationId
		{
			get
			{
				return _locationId;
			}
		}
		protected Location _currentLocation;
		public Location CurrentLocation
		{
			get
			{
				return _currentLocation;
			}
		}
		protected string _locationName;
		public string LocationName
		{
			get
			{
				return _locationName;
			}
		}

		protected string _locationType;
		public string LocationType
		{
			get
			{
				return _locationType;
			}
		}


		public void SetLocation(int id, string name, string type, Vector2d latitudeLongitude, float heading)
		{
			_locationId = id;
			_locationName = name;
			_locationType = type;
			_currentLocation.Heading = heading;
			_currentLocation.LatitudeLongitude = latitudeLongitude;
			_currentLocation.Accuracy = 1;
			_currentLocation.Timestamp = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

			((SyncronizationPointsLocationProvider)LocationProviderFactory.Instance.FixedLocationProvider).Register(this);
		}
	}
}
