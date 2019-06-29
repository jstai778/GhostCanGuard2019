﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Ghost_targeting : MonoBehaviour
{
    //目標を追う速度
    public float chasingSpeed = 5f;
    //体を回す速度
    public float turnSpeed = 0.5f;
    //パトロール速度
    public float walkSpeed = 2f;
    //目標があるか
    public bool isTargeting;
    //プレイヤーを追う距離
    public float distanceOfChasingPlayer = 10f;
    //追いかけを止める距離
    public float distanceOfGiveUpChase = 15f;
    //パトロールの最大範囲
    public float maxPatrolRange=5f;
    //パトロールの間隔
    public float waitTime = 5f;

    //目標のゲームオブジェクト
    GameObject targetobj;
    //泥棒のゲームオブジェクト
    GameObject thief;
    //プレイヤーのゲームオブジェクト
    GameObject player;
    //泥棒のrigibody(速度を取得ため)
    Rigidbody trb;
    //泥棒が隠しますかどうか
    bool ifThiefHide;

    //プレイヤーまでの距離
    float distancetoPlayer;
    //泥棒までの距離
    float distancetoThief;


    //目標までの回転
    Quaternion targetQuaternion;

   

    //目標を失う位置(パトロールの範囲判定用)
    Vector3 missingPosition;
    //パトロールの目標位置
    Vector3 patroPosition;
    //前回のパトロールが始まる時間
    float lastActTime;
    //パトロールの距離(パトロールの範囲判定用)
    float patrolDistance = 0f;


    //目標の瞬時位置(バイブルに影響された時用)
    Vector3 targetpos;
    //バイブルに影響されたが
    bool ifBibleAffect = false;

    // Start is called before the first frame update
    void Start()
    {
        //デフォルトで泥棒が設定されてない場合
        if (thief == null)
        {
            thief = GameObject.FindGameObjectWithTag("Thief");
        }
        //デフォルトでプレイヤーが設定されてない場合
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        //泥棒が見つかる場合
        if (thief != null)
        {
            trb = thief.GetComponent<Rigidbody>();
            ifThiefHide = thief.GetComponent<thiefHide>().ifHide;
        }
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //目標がある場合
        if (isTargeting)
            Chase();
        //目標がない場合
        else
            patrol();
    }
    private void Update()
    {
        //目標を検知
        targetCheck();
    }

    /// <summary>
    /// 目標を追跡
    /// </summary>
    void Chase()
    {
        //目標がないと何もしない
        if (targetobj == null)
        {
            return;
        }
        //バイブルに影響された時
        if(ifBibleAffect){
            move(targetpos, -chasingSpeed, Vector3.zero);
            return;
        }
        //目標が泥棒の時
        if (targetobj == thief)
        {
            move(thief.transform.position,chasingSpeed, trb.velocity.normalized);
        }
        //目標がプレイヤーの時
        if (targetobj == player)
        {
            move(player.transform.position,chasingSpeed, Vector3.zero);//目安追跡を辞めます
        }
        //行動の開始時間を記録します
        lastActTime = Time.time;
    }
    /// <summary>
    /// 殺人鬼を移動する
    /// </summary>
    /// <param name="target">移動先位置</param>
    /// <param name="speed">移動速度</param>
    /// <param name="advance_speed">目安追跡ポイントを計算</param>
    void move(Vector3 target,float speed, Vector3 advance_speed)
    {
        if((target - transform.position).sqrMagnitude < 0.2f)  return;//ターゲットに近づく時に止まる
        
        
        //毎フレイムの移動距離を計算する
        Vector3 moveSpeed = (target + advance_speed - transform.position).normalized * speed * Time.deltaTime;
        //壁を無視する移動から、transformで位置を操作する
        transform.position += moveSpeed;
        //Debug.Log(moveSpeed);
        if (target - transform.position != Vector3.zero)
            //目標までの回転を計算する
            targetQuaternion = Quaternion.LookRotation(target - transform.position, Vector3.up);
        //slrepで回転する
        transform.rotation = Quaternion.Slerp(transform.rotation, targetQuaternion, turnSpeed);
    }

    /// <summary>
    /// パトロール
    /// </summary>
    void patrol()
    {
        //バイブルに影響された時
        if (ifBibleAffect)
        {
            move(targetpos, -chasingSpeed, Vector3.zero);
            return;
        }
        if (Time.time - lastActTime > waitTime)　//前回の行動に一定時間を経過すると
        {
            generateRandomTarget();　//新しパトロール目標を生成する
            lastActTime = Time.time + (patroPosition-transform.position).magnitude / walkSpeed;　//パトロールの終了時間を計算する
        }
        move(patroPosition ,walkSpeed, Vector3.zero);//移動する
        

    }
    /// <summary>
    /// パトロール範囲内でランダムの目標を生成する
    /// </summary>
    void generateRandomTarget()
    {
        //パトロール距離を生成する
        patrolDistance = -maxPatrolRange / (Random.Range(0f, maxPatrolRange - 1f) - maxPatrolRange);
        //Debug.Log(patrolDistance);
        //パトロール方向を生成する
        Vector2 point = Random.insideUnitCircle * patrolDistance;
        //パトロールの目標を計算する
        patroPosition = new Vector3(transform.position.x + point.x, transform.position.y, transform.position.z + point.y);
        if ((patroPosition - missingPosition).magnitude > maxPatrolRange) //最大距離を超える時
        {
            //Debug.Log("離れすぎから、もう一度生成します");
            generateRandomTarget();
        }
        
    }
    /// <summary>
    /// 目標を検知
    /// </summary>
    void targetCheck()
    {
        //オブジェクトがない時
        if (thief == null || player == null)
            return;
        //デフォルトで泥棒を追う
        if (targetobj == null && !ifThiefHide)
        {
            targetobj = thief;
        }
        distancetoPlayer = Vector3.Distance(transform.position, player.transform.position);
        //Debug.Log(distancetoPlayer);
        distancetoThief = Vector3.Distance(transform.position, thief.transform.position);
        if (!ifThiefHide)  //泥棒が隠しない場合
        {
            if (distancetoPlayer < distanceOfChasingPlayer && distancetoPlayer < distancetoThief)
            {
                //プレイヤーが泥棒より殺人鬼に近づくかつ、殺人鬼の警戒範囲内に入る
                targetobj = player;
            }

            if (distancetoPlayer > distanceOfGiveUpChase || distancetoPlayer > distancetoThief)
            {
                //プレイヤーが泥棒より殺人鬼に離れた場合、または殺人鬼の最大追跡範囲に出す
                targetobj = thief;
            }
        }
        else  //泥棒が隠す場合
        {
            if (targetobj == thief)  //最初に目標をnullにします
            {
                targetobj = null;
                missingPosition = transform.position;
                isTargeting = false;
            }
            if (distancetoPlayer < distanceOfChasingPlayer)
            {
                isTargeting = true;
                targetobj = player;
            }

            if (distancetoPlayer > distanceOfGiveUpChase)
            {
                targetobj = null;
                isTargeting = false;
            }
        }
    }
    /// <summary>
    /// バイブルの効果
    /// </summary>
    /// <param name="time">効果時間</param>
    public void bible(float time)
    {
        
        StartCoroutine(bibleEffect(time));

    }
    IEnumerator bibleEffect(float time)
    {
        ifBibleAffect = true;
        targetpos = targetobj.transform.position;
        //Debug.Log("bible affected");
        yield return new WaitForSeconds(time);
        ifBibleAffect = false;
    }
}