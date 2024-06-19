using FishNet;
using FishNet.Object;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Runtime.Landmarks.Voodoo;
using _Project.Scripts.Runtime.Landmarks.Zoom;
using _Project.Scripts.Runtime.Networking;
using _Project.Scripts.Runtime.Player;
using _Project.Scripts.Runtime.Utils;
using UnityEngine;
using Logger = _Project.Scripts.Runtime.Utils.Logger;
using Sirenix.Utilities;
using Random = UnityEngine.Random;

public class ProcGenInstanciator : MonoBehaviour
{
    public bool _patxiMode = false;

    [SerializeField] private Vector2 _regionSize;

    [HideIf("@_patxiMode == true"), SerializeField] private NetworkObject _ground;
    [HideIf("@_patxiMode == true"), SerializeField] private NetworkObject _invisibleWall;
    [HideIf("@_patxiMode == true"), SerializeField] private NetworkObject _outerBoundDecorations;
    [HideIf("@_patxiMode == true"), SerializeField] private float _outerBoundDecoDistance;

    [HideIf("@_patxiMode == true"), SerializeField] private NetworkObject _playerPrefab;

    [Title("    Team A")]
    [SerializeField] private ProcGenParameters _teamAParameters;
    [HideIf("@_patxiMode == true"), SerializeField] private NetworkObject _teamAPrefab;
    private List<Vector2> _teamAPoints;

    [Title("    Team B")]
    [SerializeField] private ProcGenParameters _teamBParameters;
    [HideIf("@_patxiMode == true"), SerializeField] private NetworkObject _teamBPrefab;
    private List<Vector2> _teamBPoints;

    [Title("    Landmarks")]
    [SerializeField] private ProcGenParameters _landmarksParameters;
    [SerializeField, ValidateInput("MaxLandmarksSuperiorOrEqualToLandmarkCount", "The number of Landmarks that should " +
        "spawn is less than the Landmarks you can spawn with these parameters " +
        "(increase the max count of some of your landmarks or add more landmarks."
        , InfoMessageType.Warning)]
    public List<SpawnableNetworkObject> _landmarksPrefabList;

    [HideInInspector] public List<Vector2> _landmarksPoints;
    [HideInInspector] public List<NetworkObject> _spawnedLandmarks;

    [Title("    Crowd")]
    [SerializeField] private ProcGenParameters _CrowdParameters;
    [HideIf("@_patxiMode == true"), SerializeField] private NetworkObject _CrowdPrefab;
    private List<Vector2> _CrowdPoints;


    [Title("    Environment")]
    [SerializeField] private ProcGenParameters _light0Parameters;
    [SerializeField] private NetworkObject _light0Prefab;
    private List<Vector2> _light0Points;
    [Space]
    [SerializeField] private ProcGenParameters _light1Parameters;
    [SerializeField] private NetworkObject _light1Prefab;
    private List<Vector2> _light1Points;
    [Space]
    [SerializeField] private ProcGenParameters _light2Parameters;
    [SerializeField] private NetworkObject _light2Prefab;
    private List<Vector2> _light2Points;
    [Space]
    [SerializeField] private ProcGenParameters _light3Parameters;
    [SerializeField] private NetworkObject _light3Prefab;
    private List<Vector2> _light3Points;

