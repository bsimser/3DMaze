using System.Collections.Generic;
using UnityEngine;

namespace SimsTools.WinMaze
{
    public class GameControl : MonoBehaviour
    {
        public GameObject player;
        
        [Header("Parent Nodes")]
        public Transform walls;
        public Transform sprites;
        public Transform spheres;
        public Transform mapIcons;
        
        [Header("Prefabs")]
        public GameObject regularWallPrefab;
        public GameObject openGLWallPrefab;
        public GameObject[] sphereObjectPrefabs;
        public GameObject openGLPrefab; 
        public GameObject startSpritePrefab;
        public GameObject finishSpritePrefab;
        public GameObject ratSpritePrefab;

        [Header("Map Icons")]
        public int mapLayer = 1;
        public GameObject mapPlayerIcon;
        public GameObject mapStartIcon;
        public GameObject mapRatIcon;
        public GameObject mapOpenGLIcon;
        public GameObject mapFinishIcon;
        public GameObject mapObjectIcon;
        
        public MazeBlockWall[,] mazeBlocks;

        public readonly int[,] wallBlocks =
        {
            { -1, 0 },
            { 0, 1 },
            { 1, 0 },
            { 0, -1 }
        };
        
        [HideInInspector]
        public List<Transform> spriteList;

        [HideInInspector]
        public Quaternion[] lookDirections =
        {
            Quaternion.Euler(0, 270, 0),
            Quaternion.Euler(0, 0, 0),
            Quaternion.Euler(0, 90, 0),
            Quaternion.Euler(0, 180, 0)
        };

        private int _mazeGenCount;
        private List<Transform> _wallList;
        private List<Vector3> _usedPositions;
        private List<SphereObj> _sphereList;
        private RatScript _rat;
        private PlayerControl _playerControl;
        private Bounds _finishBounds;
        private readonly Vector3 _boundsSize = new Vector3(4f, 4f, 4f);
        private readonly Quaternion _mapObjectRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);

        private const int MazeSize = 20;
        private const int MazeBlocks = MazeSize * MazeSize;
        private const int MazeWallBlock = 4;
        private const float WallDistance = 2f;
        private const float WallDistanceGrid = WallDistance * 2f;
        private const float WallStart = -(MazeSize * WallDistanceGrid) / 2f + WallDistance;

        private readonly MazeWallOffset[] _wallOffsets =
        {
            new() { Position = new Vector3(-WallDistance, -2, 0), Rotation = Quaternion.Euler(0, 90, 0) },
            new() { Position = new Vector3(0, -2, WallDistance), Rotation = Quaternion.Euler(0, 0, 0) },
            new() { Position = new Vector3(WallDistance, -2, 0), Rotation = Quaternion.Euler(0, 90, 0) },
            new() { Position = new Vector3(0, -2, -WallDistance), Rotation = Quaternion.Euler(0, 0, 0) }
        };

        private void Awake()
        {
            spriteList = new List<Transform>();
            _wallList = new List<Transform>();
            _usedPositions = new List<Vector3>();
            _sphereList = new List<SphereObj>();
            _playerControl = player.GetComponent<PlayerControl>();
        }

        private void Start()
        {
            CreateMaze();
        }

        private void CreateMaze()
        {
            CreateMazeWalls();
            SetPlayerPosition();
            CreateMapPlayer();
            CreateStartSprite();
            CreateFinishBounds();
            CreateSpheres();
            for (var i = 0; i < Random.Range(5, 10); i++)
            {
                CreateOpenGLMapObject();
            }
            CreateRat();
            AdjustPlayerPosition();
            MazeScaler.instance.Scale(1f, () => EnablePlayer(true));
        }

        private void CreateMapPlayer()
        {
            if (_rat == null)
            {
                CreateMovingMapIcon(player.transform, mapPlayerIcon, new Vector3(0f, 6f, 0f));
            }
        }

        private void AdjustPlayerPosition()
        {
            player.transform.rotation = Quaternion.identity;
            player.transform.position += new Vector3(0f, 0f, -3f);
        }

        private void CreateStartSprite()
        {
            var tr = CreateSprite(player.transform.position, startSpritePrefab);
            CreateMapIcon(tr.position, mapStartIcon);
        }

