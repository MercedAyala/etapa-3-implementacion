using UnityEngine;
using System.Collections;

public class WigglyTentacleGrabber : MonoBehaviour
{
    [Header("Configuracion de Agarre")]
    public float distanciaAgarrar = 10f;
    public float velocidadSeguir = 10f;
    public float fuerzaLanzar = 1500f;
    public Transform puntoSujecion;
    public Camera cam;

    [Header("Control de Distancia")]
    public float distanciaActual = 5f;
    public float distanciaMinima = 0.6f;
    public float distanciaMaxima = 20f;
    public float velocidadScroll = 2f;

    [Header("Visualizacion del Tentaculo")]
    public Material materialTentaculo;
    public Material materialTentaculoError;
    public int cantidadSegmentos = 20;
    public float amplitudOnda = 0.1f;
    public float frecuenciaOnda = 2f;
    public float velocidadOnda = 5f;
    public float shakeIntensidad = 0.3f;
    public float shakeDuracion = 0.3f;
    public float intensidadShakeCirculares = 0.5f;

    [Header("Sonidos")]
    public AudioSource sonidoSujecion;
    public AudioSource sonidoAgarrar;
    public AudioSource sonidoSoltar;
    public AudioSource sonidoLanzar;
    public AudioSource sonidoError;

    [Range(0f, 1f)]
    public float volumenMaximo = 1f;
    public float tiempoFade = 0.5f;

    [Header("Circulando Tentaculos")]
    public int cantidadBeamsCirculares = 3;
    public float radioCirculo = 0.2f;
    public float velocidadRotacionBase = 180f;

    private Rigidbody objetoAgarrado;
    private Vector3 puntoAgarrado;
    private bool esObjetoAgarrable;
    private LineRenderer lineaPrincipal;
    private LineRenderer[] lineasCirculares;
    private float masaObjeto;
    private Vector3 velocidadInicial;
    private Coroutine fadeCoroutine;
    private bool estaTemblando = false;
    private float tiempoShake = 0f;
    private bool estaRetrayendo = false;
    private float tiempoRetraccion = 0f;

    void Start()
    {
        if (cam == null)
            cam = Camera.main;

        GameObject objetoLinea = new GameObject("LineaTentaculoPrincipal");
        objetoLinea.transform.parent = transform;
        lineaPrincipal = objetoLinea.AddComponent<LineRenderer>();
        lineaPrincipal.material = materialTentaculo;
        lineaPrincipal.startColor = Color.cyan;
        lineaPrincipal.endColor = Color.magenta;
        lineaPrincipal.startWidth = 0.02f;
        lineaPrincipal.endWidth = 0.1f;
        lineaPrincipal.positionCount = cantidadSegmentos;
        lineaPrincipal.numCapVertices = 10;
        lineaPrincipal.enabled = false;

        lineasCirculares = new LineRenderer[cantidadBeamsCirculares];
        for (int i = 0; i < cantidadBeamsCirculares; i++)
        {
            GameObject obj = new GameObject("LineaTentaculoCircular_" + i);
            obj.transform.parent = transform;
            LineRenderer lr = obj.AddComponent<LineRenderer>();
            lr.material = materialTentaculo;
            lr.startColor = Color.white;
            lr.endColor = Color.white;
            lr.startWidth = 0.01f;
            lr.endWidth = 0.01f;
            lr.positionCount = cantidadSegmentos;
            lr.numCapVertices = 10;
            lr.enabled = false;
            lineasCirculares[i] = lr;
        }

        if (sonidoSujecion != null)
            sonidoSujecion.loop = true;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) IntentarAgarrarObjeto();
        if (Input.GetMouseButtonUp(0)) SoltarObjeto();
        if (Input.GetMouseButtonDown(1)) LanzarObjeto();

        if (objetoAgarrado != null || lineaPrincipal.enabled)
        {
            ActualizarTentaculo();
            ManejarScroll();

            if (objetoAgarrado != null && !esObjetoAgarrable)
            {
                float distanciaDesdePunto = Vector3.Distance(cam.transform.position, puntoAgarrado);
                if (distanciaDesdePunto > distanciaMaxima + 0.5f)
                {
                    SoltarObjeto();
                }
            }
        }

