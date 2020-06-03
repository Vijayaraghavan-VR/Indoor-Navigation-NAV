namespace Mapbox.Unity.MeshGeneration.Modifiers
{
	using UnityEngine;
	using Mapbox.Utils;
	using Mapbox.Unity.Utilities;
	using Mapbox.Unity.MeshGeneration.Data;
	using Mapbox.Unity.Location;

	[CreateAssetMenu(menuName = "Mapbox/Modifiers/Fixed Location MonoBehaviour Modifier")]
	public class FixedLocationMonoBehaviorModifier : GameObjectModifier
	{
		[SerializeField]
		string _syncLocationIdKey;

		[SerializeField]
		string _syncLocationNameKey;

		[SerializeField]
		string _syncLocationPositionKey;

		[SerializeField]
		string _syncLocationHeadingKey;

		public override void Run(VectorEntity ve, UnityTile tile)
		{
			var fixedLocationProvider = new FixedLocationData();

			var feature = ve.Feature;

			int id = 0;
			float heading = 0.0f;
			string locationName = string.Empty;
			string latitudeLongitudeString;
			Vector2d latitudeLongitude = Vector2d.zero;

			if (feature.Properties.ContainsKey(_syncLocationIdKey))
			{
				if (!int.TryParse(feature.Properties[_syncLocationIdKey].ToString(), out id))
				{
					Debug.Log("No property with key : " + _syncLocationIdKey + "found!");
				}
			}

			if (feature.Properties.ContainsKey(_syncLocationNameKey))
			{
				locationName = feature.Properties[_syncLocationNameKey].ToString();
				if (string.IsNullOrEmpty(locationName))
				{
					Debug.Log("No property with key : " + _syncLocationNameKey + "found!");
				}
			}

			if (feature.Properties.ContainsKey(_syncLocationPositionKey))
			{
				latitudeLongitudeString = feature.Properties[_syncLocationPositionKey].ToString();
				latitudeLongitude = Conversions.StringToLatLon(latitudeLongitudeString);
			}

			if (feature.Properties.ContainsKey(_syncLocationHeadingKey))
			{
				if (!float.TryParse(feature.Properties[_syncLocationHeadingKey].ToString(), out heading))
				{
					Debug.Log("No property with key : " + _syncLocationHeadingKey + "found!");
				}
			}

			fixedLocationProvider.SetLocation(id, locationName, "sync-point", latitudeLongitude, heading);
		}
	}
}