        private void CreateRat()
        {
            if (_rat == null)
            {
                var go = Instantiate(ratSpritePrefab);
                _rat = go.GetComponent<RatScript>();
                _rat.enabled = false;
                CreateMovingMapIcon(go.transform, mapRatIcon, new Vector3(0f, 5f, 0f));
            }
            var ratScript = _rat.GetComponent<RatScript>();
            ratScript.SetPosition(Random.Range(0, MazeSize), Random.Range(0, MazeSize), this);
        }

        private void CreateOpenGLMapObject()
        {
            CreateMapIcon(CreateSprite(GetRandomPos(), openGLPrefab).position, mapOpenGLIcon);
        }

        private void CreateFinishBounds()
        {
            Vector3 randomPosition;
            
            do
            {
                randomPosition = GetRandomPos();
            } while (randomPosition == player.transform.position);
            var position = CreateSprite(randomPosition, finishSpritePrefab).position;
            CreateMapIcon(position, mapFinishIcon);
            _finishBounds = CreateBounds(position);
        }

        private Bounds CreateBounds(Vector3 pos)
        {
            return new Bounds(pos, _boundsSize);
        }

        private void CreateSpheres()
        {
            for (var i = 0; i < Random.Range(5, 10); i++)
            {
                CreateSphere(GetRandomPos(), Random.Range(0, 3));
            }
        }

        private void CreateSphere(Vector3 pos, int typ)
        {
            var go = Instantiate(sphereObjectPrefabs[typ], spheres, true);
            
            go.transform.position = pos + new Vector3(0f, -0.5f, 0f);
            _usedPositions.Add(pos);
            var mapObj = CreateSpinningMapIcon(go.transform, mapObjectIcon);
            var spr = new SphereObj
            {
                GameObject = go,
                MapObject = mapObj,
                Bounds = CreateBounds(pos)
            };
            _sphereList.Add(spr);
        }

        private Vector3 GetRandomPos()
        {
            var position = Vector3.zero;
            
            do
            {
                position = GetGridCoords(Random.Range(0, MazeSize), Random.Range(0, MazeSize));
            } while (_usedPositions.Contains(position));

            return position;
        }

        private void SetPlayerPosition()
        {
            _playerControl.SetPosition(Random.Range(0, MazeSize), Random.Range(0, MazeSize), this);
        }

        private void CreateMazeWalls()
        {
            mazeBlocks = new MazeBlockWall[MazeSize, MazeSize];
            for (var i = 0; i < MazeSize; i++)
            {
                for (var j = 0; j < MazeSize; j++)
                {
                    mazeBlocks[i, j] = MazeBlockWall.All;
                }
            }
            GenerateMaze();
            for (var i = 0; i < MazeSize; i++)
            {
                for (var j = 0; j < MazeSize; j++)
                {
                    var block = mazeBlocks[i, j];
                    if ((block & MazeBlockWall.Down) != MazeBlockWall.None)
                    {
                        CreateWall(3, GetGridCoords(i, j));
                    }

                    if ((block & MazeBlockWall.Left) != MazeBlockWall.None)
                    {
                        CreateWall(0, GetGridCoords(i, j));
                    }

                    if (i == MazeSize - 1 && (block & MazeBlockWall.Right) != MazeBlockWall.None)
                    {
                        CreateWall(2, GetGridCoords(i, j));
                    }

                    if (j == MazeSize - 1 && (block & MazeBlockWall.Up) != MazeBlockWall.None)
                    {
                        CreateWall(1, GetGridCoords(i, j));
                    }
                }
            }
        }

        private Transform CreateSprite(Vector3 pos, GameObject sprite)
        {
            var tr = Instantiate(sprite).transform;
            
            tr.parent = sprites;
            tr.position = pos;
            spriteList.Add(tr);
            _usedPositions.Add(pos);
            
            return tr;
        }

        private void CreateWall(int offset, Vector3 pos)
        {
            var wall = Instantiate(Random.Range(0, 100) < 5 ? openGLWallPrefab : regularWallPrefab, 
                pos + _wallOffsets[offset].Position, _wallOffsets[offset].Rotation);

            wall.transform.parent = walls;
            _wallList.Add(wall.transform);
        }

        public static Vector3 GetGridCoords(int x, int y)
        {
            return new Vector3(WallStart + WallDistanceGrid * x, 0, WallStart + WallDistanceGrid * y);
        }

