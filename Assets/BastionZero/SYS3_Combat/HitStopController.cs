using System.Collections.Generic;
using BastionZero.Core;
using BastionZero.Damage;
using UnityEngine;

namespace BastionZero.Combat
{
    /// <summary>
    /// SYS3 §6 rules 1–2 and §8: hit-stop is local (attacker and target only); only crits,
    /// kills, guard breaks and blocked heavies reach beyond the two actors. Global stops are
    /// driven through <see cref="Time.timeScale"/>, and every timer here runs on unscaled time
    /// so a stop inside a slow-down cannot stall.
    ///
    /// The death sequence (SYS2 §9 D-1) also freezes the world with Time.timeScale. This
    /// controller cancels itself the moment the hero dies, so the two never fight over it.
    /// </summary>
    public class HitStopController : MonoBehaviour
    {
        private sealed class FrozenActor
        {
            public Animator Animator;
            public float RestoreSpeed;
            public float Timer;
        }

        private readonly List<FrozenActor> frozen = new List<FrozenActor>();
        private float localTimer;
        private float globalFreezeTimer;
        private float slowTimer;
        private float slowScale = 1f;
        private bool ownsTimeScale;
        private HeroHealth heroHealth;

        /// <summary>The frame count of the local stop that is currently running.</summary>
        public int LocalStopFrames { get; private set; }

        /// <summary>True while the world is frozen or slowed by a SYS3 hit.</summary>
        public bool IsGlobalStopped => globalFreezeTimer > 0f || slowTimer > 0f;

        /// <summary>True while at least one actor is held by a local stop.</summary>
        public bool IsLocalStopped => localTimer > 0f;

        private void Awake()
        {
            heroHealth = GetComponentInParent<HeroHealth>();
            if (heroHealth == null)
            {
                heroHealth = FindAnyObjectByType<HeroHealth>(FindObjectsInactive.Include);
            }

            if (heroHealth != null) heroHealth.Died += OnHeroDied;
        }

        private void OnDestroy()
        {
            if (heroHealth != null) heroHealth.Died -= OnHeroDied;
            CancelAll();
        }

        private void OnDisable()
        {
            CancelAll();
        }

        private void OnHeroDied(DamageInfo info)
        {
            // Died fires before D-1 takes the world, so releasing here hands a clean time scale
            // to the death routine.
            CancelAll();
        }

        /// <summary>§8: freezes the attacker's and the target's animators for a hit's frames.</summary>
        public void PlayLocal(int frames, GameObject attacker, GameObject target)
        {
            PlayLocal(frames, frames, attacker, target);
        }

        /// <summary>
        /// §8: the attacker and the target hold for their own frame counts, which is what the
        /// feel table asks for once the H7 kill bonus is folded into one row.
        /// </summary>
        public void PlayLocal(int attackerFrames, int targetFrames, GameObject attacker, GameObject target)
        {
            if (attackerFrames <= 0 && targetFrames <= 0) return;

            float attackerSeconds = Mathf.Max(0, attackerFrames) / (float)SpecUnits.FramesPerSecond;
            float targetSeconds = Mathf.Max(0, targetFrames) / (float)SpecUnits.FramesPerSecond;

            Capture(attacker, attackerSeconds);
            Capture(target, targetSeconds);

            LocalStopFrames = Mathf.Max(LocalStopFrames, Mathf.Max(attackerFrames, targetFrames));
            localTimer = Mathf.Max(localTimer, Mathf.Max(attackerSeconds, targetSeconds));
        }

