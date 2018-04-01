using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public delegate void OnConnectionMade();
public delegate void OnConnectionLost();
public delegate void OnDataReceived(int _protoId, byte[] _data);
public delegate void OnSocketFaild(En_SocketFaildType _faildType);

public enum En_SocketFaildType {
    None,
    ConnectFaild,
    SendFaild,
    ReceiveFaild,
}

/// <summary>
/// 包体结构：[2个字节FLAG-ushort][8个字节GUID-ulong][4个字节协议ID-int][4个字节数据长度-uint](数据)
/// 包头长度：18字节
/// </summary>
public class VSocket {

    public event OnConnectionMade onConnectionMadeEvent;
    public event OnConnectionLost onConnectionLostEvent;
    public event OnDataReceived onDataReceivedEvent;
    public event OnSocketFaild onSocketFaild;

    public ushort PACKAGE_FLAG = 0xf638;
    public ulong PACKAGE_GUID = ulong.MaxValue;
    public int PACKAGE_HEADER_LEN = 18;

    public const int PACKAGE_FLAT_LEN = 2;
    public const int PACKAGE_GUID_LEN = 8;
    public const int PACKAGE_PROTO_ID_LEN = 4;
    public const int PACKAGE_DATA_LEN = 4;

    private Socket ioSocket;
    private IPEndPoint ipEndPoint;

    private const int BUFFER_SIZE = 10240;
    private List<byte> receiveBufferQueue;
    private byte[] receiveByteArr;

    private bool connected;

    private SocketAsyncEventArgs connEventArgs;
    private List<VSocketAsyncEventArgs> sendArgsPool;
    private VSocketAsyncEventArgs receiveEventArgs;

    private bool disposed = false;

    /// <summary>
    /// the last proto we processed
    /// </summary>
    private int lastProto = -1;

    public bool IsConnected {
        get { return ioSocket != null && ioSocket.Connected; }
    }

    public string HostIP {
        get { return ipEndPoint.Address.ToString(); }
    }

    public int HostPort {
        get { return ipEndPoint.Port; }
    }

    private void InitSocket() {
        receiveByteArr = new byte[BUFFER_SIZE];
        connEventArgs = new SocketAsyncEventArgs();
        receiveEventArgs = new VSocketAsyncEventArgs();
        receiveBufferQueue = new List<byte>();
        sendArgsPool = new List<VSocketAsyncEventArgs>();
    }

    public void Connect(string _host, int _port) {
        try {
            if (ioSocket != null) {
                if (ioSocket.Connected) {
                    return;
                }
            }

            InitSocket();

            if (ioSocket == null) {
                ioSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }

            IPHostEntry hostInfo;
            bool isIP = IPAddress.TryParse(_host, out IPAddress ipAddress);
            string hostIP = string.Empty;
            if (!isIP) {
                try {
                    hostInfo = Dns.GetHostEntry(_host);
                    hostIP = hostInfo.AddressList[0].ToString();
                    ipAddress = IPAddress.Parse(hostIP);
                } catch (Exception e) {
                    Ulog.LogError("Get host entry exception: ", e);
                }
            }

            ipEndPoint = new IPEndPoint(ipAddress, _port);

            // init conn args
            connEventArgs.UserToken = ioSocket;
            connEventArgs.RemoteEndPoint = ipEndPoint;
            connEventArgs.Completed += ConnectEventHandler;

            // sync complete connect operation
            if (!ioSocket.ConnectAsync(connEventArgs)) {
                ConnectedHandler(connEventArgs);
            }

        } catch (Exception e) {
            Ulog.LogError("Connect exception ", e);
        }
    }

    public void Reconnect() {

    }

    public void Send(int _protoId, byte[] _byteData) {
        if (connected) {
            if (ioSocket == null || !ioSocket.Connected) {
                return;
            }

            if (!ioSocket.Poll(-1, SelectMode.SelectWrite)) {
                return;
            }

            byte[] packageData = PackData(_protoId, _byteData);
            VSocketAsyncEventArgs sendArgs = sendArgsPool.Find(args => args.IsUsing == false);

            if (sendArgs == null) {
                sendArgs = InitNewSendArgs();
            }

            lock (sendArgs) {
                sendArgs.IsUsing = true;
                sendArgs.SetBuffer(packageData, 0, packageData.Length);
            }

            if (ioSocket != null) {
                if (!ioSocket.SendAsync(sendArgs)) {
                    SendHandler(sendArgs);
                }
            }
        } else {
            throw new SocketException((Int32)SocketError.NotConnected);
        }

    }