        if (estaRetrayendo && Time.time > tiempoRetraccion)
        {
            lineaPrincipal.enabled = false;
            foreach (var lr in lineasCirculares)
                lr.enabled = false;
            estaRetrayendo = false;
        }
    }

    void FixedUpdate()
    {
        if (objetoAgarrado != null && esObjetoAgarrable)
        {
            Vector3 posicionObjetivo = cam.transform.position + cam.transform.forward * distanciaActual;
            float dificultadArrastre = Mathf.Clamp(masaObjeto / 6f, 1f, 5f);
            Vector3 direccionMovimiento = (posicionObjetivo - objetoAgarrado.position) * velocidadSeguir / dificultadArrastre;
            objetoAgarrado.linearVelocity = direccionMovimiento;
        }
    }

    void IntentarAgarrarObjeto()
    {
        Ray rayo = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(rayo, out RaycastHit impacto, distanciaAgarrar))
        {
            Rigidbody rb = impacto.collider.GetComponent<Rigidbody>();
            objetoAgarrado = rb;
            puntoAgarrado = impacto.point;
            esObjetoAgarrable = impacto.collider.CompareTag("canPickUp");

            if (esObjetoAgarrable && rb != null)
            {
                objetoAgarrado.useGravity = false;
                masaObjeto = objetoAgarrado.mass;
                velocidadInicial = objetoAgarrado.linearVelocity;
                distanciaActual = distanciaMinima;

                if (sonidoSujecion != null)
                {
                    sonidoSujecion.pitch = Mathf.Lerp(0.5f, 1.5f, Mathf.Clamp01(masaObjeto / 25f));
                    if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                    fadeCoroutine = StartCoroutine(FadeAudioIn());
                }

                if (sonidoAgarrar != null)
                {
                    sonidoAgarrar.pitch = Mathf.Lerp(1.5f, 0.7f, Mathf.Clamp01(masaObjeto / 25f));
                    sonidoAgarrar.Play();
                }

                lineaPrincipal.material = materialTentaculo;
                foreach (var lr in lineasCirculares)
                    lr.material = materialTentaculo;
            }
            else
            {
                objetoAgarrado = null;
                lineaPrincipal.material = materialTentaculoError;
                foreach (var lr in lineasCirculares)
                    lr.material = materialTentaculoError;

                if (sonidoError != null)
                    sonidoError.Play();

                estaTemblando = true;
                tiempoShake = Time.time + shakeDuracion;
                tiempoRetraccion = Time.time + 0.3f; // Retracción después de 0.3 segundos
                estaRetrayendo = true;
            }

            lineaPrincipal.enabled = true;
            foreach (var lr in lineasCirculares)
                lr.enabled = true;
        }
    }

    void SoltarObjeto()
    {
        if (objetoAgarrado != null && esObjetoAgarrable)
        {
            objetoAgarrado.useGravity = true;
            objetoAgarrado = null;

            if (sonidoSujecion != null)
            {
                if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                fadeCoroutine = StartCoroutine(FadeAudioOut());
            }

            if (sonidoSoltar != null)
            {
                sonidoSoltar.pitch = Mathf.Lerp(1.5f, 0.7f, Mathf.Clamp01(masaObjeto / 25f));
                sonidoSoltar.Play();
            }
        }

        lineaPrincipal.enabled = false;
        foreach (var lr in lineasCirculares)
            lr.enabled = false;
    }

    void LanzarObjeto()
    {
        if (objetoAgarrado != null && esObjetoAgarrable)
        {
            objetoAgarrado.useGravity = true;

            // Multiplicamos aún más la fuerza según la masa del objeto
            float factor = Mathf.InverseLerp(0f, 25f, masaObjeto);
            float fuerzaFinal = fuerzaLanzar * Mathf.Lerp(0.5f, 4f, factor);  // Más multiplicación para objetos más pesados
            Vector3 direccionLanzamiento = (cam.transform.forward + Vector3.up * 0.1f).normalized;
            objetoAgarrado.AddForce(direccionLanzamiento * fuerzaFinal);

            if (sonidoLanzar != null)
            {
                sonidoLanzar.pitch = Mathf.Lerp(2.0f, 0.5f, Mathf.Clamp01(masaObjeto / 25f));
                sonidoLanzar.Play();
            }

            objetoAgarrado = null;

            if (sonidoSujecion != null)
            {
                if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                fadeCoroutine = StartCoroutine(FadeAudioOut());
            }
        }

        lineaPrincipal.enabled = false;
        foreach (var lr in lineasCirculares)
            lr.enabled = false;
    }

    IEnumerator FadeAudioIn()
    {
        sonidoSujecion.volume = 0f;
        if (!sonidoSujecion.isPlaying) sonidoSujecion.Play();

        float timer = 0f;
        while (timer < tiempoFade)
        {
            sonidoSujecion.volume = Mathf.Lerp(0f, volumenMaximo, timer / tiempoFade);
            timer += Time.deltaTime;
            yield return null;
        }
        sonidoSujecion.volume = volumenMaximo;
    }

    IEnumerator FadeAudioOut()
    {
        float volumenInicial = sonidoSujecion.volume;
        float timer = 0f;
        while (timer < tiempoFade)
        {
            sonidoSujecion.volume = Mathf.Lerp(volumenInicial, 0f, timer / tiempoFade);
            timer += Time.deltaTime;
            yield return null;
        }
        sonidoSujecion.volume = 0f;
        sonidoSujecion.Stop();
    }

    void ActualizarTentaculo()
    {
        lineaPrincipal.enabled = true;

        Vector3 esquinaInferiorDerecha = new Vector3(Screen.width, 0, 1f);
        Vector3 inicioMundo = cam.ScreenToWorldPoint(esquinaInferiorDerecha);
        Vector3 finMundo = (objetoAgarrado != null && esObjetoAgarrable) ? objetoAgarrado.worldCenterOfMass : puntoAgarrado;

        Vector3 direccion = (finMundo - inicioMundo);
        float distancia = direccion.magnitude;
        Vector3 direccionBase = direccion.normalized;

        Vector3 direccionOnda = Vector3.Cross(direccionBase, cam.transform.up).normalized;
        if (direccionOnda.magnitude < 0.01f)
            direccionOnda = Vector3.Cross(direccionBase, Vector3.right).normalized;

        float intensidad = (estaTemblando && Time.time < tiempoShake) ? shakeIntensidad : 0f;

        for (int i = 0; i < cantidadSegmentos; i++)
        {
            float t = i / (float)(cantidadSegmentos - 1);
            Vector3 posicion = Vector3.Lerp(inicioMundo, finMundo, t);
            float pulso = Mathf.Sin((Time.time * velocidadOnda) + (t * frecuenciaOnda * Mathf.PI * 2f)) * amplitudOnda;
            if (intensidad > 0f) pulso += Random.Range(-intensidad, intensidad);
            posicion += direccionOnda * pulso;
            lineaPrincipal.SetPosition(i, posicion);
        }

        float anguloBase = Time.time * velocidadRotacionBase;

        for (int b = 0; b < cantidadBeamsCirculares; b++)
        {
            LineRenderer lr = lineasCirculares[b];
            float shakeFactor = estaTemblando && Time.time < tiempoShake ? intensidadShakeCirculares : 0f;

            // Ajusta la velocidad de rotación de los rayos circulares en función de la masa
            float velocidadRotacion = Mathf.Lerp(360f, 30f, masaObjeto / 25f);

            for (int i = 0; i < cantidadSegmentos; i++)
            {
                float t = i / (float)(cantidadSegmentos - 1);
                Vector3 basePos = Vector3.Lerp(inicioMundo, finMundo, t);
                float angulo = anguloBase + (360f / cantidadBeamsCirculares) * b + t * 360f;
                Quaternion rotacion = Quaternion.AngleAxis(angulo, direccionBase);
                Vector3 offset = rotacion * (direccionOnda * radioCirculo);
                if (intensidad > 0f) offset += Random.insideUnitSphere * intensidad * 0.1f;
                lr.SetPosition(i, basePos + offset);
            }
        }
        if (estaTemblando && Time.time > tiempoShake) { estaTemblando = false; }}
    void ManejarScroll()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distanciaActual = Mathf.Clamp(distanciaActual + scroll * velocidadScroll, distanciaMinima, distanciaMaxima);
    }
}