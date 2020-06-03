using System.Collections;
using System.Collections.Generic;
using UnityEngine;




namespace Mapbox.Unity.Location
{
	using UnityEngine;
	using System.Collections.Generic;
	using IndoorMappingDemo;
	using System;
	using Mapbox.Utils;

	public class DestinationPointData : IFixedLocation
	{
		private int _locationId;
		public int LocationId
		{
			get
			{
				return _locationId;
			}
		}

		private Vector3 _location;
		public Vector3 Location
		{
			get
			{
				return _location;
			}
		}

		private string _locationName;
		public string LocationName
		{
			get
			{
				return _locationName;
			}
		}

		private string _locationType;
		public string LocationType
		{
			get
			{
				return _locationType;
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

		public void SetLocation(int id, string name, string type, Vector2d latitudeLongitude, float heading)
		{
			_locationId = id;
			_locationName = name;
			_locationType = type;
			_currentLocation.Heading = heading;
			_currentLocation.LatitudeLongitude = latitudeLongitude;
			_currentLocation.Accuracy = 1;
			_currentLocation.Timestamp = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
		}
	}


	public class DestinationPointLocationProvider : MonoBehaviour
	{
		public event Action<Location> OnLocationUpdated = delegate { };

		protected void SendLocation(Location location)
		{
			OnLocationUpdated(location);
		}
		/// <summary>
		/// The singleton instance of this factory.
		/// </summary>
		private static DestinationPointLocationProvider _instance;
		public static DestinationPointLocationProvider Instance
		{
			get
			{
				return _instance;
			}

			private set
			{
				_instance = value;
			}
		}

		private object _syncLock = new object();
		protected Dictionary<int, IFixedLocation> _syncronizationPoints = new Dictionary<int, IFixedLocation>();
		protected Queue<IFixedLocation> _syncronizationPointQueue = new Queue<IFixedLocation>();

		private void Awake()
		{
			if (Instance != null)
			{
				DestroyImmediate(gameObject);
				return;
			}
			Instance = this;
			DontDestroyOnLoad(gameObject);

			if (_syncronizationPoints != null)
			{
				_syncronizationPoints = new Dictionary<int, IFixedLocation>();
			}
			if (_syncronizationPointQueue != null)
			{
				_syncronizationPointQueue = new Queue<IFixedLocation>();
			}
		}

		protected int Count
		{
			get
			{
				lock (_syncLock)
				{
					return _syncronizationPointQueue.Count;
				}
			}
		}

		protected void Enqueue(IFixedLocation locationProvider)
		{
			lock (_syncLock)
			{
				_syncronizationPointQueue.Enqueue(locationProvider);
				//Debug.Log("Enqueue " + _syncronizationPointQueue.Count);
			}
		}

		protected IFixedLocation Dequeue()
		{
			lock (_syncLock)
			{
				//Debug.Log("Dequeue " + _syncronizationPointQueue.Count);
				return _syncronizationPointQueue.Dequeue();
			}
		}

		public void Register(IFixedLocation locationProvider)
		{
			Enqueue(locationProvider);
		}

		private void Update()
		{
			//HACK : To add buttons in increasing order. 

			if (Count < 10)
			{
				return;
			}
			else
			{
				while (Count > 0)
				{
					var locationProvider = Dequeue();
					if (!_syncronizationPoints.ContainsKey(locationProvider.LocationId))
					{
						_syncronizationPoints.Add(locationProvider.LocationId, locationProvider);
					}
				}

				List<int> phoneRoomIds = new List<int>();
				foreach (var id in _syncronizationPoints.Keys)
				{
					Debug.Log("Update Destination");
					if (_syncronizationPoints[id].LocationType != "phone-room")
					{
						ApplicationUIManager.Instance.AddToDestinationPointUI(id, _syncronizationPoints[id].LocationName, _syncronizationPoints[id].LocationType, OnSyncRequested);
					}
					else
					{
						phoneRoomIds.Add(id);
					}
				}

				foreach (var phRoomId in phoneRoomIds)
				{
					ApplicationUIManager.Instance.AddToDestinationPointUI(phRoomId, _syncronizationPoints[phRoomId].LocationName, _syncronizationPoints[phRoomId].LocationType, OnSyncRequested);
				}
			}
		}

		public void OnSyncRequested(int id)
		{
			Debug.Log("Pressed button");
			SendLocation(_syncronizationPoints[id].CurrentLocation);
			ApplicationUIManager.Instance.OnStateChanged(ApplicationState.Destination_Selection);

		}
	}
}

