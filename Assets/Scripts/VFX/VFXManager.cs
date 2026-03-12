using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RobotTD.VFX
{
    public enum VFXType
    {
        // Tower muzzle
        LaserBeam,
        PlasmaMuzzle,
        RocketLaunch,
        FreezeShot,
        ShockArc,
        SniperTracer,
        FlameSpray,
        TeslaBolt,

        // Impacts / explosions
        BulletImpact,
        RocketExplosion,
        FreezeImpact,
        PlasmaImpact,
        ShockImpact,

        // Enemy
        EnemyDeath,
        EnemyBossDeath,
        EnemyBurnDOT,
        EnemyFreezeAura,
        EnemyShieldHit,
        EnemySpeedUp,   // Healer buff

        // Environment / UI
        TowerPlaceFlash,
        TowerUpgradeFlash,
        WaveStartPulse,
        VictoryBurst,
        CreditPickup,
    }

    [System.Serializable]
    public class VFXEntry
    {
        public VFXType type;
        public GameObject prefab;
        public float autoReturnTime = 2f;
        public int poolSize = 10;
    }

    /// <summary>
    /// Central VFX manager.  All particle / visual effects go through here.
    /// Uses object pooling to stay mobile-friendly.
    /// </summary>
    public class VFXManager : MonoBehaviour
    {
        public static VFXManager Instance { get; private set; }

        [Header("VFX Catalogue")]
        [SerializeField] private VFXEntry[] entries;

        // pool: type -> queue of inactive instances
        private Dictionary<VFXType, Queue<GameObject>> pools
            = new Dictionary<VFXType, Queue<GameObject>>();
        private Dictionary<VFXType, VFXEntry> entryMap
            = new Dictionary<VFXType, VFXEntry>();

        private Transform poolRoot;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            poolRoot = new GameObject("VFX_Pool").transform;
            poolRoot.SetParent(transform);

            foreach (var e in entries)
            {
                entryMap[e.type] = e;
                pools[e.type] = new Queue<GameObject>();

                if (e.prefab != null)
                    PrewarmPool(e);
            }
        }

        private void PrewarmPool(VFXEntry entry)
        {
            for (int i = 0; i < entry.poolSize; i++)
            {
                GameObject go = Instantiate(entry.prefab, poolRoot);
                go.SetActive(false);
                pools[entry.type].Enqueue(go);
            }
        }

        // ── Public API ───────────────────────────────────────────────────────

        /// <summary>Play a VFX at world position with optional rotation.</summary>
        public GameObject Play(VFXType type, Vector3 position, Quaternion? rotation = null)
        {
            if (!entryMap.TryGetValue(type, out VFXEntry entry)) return null;

            GameObject go = GetFromPool(entry);
            go.transform.position = position;
            go.transform.rotation = rotation ?? Quaternion.identity;
            go.SetActive(true);

            // Restart all particle systems
            var particles = go.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particles) ps.Play();

            StartCoroutine(ReturnAfterDelay(go, type, entry.autoReturnTime));
            return go;
        }

        /// <summary>Play and attach VFX to a parent transform.</summary>
        public GameObject PlayAttached(VFXType type, Transform parent, Vector3 localOffset = default)
        {
            if (!entryMap.TryGetValue(type, out VFXEntry entry)) return null;

            GameObject go = GetFromPool(entry);
            go.transform.SetParent(parent);
            go.transform.localPosition = localOffset;
            go.transform.localRotation = Quaternion.identity;
            go.SetActive(true);

            var particles = go.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particles) ps.Play();

            StartCoroutine(ReturnAttachedAfterDelay(go, type, parent, entry.autoReturnTime));
            return go;
        }

        /// <summary>Stop and immediately return a VFX to pool.</summary>
        public void Stop(GameObject go, VFXType type)
        {
            if (go == null) return;
            ReturnToPool(go, type);
        }

        // ── Pool helpers ─────────────────────────────────────────────────────

        private GameObject GetFromPool(VFXEntry entry)
        {
            if (pools[entry.type].Count > 0)
            {
                var pooled = pools[entry.type].Dequeue();
                if (pooled != null) return pooled;
            }

            // Pool exhausted — spawn new
            if (entry.prefab == null) return new GameObject($"VFX_{entry.type}_Empty");
            return Instantiate(entry.prefab);
        }

        private void ReturnToPool(GameObject go, VFXType type)
        {
            var particles = go.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particles) ps.Stop();

            go.transform.SetParent(poolRoot);
            go.SetActive(false);
            pools[type].Enqueue(go);
        }

        private IEnumerator ReturnAfterDelay(GameObject go, VFXType type, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (go != null) ReturnToPool(go, type);
        }

        private IEnumerator ReturnAttachedAfterDelay(GameObject go, VFXType type, Transform originalParent, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (go != null) ReturnToPool(go, type);
        }
    }

    // ── Convenience component — attach to towers/enemies ────────────────────

    /// <summary>
    /// Attach to a tower to automatically play VFX on fire/upgrade/place.
    /// </summary>
    public class TowerVFX : MonoBehaviour
    {
        [Header("VFX Types")]
        [SerializeField] private VFXType muzzleEffect;
        [SerializeField] private VFXType upgradeEffect = VFXType.TowerUpgradeFlash;
        [SerializeField] private VFXType placeEffect = VFXType.TowerPlaceFlash;

        [Header("Muzzle Point")]
        [SerializeField] private Transform muzzlePoint;

        private bool hasInit;

        private void Start()
        {
            // Play place effect on spawn
            VFXManager.Instance?.Play(placeEffect, transform.position);
            hasInit = true;
        }

        public void PlayMuzzle()
        {
            if (VFXManager.Instance == null) return;
            Vector3 pos = muzzlePoint != null ? muzzlePoint.position : transform.position;
            VFXManager.Instance.Play(muzzleEffect, pos, transform.rotation);
        }

        public void PlayUpgrade()
        {
            VFXManager.Instance?.Play(upgradeEffect, transform.position);
        }
    }

    /// <summary>
    /// Attach to an enemy to handle status-effect VFX (burn, freeze, stun aura).
    /// </summary>
    public class EnemyVFX : MonoBehaviour
    {
        private GameObject burnEffect;
        private GameObject freezeEffect;
        private bool burnActive;
        private bool freezeActive;

        private VFXType burnType = VFXType.EnemyBurnDOT;
        private VFXType freezeType = VFXType.EnemyFreezeAura;

        public void SetBurning(bool active)
        {
            if (active == burnActive) return;
            burnActive = active;

            if (active)
                burnEffect = VFXManager.Instance?.PlayAttached(burnType, transform);
            else if (burnEffect != null)
                VFXManager.Instance?.Stop(burnEffect, burnType);
        }

        public void SetFrozen(bool active)
        {
            if (active == freezeActive) return;
            freezeActive = active;

            if (active)
                freezeEffect = VFXManager.Instance?.PlayAttached(freezeType, transform);
            else if (freezeEffect != null)
                VFXManager.Instance?.Stop(freezeEffect, freezeType);
        }

        public void PlayDeathEffect(bool isBoss = false)
        {
            VFXType deathType = isBoss ? VFXType.EnemyBossDeath : VFXType.EnemyDeath;
            VFXManager.Instance?.Play(deathType, transform.position);
        }

        public void PlayShieldHit()
        {
            VFXManager.Instance?.Play(VFXType.EnemyShieldHit, transform.position);
        }
    }
}
