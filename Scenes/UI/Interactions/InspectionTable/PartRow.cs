using System;
using Godot;
using SoYouWANNAJam2025.Code.Interactive.Items;

namespace SoYouWANNAJam2025.Scenes.UI.Interactions.InspectionTable;

public partial class PartRow : HBoxContainer
{
    private Label _nameText;
    private TextureRect _nameIcon;
    private Label _actionTextStart;
    private Label _actionTextHighlight;
    private Label _actionTextEnd;
    private Label _actionTextBelow;
    private TextureRect _actionIcon;
    private TextureRect _partIcon;

    private Texture2D _iconTick = GD.Load<Texture2D>("res://Assets/Sprites/UI/Icon_Tick.png");
    private Texture2D _iconCross = GD.Load<Texture2D>("res://Assets/Sprites/UI/Icon_Cross.png");
    private Texture2D _iconRepair = GD.Load<Texture2D>("res://Assets/Sprites/UI/Icon_Repair.png");
    private Texture2D _iconReplace = GD.Load<Texture2D>("res://Assets/Sprites/UI/Icon_Replace.png");

    public enum EActionType
    {
        None,
        Repair,
        Replace
    }

    public void Initialise(ModularPartTemplate part, EActionType actionType, ModularPartTemplate replacement = null)
    {
        _nameText = GetNode<Label>("VBoxContainer/NameContainer/NameText");
        _nameIcon = GetNode<TextureRect>("VBoxContainer/NameContainer/NameIcon");
        _actionTextStart = GetNode<Label>("VBoxContainer/ActionContainer/ActionVB/ActionHB/ActionTextStart");
        _actionTextHighlight = GetNode<Label>("VBoxContainer/ActionContainer/ActionVB/ActionHB/ActionTextHighlight");
        _actionTextEnd = GetNode<Label>("VBoxContainer/ActionContainer/ActionVB/ActionHB/ActionTextEnd");
        _actionTextBelow = GetNode<Label>("VBoxContainer/ActionContainer/ActionVB/ActionTextBelow");
        _actionIcon = GetNode<TextureRect>("VBoxContainer/ActionContainer/ActionIcon");
        _partIcon = GetNode<TextureRect>("PartIcon");


        GD.Print(part);
        GD.Print(part.DisplayName);
        var partImage = part.GetItemImage();
        var partTexture = ImageTexture.CreateFromImage(partImage);
        _partIcon.Texture = partTexture;
        _nameText.Text = part.DisplayName;

        switch (actionType)
        {
            case EActionType.Repair:
                _nameIcon.Texture = _iconCross;
                _actionTextStart.Text = "Part needs to be ";
                _actionTextHighlight.Text = "repaired";
                _actionTextHighlight.Modulate = Colors.Purple;
                _actionTextEnd.Text = ".";
                _actionTextBelow.QueueFree();
                _actionIcon.Texture = _iconRepair;
                break;
            case EActionType.Replace:
                _nameIcon.Texture = _iconCross;
                _actionTextStart.Text = "Part needs to be ";
                _actionTextHighlight.Text = "replaced ";
                _actionTextHighlight.Modulate = Colors.Yellow;
                _actionTextEnd.Text = "with...";
                _actionTextBelow.Text = replacement!.DisplayName;
                _actionIcon.Texture = _iconReplace;
                break;
            case EActionType.None:
                _nameIcon.Texture = _iconTick;
                _actionTextStart.Text = "No action needed.";
                _actionTextStart.Modulate = Colors.DimGray;
                _actionTextHighlight.QueueFree();
                _actionTextEnd.QueueFree();
                _actionTextBelow.QueueFree();
                _actionIcon.Texture = null;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null);
        }
    }
}