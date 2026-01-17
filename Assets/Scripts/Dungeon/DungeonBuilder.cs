using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
public class DungeonBuilder : SingletonMonoBehaviour<DungeonBuilder>
{
    public Dictionary<string, Room> dungeonBuilderRoomDictionary = new Dictionary<string, Room>();
    private Dictionary<string, RoomTemplateSO> roomTemplateDictionary =
        new Dictionary<string, RoomTemplateSO>();
    private List<RoomTemplateSO> roomTemplateList = null;
    private RoomNodeTypeListSO roomNodeTypeList;
    private bool dungeonBuildSuccessful;

    protected override void Awake()
    {
        base.Awake();

        LoadRoomNodeTypeList();

        GameResources.Instance.dimmedMaterial.SetFloat("Alpha_Slider", 1f);
    }

    private void LoadRoomNodeTypeList()
    {
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    public bool GenerateDungeon(DungeonLevelSO currentDungeonLevel)
    {
        roomTemplateList = currentDungeonLevel.roomTemplateList;

        LoadRoomTemplatesIntoDictionary();

        dungeonBuildSuccessful = false;
        int dungeonBuildAttempts = 0;

        while (!dungeonBuildSuccessful && dungeonBuildAttempts < Settings.maxDungeonBuildAttempts)
        {
            dungeonBuildAttempts++;
            RoomNodeGraphSO roomNodeGraph = SelectRandomRoomNodeGraph(
                currentDungeonLevel.roomNodeGraphList
            );

            int dungeonRebuildAttemptsForRoomGraph = 0;
            dungeonBuildSuccessful = false;

            while (
                !dungeonBuildSuccessful
                && dungeonRebuildAttemptsForRoomGraph
                    < Settings.maxDungeonRebuildAttemptsForRoomGraph
            )
            {
                ClearDungeon();

                dungeonRebuildAttemptsForRoomGraph++;

                dungeonBuildSuccessful = AttemptToBuildRandomDungeon(roomNodeGraph);
            }
            if (dungeonBuildSuccessful)
            {
                InstantiateRoomGameObjects();
            }
        }
        return dungeonBuildSuccessful;
    }

    private void InstantiateRoomGameObjects() { }

    private bool AttemptToBuildRandomDungeon(RoomNodeGraphSO roomNodeGraph)
    {
        Queue<RoomNodeSO> openRoomNodeQueue = new Queue<RoomNodeSO>();

        RoomNodeSO entranceNode = roomNodeGraph.GetRoomNode(
            roomNodeTypeList.list.Find(x => x.isEntrance)
        );

        if (entranceNode != null)
        {
            openRoomNodeQueue.Enqueue(entranceNode);
        }
        else
        {
            Debug.Log("No entrance node");
            return false;
        }

        bool noRoomOverlaps = true;
        noRoomOverlaps = ProcessRoomInOpenRoomNodeQueue(
            roomNodeGraph,
            openRoomNodeQueue,
            noRoomOverlaps
        );

        if (openRoomNodeQueue.Count == 0 && noRoomOverlaps)
        {
            return true;
        }
        return false;
    }

    private bool ProcessRoomInOpenRoomNodeQueue(
        RoomNodeGraphSO roomNodeGraph,
        Queue<RoomNodeSO> openRoomNodeQueue,
        bool noRoomOverlaps
    )
    {
        while (openRoomNodeQueue.Count > 0 && noRoomOverlaps == true)
        {
            RoomNodeSO roomNode = openRoomNodeQueue.Dequeue();

            foreach (RoomNodeSO childRoomNode in roomNodeGraph.GetChildRoomNodes(roomNode))
            {
                openRoomNodeQueue.Enqueue(childRoomNode);
            }

            if (roomNode.roomNodeType.isEntrance)
            {
                RoomTemplateSO roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);

                Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);
                room.isPositioned = true;
                dungeonBuilderRoomDictionary.Add(room.id, room);
            }
            else
            {
                Room parentRoom = dungeonBuilderRoomDictionary[roomNode.parentRoomNodeIDList[0]];

                noRoomOverlaps = CanPlaceRoomWithNoOverlaps(roomNode, parentRoom);
            }
        }
        return noRoomOverlaps;
    }

    private bool CanPlaceRoomWithNoOverlaps(RoomNodeSO roomNode, Room parentRoom)
    {
        bool roomOverlaps = true;

        while (roomOverlaps)
        {
            List<Doorway> unconnectedAvailableParentDoorways = GetUnconnectedAvailableDoorways(
                    parentRoom.doorwayList
                )
                .ToList();

            if (unconnectedAvailableParentDoorways.Count == 0)
            {
                return false;
            }

            Doorway doorwayParent = unconnectedAvailableParentDoorways[
                UnityEngine.Random.Range(0, unconnectedAvailableParentDoorways.Count)
            ];

            RoomTemplateSO roomTemplate = GetRandomTemplateForRoomConsistentWithParent(
                roomNode,
                doorwayParent
            );

            Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

            if (PlaceTheRoom(parentRoom, doorwayParent, room))
            {
                roomOverlaps = false;
                room.isPositioned = true;
                dungeonBuilderRoomDictionary.Add(room.id, room);
            }
            else
            {
                roomOverlaps = true;
            }
        }
        return true;
    }

    private bool PlaceTheRoom(Room parentRoom, Doorway doorwayParent, Room room)
    {
        Doorway doorway = GetOppositeDoorway(doorwayParent, room.doorwayList);

        if (doorway == null)
        {
            doorwayParent.isUnavailable = true;
            return false;
        }

        Vector2Int parentDoorwayPosition =
            parentRoom.lowerBounds + doorwayParent.position - parentRoom.templateLowerBounds;

        Vector2Int adjustment = Vector2Int.zero;

        switch (doorway.orientation)
        {
            case Orientation.north:
                adjustment = new Vector2Int(0, -1);
                break;
            case Orientation.south:
                adjustment = new Vector2Int(0, 1);
                break;
            case Orientation.east:
                adjustment = new Vector2Int(-1, 0);
                break;
            case Orientation.west:
                adjustment = new Vector2Int(1, 0);
                break;
            default:
                break;
        }

        room.lowerBounds =
            parentDoorwayPosition + adjustment + room.templateLowerBounds - doorway.position;
        room.upperBounds = room.lowerBounds + room.templateUpperBounds - room.templateLowerBounds;

        Room overlappingRoom = CheckForRoomOverlap(room);

        if (overlappingRoom == null)
        {
            doorwayParent.isConnected = true;
            doorwayParent.isUnavailable = true;

            doorway.isConnected = true;
            doorway.isUnavailable = true;
            return true;
        }
        doorwayParent.isUnavailable = true;
        return false;
    }

    public RoomTemplateSO GetRoomTemplate(string roomTemplateID)
    {
        if (roomTemplateDictionary.TryGetValue(roomTemplateID, out RoomTemplateSO roomTemplate))
        {
            return roomTemplate;
        }
        return null;
    }

    public Room GetRoomByRoomID(string roomID)
    {
        if (dungeonBuilderRoomDictionary.TryGetValue(roomID, out Room room))
            return room;
        return null;
    }

    private Room CheckForRoomOverlap(Room roomToTest)
    {
        foreach (KeyValuePair<string, Room> kv in dungeonBuilderRoomDictionary)
        {
            Room room = kv.Value;

            if (room.id == roomToTest.id || !room.isPositioned)
                continue;

            if (IsOverlappingRoom(roomToTest, room))
            {
                return room;
            }
        }
        return null;
    }

    private bool IsOverlappingRoom(Room room1, Room room2)
    {
        bool isOverlappingX = IsOverlappingInterval(
            room1.lowerBounds.x,
            room1.upperBounds.x,
            room2.lowerBounds.x,
            room2.upperBounds.x
        );
        bool isOverlappingY = IsOverlappingInterval(
            room1.lowerBounds.y,
            room1.upperBounds.y,
            room2.lowerBounds.y,
            room2.upperBounds.y
        );

        return isOverlappingX && isOverlappingY;
    }

    private bool IsOverlappingInterval(int min1, int max1, int min2, int max2)
    {
        if (Mathf.Max(min1, min2) <= Mathf.Min(max1, max2))
        {
            return true;
        }
        return false;
    }

    private Doorway GetOppositeDoorway(Doorway parentDoorway, List<Doorway> doorwayList)
    {
        foreach (Doorway doorway in doorwayList)
        {
            if (
                parentDoorway.orientation == Orientation.east
                && doorway.orientation == Orientation.west
            )
            {
                return doorway;
            }
            else if (
                parentDoorway.orientation == Orientation.west
                && doorway.orientation == Orientation.east
            )
            {
                return doorway;
            }
            else if (
                parentDoorway.orientation == Orientation.north
                && doorway.orientation == Orientation.south
            )
            {
                return doorway;
            }
            else if (
                parentDoorway.orientation == Orientation.south
                && doorway.orientation == Orientation.north
            )
            {
                return doorway;
            }
        }
        return null;
    }

    private RoomTemplateSO GetRandomTemplateForRoomConsistentWithParent(
        RoomNodeSO roomNode,
        Doorway doorwayParent
    )
    {
        RoomTemplateSO roomTemplate = null;

        if (roomNode.roomNodeType.isCorridor)
        {
            switch (doorwayParent.orientation)
            {
                case Orientation.north:
                case Orientation.south:
                    roomTemplate = GetRandomRoomTemplate(
                        roomNodeTypeList.list.Find(x => x.isCorridorNS)
                    );
                    break;
                case Orientation.east:
                case Orientation.west:
                    roomTemplate = GetRandomRoomTemplate(
                        roomNodeTypeList.list.Find(x => x.isCorridorEW)
                    );
                    break;
                case Orientation.none:
                    break;
                default:
                    break;
            }
        }
        else
        {
            roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);
        }
        return roomTemplate;
    }

    private IEnumerable<Doorway> GetUnconnectedAvailableDoorways(List<Doorway> roomDoorwayList)
    {
        foreach (Doorway doorway in roomDoorwayList)
        {
            if (!doorway.isConnected && !doorway.isUnavailable)
            {
                yield return doorway;
            }
        }
    }

    private Room CreateRoomFromRoomTemplate(RoomTemplateSO roomTemplate, RoomNodeSO roomNode)
    {
        Room room = new Room();

        room.templateID = roomTemplate.guid;
        room.id = roomNode.id;
        room.prefab = roomTemplate.prefab;
        room.roomNodeType = roomTemplate.roomNodeType;
        room.lowerBounds = roomTemplate.lowerBounds;
        room.upperBounds = roomTemplate.upperBounds;
        room.spawnPositionArray = roomTemplate.spawnPositionArray;
        room.templateLowerBounds = roomTemplate.lowerBounds;
        room.templateUpperBounds = roomTemplate.upperBounds;

        room.childRoomIDList = CopyStringList(roomNode.childRoomNodeIDList);
        room.doorwayList = CopyDoorwayList(roomTemplate.doorwayList);

        if (roomNode.parentRoomNodeIDList.Count == 0)
        {
            room.parentRoomID = "";
            room.isPreviouslyVisited = true;
        }
        else
        {
            room.parentRoomID = roomNode.parentRoomNodeIDList[0];
        }
        return room;
    }

    private List<Doorway> CopyDoorwayList(List<Doorway> oldDoorwayList)
    {
        List<Doorway> newDoorwayList = new List<Doorway>();

        foreach (Doorway stringValue in oldDoorwayList)
        {
            newDoorwayList.Add(stringValue);
        }
        return newDoorwayList;
    }

    private List<string> CopyStringList(List<string> oldStringList)
    {
        List<string> newStringList = new List<string>();

        foreach (string stringValue in oldStringList)
        {
            newStringList.Add(stringValue);
        }
        return newStringList;
    }

    private RoomTemplateSO GetRandomRoomTemplate(RoomNodeTypeSO roomNodeType)
    {
        List<RoomTemplateSO> matchingRoomTemplateList = new List<RoomTemplateSO>();

        foreach (RoomTemplateSO roomTemplate in roomTemplateList)
        {
            if (roomTemplate.roomNodeType == roomNodeType)
            {
                matchingRoomTemplateList.Add(roomTemplate);
            }
        }

        if (matchingRoomTemplateList.Count == 0)
            return null;

        return matchingRoomTemplateList[
            UnityEngine.Random.Range(0, matchingRoomTemplateList.Count)
        ];
    }

    private void ClearDungeon()
    {
        if (dungeonBuilderRoomDictionary.Count > 0)
        {
            foreach (KeyValuePair<string, Room> kv in dungeonBuilderRoomDictionary)
            {
                Room room = kv.Value;

                if (room.istantiatedRoom != null)
                {
                    Destroy(room.istantiatedRoom.gameObject);
                }
            }
            dungeonBuilderRoomDictionary.Clear();
        }
    }

    private RoomNodeGraphSO SelectRandomRoomNodeGraph(List<RoomNodeGraphSO> roomNodeGraphList)
    {
        if (roomNodeGraphList.Count > 0)
        {
            return roomNodeGraphList[UnityEngine.Random.Range(0, roomNodeGraphList.Count)];
        }
        else
        {
            Debug.Log("No room node graphs in list");
            return null;
        }
    }

    private void LoadRoomTemplatesIntoDictionary()
    {
        roomTemplateDictionary.Clear();

        foreach (RoomTemplateSO roomTemplate in roomTemplateList)
        {
            if (!roomTemplateDictionary.ContainsKey(roomTemplate.guid))
            {
                roomTemplateDictionary.Add(roomTemplate.guid, roomTemplate);
            }
            else
            {
                Debug.Log("Duplicate Room Template Key in " + roomNodeTypeList);
            }
        }
    }
}