    [Space]
    [SerializeField] private ProcGenParameters _FernParameters;
    [SerializeField] private NetworkObject _FernPrefab;
    private List<Vector2> _FernPoints;
    [Space]
    [SerializeField] private ProcGenParameters _Tree1Parameters;
    [SerializeField] private NetworkObject _Tree1Prefab;
    private List<Vector2> _Tree1Points;
    [Space]
    [SerializeField] private ProcGenParameters _Tree2Parameters;
    [SerializeField] private NetworkObject _Tree2Prefab;
    private List<Vector2> _Tree2Points;
    [Space]
    [SerializeField] private ProcGenParameters _Tree3Parameters;
    [SerializeField] private NetworkObject _Tree3Prefab;
    private List<Vector2> _Tree3Points;
    [Space]
    [SerializeField] private ProcGenParameters _deco0Parameters;
    [SerializeField] private NetworkObject _deco0Prefab= null;
    private List<Vector2> _deco0Points;
    [Space]
    [SerializeField] private ProcGenParameters _deco1Parameters;
    [SerializeField] private NetworkObject _deco1Prefab = null;
    private List<Vector2> _deco1Points;
    [Space]
    [SerializeField] private ProcGenParameters _deco2Parameters;
    [SerializeField] private NetworkObject _deco2Prefab = null;
    private List<Vector2> _deco2Points;
    [Space]
    [SerializeField] private ProcGenParameters _deco3Parameters;
    [SerializeField] private NetworkObject _deco3Prefab = null;
    private List<Vector2> _deco3Points;
    [Space]
    [SerializeField] private ProcGenParameters _deco4Parameters;
    [SerializeField] private NetworkObject _deco4Prefab = null;
    private List<Vector2> _deco4Points;
    [Space]
    [SerializeField] private ProcGenParameters _deco5Parameters;
    [SerializeField] private NetworkObject _deco5Prefab = null;
    private List<Vector2> _deco5Points;
    [Space]
    [SerializeField] private ProcGenParameters _deco6Parameters;
    [SerializeField] private NetworkObject _deco6Prefab = null;
    private List<Vector2> _deco6Points;
    [Space]
    [SerializeField] private ProcGenParameters _deco7Parameters;
    [SerializeField] private NetworkObject _deco7Prefab = null;
    private List<Vector2> _deco7Points;
    [Space]
    [SerializeField] private ProcGenParameters _deco8Parameters;
    [SerializeField] private NetworkObject _deco8Prefab = null;
    private List<Vector2> _deco8Points;
    [Space]
    [SerializeField] private ProcGenParameters _deco9Parameters;
    [SerializeField] private NetworkObject _deco9Prefab = null;
    private List<Vector2> _deco9Points;
    [Space]
    [SerializeField] private ProcGenParameters _deco10Parameters;
    [SerializeField] private NetworkObject _deco10Prefab = null;
    private List<Vector2> _deco10Points;
    [Space]
    [SerializeField] private ProcGenParameters _deco11Parameters;
    [SerializeField] private NetworkObject _deco11Prefab = null;
    private List<Vector2> _deco11Points;
    [Space]
    [SerializeField] private ProcGenParameters _deco12Parameters;
    [SerializeField] private NetworkObject _deco12Prefab = null;
    private List<Vector2> _deco12Points;
    [Space]
    [SerializeField] private ProcGenParameters _deco13Parameters;
    [SerializeField] private NetworkObject _deco13Prefab = null;
    private List<Vector2> _deco13Points;
    [Space]
    [SerializeField] private ProcGenParameters _deco14Parameters;
    [SerializeField] private NetworkObject _deco14Prefab = null;
    private List<Vector2> _deco14Points;
    [Space]
    [SerializeField] private ProcGenParameters _deco15Parameters;
    [SerializeField] private NetworkObject _deco15Prefab = null;
    private List<Vector2> _deco15Points;
    

    private List<List<Vector2>> _alreadySpawnedPoints = new();
    private List<float> _alreadySpawnedPointsRadius = new();
    private List<float> _alreadySpawnedPointsEdgeDistance = new();
    private List<Vector2> _alreadySpawnedPointsRegionSize = new();
    private List<NetworkObject> _spawnedObjects = new(); // used for regenerating the maps by despawning all the objects

    private bool _readyToSpawnPrefabs = false;
    private readonly int _framesBetweenSpawn = 1; // to avoid blocking the main thread, we launch the main method in a coroutine and wait between each spawn
    private List<SpawnableNetworkObject> _copyLandmarksPrefabList; // make a copy of the list to avoid modifying the original list for regenerating the map
    
    // Events
    public event Action OnBeginMapGeneration;
    public event Action OnMapGenerated;
    public event Action OnBeginPrefabSpawning;
    public event Action OnPrefabSpawned;
    public event Action<float, string> OnLoadingProgressChanged;

    private void Awake()
    {
        VerifyPrefabSetup();
        _copyLandmarksPrefabList = _landmarksPrefabList.Select(x => (SpawnableNetworkObject)x.Clone()).ToList();
    }

