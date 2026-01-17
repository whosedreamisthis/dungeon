using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DungeonLevel_", menuName = "Scriptable Objects/Dungeon/Dungeon Level")]
public class DungeonLevelSO : ScriptableObject
{
    #region  Header BASIC LEVEL DETAILS
    [Space(10)]
    [Header("BASIC LEVEL DETAILS")]
    #endregion Header BASIC LEVEL DETAILS

    #region  Tooltip
    [Tooltip("The name for the level")]
    #endregion Tooltip

    public string levelName;

    #region  Header ROOM TEMPLATES FOR LEVEL
    [Space(10)]
    [Header("ROOM TEMPLATES FOR LEVEL")]
    #endregion Header ROOM TEMPLATES FOR LEVEL
    #region  Tooltip
    [Tooltip(
        "Populate the list with the room templates that you want to be part for the level. You need to ensure that room templates are included for all room node types that are specified in the room node graph for the level."
    )]
    #endregion Tooltip
    public List<RoomTemplateSO> roomTemplateList;

    #region  Header ROOM NODE GRAPHS FOR LEVEL
    [Space(10)]
    [Header("ROOM NODE GRAPHS FOR LEVEL")]
    #endregion Header ROOM NODE GRAPHS FOR LEVEL
    #region  Tooltip
    [Tooltip(
        "Populate the list with the room node graphs which should be randomly selected from the level."
    )]
    #endregion Tooltip
    public List<RoomNodeGraphSO> roomNodeGraphList;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(levelName), levelName);
        if (
            HelperUtilities.ValidateCheckEnumerableValues(
                this,
                nameof(roomTemplateList),
                roomTemplateList
            )
        )
            return;

        if (
            HelperUtilities.ValidateCheckEnumerableValues(
                this,
                nameof(roomNodeGraphList),
                roomNodeGraphList
            )
        )
            return;

        bool isEWCorridor = false;
        bool isNSCorridor = false;
        bool isEntrance = false;

        foreach (RoomTemplateSO roomTemplateSO in roomTemplateList)
        {
            if (roomTemplateSO == null)
                return;

            if (roomTemplateSO.roomNodeType.isCorridorEW)
                isEWCorridor = true;

            if (roomTemplateSO.roomNodeType.isCorridorNS)
                isNSCorridor = true;

            if (roomTemplateSO.roomNodeType.isEntrance)
                isEntrance = true;
        }

        if (!isEntrance)
            Debug.Log("In " + this.name.ToString() + " : No Entranec Room type specified");

        if (!isNSCorridor)
            Debug.Log("In " + this.name.ToString() + " : No NS Corridor Room type specified");

        if (!isEWCorridor)
            Debug.Log("In " + this.name.ToString() + " : No EW Corridor Room type specified");

        foreach (RoomNodeGraphSO roomNodeGraph in roomNodeGraphList)
        {
            if (roomNodeGraph == null)
                return;

            foreach (RoomNodeSO roomNode in roomNodeGraph.roomNodeList)
            {
                if (roomNode == null)
                    continue;

                if (
                    roomNode.roomNodeType.isEntrance
                    || roomNode.roomNodeType.isCorridorEW
                    || roomNode.roomNodeType.isCorridorNS
                    || roomNode.roomNodeType.isCorridor
                    || roomNode.roomNodeType.isNone
                )
                    continue;

                bool isRoomNodeTypeFound = false;

                foreach (RoomTemplateSO roomTemplate in roomTemplateList)
                {
                    if (roomTemplate == null)
                        continue;

                    if (roomTemplate.roomNodeType == roomNode.roomNodeType)
                    {
                        isRoomNodeTypeFound = true;
                        break;
                    }
                }

                if (!isRoomNodeTypeFound)
                    Debug.Log(
                        "In "
                            + this.name.ToString()
                            + " : No room template "
                            + roomNode.roomNodeType.name.ToString()
                            + " found for node graph "
                            + roomNodeGraph.name.ToString()
                    );
            }
        }
    }
#endif
    #endregion Validation
}
