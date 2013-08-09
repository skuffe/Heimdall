using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using SnmpSharpNet;

namespace DataCollectionHost
{
    class SNMPInterface
    {
        Socket socket = null;
        IPEndPoint peerIP;
        byte[] inbuffer;

        public SNMPInterface()
        {
            initializeSocket();
        }

        public void threadWrapper()
        {
            registerReceiveOperation();
        }

        public bool initializeSocket()
        {
            if (socket != null)
                stopSocket();

            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                EndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 162);
                socket.Bind(localEndPoint);
                return true;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("SNMP Interface", "SNMP: trap receiver socket initialization failed with error: " + ex.Message);
                return false;
            }
        }

        public void stopSocket()
        {
            if (socket != null)
            {
                socket.Close();
                socket = null;
            }
        }

        public bool registerReceiveOperation()
        {
            if (socket == null)
                return false;

            try
            {
                peerIP = new IPEndPoint(IPAddress.Any, 0);

                EndPoint ep = (EndPoint)peerIP;
                inbuffer = new byte[64 * 1024];
                socket.BeginReceiveFrom(inbuffer, 0, 64 * 1024, SocketFlags.None, ref ep, new AsyncCallback(receiveCallback), socket);
            }
            catch(Exception ex)
            {
                EventLog.WriteEntry("SNMP Interface", "SNMP: Registering receive opreation failed with message: " + ex.Message);
            }
            if (socket == null)
                return false;

            return true;
        }

        public void receiveCallback(IAsyncResult result)
        {
            Socket sock = (Socket)result.AsyncState;

            peerIP = new IPEndPoint(IPAddress.Any, 0);
            int inlen;

            try
            {
                EndPoint ep = (EndPoint)peerIP;
                inlen = sock.EndReceiveFrom(result, ref ep);
                peerIP = (IPEndPoint)ep;
            }
            catch (Exception ex)
            {
                if (socket != null)
                    EventLog.WriteEntry("SNMP Interface", "SNMP: Receive operation failed with message " + ex.Message);
                inlen = -1;
            }

            if(socket == null)
                return;

            if (inlen <= 0)
            {
                registerReceiveOperation();
                return;
            }

            int packetVersion = SnmpPacket.GetProtocolVersion(inbuffer, inlen);

            if (packetVersion == (int)SnmpVersion.Ver1)
            {
                SnmpV1TrapPacket pkt = new SnmpV1TrapPacket();
                try
                {
                    pkt.decode(inbuffer, inlen);
                }
                catch (Exception ex)
                {
                    EventLog.WriteEntry("SNMP Interface", "SNMP: Error parsing SNMPv1 Trap " + ex.Message);
                }
                if (pkt != null)
                {
                    foreach (Vb vb in pkt.Pdu.VbList)
                    {
                        //do stuff with the information
                    }
                }
            }
            else if (packetVersion == (int)SnmpVersion.Ver2)
            {
                SnmpV2Packet pkt = new SnmpV2Packet();
                try
                {
                    pkt.decode(inbuffer, inlen);
                }
                catch (Exception ex)
                {
                    EventLog.WriteEntry("SNMP Interface", "SNMP: Error parsing SNMPv1 Trap " + ex.Message);
                }

                if (pkt != null)
                {
                    foreach (Vb vb in pkt.Pdu.VbList)
                    {
                        //Do stuff with the information
                    }
                    if (pkt.Pdu.Type != PduType.V2Trap)
                    {
                        SnmpV2Packet response = pkt.BuildInformResponse();
                        byte[] buf = response.encode();
                        socket.SendTo(buf, (EndPoint)peerIP);
                    }
                }
            }
            registerReceiveOperation();
        }
    }
}