    [Button]
    public IEnumerator GenerateMap()
    {
        Logger.LogDebug("Generating map...", Logger.LogType.Server, this);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // clone the list to avoid modifying the original list
        _landmarksPrefabList = _copyLandmarksPrefabList.Select(x => (SpawnableNetworkObject)x.Clone()).ToList();
        
        // Remove Landmark_Zoom if not allowed
        if (!GameManager.Instance.CanLandmarkZoomSpawnFromGameSettings)
        {
            foreach (var prefab in _landmarksPrefabList.Where(prefab => prefab.Object.name == nameof(Landmark_Zoom)))
            {
                _landmarksPrefabList.Remove(prefab);
                Logger.LogDebug("Landmark_Zoom removed from the list because it's not allowed to spawn", Logger.LogType.Server, this);
                break;
            }
        }
        
        // Remove Landmark_Voodoo if not allowed
        if (!GameManager.Instance.CanLandmarkVoodooSpawnFromGameSettings)
        {
            foreach (var prefab in _landmarksPrefabList.Where(prefab => prefab.Object.name == nameof(Landmark_Voodoo)))
            {
                _landmarksPrefabList.Remove(prefab);
                Logger.LogDebug("Landmark_Voodoo removed from the list because it's not allowed to spawn", Logger.LogType.Server, this);
                break;
            }
        }
        
        
        _alreadySpawnedPoints.Clear();
        _alreadySpawnedPointsRadius.Clear();
        _alreadySpawnedPointsEdgeDistance.Clear();
        _alreadySpawnedPointsRegionSize.Clear();
        _readyToSpawnPrefabs = false;
        
        if (_spawnedObjects.Count > 0)
        {
            Logger.LogDebug("Despawning all objects because we are regenerating the maps", Logger.LogType.Server, this);
            foreach (var obj in _spawnedObjects)
            {
                InstanceFinder.ServerManager.Despawn(obj);
            }
            _spawnedObjects.Clear();
        }
        
        if(_spawnedLandmarks.Count > 0)
        {
            Logger.LogDebug("Despawning all landmarks because we are regenerating the maps", Logger.LogType.Server, this);
            foreach (var obj in _spawnedLandmarks)
            {
                InstanceFinder.ServerManager.Despawn(obj);
            }
            _spawnedLandmarks.Clear();
        }
        
        OnBeginMapGeneration?.Invoke(); 
        
        yield return GenerateTerrain();
        
        OnLoadingProgressChanged?.Invoke(2/10f, "Generating players points");
        _teamAPoints = GeneratePoints(_teamAParameters, true, true, true);
        _teamBPoints = GeneratePoints(_teamBParameters, true, true, true);
        OnLoadingProgressChanged?.Invoke(3/10f, "Generating landmarks points");
        _landmarksPoints = GeneratePoints(_landmarksParameters, true, true, true);
        Logger.LogDebug("IMPORTANT STUFF generated in " + stopwatch.ElapsedMilliseconds + "ms", Logger.LogType.Server, this);

        OnLoadingProgressChanged?.Invoke(5 / 10f, "Generating crowd points");
        _CrowdPoints = GeneratePoints(_CrowdParameters, false, false, true);

        // Decoration
        OnLoadingProgressChanged?.Invoke(4/10f, "Generating environment points");
        _FernPoints = GeneratePoints(_FernParameters, false, false, true, true);
        _Tree1Points = GeneratePoints(_Tree1Parameters, false, true, true, true);
        _Tree2Points = GeneratePoints(_Tree2Parameters, false, true, true, true);
        _Tree3Points = GeneratePoints(_Tree3Parameters, false, true, true, true);
        //_testLandmarkPoints = GeneratePoints(_testLandmarkParameters, false, true, true);

        _light0Points = GeneratePoints(_light0Parameters, false, false, true, true);
        _light1Points = GeneratePoints(_light1Parameters, false, false, true, true);
        _light2Points = GeneratePoints(_light2Parameters, false, false, true, true);
        _light3Points = GeneratePoints(_light3Parameters, false, false, true, true) ;
        Logger.LogDebug("Light generated in " + stopwatch.ElapsedMilliseconds + "ms", Logger.LogType.Server, this);

        _deco0Points = GeneratePoints(_deco0Parameters, false, true, true, true);
        Logger.LogDebug("Dec0 generated in " + stopwatch.ElapsedMilliseconds + "ms", Logger.LogType.Server, this);
        _deco1Points = GeneratePoints(_deco1Parameters, false, true, true, true);
        Logger.LogDebug("Dec1 generated in " + stopwatch.ElapsedMilliseconds + "ms", Logger.LogType.Server, this);
        _deco2Points = GeneratePoints(_deco2Parameters, false, true, true, true);
        Logger.LogDebug("Dec2 generated in " + stopwatch.ElapsedMilliseconds + "ms", Logger.LogType.Server, this);
        _deco3Points = GeneratePoints(_deco3Parameters, false, true, true, true);
        Logger.LogDebug("Dec3 generated in " + stopwatch.ElapsedMilliseconds + "ms", Logger.LogType.Server, this);
        _deco4Points = GeneratePoints(_deco4Parameters, false, true, true, true);
        Logger.LogDebug("Dec4 generated in " + stopwatch.ElapsedMilliseconds + "ms", Logger.LogType.Server, this);
        _deco5Points = GeneratePoints(_deco5Parameters, false, true, true, true);
        Logger.LogDebug("Dec5 generated in " + stopwatch.ElapsedMilliseconds + "ms", Logger.LogType.Server, this);
        _deco6Points = GeneratePoints(_deco6Parameters, false, true, true, true);
        Logger.LogDebug("Dec6 generated in " + stopwatch.ElapsedMilliseconds + "ms", Logger.LogType.Server, this);
        _deco7Points = GeneratePoints(_deco7Parameters, false, true, true, true);
        Logger.LogDebug("Dec7 generated in " + stopwatch.ElapsedMilliseconds + "ms", Logger.LogType.Server, this);
        _deco8Points = GeneratePoints(_deco8Parameters, false, true, true, true);
        Logger.LogDebug("Dec8 generated in " + stopwatch.ElapsedMilliseconds + "ms", Logger.LogType.Server, this);
        _deco9Points = GeneratePoints(_deco9Parameters, false, true, true, true);
        Logger.LogDebug("Dec9 generated in " + stopwatch.ElapsedMilliseconds + "ms", Logger.LogType.Server, this);
        _deco10Points = GeneratePoints(_deco10Parameters, false, true, true, true);
        Logger.LogDebug("Dec10 generated in " + stopwatch.ElapsedMilliseconds + "ms", Logger.LogType.Server, this);
        _deco11Points = GeneratePoints(_deco11Parameters, false, true, true, true);
        Logger.LogDebug("Dec11 generated in " + stopwatch.ElapsedMilliseconds + "ms", Logger.LogType.Server, this);
        _deco12Points = GeneratePoints(_deco12Parameters, false, true, true, true);
        Logger.LogDebug("Dec12 generated in " + stopwatch.ElapsedMilliseconds + "ms", Logger.LogType.Server, this);
        _deco13Points = GeneratePoints(_deco13Parameters, false, true, true, true);
        Logger.LogDebug("Dec13 generated in " + stopwatch.ElapsedMilliseconds + "ms", Logger.LogType.Server, this);
        _deco14Points = GeneratePoints(_deco14Parameters, false, true, true , true);
        Logger.LogDebug("Dec14 generated in " + stopwatch.ElapsedMilliseconds + "ms", Logger.LogType.Server, this);
        _deco15Points = GeneratePoints(_deco15Parameters, false, true, true, true);

        _readyToSpawnPrefabs = true;
        
        OnMapGenerated?.Invoke();
        stopwatch.Stop();
        Logger.LogDebug("Map generated in " + stopwatch.ElapsedMilliseconds + "ms", Logger.LogType.Server, this);
    }

