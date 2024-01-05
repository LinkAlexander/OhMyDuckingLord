

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
using Microsoft.VisualBasic.CompilerServices;
using OpenTK.Windowing.Common.Input;
using Vector3 = OpenTK.Mathematics.Vector3;
using Vector4 = OpenTK.Mathematics.Vector4;

#endregion --- Using Directives ---

namespace cgi;

public class ExampleProject : GameWindow
{

    private float cameraSpeed = 0.15f;

    private BitmapGraphic logoSprite;
    private BitmapFont bitmapFont;
    private BitmapFont timermapFont;

    // the 3D-Object we load
    private List<ObjLoaderObject3D> ducks;
    private ObjLoaderObject3D street = null!;

    private int randomDuckNumber;
    
    // our texture-IDs
    private int woodTexture;

    private int cellshading;

    // Materials
    private SimpleTextureMaterial simpleTextureMaterial;
    private Wobble2Material wobbleMaterial;
    private List<Vector3> duckPositions = new List<Vector3>();
    Vector3 unreachableposition = new Vector3(99999, 99999, 9999);
    private PickingRay pickingRay;

    // Updating the time
    private float updateTime;
    
    //Gameplay Timer 
    private static float timer; 
    private readonly  float starttimer = 3;
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
        Camera.Transformation = Matrix4.CreateTranslation(0,-20,-10);
        Camera.Transformation *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(45));
    }
    protected override void OnLoad()
    {

        base.OnLoad();
        updateTime = 0;
        //Lighting
        Light.SetDirectionalLight(new Vector3(1, 1, 1), new Vector4(0.5f, 0.5f, 0.5f, 1), new Vector4(0.5f, 0.5f, 0.5f, 1),
            new Vector4(0f, 0f, 0f, 1));
        // Initialize Camera
        Camera.Init();
        Camera.SetWidthHeightFov(1920, 1080, 60);
        resetCamera();
        
        duckPositions.Add(new Vector3(10.3f,3.8f,-17.3f)); //Ente Bank
        duckPositions.Add(new Vector3(-3,4f,-8f)); //Ente Auto
        duckPositions.Add(new Vector3(17.5f,5f,5f)); //Ente Brunnen
        duckPositions.Add(new Vector3(-17.5f,10f,-27f)); //Ente Schornstein
        duckPositions.Add(new Vector3(-10f,6.5f,12f)); //Ente Baum unten links
        duckPositions.Add(new Vector3(14.5f,3.5f,-27f)); //Ente Haustür oben rechts
        duckPositions.Add(new Vector3(-20f,5.5f,-6f)); //Ente Haus Fenster mitte links
        duckPositions.Add(new Vector3(-10f,5.5f,-15f)); //Ente Laterne hinten links
        duckPositions.Add(new Vector3(17f,3.5f,12f)); //Ente Horizontale Bank unten rechts
        duckPositions.Add(new Vector3(0f,2.5f,13f)); //Ente Straße unten
        duckPositions.Add(new Vector3(0f,3f,-35f)); //Ente Straße oben
        duckPositions.Add(new Vector3(21f,6f,-5f)); //Ente Garage rechts mitte
        duckPositions.Add(new Vector3(25f,3.5f,-20f)); //Ente hinter haus oben rechts

        
        
        int duckCount = duckPositions.Count;
        // Loading the object
        for (int i = 0; i < duckCount; i++)
        {
            ducks.Add(new ObjLoaderObject3D("data/objects/duck_smooth.obj"));            
        }
        
        street = new ObjLoaderObject3D("data/objects/szene.obj");
        //Once the Object is loaded, put it in front of the camera
        
        street.Transformation *= Matrix4.CreateTranslation(0, 0, -10);
        int duckPlacementCounter = 1;
        foreach (var duck in ducks)
        {
            if (duckPlacementCounter > duckCount) duckPlacementCounter = duckCount;
            duck.Transformation = Matrix4.CreateTranslation(unreachableposition);
            duckPlacementCounter++;
        }
        placeOneDuck();
        // Loading the texture
        woodTexture = TextureManager.LoadTexture("data/textures/duck_texture.png");
        cellshading = TextureManager.LoadTexture("data/textures/szene-texture2.png");

        int spriteTexture = TextureManager.LoadTexture("data/textures/sprites.png");
        logoSprite = new BitmapGraphic(spriteTexture, 256, 256, 10, 110, 196, 120);
        bitmapFont = new BitmapFont("data/fonts/abel_normal.fnt", "data/fonts/abel_normal.png");    
        
        // enable z-buffer
        GL.Enable(EnableCap.DepthTest);

        // backface culling enabled
        GL.Enable(EnableCap.CullFace);
        GL.CullFace(CullFaceMode.Front);
        
    }

    protected void placeOneDuck()
    {
        Random rnd = new Random();
        int randomDuckNumber = rnd.Next(ducks.Count-1);
        ducks[randomDuckNumber].Transformation = Matrix4.CreateTranslation(duckPositions[randomDuckNumber]);
        ducks[randomDuckNumber].ScaleInPlace(0.1f);
        ducks[randomDuckNumber].RotateObjectX(rnd.Next(360));
        ducks[randomDuckNumber].RotateObjectZ(rnd.Next(360));
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
            duck.Transformation = Matrix4.CreateTranslation(unreachableposition);
            score++;
            cameraSpeed *= 0.9f;
            updateTime = 0;
            placeOneDuck();
        }
        //Console.WriteLine(Camera.Transformation.Inverted().ExtractTranslation());
    }
        
    protected override void OnUpdateFrame(FrameEventArgs e)
    {
            
        // updateCounter simply increases
        updateTime += (float)e.Time;
        timer = starttimer - updateTime* score/5;
        if (timer <= 0)
        {
            resetCamera();
            
            bitmapFont.DrawString("GAME OVER \n " +
                                  "Score: " + score, -Size.X, -Size.Y, 255, 255, 255, 255);
               //TODO Label richtig setzen
        }
        else{
        
        int xbound = 28;
        int zbound = 38;
        //Movement of the camera according to the keys pressed, only when within the boundaries

        if (KeyboardState.IsKeyDown(Keys.W) && Camera.Transformation.ExtractTranslation().Z < zbound-40)
            Camera.Transformation *= Matrix4.CreateTranslation(new Vector3(0,-cameraSpeed,cameraSpeed));
        if (KeyboardState.IsKeyDown(Keys.S) && -Camera.Transformation.ExtractTranslation().Z < zbound)
            Camera.Transformation *= Matrix4.CreateTranslation(new Vector3(0,cameraSpeed,-cameraSpeed));
        if (KeyboardState.IsKeyDown(Keys.A) && Camera.Transformation.ExtractTranslation().X < xbound)
            Camera.Transformation *= Matrix4.CreateTranslation(new Vector3(cameraSpeed,0,0));
        if (KeyboardState.IsKeyDown(Keys.D) && -Camera.Transformation.ExtractTranslation().X < xbound)
            Camera.Transformation *= Matrix4.CreateTranslation(new Vector3(-cameraSpeed,0,0));
        
        if(KeyboardState.IsKeyDown(Keys.Up))
            Camera.Transformation *= Matrix4.CreateTranslation(new Vector3(0,0,-cameraSpeed));
        if(KeyboardState.IsKeyDown(Keys.Down))
            Camera.Transformation *= Matrix4.CreateTranslation(new Vector3(0,0,cameraSpeed));
        
        // Seitwärts gucken (funktioniert so halb)
//        if (KeyboardState.IsKeyDown(Keys.E) && Camera.Transformation.ExtractTranslation().Y < bound)
//            Camera.Transformation *= Matrix4.CreateRotationY(cameraSpeed/100);
//        if (KeyboardState.IsKeyDown(Keys.Q) && Camera.Transformation.ExtractTranslation().Y < bound)
//            Camera.Transformation *= Matrix4.CreateRotationY(-cameraSpeed/100);

        }
        
    }
    
    protected override void OnRenderFrame(FrameEventArgs e)
    {
        MousePosition = new Vector2(Size.X/2, Size.Y/2);
        // specify the clear color
        GL.ClearColor(0.3f, 0.3f, 0.3f, 1.0f);

        // the screen color and the depth-buffer are cleared
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
        AmbientDiffuseSpecularMaterial ambientDiffuseMaterial = new AmbientDiffuseSpecularMaterial();
        ambientDiffuseMaterial.Draw(street, cellshading,0.1f);
        foreach (var duck in ducks)
        {
            ambientDiffuseMaterial.Draw(duck, woodTexture,0.1f);
        }
        GL.Disable(EnableCap.CullFace);
        
        bitmapFont.DrawString("Score: " + score + "| Remaining Time:" + Math.Floor(timer), -Size.X/2, -Size.Y/2, 255, 255, 255, 255);

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