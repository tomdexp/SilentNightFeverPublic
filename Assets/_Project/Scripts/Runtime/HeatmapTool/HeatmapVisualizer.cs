using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatmapVisualizer : MonoBehaviour
{
    [SerializeField, AssetSelector(Filter = ("GameInfos"))] private TextAsset _dataFile;

    private GameInfos _gameInfos = null;

    [SerializeField, OnValueChanged("GenerateHeatmap"), ShowIf("@_dataFile"), PropertySpace(spaceAfter:5, spaceBefore:20), HideInEditorMode()] 
    private bool _progressiveHeatmap = true;



    private RoundInfos _round1Infos = null;
    private List<List<BoxColorPair>> _heatmapval1 = new();

    private RoundInfos _round2Infos = null;
    private List<List<BoxColorPair>> _heatmapval2 = new();

    private RoundInfos _round3Infos = null;
    private List<List<BoxColorPair>> _heatmapval3 = new();

    private RoundInfos _round4Infos = null;
    private List<List<BoxColorPair>> _heatmapval4 = new();

    private RoundInfos _round5Infos = null;
    private List<List<BoxColorPair>> _heatmapval5 = new();

    [TabGroup("Round 1", VisibleIf = "_dataFile")]
    [SerializeField, PropertyRange(0, "getRound1Duration"), OnValueChanged("GenerateHeatmap"), HideInEditorMode()] 
    private int _round1Time;

    [TabGroup("Round 2")]
    [SerializeField, PropertyRange(0, "getRound2Duration"), OnValueChanged("GenerateHeatmap"), HideInEditorMode()] 
    private int _round2Time;

    [TabGroup("Round 3")]
    [SerializeField, PropertyRange(0, "getRound3Duration"), OnValueChanged("GenerateHeatmap"), HideInEditorMode()] 
    private int _round3Time;

    [TabGroup("Round 4")]
    [SerializeField, PropertyRange(0, "getRound4Duration"), HideIf("@_gameInfos?.RoundInfo?.Count < 4"), OnValueChanged("GenerateHeatmap"), HideInEditorMode()] 
    private int _round4Time;

    [TabGroup("Round 5")]
    [SerializeField, PropertyRange(0, "getRound5Duration"), HideIf("@_gameInfos?.RoundInfo?.Count < 5"), OnValueChanged("GenerateHeatmap"), HideInEditorMode()] 
    private int _round5Time;

    #region GetRoundsDurationFunctions
    private int getRound1Duration()
    {
        // Try catch because weird errors I couldn't fix
        try
        {
            return (int)Mathf.Floor(((int)_round1Infos.PlayerInfos.Count - 1) * _round1Infos.timeInterval);
        }
        catch
        {
            return 0;
        }
    }

    private int getRound2Duration()
    {
        try
        {
            return (int)Mathf.Floor(((int)_round2Infos.PlayerInfos.Count - 1) * _round2Infos.timeInterval);
        }
        catch
        {
            return 0;
        }
    }

    private int getRound3Duration()
    {
        try
        {
            return (int)Mathf.Floor(((int)_round3Infos.PlayerInfos.Count - 1) * _round3Infos.timeInterval);

        }
        catch
        {
            return 0;
        }
    }

    private int getRound4Duration()
    {
        try
        {
            return (int)Mathf.Floor(((int)_round4Infos.PlayerInfos.Count - 1) * _round4Infos.timeInterval);

        }
        catch
        {
            return 0;
        }
    }

    private int getRound5Duration()
    {
        try
        {
            return (int)Mathf.Floor(((int)_round5Infos.PlayerInfos.Count - 1) * _round5Infos.timeInterval);

        }
        catch
        {
            return 0;
        }
    }
    #endregion

    // TODO : Change to have the json record the size of the map
    private int MAP_SIZE = 125;

    private int _currentHeatMap = 1;
    private bool _replayIsPlaying = false;

    private void Start()
    {
        string json = _dataFile.text;
        _gameInfos = JsonUtility.FromJson<GameInfos>(json);
        _round1Infos = _gameInfos.RoundInfo[0];
        _round2Infos = _gameInfos.RoundInfo[1];
        _round3Infos = _gameInfos.RoundInfo[2];

        if (_gameInfos.RoundInfo.Count < 4) { return; }
        _round4Infos = _gameInfos.RoundInfo[3];
        if (_gameInfos.RoundInfo.Count < 5) { return; }
        _round5Infos = _gameInfos.RoundInfo[4];

    }

    private void OnDestroy()
    {
        _round1Time = 0;
        _round2Time = 0;
        _round3Time = 0;
        _round4Time = 0;
        _round5Time = 0;

        _gameInfos = null;
    }

    #region GenerateRoundsHeatMap
    [TabGroup("Round 1")]
    [Button("Generate Heatmap"), HideInEditorMode()]
    private void GenerateRound1Heatmap()
    {

        _currentHeatMap = 1;
        GenerateHeatmap();
    }

    [TabGroup("Round 2")]
    [Button("Generate Heatmap"), HideInEditorMode()]
    private void GenerateRound2Heatmap()
    {
        _currentHeatMap = 2;
        GenerateHeatmap();
    }

    [TabGroup("Round 3")]
    [Button("Generate Heatmap"), HideInEditorMode()]
    private void GenerateRound3Heatmap()
    {
        _currentHeatMap = 3;
        GenerateHeatmap();
    }


    [TabGroup("Round 4")]
    [Button("Generate Heatmap"), HideIf("@_gameInfos?.RoundInfo?.Count < 4"), HideInEditorMode()]
    private void GenerateRound4Heatmap()
    {
        _currentHeatMap = 4;
        GenerateHeatmap();
    }


    [TabGroup("Round 5")]
    [Button("Generate Heatmap"), HideIf("@_gameInfos?.RoundInfo?.Count < 5"), HideInEditorMode()]
    private void GenerateRound5Heatmap()
    {
        _currentHeatMap = 5;
        GenerateHeatmap();
    }
    #endregion


    private void GenerateHeatmap()
    {
        List<List<BoxColorPair>> heatmapval;
        RoundInfos roundInfos;
        int roundTime;

        switch (_currentHeatMap)
        {
            case 1:
                heatmapval = _heatmapval1;
                roundInfos = _round1Infos;
                roundTime = _round1Time;
                break;

            case 2:
                heatmapval = _heatmapval2;
                roundInfos = _round2Infos;
                roundTime = _round2Time;
                break;

            case 3:
                heatmapval = _heatmapval3;
                roundInfos = _round3Infos;
                roundTime = _round3Time;
                break;

            case 4:
                heatmapval = _heatmapval4;
                roundInfos = _round4Infos;
                roundTime = _round4Time;
                break;

            case 5:
                heatmapval = _heatmapval5;
                roundInfos = _round5Infos;
                roundTime = _round5Time;
                break;


            default:
                return;
        }
        roundTime = (int)Mathf.Floor(roundTime / _round1Infos.timeInterval);
        heatmapval.Clear();

        heatmapval = new List<List<BoxColorPair>>();
        for (int i = 0; i <= MAP_SIZE; i++)
        {
            heatmapval.Add(new List<BoxColorPair>());
            for (int j = 0; j <= MAP_SIZE; j++)
            {
                heatmapval[i].Add(new BoxColorPair());
            }
        }


        int roundFrames = 0;
        // If we chose progressive heatmap, then the heatmap will show only where the players already went
        if (_progressiveHeatmap == true)
        {
            roundFrames = roundTime;
        }
        // Else we display heatmap for the round
        else
        {
            roundFrames = roundInfos.PlayerInfos.Count;
        }

        for (int i = 0; i < roundFrames; i++)
        {
            Vector2Int PlayerAPos = new Vector2Int((int)Mathf.Round(roundInfos.PlayerInfos[i].PlayerALocation.x), (int)Mathf.Round(roundInfos.PlayerInfos[i].PlayerALocation.z));
            heatmapval[PlayerAPos.x][PlayerAPos.y].IncreaseColor();

            Vector2Int PlayerBPos = new Vector2Int((int)Mathf.Round(roundInfos.PlayerInfos[i].PlayerBLocation.x), (int)Mathf.Round(roundInfos.PlayerInfos[i].PlayerBLocation.z));
            heatmapval[PlayerBPos.x][PlayerBPos.y].IncreaseColor();

            Vector2Int PlayerCPos = new Vector2Int((int)Mathf.Round(roundInfos.PlayerInfos[i].PlayerCLocation.x), (int)Mathf.Round(roundInfos.PlayerInfos[i].PlayerCLocation.z));
            heatmapval[PlayerCPos.x][PlayerCPos.y].IncreaseColor();

            Vector2Int PlayerDPos = new Vector2Int((int)Mathf.Round(roundInfos.PlayerInfos[i].PlayerDLocation.x), (int)Mathf.Round(roundInfos.PlayerInfos[i].PlayerDLocation.z));
            heatmapval[PlayerDPos.x][PlayerDPos.y].IncreaseColor();
        }

        // TODO : ugly code, find a way to use pointers somehow ?
        switch (_currentHeatMap)
        {
            case 1:
                _heatmapval1 = heatmapval;
                break;

            case 2:
                _heatmapval2 = heatmapval;
                break;

            case 3:
                _heatmapval3 = heatmapval;
                break;

            case 4:
                _heatmapval4 = heatmapval;
                break;

            case 5:
                _heatmapval5 = heatmapval;
                break;

            default:
                return;
        }
    }

    [Button, HideIf("@_replayIsPlaying == true || _gameInfos == null")]
    private void StartReplay()
    {
        _replayIsPlaying = true;
        StartCoroutine(PlayReplay());
    }

    [Button, HideIf("@_replayIsPlaying == false || _gameInfos == null")]
    private void StopReplay()
    {
        _replayIsPlaying = false;
        StopCoroutine(PlayReplay());
    }

    public IEnumerator PlayReplay()
    {

        int roundTime;
        RoundInfos roundInfos;
        // I Wish there was a better way of doing it but well
        switch (_currentHeatMap)
        {
            case 1:
                roundInfos = _round1Infos;
                roundTime = _round1Time;
                break;

            case 2:
                roundInfos = _round2Infos;
                roundTime = _round2Time;
                break;

            case 3:
                roundInfos = _round3Infos;
                roundTime = _round3Time;
                break;

            case 4:
                roundInfos = _round4Infos;
                roundTime = _round4Time;
                break;

            case 5:
                roundInfos = _round5Infos;
                roundTime = _round5Time;
                break;


            default:
                roundInfos = _round1Infos;
                roundTime = _round1Time;
                break;
        }

        while (roundTime < (int)Mathf.Floor(roundInfos.PlayerInfos.Count * _round1Infos.timeInterval) - 5 && _replayIsPlaying)
        {
            roundTime++;
            switch (_currentHeatMap)
            {
                case 1:
                    _round1Time = roundTime;
                    break;

                case 2:
                    _round2Time = roundTime;
                    break;

                case 3:
                    _round3Time = roundTime;
                    break;

                case 4:
                    _round4Time = roundTime;
                    break;

                case 5:
                    _round5Time = roundTime;
                    break;


                default:
                    _round1Time = roundTime;
                    break;
            }
            GenerateHeatmap();
            yield return new WaitForSeconds(_round1Infos.timeInterval/2);

        }

    }

    private void OnDrawGizmos()
    {
        List<List<BoxColorPair>> heatmapval;
        RoundInfos roundInfos;
        int roundTime;

        switch (_currentHeatMap)
        {
            case 1:
                heatmapval = _heatmapval1;
                roundInfos = _round1Infos;
                roundTime = _round1Time;
                break;

            case 2:
                heatmapval = _heatmapval2;
                roundInfos = _round2Infos;
                roundTime = _round2Time;
                break;

            case 3:
                heatmapval = _heatmapval3;
                roundInfos = _round3Infos;
                roundTime = _round3Time;
                break;

            case 4:
                heatmapval = _heatmapval4;
                roundInfos = _round4Infos;
                roundTime = _round4Time;
                break;

            case 5:
                heatmapval = _heatmapval5;
                roundInfos = _round5Infos;
                roundTime = _round5Time;
                break;


            default:
                return;
        }


        if (heatmapval.IsNullOrEmpty()) { return; };
        roundTime = (int)Mathf.Floor(roundTime / _round1Infos.timeInterval);

        // Draw Black background
        Gizmos.color = Color.black;
        Gizmos.DrawCube(new Vector3(transform.position.x + MAP_SIZE / 2, transform.position.y + +MAP_SIZE / 2, transform.position.z), new Vector3(MAP_SIZE, MAP_SIZE, 1));

        // Draw Heatmap
        for (int i = 0; i <= MAP_SIZE; i++)
        {
            for (int j = 0; j <= MAP_SIZE; j++)
            {
                if (heatmapval[i][j].color == Color.black) { continue; }
                Gizmos.color = heatmapval[i][j].color;
                Gizmos.DrawCube(new Vector3(transform.position.x + i, transform.position.y + j, transform.position.z), Vector3.one);
            }
        }

        // Draw Landsmarks
        for (int i = 0; i < _gameInfos.LandmarksLocation.Count; i++)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(new Vector3(transform.position.x + _gameInfos.LandmarksLocation[i].Location.x, transform.position.y + _gameInfos.LandmarksLocation[i].Location.y, transform.position.z), 5);
        }

        // Draw Player
        // Team A
        Gizmos.color = Color.blue;
        Vector3 PlayerAPos = roundInfos.PlayerInfos[roundTime].PlayerALocation;
        Gizmos.DrawSphere(new Vector3(transform.position.x + PlayerAPos.x, transform.position.y + PlayerAPos.z, transform.position.z), 2);

        Vector3 PlayerCPos = roundInfos.PlayerInfos[roundTime].PlayerCLocation;
        Gizmos.DrawSphere(new Vector3(transform.position.x + PlayerCPos.x, transform.position.y + PlayerCPos.z, transform.position.z), 2);

        // Team B
        Gizmos.color = Color.green;
        Vector3 PlayerBPos = roundInfos.PlayerInfos[roundTime].PlayerBLocation;
        Gizmos.DrawSphere(new Vector3(transform.position.x + PlayerBPos.x, transform.position.y + PlayerBPos.z, transform.position.z), 2);

        Vector3 PlayerDPos = roundInfos.PlayerInfos[roundTime].PlayerDLocation;
        Gizmos.DrawSphere(new Vector3(transform.position.x + PlayerDPos.x, transform.position.y + PlayerDPos.z, transform.position.z), 2);
    }
}

class BoxColorPair
{
    public Color color = Color.black;

    public void IncreaseColor()
    {
        //color.b = Mathf.Clamp(color.b + 0.1f, 0, 1);
        //color.g = Mathf.Clamp(color.b - 1 + color.g + 0.1f, 0, 1);
        //color.r = Mathf.Clamp(color.b - 1 + color.g - 1 + color.r + 0.1f, 0, 1);

        color.r = Mathf.Clamp(color.r + 0.2f, 0, 1);
        color.g = Mathf.Clamp(color.r - 1 + color.g + 0.06f, 0, 1);
        color.b = Mathf.Clamp(color.r - 1 + color.g - 1 + color.b + 0.1f, 0, 1);
    }
}
