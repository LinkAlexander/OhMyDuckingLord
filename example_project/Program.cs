

#region --- Using Directives ---
using OpenTK.Graphics.OpenGL;
using cgimin.engine.object3d;
using cgimin.engine.texture;
using cgimin.engine.material.simpletexture;
using cgimin.engine.camera;
using cgimin.engine.gui;
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

namespace cgi;

public class ExampleProject : GameWindow
{

    private float cameraSpeed = 0.01f;

    private BitmapGraphic logoSprite;
    private BitmapFont bitmapFont;

    // the 3D-Object we load
    private List<ObjLoaderObject3D> ducks;
    private ObjLoaderObject3D street = null!;

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
        : base(gameWindowSettings, nativeWindowSettings)
    {
        Size = (width, height);
        KeyDown += KeyboardKeyDown;
        // initialize materials
        simpleTextureMaterial = new SimpleTextureMaterial();
        wobbleMaterial = new Wobble2Material();
        ducks = new List<ObjLoaderObject3D>();
        Cursor = MouseCursor.Crosshair;
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

        if (e.Key == Keys.Space)
            PickDuck();
        //Press Backspace to reset the exampleObject to its original position
        if (e.Key == Keys.Backspace)
            resetCamera();

    }

    void resetCamera()
    {
        Camera.Transformation = Matrix4.CreateTranslation(0,-3,0);
        Camera.Transformation *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(45));
    }
    protected override void OnLoad()
    {

        base.OnLoad();
        updateTime = 0;
        //Lighting
        Light.SetDirectionalLight(new Vector3(1, 1, 1), new Vector4(1, 1, 1, 1), new Vector4(1, 1, 1, 1),
            new Vector4(1, 1, 1, 1));
        // Initialize Camera
        Camera.Init();
        Camera.SetWidthHeightFov(1920, 1080, 60);
        resetCamera();
        
        int duckCount = 5;
        // Loading the object
        for (int i = 0; i < duckCount; i++)
        {
            ducks.Add(new ObjLoaderObject3D("data/objects/duck_smooth.obj"));            
        }
        
        street = new ObjLoaderObject3D("data/objects/bigscene.obj");
        //Once the Object is loaded, put it in front of the camera
         
        street.Transformation *= Matrix4.CreateTranslation(0, -5, -10);
        int duckPlacementCounter = -2;
        foreach (var duck in ducks)
        {
            duck.Transformation = Matrix4.CreateTranslation(duckPlacementCounter * 5, 0, -5);
            duckPlacementCounter++;
        }
            
        // Loading the texture
        woodTexture = TextureManager.LoadTexture("data/textures/duck_texture.png");
        cellshading = TextureManager.LoadTexture("data/textures/bigscene1.png");

        int spriteTexture = TextureManager.LoadTexture("data/textures/sprites.png");
        logoSprite = new BitmapGraphic(spriteTexture, 256, 256, 10, 110, 196, 120);
        bitmapFont = new BitmapFont("data/fonts/abel_normal.fnt", "data/fonts/abel_normal.png");    
        
        // enable z-buffer
        GL.Enable(EnableCap.DepthTest);

        // backface culling enabled
        GL.Enable(EnableCap.CullFace);
        GL.CullFace(CullFaceMode.Front);
        
    }

    private int score = 0;
    protected void PickDuck()
    {
        Vector3 nearPoint = Camera.Transformation.Inverted().ExtractTranslation();
        Vector3 farPoint =
            Vector3.Unproject(new Vector3(Size.X/2, Size.Y/2, nearPoint.Z - 1f), Camera.Transformation.ExtractTranslation().X,
                Camera.Transformation.ExtractTranslation().Y,
                Size.X, Size.Y, 1f, 1000.0f, Camera.Transformation.Inverted());
        pickingRay = new PickingRay(nearPoint, farPoint );
        foreach (var duck in ducks)
        {
            if (!duck.RayIntersectsObject(pickingRay)) continue;
            duck.Transformation *= Matrix4.CreateTranslation(0, -500, 0);
            score++;
            Console.WriteLine("Score: " + score);
            cameraSpeed += 0.05f;
        }
    }
        
    protected override void OnUpdateFrame(FrameEventArgs e)
    {
            
        // updateCounter simply increases
        updateTime += (float)e.Time;

        int bound = 25;
        //Movement of the camera according to the keys pressed, only when within the boundaries
        /*if (KeyboardState.IsKeyDown(Keys.W) && Camera.Transformation.ExtractTranslation().Z < bound)
            Camera.Transformation *= Matrix4.CreateTranslation(0, 0, cameraSpeed);
        if (KeyboardState.IsKeyDown(Keys.S) && Camera.Transformation.ExtractTranslation().Z > -bound)
            Camera.Transformation *= Matrix4.CreateTranslation(0, 0, -cameraSpeed);
        if (KeyboardState.IsKeyDown(Keys.A)&& Camera.Transformation.ExtractTranslation().X < bound)
            Camera.Transformation *= Matrix4.CreateTranslation(cameraSpeed, 0, 0);
        if (KeyboardState.IsKeyDown(Keys.D)&& Camera.Transformation.ExtractTranslation().X > -bound)
            Camera.Transformation *= Matrix4.CreateTranslation(-cameraSpeed, 0, 0);*/


        if (KeyboardState.IsKeyDown(Keys.W) && Camera.Transformation.ExtractTranslation().Z < bound)
            Camera.Transformation *= Matrix4.CreateTranslation(new Vector3(0,-cameraSpeed,cameraSpeed));
        if (KeyboardState.IsKeyDown(Keys.S) && -Camera.Transformation.ExtractTranslation().Z < bound)
            Camera.Transformation *= Matrix4.CreateTranslation(new Vector3(0,cameraSpeed,-cameraSpeed));
        if (KeyboardState.IsKeyDown(Keys.A) && Camera.Transformation.ExtractTranslation().X < bound)
            Camera.Transformation *= Matrix4.CreateTranslation(new Vector3(cameraSpeed,0,0));
        if (KeyboardState.IsKeyDown(Keys.D) && -Camera.Transformation.ExtractTranslation().X < bound)
            Camera.Transformation *= Matrix4.CreateTranslation(new Vector3(-cameraSpeed,0,0));
        if(KeyboardState.IsKeyDown(Keys.Left))
            Camera.Transformation *= Matrix4.CreateRotationY(-cameraSpeed);
        if(KeyboardState.IsKeyDown(Keys.Right))
            Camera.Transformation *= Matrix4.CreateRotationY(cameraSpeed);
    }


    protected override void OnRenderFrame(FrameEventArgs e)
    {
        MousePosition = new Vector2(Size.X/2, Size.Y/2);
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
        GL.Disable(EnableCap.CullFace);

        
        bitmapFont.DrawString("Score: " + score, -Size.X/2, -Size.Y/2, 255, 255, 255, 255);

        GL.Enable(EnableCap.CullFace);
        
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