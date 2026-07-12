using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class PacketHandler
{
    public static void C_PlayerInfoRequestHandler(PacketSession session, IPacket packet)
    {
        C_PlayerInfoRequest request = packet as C_PlayerInfoRequest;

        Console.WriteLine($"PlayerInfoReq: {request.playerId} / {request.name}");

        foreach (C_PlayerInfoRequest.Skill skill in request.skills)
        {
            Console.WriteLine($"Skill({skill.id})({skill.level})({skill.duration})");
        }
    }
}
