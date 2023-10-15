using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
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
        DrawDebugGizmo();
    }

    public void SetHexNodes() 
    {
        SetForestHexNodes();
    }

    private void SetForestHexNodes() 
    {
        GroupNodes();
        MergeNodes();
        ReSortTileDict();
    }

    Dictionary<int, Dictionary<int, NodeGroup>> tileNodeDict = new Dictionary<int, Dictionary<int, NodeGroup>>();

    private void GroupNodes() 
    {
        List<HexTile> searchedHexTiles = new List<HexTile>();

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

                //Check center
                if (!center.HasGroup)
                {
                    NodeGroup group = new NodeGroup(tileIndex, groupIndex);
                    center.group = group;
                    group.AddNode(center);

                    groupDict.Add(groupIndex, group);
                    groupIndex++;
                }

                //Check right
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

                //Check left
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

    private void MergeNodes() 
    {
        foreach (HexTile tile in HexGridManager.Instance.HextTileDict.Values) 
        {
            foreach (LandscapeModel landscape in tile.Lanscapes)
            {
                if (!landscape.HasGroup)
                    continue;

                LandscapeModel neighbour = HexGridManager.Instance.GetNeighbourHexLandscape(tile.hex, landscape.direction);

                if (neighbour == null || !neighbour.HasGroup || neighbour.landscapeType != landscape.landscapeType)
                    continue;

                List<LandscapeModel> neighbourNodes = neighbour.group.nodes;

                //Dictionary<int, NodeGroup> neighbourGroupDict = null;
                //tileNodeDict.TryGetValue(neighbour.group.tileId, out neighbourGroupDict);
                //neighbourGroupDict.Remove(neighbour.group.groupId);

                foreach (LandscapeModel node in neighbourNodes)
                    node.group = landscape.group;

                neighbour.group = landscape.group;

                landscape.group.AddNode(neighbourNodes);
            }//foreach (LandscapeModel landscape in tile.Lanscapes)

        }//foreach (HexTile tile in HexGridManager.Instance.HextTileDict.Values)
    }

    private void ReSortTileDict() 
    {
        tileNodeDict.Clear();

        foreach (HexTile tile in HexGridManager.Instance.HextTileDict.Values) 
        {
            foreach (LandscapeModel landscape in tile.Lanscapes) 
            {
                if (!landscape.HasGroup)
                    continue;

                int tileId = landscape.group.tileId;
                int groupId = landscape.group.groupId;

                if (tileNodeDict.ContainsKey(tileId))
                {
                    Dictionary<int, NodeGroup> nodeGroupDict = null;
                    tileNodeDict.TryGetValue(tileId, out nodeGroupDict);

                    if (nodeGroupDict.ContainsKey(groupId))
                    {
                        continue;
                    }
                    else
                    {
                        nodeGroupDict.Add(groupId, landscape.group);

                    }//if (nodeGroupDict.ContainsKey(groupId))
                }
                else
                {
                    Dictionary<int, NodeGroup> nodeGroupDict = new Dictionary<int, NodeGroup>();
                    nodeGroupDict.Add(groupId, landscape.group);
                    tileNodeDict.Add(tileId, nodeGroupDict);

                }//if (tileNodeDict.ContainsKey(tileId))

            }//foreach (LandscapeModel landscape in tile.Lanscapes) 
        }//foreach (HexTile tile in HexGridManager.Instance.HextTileDict.Values)
    }

    private void DrawDebugGizmo() 
    {
        if (tileNodeDict == null || tileNodeDict.Count < 1)
            return;

        GUIStyle style = EditorStyles.boldLabel;
        style.fontSize = 18;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.blue;

        foreach (var groupDict in tileNodeDict.Values)
        {
            foreach (var nodeGroup in groupDict.Values)
            {
                /*
                Vector3 pos = nodeGroup.nodes[0].position;
                pos.y += 0.25f;

                string text = $"{nodeGroup.tileId}-{nodeGroup.groupId}";

                Handles.Label(pos, text, style);
                */

                foreach (var node in nodeGroup.nodes)
                {
                    Vector3 pos = node.position;
                    pos.y += 0.25f;

                    string text = $"{node.group.tileId}-{node.group.groupId}";

                    Handles.Label(pos, text, style);
                }
            }
        }
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

    public bool IsSame(NodeGroup group) 
    {
        return tileId == group.tileId && groupId == group.groupId;
    }
}
