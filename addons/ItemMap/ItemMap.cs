#if TOOLS
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using SoYouWANNAJam2025.Code.Interactive.Stations;
using static Godot.GD;

namespace SoYouWANNAJam2025.addons.ItemMap;

[Tool]
public partial class ItemMap : EditorPlugin
{
	private Control _panelUi;
	private static readonly PackedScene ItemMapControl = Load<PackedScene>("res://addons/ItemMap/_itemMap_UI.tscn");

	private static readonly NodePath WorldMap = "%Entities";
	private static readonly NodePath Entities = "res://Entities";
	private TileMapLayer _worldMap;
	private SubViewport _editor = EditorInterface.Singleton.GetEditorViewport2D();
	
	private EditorUndoRedoManager _undoRedoManager;
	
	public override bool _ForwardCanvasGuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton)
		{
			//GD.Print(@event, "Wololo2");
			
			return true;
		}
		//GD.Print(@event, "Wololo1");
		return false;
	}

	private PackedScene _loadObj;
	private bool _placed = true;
	
	public override void _EnterTree()
	{
		// Initialization of the plugin goes here.
		if (!Engine.IsEditorHint()) return; // This turn itself off if not in editor.
		_panelUi = ItemMapControl.Instantiate() as Control;
		AddControlToDock(EditorPlugin.DockSlot.LeftBl, _panelUi);

		PopulateItemList();
		
		SceneChanged += OnSceneChanged;
		MainScreenChanged += OnMainSceneChanged;
		SceneClosed += OnSceneClosedChanged;
		
		_panelUi.GetNode<ItemList>("ItemList").ItemClicked += OnItemClicked;
		_panelUi.GetNode<Button>("Button").ButtonUp += PopulateItemList;
	}

	private void OnItemClicked(long index, Vector2 position, long id)
	{
		if (Input.IsMouseButtonPressed(MouseButton.Left))
		{
			var entityPath = _panelUi.GetNode<ItemList>("ItemList").GetItemText(index.ToString().ToInt());
			_loadObj = Load<PackedScene>(entityPath);
			GD.Print("Loading Entity: ", entityPath);
			//var pos = _worldMap.MapToLocal(_worldMap.LocalToMap(_worldMap.ToLocal(_editor.GetMousePosition())));
			_placed = false;
		}
	}
	
	private void OnSceneChanged(Node sceneNode) //Find the WorldMap TileMapLayer node;
	{
		if (sceneNode != null)
		{
			//Weird timing error, need to fix. Not fatal.
				var node = EditorInterface.Singleton.GetEditedSceneRoot().GetNodeOrNull<TileMapLayer>(WorldMap);
				if (node != null)
				{
					GD.Print(node, "test");
					_worldMap = node;
				}
		}
		else
		{
			_worldMap = null;
			//Change the interface to be empty with a warning saying the WorldMap doesn't exist.
		}
		
	}

	private void OnMainSceneChanged(string sceneNode)
	{
		GD.Print(sceneNode);
	}

	private void OnSceneClosedChanged(string sceneNode)
	{
		
	}

	private Array GetDirectories(string path)
	{
		var directories = new Array();
		
		//FileAccess
		using var dir = DirAccess.Open(path);
		if (dir != null)
		{
			dir.ListDirBegin();
			string dirPath = dir.GetNext();
			while (true)
			{
				if (dirPath == "") break;

				string fullPath = $"{path}/{dirPath}";
				//GD.Print(fullPath);
				if (dir.CurrentIsDir())
				{
					//GD.Print(dirPath);
					directories.Add(fullPath);
					directories.AddRange(GetDirectories(fullPath));
				}
				else
				{
					
					directories.Add(fullPath); 
				}
				
				dirPath = dir.GetNext();
			}
		}
		dir.ListDirEnd();
		return directories;
	}

	private void PopulateItemList()
	{
		var fileList = GetDirectories(Entities);
		var ui = _panelUi.GetNode<ItemList>("ItemList");
		ui.Clear();
		foreach (var file in fileList)
		{
			if (file.ToString().Contains(".tscn"))
				{
					//GD.Print(file.ToString());
					ui.AddItem(file.ToString());
				}
		}
	}
	
	public override void _Input(InputEvent @event)
	{
		//GD.Print(_ForwardCanvasGuiInput(@event));

		if (!_ForwardCanvasGuiInput(@event)) return;
		
		if(@event is InputEventMouseButton { Pressed: true } mouseEvent)
		{
			// _editor.GetViewport().GuiDisableInput = false;
			// //GD.Print(EditorInterface.Singleton.GetInspector().GetViewportRect().HasPoint(mouseEvent.Position));
			// GD.Print(_editor.GetViewport().GuiDisableInput);
			// // GD.Print(EditorInterface.Singleton.GetEditorMainScreen().GlobalPosition);
			// // GD.Print(mouseEvent.Position);
			//GD.Print(EditorInterface.Singleton.GetEditedSceneRoot().GetViewport().GetVisibleRect().HasPoint(mouseEvent.Position));
			
			if (mouseEvent.ButtonMask != MouseButtonMask.Left) return;
			if (_placed) return;
			var unpackObj = _loadObj.Instantiate<Node2D>();
			
			if (Input.IsKeyPressed((Key.Shift)))
			{
				//if (_editor.IsInputHandled()) return;
				//_undoRedoManager.CreateAction("Item Manager; Placed Obj");

				//var loadObj = Load<PackedScene>("res://Entities/CraftingStations/CraftingBench.tscn");
				//GD.Print(unpackObj);
				if (_worldMap == null) return;
				var pos = _worldMap.MapToLocal(_worldMap.LocalToMap(_worldMap.ToLocal(_editor.GetMousePosition())));
				GD.Print(pos);
				var ogName = unpackObj.Name;
				
				unpackObj.Position = pos;
				_worldMap.AddChild(unpackObj);
				unpackObj.Name = ogName;
				unpackObj.Owner = GetTree().EditedSceneRoot;
				EditorInterface.Singleton.GetSelection().Clear();
				EditorInterface.Singleton.GetSelection().AddNode(unpackObj);
			}
			else
			{
				EditorInterface.Singleton.GetSelection().Clear();
				EditorInterface.Singleton.GetSelection().AddNode(unpackObj);
				
				_placed = true;
				unpackObj.QueueFree();
			}

			//_editor.SetInputAsHandled();
			//_undoRedoManager.AddDoMethod(_worldMap, "AddChild",unpackObj);
			//_undoRedoManager.AddUndoMethod(_worldMap, "RemoveChild", unpackObj);

			//_undoRedoManager.CommitAction();
			//GD.Print(_editor.GetMousePosition());
		}
	}


	public override void _ExitTree()
	{
		// Clean-up of the plugin goes here.
		RemoveControlFromDocks(_panelUi);
		SceneChanged -= OnSceneChanged;
		MainScreenChanged -= OnMainSceneChanged;
		SceneClosed -= OnSceneClosedChanged;
		
		_panelUi.QueueFree();
	}
}
#endif
