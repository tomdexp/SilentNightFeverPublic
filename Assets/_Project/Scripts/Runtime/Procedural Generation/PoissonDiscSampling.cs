using System.Collections.Generic;
using UnityEngine;

public static class PoissonDiscSampling
{
    public static List<Vector2> GeneratePoints(float radius, Vector2 sampleRegionSize, int numSamplesBeforeRejection = 30, int minPoints = -1, int maxPoints = -1)
    {
        float cellSize = radius / Mathf.Sqrt(2);

        int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];
        List<Vector2> points = new List<Vector2>();
        List<Vector2> spawnPoints = new List<Vector2>();

        spawnPoints.Add(sampleRegionSize / 2);
        while (spawnPoints.Count > 0 /*|| (minPoints != -1 && points.Count < minPoints)*/ && !(maxPoints != -1 && points.Count >= maxPoints))
        {
            int spawnIndex = Random.Range(0, spawnPoints.Count);
            Vector2 spawnCentre = spawnPoints[spawnIndex];
            bool candidateAccepted = false;

            for (int i = 0; i < numSamplesBeforeRejection; i++)
            {
                float angle = Random.value * Mathf.PI * 2;
                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                Vector2 candidate = spawnCentre + dir * Random.Range(radius, 2 * radius);
                if (IsValid(candidate, sampleRegionSize, cellSize, radius, points, grid))
                {
                    points.Add(candidate);
                    spawnPoints.Add(candidate);
                    grid[(int)(candidate.x / cellSize), (int)(candidate.y / cellSize)] = points.Count;
                    candidateAccepted = true;
                    break;
                }
            }
            if (!candidateAccepted)
            {
                spawnPoints.RemoveAt(spawnIndex);
            }

        }

