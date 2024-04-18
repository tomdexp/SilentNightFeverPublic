using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class ProcGenVisualizer : MonoBehaviour
{
    [Title("     Generation parameters")]
    [SerializeField, MinValue(1)] private float _minDistance = 1;
    [SerializeField, MinValue("@_minDistance"), MaxValue("@_minDistance*3")] private float _maxDistance = 1;
    [SerializeField, MinValue(1)] private Vector2 _regionSize = Vector2.one;
    [SerializeField, Range(0, 100), SuffixLabel("%")] private float edgeDistance = 0;
    [SerializeField, MinValue(1)] private int _numOfPoints;

    [Title("     Preview")]
    [SerializeField, MinValue(1)] private float _objectRadius = 1;
    [SerializeField] private Color _objectColor = Color.black;

    [Title("     Distance tool")]
    [SerializeField, MinValue(1)] private float _startMinDistance;

    [Range(1, 50)] private int _rejectionSamples;
    [MinValue(1)] private int _maxFailedAttempts;
    private List<Vector2> _points;

    [Button]
    void CalculateMaximumRadius()
    {
        List<List<Vector2>> tmpPoints = new List<List<Vector2>>();
        float tmpRadius = _startMinDistance;


        for (int i = 0; i < 100; i++)
        {
            tmpPoints.Add(new List<Vector2>());
        }
        bool res = true;

        do
        {
            res = true;
            tmpRadius--;
            for (int i = 0; i < tmpPoints.Count; i++)
            {
                Vector2 newRegionSize = _regionSize;
                newRegionSize.x *= (100 - edgeDistance) / 100;
                newRegionSize.y *= (100 - edgeDistance) / 100;
                tmpPoints[i] = PoissonDiscSampling.GenerateExactNumberOfPoints(tmpRadius, _regionSize, _numOfPoints, 720, 10000);

                if (tmpPoints[i].Count < _numOfPoints)
                {
                    res = false;
                    break;
                }
            }

        } while (res == false);
        int tmpRadiusSecured = Mathf.FloorToInt(tmpRadius * 0.9f);


        Debug.Log("La plus grande distance maximal trouvée est : " + tmpRadiusSecured + "\n(Sans sécurité) : " + tmpRadius);
        _minDistance = tmpRadiusSecured;
    }

    [Title("     GO !!!")]
    [Button]
    public void GenerateNew()
    {
        _rejectionSamples = 720;
        _maxFailedAttempts = 10000;
        Vector2 newRegionSize = _regionSize;
        newRegionSize.x *= (100 - edgeDistance) / 100;
        newRegionSize.y *= (100 - edgeDistance) / 100;
        _points = PoissonDiscSampling.GenerateExactNumberOfPoints(_minDistance, _maxDistance, newRegionSize, _numOfPoints, _rejectionSamples, _maxFailedAttempts);
        if (_points.Count < _numOfPoints)
        {
            Debug.Log("Not enougth _points, something went wrong? \n Number of spawned objects : " + _points.Count);
        }
    }

    [Button]
    public void GenerateMore()
    {
        _rejectionSamples = 720;
        _maxFailedAttempts = 10000;
        Vector2 newRegionSize = _regionSize;
        newRegionSize.x *= (100 - edgeDistance) / 100;
        newRegionSize.y *= (100 - edgeDistance) / 100;
        _points.AddRange(PoissonDiscSampling.GenerateExactNumberOfPoints(_minDistance, _maxDistance, newRegionSize, _numOfPoints, _rejectionSamples, _maxFailedAttempts));
        if (_points.Count < _numOfPoints)
        {
            Debug.Log("Not enougth _points, something went wrong? \n Number of spawned objects : " + _points.Count);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = _objectColor;

        Vector2 newRegionSize = _regionSize;
        newRegionSize.x *= (100 - edgeDistance) / 100;
        newRegionSize.y *= (100 - edgeDistance) / 100;
        Gizmos.DrawWireCube(transform.position, newRegionSize);

        //for (float i = 0; i < 720; i++)
        //{
        //    float angle = i/360 * Mathf.PI * 2;
        //    Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
        //    //Vector2 candidate = spawnCentre + dir * Random.Range(minRadius, maxRadius);
        //    //Vector2 candidate = transform.position + new Vector3(dir.x * 100.0f, dir.y * 100.0f, transform.position.z);
        //    Vector2 candidate = transform.position + new Vector3(dir.x * Random.Range(_minDistance, _maxDistance), dir.y * Random.Range(_minDistance, _maxDistance), transform.position.z);
        //    Gizmos.DrawSphere(candidate, 10);
        //}


        if (_points != null)
        {
            foreach (Vector2 point in _points)
            {

                Vector3 pointCenter = point;
                pointCenter.x -= newRegionSize.x / 2;
                pointCenter.x += transform.position.x;

                pointCenter.y -= newRegionSize.y / 2;
                pointCenter.y += transform.position.y;

                pointCenter.z += transform.position.z;
                Gizmos.DrawSphere(pointCenter, _objectRadius);
            }
        }
    }
}