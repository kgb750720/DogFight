using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;


public enum NetEvent { ConnectSucc = 1, ConnectFail = 2, Close = 3 };
public static class NetManager
{
    static Socket socket;
    static ByteArray readBuff;
    static Queue<ByteArray> writeQueue;
    static bool isClosing = false;

    public delegate void EventListener(string err);
    static Dictionary<NetEvent, EventListener>
        eventListener = new Dictionary<NetEvent, EventListener>();

    public delegate void MsgListener(MsgBase msgBase);
    static Dictionary<string, MsgListener>
        msgListener = new Dictionary<string, MsgListener>();

    static bool isConnecting = false;

    static List<MsgBase> msgList = new List<MsgBase>();
    static int msgCount = 0;
    readonly static int MAX_MESSAGE_FIRE = 10;

    public static bool isUsePing = true;
    public static int pingInterval = 3;
    static float lastPingTime = 0;
    static float lastPongTime = 0;


    public static void Update()
    {
        MsgUpdate();
       // PingUpdate();
    }

    private static void PingUpdate()
    {
        if (!isUsePing)
        {
            return;
        }

        if (Time.time - lastPingTime > pingInterval)
        {

            MsgPing ping = new MsgPing();
            Send(ping);
            Debug.Log("pingppp");
            lastPingTime = Time.time;
        }
        if (Time.time - lastPongTime > pingInterval * 4)
        {
            Close();
        }

    }

    private static void MsgUpdate()
    {
        if (msgCount == 0)
        {
            return;
        }
        for (int i = 0; i < MAX_MESSAGE_FIRE; i++)
        {
            MsgBase msgBase = null;
            lock (msgList)
            {
                if (msgList.Count > 0)
                {
                    msgBase = msgList[0];
                    msgList.RemoveAt(0);
                    msgCount--;
                }
            }
            if (msgBase != null)
            {
                FireMsg(msgBase.protoName, msgBase);
            }
            else
            {
                break;
            }
        }
    }

