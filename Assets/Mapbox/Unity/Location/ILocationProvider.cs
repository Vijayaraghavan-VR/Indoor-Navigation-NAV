namespace Mapbox.Unity.Location
{
	using System;
	using Mapbox.Utils;

	/// <summary>
	/// Implement ILocationProvider to send Heading and Location updates.
	/// </summary>
	public interface ILocationProvider
	{
		event Action<Location> OnLocationUpdated;
		Location CurrentLocation { get; }
	}

	public interface IFixedLocation
	{
		int LocationId { get; }
		string LocationName { get; }
		string LocationType { get; }
		Location CurrentLocation { get; }
		void SetLocation(int id, string name, string type, Vector2d latitudeLongitude, float heading);
	}
}