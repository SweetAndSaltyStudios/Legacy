using UnityEngine;
using System.Collections.Generic;

public class EndlessMap : MonoBehaviour
{
    #region Variables

    public LevelOfDetailInfo[] DetailLevels;
    private static float maxViewDistance;

    public Transform viewer;
    public Material mapMaterial;

    public static Vector2 viewerPosition;
    private int chunkSize;
    private int chunkVisibleInViewDistance;

    private Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    private List<TerrainChunk> terrainChuncksVisibleLastUpdate = new List<TerrainChunk>();

    #endregion VARIABLES

    [System.Serializable]
    public struct LevelOfDetailInfo
    {
        public int levelOfDetail;
        public float visibleDistanceThreshold;
    }

    public class TerrainChunk
    {
        private GameObject meshObject;
        private Vector2 position;
        private Bounds bounds;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;

        private LevelOfDetailInfo[] levelOfDetails;
        private LevelOfDetailMesh[] levelOfDetailMeshes;

        private MapData mapData;
        private bool mapDataRecieved;
        private int previousLevelOfDetailIndex = -1;

        public TerrainChunk
            (
            Vector2 coordinates, 
            int size,
            LevelOfDetailInfo[] levelOfDetailInfos,
            Transform parent, Material material
            )
        {
            levelOfDetails = levelOfDetailInfos;

            position = coordinates * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 worldPosition = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshRenderer.material = material;
            meshObject.transform.position = worldPosition;
            meshObject.transform.SetParent(parent);

            SetVisible(false);

            levelOfDetailMeshes = new LevelOfDetailMesh[levelOfDetailInfos.Length];

            for (int i = 0; i < levelOfDetailInfos.Length; i++)
            {
                levelOfDetailMeshes[i] = new LevelOfDetailMesh(levelOfDetailInfos[i].levelOfDetail);
            }

            MapGenerator.Instance.RequestMapData(OnMapDataReceived);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }

        private void OnMapDataReceived(MapData mapData)
        {
            this.mapData = mapData;
            mapDataRecieved = true;
        }

        public void UpdateTerrainChunk()
        {
            if (mapDataRecieved)
            {
                var viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;

                if (visible)
                {
                    int currentLevelOfDetailIndex = 0;

                    for (int i = 0; i < levelOfDetails.Length - 1; i++)
                    {
                        if (viewerDistanceFromNearestEdge > levelOfDetails[i].visibleDistanceThreshold)
                        {
                            currentLevelOfDetailIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (currentLevelOfDetailIndex != previousLevelOfDetailIndex)
                    {
                        LevelOfDetailMesh levelOfDetailMesh = levelOfDetailMeshes[currentLevelOfDetailIndex];
                        if (levelOfDetailMesh.hasMesh)
                        {
                            previousLevelOfDetailIndex = currentLevelOfDetailIndex;
                            meshFilter.mesh = levelOfDetailMesh.mesh;
                        }
                        else if (!levelOfDetailMesh.hasRequestedMesh)
                        {
                            levelOfDetailMesh.RequestMesh(mapData);
                        }
                    }
                }

                SetVisible(visible);
            }       
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }
    }

    private class LevelOfDetailMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;

        private int levelOfDetail;

        public LevelOfDetailMesh(int levelOfDetail)
        {
            this.levelOfDetail = levelOfDetail;
        }

        private void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;
        }

        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            MapGenerator.Instance.RequestMeshData(mapData, levelOfDetail, OnMeshDataReceived);
        }
    }

    private void Start()
    {
        maxViewDistance = DetailLevels[DetailLevels.Length - 1].visibleDistanceThreshold;
        chunkSize = MapGenerator.MAP_CHUNK_SIZE - 1;
        chunkVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);
    }

    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
    }

    private void UpdateVisibleChunks()
    {
        for (int i = 0; i < terrainChuncksVisibleLastUpdate.Count; i++)
        {
            terrainChuncksVisibleLastUpdate[i].SetVisible(false);
        }

        terrainChuncksVisibleLastUpdate.Clear();

        int currentChunkCoordinateX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordinateY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunkVisibleInViewDistance; yOffset <= chunkVisibleInViewDistance; yOffset++)
        {
            for (int xOffset = -chunkVisibleInViewDistance; xOffset <= chunkVisibleInViewDistance; xOffset++)
            {
                Vector2 viewedChunkCoordinates = new Vector2(currentChunkCoordinateX + xOffset, currentChunkCoordinateY + yOffset);

                if (terrainChunkDictionary.ContainsKey(viewedChunkCoordinates))
                {               
                    terrainChunkDictionary[viewedChunkCoordinates].UpdateTerrainChunk();
                    if (terrainChunkDictionary[viewedChunkCoordinates].IsVisible())
                    {
                        terrainChuncksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoordinates]);
                    }
                }
                else
                {
                    terrainChunkDictionary.Add(viewedChunkCoordinates, new TerrainChunk(viewedChunkCoordinates, chunkSize, DetailLevels, transform, mapMaterial));
                }
            }
        }
    }  
}
