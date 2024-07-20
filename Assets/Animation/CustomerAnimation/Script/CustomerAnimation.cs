using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerAnimation : MonoBehaviour
{
    Animator animCus;//自分のアニメーター

    bool nowSit = false;//今座っているか

    void Start()
    {
        animCus = GetComponent<Animator>();
    }

    public enum AnimationName//enum
    {
        neutralCus,    //待機
        walkCus,       //歩く
        sitCus,        //座る
        look_menuCus,  //メニューを見る
        eat_foodCus,   //料理を食べる
        registerCus    //レジ打ち
    }

    //最初はNeutralをセット
    AnimationName aniName = AnimationName.neutralCus;

    #region Check
    public bool NowAnimationPlayed(AnimationName name)//現状確認(コマンドだけ)
    {
        switch (name)
        {
            /*一応残してます
            case AnimationName.walkCus:
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("WalkCus"))
                //名前がAnimatorのステート名と一致したら
                {
                    return true;
                }
                else
                {
                    return false;
                }
                break;
            case AnimationName.neutralCus:
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("NeutralCus"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
                break;
            */
            case AnimationName.sitCus://座る
                if (animCus.GetCurrentAnimatorStateInfo(0).IsName("Sit_Chair"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
                break;

            case AnimationName.look_menuCus://メニューを見る
                if (animCus.GetCurrentAnimatorStateInfo(0).IsName("Look_menu"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
                break;

            case AnimationName.eat_foodCus://料理を食べる
                if (animCus.GetCurrentAnimatorStateInfo(0).IsName("Eat_food"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
                break;

        }
        return false;
    }

    #endregion

    #region Play
    public bool AnimationPlay(AnimationName name)//再生
    {
        switch (name)
        {
            case AnimationName.walkCus://歩く
                animCus.SetBool("WalkCus", true);
                return true;

            case AnimationName.neutralCus://待機
                animCus.SetBool("WalkCus", false);
                return true;

            case AnimationName.sitCus://座る
                animCus.SetBool("SitCus", true);
                animCus.SetBool("Look_menuCus", false);
                animCus.SetBool("Eat_foodCus", false);
                nowSit = true;
                return true;

            case AnimationName.look_menuCus://メニューを見る
                animCus.SetBool("Look_menuCus", true);
                animCus.SetBool("Eat_foodCus", false);
                return true;

            case AnimationName.eat_foodCus://料理を食べる
                animCus.SetBool("Eat_foodCus", true);
                animCus.SetBool("Look_menuCus", false);
                return true;
        }
        return false;
    }


    #endregion

    #region Stop

    public bool AnimationStop(AnimationName name)//停止
    {
        switch (name)
        {

            case AnimationName.sitCus://座る
                animCus.SetBool("SitCus", false);
                nowSit = false;
                return false;

            case AnimationName.look_menuCus://メニューを見る
                animCus.SetBool("Look_menuCus", false);
                animCus.SetBool("SitCus", false);
                nowSit = false;

                return false;

            case AnimationName.eat_foodCus://料理を食べる
                animCus.SetBool("Eat_foodCus", false);
                animCus.SetBool("SitCus", false);
                nowSit = false;

                return false;
        }
        return false;
    }

    #endregion

    void Update()
    {
        /*
        //座りアクション（食べる、メニュー拝見）時のアニメーション解除について↓

        //座りアクション中に普通の座り状態に戻したい時は、「aniName」をsitCusに変えてね
        //食べ・メニュ見　のアニメーション状態は自動で解除されるようになっています

        //アクション中に普通の座りを経由せず、そのまま立ちたい場合は「AnimationStop」
        //を呼び出してね


        if (!nowSit)//座っていないとき
        {
            //歩く判定
            if (Input.GetKey(KeyCode.W))
            {
                aniName = AnimationName.walkCus;
            }
            else//動いていないとき
            {
                aniName = AnimationName.neutralCus;
            }
        }


        //座る瞬間
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            aniName = AnimationName.sitCus;
        }

        //座っていてメニューを見る瞬間
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            aniName = AnimationName.look_menuCus;
        }

        //座っていて料理を食べる瞬間
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            aniName = AnimationName.eat_foodCus;
        }


        //再生
        AnimationPlay(aniName);

        //停止する瞬間
        if (Input.GetKeyDown(KeyCode.M) && NowAnimationPlayed(aniName))
        {
            //現状確認をしてtrueだったら止める
            AnimationStop(aniName);
        }
        */
    }
    
}