    private IEnumerator GenerateTerrain()
    {
        OnLoadingProgressChanged?.Invoke(1/10f, "Generating terrain");
        // Generate Map ground
        NetworkObject ground = Instantiate(_ground, new Vector3(_regionSize.x / 2, -1f, _regionSize.y / 2), Quaternion.identity);
        ground.transform.localScale = new Vector3(_regionSize.x / 10 + _regionSize.x / 25, 1, _regionSize.y / 10 + _regionSize.x / 25);
        InstanceFinder.ServerManager.Spawn(ground);

        // Generate Map invisible wall boundings
        // North
        NetworkObject wallNorth = Instantiate(_invisibleWall, new Vector3(_regionSize.x / 2, 1, _regionSize.y + 1), Quaternion.identity);
        wallNorth.transform.localScale = new Vector3(_regionSize.x + 1, 4, 1);
        InstanceFinder.ServerManager.Spawn(wallNorth);

        // South
        NetworkObject wallSouth = Instantiate(_invisibleWall, new Vector3(_regionSize.x / 2, 1, -1), Quaternion.identity);
        wallSouth.transform.localScale = new Vector3(_regionSize.x + 1, 4, 1);
        InstanceFinder.ServerManager.Spawn(wallSouth);

        // East
        NetworkObject wallEast = Instantiate(_invisibleWall, new Vector3(_regionSize.y + 1, 1, _regionSize.y / 2), Quaternion.identity);
        wallEast.transform.localScale = new Vector3(1, 4, _regionSize.y + 1);
        InstanceFinder.ServerManager.Spawn(wallEast);

        // West
        NetworkObject wallWest = Instantiate(_invisibleWall, new Vector3(-1, 1, _regionSize.y / 2), Quaternion.identity);
        wallWest.transform.localScale = new Vector3(1, 4, _regionSize.y + 1);
        InstanceFinder.ServerManager.Spawn(wallWest);
        
        _spawnedObjects.Add(ground);
        _spawnedObjects.Add(wallNorth);
        _spawnedObjects.Add(wallSouth);
        _spawnedObjects.Add(wallEast);
        _spawnedObjects.Add(wallWest);

        // Generate map decoratives borders
        // North
        NetworkObject decoNorth = Instantiate(_outerBoundDecorations, new Vector3((_regionSize.x / 2), 0, _regionSize.y +_outerBoundDecoDistance ), Quaternion.identity);
      
        InstanceFinder.ServerManager.Spawn(decoNorth);

        // South
        NetworkObject decoSouth = Instantiate(_outerBoundDecorations, new Vector3(_regionSize.x / 2, 0, -_outerBoundDecoDistance), Quaternion.identity);
        decoSouth.transform.eulerAngles = new Vector3(0, 180, 0);
        InstanceFinder.ServerManager.Spawn(decoSouth);

        // East
        NetworkObject decoEast = Instantiate(_outerBoundDecorations, new Vector3(_regionSize.y + _outerBoundDecoDistance, 0, _regionSize.y / 2), Quaternion.identity);
        decoEast.transform.eulerAngles = new Vector3(0, 90, 0);
        InstanceFinder.ServerManager.Spawn(decoEast);

        // West
        NetworkObject decoWest = Instantiate(_outerBoundDecorations, new Vector3(-_outerBoundDecoDistance, 0, _regionSize.y / 2), Quaternion.identity);
        decoWest.transform.eulerAngles = new Vector3(0, 270, 0);
        InstanceFinder.ServerManager.Spawn(decoWest);

        _spawnedObjects.Add(decoNorth);
        _spawnedObjects.Add(decoSouth);
        _spawnedObjects.Add(decoEast);
        _spawnedObjects.Add(decoWest);

        yield return new WaitForFrames(_framesBetweenSpawn);
    }

