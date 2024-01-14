

#region --- Using Directives ---
using OpenTK.Graphics.OpenGL;
using cgimin.engine.object3d;
using cgimin.engine.texture;
using cgimin.engine.material.simpletexture;
using cgimin.engine.camera;
using cgimin.engine.gui;
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

public class OhMyDuckingLordProject : GameWindow
{
    //Global Variables

    // Camera Speed. Gets changed during the gameplay
    private float cameraSpeed = 0.15f;
    
    // the 3D-Object we load
    private List<ObjLoaderObject3D> ducks;
    private ObjLoaderObject3D street = null!;
    
    // our texture-IDs
    private int woodTexture;
    private int cellshading;

    // Materials and Textures
    private SimpleTextureMaterial simpleTextureMaterial;
    
    //List of all duck positions
    private List<Vector3> duckPositions = new List<Vector3>();
    
    //Position where ducks go, that aren't on the map
    Vector3 unreachableposition = new Vector3(99999, 99999, 9999);
    
    //Picking Ray for picking the ducks
    private PickingRay pickingRay;
    
    //Variables for the printing Text to the Screen. Consists of a string and XY coordinates
    private String printString;
    private int printStringX;
    private int printStringY;
    private BitmapFont bitmapFont;
    private BitmapGraphic logoSprite;
    private BitmapFont infomapFont;

    
    // Updating the time
    private float updateTime;
    //Variables used for the random duck placement
    private int lastrandom = 999;
    private int randomDuckNumber;
    //Gameplay Timer 
    private static float timer; 
    private readonly  float starttimer = 20;
    
    private int score = 0;

    
    public OhMyDuckingLordProject(int width, int height, GameWindowSettings gameWindowSettings,
        NativeWindowSettings nativeWindowSettings)
        : base(gameWindowSettings, nativeWindowSettings)
    {
        Size = (width, height);
        KeyDown += KeyboardKeyDown;
        // initialize materials
        simpleTextureMaterial = new SimpleTextureMaterial();
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
        //Press Backspace to reset the Camera
        if (e.Key == Keys.Backspace)
            Camera.Reset();
        if (e.Key == Keys.F12)
        {
            score = 0;
            Camera.Reset();
            printStringX = -Size.X/2;
            printStringY = -Size.Y/2;
            cameraSpeed = 0.15f;
        }
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        updateTime = 0;
        //Lighting
        Light.SetDirectionalLight(new Vector3(0, 100, 100), 
            new Vector4(0.5f, 0.5f, 0.5f, 1), 
            new Vector4(0.5f, 0.5f, 0.5f, 1),
            new Vector4(0f, 0f, 0f, 1));
        // Initialize Camera
        Camera.Init();
        Camera.SetWidthHeightFov(1920, 1080, 60);
        Camera.Reset();
        printStringX = -Size.X/2;
        printStringY = -Size.Y/2;
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
        
        //new and old scene to improve texture (uv mapping)
        street = new ObjLoaderObject3D("data/objects/szene-neu.obj");
        //street = new ObjLoaderObject3D("data/objects/szene.obj");
        
        //Once the Object is loaded, put it in front of the camera
        street.Transformation *= Matrix4.CreateTranslation(0, 0, -10);
        foreach (var duck in ducks)
        {
            duck.Transformation = Matrix4.CreateTranslation(unreachableposition);
        }
        placeOneDuck();
        // Loading the texture
        woodTexture = TextureManager.LoadTexture("data/textures/duck_texture.png");
        
        //try different types of teturisation in gimp ("realistic" and "self-drawn") 
        // !! note: szene-texture.png for szene,obj 
        // !! note: szene-textur-neu-realistisch.png OR szene-textur-neu-gemalt.jpg for szene-neu.obj
        //cellshading = TextureManager.LoadTexture("data/textures/szene-texture.png");
        cellshading = TextureManager.LoadTexture("data/textures/szene-textur-neu-realistisch.png");
        //cellshading = TextureManager.LoadTexture("data/textures/szene-textur-neu-gemalt.jpg");
        
        int spriteTexture = TextureManager.LoadTexture("data/textures/sprites.png");
        logoSprite = new BitmapGraphic(spriteTexture, 256, 256, 10, 110, 196, 120);
        bitmapFont = new BitmapFont("data/fonts/abel_normal.fnt", "data/fonts/abel_normal.png");   
        infomapFont = new BitmapFont("data/fonts/abel_normal.fnt", "data/fonts/abel_normal.png");
        // enable z-buffer
        GL.Enable(EnableCap.DepthTest);
        // backface culling enabled
        GL.Enable(EnableCap.CullFace);
        GL.CullFace(CullFaceMode.Front);
        
    }
    /// <summary>
    /// Places one duck at a random position. These Positions are defined in the duckPositions List.
    /// It is prevented that the same duck is placed twice in a row. It may appear multiple times however.
    /// </summary>
    private void placeOneDuck()
    {
        Random rnd = new Random();
        //Ensuring that the ducks position changes each time
        while (lastrandom == randomDuckNumber)
        {
            randomDuckNumber = rnd.Next(ducks.Count-1);    
        }
        lastrandom = randomDuckNumber;
        ducks[randomDuckNumber].Transformation = Matrix4.CreateTranslation(duckPositions[randomDuckNumber]);
        ducks[randomDuckNumber].ScaleInPlace(0.1f);
        ducks[randomDuckNumber].RotateObjectX(rnd.Next(360));
        ducks[randomDuckNumber].RotateObjectZ(rnd.Next(360));
    }

