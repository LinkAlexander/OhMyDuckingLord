

#region --- Using Directives ---

using System.Drawing;
using System.Numerics;
using System.Security.Principal;
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
using cgimin.engine.material.ambientdiffuse;
using OpenTK.Windowing.Common.Input;
using Vector3 = OpenTK.Mathematics.Vector3;
using Vector4 = OpenTK.Mathematics.Vector4;

#endregion --- Using Directives ---

namespace cgi
{

    public class ExampleProject : GameWindow
    {

        private float cameraSpeed = 0.001f;
        
        // the 3D-Object we load
        private List<ObjLoaderObject3D> ducks;
        private ObjLoaderObject3D street = null!;

        private ObjLoaderObject3D rayStartMarker;
        private ObjLoaderObject3D rayEndMarker;
        
        // our texture-IDs
        private int woodTexture;
        private int cellshading;
        // Materials
        private SimpleTextureMaterial simpleTextureMaterial;
        private Wobble2Material wobbleMaterial;

        private PickingRay pickingRay;

        // Updating the time
        private float updateTime;

        public ExampleProject(int width, int height, GameWindowSettings gameWindowSettings, 
            NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings) {
            Size = (width, height);
            KeyDown += KeyboardKeyDown;
            // initialize materials
            simpleTextureMaterial = new SimpleTextureMaterial();
            wobbleMaterial = new Wobble2Material();
            ducks = new List<ObjLoaderObject3D>();
        }


        void KeyboardKeyDown(KeyboardKeyEventArgs e)
        {
            if (e.Key == Keys.Escape)
                Close();

            if (e.Key == Keys.F11)
                if (WindowState == WindowState.Fullscreen)
                    WindowState = WindowState.Normal;
                else
                    WindowState = WindowState.Fullscreen;       

            
            //Press Backspace to reset the exampleObject to its original position
            if (e.Key == Keys.Backspace) 
            {
                Camera.Transformation = Matrix4.CreateTranslation(0, 0, 0);
            }

            if (e.Key == Keys.Up)
            {
                Camera.Transformation *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(90));
            }
            if (e.Key == Keys.Right)
            {
                Camera.Transformation *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(90));
            }
            
            
        }
        
        protected override void OnLoad()
        {
            
            base.OnLoad();
            updateTime = 0;
            //Lighting
            Light.SetDirectionalLight(new Vector3(1,1,1), new Vector4(1,1,1,1), new Vector4(1,1,1,1), new Vector4(1,1,1,1));
            
            // Initialize Camera
            Camera.Init();
            Camera.SetWidthHeightFov(1920, 1080, 60);
            
            // Loading the object
            ducks.Add(new ObjLoaderObject3D("data/objects/duck_smooth.obj"));
            ducks.Add(new ObjLoaderObject3D("data/objects/duck_smooth.obj"));
            ducks.Add(new ObjLoaderObject3D("data/objects/duck_smooth.obj"));
            street = new ObjLoaderObject3D("data/objects/simpleStreet.obj");
            //Once the Object is loaded, put it in front of the camera
            int count = 0; 
            street.Transformation *= Matrix4.CreateTranslation(10, -5, -10);
            foreach (var duck in ducks)
            {
                duck.Transformation *= Matrix4.CreateTranslation(count * 5, 0, -5);
                count++;
            }
            rayStartMarker = new ObjLoaderObject3D("data/objects/cube.obj");
            rayEndMarker = new ObjLoaderObject3D("data/objects/cube.obj");
            
            // Loading the texture
            woodTexture = TextureManager.LoadTexture("data/textures/duck_texture.png");
            cellshading = TextureManager.LoadTexture("data/textures/cellshading.png");

            
            // enable z-buffer
            GL.Enable(EnableCap.DepthTest);

            // backface culling enabled
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);

        }
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
             
            float mouseX = MousePosition.X;
            float mouseY = -MousePosition.Y/2 + Size.Y*0.75f;
            Vector3 nearPoint = Camera.Transformation.Inverted().ExtractTranslation();
                Vector3 farPoint =
                Vector3.Unproject(new Vector3(mouseX, mouseY, -0.01f), Camera.Transformation.ExtractTranslation().X,
                    Camera.Transformation.ExtractTranslation().Y,
                    Size.X, Size.Y, 0.01f, 1000.0f, Camera.Transformation.Inverted());
            //https://gamedev.stackexchange.com/questions/51820/how-can-i-convert-screen-coordinatess-to-world-coordinates-in-opentk.
            // Vllt einfach den Punkt setzen und X und Y um einen Faktor skalieren?
            //Unproject durchgehen und verstehen
            Console.WriteLine(farPoint);
            
            rayStartMarker.Transformation = Matrix4.CreateTranslation(nearPoint);
            rayStartMarker.ScaleInPlace(0.05f);
            rayEndMarker.Transformation = Matrix4.CreateTranslation(farPoint);
            rayEndMarker.ScaleInPlace(0.05f);
            
            pickingRay = new PickingRay(nearPoint, farPoint );
            
            
            foreach (var duck in ducks)
            {
                if (duck.RayIntersectsObject(pickingRay))
                {
                    duck.RotateObjectX(MathHelper.DegreesToRadians(90));
                }
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            // updateCounter simply increases
            updateTime += (float)e.Time;
            //Move the camera according to mouse input
            //Get the change in mouse position and rotate the camera accordingly
            //var deltaX = MouseState.Delta.X;
            //var deltaY = MouseState.Delta.Y;
            //Camera.Transformation *= Matrix4.CreateRotationY(deltaX * 0.01f);
            //Camera.Transformation *= Matrix4.CreateRotationX(deltaY * 0.01f);
            //TODO Do we need this? perhaps let the cursor be freely movable and fixate the camera?

            int bound = 10;
            //Movement of the camera according to the keys pressed, only when within the boundaries
            if (KeyboardState.IsKeyDown(Keys.W) && Camera.Transformation.ExtractTranslation().Z < bound)
                Camera.Transformation *= Matrix4.CreateTranslation(0, 0, cameraSpeed);
            if (KeyboardState.IsKeyDown(Keys.S) && Camera.Transformation.ExtractTranslation().Z > -bound)
                Camera.Transformation *= Matrix4.CreateTranslation(0, 0, -cameraSpeed);
            if (KeyboardState.IsKeyDown(Keys.A)&& Camera.Transformation.ExtractTranslation().X < bound)
                Camera.Transformation *= Matrix4.CreateTranslation(cameraSpeed, 0, 0);
            if (KeyboardState.IsKeyDown(Keys.D)&& Camera.Transformation.ExtractTranslation().X > -bound)
                Camera.Transformation *= Matrix4.CreateTranslation(-cameraSpeed, 0, 0);
            if (KeyboardState.IsKeyDown(Keys.Space)&& Camera.Transformation.ExtractTranslation().Y > -bound)
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
            
            AmbientDiffuseSpecularMaterial ambientDiffuseMaterial = new AmbientDiffuseSpecularMaterial();
            ambientDiffuseMaterial.Draw(street, cellshading,5);
            foreach (var duck in ducks)
            {
                ambientDiffuseMaterial.Draw(duck, woodTexture,5);
            }
            
            if (rayStartMarker != null)
            {
                ambientDiffuseMaterial.Draw(rayStartMarker,cellshading, 5);
            }

            if (rayEndMarker != null)
            {
                ambientDiffuseMaterial.Draw(rayEndMarker,cellshading, 5);  
            }
            
            
            SwapBuffers();
        }


        protected override void OnUnload()
        {
            foreach (var duck in ducks)
            {
                duck.UnLoad();
            }

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