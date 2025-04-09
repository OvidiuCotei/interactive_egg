using UnityEngine;
using System.Collections;

public class EggCrackManager : MonoBehaviour
{
    public GameObject eggBase;              // Oul intact
    public GameObject eggShardParent;
    public SpriteRenderer[] crackStages;    // Sprite-uri care se vor face fade-in
    public GameObject character;            // Personajul 3D
    public float fadeDuration = 0.5f;       // Durată fade-in

    private int tapCount = 0;
    private bool isRevealing = false;
    public Vector3 targetScale = Vector3.one;

    public float explosionForce = 300f;
    public float explosionRadius = 2f;
    public Vector3 explosionOriginOffset = Vector3.up * 0.2f;

    void Start()
    {
        eggBase.SetActive(true);
        character.SetActive(false);

        // Setăm alpha 0 pentru toate crăpăturile la început
        foreach (var crack in crackStages)
        {
            SetAlpha(crack, 0f);
            crack.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        HandleInput();

        if (isRevealing)
            AnimateCharacterAppearance();
    }

    void HandleInput()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
            TryTapEgg(Input.mousePosition);
#else
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            TryTapEgg(Input.GetTouch(0).position);
#endif
    }

    void TryTapEgg(Vector2 screenPosition)
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

    void TapEgg()
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

    void RevealCharacter()
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

    void AnimateCharacterAppearance()
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

    IEnumerator FadeIn(SpriteRenderer sprite)
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

    void SetAlpha(SpriteRenderer sprite, float alpha)
    {
        Color c = sprite.color;
        c.a = alpha;
        sprite.color = c;
    }

    void ExplodeEggShells()
    {
        eggShardParent.SetActive(true); // Activezi cojile

        foreach (Transform shard in eggShardParent.transform)
        {
            Rigidbody rb = shard.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.isKinematic = false; // Permite fizica
                rb.AddExplosionForce(explosionForce, eggShardParent.transform.position + explosionOriginOffset, explosionRadius, 0.2f, ForceMode.Impulse);
            }
        }
    }
}
