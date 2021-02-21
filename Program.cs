using System;
using System.Net;
using GLFW;
using static OpenGL.GL;

// CMD LINE ARGUMENTS:
//
// listen for reports only: program.exe --listenonly
//
// visualize existing data only: program.exe -w window-width -h window-height --viewonly
//
// listen and vizualize: program.exe -w window-width -h window-height
//

// launch VS (or exe) as Administrator in order to bind HTTP listener to anything other than 127.0.0.1
//
// set project framework to .NET 5.0 to support JsonNumberHandling.AllowNamedFloatingPointLiterals
//
// set project to allow unsafe code (pointers are required by OpenGL stuff)

namespace MerakiLocationVisualizer
{
    class Program
    {
        static bool Running { get; set; }
        static int myWidth;
        static int myHeight;
        private static KeyCallback mlvKeyCallback; // required to keep c# garbage collector from moving the callbacks

        static void Main(string[] args)
        {
            bool listenOnlyMode = false;
            bool viewOnlyMode = false;
            bool mixedMode = false;
            int height, width;
            string title;
            HTTPServer httpServer = null;

            if (args.Length == 1 && args[0] == "--listenonly")
            {
                listenOnlyMode = true;
            }
            else if (args.Length == 3)
            {
                if (args[0] != "-h" || Convert.ToInt32(args[1]) < 1 || args[2] != "--viewonly")
                {
                    Console.WriteLine("\nUSAGE:");
                    Console.WriteLine("======================================================================================");
                    Console.WriteLine("mlv.exe --listenonly (listen for new data only)");
                    Console.WriteLine("mlv.exe -h window-height --viewonly (visualize existing data only)");
                    Console.WriteLine("mlv.exe -h window-height (listen and visualize)");

                    return;
                }
                viewOnlyMode = true;
            }
            else if (args.Length == 2)
            {
                if (args[0] != "-h" || Convert.ToInt32(args[1]) < 1)
                {
                    Console.WriteLine("\nUSAGE:");
                    Console.WriteLine("======================================================================================");
                    Console.WriteLine("mlv.exe --listenonly (listen for new data only)");
                    Console.WriteLine("mlv.exe -h window-height --viewonly (visualize existing data only)");
                    Console.WriteLine("mlv.exe -h window-height (listen and visualize)");

                    return;
                }
                mixedMode = true;
            }
            else
            {
                Console.WriteLine("\nUSAGE:");
                Console.WriteLine("======================================================================================");
                Console.WriteLine("mlv.exe --listenonly (listen for new data only)");
                Console.WriteLine("mlv.exe -h window-height --viewonly (visualize existing data only)");
                Console.WriteLine("mlv.exe -h window-height (listen and visualize)");

                return;
            }

            if (listenOnlyMode || mixedMode)
            { 
                // launch the http server thread
                var httpListener = new HttpListener();

                // local machine binding
                //httpServer = new HTTPServer(httpListener, "https://IP.ADDRESS:PORT/URL/");

                httpServer = new HTTPServer(httpListener, "https://192.168.100.100:1234/MerakiLocationVisualizer/");

                httpServer.Start();
            }

            if (viewOnlyMode || mixedMode)
            {
                height = Convert.ToInt32(args[1]);
                width = height; // hard coding square viewport
                myHeight = height;
                myWidth = width;
                title = "WiFi Radar";

                // load the initial batch of observation reports
                bool successfulDataInit = DataSelector.InitData(width, height);

                if (VerticeData.NumberOfAPs == 0)
                {
                    Console.WriteLine("There are no reporting access points.  Aborting!");
                    Console.Read();

                    return;
                }

                if (!successfulDataInit)
                {
                    Console.WriteLine("There is no observation data in the database.  Aborting!");
                    Console.Read();

                    return;
                }

                Running = true;

                Glfw.Init();

                DisplayManager.CreateWindow(height, width, title);

                // input events registered below in KeyCallBack()
                Glfw.SetKeyCallback(DisplayManager.Window, mlvKeyCallback = KeyCallback);

                uint[] textures = Renderer.LoadTextures();

                Shader.Load();

                float degrees = 0f;
                float sweepDegrees = 0f;
                bool isAscending = true;

                // visualization loop
                while (!Glfw.WindowShouldClose(DisplayManager.Window) && Running)
                {
                    glClearColor(0, 0, 0, 0);
                    glClear(GL_COLOR_BUFFER_BIT);

                    Shader.Use();

                    // BACKGROUND GRAPHIC
                    glActiveTexture(GL_TEXTURE1);
                    glBindTexture(GL_TEXTURE_2D, textures[0]);
                    glUniform1i(glGetUniformLocation(Shader.ProgramID, "backgroundTexture"), 0);

                    // tell the shaders what is being sent to them
                    glUniform1i(glGetUniformLocation(Shader.ProgramID, "isAp"), 0);
                    glUniform1i(glGetUniformLocation(Shader.ProgramID, "isBackground"), 1);
                    glUniform1i(glGetUniformLocation(Shader.ProgramID, "isClient"), 0);
                    glUniform1i(glGetUniformLocation(Shader.ProgramID, "isSweep"), 0);

                    float alphaTime = ((((float)Math.Sin(degrees * 3.0f)) + 1f) / 2) + .05f;
                    glUniform1f(glGetUniformLocation(Shader.ProgramID, "alphaTime"), alphaTime);

                    uint vao = Renderer.Render(VerticeData.backgroundVertices());

                    glBindVertexArray(vao);

                    glDrawArrays(GL_TRIANGLES, 0, VerticeData.backgroundVertices().Length / 8);

                    glBindVertexArray(0);
                    glDeleteVertexArray(vao);
                    glBindTexture(GL_TEXTURE_2D, 0);

                    // RADAR SWEEP GRAPHIC
                    glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

                    glUniform1i(glGetUniformLocation(Shader.ProgramID, "isAp"), 0);
                    glUniform1i(glGetUniformLocation(Shader.ProgramID, "isBackground"), 0);
                    glUniform1i(glGetUniformLocation(Shader.ProgramID, "isClient"), 0);
                    glUniform1i(glGetUniformLocation(Shader.ProgramID, "isSweep"), 1);

                    vao = Renderer.Render(VerticeData.sweepVertices(sweepDegrees));

                    if (sweepDegrees == 360)
                    {
                        sweepDegrees = 0.0f;
                    }
                    else
                    {
                        sweepDegrees += 0.001f;
                    }

                    glBindVertexArray(vao);

                    glDrawArrays(GL_TRIANGLES, 0, 3);

                    glBindVertexArray(0);
                    glDeleteVertexArray(vao);

                    // WIFI CLIENT DEVICE GRAPHICS
                    glBlendFunc(GL_ONE, GL_ONE);

                    glActiveTexture(GL_TEXTURE1);
                    glBindTexture(GL_TEXTURE_2D, textures[1]);
                    glUniform1i(glGetUniformLocation(Shader.ProgramID, "greenTexture"), 1);

                    glActiveTexture(GL_TEXTURE2);
                    glBindTexture(GL_TEXTURE_2D, textures[2]);
                    glUniform1i(glGetUniformLocation(Shader.ProgramID, "redTexture"), 2);

                    glUniform1i(glGetUniformLocation(Shader.ProgramID, "isAp"), 0);
                    glUniform1i(glGetUniformLocation(Shader.ProgramID, "isBackground"), 0);
                    glUniform1i(glGetUniformLocation(Shader.ProgramID, "isClient"), 1);
                    glUniform1i(glGetUniformLocation(Shader.ProgramID, "isSweep"), 0);

                    if (isAscending)
                    {
                        degrees += 0.0012f;
                    }
                    else
                    {
                        degrees -= 0.0012f;
                    }
                    if (degrees == 0)
                    {
                        isAscending = true;
                    }
                    else if (degrees == 360.0f)
                    {
                        isAscending = false;
                    }

                    vao = Renderer.Render(VerticeData.SquareVertices);
                    glBindVertexArray(vao);

                    glDrawArrays(GL_TRIANGLES, 0, VerticeData.SquareVertices.Length / 8);

                    glBindVertexArray(0);
                    glDeleteVertexArray(vao);
                    glBindTexture(GL_TEXTURE_2D, 0);

                    // ACCESS POINT GRAPHIC
                    glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

                    glActiveTexture(GL_TEXTURE + DataSelector.currentAP + 3);
                    glBindTexture(GL_TEXTURE_2D, textures[DataSelector.currentAP + 3]);
                    glUniform1i(glGetUniformLocation(Shader.ProgramID, "apTexture"), DataSelector.currentAP + 3);

                    glUniform1i(glGetUniformLocation(Shader.ProgramID, "isAp"), 1);
                    glUniform1i(glGetUniformLocation(Shader.ProgramID, "isBorder"), 0);
                    glUniform1i(glGetUniformLocation(Shader.ProgramID, "isClient"), 0);
                    glUniform1i(glGetUniformLocation(Shader.ProgramID, "isSweep"), 0);

                    float[] tmpVertices = VerticeData.apVertices();
                    vao = Renderer.Render(tmpVertices);
                    glBindVertexArray(vao);

                    glDrawArrays(GL_TRIANGLES, 0, tmpVertices.Length / 8);

                    glBindVertexArray(0);
                    glDeleteVertexArray(vao);
                    glBindTexture(GL_TEXTURE_2D, 0);

                    Glfw.SwapBuffers(DisplayManager.Window);
                    Glfw.PollEvents();
                }

                DisplayManager.DestroyWindow();
            }
            else // listen only
            {
                ConsoleKey input;

                do
                {
                    input = Console.ReadKey(true).Key;

                    if (input == ConsoleKey.Escape)
                    {
                        httpServer.Stop();
                        Console.WriteLine("Listener terminated...");
                    }
                } while (input != ConsoleKey.Escape);
            }
        }

