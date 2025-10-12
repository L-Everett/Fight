using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] float parallaxSpeed = 0.5f;

    Transform cam;
    Vector3 lastCamPos; 

    void Start()
    {
        //player = GameObject.FindGameObjectWithTag("Player").transform;
        cam = Camera.main.transform;
        lastCamPos = cam.position;
        //startPosition = transform.position;
    }

    void LateUpdate()
    {
        Vector3 playerDelta = cam.position - lastCamPos;
        transform.position = transform.position - new Vector3(playerDelta.x * parallaxSpeed, 0, 0);

        ApplyWindEffect();
        lastCamPos = cam.position;
    }

    void ApplyWindEffect()
    {
        // 远处图层应用风效
        if (parallaxSpeed < 0.2f)
        {
            float windEffect = Mathf.Sin(Time.time * 0.5f) * 0.001f;
            transform.position += Vector3.up * windEffect;
        }
    }
}