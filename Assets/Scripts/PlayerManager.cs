using System.Collections.Generic;
using UnityEngine;

public class PlayerManager
{
    MyPlayer player;
    Dictionary<int, Player> players = new();

    public static PlayerManager Instance { get; } = new PlayerManager();

    public void Add(S_PlayerList packet)
    {
        Object obj = Resources.Load("Player");

        foreach(var p in packet.players)
        {
            GameObject go = Object.Instantiate(obj) as GameObject;

            if(p.isSelf)
            {
                MyPlayer player =  go.AddComponent<MyPlayer>();
                player.PlayerId = p.PlayerId;
                player.transform.position = new Vector3(p.posX, p.posY, p.posZ);
                this.player = player;
            }
            else
            {
                Player player = go.AddComponent<Player>();
                player.PlayerId = p.PlayerId;
                player.transform.position = new Vector3(p.posX, p.posY, p.posZ);
                players.Add(p.PlayerId, player);
            }
        }
    }
    public void Move(S_BroadcastMove packet)
    {
        if (packet.PlayerId == player.PlayerId)
        {
            player.transform.position = new(packet.posX, packet.posY, packet.posZ);
        }
        else
        {
            Player player = null;
            if (players.TryGetValue(packet.PlayerId, out player))
            {
                player.transform.position = new(packet.posX, packet.posY, packet.posZ);
            }
        }
    }

    public void EnterGame(S_BroadcastEnterGame packet)
    {
        if (this.player.PlayerId == packet.PlayerId)
            return;

        Object obj = Resources.Load("Player");
        GameObject go = Object.Instantiate(obj) as GameObject;

        Player player = go.AddComponent<Player>();
        player.PlayerId = packet.PlayerId;
        player.transform.position = new Vector3(packet.posX, packet.posY, packet.posZ);
        players.Add(packet.PlayerId, player);
    }

    public void LeaveGame(S_BroadcastLeaveGame packet)
    {
        if(player.PlayerId == packet.playerId)
        {
            GameObject.Destroy(player.gameObject);
            player = null;
        }
        else
        {
            Player player = null;
            if(players.TryGetValue(packet.playerId, out player))
            {
                GameObject.Destroy(player.gameObject);
                players.Remove(packet.playerId);
            }
        }
    }
}
