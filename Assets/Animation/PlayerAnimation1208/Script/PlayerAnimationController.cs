using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] public  Animator myAnimator;//自分のアニメーター
    [SerializeField] Rigidbody myRigidBody;//自分のリジッドボディ

    ///////////////////////////////

    float playerMoveStartSpeed = 1f;

    ///////////////////////////////




    //Animator anim;//自分のアニメーター
    //Rigidbody rb;//自分のリジッドボディ
    bool move = false;//移動OKな状態か
    bool command = false;//コマンドが入力されているか
    bool nowHit = false;//いま押されているか

    void Start()
    {
        /*
        if(myRigidBody == null)
        {
            myRigidBody = GetComponent<Rigidbody>();
        }
        if(myAnimator == null)
        {
            myAnimator = GetComponent<Animator>();
        }
        */
    }
    public enum AnimationName//enum
    {
        neutral,    //待機
        walk,       //歩く
        run,        //走る
        order,      //注文
        register,   //レジ打ち
        bussing,    //バッシング（片付け）
        push,       //押す
        surprise,   //驚く
        hit,        //押される
        Null        //何もない
    }
    AnimationName aniName = AnimationName.neutral;

    public enum BringAnimationName//ものを持つアニメーションのenum
    {
        neutral,    //何もなし
        normal,     //通常
        bigCake     //メチャデカケーキ用
    }

    BringAnimationName briName = BringAnimationName.neutral;


    #region Check
    public AnimationName NowAnimationPlayed()//現状確認(コマンドだけ)
    {
        if (myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Order"))
        {
            return AnimationName.order;
        }
        else if(myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Cash_Register1"))
        {
            return AnimationName.register;
        }
        else if (myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Bussing"))
        {
            return AnimationName.bussing;
        }
        else
        {
            return AnimationName.Null;
        }
        /*
        switch (name)
        {
                
            case AnimationName.order://注文
                if (myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Order"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
                break;

            case AnimationName.register://レジ
                if (myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Cash_Register1"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
                break;

            case AnimationName.bussing://バッシング
                if (myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Bussing"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
                break;

            case AnimationName.push://押す
                if (myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Push"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
                break;

            case AnimationName.surprise://ものを落として驚く
                if (myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Surprise"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
                break;

            case AnimationName.hit://押される
                if (myAnimator.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
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
        */
    }

    //ものを持つアニメーションの現状確認
    public bool nowBringAnimationPlayed(BringAnimationName briName)
    {
        switch (briName)
        {
            case BringAnimationName.normal://通常
                if (myAnimator.GetCurrentAnimatorStateInfo(1).IsName("Bring_Hand"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
                break;

            case BringAnimationName.bigCake://メチャデカケーキ
                if (myAnimator.GetCurrentAnimatorStateInfo(1).IsName("Bring_Hand1"))
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
            case AnimationName.walk://歩く
                myAnimator.SetBool("Run", false);
                myAnimator.SetBool("Walk", true);//走るをOFF
                return true;

            case AnimationName.run://走る
                myAnimator.SetBool("Walk", false);//歩くをOFF
                myAnimator.SetBool("Run", true);
                return true;

            case AnimationName.neutral://待機
                myAnimator.SetBool("Walk", false);
                myAnimator.SetBool("Run", false);
                return true;

            case AnimationName.order://オーダー
                myAnimator.SetBool("Order", true);
                move = true;//移動系アニメーションOFF
                command = true;//コマンドON
                return true;

            case AnimationName.register://レジ
                myAnimator.SetBool("Cash_Register", true);
                move = true;//移動系アニメーションOFF
                command = true;//コマンドON
                return true;

            case AnimationName.bussing://バッシング
                myAnimator.SetBool("Bussing", true);
                move = true;//移動系アニメーションOFF
                command = true;//コマンドON
                return true;

            case AnimationName.push://押す
                if (!command)//これをしないとトリガーが２回再生されてしまう
                {
                    myAnimator.SetTrigger("Push");
                    move = true;//移動系アニメーションOFF
                    command = true;//コマンドON
                }
                return true;

            case AnimationName.surprise://ものを落として驚く
                if (!nowHit)
                {
                    myAnimator.SetTrigger("Surprise");
                    move = true;//移動系アニメーションOFF
                    command = false;//コマンドOFF
                    nowHit = true;//押されON
                }
                return true;

            case AnimationName.hit://押される
                if (!nowHit)
                {
                    myAnimator.SetTrigger("Hit");
                    move = true;//移動系アニメーションOFF
                    command = false;//コマンドOFF
                    nowHit = true;//押されON
                }
                return true;
        }
        return false;

    }

    //ものを持つときのアニメーション再生
    public bool BringAnimationPlay(BringAnimationName briName)
    {
        switch (briName)
        {
            case BringAnimationName.normal://通常
                myAnimator.SetBool("Bring1", false);
                myAnimator.SetBool("Bring2", true);
                return true;

            case BringAnimationName.bigCake://ビッグケーキ
                
                myAnimator.SetBool("Bring1", true);
                myAnimator.SetBool("Bring2", false);
                return true;
        }
        return false;
    }

    #endregion

    #region Stop

    public bool AnimationStop(AnimationName name, bool finish = true)//停止
    {
        switch (name)
        {
            case AnimationName.order://オーダー
                myAnimator.SetBool("Order", false);
                command = false;//コマンドOFF
                move = false;//移動系アニメーションON
                return false;

            case AnimationName.register://レジ
                myAnimator.SetBool("Cash_Register", false);
                if (finish)
                {
                    myAnimator.SetTrigger("Cash_Register_F");//トリガーでオジギする
                }
                command = false;//コマンドOFF
                move = false;//移動系アニメーションON
                return true;

            case AnimationName.bussing://バッシング
                myAnimator.SetBool("Bussing", false);
                command = false;//コマンドOFF
                move = false;//移動系アニメーションON
                return true;
        }
        return false;
    }

    //
    public bool BringAnimationStop(BringAnimationName briName)
    {
        switch(briName)
        {
            case BringAnimationName.normal:
                myAnimator.SetBool("Bring1", false);
                myAnimator.SetBool("Bring2", false);
                return false;

            case BringAnimationName.bigCake:
                myAnimator.SetBool("Bring1", false);
                myAnimator.SetBool("Bring2", false);
                return false;
        }
        return false;

    }

    #endregion

    //トリガーアニメーション用
    public void AnimationFinish()
    {
        command = false;//コマンドOFF
        nowHit = false;//押されOFF
        move = false;//移動系アニメーションON
    }

    //押されたり などした時に、作業中のアニメーションを停止する
    public void WorkAnimationOFF()
    {
        myAnimator.SetBool("Order", false);
        myAnimator.SetBool("Cash_Register", false);
        myAnimator.SetBool("Bussing", false);
    }


    void Update()
    {

        if (!move && myRigidBody != null && myAnimator != null)//動けるとき
        {
            //歩く判定
            if (myRigidBody.velocity.magnitude > playerMoveStartSpeed)
            {
                if(aniName != AnimationName.walk)
                {
                    aniName = AnimationName.walk;
                    AnimationPlay(aniName);
                }
                
            }
            //走る判定(サブ)
            /*else if (rb.velocity.magnitude > 0   && 走る用フラグ )
            {
                aniName = AnimationName.run;
            }*/
            //待機判定
            else if (myRigidBody.velocity.magnitude <= playerMoveStartSpeed)
            {
                if(aniName != AnimationName.neutral)
                {
                    aniName = AnimationName.neutral;
                    AnimationPlay(aniName);
                }
            }
        }
        

        /*
        if (!command)//コマンド中ではないとき
        {

            //オーダーとる瞬間
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                aniName = AnimationName.order;
            }

            //レジする瞬間
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                aniName = AnimationName.register;
            }

            //バッシングする瞬間
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                aniName = AnimationName.bussing;
            }

            //押す瞬間
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                aniName = AnimationName.push;
            }


        }

        if (!nowHit)//殴られていないとき
        {
            //ものを落として驚く
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                aniName = AnimationName.surprise;
                WorkAnimationOFF();
            }

            //押される
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                aniName = AnimationName.hit;
                WorkAnimationOFF();
            }

            
            //突進は今はなし(一応残しときます)
            if (Input.GetKey(KeyCode.LeftShift))
            {
                anim.SetBool("Rush", true);
            }
            else
            {
                anim.SetBool("Rush", false);
            }


            //突進される
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                anim.SetTrigger("Rush_Receive");
            }
            

        }

        //何も持っていないとき
        if(briName == BringAnimationName.neutral)
        {
            //持った瞬間(通常)
            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                briName = BringAnimationName.normal;
            }

            //持った瞬間(メチャデカケーキ)
            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                briName = BringAnimationName.bigCake;
            }

        }

        //再生
        AnimationPlay(aniName);

        //停止する瞬間
        if (Input.GetKeyDown(KeyCode.M) && NowAnimationPlayed(aniName))
        {
            //現状確認をしてtrueだったら止める
            AnimationStop(aniName);
        }

        //物持ちアニメーション再生
        BringAnimaitonPlay(briName);

        //物持ちを停止する
        if (Input.GetKeyDown(KeyCode.N) && nowBringAnimationPlayed(briName))
        {
            BringAnimationStop(briName);
            briName = BringAnimationName.neutral;
        }
        */
    }
}
