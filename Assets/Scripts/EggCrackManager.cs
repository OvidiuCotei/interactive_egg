using UnityEngine;
using System.Collections;

public class EggCrackManager : MonoBehaviour
{
    public GameObject eggBase;              // The intact egg model shown at the beginning
    public GameObject eggShardParent;       // Parent object that contains all broken egg shell pieces
    public SpriteRenderer[] crackStages;    // An array of crack sprites to be shown one by one with fade-in effect
    public GameObject character;            // The 3D character that appears after the egg breaks
    public float fadeDuration = 0.5f;       // Time in seconds to fully fade in each crack sprite

    private int tapCount = 0;
    private bool isRevealing = false;
    public Vector3 targetScale = Vector3.one;   // Target scale for the character during the reveal animation

    public float explosionForce = 300f;     // Force applied to egg pieces during the explosion
    public float explosionRadius = 2f;      // Radius of the explosion effect
    public Vector3 explosionOriginOffset = Vector3.up * 0.2f;  // Position offset from the center of the egg for the explosion origin

    /// <summary>
    /// Activates the intact egg and hides the character
    /// Hides all crack sprites and sets their alpha to 0
    /// </summary>
    private void Start()
    {
        eggBase.SetActive(true);
        character.SetActive(false);

        // We set alpha 0 for all cracks at the beginning
        foreach (var crack in crackStages)
        {
            SetAlpha(crack, 0f);
            crack.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Listens for input (mouse click or touch)
    /// If the character is being revealed, triggers a scale-up animation
    /// </summary>
    private void Update()
    {
        HandleInput();

        if (isRevealing)
            AnimateCharacterAppearance();
    }

    /// <summary>
    /// Detects mouse clicks (in Editor) or touches (on mobile)
    /// Sends screen position to TryTapEgg()
    /// </summary>
    private void HandleInput()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
            TryTapEgg(Input.mousePosition);
#else
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            TryTapEgg(Input.GetTouch(0).position);
#endif
    }

    /// <summary>
    /// Performs a Raycast from the screen position
    /// If the user taps on the egg (Tag = "Egg"), triggers TapEgg()
    /// </summary>
    /// <param name="screenPosition"></param>
    private void TryTapEgg(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Egg"))
            {
                TapEgg();
            }
        }
    }

    /// <summary>
    /// Activates the next crack sprite
    /// Starts a fade-in coroutine
    /// Increments the tapCount
    /// Plays a crack sound using AudioManager.Instance.PlayCrackSound()
    /// If all cracks are revealed: Calls RevealCharacter()
    /// </summary>
    private void TapEgg()
    {
        if (tapCount < crackStages.Length)
        {
            SpriteRenderer crack = crackStages[tapCount];
            crack.gameObject.SetActive(true);
            StartCoroutine(FadeIn(crack));
            tapCount++;
            AudioManager.Instance.PlayCrackSound();
        }
        else
        {
            RevealCharacter();
        }
    }

    /// <summary>
    /// Hides the intact egg and all crack sprites
    /// Activates the shattered egg pieces and triggers their explosion
    /// Activates the character and starts a scale-up animation
    /// </summary>
    private void RevealCharacter()
    {
        eggBase.SetActive(false);
        ExplodeEggShells();

        foreach (var crack in crackStages)
        {
            crack.gameObject.SetActive(false);
        }

        character.SetActive(true);
        character.transform.localScale = Vector3.zero;
        isRevealing = true;
    }
    /// <summary>
    /// Gradually scales the character from Vector3.zero to Vector3.one using Lerp()
    /// When the target size is reached, stops the animation
    /// </summary>
    private void AnimateCharacterAppearance()
    {
        character.transform.localScale = Vector3.Lerp(
            character.transform.localScale,
            targetScale,
            Time.deltaTime * 5f
        );

        if (Vector3.Distance(character.transform.localScale, targetScale) < 0.01f)
        {
            character.transform.localScale = targetScale;
            isRevealing = false;
        }
    }

    /// <summary>
    /// Smoothly fades in a crack sprite by increasing its alpha from 0 to 1 over fadeDuration seconds
    /// </summary>
    /// <param name="sprite"></param>
    /// <returns></returns>
    private IEnumerator FadeIn(SpriteRenderer sprite)
    {
        float elapsed = 0f;
        Color c = sprite.color;
        c.a = 0f;
        sprite.color = c;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsed / fadeDuration);
            sprite.color = c;
            yield return null;
        }

        c.a = 1f;
        sprite.color = c;
    }

    /// <summary>
    /// Manually sets a sprite’s transparency level
    /// </summary>
    /// <param name="sprite"></param>
    /// <param name="alpha"></param>
    private void SetAlpha(SpriteRenderer sprite, float alpha)
    {
        Color c = sprite.color;
        c.a = alpha;
        sprite.color = c;
    }

    /// <summary>
    /// Activates all shell fragments (eggShardParent)
    /// For each child object: Enables physics (isKinematic = false), Applies an explosion force via AddExplosionForce() to simulate shell breakage and scattering
    /// </summary>
    private void ExplodeEggShells()
    {
        eggShardParent.SetActive(true); // Activate the shells

        foreach (Transform shard in eggShardParent.transform)
        {
            Rigidbody rb = shard.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.isKinematic = false; // It allows physics
                rb.AddExplosionForce(explosionForce, eggShardParent.transform.position + explosionOriginOffset, explosionRadius, 0.2f, ForceMode.Impulse);
            }
        }
    }
}
