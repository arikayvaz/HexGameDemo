using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        //DrawMergeGizmo();
        //DrawResourceNodeGizmo();
    }

    public void SetHexNodes() 
    {
        GroupNodes();
        MergeNodes();
        ReSortTileDict();
        SetResourceNodes();
    }

    Dictionary<int, TileNodeGroup> tileNodeDict = new Dictionary<int, TileNodeGroup>();

    private void GroupNodes() 
    {
        List<HexTile> searchedHexTiles = new List<HexTile>();

        int tileIndex = 0;

        foreach (HexTile tile in HexGridManager.Instance.PlacedHexTileDict.Values)
        {
            if (searchedHexTiles.Contains(tile))
                continue;

            TileNodeGroup tileNodeGroup = new TileNodeGroup();

            for (int i = 0; i < tile.Lanscapes.Length; i++)
            {
                int rightIndex = i == tile.Lanscapes.Length - 1 ? 0 : i + 1;
                int leftIndex = i == 0 ? tile.Lanscapes.Length - 1 : i - 1;

                LandscapeModel center = tile.Lanscapes[i];

                if (center.landscapeType == LandscapeTypes.Empty || center.landscapeType == LandscapeTypes.None)
                    continue;

                LandscapeModel right = tile.Lanscapes[rightIndex];
                LandscapeModel left = tile.Lanscapes[leftIndex];

                //Check center
                if (!center.HasGroup)
                {
                    NodeGroup group = new NodeGroup(tileIndex, tileNodeGroup.GroupIndex(center.landscapeType));
                    center.group = group;
                    group.AddNode(center);

                    tileNodeGroup.AddNodeGroup(center.landscapeType, group);
                }

                //Check right
                if (right.landscapeType == center.landscapeType)
                    GroupSideLandscape(center, right);

                //Check left
                if (left.landscapeType == center.landscapeType)
                    GroupSideLandscape(center, left);

            }//for (int i = 0; i < tile.Lanscapes.Length; i++)

            searchedHexTiles.Add(tile);

            tileNodeDict.Add(tileIndex, tileNodeGroup);

            tileIndex++;

        }//foreach (HexTile tile in HexGridManager.Instance.HextTileDict.Values)
    }

    private void GroupSideLandscape(LandscapeModel center, LandscapeModel side) 
    {
        if (side.HasGroup)
        {
            List<LandscapeModel> sideNodes = side.group.nodes;

            side.group = center.group;
            center.group.AddNode(sideNodes);
            return;
        }

        side.group = center.group;
        center.group.AddNode(side);
    }

    private void MergeNodes() 
    {
        foreach (HexTile tile in HexGridManager.Instance.PlacedHexTileDict.Values) 
        {
            foreach (LandscapeModel landscape in tile.Lanscapes)
            {
                if (!landscape.HasGroup)
                    continue;

                LandscapeModel neighbour = HexGridManager.Instance.GetNeighbourHexLandscape(tile.hex, landscape.direction);

                if (neighbour == null || !neighbour.HasGroup || neighbour.landscapeType != landscape.landscapeType)
                    continue;

                List<LandscapeModel> neighbourNodes = neighbour.group.nodes;

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

        foreach (HexTile tile in HexGridManager.Instance.PlacedHexTileDict.Values) 
        {
            foreach (LandscapeModel landscape in tile.Lanscapes) 
            {
                if (!landscape.HasGroup)
                    continue;

                int tileId = landscape.group.tileId;
                int groupId = landscape.group.groupId;

                if (tileNodeDict.ContainsKey(tileId))
                {
                    TileNodeGroup tileNodeGroup = null;
                    tileNodeDict.TryGetValue(tileId, out tileNodeGroup);

                    if (tileNodeGroup.ContainsKey(landscape.landscapeType, groupId))
                        continue;

                    tileNodeGroup.AddNodeGroup(landscape.landscapeType, groupId, landscape.group);
                }
                else
                {
                    TileNodeGroup tileNodeGroup = new TileNodeGroup();
                    tileNodeGroup.AddNodeGroup(landscape.landscapeType, groupId, landscape.group);
                    tileNodeDict.Add(tileId, tileNodeGroup);

                }//if (tileNodeDict.ContainsKey(tileId))

            }//foreach (LandscapeModel landscape in tile.Lanscapes) 
        }//foreach (HexTile tile in HexGridManager.Instance.HextTileDict.Values)
    }

    private Dictionary<(int, int), NodeGroup> forestResourceNodeDict = new Dictionary<(int, int), NodeGroup>();
    private Dictionary<(int, int), NodeGroup> fieldResourceNodeDict = new Dictionary<(int, int), NodeGroup>();

    private void SetResourceNodes() 
    {
        if (tileNodeDict == null || tileNodeDict.Count < 1)
            return;

        foreach (TileNodeGroup tileNodeGroup in tileNodeDict.Values)
        {
            //Set Forest Resource Nodes
            foreach (NodeGroup nodeGroup in tileNodeGroup.ForestNodeGroups)
            {
                (int, int) key = (nodeGroup.tileId, nodeGroup.groupId);

                if (forestResourceNodeDict.ContainsKey(key))
                    continue;

                forestResourceNodeDict.Add(key, nodeGroup);

            }//foreach (NodeGroup nodeGroup in tileNodeGroup.ForestNodeGroups)

            //Set Field Resource Nodes
            foreach (NodeGroup nodeGroup in tileNodeGroup.FieldNodeGroups)
            {
                (int, int) key = (nodeGroup.tileId, nodeGroup.groupId);

                if (fieldResourceNodeDict.ContainsKey(key))
                    continue;

                fieldResourceNodeDict.Add(key, nodeGroup);

            }//foreach (NodeGroup nodeGroup in tileNodeGroup.ForestNodeGroups)


        }//foreach (TileNodeGroup tileNodeGroup in tileNodeDict.Values)
    }

    private void DrawMergeGizmo() 
    {
        if (tileNodeDict == null || tileNodeDict.Count < 1)
            return;

        GUIStyle style = EditorStyles.boldLabel;
        style.fontSize = 18;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.blue;

        foreach (var tileNodeGroup in tileNodeDict.Values)
        {
            foreach (var nodeGroup in tileNodeGroup.ForestNodeGroups)
            {
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

    private void DrawResourceNodeGizmo() 
    {
        if (forestResourceNodeDict == null || forestResourceNodeDict.Count < 1)
            return;

        if (fieldResourceNodeDict == null || fieldResourceNodeDict.Count < 1)
            return;

        int index = 0;
        foreach (NodeGroup nodeGroup in forestResourceNodeDict.Values)
        {
            Gizmos.color = Color.red;
            Vector3 pos = nodeGroup.nodes[0].position;
            pos.y += 0.5f;
            Gizmos.DrawCube(pos, Vector3.one * 0.1f);

            Gizmos.color = HexUtils.GizmosColors[index % HexUtils.GizmosColors.Length];//Color.green;
            int nodeIndex = 0;
            foreach (LandscapeModel node in nodeGroup.nodes)
            {
                pos = node.position;
                pos.y += 0.25f;

                Gizmos.DrawSphere(pos, 0.1f);

                if (nodeIndex > 0)
                {
                    Vector3 prevNodePos = nodeGroup.nodes[nodeIndex - 1].position;
                    prevNodePos.y += 0.25f;

                    Gizmos.DrawLine(prevNodePos, pos);
                }

                nodeIndex++;
            }

            index++;
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

public class TileNodeGroup 
{
    NodeCollection forestNodeCollection;
    NodeCollection fieldNodeCollection;

    public IEnumerable<NodeGroup> ForestNodeGroups => forestNodeCollection.NodeGroups;
    public IEnumerable<NodeGroup> FieldNodeGroups => fieldNodeCollection.NodeGroups;

    public TileNodeGroup()
    {
        forestNodeCollection = new NodeCollection();
        fieldNodeCollection = new NodeCollection();
    }

    public int GroupIndex(LandscapeTypes landscapeType) 
    {
        switch (landscapeType)
        {
            case LandscapeTypes.Forest:
                return forestNodeCollection.groupIndex;
            case LandscapeTypes.Field:
                return fieldNodeCollection.groupIndex;
            default:
                return -1;
        }
    }

    public void AddNodeGroup(LandscapeTypes landscapeType, NodeGroup group) 
    {
        switch (landscapeType)
        {
            case LandscapeTypes.Forest:
                forestNodeCollection.AddNodeGroup(group);
                break;
            case LandscapeTypes.Field:
                fieldNodeCollection.AddNodeGroup(group);
                break;
        }
    }

    public void AddNodeGroup(LandscapeTypes landscapeType, int groupId, NodeGroup group) 
    {
        switch (landscapeType)
        {
            case LandscapeTypes.Forest:
                forestNodeCollection.AddNodeGroup(groupId, group);
                break;
            case LandscapeTypes.Field:
                fieldNodeCollection.AddNodeGroup(groupId, group);
                break;
        }
    }

    public bool ContainsKey(LandscapeTypes landscapeType, int groupId) 
    {
        switch (landscapeType)
        {
            case LandscapeTypes.Forest:
                return forestNodeCollection.ContainsKey(groupId);
            case LandscapeTypes.Field:
                return fieldNodeCollection.ContainsKey(groupId);
            default:
                return false;
        }
    }
}

public class NodeCollection 
{
    public int groupIndex;
    public Dictionary<int, NodeGroup> groupDict;

    public IEnumerable<NodeGroup> NodeGroups => groupDict.Values;

    public NodeCollection()
    {
        groupIndex = 0;
        groupDict = new Dictionary<int, NodeGroup>();
    }

    public void AddNodeGroup(NodeGroup group) 
    {
        groupDict.Add(groupIndex, group);
        groupIndex++;
    }

    public void AddNodeGroup(int groupIndex, NodeGroup group) 
    {
        groupDict.Add(groupIndex, group);
    }

    public bool ContainsKey(int groupId) => groupDict.ContainsKey(groupId);
}