    /// <summary>
    /// Generate points for object to spawn
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="forceExactNumber"> If we want the exact same number of elements spawn than specified in parameters</param>
    /// <param name="addToAlreadySpawnedPoints"> If we want other objects to consider the presence of these object to be taken into consideration when spawning them</param>
    /// <param name="considerAlreadySpawnedObjects">If we want these object to not spawn on other objects (where addToAlreadySpawnedPoints was used for them) </param>
    /// <returns></returns>
    private List<Vector2> GeneratePoints(ProcGenParameters parameters, bool forceExactNumber, bool addToAlreadySpawnedPoints, bool considerAlreadySpawnedObjects, bool ignoreSafeValues = false)
    {
        int sampleBeforeRejection = 720;
        int maxFailAttemps = 3000;

        Vector2 newRegionSize = _regionSize;
        newRegionSize.x *= (100 - parameters._edgeDistance) / 100;
        newRegionSize.y *= (100 - parameters._edgeDistance) / 100;

        List<Vector2> points;

        if (considerAlreadySpawnedObjects && !_alreadySpawnedPoints.IsNullOrEmpty())
        {
            // We create a copy of AlreadySpawnedPoints but the point are centered arround the region size we are trying to spawn our new points. (Hard to explain sorry)
            List<List<Vector2>> tmpAlreadySpawnedPoints = new();
            List<float> tmpAlreadySpawnedPointsRadius = new();


            for (int i = 0; i < _alreadySpawnedPoints.Count; i++)
            {
                tmpAlreadySpawnedPoints.Add(new List<Vector2>());
                tmpAlreadySpawnedPointsRadius.Add(_alreadySpawnedPointsRadius[i] + parameters._distanceWithOtherObjects);

                Vector2 centerOffset = newRegionSize / 2 - _alreadySpawnedPointsRegionSize[i] / 2;
                for (int j = 0; j < _alreadySpawnedPoints[i].Count; j++)
                {
                    Vector2 tmpPoint = _alreadySpawnedPoints[i][j];
                    // We recenter every points
                    tmpPoint += centerOffset;
                    tmpAlreadySpawnedPoints[i].Add(tmpPoint);
                }
            }

            points = PoissonDiscSampling.GenerateExactNumberOfPoints(parameters._minDistance, parameters._maxDistance, newRegionSize, parameters._numOfPoints, tmpAlreadySpawnedPoints, tmpAlreadySpawnedPointsRadius, sampleBeforeRejection, maxFailAttemps);
        }
        else
        {
            points = PoissonDiscSampling.GenerateExactNumberOfPoints(parameters._minDistance, parameters._maxDistance, newRegionSize, parameters._numOfPoints, sampleBeforeRejection, maxFailAttemps);
        }

        if (points.Count < parameters._numOfPoints && forceExactNumber)
        {
            Logger.LogWarning("Not enough points, something went wrong? \n Number of spawned objects : " + points.Count, Logger.LogType.Server, this);
        }

        if (addToAlreadySpawnedPoints)
        {

            List<Vector2> test = new();
            for (int i = 0; i < points.Count; i++)
            {
                test.Add(points[i]);
            }

            _alreadySpawnedPoints.Add(test);
            _alreadySpawnedPointsRadius.Add(parameters._distanceWithOtherObjects);
            _alreadySpawnedPointsRegionSize.Add(newRegionSize);
            _alreadySpawnedPointsEdgeDistance.Add(parameters._edgeDistance);
        }
 

        for (int i = 0; i < points.Count; i++)
        {
            Vector2 point = points[i];
            point.x += (parameters._edgeDistance / 100 * _regionSize.x) / 2;
            point.y += (parameters._edgeDistance / 100 * _regionSize.y) / 2;
            points[i] = point;
        }
        return points;
    }

