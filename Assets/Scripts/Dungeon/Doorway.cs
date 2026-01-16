using UnityEngine;

[System.Serializable]
public class Doorway
{
    public Vector2Int position;
    public Orientation orientation;
    public GameObject doorPrefab;

    #region
    [Header("The Upper Left Position To Start Copying From")]
    #endregion
    public Vector2Int doorwayStartCopyPosition;

    #region
    [Header("The width of tiles in the doorway to copy over")]
    #endregion
    public Vector2Int doorwayCopyTileWidth;

    #region
    [Header("The height of tiles in the doorway to copy over")]
    #endregion
    public Vector2Int doorwayCopyTileHeight;

    [HideInInspector]
    public bool isConnected = false;

    [HideInInspector]
    public bool isUnavailable = false;
}
