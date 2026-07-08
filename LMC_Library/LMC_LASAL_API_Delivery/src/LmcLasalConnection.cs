using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace LmcLasalMotionApi
{
    public sealed class LMCConnection : IDisposable
    {
        private readonly object sync=new object(); private TcpClient client;
        public void LMC_RpcInitConnection(string remoteAddress,int remotePort,string localAddress)
        { LMC_CloseConnection(false);client=new TcpClient(new IPEndPoint(IPAddress.Parse(localAddress),0));client.NoDelay=true;client.ReceiveTimeout=3000;client.SendTimeout=3000;client.Connect(IPAddress.Parse(remoteAddress),remotePort); }
        public void LMC_CloseConnection(){LMC_CloseConnection(false);}
        private void LMC_CloseConnection(bool send){if(client==null)return;try{if(send&&client.Connected)Exchange(new byte[]{0x5D,0x40,0,0,1,0,0,0,0});}catch{}finally{client.Close();client=null;}}
        internal byte[] Exchange(byte[] request){lock(sync){if(client==null||!client.Connected)throw new InvalidOperationException("LMC LASAL connection is not open.");var s=client.GetStream();s.Write(request,0,request.Length);var h=ReadExact(s,8);var n=LMC_Frame.U16(h,2);var p=n==0?new byte[0]:ReadExact(s,n);var all=new byte[8+n];Buffer.BlockCopy(h,0,all,0,8);if(n>0)Buffer.BlockCopy(p,0,all,8,n);return all;}}
        private static byte[] ReadExact(NetworkStream s,int count){var b=new byte[count];var o=0;while(o<count){var n=s.Read(b,o,count-o);if(n<=0)throw new EndOfStreamException();o+=n;}return b;}
        internal static LMC_Response Parse(byte[] raw){var r=new LMC_Response{Raw=raw};if(raw!=null&&raw.Length>=12){r.Status=LMC_Frame.U16(raw,raw.Length-4);r.ErrorId=unchecked((short)LMC_Frame.U16(raw,raw.Length-2));}return r;}
        public void Dispose(){LMC_CloseConnection(false);}
    }
}
