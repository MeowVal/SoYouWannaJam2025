#if TOOLS
using Godot;
using static Godot.GD;

namespace SoYouWANNAJam2025.addons.ItemMap;

[Tool]
public partial class ItemMap : EditorPlugin
{
	private Control _panelUi;
	private static readonly PackedScene ItemMapControl = Load<PackedScene>("res://addons/ItemMap/_itemMap_UI.tscn");

	public override void _EnterTree()
	{
		// Initialization of the plugin goes here.
		AddCustomType();
		_panelUi = ItemMapControl.Instantiate() as Control;
		AddControlToDock(EditorPlugin.DockSlot.LeftBl, _panelUi);
	}

	public override void _ExitTree()
	{
		// Clean-up of the plugin goes here.
		RemoveControlFromDocks(_panelUi);
		_panelUi.QueueFree();
	}
}
#endif
