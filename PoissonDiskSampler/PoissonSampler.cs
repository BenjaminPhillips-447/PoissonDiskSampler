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

    private static void DoPoissonSampler(int[] sampleSize, float radius, int seed)
    {
        //checking accelerated with a grid
        //gridSize caclulated from the maximum diagonal size of a gridsquare
        float gridSquareSize = radius / (float)Math.Sqrt(sampleSize.Length);

        //create encompassing grid
        int[] gridSize = new int[sampleSize.Length];
        int packedSize = 0;
        for (int i = 0; i < sampleSize.Length; i++) { gridSize[i] = (int)Math.Ceiling(sampleSize[i] / gridSquareSize); packedSize += gridSize[i];}
        
        
        Random random = new Random();
        
        //flat packed n dimensional grid
        float[][] packedGrid = new float[packedSize][];
        bool[] packedPresenceGrid = new bool[packedSize];
        
        float[] startPosition = new float[gridSize.Length];
        int[] gridPos = new int[gridSize.Length];
        for (int i = 0; i < startPosition.Length; i++) { startPosition[i] = random.NextSingle() * gridSize[i]; gridPos[i] = (int)Math.Floor(startPosition[i]); }
        int startIndex = getPackedCoordinate(gridPos, gridSize);
        packedGrid[startIndex] = startPosition;
        packedPresenceGrid[startIndex] = true;



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

    //get the 5x5x... grid square's packed index around the given coordinate using only addition and subtraction to reduce the insane amount of calculation this sampler needs
    private static int[][] getLocalProximity(int[] coordinate, int[] gridSize)
    {
        int[][] localPositions = new int[(int)Math.Pow(5, gridSize.Length)][];
        int packedCoordinate = getPackedCoordinate(coordinate, gridSize);

        for (int i = 0; i < localPositions.Length; i++)
        {
            localPositions[i] = new int[gridSize.Length];
            for (int k = 0; k < gridSize.Length; k++)
            {
                
            }
        }
    }
}
