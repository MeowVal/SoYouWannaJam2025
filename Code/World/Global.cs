using Godot;
using SoYouWANNAJam2025.Code.Player;

//using SoYouWANNAJam2025.Code.Camera;

namespace SoYouWANNAJam2025.Code.World;

public partial class Global : Node2D
{
    
    public const int GameScale = 4;
    public static float GameTimer;
    public static int GameDay;
    public static CharacterControl Player;
    public static TileMapLayer Grid;
    public static GameManager GameManager;
    public static Camera Camera;
    public static bool FreezeDay = false;
    [Signal]
    public delegate void CurrentSceneChangedEventHandler(string sceneName);
    
    private static string _currentScene = "MainMenu";

    public string CurrentScene
    {
        get => _currentScene;
        set 
        {
            if (_currentScene != value)
            {
                _currentScene = value;
                EmitSignal(nameof(CurrentSceneChanged), _currentScene);;
            }
        }
    }

    public override void _Ready()
    {
        base._Ready();
       // Grid = GetNode<TileMapLayer>("/root/GameManager/GameWorld/Entities");
       // GameManager = GetNode<GameManager>("/root/GameManager");
       // Camera = GetNode<Camera>("/root/GameManager/Camera");
    }
}