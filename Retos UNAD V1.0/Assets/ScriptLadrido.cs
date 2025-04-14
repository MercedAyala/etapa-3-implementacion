using System.Collections;
using UnityEngine;

public class SoundInteractable : MonoBehaviour, IInteractable
{
    // 🎧 Sonido que se reproducirá al interactuar
    public AudioSource audioSource;

    // ⏱️ Tiempo de espera entre interacciones (en segundos)
    public float cooldown = 4f;

    // 🔒 Control interno para evitar múltiples activaciones
    private bool canPlay = true;

    public void Interact()
    {
        if (canPlay && audioSource != null)
        {
            audioSource.Play();
            StartCoroutine(ResetCooldown());
        }
    }

    // ⏳ Corrutina que reinicia la habilidad de reproducir el sonido
    private IEnumerator ResetCooldown()
    {
        canPlay = false;
        yield return new WaitForSeconds(cooldown);
        canPlay = true;
    }
}