    private IEnumerator SpawnPrefabs(List<Vector2> pointsLocation, NetworkObject prefab)
    {
        for (int i = 0; i < pointsLocation.Count; i++)
        {
            NetworkObject pref = Instantiate(prefab, new Vector3(pointsLocation[i].x, 0, pointsLocation[i].y), Quaternion.identity);
            pref.transform.Rotate(new Vector3(0, UnityEngine.Random.Range(0, 360), 0));
            InstanceFinder.ServerManager.Spawn(pref);
            _spawnedObjects.Add(pref);
        }
        yield return null;
    }

    // Tries to spawns each prefabs respecting the min and max amount specified
    private IEnumerator SpawnPrefabs(List<Vector2> pointsLocation, List<SpawnableNetworkObject> SNOList)
    {
        List<SpawnableNetworkObject> minList = new();

        // We create a list with min*element there is in our SNOList
        foreach (var SNO in SNOList)
        {
            for (int i = 0; i < SNO.Min; i++)
            {
                minList.Add(SNO);
            }
        }

        int spawnIndex = 0;

        // For each element of our minList, we spawn its prefab the it and tell the SNOList we spawned it
        while (!minList.IsNullOrEmpty() && spawnIndex < pointsLocation.Count)
        {
            int random = Random.Range(0, minList.Count - 1);
            NetworkObject prefab = minList[random].Object;
            minList.RemoveAt(random);

            foreach (var SNO in SNOList)
            {
                if (SNO.Object == prefab)
                {
                    SNO.Min -= 1;
                    SNO.Max -= 1;
                    if (SNO.Max == 0)
                    {
                        SNOList.Remove(SNO);
                    }
                    break;
                }
            }

            NetworkObject pref = Instantiate(prefab, new Vector3(pointsLocation[spawnIndex].x, -1, pointsLocation[spawnIndex].y), Quaternion.identity);
            _spawnedLandmarks.Add(pref);
            pref.transform.Rotate(new Vector3(0, Random.Range(0, 360), 0));
            InstanceFinder.ServerManager.Spawn(pref);
            spawnIndex++;
            yield return new WaitForFrames(_framesBetweenSpawn);
        }

        // Then we spawn the remaining elements from the SNOList (considering the max attributes)
        while (!SNOList.IsNullOrEmpty() && spawnIndex < pointsLocation.Count)
        {
            int random = Random.Range(0, SNOList.Count - 1);
            NetworkObject prefab = SNOList[random].Object;
            SNOList[random].Max -= 1;

            if (SNOList[random].Max == 0)
            {
                SNOList.RemoveAt(random);
            }

            NetworkObject pref = Instantiate(prefab, new Vector3(pointsLocation[spawnIndex].x, 0, pointsLocation[spawnIndex].y), Quaternion.identity);
            _spawnedLandmarks.Add(pref);
            pref.transform.Rotate(new Vector3(0, Random.Range(0, 360), 0));
            InstanceFinder.ServerManager.Spawn(pref);

            spawnIndex++;
        }

        if (spawnIndex < pointsLocation.Count)
        {
            Debug.LogWarning("Max number of prefab too low, spawned less landmarks than anticipated, spawned : " + spawnIndex + " instead of : " + pointsLocation.Count);
        }
    }



