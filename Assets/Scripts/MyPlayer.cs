using ServerCore;
using System;
using System.Collections;
using UnityEngine;

public class MyPlayer : Player
{
    NetworkManager networkManager;

    void Start()
    {
        StartCoroutine(CoSendPacket());
        networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
    }

    IEnumerator CoSendPacket()
    {
        while (true)
        {
            yield return new WaitForSeconds(3.0f);

            C_Move movePacket = new();
            movePacket.posX = UnityEngine.Random.Range(-50, 50);
            movePacket.posY = 0;
            movePacket.posZ = UnityEngine.Random.Range(-50, 50);
            networkManager.Send(movePacket.Write());
        }
    }
}
