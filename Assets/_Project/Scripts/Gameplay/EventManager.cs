using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DemHoiDenLong.Gameplay
{
    public enum SpecialEventType
    {
        MeteorShower,
        MoonRabbitsBlessing,
        GoldenLanterns,
        FoggyNight,
        GustOfWind
    }

    public class EventManager : MonoBehaviour
    {
        public static EventManager Instance { get; private set; }

        [Header("Event Probabilities")]
        [Tooltip("Percentage chance an event happens when a wave completes")]
        [SerializeField] private float eventTriggerChance = 10f; 

        private HashSet<SpecialEventType> triggeredEventsInLevel = new HashSet<SpecialEventType>();
        private bool isEventActive = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnWaveCompleted += HandleWaveCompleted;
                WaveManager.Instance.OnLevelCompleted += ResetLevelEvents;
            }
        }

        private void OnDestroy()
        {
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnWaveCompleted -= HandleWaveCompleted;
                WaveManager.Instance.OnLevelCompleted -= ResetLevelEvents;
            }
        }

        private void ResetLevelEvents()
        {
            triggeredEventsInLevel.Clear();
            isEventActive = false;
        }

        private void HandleWaveCompleted(int waveIndex)
        {
            if (isEventActive) return;

            if (Random.Range(0f, 100f) <= eventTriggerChance)
            {
                TriggerRandomEvent();
            }
        }

        private void TriggerRandomEvent()
        {
            List<SpecialEventType> availableEvents = new List<SpecialEventType>();
            List<int> weights = new List<int>();

            AddEventIfAvailable(availableEvents, weights, SpecialEventType.MeteorShower, 30);
            AddEventIfAvailable(availableEvents, weights, SpecialEventType.MoonRabbitsBlessing, 30);
            AddEventIfAvailable(availableEvents, weights, SpecialEventType.GoldenLanterns, 25);
            AddEventIfAvailable(availableEvents, weights, SpecialEventType.FoggyNight, 10);
            AddEventIfAvailable(availableEvents, weights, SpecialEventType.GustOfWind, 5);

            if (availableEvents.Count == 0) return;

            int totalWeight = 0;
            foreach (var w in weights) totalWeight += w;

            int randomVal = Random.Range(0, totalWeight);
            int currentWeight = 0;
            SpecialEventType chosenEvent = availableEvents[0];

            for (int i = 0; i < availableEvents.Count; i++)
            {
                currentWeight += weights[i];
                if (randomVal < currentWeight)
                {
                    chosenEvent = availableEvents[i];
                    break;
                }
            }

            triggeredEventsInLevel.Add(chosenEvent);
            ExecuteEvent(chosenEvent);
        }

        private void AddEventIfAvailable(List<SpecialEventType> availableEvents, List<int> weights, SpecialEventType type, int weight)
        {
            if (!triggeredEventsInLevel.Contains(type))
            {
                availableEvents.Add(type);
                weights.Add(weight);
            }
        }

        private void ExecuteEvent(SpecialEventType type)
        {
            Debug.Log($"<color=cyan>SPECIAL EVENT TRIGGERED: {type}</color>");
            switch (type)
            {
                case SpecialEventType.MeteorShower:
                    StartCoroutine(MeteorShowerRoutine());
                    break;
                case SpecialEventType.MoonRabbitsBlessing:
                    StartCoroutine(MoonRabbitsBlessingRoutine());
                    break;
                case SpecialEventType.GoldenLanterns:
                    StartCoroutine(GoldenLanternsRoutine());
                    break;
                case SpecialEventType.FoggyNight:
                    StartCoroutine(FoggyNightRoutine());
                    break;
                case SpecialEventType.GustOfWind:
                    ExecuteGustOfWind();
                    break;
            }
        }

        private IEnumerator MeteorShowerRoutine()
        {
            isEventActive = true;
            for (int i = 0; i < 15; i++)
            {
                float randomX = Random.Range(-4f, 4f);
                float y = Camera.main != null ? Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0)).y + 1f : 10f;
                PowerUpManager.Instance?.ForceDropStar(new Vector3(randomX, y, 0), 2);
                yield return new WaitForSeconds(0.3f);
            }
            isEventActive = false;
        }

        private IEnumerator MoonRabbitsBlessingRoutine()
        {
            isEventActive = true;
            for (int i = 0; i < 3; i++)
            {
                float randomX = Random.Range(-3f, 3f);
                float y = Camera.main != null ? Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0)).y + 1f : 10f;
                PowerUpManager.Instance?.ForceDropRandomPowerUp(new Vector3(randomX, y, 0));
                yield return new WaitForSeconds(0.5f);
            }
            isEventActive = false;
        }

        private IEnumerator GoldenLanternsRoutine()
        {
            isEventActive = true;
            for (int i = 0; i < 10; i++)
            {
                float randomX = Random.Range(-4f, 4f);
                float y = Camera.main != null ? Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0)).y + 1f : 10f;
                PowerUpManager.Instance?.ForceDropStar(new Vector3(randomX, y, 0), 5);
                yield return new WaitForSeconds(0.2f);
            }
            isEventActive = false;
        }

        private IEnumerator FoggyNightRoutine()
        {
            isEventActive = true;
            if (EnemySpawner.Instance != null)
            {
                EnemySpawner.Instance.ApplyTemporarySpeedBuff(1.5f, 10f); // 50% faster for 10 seconds
            }
            yield return new WaitForSeconds(10f);
            isEventActive = false;
        }

        private void ExecuteGustOfWind()
        {
            if (EnemySpawner.Instance != null)
            {
                EnemySpawner.Instance.KillAllActiveEnemies();
            }
            // Can add a screen flash or shake here
            if (VFX.CameraShake.Instance != null) VFX.CameraShake.Instance.Shake(0.5f, 0.5f);
        }
    }
}
