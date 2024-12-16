using Car.Controller.V1;
using DG.Tweening;
using UnityEngine;

public class OilTrapController : MonoBehaviour
{
    [SerializeField] bool isEnabled;
    [SerializeField] float oilTime;
    [Tooltip("X = Sideways slip & Y = Forward slip")]
    [SerializeField] Vector2 tireStiffnessAfterBeingOiled;
    [Header("Timers")]
    [SerializeField] float activeTime;
    public float timeUntilDeath;
    [Header("Refs")]
    [SerializeField] WheelControl wheelControlAffected;
    [SerializeField] ParticleSystem particles;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timeUntilDeath = activeTime;
        if (particles == null)
            particles = gameObject.GetComponentInChildren<ParticleSystem>();
    }
    public void OnTriggerEnter(Collider other)
    {
        print(other.name + " has entered");
        if (isEnabled)
        {
            print("is enabled");
            if (other.GetComponent<WheelControl>())
            {
                wheelControlAffected = other.GetComponent<WheelControl>();
                print("tag is good");
                StartCoroutine(wheelControlAffected.Oiled(oilTime, tireStiffnessAfterBeingOiled));
                return;
            }
            print("heuuuuu.........j'espère que c'etait pas la voiture parceque la je viens de la laisser passer");
        }
    }
    private void Update()
    {
        if (isEnabled)
        {
            if (timeUntilDeath > 0)
            {
                timeUntilDeath -= Time.deltaTime;
            }
            else
            {
                isEnabled = false;
                transform.DOMoveY(transform.position.y - 0.5f, 0.5f);
                //TODO trouver un moyen de faire disparaitre la mine sans que ce soit trop cher !!!
            }
        }
    }
}