    [Button, HideIf("@_readyToSpawnPrefabs == false")]
    public void SpawnAllPrefabs()
    {
        StartCoroutine(SpawnAllPrefabsCoroutine());
    }

    public IEnumerator SpawnAllPrefabsCoroutine()
    {
        Logger.LogDebug("Spawning prefabs...", Logger.LogType.Server, this);
        OnBeginPrefabSpawning?.Invoke();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        if (_readyToSpawnPrefabs == false)
        {
            Debug.LogError("You must generate spawn points before trying to spawn them");
            yield break;
        }
        OnLoadingProgressChanged?.Invoke(6/10f, "Spawning players 1/2");
        yield return SpawnPrefabs(_teamAPoints, _teamAPrefab);
        OnLoadingProgressChanged?.Invoke(7/10f, "Spawning players 2/2");
        yield return SpawnPrefabs(_teamBPoints, _teamBPrefab);
        OnLoadingProgressChanged?.Invoke(8/10f, "Spawning landmarks");
        yield return SpawnPrefabs(_landmarksPoints, _landmarksPrefabList);
        //yield return SpawnPrefabs(_testLandmarkPoints, _testLandmarkPrefab);
        OnLoadingProgressChanged?.Invoke(9/10f, "Spawning environment");
        
        if (_FernPrefab) yield return SpawnPrefabs(_FernPoints, _FernPrefab);
        if (_Tree1Prefab) yield return SpawnPrefabs(_Tree1Points, _Tree1Prefab);
        if (_Tree2Prefab) yield return SpawnPrefabs(_Tree2Points, _Tree2Prefab);
        if (_Tree3Prefab) yield return SpawnPrefabs(_Tree3Points, _Tree3Prefab);
        if (_light0Prefab) yield return SpawnPrefabs(_light0Points, _light0Prefab);
        if (_light1Prefab) yield return SpawnPrefabs(_light1Points, _light1Prefab);
        if (_light2Prefab) yield return SpawnPrefabs(_light2Points, _light2Prefab);
        if (_light3Prefab) yield return SpawnPrefabs(_light3Points, _light3Prefab);

        if (_deco0Prefab) yield return SpawnPrefabs(_deco0Points, _deco0Prefab);
        if (_deco1Prefab) yield return SpawnPrefabs(_deco1Points, _deco1Prefab);
        if (_deco2Prefab) yield return SpawnPrefabs(_deco2Points, _deco2Prefab);
        if (_deco3Prefab) yield return SpawnPrefabs(_deco3Points, _deco3Prefab);
        if (_deco4Prefab) yield return SpawnPrefabs(_deco4Points, _deco4Prefab);
        if (_deco5Prefab) yield return SpawnPrefabs(_deco5Points, _deco5Prefab);
        if (_deco6Prefab) yield return SpawnPrefabs(_deco6Points, _deco6Prefab);
        if (_deco7Prefab) yield return SpawnPrefabs(_deco7Points, _deco7Prefab);
        if (_deco8Prefab) yield return SpawnPrefabs(_deco8Points, _deco8Prefab);
        if (_deco9Prefab) yield return SpawnPrefabs(_deco9Points, _deco9Prefab);
        if (_deco10Prefab) yield return SpawnPrefabs(_deco10Points, _deco10Prefab);
        if (_deco11Prefab) yield return SpawnPrefabs(_deco11Points, _deco11Prefab);
        if (_deco12Prefab) yield return SpawnPrefabs(_deco12Points, _deco12Prefab);
        if (_deco13Prefab) yield return SpawnPrefabs(_deco13Points, _deco13Prefab);
        if (_deco14Prefab) yield return SpawnPrefabs(_deco14Points, _deco14Prefab);
        if (_deco15Prefab) yield return SpawnPrefabs(_deco15Points, _deco15Prefab);

        OnLoadingProgressChanged?.Invoke(10/10f, "Spawning crowd");
        yield return SpawnPrefabs(_CrowdPoints, _CrowdPrefab);
        OnPrefabSpawned?.Invoke();
        stopwatch.Stop();
        Logger.LogDebug("Prefabs spawned in " + stopwatch.ElapsedMilliseconds + "ms", Logger.LogType.Server, this);
    }

