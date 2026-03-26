using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Tilemaps;

public enum Layers
{
    Tiles = 0,
    Details = 1,
    Objects = 2,
    Background = 3
}

// Used to transition between scenes
public class GlobalVariables
{
    static public bool GameLocked = true;
    static public bool SubscriptionService = false;
	static public int WorldIndex = 0;
	static public DirectionEnum direction = DirectionEnum.Forwards;

    static public bool IsBonus = false;

    static public bool LoadSaved = false;
    static public int LevelIndex = -1;

    static public int ForceLevelNumber = -1;
    static public int LastLevelNumber = -1;

    static public int SavedLevelIndex = -1;
    static public int SavedWorldIndex = -1;
    static public int SaveSlot = -1;
    static public int StartHealth = 5;
    static public int StartEnergy = 0;
    static public string Leaderboard;
    static public int SavedSpecialGems = 0;

    // PROGRESSION
    static public bool CanShoot = true;
    static public bool CanDoubleJump = true;
    static public bool CanMelee = false;

    // PLAYER ATTRIBUTES
    static public int MaxHealth = 5;
    static public int MaxGems = 599;
    static public int MaxWeapon = 0;

    static public bool TwoPlayer = false;
    static public int PlayerTwoHealth = 5;
    static public float TotalGameTime = 0;
}

public class LevelVariables
{
    static public int switchCount;
    static public bool[] switchState = { false, false, false, false, false, false, false, false, false, false };

    static public bool partFound;
    static public bool critterFound;
    static public bool bossDefeated;
    static public bool bonusVisited;

    static public int blockCount;
    static public bool[] blockState = { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false};

    static public int specialGemCount;
    static public bool[] specialGemState = { false, false, false };

    // Not saved
    static public int stolenGems;
}

[System.Serializable]
public class PickerSave
{
    public int SavedLevelIndex;
    public int SavedWorldIndex;
    public float SavedGameTime;
    public int SavedHealth;
    public int SavedEnergy;
    public int SavedMaxHealth;
    public int SavedMaxGems;
    public int SavedMaxWeapon;
    public bool SavedMelee;
    public int SavedPlayerTwoHealth;
    public int SavedSpecialGems;
    public bool SavedTwoPlayers;
}

[System.Serializable]
public class PickerLevelSave
{
    public bool[] switchState;
    public bool partFound;
    public bool critterFound;
    public bool bossDefeated;
    public bool bonusVisited;
    public bool[] blockState;
    public bool[] specialGemState;
}

public class PickerTileMap : MonoBehaviour
{
    private Sprite[] tiles;
    private Sprite[] details;
    private Sprite[] backgrounds;
    private Sprite[] objects; //temp
    private TextAsset txt;
    private Material mat;

    public Tilemap TileMap;
    public Tilemap DetailMap;
    public Tilemap BackgroundMap;
    public GameObject Objects;

    public CharacterBehavior player;

    public int WorldIndex = 1;
    public int LevelIndex = 1;

    public int LevelNumber = 0;

    public int[] Levels = { -1, -1, -1, -1, -1, -1 };

    [System.Serializable]
    public class PickerCell
    {
        public string Name;
        public int Frame;
        public int Layer;
        public int DirX;
		public int DirY;
		public int Rotation;
        public string Prefab;
    }

    [System.Serializable]
    public class PickerRow
    {
        public PickerCell[] Cells; // The wrapped array.
    }

    [System.Serializable]
    public class PickerLevel
    {
        public PickerRow[] Details;
        public PickerRow[] Objects;
        public PickerRow[] Tiles;
        public PickerRow[] Background;
    }

    private PickerLevel levelData;

    
    

    public virtual void Awake()
    {
        Setup();
    }

