using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
public class SonidoImpactoPro : MonoBehaviour
{
    [Header("Configuración de Impacto")]
    public float umbralImpacto = 2f;          // Fuerza mínima para empezar a sonar
    public AudioClip sonidoImpacto;           // Sonido a reproducir al chocar
    public float volumenMinimo = 0.2f;         // Volumen mínimo del sonido
    public float volumenMaximo = 1f;           // Volumen máximo del sonido
    public float tiempoEspera = 0.1f;          // Tiempo de espera entre sonidos

    private AudioSource fuenteAudio;
    private Rigidbody rb;
    private float ultimoTiempoReproduccion;

    void Start()
    {
        fuenteAudio = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();

        // Configuramos el AudioSource para que sea 3D
        fuenteAudio.spatialBlend = 1f;   // 1 = completamente 3D
        fuenteAudio.playOnAwake = false;
    }

    void OnCollisionEnter(Collision colision)
    {
        float fuerzaImpacto = colision.relativeVelocity.magnitude;

        if (fuerzaImpacto >= umbralImpacto && Time.time > ultimoTiempoReproduccion + tiempoEspera)
        {
            ReproducirSonidoImpacto(fuerzaImpacto);
        }
    }

    void ReproducirSonidoImpacto(float fuerzaImpacto)
    {
        if (sonidoImpacto != null)
        {
            // Calculamos volumen según la fuerza
            float volumenCalculado = Mathf.InverseLerp(umbralImpacto, umbralImpacto * 5f, fuerzaImpacto); // Normalizamos fuerza
            volumenCalculado = Mathf.Lerp(volumenMinimo, volumenMaximo, volumenCalculado);                // Escalamos a volumen

            fuenteAudio.volume = Mathf.Clamp(volumenCalculado, volumenMinimo, volumenMaximo);
            fuenteAudio.PlayOneShot(sonidoImpacto, fuenteAudio.volume);

            ultimoTiempoReproduccion = Time.time;
        }
    }
}