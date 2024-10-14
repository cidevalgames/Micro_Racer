using DG.Tweening;
using UnityEngine;
public class MenuCameraPDV : MonoBehaviour
{
    [SerializeField]
    public Transform mainCamera, position1, position2;

    public void lenomdelafonction1()
    {
        mainCamera.DOMove(position1.position, 2f);
        mainCamera.DORotate(position1.rotation.eulerAngles, 2f);
        return;
    }
    public void lenomdelafonction2()
    {
        mainCamera.DOMove(position2.position, 2f);
        mainCamera.DORotate(position2.rotation.eulerAngles, 2f);
        return;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
