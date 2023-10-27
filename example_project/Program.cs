

#region --- Using Directives ---

using System.Drawing.Drawing2D;
using OpenTK.Graphics.OpenGL;
using cgimin.engine.object3d;
using cgimin.engine.texture;
using cgimin.engine.material.simpletexture;
using cgimin.engine.camera;
using cgimin.engine.material.wobble2;
using cgimin.engine.light;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Runtime.InteropServices;
using cgimin.engine.material.ambientdiffuse;
using OpenTK.Windowing.Common.Input;

#endregion --- Using Directives ---

namespace Examples.Tutorial
{

    public class ExampleProject : GameWindow
    {

        private float cameraSpeed = 0.1f;
        
        // the 3D-Object we load
        private ObjLoaderObject3D exampleObject;
        private ObjLoaderObject3D street;
        
        // our texture-IDs
        private int woodTexture;
        private int cellshading;
        // Materials
        private SimpleTextureMaterial simpleTextureMaterial;
        private Wobble2Material wobbleMaterial;
        

        // Updating the time
        private float updateTime = 0;

        public ExampleProject(int width, int height, GameWindowSettings gameWindowSettings, 
            NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings) {
            Size = (width, height);
            this.KeyDown += KeyboardKeyDown;
        }


        void KeyboardKeyDown(KeyboardKeyEventArgs e)
        {
            if (e.Key == Keys.Escape)
                this.Close();

            if (e.Key == Keys.F11)
                if (this.WindowState == WindowState.Fullscreen)
                    this.WindowState = WindowState.Normal;
                else
                    this.WindowState = WindowState.Fullscreen;       

            
            //Press Backspace to reset the exampleObject to its original position
            if (e.Key == Keys.Backspace) 
            {
                Camera.Transformation = Matrix4.CreateTranslation(0, 0, 0);
            }
            
            
        }
        
        protected override void OnLoad()
        {
            
            base.OnLoad();
            // Set the mouse cursor to be a crosshair
            Cursor = MouseCursor.Crosshair;
            
            //Lighting
            // todo fill with data for ambient, diffuse and specular light between 0 and 1
            Light.SetDirectionalLight(new Vector3(1,1,1), new Vector4(1,1,1,1), new Vector4(1,1,1,1), new Vector4(1,1,1,1));
            
            //todo make the duck react to the light somehow --> perhaps from the slides 
            
            // Initialize Camera
            Camera.Init();
            Camera.SetWidthHeightFov(1920, 1080, 60);

            // Loading the object
            exampleObject = new ObjLoaderObject3D("data/objects/duck_smooth.obj");
            street = new ObjLoaderObject3D("data/objects/streetv1.obj");
            //Once the Object is loaded, put it in front of the camera
            exampleObject.Transformation *= Matrix4.CreateTranslation(0, 0, -5);
            street.Transformation *= Matrix4.CreateTranslation(0, -3, -5);
            

            // Loading the texture
            woodTexture = TextureManager.LoadTexture("data/textures/duck_texture.png");
            cellshading = TextureManager.LoadTexture("data/textures/cellshading.png");
            // initialize materials
            simpleTextureMaterial = new SimpleTextureMaterial();
            wobbleMaterial = new Wobble2Material();
            
            // enable z-buffer
            GL.Enable(EnableCap.DepthTest);

            // backface culling enabled
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);

        }
 

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            // updateCounter simply increases
            //updateTime += (float)e.Time;
            //Move the camera according to mouse input
            //Get the mouse input
            //var mouse = MouseState;
            //Get the change in mouse position and rotate the camera accordingly
            //var deltaX = mouse.Delta.X;
            //var deltaY = mouse.Delta.Y;
            //Camera.Transformation *= Matrix4.CreateRotationY(deltaX * 0.01f);
            //Camera.Transformation *= Matrix4.CreateRotationX(deltaY * 0.01f);
            
            //TODO Do we need this? perhaps let the cursor be freely movable and fixate the camera?
            //TODO Picking Ray? Camera Transformation und Mouse Position nutzen um einen Ray zu erstellen, der dann mit dem Objekt geschnitten wird
            //TODO https://www.youtube.com/watch?v=DLKN0jExRIM
            
