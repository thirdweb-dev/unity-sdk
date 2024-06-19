using System.Collections;
using System.Collections.Generic;
using Thirdweb.Redcode.Awaiting;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Thirdweb.Unity.Examples
{
    public class GameResult
    {
        public int score;
        public int maxCombo;
    }

    public class MusicGameManager : MonoBehaviour
    {
        [field: SerializeField, Header("UI")]
        private TMP_Text scoreText;

        [field: SerializeField]
        private TMP_Text comboText;

        [field: SerializeField]
        private TMP_Text maxComboText;

        [field: SerializeField, Header("EVENTS")]
        private UnityEvent<GameResult> OnGameEnded;

        [field: SerializeField, Header("MUSIC SETTINGS")]
        private AudioSource audioSource;

        [field: SerializeField, Header("GAME SETTINGS")]
        private Tile tilePrefab;

        [field: SerializeField]
        private Transform[] tileSpawnPoints; // Array of spawn points for three lanes

        [field: SerializeField]
        private Transform[] hitAreas; // Array of hit areas for left, center, and right

        [field: SerializeField]
        private float tileFallSpeed = 5f;

        [field: SerializeField]
        private GameObject hitEffectPrefab;

        internal int Score { get; set; }

        private float[] spectrumData = new float[64];
        private bool isGameRunning;
        private bool isGameEnding;
        private float peakThreshold;
        private float fallTime;
        private float lastSpawnTime;
        private float minSpawnInterval = 0.3f; // Minimum interval between spawns to prevent overlap
        private Queue<float> spawnQueue = new Queue<float>();
        private int combo = 0;
        private int maxCombo = 0;

        internal static MusicGameManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        public void StartGame()
        {
            Score = 0;
            combo = 0;
            maxCombo = 0;

            scoreText.text = "Score: 0";
            comboText.text = "Combo: 0";
            maxComboText.text = "Max Combo: 0";

            isGameRunning = true;
            isGameEnding = false;
            StartMusic(Song._selectedSong.Clip);
            CalculateFallTime();
            StartCoroutine(CalibrateThreshold());
        }

        private void StartMusic(AudioClip song)
        {
            audioSource.clip = song;
            audioSource.loop = false;
            audioSource.Play();
        }

        private void CalculateFallTime()
        {
            // Calculate the fall time based on the distance between spawn point and hit area, and the fall speed
            float distance = Vector3.Distance(tileSpawnPoints[0].position, hitAreas[0].position);
            fallTime = distance / tileFallSpeed;
        }

        private IEnumerator CalibrateThreshold()
        {
            // Analyze initial spectrum data to set a reasonable peak threshold
            float sum = 0f;
            int samples = 100;
            for (int i = 0; i < samples; i++)
            {
                audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);
                sum += GetAverageIntensity(spectrumData);
                yield return new WaitForSeconds(0.05f);
            }
            peakThreshold = (sum / samples) * 1.5f; // Set threshold to 1.5 times the average initial intensity
            StartCoroutine(AnalyzeMusic());
            StartCoroutine(ProcessSpawnQueue());
        }

        private IEnumerator AnalyzeMusic()
        {
            float previousIntensity = 0f;

            while (isGameRunning && !isGameEnding)
            {
                audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);
                float currentIntensity = GetAverageIntensity(spectrumData);

                // Detect peaks based on a significant increase in intensity
                if (currentIntensity > previousIntensity * 1.1f && currentIntensity > peakThreshold)
                {
                    if (Time.time - lastSpawnTime >= minSpawnInterval)
                    {
                        EnqueueTileSpawn();
                        lastSpawnTime = Time.time;
                    }
                }

                previousIntensity = currentIntensity;

                yield return new WaitForSeconds(0.05f); // Check more frequently for better synchronization
            }
        }

        private IEnumerator ProcessSpawnQueue()
        {
            while (isGameRunning && !isGameEnding)
            {
                if (spawnQueue.Count > 0)
                {
                    float delay = spawnQueue.Dequeue();
                    yield return new WaitForSeconds(delay);
                    SpawnTile();
                }
                else
                {
                    yield return null;
                }
            }
        }

        private void EnqueueTileSpawn()
        {
            // Add a delay to the spawn queue to ensure proper intervals between tile spawns
            float delay = Random.Range(0f, minSpawnInterval);
            spawnQueue.Enqueue(delay);
        }

        private float GetAverageIntensity(float[] data)
        {
            float sum = 0f;
            for (int i = 0; i < data.Length; i++)
            {
                sum += data[i];
            }
            return sum / data.Length;
        }

        private void SpawnTile()
        {
            // Randomly select a spawn point from the array
            Transform selectedSpawnPoint = tileSpawnPoints[Random.Range(0, tileSpawnPoints.Length)];
            Tile tile = Instantiate(tilePrefab, selectedSpawnPoint.position, Quaternion.identity);

            // Set the fall speed and initialize the tile
            tile.Initialize(tileFallSpeed);
        }

        private void Update()
        {
            if (!isGameRunning && !isGameEnding)
                return;

            if (!isGameEnding && !audioSource.isPlaying)
            {
                EndGame();
            }

            // Input handling for A, S, and D keys
            if (Input.GetKeyDown(KeyCode.A))
            {
                HandleInput(0);
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                HandleInput(1);
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                HandleInput(2);
            }
        }

        private void LateUpdate()
        {
            if (!isGameRunning && !isGameEnding)
                return;

            scoreText.text = $"Score: {Score}";
            comboText.text = $"Combo: {combo}";
            maxComboText.text = $"Max Combo: {maxCombo}";
        }

        private void HandleInput(int hitAreaIndex)
        {
            Collider2D hitAreaCollider = hitAreas[hitAreaIndex].GetComponent<Collider2D>();

            if (hitAreaCollider != null)
            {
                Collider2D[] colliders = Physics2D.OverlapBoxAll(hitAreaCollider.bounds.center, hitAreaCollider.bounds.size, 0f);
                Collider2D lowestTile = null;

                foreach (Collider2D collider in colliders)
                {
                    if (collider.CompareTag("Tile"))
                    {
                        if (lowestTile == null || collider.transform.position.y < lowestTile.transform.position.y)
                        {
                            lowestTile = collider;
                        }
                    }
                }

                if (lowestTile != null)
                {
                    Destroy(lowestTile.gameObject); // Destroy the lowest tile on hit
                    Score += 100; // Increase score
                    combo++; // Increase combo
                    maxCombo = Mathf.Max(maxCombo, combo); // Track max combo
                    StartCoroutine(ShowHitEffect(hitAreaCollider.bounds.center)); // Show hit effect
                }
                else
                {
                    combo = 0; // Reset combo on miss
                    Score -= 25; // Decrease score on miss
                    CameraShake.Instance.Shake(0.1f, 0.1f); // Shake the camera on miss
                }
            }
        }

        private IEnumerator ShowHitEffect(Vector3 position)
        {
            GameObject hitEffect = Instantiate(hitEffectPrefab, position, Quaternion.identity);
            yield return new WaitForSeconds(0.5f);
            Destroy(hitEffect);
        }

        private async void EndGame()
        {
            isGameEnding = true;
            audioSource.Stop();
            await new WaitForSeconds(5f);
            isGameRunning = false;
            isGameEnding = false;
            OnGameEnded.Invoke(new GameResult { score = Score, maxCombo = maxCombo });
        }
    }
}
