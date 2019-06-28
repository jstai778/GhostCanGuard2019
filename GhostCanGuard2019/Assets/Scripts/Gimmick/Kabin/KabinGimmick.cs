﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class KabinGimmick : GimmickBase
{
    private Vector3 throwPos = Vector3.zero;   // 花瓶が飛んでいく場所

    [SerializeField]
    private float power = 400f; // 花瓶を押す力
    [SerializeField]
    private GameObject player = null;
    [SerializeField]
    private float speed = 0.01f;

    private bool kabinSetPos = false;

    private bool kabinFinish = false;


    private Rigidbody rb = null;

    protected override void Start()
    {
        base.Start();
        GimmickEventSetUp(EventTriggerType.PointerDown, GimmickEventOpen);

        rb = GetComponent<Rigidbody>();
    }

    private void KabinGimmickSetup()
    {
        Vector3 playerPosition = player.transform.position;
        playerPosition += player.transform.forward;

        KabinToPlayer( playerPosition );
       
    }

    private void KabinGimmickAction()
    {
        KabinGimmickSetup();
        
        if (!kabinSetPos) return;
        if (!Input.GetMouseButtonDown(0)) return;

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f;
        throwPos = Camera.main.ScreenToWorldPoint(mousePos);
        Debug.Log(throwPos);
        throwPos.y = 1.0f;

        // 力を加える方向をきめる
        Vector3 direction = (throwPos - this.transform.position).normalized;
        Debug.Log(direction);
        Debug.Log("thorw!");
        rb.AddForce(direction * power);
        throwPos = Vector3.zero;

        GimmickManager.Instance.ClearGimmick();
        GimmickUIClose();
        ClearGimmickEvent();

    }

    // ButtonのonClickで呼ぶ関数
    public void ClickUIStart()
    {
        GimmickManager.Instance.SetGimmickAction( () => KabinGimmickAction() );
        GimmickUIsOnOff(false);
    }

    private void KabinToPlayer(Vector3 pPos)
    {
        while(transform.position != pPos)
        {
            transform.position = Vector3.MoveTowards(transform.position, pPos, speed);
        }
        kabinSetPos = true;
    }
}
