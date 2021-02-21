using System.IO;
using GLFW;
using static OpenGL.GL;
using StbImageSharp;
using System;


namespace MerakiLocationVisualizer
{
    static internal class Renderer
    {
        public static unsafe uint Render(float[] verticeArray)
        {
            uint vao = glGenVertexArray();
            glBindVertexArray(vao);

            Glfw.GetError(out string description);
            if (description != null)
            {
                Console.WriteLine("ERROR: {0}", description);
            }

            uint vbo = glGenBuffer();
            glBindBuffer(GL_ARRAY_BUFFER, vbo);

            fixed (float* vertPtr = &verticeArray[0])
            {
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * verticeArray.Length, vertPtr, GL_DYNAMIC_DRAW);
            }

            // x,y
            glVertexAttribPointer(0, 2, GL_FLOAT, false, 8 * sizeof(float), (void*)0);
            glEnableVertexAttribArray(0);
            // r,g,b,a
            glVertexAttribPointer(1, 4, GL_FLOAT, false, 8 * sizeof(float), (void*)(2 * sizeof(float)));
            glEnableVertexAttribArray(1);
            // texture coords
            glVertexAttribPointer(2, 2, GL_FLOAT, false, 8 * sizeof(float), (void*)(6 * sizeof(float)));
            glEnableVertexAttribArray(2);

            glBindBuffer(GL_ARRAY_BUFFER, 0);
            glBindVertexArray(0);
            glDeleteBuffer(vbo);

            Import(Glfw.GetProcAddress);

            return vao;
        }

        unsafe public static uint[] LoadTextures()
        {
            uint[] textures = new uint[8]; // background, red client, green client and (5) ap images
            string backgroundFile = "Resources/textures/1200_scope-background.png";
            string apFile = "Resources/textures/256_ap-0";
            string greenSphereFile = "Resources/textures/256_green-sphere.png";
            string redSphereFile = "Resources/textures/256_red-sphere.png";

            fixed (uint* txPtr = &textures[0])
            {
                glGenTextures(8, txPtr);
            }

            glEnable(GL_BLEND);
            glBlendFunc(GL_ONE, GL_ONE);

            // BACKGROUND scope texture
            glActiveTexture(GL_TEXTURE0);
            glBindTexture(GL_TEXTURE_2D, textures[0]);

            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR_MIPMAP_LINEAR);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

            using (var stream = File.OpenRead(backgroundFile))
            {
                ImageResult imageBackground = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                fixed (byte* bgPtr = imageBackground.Data)
                {
                    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, imageBackground.Width, imageBackground.Height, 0, GL_RGBA, GL_UNSIGNED_BYTE, bgPtr);
                    glGenerateMipmap(GL_TEXTURE_2D);
                }
            }

            // GREEN SPHERE texture for associated wifi clients
            glActiveTexture(GL_TEXTURE1);
            glBindTexture(GL_TEXTURE_2D, textures[1]);

            using (var stream = File.OpenRead(greenSphereFile))
            {
                ImageResult imageGreen = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                fixed (byte* gPtr = imageGreen.Data)
                {
                    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, imageGreen.Width, imageGreen.Height, 0, GL_RGBA, GL_UNSIGNED_BYTE, gPtr);
                    glGenerateMipmap(GL_TEXTURE_2D);
                }
            }

            // RED SPHERE texture for rogue wifi clients
            glActiveTexture(GL_TEXTURE2);
            glBindTexture(GL_TEXTURE_2D, textures[2]);

            using (var stream = File.OpenRead(redSphereFile))
            {
                ImageResult imageRed = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                fixed (byte* rPtr = imageRed.Data)
                {
                    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, imageRed.Width, imageRed.Height, 0, GL_RGBA, GL_UNSIGNED_BYTE, rPtr);
                    glGenerateMipmap(GL_TEXTURE_2D);
                }
            }

            // ACCESS POINT textures
            for (int i = 3; i < 8; i++)
            {
                glActiveTexture(GL_TEXTURE0 + i);
                glBindTexture(GL_TEXTURE_2D, textures[i]);

                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR_MIPMAP_LINEAR);
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

                string tmpFile = apFile + (i - 2) + ".png";
                using (var stream = File.OpenRead(tmpFile))
                {
                    ImageResult apImage = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                    fixed (byte* aPtr = apImage.Data)
                    {
                        glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, apImage.Width, apImage.Height, 0, GL_RGBA, GL_UNSIGNED_BYTE, aPtr);
                        glGenerateMipmap(GL_TEXTURE_2D);
                    }
                }
            }

            glBindTexture(GL_TEXTURE_2D, 0);

            return textures;
        }
    }
}
