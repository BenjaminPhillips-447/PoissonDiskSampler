namespace PoissonDiskSampler;

public static class PoissonSampler
{
    // sampleSize designates a NxNx... sized box to fill with points
    public static void SimpleSampler(int[] sampleSize, float radius, int attemptsPerPoint)
    {
        Random random = new Random();
        DoPoissonSampler(sampleSize, radius, (int)random.NextInt64(), attemptsPerPoint);
    }

    public static void SimpleSampler(int[] sampleSize, float radius, int seed, int attemptsPerPoint)
    {
        DoPoissonSampler(sampleSize, radius, seed, attemptsPerPoint);
    }

    private static void DoPoissonSampler(int[] sampleSize, float radius, int seed, int attempts)
    {
        //checking accelerated with a grid
        //gridSize caclulated from the maximum diagonal size of a gridsquare
        float gridSquareSize = radius / (float)Math.Sqrt(sampleSize.Length);

        //create encompassing grid
        int[] gridSize = new int[sampleSize.Length];
        int packedSize = 0;
        for (int i = 0; i < sampleSize.Length; i++) { gridSize[i] = (int)Math.Ceiling(sampleSize[i] / gridSquareSize); packedSize += gridSize[i];}
        
        
        Random random = new Random(seed);
        
        //flat packed dimensions dimensional grid
        float[][] packedGrid = new float[packedSize][];
        bool[] packedPresenceGrid = new bool[packedSize];
        List<float[]> points = new List<float[]>();
        
        float[] startPosition = new float[gridSize.Length];
        for (int i = 0; i < startPosition.Length; i++) { startPosition[i] = random.NextSingle() * gridSize[i]; }
        int[] gridPos = floorVec(startPosition);
        int startIndex = getPackedCoordinate(gridPos, gridSize);
        packedGrid[startIndex] = startPosition;
        packedPresenceGrid[startIndex] = true;

        Queue<float[]> frontier = new Queue<float[]>();
        frontier.Append(startPosition);
        points.Add(startPosition);
        
        while (frontier.Count > 0)
        {
            float[] point = frontier.Dequeue();

            for (int attempt = 0; attempt < attempts; attempt++)
            {
                float r = radius * (1 + random.NextSingle());
                float[] offset = new float[sampleSize.Length];
                float[] angles = new float[sampleSize.Length - 1];

                for(int i = 0; i < angles.Length - 1; i++)
                {
                    angles[i] = (float)(random.NextSingle() * Math.PI);
                }
                angles[angles.Length - 1] = (float)(random.NextSingle() * Math.Tau);

                for (int i = 0; i < sampleSize.Length; i++)
                {
                    point[i] = r;
                    for(int k = 0; k <= Math.Min(i, sampleSize.Length - 2); k++)
                    {
                        offset[i] *= (float)(k == i ? Math.Cos(angles[i]) : Math.Sin(angles[i]));
                    }
                }

                float[] newPoint = addVector(point, offset);

                bool checkValidity(float[] checkPoint)
                {
                    int[] gridPos = floorVec(checkPoint);
                    int packedPos = getPackedCoordinate(gridPos, sampleSize);
                    if(!packedPresenceGrid[packedPos])
                    {
                        int[] adjacentSlots = getLocalProximity(gridPos, sampleSize);
                        foreach(int slot in adjacentSlots)
                        {
                            if(packedPresenceGrid[slot])
                            {
                                if (squaredEuclideanDistance(checkPoint, packedGrid[slot]) < radius * radius) return false;
                            }
                        }
                    }
                    return true;
                }

                if (checkValidity(newPoint))
                {
                    frontier.Append(newPoint);
                    points.Add(newPoint);
                    int newIndex = getPackedCoordinate(floorVec(newPoint), gridSize);
                    packedGrid[newIndex] = startPosition;
                    packedPresenceGrid[newIndex] = true;
                }
            }
        }


    }

    private static float[] addVector(float[] a, float[] b)
    {
        for (int i = 0; i < a.Length; i++) { a[i] += b[i]; }
        return a;
    }

    private static int[] floorVec(float[] a)
    {
        int[] ints = new int[a.Length];
        for (int i = 0; i < a.Length; i++)
        {
            ints[i] = (int)Math.Floor(a[i]);
        }
        return ints;
    }

    private static float squaredEuclideanDistance(float[] a, float[] b)
    {
        float d = 0;
        for (int i = 0; i < a.Length; i++)
        {
            d = (a[i] - b[i]) * (a[i] - b[i]); 
        }
        return d;
    }

    private static int getPackedCoordinate(int[] coordinate, int[] gridSize)
    {
        int packedCoordinate = 0;
        for (int i = 0; i < coordinate.Length; i++)
        {
            int mult = 1;
            for (int j = 0; j < i; j++) { mult *= gridSize[j]; }
            packedCoordinate += coordinate[i] * mult;
        }
        return packedCoordinate;
    }

    private static int[] getLocalProximity(int[] coordinate, int[] gridSize)
    {
        int[][] localPositions = new int[(int)Math.Pow(5, gridSize.Length)][];
        for (int i = 0; i < localPositions.Length; i++)
        {
            localPositions[i] = new int[gridSize.Length];
            for (int d = 0; d < gridSize.Length; d++)
            {
                localPositions[i][d] = coordinate[d] + (i / (int)Math.Pow(d, 5) % 5);
                if(localPositions[i][d] >= gridSize[d] || localPositions[i][d] < 0) localPositions[i] = [-1];
            }
        }
        List<int> valid = new List<int>();
        for (int i = 0; i < localPositions.Length; i++)
        {
            if(localPositions[i][0] != -1) valid.Add(getPackedCoordinate(localPositions[i], gridSize)); 
        }
        return valid.ToArray();
    }
}