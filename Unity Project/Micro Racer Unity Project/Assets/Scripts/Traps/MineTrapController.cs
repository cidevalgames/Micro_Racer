using System.Collections;
using UnityEngine;

public class MineTrapController : MonoBehaviour
{
    [SerializeField] bool isEnabled;
    [SerializeField] bool isTriggered;
    [SerializeField] float strength;
    [Header("Timers")]
    [SerializeField] float fuseTime;
    [SerializeField] float coolDown;
    [Header("Refs")]
    [SerializeField] PlayerLife playerLifeAffected;
    [SerializeField] ParticleSystem particles;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (particles == null)
            particles = gameObject.GetComponentInChildren<ParticleSystem>();
        isTriggered = false;
    }
    public void OnTriggerEnter(Collider other)
    {
        //print(other.name + " has entered");
        if (isEnabled)
        {
            //print("is enabled");
            if (other.CompareTag("Player") && !isTriggered)
            {
                //print("tag is good");
                isTriggered = true;
                playerLifeAffected = other.GetComponent<PlayerLife>();
                StartCoroutine(MineTriggered(fuseTime));
                return;

            }
            //print("heuuuuu.........j'espère que c'etait pas la voiture parceque la je viens de la laisser passer");
        }
    }
    IEnumerator MineTriggered(float fuseTime)
    {
        RaycastHit hit = new();
        yield return new WaitForSeconds(fuseTime);
        Physics.Raycast(transform.position, gameObject.transform.up, out hit, 0.3f);
        particles.Play();
        playerLifeAffected.MineAttack(strength, hit.point);
        yield return new WaitForSeconds(coolDown);
        isTriggered = false;
    }
}