    public static void Connect(string ip, int port)
    {
        if (socket != null && socket.Connected)
        {
            Debug.Log("Connect fail,already connected");
            return;
        }
        if (isConnecting)
        {
            Debug.Log("Connect fail,isconnecting");
            return;
        }
        InitState();
        socket.NoDelay = true;
        isConnecting = true;
        socket.BeginConnect(ip, port, ConnectCallBack, socket);
    }
    private static void ConnectCallBack(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);
            Debug.Log("Socket connect succ");
            FireEvent(NetEvent.ConnectSucc, "");
            isConnecting = false;
            socket.BeginReceive(readBuff.bytes,
                readBuff.writeIdx, readBuff.remain, 0, ReceiveCallBack, socket);
        }
        catch (SocketException ex)
        {
            Debug.Log("Socket connect fail" + ex.ToString());
            FireEvent(NetEvent.ConnectFail, ex.ToString());
            isConnecting = false;
        }
    }

    private static void ReceiveCallBack(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            int count = socket.EndReceive(ar);
            if (count == 0)
            {
                Close();
                return;
            }
            readBuff.writeIdx += count;
            OnReceiveData();
            if (readBuff.remain < 8)
            {
                readBuff.MoveBytes();
                readBuff.ReSize(readBuff.length * 2);
            }
            socket.BeginReceive(readBuff.bytes,
                readBuff.writeIdx, readBuff.remain, 0, ReceiveCallBack, socket);
        }
        catch (SocketException ex)
        {
            Debug.Log("Socket receive fail" + ex.ToString());
        }
    }

    private static void OnReceiveData()
    {
        if (readBuff.length <= 2)
        {
            return;
        }
        int readIdx = readBuff.readIdx;
        byte[] bytes = readBuff.bytes;
        Int16 bodyLength = (Int16)(bytes[readIdx + 1] << 8 | bytes[readIdx]);
        if (readBuff.length < bodyLength + 2)
        {
            return;
        }
        readBuff.readIdx += 2;
        int nameCount = 0;
        string protoName = MsgBase.DecodeName(
            readBuff.bytes, readBuff.readIdx, out nameCount);

        if (protoName == "")
        {
            Debug.Log("onreceivedata decodename fail");
            return;
        }
        readBuff.readIdx += nameCount;
        int bodyCount = bodyLength - nameCount;
        MsgBase msgBase = MsgBase.Decode(protoName, readBuff.bytes, readBuff.readIdx, bodyCount);
        readBuff.readIdx += bodyCount;
        readBuff.CheckAndMoveBytes();
        lock (msgList)
        {
            msgList.Add(msgBase);
        }
        msgCount++;
        if (readBuff.length > 2)
        {
            OnReceiveData();
        }
    }

    static void InitState()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        readBuff = new ByteArray();
        writeQueue = new Queue<ByteArray>();
        isConnecting = false;
        isClosing = false;
        msgList = new List<MsgBase>();
        msgCount = 0;
        lastPingTime = Time.time;
        lastPongTime = Time.time;
        if (!msgListener.ContainsKey("MsgPong"))
        {
            AddMsgListener("MsgPong",OnMsgPong);
        }
            
    }

    private static void OnMsgPong(MsgBase msg)
    {
        lastPongTime = Time.time;
    }

    public static void AddEventListener(NetEvent netEvent, EventListener listener)
    {
        if (eventListener.ContainsKey(netEvent))
        {
            eventListener[netEvent] += listener;
        }
        else
        {
            eventListener[netEvent] = listener;
        }
    }

    public static void AddMsgListener(string msgName, MsgListener listener)
    {
        if (msgListener.ContainsKey(msgName))
        {
            msgListener[msgName] += listener;
        }
        else
        {
            msgListener[msgName] = listener;
        }
    }

    public static void RemoveEventListener(NetEvent netEvent, EventListener listener)
    {
        if (eventListener.ContainsKey(netEvent))
        {
            eventListener[netEvent] -= listener;
            if (eventListener[netEvent] == null)
            {
                eventListener.Remove(netEvent);
            }
        }
    }

    public static void RemoveMsgListener(string msgName, MsgListener listener)
    {
        if (msgListener.ContainsKey(msgName))
        {
            msgListener[msgName] -= listener;
            if (msgListener[msgName] == null)
            {
                msgListener.Remove(msgName);
            }
        }
    }

    public static void ClearMsgListener(string msgName)
    {
        if (msgListener.ContainsKey(msgName))
        {
            MsgListener []list = msgListener[msgName].GetInvocationList() as MsgListener[];
            foreach (MsgListener fuc in list)
                msgListener[msgName] -= fuc;
            msgListener.Remove(msgName);
        }
    }

    static void FireEvent(NetEvent netEvent, string err)
    {
        if (eventListener.ContainsKey(netEvent))
        {
            eventListener[netEvent](err);
        }
    }

    static void FireMsg(string msgName, MsgBase msg)
    {
        if (msgListener.ContainsKey(msgName))
        {
            msgListener[msgName](msg);
        }
    }
    public static void Close()
    {
        if (socket == null || !socket.Connected)
        {
            return;
        }
        if (isConnecting)
        {
            return;
        }
        if (writeQueue.Count > 0)
        {
            isClosing = true;
        }
        else
        {
            socket.Close();
            FireEvent(NetEvent.Close, "");
        }

    }


    public static void Send(MsgBase msg)
    {
        if (socket == null || !socket.Connected)
        {
            return;
        }
        if (isConnecting)
        {
            return;
        }
        if (isClosing)
        {
            return;
        }
        byte[] nameBytes = MsgBase.EncodeName(msg);
        byte[] bodyBytes = MsgBase.Encode(msg);
        int len = nameBytes.Length + bodyBytes.Length;
        byte[] sendBytes = new byte[2 + len];

        sendBytes[0] = (byte)(len % 256);
        sendBytes[1] = (byte)(len / 256);
        Debug.Log(sendBytes[0] + " " + sendBytes[1]);
        Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
        Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);
        ByteArray ba = new ByteArray(sendBytes);
        int count = 0;
        lock (writeQueue)
        {
            writeQueue.Enqueue(ba);
            count = writeQueue.Count;
        }
        if (count == 1)
        {
            socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallBack, socket);
        }
    }

    private static void SendCallBack(IAsyncResult ar)
    {
        Socket socket = (Socket)ar.AsyncState;
        if (socket == null || !socket.Connected|| writeQueue.Count == 0)
        {
            return;
        }
        int count = socket.EndSend(ar);
        ByteArray ba;
        lock (writeQueue)
        {
            ba = writeQueue.First();
        }
        ba.readIdx += count;
        if (ba.length == 0)
        {
            lock (writeQueue)
            {
                writeQueue.Dequeue();
                if (writeQueue.Count != 0)
                    ba = writeQueue.First();
            }
        }
        if (ba != null)
        {
            socket.BeginSend(ba.bytes, ba.readIdx, ba.length, 0, SendCallBack, socket);
        }
        else if (isClosing)
        {
            socket.Close();
        }

    }
}
