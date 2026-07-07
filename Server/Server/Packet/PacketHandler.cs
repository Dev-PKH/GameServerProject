using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class PacketHandler
    {
        public static void PlayerInfoReqeustHandler(PacketSession session, IPacket packet)
        {
            PlayerInfoRequest request = packet as PlayerInfoRequest;

            Console.WriteLine($"PlayerInfoReq: {request.playerId} / {request.name}");

            foreach (PlayerInfoRequest.Skill skill in request.skills)
            {
                Console.WriteLine($"Skill({skill.id})({skill.level})({skill.duration})");
            }
        }
    }
}