        /// <summary>
        /// §8 H3/H19: the world freezes for the crit frames, then holds the crit slow-down,
        /// then returns to normal speed.
        /// </summary>
        public void PlayGlobal(int freezeFrames, float holdSeconds, float holdTimeScale)
        {
            if (heroHealth != null && heroHealth.IsDead) return;

            // Never steal a freeze the death routine already owns.
            if (!ownsTimeScale && Time.timeScale <= 0.0001f) return;

            if (freezeFrames > 0)
            {
                globalFreezeTimer = Mathf.Max(globalFreezeTimer, freezeFrames / (float)SpecUnits.FramesPerSecond);
            }

            if (holdSeconds > 0f)
            {
                slowTimer = Mathf.Max(slowTimer, holdSeconds);
                slowScale = Mathf.Clamp(holdTimeScale, 0.01f, 1f);
            }

            if (globalFreezeTimer > 0f)
            {
                ownsTimeScale = true;
                Time.timeScale = 0f;
            }
            else if (slowTimer > 0f)
            {
                ownsTimeScale = true;
                Time.timeScale = slowScale;
            }
        }

        /// <summary>Restores every animator and the time scale. Safe to call at any time.</summary>
        public void CancelAll()
        {
            localTimer = 0f;
            LocalStopFrames = 0;
            ReleaseActors();

            globalFreezeTimer = 0f;
            slowTimer = 0f;

            if (ownsTimeScale)
            {
                Time.timeScale = 1f;
                ownsTimeScale = false;
            }
        }

        private void Capture(GameObject actor, float seconds)
        {
            if (actor == null || seconds <= 0f) return;

            Animator[] animators = actor.GetComponentsInChildren<Animator>(true);
            for (int i = 0; i < animators.Length; i++)
            {
                Animator animator = animators[i];
                if (animator == null) continue;

                // An actor already held by an earlier stop is lengthened, never captured twice,
                // otherwise its RestoreSpeed would record the 0 the last hit left behind.
                FrozenActor existing = FindFrozen(animator);
                if (existing != null)
                {
                    existing.Timer = Mathf.Max(existing.Timer, seconds);
                    continue;
                }

                frozen.Add(new FrozenActor
                {
                    Animator = animator,
                    RestoreSpeed = animator.speed,
                    Timer = seconds,
                });

                animator.speed = 0f;
            }
        }

        private FrozenActor FindFrozen(Animator animator)
        {
            for (int i = 0; i < frozen.Count; i++)
            {
                if (frozen[i].Animator == animator) return frozen[i];
            }

            return null;
        }

        /// <summary>Releases each actor on its own clock, so uneven stops end unevenly.</summary>
        private void TickActors(float unscaledDelta)
        {
            for (int i = frozen.Count - 1; i >= 0; i--)
            {
                FrozenActor actor = frozen[i];
                actor.Timer -= unscaledDelta;
                if (actor.Timer > 0f) continue;

                if (actor.Animator != null) actor.Animator.speed = actor.RestoreSpeed;
                frozen.RemoveAt(i);
            }
        }

        private void Update()
        {
            float unscaled = Time.unscaledDeltaTime;

            if (frozen.Count > 0) TickActors(unscaled);

            if (localTimer > 0f)
            {
                localTimer -= unscaled;
                if (localTimer <= 0f)
                {
                    localTimer = 0f;
                    LocalStopFrames = 0;
                    ReleaseActors();
                }
            }

            if (globalFreezeTimer > 0f)
            {
                globalFreezeTimer -= unscaled;
                if (globalFreezeTimer <= 0f)
                {
                    globalFreezeTimer = 0f;

                    if (slowTimer > 0f)
                    {
                        Time.timeScale = slowScale;
                    }
                    else if (ownsTimeScale)
                    {
                        Time.timeScale = 1f;
                        ownsTimeScale = false;
                    }
                }

                return;
            }

            if (slowTimer > 0f)
            {
                slowTimer -= unscaled;
                if (slowTimer <= 0f)
                {
                    slowTimer = 0f;
                    if (ownsTimeScale)
                    {
                        Time.timeScale = 1f;
                        ownsTimeScale = false;
                    }
                }
            }
        }

        private void ReleaseActors()
        {
            for (int i = 0; i < frozen.Count; i++)
            {
                FrozenActor actor = frozen[i];
                if (actor.Animator != null) actor.Animator.speed = actor.RestoreSpeed;
            }

            frozen.Clear();
        }
    }
}
