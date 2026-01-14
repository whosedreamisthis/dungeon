using UnityEditor;
using UnityEngine;

public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle roomNodeStyle;
    private const float nodeWidth = 160;
    private const float nodeHeight = 75;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;

    [MenuItem("Room Node Graph Editor", menuItem = "Window/Dungeon Editor/ Room Node Graph Editor")]
    private static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Editor Graph");
    }

    private void OnEnable()
    {
        roomNodeStyle = new GUIStyle();
        roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        roomNodeStyle.normal.textColor = Color.white;
        roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(
            new Rect(new Vector2(100, 100), new Vector2(nodeWidth, nodeHeight)),
            roomNodeStyle
        );
        EditorGUILayout.LabelField("Node 1");
        GUILayout.EndArea();
        GUILayout.BeginArea(
            new Rect(new Vector2(300, 300), new Vector2(nodeWidth, nodeHeight)),
            roomNodeStyle
        );
        EditorGUILayout.LabelField("Node 2");
        GUILayout.EndArea();
    }
}