        private static void KeyCallback(Window window, Keys key, int scanCode, InputState state, ModifierKeys mods)
        {
            if (key == GLFW.Keys.W && state == GLFW.InputState.Press)
            {
                // load following minute's observations
                VerticeData.VerticeSlice = DataSelector.FetchNewerRecords(VerticeData.NormalizedReports, DataSelector.currentAP);
                VerticeData.SquareVertices = DataTransform.GenerateQuads(VerticeData.VerticeSlice, DisplayManager.ViewportWidth, DisplayManager.ViewportHeight);
            }
            else if (key == GLFW.Keys.S && state == GLFW.InputState.Press)
            {
                // load preceeding minute
                VerticeData.VerticeSlice = DataSelector.FetchOlderRecords(VerticeData.NormalizedReports, DataSelector.currentAP);
                VerticeData.SquareVertices = DataTransform.GenerateQuads(VerticeData.VerticeSlice, DisplayManager.ViewportWidth, DisplayManager.ViewportHeight);
            }
            else if (key == GLFW.Keys.A && state == GLFW.InputState.Press)
            {
                // load previous AP's data
                DataSelector.currentAP = DataSelector.currentAP - 1;
                if (DataSelector.currentAP < 0)
                {
                    DataSelector.currentAP = VerticeData.NumberOfAPs - 1;
                }
                VerticeData.VerticeSlice = DataSelector.FetchOlderRecords(VerticeData.NormalizedReports, DataSelector.currentAP);
                VerticeData.SquareVertices = DataTransform.GenerateQuads(VerticeData.VerticeSlice, DisplayManager.ViewportWidth, DisplayManager.ViewportHeight);
            }
            else if (key == GLFW.Keys.D && state == GLFW.InputState.Press)
            {
                // load next AP's data
                DataSelector.currentAP = DataSelector.currentAP + 1;
                if (DataSelector.currentAP > VerticeData.NumberOfAPs - 1)
                {
                    DataSelector.currentAP = 0;
                }
                VerticeData.VerticeSlice = DataSelector.FetchOlderRecords(VerticeData.NormalizedReports, DataSelector.currentAP);
                VerticeData.SquareVertices = DataTransform.GenerateQuads(VerticeData.VerticeSlice, DisplayManager.ViewportWidth, DisplayManager.ViewportHeight);
            }
            else if (key == GLFW.Keys.R && state == GLFW.InputState.Press)
            {
                // pull fresh observation data from the DB
                DataSelector.InitData(myWidth, myHeight);

                VerticeData.VerticeSlice = DataSelector.FetchOlderRecords(VerticeData.NormalizedReports, DataSelector.currentAP);
                VerticeData.SquareVertices = DataTransform.GenerateQuads(VerticeData.VerticeSlice, DisplayManager.ViewportWidth, DisplayManager.ViewportHeight);
            }
            else if (key == GLFW.Keys.Escape && state == GLFW.InputState.Press)
            {
                // die
                Running = false;
            }

        }

    }
}