    public void Setup()
    {

        GameManager.Instance.Grid = this;

#if UNITY_TVOS
        UnityEngine.tvOS.Remote.allowExitToHome = false;
#endif

        if (GlobalVariables.LoadSaved)
        {
#if UNITY_TVOS
            string saveData = PlayerPrefs.GetString("SavedGame" + GlobalVariables.SaveSlot, "");
                
            if(saveData != "")
            {
                PickerSave savedGame = JsonUtility.FromJson<PickerSave>(saveData);
#else
            var filePath = Application.persistentDataPath + "/savedgame" + GlobalVariables.SaveSlot + ".json";
            if (File.Exists(filePath))
            {

                PickerSave savedGame = JsonUtility.FromJson<PickerSave>(File.ReadAllText(filePath));
#endif

                if (savedGame != null)
                {
                    GlobalVariables.SavedLevelIndex = savedGame.SavedLevelIndex;
                    GlobalVariables.SavedWorldIndex = savedGame.SavedWorldIndex;
                    GlobalVariables.StartHealth = savedGame.SavedHealth;
                    GlobalVariables.StartEnergy = savedGame.SavedEnergy;
                    GlobalVariables.MaxHealth = savedGame.SavedMaxHealth;
                    GlobalVariables.MaxGems = savedGame.SavedMaxGems;
                    GlobalVariables.MaxWeapon = savedGame.SavedMaxWeapon;
                    GlobalVariables.CanMelee = savedGame.SavedMelee;
                    GlobalVariables.PlayerTwoHealth = savedGame.SavedPlayerTwoHealth;
                    GlobalVariables.TotalGameTime = savedGame.SavedGameTime;
                    GlobalVariables.SavedSpecialGems = savedGame.SavedSpecialGems;
                    GlobalVariables.TwoPlayer = savedGame.SavedTwoPlayers;

                    Debug.Log("Loading saved game " + GlobalVariables.StartHealth);
                }
            }

            GlobalVariables.LoadSaved = false;
        }

        if (GlobalVariables.SavedLevelIndex != -1)
        {
            GlobalVariables.LevelIndex = GlobalVariables.SavedLevelIndex;
            GlobalVariables.WorldIndex = GlobalVariables.SavedWorldIndex;
            Debug.Log("Loading saved level " + LevelIndex);
        }

        // Load up saved level
        if (GlobalVariables.LevelIndex != -1)
            LevelIndex = GlobalVariables.LevelIndex;
        else
            LevelIndex = Levels.Length - 1; // Going backwards

        GlobalVariables.LevelIndex = LevelIndex;

        if (GlobalVariables.WorldIndex == -1)
            GlobalVariables.WorldIndex = WorldIndex;

        Debug.Log(GlobalVariables.WorldIndex + " - " + GlobalVariables.LevelIndex);

        tiles = Resources.LoadAll<Sprite>("Tiles/world_" + GlobalVariables.WorldIndex);
		details = Resources.LoadAll<Sprite>("details");
        backgrounds = Resources.LoadAll<Sprite>("backgrounds");
        objects = Resources.LoadAll<Sprite>("objects");
    }


    public void JumpLevel()
    {
        RenderSettings.ambientIntensity = 0;
        DynamicGI.UpdateEnvironment();
        StartCoroutine(Jump(1.1f));
    }

    public void NextLevel()
    {
        RenderSettings.ambientIntensity = 0;
        DynamicGI.UpdateEnvironment();
        StartCoroutine(Next(0.1f));
    }


    public void PrevLevel()
    {
        RenderSettings.ambientIntensity = 0;
        DynamicGI.UpdateEnvironment();
        StartCoroutine(Prev(0.1f));
    }


    protected virtual IEnumerator Jump(float duration)
    {
        yield return new WaitForSeconds(duration);

        LoadLayout();
    }


    public virtual IEnumerator Next(float duration)
    {
        yield return new WaitForSeconds(duration);

        SaveLevel();

        GlobalVariables.direction = DirectionEnum.Forwards;

        if (GlobalVariables.IsBonus)
        {
            AnalyticsEvent.Custom("bonus_found", new Dictionary<string, object>
            {
                { "world", GlobalVariables.WorldIndex },
                { "time_elapsed", Time.timeSinceLevelLoad }
            });

            string achievement = "mr.achievements.bonusfound" + GlobalVariables.WorldIndex;
            LevelManager.Instance.SaveAchievement(achievement);

            if (GlobalVariables.ForceLevelNumber < 1000)
            {
                GlobalVariables.ForceLevelNumber = LevelNumber + 1000;
                GlobalVariables.LoadSaved = false;
            }
            else
            {
                GlobalVariables.ForceLevelNumber = -1;
            }
        }
        else
        {
            AnalyticsEvent.Custom("level_complete", new Dictionary<string, object>
            {
                { "level", GlobalVariables.LevelIndex },
                { "world", GlobalVariables.WorldIndex },
                { "time_elapsed", Time.timeSinceLevelLoad }
            });

            LevelIndex++;

            if (LevelIndex >= Levels.Length)
            {
                AnalyticsEvent.Custom("world_complete", new Dictionary<string, object>
                {
                    { "world", GlobalVariables.WorldIndex }
                });

                string achievement = "mr.achievements.worldcompleted" + GlobalVariables.WorldIndex;
                LevelManager.Instance.SaveAchievement(achievement);

                GlobalVariables.WorldIndex++;
                LevelIndex = 0;
                SoundManager.Instance.ResetMusicPosition();
            }
            else
            {
                SoundManager.Instance.SaveMusicPosition();
            }

            GlobalVariables.LevelIndex = LevelIndex;
        }

        Save();

#if DEMOMODE
        if (GlobalVariables.WorldIndex > 2)
        {
            LevelManager.Instance.GoToDemo();
        }
        else
        {
            LevelManager.Instance.RestartLevel();
        }
#else
        if(GlobalVariables.WorldIndex > 10)
            LevelManager.Instance.EndStory();
        else
            LevelManager.Instance.RestartLevel();
#endif
    }


    protected virtual IEnumerator Prev(float duration)
    {
        yield return new WaitForSeconds(duration);

        SaveLevel();

        GlobalVariables.direction = DirectionEnum.Backwards;

        if (GlobalVariables.IsBonus)
        {
            if (GlobalVariables.ForceLevelNumber < 1000)
            {
                GlobalVariables.ForceLevelNumber = LevelNumber + 1000;
            }
            else
            {
                GlobalVariables.ForceLevelNumber = -1;
                GlobalVariables.LoadSaved = true;
            }
        }
        else
        {
            LevelIndex--;

            if (LevelIndex < 0)
            {
                GlobalVariables.WorldIndex--;
                LevelIndex = -1;
                SoundManager.Instance.ResetMusicPosition();
            }
            else
            {
                SoundManager.Instance.SaveMusicPosition();
            }

            GlobalVariables.LevelIndex = LevelIndex;
        }

        Save();

        LevelManager.Instance.RestartLevel();
    }


    public void Disconnect()
    {
        GameManager.Instance.Player = null;
        //LevelManager.Instance.SetPlayer(null);
    }


    private void LoadLayout()
    {
        GameManager.Instance.Player = null;

        Cursor.visible = false;

        // Compensate for world number
        int l = LevelIndex;

        if (GlobalVariables.ForceLevelNumber != -1)
        {
            LevelNumber = GlobalVariables.ForceLevelNumber;
            Debug.Log("Forcing level load of " + LevelNumber);
        }
        else
        {
            LevelNumber = Levels[l];
            Debug.Log("Adjusted index is " + l);
        }

        Debug.Log("Level number is" + LevelNumber);

        // Load special dynamic layers that can't be created in Picker
        var dynamicPrefab = Resources.Load("Environment/Dynamic" + LevelNumber) as GameObject;
        if (dynamicPrefab != null)
        {
            var dynamic = Instantiate(dynamicPrefab, Vector3.zero, Quaternion.identity);
            dynamic.transform.parent = transform;
        }

        // Load tiles
        txt = (TextAsset)Resources.Load("Levels/level" + LevelNumber, typeof(TextAsset));

        //Debug.Log ("Loading layout for " + LevelNumber + " at index " + LevelIndex);

        // Clear out the Grid, unless it's a player
        TileMap.ClearAllTiles();
        BackgroundMap.ClearAllTiles();
        DetailMap.ClearAllTiles();

        foreach (Transform child in Objects.transform)
            Destroy(child.gameObject);

        Load();
    }

    void Start()
    {

        LoadLayout();

        // Temp
        //StartCoroutine(Next(5));
    }


    Vector2 positionFromIndex(int index)
    {
        float scale = 2f;

        int row = index;
        int col = 0;
        while (row >= 32)
        {
            row -= 32;
            col++;
        }

		float x = scale * col - 31;
        float y = scale * (31 - row) - 31;

		return new Vector2(x, y);
    }



    GameObject Draw(string name, string sortLayer, Sprite[] source, int index, int frame, float z, bool collider)
    {
        if (frame < 0)
        {
            return null;
        }

        // Index needs to be converted into a position
        Vector2 pos = positionFromIndex(index);

        if (frame > source.Length)
        {
            //Debug.Log (frame + " is bigger than  cap of " + source.Length);
            return null;
        }

        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = source[frame];

        if (source == tiles)
        {
            tile.colliderType = Tile.ColliderType.Sprite;
            TileMap.SetTile(new Vector3Int((int)pos.x, (int)pos.y, (int)z), tile);
        }
        else if (source == backgrounds)
        {
            tile.colliderType = Tile.ColliderType.None;
            BackgroundMap.SetTile(new Vector3Int((int)pos.x, (int)pos.y, (int)z), tile);
        }
        else
        {
            tile.colliderType = Tile.ColliderType.None;
            DetailMap.SetTile(new Vector3Int((int)pos.x, (int)pos.y, (int)z), tile);
        }

        return null;
    }


    public void Save()
    {
        PickerSave save = new PickerSave();
        save.SavedLevelIndex = GlobalVariables.LevelIndex;
        save.SavedWorldIndex = GlobalVariables.WorldIndex;
        save.SavedHealth = GameManager.Instance.Player.Health;
        save.SavedEnergy = GameManager.Instance.Points;
        save.SavedMaxHealth = GameManager.Instance.Player.BehaviorParameters.MaxHealth;
        save.SavedMaxGems = GameManager.Instance.Player.BehaviorParameters.MaxGems;
        save.SavedMaxWeapon = GameManager.Instance.Player.BehaviorParameters.MaxWeapon;
        save.SavedMelee = GameManager.Instance.Player.BehaviorState.CanMelee;
        save.SavedPlayerTwoHealth = (GameManager.Instance.PlayerTwo != null) ? GameManager.Instance.PlayerTwo.Health : 0;
        save.SavedSpecialGems = GameManager.Instance.SpecialPoints;
        save.SavedTwoPlayers = GlobalVariables.TwoPlayer;

        GlobalVariables.TotalGameTime += Time.timeSinceLevelLoad;
        save.SavedGameTime = GlobalVariables.TotalGameTime;

        string jsonData = JsonUtility.ToJson(save, true);

#if UNITY_TVOS
        PlayerPrefs.SetString("SavedGame" + GlobalVariables.SaveSlot, jsonData);
#else
        File.WriteAllText(Application.persistentDataPath + "/savedgame" + GlobalVariables.SaveSlot + ".json", jsonData);
#endif

        GlobalVariables.LoadSaved = true;
    }


    public void SaveLevel()
    {
        Debug.Log("Attempting to save level number " + LevelNumber);
        if (LevelNumber > 1000)
            return;

        PickerLevelSave saveLevel = new PickerLevelSave();

        saveLevel.switchState = new bool[LevelVariables.switchCount];

        for (int i = 0; i < LevelVariables.switchCount; i++)
        {
            saveLevel.switchState[i] = LevelVariables.switchState[i];
        }

        saveLevel.blockState = new bool[LevelVariables.blockCount];

        for (int i = 0; i < LevelVariables.blockCount; i++)
        {
            saveLevel.blockState[i] = LevelVariables.blockState[i];
        }

        saveLevel.specialGemState = new bool[LevelVariables.specialGemCount];

        for (int i = 0; i < LevelVariables.specialGemCount; i++)
        {
            saveLevel.specialGemState[i] = LevelVariables.specialGemState[i];
        }

        saveLevel.critterFound = LevelVariables.critterFound;
        saveLevel.partFound = LevelVariables.partFound;
        saveLevel.bossDefeated = LevelVariables.bossDefeated;
        saveLevel.bonusVisited = LevelVariables.bonusVisited;

        string levelJsonData = JsonUtility.ToJson(saveLevel, true);

#if UNITY_TVOS
        PlayerPrefs.SetString("SavedLevel" + GlobalVariables.SaveSlot + "_" + GlobalVariables.WorldIndex + "_" + GlobalVariables.LevelIndex, levelJsonData);
#else
        File.WriteAllText(Application.persistentDataPath + "/savedlevel" + GlobalVariables.SaveSlot + "_" + GlobalVariables.WorldIndex + "_" + GlobalVariables.LevelIndex + ".json", levelJsonData);
#endif
        Debug.Log("Saving " + GlobalVariables.SaveSlot + "_" + GlobalVariables.WorldIndex + "_" + GlobalVariables.LevelIndex + ".json");

        GlobalVariables.LoadSaved = true;
    }


    // Backwards compatibility with old format
	GameObject specialDetail(int frame, int index)
	{
		Vector2 pos = positionFromIndex(index);

		if (frame == 6 || frame == 145)
		{
			var crystalPrefab = Resources.Load("Environment/CrystalLarge") as GameObject;
			var crystal = Instantiate(crystalPrefab, new Vector3(pos.x, pos.y, 0f), Quaternion.identity);
			crystal.transform.parent = Objects.transform;

			return crystal;
		}
		else if (frame == 5)
		{
			var crystalPrefab = Resources.Load("Environment/CrystalSmall") as GameObject;
			var crystal = Instantiate(crystalPrefab, new Vector3(pos.x, pos.y, 0f), Quaternion.identity);
            crystal.transform.parent = Objects.transform;

			return crystal;
		}
        else if (frame == 66)
        {
            var crystalPrefab = Resources.Load("Environment/IceCrystalSmall") as GameObject;
            var crystal = Instantiate(crystalPrefab, new Vector3(pos.x, pos.y, 0f), Quaternion.identity);
            crystal.transform.parent = Objects.transform;

            return crystal;
        }
        else if (frame == 67)
        {
            var crystalPrefab = Resources.Load("Environment/IceCrystalLarge") as GameObject;
            var crystal = Instantiate(crystalPrefab, new Vector3(pos.x, pos.y, 0f), Quaternion.identity);
            crystal.transform.parent = Objects.transform;

            return crystal;
        }
        else if (frame == 177)
        {
            var crystalPrefab = Resources.Load("Environment/PurpleCrystalLarge") as GameObject;
            var crystal = Instantiate(crystalPrefab, new Vector3(pos.x, pos.y, 0f), Quaternion.identity);
            crystal.transform.parent = Objects.transform;

            return crystal;
        }
        else if (frame == 209)
        {
            var crystalPrefab = Resources.Load("Environment/PinkCrystalLarge") as GameObject;
            var crystal = Instantiate(crystalPrefab, new Vector3(pos.x, pos.y, 0f), Quaternion.identity);
            crystal.transform.parent = Objects.transform;

            return crystal;
        }
        else
		{
			if (frame >= 150 && frame < 157)
			{
                GameObject cordPrefab = null;
                if (frame < 152)
                    cordPrefab = Resources.Load("Environment/PowerCordStraight") as GameObject;
                else if (frame >= 152 && frame < 156)
                     cordPrefab = Resources.Load("Environment/PowerCordCorner") as GameObject;
                else
                    cordPrefab = Resources.Load("Environment/PowerCordBoth") as GameObject;

                var cord = Instantiate(cordPrefab, new Vector3(pos.x, pos.y, 0f), Quaternion.identity);

                float rotation = 0;

                switch(frame)
                {
                    case 150:
                        rotation = 0;
                        break;
                    case 151:
                        rotation = 90;
                        break;
                    case 152:
                        rotation = 0;
                        break;
                    case 153:
                        rotation = 180;
                        break;
                    case 154:
                        rotation = 270;
                        break;
                    case 155:
                        rotation = 90;
                        break;
                    case 156:
                        rotation = 270;
                        break;
                }


                cord.GetComponent<Cord>().ReferenceFrame = frame;
                cord.transform.rotation = Quaternion.Euler(0, 0, rotation);
                cord.transform.parent = Objects.transform;
				cord.layer = 19;
				cord.isStatic = true;
                cord.name = "special_detail_" + frame;

				return cord;
			}
		}

		return null;
	}


	// Backwards compatibility with old format
    GameObject specialBackground(int frame, int index)
    {
        Vector2 pos = positionFromIndex(index);

       if (frame == 242 || frame == 243)
        {
            var vinePrefab = Resources.Load("Environment/Vine") as GameObject;
            var vine = Instantiate(vinePrefab, new Vector3(pos.x, pos.y + 0.2f, 0f), Quaternion.identity);

            if (frame == 243)
            {
                vine.GetComponent<SpriteRenderer>().flipX = true;
            }

            vine.transform.parent = Objects.transform;

            return vine;
        }

        return null;
    }


    public GameObject LoadPrefab(int index, GameObject prefab, PickerCell cell)
    {
        Vector3 offset = Vector3.zero;

        var np = positionFromIndex(index);
        var obj = Instantiate(prefab, new Vector3(np.x, np.y, 7), Quaternion.identity);

        var PickerOffset = obj.GetComponent<PickerOffset>();

        if (PickerOffset != null)
        {
            offset = new Vector3((float)cell.DirX * PickerOffset.Offset.x, (float)cell.DirY * PickerOffset.Offset.y, PickerOffset.Offset.z);

            if (PickerOffset.RotateOffset)
                offset = Quaternion.Euler(0, 0, cell.Rotation) * offset;
        }

        obj.transform.parent = Objects.transform;
        obj.transform.localScale = new Vector3((float)cell.DirX * obj.transform.localScale.x, (float)cell.DirY * obj.transform.localScale.y, 1);
        obj.transform.Translate(offset);

        return obj;
    }


    public void Load()
    {
        if (txt == null)
        {
            //Debug.Log("No level data!");
            return;
        }

        LevelVariables.blockCount = 0;
        for (int i = 0; i < 30; i++)
            LevelVariables.blockState[i] = false;

        LevelVariables.specialGemCount = 0;
        for (int i = 0; i < 3; i++)
            LevelVariables.specialGemState[i] = false;

        LevelVariables.critterFound = false;
        LevelVariables.partFound = false;
        LevelVariables.bossDefeated = false;
        LevelVariables.stolenGems = 0;
        LevelVariables.bonusVisited = false;

        LevelVariables.switchCount = 0;
        for (int i = 0; i < 10; i++)
            LevelVariables.switchState[i] = false;

#if UNITY_TVOS
        string saveLevelData = PlayerPrefs.GetString("SavedLevel" + GlobalVariables.SaveSlot + "_" + GlobalVariables.WorldIndex + "_" + GlobalVariables.LevelIndex, "");
        PickerLevelSave savedLevel = JsonUtility.FromJson<PickerLevelSave>(saveLevelData);

        if (saveLevelData != "" && saveLevelData != null)
        {
#else
        var levelFilePath = Application.persistentDataPath + "/savedlevel" + GlobalVariables.SaveSlot + "_" + GlobalVariables.WorldIndex + "_" + GlobalVariables.LevelIndex + ".json";
        if (File.Exists(levelFilePath))
        {
            PickerLevelSave savedLevel = JsonUtility.FromJson<PickerLevelSave>(File.ReadAllText(levelFilePath));
#endif
            if (savedLevel != null && GlobalVariables.ForceLevelNumber < 1000)
            {
                if (savedLevel.switchState != null)
                {
                    for (int i = 0; i < savedLevel.switchState.Length; i++)
                        LevelVariables.switchState[i] = savedLevel.switchState[i];
                }

                if (savedLevel.blockState != null)
                {
                    for (int i = 0; i < savedLevel.blockState.Length; i++)
                        LevelVariables.blockState[i] = savedLevel.blockState[i];
                }

                if (savedLevel.specialGemState != null)
                {
                    for (int i = 0; i < savedLevel.specialGemState.Length; i++)
                        LevelVariables.specialGemState[i] = savedLevel.specialGemState[i];
                }

                LevelVariables.critterFound = savedLevel.critterFound;
                LevelVariables.partFound = savedLevel.partFound;
                LevelVariables.bossDefeated = savedLevel.bossDefeated;
                LevelVariables.bonusVisited = savedLevel.bonusVisited || (GlobalVariables.LastLevelNumber > 1000);

                Debug.Log("Loading saved level. Boss defeated: " + LevelVariables.bossDefeated);
            }
        }

        GlobalVariables.LastLevelNumber = LevelNumber;

        Debug.Log("Last number is " + GlobalVariables.LastLevelNumber);

        levelData = JsonUtility.FromJson<PickerLevel>(txt.text);

        int index = 0;
        for (int t = 0; t < levelData.Tiles.Length; t++)
        {
            PickerRow tCells = levelData.Tiles[t];

            for (int c = 0; c < tCells.Cells.Length; c++)
            {
                PickerCell cell = tCells.Cells[c];

                if (cell.Prefab != "")
                {
                    if (cell.Prefab.Contains("World" + GlobalVariables.WorldIndex))
                    {
                        var prefab = Resources.Load(cell.Prefab) as GameObject;

                        if (prefab != null)
                        {
                            var obj = LoadPrefab(index, prefab, cell);
                            obj.transform.localScale = new Vector2((float)cell.DirX, (float)cell.DirY);
                            index++;
                            continue;
                        }
                        else
                        {
                            Debug.Log(cell.Prefab + " is NULL!");
                        }
                    }
                }

                Draw("tile_" + index, "Platforms", tiles, index, cell.Frame, 8, true);
                index++;
            }
        }

        int bindex = 0;
        for (int t = 0; t < levelData.Background.Length; t++)
        {
            PickerRow tCells = levelData.Background[t];

            for (int c = 0; c < tCells.Cells.Length; c++)
            {
                PickerCell cell = tCells.Cells[c];

                if (specialBackground(cell.Frame, bindex) == null)
                    Draw("background_" + bindex, "Default", backgrounds, bindex, cell.Frame, 26, false);
                bindex++;
            }
        }

        int dindex = 0;
        for (int t = 0; t < levelData.Details.Length; t++)
        {
            PickerRow tCells = levelData.Details[t];

            for (int c = 0; c < tCells.Cells.Length; c++)
            {
                PickerCell cell = tCells.Cells[c];

                if (specialDetail(cell.Frame, dindex) == null)
                {
                    if (cell.Frame > 172)
                        Draw("details_" + dindex, "Foreground", details, dindex, cell.Frame, 7, false);
                    else
                        Draw("details_" + dindex, "Default", details, dindex, cell.Frame, 7, false);
                }
                dindex++;
            }
        }

        TileMap.RefreshAllTiles();

        int oindex = 0;
        for (int t = 0; t < levelData.Objects.Length; t++)
        {
            PickerRow tCells = levelData.Objects[t];

            for (int c = 0; c < tCells.Cells.Length; c++)
            {
                PickerCell cell = tCells.Cells[c];

                //var go = Draw(cell.Name, "Platforms", objects, oindex, cell.Frame, 7, false);

                if (cell.Prefab != "")
                {
                    var prefab = Resources.Load(cell.Prefab) as GameObject;

                    if (prefab == null)
                    {
                        Debug.Log("Warning: " + cell.Prefab + " not found!");
                        oindex++;
                        continue;
                    }

                    var obj = LoadPrefab(oindex, prefab, cell);

                    AIOmniWalk omni = obj.GetComponent<AIOmniWalk>();

                    if (omni != null)
                    {
                        if (obj.transform.localScale.x == -1)
                            obj.GetComponent<SpriteRenderer>().flipX = true;

                        oindex++;
                        continue;
                    } 

                    obj.transform.localScale = new Vector2((float)cell.DirX, (float)cell.DirY);
                    obj.transform.rotation = Quaternion.Euler(0, 0, cell.Rotation);

                    // Some special cases for backward compatilibilty with the old level format

                    Door door = obj.GetComponent<Door>();
                    if(door != null)
                        obj.transform.rotation = Quaternion.Euler(0, 0, 180);

                    // Gem
                    Gem gem = obj.GetComponent<Gem>();
                    if (gem != null)
                    {
                        gem.IsStatic = true;
                        gem.Init(cell.Frame);

                        // Coin
                        Coin coin = obj.GetComponent<Coin>();

                        if (coin != null)
                        {
                            if (coin.Special > 0)
                            {
                                coin.SaveIndex = LevelVariables.specialGemCount;
                                if (LevelVariables.specialGemState[LevelVariables.specialGemCount] == true)
                                {
                                    obj.SetActive(false);
                                }
                                Debug.Log("Special gem " + LevelVariables.specialGemCount);
                                LevelVariables.specialGemCount++;
                                oindex++;
                                continue;
                            }
                        }
                        else
                        {
                            oindex++;
                            continue;
                        }
                    }


                    // Walkers that should be standing still or turning the opposite of the default direction
                    AISimpleWalk walk = obj.GetComponent<AISimpleWalk>();

                    if (walk != null)
                    {
                        obj.transform.localScale = Vector3.one;
                        if (cell.DirX == -1)
                        {
                            obj.GetComponent<SpriteRenderer>().flipX = true;
                            walk.FlipDir();
                        }

                        if (cell.Frame < 6 || cell.Frame == 26 || cell.Frame == 27)
                            walk.HardDisable();

                        oindex++;
                        continue;
                    }


                    AIShootOnSight shoot = obj.GetComponent<AIShootOnSight>();
                    BossWasp wasp = obj.GetComponent<BossWasp>();

                    if(shoot != null && wasp == null)
                    {
                        obj.GetComponent<SpriteRenderer>().flipX = true;
                        oindex++;
                        continue;
                    }

                    Fuse fuse = obj.GetComponent<Fuse>();

                    if (fuse != null)
                    {
                        fuse.SaveIndex = LevelVariables.switchCount;

                        if (LevelVariables.switchState[LevelVariables.switchCount] == true)
                            fuse.Pull(true);

                        LevelVariables.switchCount++;

                        oindex++;
                        continue;
                    }

                    // Switchables
                    Plug plug = obj.GetComponent<Plug>();

                    if(plug != null)
                    {
                        plug.SaveIndex = LevelVariables.switchCount;

                        if(LevelVariables.switchState[LevelVariables.switchCount] == true)
                            plug.Pull(true);

                        LevelVariables.switchCount++;
                        oindex++;
                        continue;
                    }

                    // Part
                    Part part = obj.GetComponent<Part>();

                    if(part != null && LevelVariables.partFound)
                    {
                        part.enabled = false;
                        obj.SetActive(false);
                        oindex++;
                        continue;
                    }

                    // Dissolve Block
                    DissolveBlock dissolveBlock = obj.GetComponent<DissolveBlock>();

                    if(dissolveBlock != null)
                    {
                        dissolveBlock.SaveIndex = LevelVariables.blockCount;
                        if (LevelVariables.blockState[LevelVariables.blockCount] == true)
                        {
                            obj.SetActive(false);
                        }
                        LevelVariables.blockCount++;
                        oindex++;
                        continue;
                    }


                    // Boss Block
                    BossWall bossWall = obj.GetComponent<BossWall>();

                    if (bossWall != null)
                    {
                        bossWall.SaveIndex = LevelVariables.blockCount;
                        if (LevelVariables.blockState[LevelVariables.blockCount] == true)
                        {
                            obj.SetActive(false);
                        }
                        LevelVariables.blockCount++;
                        oindex++;
                        continue;
                    }

                    // End special cases
                }

                oindex++;
            }
        }
    }
}

