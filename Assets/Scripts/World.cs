﻿using System.Collections;
using UnityEngine;

public class World : MonoBehaviour
{
    public string levelName;
    Vector3 entrance;

    void Start ()
    {
        var town = new Level("Act 1 - Town");
        town.Instantiate(new Vector2i(0, 0));
        entrance = new Vector3(30, -15);

        var bloodMoor = new Level("Act 1 - Wilderness 1");
        var river = DS1.Load(@"data\global\tiles\act1\outdoors\river.ds1");
        var bord2 = LevelPreset.Find("Act 1 - Wild Border 2");
        var bord3 = LevelPreset.Find("Act 1 - Wild Border 3");
        var bord6 = LevelPreset.Find("Act 1 - Wild Border 6");
        var cottage = LevelPreset.Find("Act 1 - Cottages 1");
        var denEntrance = LevelPreset.Find("Act 1 - DOE Entrance");

        for (int i = 0; i < bloodMoor.height / river.height; ++i)
            bloodMoor.Place(river, new Vector2i(bloodMoor.width - river.width, bloodMoor.height - (i + 1) * river.height));
        
        for (int i = 0; i < bloodMoor.height / bord2.sizeY; ++i)
            bloodMoor.Place(bord2, new Vector2i(0, bloodMoor.height - (i + 1) * bord2.sizeY));
        
        for (int i = 0; i < (bloodMoor.width - river.width) / bord3.sizeX; ++i)
            bloodMoor.Place(bord3, new Vector2i(i * bord3.sizeX, 0));

        bloodMoor.Place(bord6, new Vector2i(0, 0));
        for (int i = 0; i < 5; ++i)
            bloodMoor.Place(cottage, new Vector2i(10 + i * 9, 30 + Random.Range(-5, 5)));
        bloodMoor.Place(denEntrance, new Vector2i(20, 50));
        bloodMoor.Instantiate(new Vector2i(0, -bloodMoor.height));
        
        var entry = town.FindEntry();
        SpawnPlayer(Iso.MapTileToWorld(entry));
        StartCoroutine(SpawnMonsters());

        for (int i = 0; i < 5; ++i)
        {
            SpawnMonster("fallen1", entrance + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f)));
        }
    }

    static void SpawnPlayer(Vector3 pos)
    {
        var player = new GameObject("Player");
        player.transform.position = pos;
        var character = player.AddComponent<Character>();
        character.basePath = @"data\global\chars";
        character.token = "BA";
        character.weaponClass = "1SS";
        character.gear = new string[] { "LIT", "LIT", "LIT", "LIT", "LIT", "AXE", "AXE", "", "LIT", "LIT", "", "", "", "", "", "" };
        //character.token = "PA";
        //character.weaponClass = "1HS";
        //character.gear = new string[] { "CRN", "HVY", "HVY", "HVY", "HVY", "SCM", "", "KIT", "", "", "", "", "", "", "", "" };
        character.directionCount = 16;
        character.run = true;
        character.walkSpeed = 7;
        character.runSpeed = 13;
        character.maxHealth = 10000;
        character.health = 10000;
        PlayerController.instance.SetCharacter(character);

        var body = player.AddComponent<Rigidbody2D>();
        body.isKinematic = true;
        var collider = player.AddComponent<CircleCollider2D>();
        collider.radius = Iso.tileSizeY;
    }

    public static Character SpawnMonster(string id, Vector3 pos)
    {
        MonStat monStat = MonStat.Find(id);
        return SpawnMonster(monStat, pos);
    }

    public static Character SpawnMonster(MonStat monStat, Vector3 pos)
    {
        var monster = new GameObject(monStat.nameStr);
        monster.transform.position = pos;
        var character = monster.AddComponent<Character>();
        character.monStat = monStat;
        character.basePath = @"data\global\monsters";
        character.token = monStat.code;
        character.weaponClass = monStat.ext.baseWeaponClass;
        character.run = false;
        character.walkSpeed = monStat.speed;
        character.runSpeed = monStat.runSpeed;
        character.health = monStat.minHP;
        character.maxHealth = monStat.minHP;

        character.gear = new string[monStat.ext.gearVariants.Length];
        for (int i = 0; i < character.gear.Length; ++i)
        {
            var variants = monStat.ext.gearVariants[i];
            if (variants == null)
                continue;
            character.gear[i] = variants[Random.Range(0, variants.Length)];
        }

        if (monStat.ai == "Npc")
            monster.AddComponent<NpcController>();
        else if (monStat.ai != "Idle")
            monster.AddComponent<MonsterController>();

        var body = monster.AddComponent<Rigidbody2D>();
        body.isKinematic = true;
        var collider = monster.AddComponent<CircleCollider2D>();
        collider.radius = monStat.ext.sizeX * Iso.tileSizeY;

        return character;
    }

    public static StaticObject SpawnObject(ObjectInfo objectInfo, Vector3 pos)
    {
        var gameObject = new GameObject();
        gameObject.transform.position = pos;
        gameObject.name = objectInfo.description;

        var staticObject = gameObject.AddComponent<StaticObject>();
        staticObject.objectInfo = objectInfo;
        return staticObject;
    }

    public static StaticObject SpawnObject(string token, Vector3 pos)
    {
        ObjectInfo objectInfo = ObjectInfo.Find(token);
        if (objectInfo == null)
        {
            Debug.LogWarning("ObjectInfo with token'" + token + "' not found");
            return null;
        }
        return SpawnObject(objectInfo, pos);
    }

    IEnumerator SpawnMonsters()
    {
        while (true)
        {
            yield return new WaitForSeconds(10);
            for (int i = 0; i < 5; ++i)
            {
                var monster = SpawnMonster("fallen1", entrance + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f)));
                monster.ressurecting = true;
                yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
            }
        }
    }
}