    /// <summary>
    /// Duck picking algorithm. Works by creating a picking ray from the camera position to the center of the screen, where the crosshair is always located.
    /// The picking Ray gets transformed into the object space of each duck and then gets checked for intersection with any triangle of the ducks model.
    /// On a miss, nothing happens. On a hit, the Camera gets slower, another duck gets spawned and the score increases.
    /// </summary>
    private void PickDuck()
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
        //Use this line to print the camera position. Useful for placing the ducks
        //Console.WriteLine(Camera.Transformation.Inverted().ExtractTranslation());
    }
        
    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        
        // updateCounter simply increases
        updateTime += (float)e.Time;
        timer = starttimer - updateTime* (score)/5;
        //Check if the win or loose condition are met. If so, print the appropriate message and fixate the camera
        if (timer <= 0 || score == ducks.Count)
        {
            if(score == ducks.Count())
            {
                printString = "Winner Winner Duck Dinner!";
            }
            if (timer <= 0)
            {
                printString = "Time's Up! Your Score was: " + score;
            }
            Camera.Reset();
            printStringX = -Size.X/8;
            printStringY = 0;

        }
        else{
            //Bounding areas for the camera. Prevents the camera from leaving the map
            int xBound = 28;
            int zBound = 38;
            
            //Creating a movement vector for the camera
            Vector3 movementVector = new Vector3(0,0,0);
            if (KeyboardState.IsKeyDown(Keys.W) && Camera.Transformation.ExtractTranslation().Z < zBound-40)
                movementVector += new Vector3(0,-1,1);
            if (KeyboardState.IsKeyDown(Keys.S) && -Camera.Transformation.ExtractTranslation().Z < zBound)
                movementVector += new Vector3(0,1,-1);
            if (KeyboardState.IsKeyDown(Keys.A) && Camera.Transformation.ExtractTranslation().X < xBound)
                movementVector += new Vector3(1,0,0);
            if (KeyboardState.IsKeyDown(Keys.D) && -Camera.Transformation.ExtractTranslation().X < xBound)
                movementVector += new Vector3(-1,0,0);
            
            if(movementVector.Length != 0)
                Camera.Transformation *= Matrix4.CreateTranslation(movementVector.Normalized()*cameraSpeed);
            printString = " score: " + score + " | remaining time: " + Math.Floor(timer);
            
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
        bitmapFont.DrawString( printString, printStringX, printStringY, 255, 255, 255, 255);
        infomapFont.DrawString(" move: [W][A][S][D] & shoot: [SPACE]" , 0, printStringY,
            255, 255, 255, 255);
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
        nativeWindowSettings.Title = "Oh My Ducking Lord";
        nativeWindowSettings.WindowState = WindowState.Fullscreen;
        using var example = new OhMyDuckingLordProject(1920, 1080, windowSettings, nativeWindowSettings);
        example.Run();
    }
}