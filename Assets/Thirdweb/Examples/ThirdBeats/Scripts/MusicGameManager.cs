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
        public bool cancelled;
    }

    public class MusicGameManager : MonoBehaviour
    {
        [field: SerializeField, Header("UI")]
        private TMP_Text scoreText;

        [field: SerializeField]
        private TMP_Text comboText;

        [field: SerializeField]
        private TMP_Text maxComboText;

        [field: SerializeField, Header("MUSIC SETTINGS")]
        private AudioSource audioSource;

        [field: SerializeField, Header("GAME SETTINGS")]
        private Tile tilePrefab;

        [field: SerializeField]
        private Transform[] tileSpawnPoints; // Array of spawn points for three lanes

        [field: SerializeField]
        private Transform[] hitAreas; // Array of hit areas for left, center, and right

        [field: SerializeField]
        private float baseTileFallSpeed = 5f; // Base fall speed for tiles

        [field: SerializeField]
        private GameObject hitEffectPrefab;

        [field: SerializeField, Header("EVENTS")]
        internal UnityEvent<GameResult> OnGameEnded;

        private int _score;
        internal int Score
        {
            get => _score;
            set
            {
                _score = value;
                if (_score < 0)
                {
                    _score = 0;
                }
            }
        }

        private float[] spectrumData = new float[64];
        private bool isGameRunning;
        private bool isGameEnding;
        private float peakThreshold;
        private float fallTime;
        private float lastSpawnTime;
        private float minSpawnInterval = 0.5f; // Adjusted minimum interval between spawns
        private Queue<float> spawnQueue = new Queue<float>();
        private int combo = 0;
        private int maxCombo = 0;
        private float musicStartTime;
        private bool firstTileSpawned = false;

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
            CalculateFallTime(baseTileFallSpeed);
            StartCoroutine(CalibrateThreshold());
        }

        private void StartMusic(AudioClip song)
        {
            audioSource.clip = song;
            audioSource.loop = false;
            audioSource.Play();
            musicStartTime = Time.time;
        }

        private void CalculateFallTime(float tileFallSpeed)
        {
            // Calculate the fall time based on the distance between spawn point and hit area, and the fall speed
            float distance = Vector3.Distance(tileSpawnPoints[0].position, hitAreas[0].position);
            fallTime = distance / tileFallSpeed;
        }

        private IEnumerator CalibrateThreshold()
        {
            // Analyze initial spectrum data to set a reasonable peak threshold
            float sum = 0f;
            int samples = 50; // Reduced number of samples for faster calibration
            for (int i = 0; i < samples; i++)
            {
                GetSpectrumDataExtension.GetSpectrumData(audioSource, spectrumData);
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
                GetSpectrumDataExtension.GetSpectrumData(audioSource, spectrumData);
                float currentIntensity = GetAverageIntensity(spectrumData);

                // Adjust the peak threshold dynamically based on the current intensity
                float dynamicThreshold = peakThreshold * (currentIntensity / previousIntensity);

                // Detect peaks based on a significant increase in intensity
                if (currentIntensity > dynamicThreshold)
                {
                    if (!firstTileSpawned)
                    {
                        // Ensure the first tile spawns exactly at the right time
                        float timeSinceMusicStart = Time.time - musicStartTime;
                        float initialDelay = fallTime - timeSinceMusicStart;

                        if (initialDelay > 0)
                        {
                            yield return new WaitForSeconds(initialDelay);
                        }

                        firstTileSpawned = true;
                    }

                    if (Time.time - lastSpawnTime >= minSpawnInterval)
                    {
                        EnqueueTileSpawn(currentIntensity);
                        lastSpawnTime = Time.time;
                    }
                }

                previousIntensity = currentIntensity;

                yield return new WaitForSeconds(0.025f); // Check more frequently for better synchronization
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
                    if (audioSource.isPlaying) // Check if the audio is still playing before spawning a tile
                    {
                        SpawnTile();
                    }
                }
                else
                {
                    yield return null;
                }
            }
        }

        private void EnqueueTileSpawn(float currentIntensity)
        {
            // Calculate a delay based on the current intensity to adjust difficulty dynamically
            float intensityFactor = Mathf.Clamp(currentIntensity / peakThreshold, 0.5f, 2f); // Ensure intensityFactor is within a reasonable range
            float adjustedMinSpawnInterval = minSpawnInterval / intensityFactor;
            float delay = Random.Range(0f, adjustedMinSpawnInterval);
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
            tile.Initialize(baseTileFallSpeed);
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
                    Score += 10; // Increase score
                    combo++; // Increase combo
                    maxCombo = Mathf.Max(maxCombo, combo); // Track max combo
                    StartCoroutine(ShowHitEffect(hitAreaCollider.bounds.center)); // Show hit effect
                }
                else
                {
                    combo = 0; // Reset combo on miss
                    Score -= 5; // Decrease score on miss
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

        private async void EndGame(bool cancelled = false)
        {
            isGameEnding = true;
            audioSource.Stop();
            await new WaitForSeconds(2f);
            isGameRunning = false;
            isGameEnding = false;
            OnGameEnded.Invoke(
                new GameResult
                {
                    score = Score,
                    maxCombo = maxCombo,
                    cancelled = cancelled
                }
            );
            await new WaitForSeconds(3f);
        }

        public void CancelGame()
        {
            EndGame(true);
        }
    }
}
