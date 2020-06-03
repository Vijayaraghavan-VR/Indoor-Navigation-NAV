namespace Mapbox.Examples
{
	using Mapbox.Unity.Location;
	using Mapbox.Unity.Utilities;
	using Mapbox.Unity.Map;
	using UnityEngine;
	using UnityEngine.AI;
	using Mapbox.Unity.Ar;
	using System.Collections.Generic;
	using System.Collections;
	using Mapbox.IndoorMappingDemo;

	[RequireComponent(typeof(NavMeshAgent), typeof(LineRenderer))]
	public class RoutingManager : SingletonBehaviour<RoutingManager>
	{
		[SerializeField]
		private AbstractMap _map;

		/// <summary>
		/// The rate at which the transform's position tries catch up to the provided location.
		/// </summary>
		[SerializeField]
		float _positionFollowFactor;

		bool _isInitialized;
		bool _isPathSet = false;
		[SerializeField]
		private GameObject directionPrefab;

		[SerializeField]
		private float tileSpacing = 2;

		[SerializeField]
		private ApplicationUIManager _applicationUIManager;

		private List<GameObject> arrowList = new List<GameObject>();

		float elapsed = 0.0f;

		NavMeshPath path;
		NavMeshAgent _agent;
		LineRenderer _line;

		Location _agentSourceLocation;

		bool _syncPointLocationUpdated = false;
		bool _syncPointAlignmentUpdated = false;

		/// <summary>
		/// The location provider.
		/// This is public so you change which concrete <see cref="T:Mapbox.Unity.Location.ILocationProvider"/> to use at runtime.
		/// </summary>
		ILocationProvider _locationProvider;
		public ILocationProvider LocationProvider
		{
			private get
			{
				if (_locationProvider == null)
				{
					_locationProvider = LocationProviderFactory.Instance.FixedLocationProvider;
				}

				return _locationProvider;
			}
			set
			{
				if (_locationProvider != null)
				{
					_locationProvider.OnLocationUpdated -= LocationProvider_OnLocationUpdated;

				}
				_locationProvider = value;
				_locationProvider.OnLocationUpdated += LocationProvider_OnLocationUpdated;
			}
		}

		[SerializeField]
		FixedLocationSynchronizationContextBehaviour _syncContext;
		public FixedLocationSynchronizationContextBehaviour SyncContext
		{
			private get
			{
				return _syncContext;
			}
			set
			{
				if (_syncContext != null)
				{
					_syncContext.OnAlignmentAvailable -= LocationProvider_OnAlignmentAvailable;

				}
				_syncContext = value;
				_syncContext.OnAlignmentAvailable += LocationProvider_OnAlignmentAvailable;
			}
		}

		Vector3 _targetPosition;

		public Vector3 CurrentTarget
		{
			get
			{
				//return path.corners == null || path.corners.Length < 1 ? Vector3.zero : path.corners[0];

				return _agent.transform.position;
			}
		}

		public override void Awake()
		{
			base.Awake();
			_agent = GetComponent<NavMeshAgent>();
			_line = GetComponent<LineRenderer>();
			_map.OnInitialized += Map_OnInitialized;
			_applicationUIManager.StateChanged += ApplicationUIManager_OnStateChanged;
		}
		void Start()
		{
			LocationProvider.OnLocationUpdated += LocationProvider_OnLocationUpdated;
			DestinationPointLocationProvider.Instance.OnLocationUpdated += DestinationProvider_OnLocationUpdated;
			SyncContext.OnAlignmentAvailable += LocationProvider_OnAlignmentAvailable;
			path = new NavMeshPath();
		}

		void Map_OnInitialized()
		{
			_isInitialized = true;
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			if (LocationProvider != null)
			{
				LocationProvider.OnLocationUpdated -= LocationProvider_OnLocationUpdated;
				DestinationPointLocationProvider.Instance.OnLocationUpdated -= DestinationProvider_OnLocationUpdated;
			}
		}

		void LocationProvider_OnAlignmentAvailable(Alignment alignment)
		{
			Debug.Log("Alignment complete");

			_syncPointAlignmentUpdated = true;
		}

		void LocationProvider_OnLocationUpdated(Location location)
		{
			Debug.Log("Agent location updated " + location.LatitudeLongitude.x + " , " + location.LatitudeLongitude.y);
			if (_isInitialized)
			{
				_agentSourceLocation = location;

				_syncPointLocationUpdated = true;
			}
		}

		void DestinationProvider_OnLocationUpdated(Location location)
		{
			if (_isInitialized)
			{
				_targetPosition = _map.Root.TransformPoint(
					Conversions.GeoToWorldPosition(location.LatitudeLongitude,
												   _map.CenterMercator,
												   _map.WorldRelativeScale).ToVector3xz());

				////Debug sphere to check the position. 
				//var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				//go.transform.localPosition = _targetPosition;
				//_targetPosition = go.transform.position;

				Debug.Log("Agent Destination updated " + _targetPosition);
				_agent.destination = _targetPosition;
				NavMesh.CalculatePath(transform.position, _targetPosition, NavMesh.AllAreas, path);
				DrawPath(path);
				_isPathSet = true;
			}
		}

		void ApplicationUIManager_OnStateChanged(ApplicationState obj)
		{
			if (obj == ApplicationState.SyncPoint_Calibration)
			{
				List<GameObject> arrows = arrowList;
				StartCoroutine(ClearArrows(arrows));
			}
		}

		void DrawPath(NavMeshPath navPath)
		{
			List<GameObject> arrows = arrowList;
			StartCoroutine(ClearArrows(arrows));
			arrowList.Clear();
			//If the path has 1 or no corners, there is no need to draw the line
			if (navPath.corners.Length < 2)
			{
				return;
			}

			// Set the array of positions to the amount of corners...
			_line.positionCount = navPath.corners.Length;
			Quaternion planerot = Quaternion.identity;
			for (int i = 0; i < navPath.corners.Length; i++)
			{
				// Go through each corner and set that to the line renderer's position...
				_line.SetPosition(i, navPath.corners[i]);
				float distance = 0;
				Vector3 offsetVector = Vector3.zero;
				if (i < navPath.corners.Length - 1)
				{
					//plane rotation calculation
					offsetVector = navPath.corners[i + 1] - navPath.corners[i];
					planerot = Quaternion.LookRotation(offsetVector);
					distance = Vector3.Distance(navPath.corners[i + 1], navPath.corners[i]);
					if (distance < tileSpacing)
						continue;

					planerot = Quaternion.Euler(90, planerot.eulerAngles.y, planerot.eulerAngles.z);

					//plane position calculation
					float newSpacing = 0;
					for (int j = 0; j < distance / tileSpacing; j++)
					{
						newSpacing += tileSpacing;
						var normalizedVector = offsetVector.normalized;
						var position = navPath.corners[i] + newSpacing * normalizedVector;
						GameObject go = Instantiate(directionPrefab, position, planerot);
						arrowList.Add(go);
					}
				}
				else
				{
					GameObject go = Instantiate(directionPrefab, navPath.corners[i], planerot);
					arrowList.Add(go);
				}
			}
		}

		private IEnumerator ClearArrows(List<GameObject> arrows)
		{
			if (arrowList.Count == 0)
				yield break;

			foreach (var arrow in arrows)
				Destroy(arrow);
		}

		void Update()
		{
			if (_syncPointLocationUpdated && _syncPointAlignmentUpdated)
			{
				transform.position = _map.Root.TransformPoint(Conversions.GeoToWorldPosition(
															  _agentSourceLocation.LatitudeLongitude,
															  _map.CenterMercator,
															  _map.WorldRelativeScale).ToVector3xz());
				Debug.Log("Agent location position updated " + transform.position.ToString());
				// Need this to place the agent correctly. Otherwise NavMesh complains
				Debug.Log("Agent Placed!");
				_agent.Warp(transform.position);
				_syncPointLocationUpdated = false;
				_syncPointAlignmentUpdated = false;
			}

			if (_agent.hasPath)
			{
				if (Vector3.Distance(_agent.transform.position, Camera.main.transform.position) > 5f)
				{
					_agent.isStopped = true;
				}
				else
				{
					_agent.isStopped = false;
				}
			}

			if (_isPathSet)
				Debug.DrawLine(transform.position, _targetPosition, Color.red);
			elapsed += Time.deltaTime;
			if (elapsed > 1.0f && _isPathSet)
			{
				elapsed -= 1.0f;
				NavMesh.CalculatePath(transform.position, _targetPosition, NavMesh.AllAreas, path);
			}
			if (_isPathSet)
			{
				for (int i = 0; i < path.corners.Length - 1; i++)
					Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red);
			}

		}
	}
}
