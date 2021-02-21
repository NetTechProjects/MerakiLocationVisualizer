using System;
using System.Collections.Generic;
using System.Numerics;

namespace MerakiLocationVisualizer
{
    internal class DataTransform
    {
        internal static List<List<string>> NormalizeCoordinates(List<List<string>> observationReports, float vpW, float vpH)
        {
            List<Vector4> v4NDC = new List<Vector4>();
            List<List<string>> normalizedReports = new List<List<string>>();

            // scaling hack
            vpW = vpW / 0.43f;
            vpH = vpH / 0.43f;

            Matrix4x4 orthoTransform2D = new Matrix4x4(1.0f / vpW, 0.0f, 0.0f, 0.0f,
                                                     0.0f, 1.0f / vpH, 0.0f, 0.0f,
                                                     0.0f, 0.0f, 1.0f, 0.0f,
                                                     0.0f, 0.0f, 0.0f, 1.0f);

            foreach(List<string> report in observationReports)
            {
                List<String> tempList = new List<string>();

                for (int i = 0; i < report.Count; i += 5)
                {
                    // ignore records with NaN coordinates
                    if (report[i] != "NaN" && report[i + 1] != "NaN")
                    {
                        Vector4 inXYZW = new Vector4(3.0f * Convert.ToSingle(report[i]), 3.0f * Convert.ToSingle(report[i + 1]), 0f, 1f);
                        Vector4 outXYZW = Vector4.Transform(inXYZW, orthoTransform2D);

                        // rogue clients will appear lower left, associated clients upper right
                        if (report[i + 4] == "")
                        {
                            tempList.Add(Convert.ToString(outXYZW.X * -1f * 20f));
                            tempList.Add(Convert.ToString(outXYZW.Y * -1f * 20f));
                        }
                        else
                        {
                            tempList.Add((outXYZW.X * 20f).ToString());
                            tempList.Add((outXYZW.Y * 20f).ToString());
                        }
                        tempList.Add(report[i + 2]);
                        tempList.Add(report[i + 3]);
                    }
                }
                
                normalizedReports.Add(tempList);
            }

            return normalizedReports;  // x, y, ap, timestamp
        }

        // build quads from the x,y point data
        public static float[] GenerateQuads(float[] vertices, float vpW, float vpH)
        {
            float  halfWidth = 20.0f;   // distance from center (x,y) of square to edge

            List<float> inputCoordinates = new List<float>();

            List<float> BLACK = new List<float> { 1.0f, 1.0f, 1.0f, 1.0f };

            for (int index = 0; index < vertices.Length; index+=2)
            {
                float x, y;

                // upper triangle (half of square) [X, Y, R, G, B, A, TXx, TXy]
                x = (float)(vertices[index] - halfWidth / vpW);
                y = (float)(vertices[index+1] - halfWidth / vpH);
                inputCoordinates.Add((float)x);   // lower left x,y
                inputCoordinates.Add((float)y);
                inputCoordinates.AddRange(BLACK);   
                inputCoordinates.Add(0.0f);
                inputCoordinates.Add(0.0f);

                x = (float)(vertices[index] - halfWidth / vpW);
                y = (float)(vertices[index + 1] + halfWidth / vpH);
                inputCoordinates.Add((float)x);   // upper left x,y
                inputCoordinates.Add((float)y);
                inputCoordinates.AddRange(BLACK);
                inputCoordinates.Add(0.0f);
                inputCoordinates.Add(1.0f);

                x = (float)(vertices[index] + halfWidth / vpW);
                y = (float)(vertices[index + 1] + halfWidth / vpH);
                inputCoordinates.Add((float)x);   // upper right x,y
                inputCoordinates.Add((float)y);
                inputCoordinates.AddRange(BLACK);
                inputCoordinates.Add(1.0f);
                inputCoordinates.Add(1.0f);

                // lower triangle (half of square)
                x = (float)(vertices[index] - halfWidth / vpW);
                y = (float)(vertices[index + 1] - halfWidth / vpH);
                inputCoordinates.Add((float)x);   // lower left x,y
                inputCoordinates.Add((float)y);
                inputCoordinates.AddRange(BLACK);
                inputCoordinates.Add(0.0f);
                inputCoordinates.Add(0.0f);

                x = (float)(vertices[index] + halfWidth / vpW);
                y = (float)(vertices[index + 1] - halfWidth / vpH);
                inputCoordinates.Add((float)x);   // lower right x,y
                inputCoordinates.Add((float)y);
                inputCoordinates.AddRange(BLACK);
                inputCoordinates.Add(1.0f);
                inputCoordinates.Add(0.0f);

                x = (float)(vertices[index] + halfWidth / vpW);
                y = (float)(vertices[index + 1] + halfWidth / vpH);
                inputCoordinates.Add((float)x);   // upper right x,y
                inputCoordinates.Add((float)y);
                inputCoordinates.AddRange(BLACK);
                inputCoordinates.Add(1.0f);
                inputCoordinates.Add(1.0f);

            }

            float[] returnVertices = inputCoordinates.ToArray();

            return returnVertices;
        }
    }
}
