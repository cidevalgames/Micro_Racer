using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu_PageSwap : MonoBehaviour
{
    public List<Button> LesBoutons;
    public List<GameObject> Vehicules;
    public float tempsAnim;

    void Start()
    {
        for (int i = 0; i < LesBoutons.Count; i++)
        {
            int index = i;
            LesBoutons
    [index].onClick.AddListener(() => SwitchCars(index));
        }
    }

    public void SwitchCars(int index)
    {
        StartCoroutine(Lacoroutine(index));
    }

    public IEnumerator Lacoroutine(int index)
    {
        for (int i = 0; i < LesBoutons.Count; i++)
        {
            if (i == index)
            {
                foreach (GameObject voiture in Vehicules)
                {
                    voiture.SetActive(false);
                }

                yield return new WaitForSeconds(tempsAnim);

                Vehicules[index].SetActive(true);

            }

        }
    }
}