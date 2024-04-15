using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class ProcGenVisualizer : MonoBehaviour
{
    [Title("     Generation parameters")]
    [SerializeField, MinValue(1)] private float _minDistance = 1;
    [SerializeField, MinValue(1)] private Vector2 _regionSize = Vector2.one;
    [SerializeField, MinValue(1)] private int _numOfPoints;

    [Title("     Preview")]
    [SerializeField, MinValue(1)] private float _objectRadius = 1;
    [SerializeField] private Color _objectColor = Color.black;

    [Title("     Distance tool")]
    [SerializeField, MinValue(1)] private float _startMinDistance;

    [Range(1, 50)] private int _rejectionSamples;
    [MinValue(1)] private int _maxFailedAttempts;
    public List<Vector2> points;

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
                tmpPoints[i] = PoissonDiscSampling.GenerateExactNumberOfPoints(tmpRadius, _regionSize, _numOfPoints, 360, 10000);
              
                if (tmpPoints[i].Count < _numOfPoints)
                {
                    res = false;
                }
            }
         
        } while (res == false);
        int tmpRadiusSecured = Mathf.FloorToInt(tmpRadius * 0.9f);


        Debug.Log("La plus grande distance maximal trouvée est : " + tmpRadiusSecured);
        Debug.Log("(Sans sécurité) : " + tmpRadius);
        _minDistance = tmpRadius;
    }

    [Title("     GO !!!")]
    [Button]
    public void Generate()
    {
        _rejectionSamples = 360;
        _maxFailedAttempts = 10000;
        points = PoissonDiscSampling.GenerateExactNumberOfPoints(_minDistance, _regionSize, _numOfPoints, _rejectionSamples, _maxFailedAttempts);
        if (points.Count < _numOfPoints)
        {
            Debug.Log("Not enougth points, something went wrong? \n Number of spawned objects : " + points.Count);
        }

        //_shownRadius = _useRealRadius ? _minDistance : _objectRadius;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = _objectColor;

        Gizmos.DrawWireCube(transform.position, _regionSize);
        if (points != null)
        {
            foreach (Vector2 point in points)
            {
                
                Vector3 pointCenter = point;
                pointCenter.x -= _regionSize.x/2;
                pointCenter.x += transform.position.x;

                pointCenter.y -= _regionSize.y/2;
                pointCenter.y += transform.position.y;

                pointCenter.z += transform.position.z;
                Gizmos.DrawSphere(pointCenter, _objectRadius);
            }
        }
    }
}