    public void DisConnect() {
        if (ioSocket == null) {
            return;
        }

        try {
            if (ioSocket.Connected) {
                ioSocket.Shutdown(SocketShutdown.Both);
                ioSocket.Disconnect(false);
            }

        } catch (Exception e) {
            Ulog.LogError("disconnect exception: " + e);
        } finally {
            ioSocket.Close();
            ioSocket = null;
        }
    }




    private VSocketAsyncEventArgs InitNewSendArgs() {
        VSocketAsyncEventArgs sendArgs = new VSocketAsyncEventArgs();
        sendArgs.Completed += IOCompleteHandler;
        sendArgs.UserToken = ioSocket;
        sendArgs.RemoteEndPoint = ipEndPoint;
        sendArgs.IsUsing = false;
        lock (sendArgsPool) {
            sendArgsPool.Add(sendArgs);
        }
        return sendArgs;
    }

    private void InitReceiveArgs(SocketAsyncEventArgs _args) {
        receiveEventArgs.Completed += IOCompleteHandler;
        receiveEventArgs.UserToken = _args.UserToken;
        receiveEventArgs.SetBuffer(receiveByteArr, 0, receiveByteArr.Length);

        // start receive data
        if (!ioSocket.ReceiveAsync(receiveEventArgs)) {
            ReceiveHandler(receiveEventArgs);
        }
    }

    
    private byte[] PackData(int _protoId, byte[] _data) {
        int dataLen = _data.Length;
        List<byte> packageData = new List<byte>();
        packageData.AddRange(BitConverter.GetBytes(PACKAGE_FLAG));
        packageData.AddRange(BitConverter.GetBytes(PACKAGE_GUID));
        packageData.AddRange(BitConverter.GetBytes(_protoId));
        packageData.AddRange(BitConverter.GetBytes(dataLen));

        if(dataLen > 0) {
            packageData.AddRange(_data);
        }

        return packageData.ToArray();
    }


    private void UnpackReceivedDataFromBuffer() {
        while(receiveBufferQueue.Count > PACKAGE_HEADER_LEN) {
            byte[] headerBytes = receiveBufferQueue.GetRange(0, PACKAGE_HEADER_LEN).ToArray();
            ushort flag = BitConverter.ToUInt16(headerBytes, 0);
            ulong guid = BitConverter.ToUInt64(headerBytes, PACKAGE_FLAT_LEN);
            int protoId = BitConverter.ToInt32(headerBytes, PACKAGE_FLAT_LEN + PACKAGE_GUID_LEN);
            lastProto = protoId;
            int dataLen = BitConverter.ToInt32(headerBytes, PACKAGE_FLAT_LEN + PACKAGE_GUID_LEN + PACKAGE_PROTO_ID_LEN);
            
            if (PACKAGE_FLAG != flag) {
                Ulog.LogError(this, "Flag error protoid: ", protoId, "flag:", PACKAGE_FLAG, "received flag:", flag);
                break;
            }

            if(protoId < 0) {
                Ulog.LogError(this, "ProtoId error :", protoId);
                break;
            }

            if(dataLen < 0) {
                Ulog.LogError(this, "Data length error protoId:", protoId, "data length:", dataLen);
                break;
            }

            if(PACKAGE_GUID == ulong.MaxValue) {
                PACKAGE_GUID = guid;
            }
            
            // Received a complete package , parse the package
            if(receiveBufferQueue.Count - PACKAGE_HEADER_LEN >= dataLen) {
                byte[] bodyDataBytes = receiveBufferQueue.GetRange(PACKAGE_HEADER_LEN, dataLen).ToArray();
                lock (receiveBufferQueue) {
                    receiveBufferQueue.RemoveRange(0, PACKAGE_HEADER_LEN + dataLen);
                }

                onDataReceivedEvent(protoId, bodyDataBytes);
            } else {
                // Received bytes not a complete yet, just wait
                break;
            }
        }
    }

