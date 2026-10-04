namespace PoissonDiskSampler;

public static class PoissonSampler
{
    // sampleSize designates a NxNx... sized box to fill with points
    public static void SimpleSampler(int[] sampleSize, float radius)
    {
        Random random = new Random();
        DoPoissonSampler(sampleSize, radius, (int)random.NextInt64());
    }

    public static void SimpleSampler(int[] sampleSize, float radius, int seed)
    {
        DoPoissonSampler(sampleSize, radius, seed);
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
        
        
        Random random = new Random();
        
        //flat packed dimensions dimensional grid
        float[][] packedGrid = new float[packedSize][];
        bool[] packedPresenceGrid = new bool[packedSize];
        
        float[] startPosition = new float[gridSize.Length];
        int[] gridPos = new int[gridSize.Length];
        for (int i = 0; i < startPosition.Length; i++) { startPosition[i] = random.NextSingle() * gridSize[i]; gridPos[i] = (int)Math.Floor(startPosition[i]); }
        int startIndex = getPackedCoordinate(gridPos, gridSize);
        packedGrid[startIndex] = startPosition;
        packedPresenceGrid[startIndex] = true;

        Queue<float[]> frontier = new Queue<float[]>();
        frontier.Append(startPosition);

        while (frontier.Count > 0)
        {
            float[] point = frontier.Dequeue();

            for(int i = 0; i < attempts; i++)
            {
                
            }
        }
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

        int dimensions = gridSize.Length;
        int[] point = new int[dimensions];

        IEnumerable<int> Loop(int i)
        {
            if (i == dimensions)
            {
                int[] result = new int[dimensions];

                for (int j = 0; j < dimensions; j++) result[j] = coordinate[j] + point[j];

                yield return getPackedCoordinate(result, gridSize);
                yield break;
            }

            for (int x = -2; x <= 2; x++)
            {
                point[i] = x;
                foreach (var p in Loop(i + 1)) yield return p;
            }
        }

        return Loop(0).ToArray();
    }
}