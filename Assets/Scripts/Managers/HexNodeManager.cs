using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HexNodeManager : MonoBehaviour
{
    public static HexNodeManager Instance { get; private set; } = null;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void OnDrawGizmos()
    {
        if (tileNodeDict == null || tileNodeDict.Count < 1)
            return;

        foreach (Dictionary<int, NodeGroup> nodeDict in tileNodeDict.Values)
        {
            if (nodeDict == null || nodeDict.Count < 1)
                continue;

            foreach (NodeGroup group in nodeDict.Values)
            {
                if (group.nodes == null || group.nodes.Count < 1)
                    continue;

                foreach (LandscapeModel node in group.nodes)
                {
                    Vector3 pos = node.position;
                    pos.y += 0.25f;

                    //Gizmos.DrawSphere(pos, 0.1f);

                    pos.y += 0.25f;
                    string text = $"TID:{group.tileId} GID:{group.groupId}";

                    Handles.color = Color.blue;
                    GUIStyle style = EditorStyles.boldLabel;
                    style.fontSize = 18;
                    style.fontStyle = FontStyle.Bold;

                    Handles.Label(pos, text, style);
                }
            }
        }
    }

    public void SetHexNodes() 
    {
        SetForestHexNodes();
    }

    private void SetForestHexNodes() 
    {
        GroupNodes();
    }

    List<HexTile> searchedHexTiles = new List<HexTile>();
    Dictionary<int, Dictionary<int, NodeGroup>> tileNodeDict = new Dictionary<int, Dictionary<int, NodeGroup>>();

    private void GroupNodes() 
    {
        int tileIndex = 0;
        int groupIndex = 0;

        foreach (HexTile tile in HexGridManager.Instance.HextTileDict.Values)
        {
            if (searchedHexTiles.Contains(tile))
                continue;

            groupIndex = 0;
            Dictionary<int, NodeGroup> groupDict = new Dictionary<int, NodeGroup>();

            for (int i = 0; i < tile.Lanscapes.Length; i++)
            {
                int rightIndex = i == tile.Lanscapes.Length - 1 ? 0 : i + 1;
                int leftIndex = i == 0 ? tile.Lanscapes.Length - 1 : i - 1;

                LandscapeModel center = tile.Lanscapes[i];

                if (center.landscapeType != LandscapeTypes.Forest)
                    continue;

                LandscapeModel right = tile.Lanscapes[rightIndex];
                LandscapeModel left = tile.Lanscapes[leftIndex];

                if (!center.HasGroup)
                {
                    NodeGroup group = new NodeGroup(tileIndex, groupIndex);
                    center.group = group;
                    group.AddNode(center);

                    groupDict.Add(groupIndex, group);
                    groupIndex++;
                }
                else
                {

                }//if (!center.HasGroup)

                if (right.landscapeType == LandscapeTypes.Forest)
                {
                    if (right.HasGroup)
                    {
                        List<LandscapeModel> rightNodes = right.group.nodes;

                        right.group = center.group;
                        right.group.AddNode(rightNodes);
                    }
                    else
                    {
                        right.group = center.group;
                        center.group.AddNode(right);

                    }//if (right.HasGroup)


                }//if (right.landscapeType == LandscapeTypes.Forest)

                if (left.landscapeType == LandscapeTypes.Forest)
                {
                    if (left.HasGroup)
                    {
                        List<LandscapeModel> leftNodes = left.group.nodes;

                        left.group = center.group;
                        left.group.AddNode(leftNodes);
                    }
                    else
                    {
                        left.group = center.group;
                        center.group.AddNode(left);

                    }//if (left.HasGroup)


                }//if (left.landscapeType == LandscapeTypes.Forest)


            }//for (int i = 0; i < tile.Lanscapes.Length; i++)

            searchedHexTiles.Add(tile);

            tileNodeDict.Add(tileIndex, groupDict);

            tileIndex++;

        }//foreach (HexTile tile in HexGridManager.Instance.HextTileDict.Values)
    }
}

public class NodeGroup 
{
    public int tileId;
    public int groupId;
    public List<LandscapeModel> nodes;

    public NodeGroup(int tileId, int groupId)
    {
        this.tileId = tileId;
        this.groupId = groupId;
        nodes = new List<LandscapeModel>();
    }

    public NodeGroup(int tileId, int groupId, List<LandscapeModel> nodes)
    {
        this.tileId = tileId;
        this.groupId = groupId;
        this.nodes = new List<LandscapeModel>();
        this.nodes.AddRange(nodes);
    }

    public void AddNode(LandscapeModel node) 
    {
        if (nodes.Contains(node))
            return;

        nodes.Add(node);
    }

    public void AddNode(List<LandscapeModel> newNodes) 
    {
        if (newNodes == null || newNodes.Count < 1)
            return;

        foreach (LandscapeModel node in newNodes)
        {
            AddNode(node);
        }
    }
}
