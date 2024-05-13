using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections.Generic;
using UnityEngine;

public class HeatmapVisualizer : MonoBehaviour
{
    [SerializeField, AssetSelector(Filter = ("GameInfos"))] private TextAsset _dataFile;
    private GameInfos _gameInfos = null;

    [ReadOnly, SerializeField, TabGroup("Round 1")]
    RoundInfos _round1Infos = null;
    List<List<BoxColorPair>> _heatmapval1 = new();

    // TODO : Change to have the json record the size of the map
    private int MAP_SIZE = 125;

    private void Start()
    {
        string json = _dataFile.text;
        _gameInfos = JsonUtility.FromJson<GameInfos>(json);
        _round1Infos = _gameInfos.RoundInfo[0];
    }

    [Button]
    private void GenerateHeatmap()
    {
        _heatmapval1.Clear();

        _heatmapval1 = new List<List<BoxColorPair>>();
        for (int i = 0; i <= MAP_SIZE; i++)
        {
            _heatmapval1.Add(new List<BoxColorPair>());
            for (int j = 0; j <= MAP_SIZE; j++)
            {
                _heatmapval1[i].Add(new BoxColorPair());
            }
        }

        for (int i = 0; i < _round1Infos.PlayerInfos.Count; i++)
        {
            Vector2Int PlayerAPos = new Vector2Int((int)Mathf.Round(_round1Infos.PlayerInfos[i].PlayerALocation.x), (int)Mathf.Round(_round1Infos.PlayerInfos[i].PlayerALocation.z));
            _heatmapval1[PlayerAPos.x][PlayerAPos.y].increaseColor();

            Vector2Int PlayerBPos = new Vector2Int((int)Mathf.Round(_round1Infos.PlayerInfos[i].PlayerBLocation.x), (int)Mathf.Round(_round1Infos.PlayerInfos[i].PlayerBLocation.z));
            _heatmapval1[PlayerBPos.x][PlayerBPos.y].increaseColor();

            Vector2Int PlayerCPos = new Vector2Int((int)Mathf.Round(_round1Infos.PlayerInfos[i].PlayerCLocation.x), (int)Mathf.Round(_round1Infos.PlayerInfos[i].PlayerCLocation.z));
            _heatmapval1[PlayerCPos.x][PlayerCPos.y].increaseColor();

            Vector2Int PlayerDPos = new Vector2Int((int)Mathf.Round(_round1Infos.PlayerInfos[i].PlayerDLocation.x), (int)Mathf.Round(_round1Infos.PlayerInfos[i].PlayerDLocation.z));
            _heatmapval1[PlayerDPos.x][PlayerDPos.y].increaseColor();
        }
    }


    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.black;

        if (_heatmapval1.IsNullOrEmpty()) { return; };
        
        for (int i = 0; i <= MAP_SIZE; i++)
        {
            for (int j = 0; j <= MAP_SIZE; j++)
            {
                Gizmos.color = _heatmapval1[i][j].color;
                Gizmos.DrawCube(new Vector3(transform.position.x + i, transform.position.y + j, transform.position.z), Vector3.one);
            }
        }
    }
}

class BoxColorPair
{
    public Color color = Color.black;

    public void increaseColor()
    {
        //color.b = Mathf.Clamp(color.b + 0.1f, 0, 1);
        //color.g = Mathf.Clamp(color.b - 1 + color.g + 0.1f, 0, 1);
        //color.r = Mathf.Clamp(color.b - 1 + color.g - 1 + color.r + 0.1f, 0, 1);

        color.r = Mathf.Clamp(color.r + 0.1f, 0, 1);
        color.g = Mathf.Clamp(color.r - 1 + color.g + 0.1f, 0, 1);
        color.b = Mathf.Clamp(color.r - 1 + color.g - 1 + color.b + 0.1f, 0, 1);
    }
}
