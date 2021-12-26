using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class FacepunchLobby : SocketManager
{
    
    public void HostSocketServer(){
        //MyServer server = SteamNetworkingSockets.CreateNormalSocket<MyServer>(Data.NetAddress.AnyIp(21893));
        SteamNetworkingSockets.CreateNormalSocket<FacepunchLobby>(Steamworks.Data.NetAddress.AnyIp(7777));
    }

}
