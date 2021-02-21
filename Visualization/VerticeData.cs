using System;
using System.Collections.Generic;

namespace MerakiLocationVisualizer
{
    static class VerticeData
    {
        // BACKGROUND QUAD
        // [X, Y, R, G, B, A, TXx, TXy]
        public static float[] backgroundVertices()
        {
            float[] vertices = new float[]
            {
                -1.0f, -1.0f,
                1f, 1f, 1f, 1f,
                0.0f, 0.0f,
                -1.0f, 1.0f,
                1f, 1f, 1f, 1f,
                0.0f, 1.0f,
                1.0f, 1.0f,
                1f, 1f, 1f, 1f,
                1.0f, 1.0f,
                -1.0f, -1.0f,
                1f, 1f, 1f, 1f,
                0.0f, 0.0f,
                1.0f, 1.0f,
                1f, 1f, 1f, 1f,
                1.0f, 1.0f,
                1.0f, -1.0f,
                1f, 1f, 1f, 1f,
                1.0f, 0.0f
            };

            return vertices;
        }

        // ACCESS POINT 
        // [X, Y, R, G, B, A, TXx, TXy]
        public static float[] apVertices()
        {
            float[] vertices = new float[]
            {
                -0.15f, -0.15f,
                1f, 1f, 1f, 1f,
                0.0f, 0.0f,
                -0.15f, 0.15f,
                1f, 1f, 1f, 1f,
                0.0f, 1.0f,
                0.15f, 0.15f,
                1f, 1f, 1f, 1f,
                1.0f, 1.0f,
                -0.15f, -0.15f,
                1f, 1f, 1f, 1f,
                0.0f, 0.0f,
                0.15f, 0.15f,
                1f, 1f, 1f, 1f,
                1.0f, 1.0f,
                0.15f, -0.15f,
                1f, 1f, 1f, 1f,
                1.0f, 0.0f
            };

            return vertices;
        }

        // RADAR SWEEP 
        // [X, Y, R, G, B, A, TXx, TXy]
        public static float[] sweepVertices(float degrees)
        {
            float[] vertices = new float[]
            {
                0.0f, 0.0f,
                0.0f, 1.0f, 1.0f, 1.0f,
                0.0f, 0.0f,
                (float)Math.Sin(degrees) * .97f, (float)Math.Cos(degrees) * .97f,
                0.0f, 0.0f, 0.1f, 1.0f,
                0.0f, 0.0f,
                (float)Math.Sin(degrees + 0.6f) * .97f, (float)Math.Cos(degrees + 0.6f) * .97f,
                0.0f, 0.0f, 0.1f, 1.0f,
                0.0f, 0.0f,
            };

            return vertices;
        }

        public static int NumberOfAPs { get; set; }

        public static float[] VerticeSlice { get; set; }

        public static float[] SquareVertices { get; set; }

        public static List<List<string>> NormalizedReports { get; set; }
    }
}
