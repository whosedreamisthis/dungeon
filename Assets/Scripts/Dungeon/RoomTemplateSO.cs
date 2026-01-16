using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Room_", menuName = "Scriptable Objects/Dungeon/Room")]
public class RoomTemplateSO : ScriptableObject
{
    [HideInInspector]
    public string guid;

    #region Header ROOM PREFAB

    [Space(10)]
    [Header("ROOM PREFAB")]
    #endregion Header ROOM PREFAB

    #region ToolTip
    [Tooltip(
        "The gameobject prefab for the room (this will contain all the tilemaps for the room and environment game objects)"
    )]
    #endregion ToolTip

    public GameObject prefab;

    [HideInInspector]
    public GameObject previousPrefab;

    #region Header ROOM CONFIGURATION

    [Space(10)]
    [Header("ROOM CONFIGURATION")]
    #endregion Header ROOM CONFIGURATION

    #region ToolTip
    [Tooltip(
        "The room node type SO. The room node types correspond to the room nodes used in the room node graph. The exceptions being with corridors. Un the room node graph there is just one corridor type 'Corridor'. For the room templates there are 2 corridor node types: CorridorNS and CorridorEW"
    )]
    #endregion ToolTip
    public RoomNodeTypeSO roomNodeType;

    #region ToolTip
    [Tooltip(
        "If you imagine a rectange around the room tilemap that just completely encloses it, the room lower bounds represent the bottom left corner of hte rectange. this should be determined from the tilemap for the room (using the coordinate bursh pointer to get the tilemap grid position)"
    )]
    #endregion ToolTip

    public Vector2Int lowerBounds;

    #region ToolTip
    [Tooltip(
        "If you imagine a rectange around the room tilemap that just completely encloses it, the room upper bounds represent the top right corner of the rectange. this should be determined from the tilemap for the room (using the coordinate bursh pointer to get the tilemap grid position)"
    )]
    #endregion ToolTip

    public Vector2Int upperBounds;

    #region ToolTip
    [Tooltip(
        "There should be a maximum of four doorways for a room - one for each compass direction. These should havea consistent 3 tile opening"
    )]
    #endregion ToolTip

    [SerializeField]
    public List<Doorway> doorwayList;

    #region ToolTip
    [Tooltip(
        "Each possible spawn position (used for enemies and chests) for the room in tilemap coordinate should be added to this array"
    )]
    #endregion ToolTip

    public Vector2Int[] spawnPositionArray;

    public List<Doorway> GetDoorwayList()
    {
        return doorwayList;
    }

    #region Validation
#if UNITY_EDITOR

    private void OnValidate()
    {
        if (guid == "" || previousPrefab != prefab)
        {
            guid = GUID.Generate().ToString();
            previousPrefab = prefab;
            EditorUtility.SetDirty(this);
        }
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(doorwayList), doorwayList);
        HelperUtilities.ValidateCheckEnumerableValues(
            this,
            nameof(spawnPositionArray),
            spawnPositionArray
        );
    }
#endif
    #endregion Validation
}
