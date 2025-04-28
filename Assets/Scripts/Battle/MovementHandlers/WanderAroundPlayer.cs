using System.Collections.Generic;
using UnityEngine;

public static class MovementHandler2D
{
    public class MovementState
    {
        public float startTime = Time.time;
        public Vector2 targetPos;
        public int currentWaypointIndex = 0;
        public float nextDecisionTime = 0f;
        public Vector2 currentVelocity = Vector2.zero;
        public bool isDiving = true;
        public float startY;
        public Vector2 climbTarget;

        // Orbit Fancy
        public float orbitBaseRadiusX = 1f;
        public float orbitBaseRadiusY = 0.5f;
        public float orbitWobbleAmplitude = 0.1f;
        public float orbitWobbleFrequency = 2f;
        public bool orbitEnableFigure8 = false;

        public AnimationCurve orbitShapeBlend = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public float orbitShapeCycleTime = 5f;
        public string currentMode = "";
        public float modeSwitchTime = -1f;
    }

    public static void SwitchMode(MovementState state, string newMode)
    {
        if (state.currentMode != newMode)
        {
            state.currentMode = newMode;
            state.modeSwitchTime = Time.time;
            state.startTime = Time.time; // 同时重置轨迹时间
        }
    }

    public static Vector2 EaseIntoTrajectory2D(Vector2 currentPos, Vector2 targetPos, MovementState state, float duration)
    {
        float t = Mathf.Clamp01((Time.time - state.startTime) / duration);
        return Vector2.Lerp(currentPos, targetPos, t);
    }

    public static Vector2 OrbitFancy2D(
        Vector2 currentPos,
        Vector2 center,
        MovementState state,
        float speed,
        float easeDuration = 0.5f)
    {
        float rawTime = Time.time - state.startTime;

        float easedSpeed = Mathf.Lerp(speed * 0.95f, speed * 1.05f, Mathf.Sin(rawTime * 0.3f) * 0.5f + 0.5f);

        float t = rawTime * easedSpeed;

        float radiusX = state.orbitBaseRadiusX + Mathf.Sin(t * state.orbitWobbleFrequency) * state.orbitWobbleAmplitude;
        float radiusY = state.orbitBaseRadiusY + Mathf.Cos(t * state.orbitWobbleFrequency * 1.5f) * state.orbitWobbleAmplitude * 0.5f;

        float shapeBlend = state.orbitShapeBlend.Evaluate((rawTime % state.orbitShapeCycleTime) / state.orbitShapeCycleTime);
        float ySin = Mathf.Lerp(Mathf.Sin(t), Mathf.Sin(2 * t), shapeBlend);

        float x = Mathf.Cos(t) * radiusX;
        float y = ySin * radiusY;

        Vector2 orbitPoint = center + new Vector2(x, y);

        float easeT = Mathf.Clamp01((Time.time - state.modeSwitchTime) / easeDuration);
        return Vector2.Lerp(currentPos, orbitPoint, easeT);
    }

    public static Vector2 OrbitEaseIn2D(Vector2 currentPos, Vector2 center, MovementState state, float radiusX, float radiusY, float speed, float easeDuration = 0.5f)
    {
        float t = (Time.time - state.startTime) * speed;
        Vector2 orbitPoint = center + new Vector2(Mathf.Cos(t) * radiusX, Mathf.Sin(t) * radiusY);

        float easeT = Mathf.Clamp01((Time.time - state.startTime) / easeDuration);
        return Vector2.Lerp(currentPos, orbitPoint, easeT);
    }

    public static Vector2 Orbit2D(Vector2 center, MovementState state, float radiusX, float radiusY, float speed)
    {
        float t = (Time.time - state.startTime) * speed;
        float x = Mathf.Cos(t) * radiusX;
        float y = Mathf.Sin(t) * radiusY;
        return center + new Vector2(x, y);
    }

    public static Vector2 Patrol2D(Vector2 currentPos, MovementState state, List<Vector2> waypoints, float speed, float threshold = 0.2f)
    {
        if (waypoints == null || waypoints.Count == 0) return currentPos;

        Vector2 target = waypoints[state.currentWaypointIndex];
        if (Vector2.Distance(currentPos, target) < threshold)
        {
            state.currentWaypointIndex = (state.currentWaypointIndex + 1) % waypoints.Count;
        }

        return Vector2.MoveTowards(currentPos, waypoints[state.currentWaypointIndex], speed * Time.deltaTime);
    }

    public static Vector2 TrackTarget2D(Vector2 currentPos, Vector2 targetPos, MovementState state, float maxSpeed, float acceleration)
    {
        Vector2 dir = (targetPos - currentPos).normalized;
        state.currentVelocity = Vector2.MoveTowards(state.currentVelocity, dir * maxSpeed, acceleration * Time.deltaTime);
        return currentPos + state.currentVelocity * Time.deltaTime;
    }

    public static Vector2 DiveAndRise2D(Vector2 currentPos, MovementState state, Vector2 target, float diveSpeed, float riseSpeed, float minY)
    {
        if (state.isDiving)
        {
            Vector2 dir = (target - currentPos).normalized;
            Vector2 next = currentPos + dir * diveSpeed * Time.deltaTime;

            if (next.y <= minY)
            {
                state.isDiving = false;
                state.climbTarget = new Vector2(next.x, state.startY);
            }

            return next;
        }
        else
        {
            Vector2 next = Vector2.MoveTowards(currentPos, state.climbTarget, riseSpeed * Time.deltaTime);
            return next;
        }
    }

    public static Vector2 WanderNearPlayer2D(Vector2 petPos, Vector2 playerPos, MovementState state, float maxDistance, float decisionInterval)
    {
        if (Time.time >= state.nextDecisionTime)
        {
            float offsetX = Random.Range(-maxDistance, maxDistance);
            float offsetY = Random.Range(-1f, 1f);
            state.targetPos = playerPos + new Vector2(offsetX, offsetY);
            state.nextDecisionTime = Time.time + decisionInterval;
        }

        return Vector2.MoveTowards(petPos, state.targetPos, 1.5f * Time.deltaTime);
    }

    public static Vector2 Float2D(Vector2 basePos, MovementState state, float amplitude, float frequency)
    {
        float offsetY = Mathf.Sin((Time.time - state.startTime) * frequency) * amplitude;
        return new Vector2(basePos.x, basePos.y + offsetY);
    }

    public static Vector2 Serpent2D(Vector2 currentPos, MovementState state, Vector2 direction, float speed, float waveSize, float waveFrequency)
    {
        float offsetY = Mathf.Sin((Time.time - state.startTime) * waveFrequency) * waveSize;
        Vector2 dir = direction.normalized;
        Vector2 normal = new Vector2(-dir.y, dir.x);
        return currentPos + (dir * speed + normal * offsetY) * Time.deltaTime;
    }
}
