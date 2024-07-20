using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LoadingFade : MonoBehaviour
{
    Image image;
    float rotateSpeed = 150;
    [SerializeField] bool rightRotation = true;

    void Awake()
    {
        if (rightRotation)
        {
            rotateSpeed = -rotateSpeed;
        }
        image = this.GetComponent<Image>();
        image.enabled = true;
    }

    void OnEnable()
    {
        StartCoroutine(LoadingAnimation());
    }

    void Update()
    {
        this.transform.Rotate(new Vector3(0, 0, rotateSpeed * Time.deltaTime));
    }

    //void FixedUpdate()
    //{
    //    this.transform.Rotate(new Vector3(0, 0, rotateSpeed));
    //}

    IEnumerator LoadingAnimation()
    {
        image.fillAmount = 0;
        if (rightRotation){
            while (true)
            {
                image.fillClockwise = true;
                image.DOFillAmount(1, 1.0f).SetEase(Ease.InCubic);
                yield return new WaitForSeconds(1.0f);
                image.fillClockwise = false;
                image.DOFillAmount(0, 1.0f).SetEase(Ease.OutCubic);
                yield return new WaitForSeconds(1.0f);
            }
        }
        else
        {
            while (true)
            {
                image.fillClockwise = false;
                image.DOFillAmount(1, 1.0f).SetEase(Ease.InCubic);
                yield return new WaitForSeconds(1.0f);
                image.fillClockwise = true;
                image.DOFillAmount(0, 1.0f).SetEase(Ease.OutCubic);
                yield return new WaitForSeconds(1.0f);
            }
        }

    }
}