    private void ConnectEventHandler(object _sender, SocketAsyncEventArgs _args) {
        ConnectedHandler(_args);
    }

    private void ConnectedHandler(SocketAsyncEventArgs _args) {
        connected = (_args.SocketError == SocketError.Success);
        if (connected) {
            onConnectionMadeEvent();
            InitNewSendArgs();
            InitReceiveArgs(_args);
        } else {
            ConnectFaildHandler(_args);
        }
    }

    private void IOCompleteHandler(object _sender, SocketAsyncEventArgs _args) {
        switch (_args.LastOperation) {
            case SocketAsyncOperation.Receive: {
                    ReceiveHandler(_args);
                }
                break;
            case SocketAsyncOperation.Send: {
                    VSocketAsyncEventArgs args = (VSocketAsyncEventArgs)_args;
                    args.IsUsing = false;
                    SendHandler(args);
                }
                break;
            default:
                throw new ArgumentException("The last operation completed on the socket was not a receive or send:" + _args.LastOperation);
        }
    }


    private void SendHandler(SocketAsyncEventArgs _args) {
        if(_args.SocketError != SocketError.Success) {
            SendFaildHandler(_args);
        }
    }

    /// <summary>
    /// NOTE：_args.Buffer lengh is constant, 
    /// _args.BytesTransferred is dynamic, it's the real data we received
    /// so, add the whole buffer, and keep real received data, and remove other bytes
    /// </summary>
    /// <param name="_args"></param>
    private void ReceiveHandler(SocketAsyncEventArgs _args) {
        try {
            Socket token = (Socket)_args.UserToken;
            if(_args.SocketError == SocketError.Success && _args.BytesTransferred > 0) {
                int byteLen = _args.BytesTransferred;
                int removeLen = _args.Buffer.Length - byteLen;

                lock (receiveBufferQueue) {
                    receiveBufferQueue.AddRange(_args.Buffer);
                    receiveBufferQueue.RemoveRange(receiveBufferQueue.Count - removeLen, removeLen);
                }

                UnpackReceivedDataFromBuffer();

                // Sync received data, direct to process
                if (!token.ReceiveAsync(_args)) {
                    ReceiveHandler(_args);
                }

            } else {
                ReceiveFaildHandler(_args);
            }
        }catch(Exception e) {
            receiveBufferQueue.Clear();
            Ulog.LogError("last proto error: ", lastProto, e);
        }
    }


    private void ConnectFaildHandler(SocketAsyncEventArgs _args) {
        SocketFaildHandler(_args, En_SocketFaildType.ConnectFaild);
    }

    private void SendFaildHandler(SocketAsyncEventArgs _args) {
        SocketFaildHandler(_args, En_SocketFaildType.SendFaild);
    }

    private void ReceiveFaildHandler(SocketAsyncEventArgs _args) {
        SocketFaildHandler(_args, En_SocketFaildType.ReceiveFaild);
    }

    private void SocketFaildHandler(SocketAsyncEventArgs _args, En_SocketFaildType _faildType) {
        Socket socket = (Socket)_args.UserToken;
        if (socket.Connected) {
            try {
                socket.Shutdown(SocketShutdown.Both);
            }catch(Exception e) {
                Ulog.LogError("Faild exception: ", e);
            } finally {
                if (socket.Connected) {
                    socket.Close();
                    connected = false;
                }
            }
        }

        foreach(SocketAsyncEventArgs args in sendArgsPool) {
            args.Completed -= IOCompleteHandler;
        }
        receiveEventArgs.Completed -= IOCompleteHandler;

        onSocketFaild(_faildType);
    }


    ~VSocket() {
        Dispose(false);
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool _disposing) {
        if (!disposed) {
            if (_disposing) {

            }

            if(ioSocket == null) {
                return;
            }

            DisConnect();
        }

        disposed = true;
    }
}