    private void VerifyPrefabSetup()
    {
        if (!_ground) Logger.LogError("Ground prefab is missing");
        if (!_invisibleWall) Logger.LogError("Invisible wall prefab is missing");
        if (!_playerPrefab) Logger.LogError("Player prefab is missing");
        if (!_teamAPrefab) Logger.LogError("Team A prefab is missing");
        if (!_teamBPrefab) Logger.LogError("Team B prefab is missing");
        if (_landmarksPrefabList.IsNullOrEmpty()) Logger.LogError("Landmark prefabs are missing");
        if (!_FernPrefab) Logger.LogError("Fern prefab is missing");
        if (!_Tree1Prefab) Logger.LogError("Tree prefab is missing");
        if (!_Tree2Prefab) Logger.LogError("Tree prefab is missing");
        if (!_Tree3Prefab) Logger.LogError("Tree prefab is missing");
        //if (!_testLandmarkPrefab) Logger.LogError("Test Landmark prefab is missing");
        if (!_light0Prefab) Logger.LogWarning("Light 0 prefab is missing, ignore if this is intended");
        if (!_light1Prefab) Logger.LogWarning("Light 1 prefab is missing, ignore if this is intended");
        if (!_light2Prefab) Logger.LogWarning("Light 2 prefab is missing, ignore if this is intended");
        if (!_light3Prefab) Logger.LogWarning("Light 3 prefab is missing, ignore if this is intended");
        if (!_CrowdPrefab) Logger.LogError("Crowd prefab is missing, can't spawn NPCs");
    }

    [Button]
    public void PlacePlayers()
    {
        Logger.LogDebug("Placing players...", Logger.LogType.Server, this);
        GameObject[] playersTeamA = PlayerManager.Instance.GetNetworkPlayers(PlayerTeamType.A).Select(player => player.gameObject).ToArray();
        GameObject[] playersTeamB = PlayerManager.Instance.GetNetworkPlayers(PlayerTeamType.B).Select(player => player.gameObject).ToArray();

        for (int i = 0; i < playersTeamA.Length; i++)
        {
            playersTeamA[i].GetComponent<PlayerController>().Teleport(new Vector3(_teamAPoints[i].x, .25f, _teamAPoints[i].y));
        }
        for (int i = 0; i < playersTeamB.Length; i++)
        {
            playersTeamB[i].GetComponent<PlayerController>().Teleport(new Vector3(_teamBPoints[i].x, .25f, _teamBPoints[i].y));
        }
    }

    public void GenerateNewPlayerPoints()
    {
        _teamAPoints = GeneratePoints(_teamAParameters, true, false, true);
        _teamBPoints = GeneratePoints(_teamBParameters, true, false, true);
    }

    private bool MaxLandmarksSuperiorOrEqualToLandmarkCount()
    {
        int sum = 0;

        foreach (var SNO in _landmarksPrefabList)
        {
            sum += SNO.Max;
        }

        bool res = (sum >= _landmarksParameters._numOfPoints);

        return res;
    }
}

[System.Serializable]
public class SpawnableNetworkObject : ICloneable
{
    [HideLabel] public NetworkObject Object;

    [HorizontalGroup("MinMax", Width = 0.4f), MinValue(0)]
    public int Min;

    [HorizontalGroup("MinMax", Width = 0.4f), MinValue("@Min"), PropertySpace(SpaceBefore = 0, SpaceAfter = 15)]
    public int Max;

    public object Clone()
    {
        return new SpawnableNetworkObject
        {
            Object = Object,
            Min = Min,
            Max = Max
        };
    }
}