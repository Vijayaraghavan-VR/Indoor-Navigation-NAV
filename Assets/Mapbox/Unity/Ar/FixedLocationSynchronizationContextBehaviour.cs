using UnityARInterface;
namespace Mapbox.Unity.Ar
{
	using Mapbox.Unity.Map;
	using Mapbox.Unity.Location;
	using UnityEngine;
	using Mapbox.Unity.Utilities;
	using UnityEngine.XR.iOS;
	using System;

	public class FixedLocationSynchronizationContextBehaviour : MonoBehaviour, ISynchronizationContext
	{
		[SerializeField]
		AbstractMap _map;

		[SerializeField]
		Transform _mapCamera;

		[SerializeField]
		Transform ARCamera;

		[SerializeField]
		SyncronizationPointsLocationProvider _locationProvider;

		[SerializeField]
		AbstractAlignmentStrategy _alignmentStrategy;

		float _lastHeight;
		float _lastHeading;

		public event Action<Alignment> OnAlignmentAvailable = delegate { };

		void Awake()
		{
			_alignmentStrategy.Register(this);
			_map.OnInitialized += Map_OnInitialized;
			ARInterface.planeAdded += ARInterface_PlaneAdded;
		}

		void OnDestroy()
		{
			_alignmentStrategy.Unregister(this);
		}

		void Map_OnInitialized()
		{
			_map.OnInitialized -= Map_OnInitialized;
			_locationProvider.OnLocationUpdated += _locationProvider_OnLocationUpdated;
		}

		void _locationProvider_OnLocationUpdated(Unity.Location.Location location)
		{
			var heading = location.Heading;

			var alignment = new Alignment();

			var originalPosition = _map.Root.position;
			float arRotationOffset = ARCamera.eulerAngles.y;
			alignment.Rotation = -heading + arRotationOffset;

			// Rotate our offset by the last heading.
			var rotation = Quaternion.Euler(0, -heading + arRotationOffset, 0);

			Debug.Log("Current Position" + ARCamera.position);

			Vector3 arPositionOffset = ARCamera.position;
			alignment.Position = rotation * (-Conversions.GeoToWorldPosition(location.LatitudeLongitude,
																			_map.CenterMercator,
																			_map.WorldRelativeScale).ToVector3xz());

			alignment.Position += arPositionOffset;
			alignment.Position.y = _lastHeight;
			Debug.Log("Alignment");
			OnAlignmentAvailable(alignment);

			// Reset camera to avoid confusion.
			var mapCameraPosition = Vector3.zero;
			mapCameraPosition.y = _mapCamera.localPosition.y;
			var mapCameraRotation = Vector3.zero;
			mapCameraRotation.x = _mapCamera.localEulerAngles.x;
			_mapCamera.localPosition = mapCameraPosition;
			_mapCamera.eulerAngles = mapCameraRotation;
		}

		void ARInterface_PlaneAdded(UnityARInterface.BoundedPlane obj)
		{
			_lastHeight = obj.center.y;
			IndoorMappingDemo.ApplicationUIManager.Instance.OnStateChanged(IndoorMappingDemo.ApplicationState.AR_Calibration);
			ARInterface.planeAdded -= ARInterface_PlaneAdded;
			Debug.Log("Plane Detected");
			ARInterface.planeAdded -= ARInterface_PlaneAdded;
		}
	}
}