

#region --- Using Directives ---

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

#endregion --- Using Directives ---

namespace Examples.Tutorial
{

    public class ExampleProject : GameWindow
    {
        // the 3D-Object we load
        private ObjLoaderObject3D exampleObject;

        // our textur-IDs
        private int woodTexture;

        // Materials
        private SimpleTextureMaterial simpleTextureMaterial;
        private Wobble2Material wobbleMaterial;


        // Updating the time
        private float updateTime = 0;

        public ExampleProject(int width, int height, GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
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
            // Move the exampleObject forward when pressing the w key
            if(e.Key == Keys.W)
                exampleObject.Transformation *= Matrix4.CreateTranslation(0, 0, -0.1f);
            // Move the exampleObject backward when pressing the s key
            if(e.Key == Keys.S)
                exampleObject.Transformation *= Matrix4.CreateTranslation(0, 0, 0.1f);
            //Move the exampleObject left when pressing the a key
            if(e.Key == Keys.A)
                exampleObject.Transformation *= Matrix4.CreateTranslation(-0.1f, 0, 0);
            //Move the exampleObject right when pressing the d key
            if(e.Key == Keys.D)
                exampleObject.Transformation *= Matrix4.CreateTranslation(0.1f, 0, 0);
            
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            // Initialize Camera
            Camera.Init();
            Camera.SetWidthHeightFov(1920, 1080, 60);

            // Loading the object
            exampleObject = new ObjLoaderObject3D("data/objects/duck_smooth.obj");

            // Loading the texture
            woodTexture = TextureManager.LoadTexture("data/textures/duck_texture.png");

            // initialize materials
            simpleTextureMaterial = new SimpleTextureMaterial();
            wobbleMaterial = new Wobble2Material();
            
            // enebale z-buffer
            GL.Enable(EnableCap.DepthTest);

            // backface culling enabled
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);         
        }
 

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            // updateCounter simply increaes
            updateTime += (float)e.Time;
         
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

            simpleTextureMaterial.Draw(exampleObject, woodTexture);
            //wobbleMaterial.Draw(exampleObject, woodTexture, updateTime);

            SwapBuffers();
        }


        protected override void OnUnload()
        {
            exampleObject.UnLoad();
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

