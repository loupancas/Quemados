using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform Target; // El objetivo al que la cámara debe mirar
    public float DistanceFromTarget = 5.0f; // Distancia de la cámara al objetivo
    public float MouseSensitivity = 10f; // Sensibilidad del ratón
    public Vector2 pitchMinMax = new Vector2(20f, 70f); // Restricción de inclinación vertical en grados

    private float pitch = 0f; // Rotación vertical
    private float yaw = 0f; // Rotación horizontal

    void LateUpdate()
    {
        if (Target == null)
        {
            return;
        }

        yaw += Input.GetAxis("Mouse X") * MouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * MouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

        Vector3 targetRotation = new Vector3(pitch, yaw);
        transform.eulerAngles = targetRotation;

        transform.position = Target.position - transform.forward * DistanceFromTarget;
    }
}