        return points;
    }

    public static List<Vector2> GenerateExactNumberOfPoints(float radius, Vector2 sampleRegionSize, int numOfPoints, int numSamplesBeforeRejection = 30, int maxFailedAttempts = 10)
    {
        float cellSize = radius / Mathf.Sqrt(2);

        int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];
        List<Vector2> points = new List<Vector2>();
        List<Vector2> spawnPoints = new List<Vector2>();

        int failedAttempts = 0;
        spawnPoints.Add(sampleRegionSize / 2);
        while (points.Count < numOfPoints && failedAttempts < maxFailedAttempts)
        {
            int spawnIndex = Random.Range(0, spawnPoints.Count);
            Vector2 spawnCentre = spawnPoints[spawnIndex];
            bool candidateAccepted = false;

            for (int i = 0; i < numSamplesBeforeRejection; i++)
            {
                float angle = Random.value * Mathf.PI * 2;
                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                Vector2 candidate = spawnCentre + dir * Random.Range(radius, 2 * radius);
                if (IsValid(candidate, sampleRegionSize, cellSize, radius, points, grid))
                {
                    points.Add(candidate);
                    spawnPoints.Add(candidate);
                    grid[(int)(candidate.x / cellSize), (int)(candidate.y / cellSize)] = points.Count;
                    candidateAccepted = true;
                    break;
                }
            }
            if (!candidateAccepted)
            {
                if (spawnPoints.Count > 1)
                {
                    spawnPoints.RemoveAt(spawnIndex);
                }
                failedAttempts++;
            }

        }

        return points;
    }

    public static List<Vector2> GenerateExactNumberOfPoints(float minRadius, float maxRadius, Vector2 sampleRegionSize, int numOfPoints, int numSamplesBeforeRejection = 30, int maxFailedAttempts = 10)
    {
        float cellSize = minRadius / Mathf.Sqrt(2);
        if (maxRadius < minRadius)
        {
            maxRadius = minRadius;
        }

        int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];
        List<Vector2> points = new List<Vector2>();
        List<Vector2> spawnPoints = new List<Vector2>();

        int failedAttempts = 0;
        spawnPoints.Add(new Vector2(Random.Range(1, sampleRegionSize.x - 1), Random.Range(1, sampleRegionSize.y - 1)));
        bool firstSpawnPoint = true;
        while (points.Count < numOfPoints && failedAttempts < maxFailedAttempts)
        {
            int spawnIndex = Random.Range(0, spawnPoints.Count);
            Vector2 spawnCentre = spawnPoints[spawnIndex];
            bool candidateAccepted = false;

            for (int i = 0; i < numSamplesBeforeRejection; i++)
            {
                float angle = Random.value * Mathf.PI * 2;
                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                Vector2 candidate = spawnCentre + dir * Random.Range(minRadius, maxRadius);
                //Vector2 candidate = spawnCentre + dir * minRadius;
                if (IsValid(candidate, sampleRegionSize, cellSize, minRadius, points, grid))
                {
                    points.Add(candidate);
                    spawnPoints.Add(candidate);
                    grid[(int)(candidate.x / cellSize), (int)(candidate.y / cellSize)] = points.Count;
                    candidateAccepted = true;
                    break;
                }
            }
            if (!candidateAccepted)
            {
                if (firstSpawnPoint)
                {
                    spawnPoints.RemoveAt(spawnIndex);
                    spawnPoints.Add(new Vector2(Random.Range(1, sampleRegionSize.x - 1), Random.Range(1, sampleRegionSize.y - 1)));
                }

                if (spawnPoints.Count > 1)
                {
                    spawnPoints.RemoveAt(spawnIndex);
                }
                failedAttempts++;
            }
            else if (firstSpawnPoint)
            {
                spawnPoints.RemoveAt(spawnIndex);
                firstSpawnPoint = false;
            }
        }

        return points;
    }

    public static List<Vector2> GenerateExactNumberOfPoints(float minRadius, float maxRadius, Vector2 sampleRegionSize, int numOfPoints, List<List<Vector2>> prevPoints, List<float> prevPointsRadius, int numSamplesBeforeRejection = 30, int maxFailedAttempts = 10)
    {
        float cellSize = minRadius / Mathf.Sqrt(2);
        if (maxRadius < minRadius)
        {
            maxRadius = minRadius;
        }

        // Check if previousPoints List are valid :
        if (prevPoints.Count != prevPointsRadius.Count)
        {
            Debug.LogWarning("List prevPointsRadius & prevPoints must be the same size : " + prevPointsRadius.Count + " vs " + prevPoints);
            return new List<Vector2>();
        }

        int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];
        List<Vector2> points = new List<Vector2>();
        List<Vector2> spawnPoints = new List<Vector2>();

        int failedAttempts = 0;
        spawnPoints.Add(new Vector2(Random.Range(1, sampleRegionSize.x - 1), Random.Range(1, sampleRegionSize.y - 1)));
        bool firstSpawnPoint = true;
        while (points.Count < numOfPoints && failedAttempts < maxFailedAttempts)
        {
            int spawnIndex = Random.Range(0, spawnPoints.Count);
            Vector2 spawnCentre = spawnPoints[spawnIndex];
            bool candidateAccepted = false;

            for (int i = 0; i < numSamplesBeforeRejection; i++)
            {
                float angle = Random.value * Mathf.PI * 2;
                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                Vector2 candidate = spawnCentre + dir * Random.Range(minRadius, maxRadius);
                //Vector2 candidate = spawnCentre + dir * minRadius;

                bool isValid = true;
                for (int j = 0; j < prevPoints.Count; j++)
                {
                    isValid = isValid && IsValid(candidate, prevPoints[j], prevPointsRadius[j]);
                }

                if (isValid && IsValid(candidate, sampleRegionSize, cellSize, minRadius, points, grid))
                {
                    points.Add(candidate);
                    spawnPoints.Add(candidate);
                    grid[(int)(candidate.x / cellSize), (int)(candidate.y / cellSize)] = points.Count;
                    candidateAccepted = true;
                    break;
                }
            }
            if (!candidateAccepted)
            {
                if (firstSpawnPoint)
                {
                    spawnPoints.RemoveAt(spawnIndex);
                    spawnPoints.Add(new Vector2(Random.Range(1, sampleRegionSize.x - 1), Random.Range(1, sampleRegionSize.y - 1)));
                }

                if (spawnPoints.Count > 1)
                {
                    spawnPoints.RemoveAt(spawnIndex);
                }
                failedAttempts++;
            }
            else if (firstSpawnPoint)
            {
                spawnPoints.RemoveAt(spawnIndex);
                firstSpawnPoint = false;
            }
        }

        return points;
    }


    static bool IsValid(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float radius, List<Vector2> points, int[,] grid)
    {
        if (candidate.x >= 0 && candidate.x < sampleRegionSize.x && candidate.y >= 0 && candidate.y < sampleRegionSize.y)
        {
            int cellX = (int)(candidate.x / cellSize);
            int cellY = (int)(candidate.y / cellSize);
            int searchStartX = Mathf.Max(0, cellX - 2);
            int searchEndX = Mathf.Min(cellX + 2, grid.GetLength(0) - 1);
            int searchStartY = Mathf.Max(0, cellY - 2);
            int searchEndY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);

            for (int x = searchStartX; x <= searchEndX; x++)
            {
                for (int y = searchStartY; y <= searchEndY; y++)
                {
                    int pointIndex = grid[x, y] - 1;
                    if (pointIndex != -1)
                    {
                        float sqrDst = (candidate - points[pointIndex]).sqrMagnitude;
                        if (sqrDst < radius * radius)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        return false;
    }


    // Brute force way to check if point is valid, used with prevPoints
    static bool IsValid(Vector2 candidate, List<Vector2> points, float radius)
    {
        for (int i = 0; i < points.Count; i++)
        {
            float sqrDst = (candidate - points[i]).sqrMagnitude;
            if (sqrDst < radius * radius)
            {
                return false;
            }
        }
        return true;
    }
}