using System.Diagnostics;
using static OpenGL.GL;

namespace MerakiLocationVisualizer
{
    public static class Shader
    {
        readonly static string verShaderCode = @"#version 330 core
                                layout (location = 0) in vec2 aPosition;
                                layout (location = 1) in vec4 aRGBA;
                                layout (location = 2) in vec2 aTexCoords;
                                out vec2 Position;
                                out vec4 vertexColor;
                                out vec2 TexCoords;

                                uniform float alphaTime;
                                uniform int isAP;
                                uniform int isBackground;
                                uniform int isClient;
                                uniform int isSweep;

                                void main() 
                                {
                                    gl_Position = vec4(aPosition.xy, 0.0, 1.0);
                            
                                    if(isAP == 1) {
                                        vertexColor = vec4(1.0, 1.0, 1.0, 1.0); 
                                    }    
                                    else if(isBackground == 1) {
                                        vertexColor = vec4(aRGBA.rgba); 
                                    }
                                    else if(isClient == 1) {
                                        vertexColor = vec4(alphaTime, alphaTime, alphaTime, 1.0); 
                                    }
                                    else if(isSweep == 1) {
                                        vertexColor = vec4(aRGBA.rgb, .5 * alphaTime + .25);
                                    }

                                    TexCoords = aTexCoords;
                                    Position = aPosition;
                                }";

        readonly static string fragShaderCode = @"#version 330 core
                                out vec4 FragColor;
                                in vec2 Position;
                                in vec4 vertexColor;
                                in vec2 TexCoords;

                                uniform sampler2D backgroundTexture;
                                uniform sampler2D greenTexture;
                                uniform sampler2D redTexture;
                                uniform sampler2D apTexture;

                                uniform int isAp;
                                uniform int isBackground;
                                uniform int isClient;
                                uniform int isSweep;

                                void main()
                                {   
                                    if(isAp == 1)
                                    {
                                        FragColor = texture(apTexture, TexCoords);
                                    }
                                    else if(isBackground == 1)
                                    {
                                        FragColor = texture(backgroundTexture, TexCoords) * vertexColor;
                                    }
                                    else if(isClient == 1) 
                                    {
                                        if(Position.x >= 0) 
                                        {
                                            FragColor = texture(greenTexture, TexCoords) * vertexColor;                                    
                                        }
                                        else 
                                        {
                                            FragColor = texture(redTexture, TexCoords) * vertexColor;
                                        }
                                        
                                    }
                                    else
                                    {
                                        FragColor = vertexColor;
                                    }
                                }";

        public static uint ProgramID { get; set; }

        public static void Load()
        {
            uint verShaderID, fragShaderID;

            verShaderID = glCreateShader(GL_VERTEX_SHADER);
            glShaderSource(verShaderID, verShaderCode);
            glCompileShader(verShaderID);

            int[] status = glGetShaderiv(verShaderID, GL_COMPILE_STATUS, 1);

            if (status[0] == 0)
            {
                string error = glGetShaderInfoLog(verShaderID);
                Debug.WriteLine(error);
            }

            fragShaderID = glCreateShader(GL_FRAGMENT_SHADER);
            glShaderSource(fragShaderID, fragShaderCode);
            glCompileShader(fragShaderID);

            status = glGetShaderiv(fragShaderID, GL_COMPILE_STATUS, 1);

            if (status[0] == 0)
            {
                string error = glGetShaderInfoLog(fragShaderID);
                Debug.WriteLine(error);
            }

            ProgramID = glCreateProgram();
            glAttachShader(ProgramID, verShaderID);
            glAttachShader(ProgramID, fragShaderID);
            glLinkProgram(ProgramID);

            status = glGetProgramiv(ProgramID, GL_LINK_STATUS, 1);

            if (status[0] == 0)
            {
                string error = glGetProgramInfoLog(ProgramID);
                Debug.WriteLine(error);
            }

            glDetachShader(ProgramID, verShaderID);
            glDeleteShader(verShaderID);

            glDetachShader(ProgramID, fragShaderID);
            glDeleteShader(fragShaderID);
        }

        public static void Use()
        {
            glUseProgram(ProgramID);
        }
    }
}
