namespace PoissonDiskSampler;

public static class PoissonSampler
{
    // sampleSize designates a NxNx... sized box to fill with points
    public static float[][] SimpleSampler(int[] sampleSize, float radius, int attemptsPerPoint)
    {
        Random random = new Random();
        return DoPoissonSampler(sampleSize, radius, (int)random.NextInt64(), attemptsPerPoint);
    }

    public static float[][] SimpleSampler(int[] sampleSize, float radius, int seed, int attemptsPerPoint)
    {
        return DoPoissonSampler(sampleSize, radius, seed, attemptsPerPoint);
    }

    private static float[][] DoPoissonSampler(int[] sampleSize, float radius, int seed, int attempts)
    {
        if (sampleSize.Length == 0 || sampleSize.Any(size => size <= 0))
        {
            throw new ArgumentException("The sample bounds must contain only positive dimensions.", nameof(sampleSize));
        }
        if (!float.IsFinite(radius) || radius <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(radius), "The radius must be a positive finite number.");
        }
        if (attempts <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(attempts), "Attempts per point must be positive.");
        }

        float cellSize = radius / ((float)Math.Sqrt(sampleSize.Length) * 1.01f);
        int[] gridSize = new int[sampleSize.Length];
        int packedSize = 1;
        for (int i = 0; i < sampleSize.Length; i++)
        {
            gridSize[i] = checked((int)Math.Ceiling(sampleSize[i] / cellSize));
            packedSize = checked(packedSize * gridSize[i]);
        }

        Random random = new Random(seed);
        float[][] packedGrid = new float[packedSize][];
        bool[] packedPresenceGrid = new bool[packedSize];
        List<float[]> points = new List<float[]>();

        float[] startPosition = new float[sampleSize.Length];
        for (int i = 0; i < startPosition.Length; i++)
        {
            startPosition[i] = random.NextSingle() * sampleSize[i];
        }
        int[] gridPos = getGridCoordinate(startPosition, cellSize, gridSize);
        int startIndex = getPackedCoordinate(gridPos, gridSize);
        packedGrid[startIndex] = startPosition;
        packedPresenceGrid[startIndex] = true;

        Queue<float[]> frontier = new Queue<float[]>();
        frontier.Enqueue(startPosition);
        points.Add(startPosition);

        while (frontier.Count > 0)
        {
            float[] point = frontier.Dequeue();
            for (int attempt = 0; attempt < attempts; attempt++)
            {
                float[] direction = getRandomUnitVector(random, sampleSize.Length);
                float candidateRadius = radius * (1 + random.NextSingle());
                float[] newPoint = new float[sampleSize.Length];
                bool withinBounds = true;
                for (int i = 0; i < newPoint.Length; i++)
                {
                    newPoint[i] = point[i] + direction[i] * candidateRadius;
                    if (newPoint[i] < 0 || newPoint[i] >= sampleSize[i])
                    {
                        withinBounds = false;
                        break;
                    }
                }

                if (!withinBounds)
                {
                    continue;
                }

                int[] newGridPosition = getGridCoordinate(newPoint, cellSize, gridSize);
                int newIndex = getPackedCoordinate(newGridPosition, gridSize);
                if (packedPresenceGrid[newIndex])
                {
                    continue;
                }

                bool tooClose = false;
                foreach (float[] existingPoint in points)
                {
                    if (squaredEuclideanDistance(newPoint, existingPoint) < radius * radius)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose)
                {
                    continue;
                }

                packedGrid[newIndex] = newPoint;
                packedPresenceGrid[newIndex] = true;
                points.Add(newPoint);
                frontier.Enqueue(newPoint);
            }
        }

        return points.ToArray();
    }

    private static float[] getRandomUnitVector(Random random, int dimensions)
    {
        float[] vector = new float[dimensions];
        float lengthSquared;
        do
        {
            lengthSquared = 0;
            for (int i = 0; i < dimensions; i++)
            {
                vector[i] = (float)(random.NextDouble() * 2 - 1);
                lengthSquared += vector[i] * vector[i];
            }
        }
        while (lengthSquared <= 0 || lengthSquared > 1);

        float length = (float)Math.Sqrt(lengthSquared);
        for (int i = 0; i < dimensions; i++)
        {
            vector[i] /= length;
        }
        return vector;
    }

    private static int[] getGridCoordinate(float[] point, float cellSize, int[] gridSize)
    {
        int[] coordinate = new int[point.Length];
        for (int i = 0; i < point.Length; i++)
        {
            coordinate[i] = (int)(point[i] / cellSize);
            if (coordinate[i] < 0 || coordinate[i] >= gridSize[i])
            {
                throw new ArgumentOutOfRangeException(nameof(point), "Point lies outside the packed grid.");
            }
        }
        return coordinate;
    }

    private static float squaredEuclideanDistance(float[] a, float[] b)
    {
        float d = 0;
        for (int i = 0; i < a.Length; i++)
        {
            float difference = a[i] - b[i];
            d += difference * difference;
        }
        return d;
    }

    private static int getPackedCoordinate(int[] coordinate, int[] gridSize)
    {
        if (coordinate.Length != gridSize.Length)
        {
            throw new ArgumentException("Coordinate and grid dimensions must match.", nameof(coordinate));
        }

        int packedCoordinate = 0;
        int stride = 1;
        for (int i = 0; i < coordinate.Length; i++)
        {
            if (coordinate[i] < 0 || coordinate[i] >= gridSize[i])
            {
                throw new ArgumentOutOfRangeException(nameof(coordinate), "Coordinate lies outside the packed grid.");
            }

            packedCoordinate = checked(packedCoordinate + coordinate[i] * stride);
            stride = checked(stride * gridSize[i]);
        }
        return packedCoordinate;
    }
}