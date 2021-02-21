using System.Drawing;
using GLFW;
using static OpenGL.GL;

namespace MerakiLocationVisualizer
{
    public static class DisplayManager
    {
        public static Window Window { get; set; }
        public static float ViewportWidth { get; set; }
        public static float ViewportHeight { get; set; }

        public static void CreateWindow(int width, int height, string title)
        {
            Glfw.WindowHint(Hint.ClientApi, ClientApi.OpenGL);
            Glfw.WindowHint(Hint.ContextVersionMajor, 3);
            Glfw.WindowHint(Hint.ContextVersionMinor, 3);
            Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);
            Glfw.WindowHint(Hint.Doublebuffer, true);
            Glfw.WindowHint(Hint.Decorated, true);
            Glfw.WindowHint(Hint.OpenglDebugContext, true);

            Window = Glfw.CreateWindow(width, height, title, Monitor.None, Window.None);

            if(Window == Window.None)
            {
                return; // window creation failed - bail
            }

            // center the window
            Rectangle screen = Glfw.PrimaryMonitor.WorkArea;
            int x = (screen.Width - width) / 2;
            int y = (screen.Height - height) / 2;
            Glfw.SetWindowPosition(Window, x, y);

            Glfw.MakeContextCurrent(Window);
            Import(Glfw.GetProcAddress);

            ViewportWidth = (float)width;
            ViewportHeight = (float)height;
            glViewport(0, 0, width, height);

            Glfw.SwapInterval(0);  // vsync 0=off
        }

        public static void DestroyWindow()
        {
            Glfw.Terminate();
        }
    }

}
