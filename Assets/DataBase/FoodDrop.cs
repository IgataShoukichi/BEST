using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodDrop : MonoBehaviour
{
    /*セット方法
     * Trayをアタッチ
     * decalをPrefabファイルからアタッチ
     * colorRGBAに汚れの色を入力
     * 今はDを押すと発動します
     */

    private float dropPos;//オブジェクトの真下のオブジェクトの位置情報

    public GameObject tray;//トレイのゲームオブジェクト
    public GameObject decal;//デカールっぽい汚れオブジェクト

    [SerializeField] private Color32 colorRGBA;//汚れの色


    void Update()
    {
        //ぶつかったときの判定（velocityとかで判断）
        if (Input.GetKeyDown("d"))
        {
            //真下にRayを飛ばす
            Ray ray = new Ray(gameObject.transform.position, -gameObject.transform.up);
            foreach (RaycastHit hit in Physics.RaycastAll(ray))
            {
                if (hit.collider.gameObject.tag == "Floor")//床"Floor"タグにレイが当たった時
                {
                    //床のｙ位置 + ｙスケール/2 + 0.01f(テクスチャがかぶらないようにする)
                    dropPos = hit.collider.gameObject.transform.position.y +
                        hit.collider.gameObject.transform.localScale.y / 2 + 0.01f;

                    //デカールをオブジェクトの真下に生成する
                    GameObject myDecal = Instantiate(decal, new Vector3(gameObject.transform.position.x, dropPos, gameObject.transform.position.z),
                        Quaternion.EulerRotation(0f, 0f, 0f));

                    //デカールの色を変更
                    myDecal.GetComponent<MeshRenderer>().material.color = colorRGBA;

                    //自分自身とトレイを非表示に
                    this.gameObject.SetActive(false);
                    tray.gameObject.SetActive(false);

                }
            }
        }
    }
}
