using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Enemy
{
    public Vector2 position = Vector2.zero;
    public float speed = 50;
    public float life = 1;
    public bool is_alive { get { return life > 0; } }
    public float attack_cooldown = 0;
    public float attack_cooldown_when_attacked = 1;
    public float attack_value = 10;
    public float attack_range = 100;
}

public class Projection
{
    public Vector2 position = Vector2.zero;
    public Vector2 direction = Vector2.zero;
    public float speed = 300;
    public float range = 10;
    public float attack_value = 1;
    public bool is_alive = true;
    public bool is_player_projected = true;
}

public class GameManager : MonoBehaviour
{
    InputAction moveAction;
    [SerializeField]
    float speed = 100;
    public float life = 100;
    public Vector2 player_position = Vector2.zero;
    List<Enemy> enemies = new List<Enemy>();
    List<Projection> projections = new List<Projection>();
    public float attack_cooldown = 0;
    public float attack_cooldown_when_attacked = 1;
    public float enemy_spawn_cooldown = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveAction = InputSystem.actions.FindAction("Move");
    }
    void AddEnemy()
    {
        Enemy enemy = new Enemy();
        float position = Random.value;
        if (position > 0.5)
        {
            if (position > 0.25)
            {
                enemy.position = new Vector2(-700, Random.Range(-300, 300));
            }
            else
            {
                enemy.position = new Vector2(700, Random.Range(-300, 300));
            }
        }
        else
        {
            if (position > 0.75)
            {
                enemy.position = new Vector2(Random.Range(-700, 700), -300);
            }
            else
            {
                enemy.position = new Vector2(Random.Range(-700, 700), 300);
            }
        }
        enemies.Add(enemy);

        // Add enemy component
        GameObject enemy_object = new GameObject();
        enemy_object.transform.parent = transform;
        enemy_object.AddComponent<CanvasRenderer>();
        enemy_object.AddComponent<RectTransform>();
        Image image = enemy_object.AddComponent<Image>();
        image.color = Color.red;
        EnemyPositionUpdate pos = enemy_object.AddComponent<EnemyPositionUpdate>();
        pos.enemy = enemy;
    }

    void AddProjection(Vector2 position, Vector2 direction)
    {
        Projection proj = new Projection();
        proj.position = position;
        proj.direction = direction.normalized;
        projections.Add(proj);

        // Add enemy component
        GameObject proj_object = new GameObject();
        proj_object.transform.parent = transform;
        proj_object.AddComponent<CanvasRenderer>();
        proj_object.AddComponent<RectTransform>().sizeDelta = new Vector2(20, 20);
        Image image = proj_object.AddComponent<Image>();
        image.color = Color.yellow;
        ProjPositionUpdate pos = proj_object.AddComponent<ProjPositionUpdate>();
        pos.proj = proj;
    }

    // Update is called once per frame
    void Update()
    {
        float delta = Time.deltaTime;
        // Player move
        Vector2 moveValue = moveAction.ReadValue<Vector2>() * delta * speed;
        player_position += moveValue;
        // Enemy move
        foreach (Enemy enemy in enemies)
        {
            enemy.position += (player_position - enemy.position).normalized * delta * enemy.speed;
        }
        // Proj move
        foreach (Projection proj in projections)
        {
            proj.position += proj.direction * delta * proj.speed;
            if (proj.position.magnitude > 3000)
            {
                proj.is_alive = false;
            }
        }
        // Enemy Attack
        foreach (Enemy enemy in enemies)
        {
            if (enemy.attack_cooldown > 0)
            {
                enemy.attack_cooldown -= delta;
            }
            else if ((player_position - enemy.position).magnitude < enemy.attack_range)
            {
                life -= enemy.attack_value;
                enemy.attack_cooldown = enemy.attack_cooldown_when_attacked;
            }
        }
        // Player attack
        if (attack_cooldown > 0)
        {
            attack_cooldown -= delta;
        }
        else if (enemies.Count > 0)
        {
            Vector2 to_attack = enemies.First().position;
            foreach (Enemy enemy in enemies)
            {
                if ((to_attack - player_position).magnitude > (enemy.position - player_position).magnitude)
                {
                    to_attack = enemy.position;
                }
            }
            AddProjection(player_position, to_attack - player_position);
            attack_cooldown = attack_cooldown_when_attacked;
        }
        // Proj Attack
        foreach (Projection proj in projections)
        {
            if (proj.is_player_projected)
            {
                foreach (Enemy enemy in enemies)
                {
                    if ((proj.position - enemy.position).magnitude < proj.range)
                    {
                        enemy.life -= proj.attack_value;
                        proj.is_alive = false;
                    }
                    if (!proj.is_alive) { break; }
                }
            }
            else
            {
                if ((proj.position - player_position).magnitude < proj.range)
                {
                    life -= proj.attack_value;
                    proj.is_alive = false;
                }
            }
        }
        // Clean up dead objects
        enemies.RemoveAll(enemy => !enemy.is_alive);
        projections.RemoveAll(proj => !proj.is_alive);
        // Add enemy
        if (enemy_spawn_cooldown > 0)
        {
            enemy_spawn_cooldown -= delta;
        }
        else
        {
            AddEnemy();
            enemy_spawn_cooldown = 5;
        }
    }
}
