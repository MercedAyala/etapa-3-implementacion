using UnityEngine;

public class GiroConstante : MonoBehaviour
{
    [Header("Velocidad de rotación en grados por segundo")]
    public Vector3 velocidadRotacion = new Vector3(0, 30, 0);

    void Update()
    {
        // Rotar el objeto según la velocidad definida y el tiempo real
        transform.Rotate(velocidadRotacion * Time.deltaTime);
    }
}