            //Movement of the camera according to the keys pressed, only when within the boundaries
            if (KeyboardState.IsKeyDown(Keys.W) && Camera.Transformation.ExtractTranslation().Z < 10)
                Camera.Transformation *= Matrix4.CreateTranslation(0, 0, cameraSpeed);
            if (KeyboardState.IsKeyDown(Keys.S) && Camera.Transformation.ExtractTranslation().Z > -10)
                Camera.Transformation *= Matrix4.CreateTranslation(0, 0, -cameraSpeed);
            if (KeyboardState.IsKeyDown(Keys.A)&& Camera.Transformation.ExtractTranslation().X < 10)
                Camera.Transformation *= Matrix4.CreateTranslation(cameraSpeed, 0, 0);
            if (KeyboardState.IsKeyDown(Keys.D)&& Camera.Transformation.ExtractTranslation().X > -10)
                Camera.Transformation *= Matrix4.CreateTranslation(-cameraSpeed, 0, 0);
            if (KeyboardState.IsKeyDown(Keys.Space)&& Camera.Transformation.ExtractTranslation().Y > -10)
                Camera.Transformation *= Matrix4.CreateTranslation(0, -cameraSpeed, 0);
            if (KeyboardState.IsKeyDown(Keys.LeftControl) && Camera.Transformation.ExtractTranslation().Y < 0)
                Camera.Transformation *= Matrix4.CreateTranslation(0, cameraSpeed, 0);
            
            
            //Press Backspace to reset the exampleObject to its original position
            if (KeyboardState.IsKeyDown(Keys.Backspace))
                Camera.Transformation = Matrix4.CreateTranslation(0, 0, 0);
            // Camera speed up
            if (KeyboardState.IsKeyDown(Keys.LeftShift)) cameraSpeed = 0.2f;
            if (KeyboardState.IsKeyReleased(Keys.LeftShift)) cameraSpeed = 0.1f;
        }


        protected override void OnRenderFrame(FrameEventArgs e)
        {
            // specify the clear color
            GL.ClearColor(0.3f, 0.3f, 0.3f, 1.0f);

            // the screen color and the depth-buffer are cleared
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //exampleObject.Transformation = Matrix4.Identity;
            //exampleObject.Transformation *= Matrix4.CreateRotationX(updateTime);
            //exampleObject.Transformation *= Matrix4.CreateRotationY(updateTime);
            //exampleObject.Transformation *= Matrix4.CreateRotationZ(updateTime);
            //exampleObject.Transformation *= Matrix4.CreateTranslation(0, 0, -1);

            //simpleTextureMaterial.Draw(exampleObject, woodTexture);
            //wobbleMaterial.Draw(exampleObject, woodTexture, updateTime);
            AmbientDiffuseSpecularMaterial ambientDiffuseMaterial = new AmbientDiffuseSpecularMaterial();
            ambientDiffuseMaterial.Draw(exampleObject, woodTexture,5);
            ambientDiffuseMaterial.Draw(street, cellshading,5);
            SwapBuffers();
        }


        protected override void OnUnload()
        {
            exampleObject.UnLoad();
            street.UnLoad();
        }


        protected override void OnResize(ResizeEventArgs e)
        {
            Size = e.Size;
            GL.Viewport(0, 0, Size.X, Size.Y);
            Camera.SetWidthHeightFov(Size.X, Size.Y, 60);
        }


        [STAThread]
        public static void Main()
        {
            var windowSettings = GameWindowSettings.Default;
            windowSettings.UpdateFrequency = 120;

            var nativeWindowSettings = NativeWindowSettings.Default;
            nativeWindowSettings.Flags = ContextFlags.ForwardCompatible | ContextFlags.Debug;
            nativeWindowSettings.Title = "CGI-MIN Example";
            nativeWindowSettings.WindowState = WindowState.Fullscreen;

            using var example = new ExampleProject(1920, 1080, windowSettings, nativeWindowSettings);
            example.Run();
        }
    }
}