        private void GenerateMaze()
        {
            var x = Random.Range(0, MazeSize);
            var y = Random.Range(0, MazeSize);

            _mazeGenCount = 0;

            while (true)
            {
                DestroyWall(x, y);
                if(_mazeGenCount < MazeBlocks)
                {
                    for(x = 0; x < MazeSize; x++)
                    {
                        for(y = 0; y < MazeSize; y++)
                        {
                            if(mazeBlocks[x, y] != MazeBlockWall.All) continue;
                            for(var i = 0; i < MazeWallBlock; i++)
                            {
                                var mx = x + wallBlocks[i, 0];
                                var my = y + wallBlocks[i, 1];
                                if (mx < 0 || mx >= MazeSize || my < 0 || my >= MazeSize) continue;
                                if(mazeBlocks[mx, my] == MazeBlockWall.All) continue;
                                x = mx;
                                y = my;
                            }
                        }
                    }
                    continue;
                }
                break;
            }
        }

        private void DestroyWall(int x, int y)
        {
            var index = Random.Range(0, MazeWallBlock);
            var mx = x + wallBlocks[index, 0];
            var my = y + wallBlocks[index, 1];

            for(var i = 0; i < MazeWallBlock; i++)
            {
                if(mx >= 0 && mx < MazeSize && my >= 0 && my < MazeSize)
                {
                    if(mazeBlocks[mx, my] == MazeBlockWall.All)
                    {
                        mazeBlocks[x, y] &= ~(MazeBlockWall)(1 << index);
                        mazeBlocks[mx, my] &= ~(MazeBlockWall)((1 << index + 2) % 15);
                        DestroyWall(mx, my);
                    }
                }
                index = i;
                mx = x + wallBlocks[index, 0];
                my = y + wallBlocks[index, 1];
            }

            _mazeGenCount++;
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.N))
            {
                return;
            }
            EnablePlayer(false);
            MazeScaler.instance.Scale(-1f, RebuildMaze);
        }

        private void FixedUpdate()
        {
            var position = player.gameObject.transform.position;
            CheckForFinish(position);
            CheckForMapObject(position);
        }

        private void CheckForMapObject(Vector3 position)
        {
            for (var i = 0; i < _sphereList.Count; i++)
            {
                var sphere = _sphereList[i];
                if (!sphere.Bounds.Contains(position))
                {
                    continue;
                }
                _sphereList.Remove(sphere);
                _playerControl.Spin(() =>
                {
                    Destroy(sphere.GameObject);
                    Destroy(sphere.MapObject);
                });
                break;
            }
        }

        private void CheckForFinish(Vector3 position)
        {
            if (!_finishBounds.Contains(position))
            {
                return;
            }
            EnablePlayer(false);
            MazeScaler.instance.Scale(-1f, RebuildMaze);
        }

        private void RebuildMaze()
        {
            ClearMaze();
            CreateMaze();
        }

        private void ClearMaze()
        {
            _wallList.Clear();
            spriteList.Clear();
            _sphereList.Clear();
            _usedPositions.Clear();
            DestroyChildren(walls);
            DestroyChildren(sprites);
            DestroyChildren(spheres);
            DestroyChildren(mapIcons);
        }

        /// <summary>
        /// Don't destroy something inside a collection you are cycling (e.g. with a foreach).
        /// Iterate backwards through the collection
        /// </summary>
        /// <param name="parent"></param>
        private static void DestroyChildren(Transform parent)
        {
            var count = parent.childCount;
            
            for (var i = count - 1; i >= 0; i--)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
        }

        public static bool CheckMovement(MazeBlockWall block, int dir)
        {
            return (block & (MazeBlockWall)((int)MazeBlockWall.Left << dir)) == MazeBlockWall.None;
        }

        private void EnablePlayer(bool isEnabled)
        {
            _playerControl.enabled = isEnabled;
            _rat.enabled = isEnabled;
            enabled = isEnabled;
        }

        private void CreateMovingMapIcon(Transform tr, GameObject icon, Vector3 offset)
        {
            var go = Instantiate(icon, tr.position, _mapObjectRotation);
            var mapIconScript = go.GetComponent<MapIcon>();
            mapIconScript.target = tr;
            mapIconScript.offset = offset;
            go.layer = mapLayer;
        }

        private GameObject CreateSpinningMapIcon(Transform tr, GameObject icon)
        {
            var go = Instantiate(icon, tr.position, _mapObjectRotation);
            go.transform.parent = mapIcons;
            go.layer = mapLayer;
            return go;
        }

        private void CreateMapIcon(Vector3 pos, GameObject icon)
        {
            var go = Instantiate(icon, pos, _mapObjectRotation);
            go.transform.parent = mapIcons;
            go.layer = mapLayer;
        }
    }
}