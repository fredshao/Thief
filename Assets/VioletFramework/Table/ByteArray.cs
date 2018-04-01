using FunEngine.Interfaces;
using System;

namespace FunEngine.Utils {
    public class ByteArray : IDisposable, IPoolableObject {

        public enum EndianEnum {
            Big = 1,
            Little = 2,
        }

        private static byte[] _temp = new byte[8];

        //获取字符串长度
        public static byte[] GetUTFBytes(string value) {
            return System.Text.Encoding.UTF8.GetBytes(value);
        }

        public static string ToUTFString(byte[] bytes) {
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        //------------------------------------------------------------
        //字节数组
        private byte[] _buffer;
        //增量
        private int _bufferIncrement;
        //当前的读取/写入位置
        private int _position;
        //有效字节长度
        private int _avaliableLength;
        //字节序
        private EndianEnum _endian;

        public ByteArray(int length = 32,
                         int bufferIncrement = 32) {
            this._buffer = new byte[length];
            this._bufferIncrement = bufferIncrement;
            this._position = 0;
            this._avaliableLength = 0;
            this._endian = EndianEnum.Little;
        }

        //引用一个字节数组
        public ByteArray(byte[] bytes,
                         int length = 0,
                         int bufferIncrement = 32,
                         EndianEnum endian = EndianEnum.Little) {
            this._bufferIncrement = bufferIncrement;
            this._position = 0;
            this._avaliableLength = 0;
            this._endian = endian;

            //计算字节数组长度
            this._buffer = bytes;
            this._position = 0;
            if(length == 0) {
                this._avaliableLength = this._buffer.Length;
            } else {
                this._avaliableLength = length;
            }
        }

        ~ByteArray() {
            this.Dispose();
        }

        public byte[] GetBuffer{
            get { return this._buffer; }
        }

        public void Dispose() {
            if(this._buffer != null)
                Array.Clear(this._buffer, 0, this._buffer.Length);
            this._buffer = null;
        }

        public void FromPool() {
        }

        public void ToPool() {
            this.Clear();
        }

        // ------------------------------------------------------------------------------
        // 属性
        // ------------------------------------------------------------------------------
        public int bufferIncrement {
            get {
                return this._bufferIncrement;
            }
            set {
                this._bufferIncrement = value;
            }
        }

        public EndianEnum endian {
            get {
                return this._endian;
            }
            set {
                this._endian = value;
            }
        }

        public int Position {
            get {
                return this._position;
            }
            set {
                if(value < 0)
                    value = 0;
                if(value > this._avaliableLength)
                    value = this._avaliableLength;
                this._position = value;
            }
        }

        public int bufferLength {
            get {
                return this._buffer.Length;
            }
        }

        public byte[] bytes {
            get {
                byte[] bytes = new byte[this._avaliableLength];
                //Array.Copy(this.buffer, 0, bytes, 0, this.avaliableLength);
                Buffer.BlockCopy(this._buffer, 0, bytes, 0, this._avaliableLength);
                return bytes;
            }
        }


        public int Length {
            get {
                return this._avaliableLength;
            }
        }

        public int bytesAvaliable {
            get {
                return this._avaliableLength - this._position;
            }
        }

        public void Clear() {
            this._position = 0;
            this._avaliableLength = 0;
            if(this._buffer.Length > 0)
                Array.Clear(this._buffer, 0, this._buffer.Length);
        }

        // ------------------------------------------------------------------------------
        // ------------------------------------------------------------------------------
        // ------------------------------------------------------------------------------
        // 各种 Read/Write 方法
        // ------------------------------------------------------------------------------
        public void WriteByte(byte value) {
            //如果容量不够了
            if(this._position >= this._buffer.Length) {
                Array.Resize<byte>(ref this._buffer, this._buffer.Length + this._bufferIncrement);
            }
            //
            this._buffer[this._position] = value;
            this._position++;
            if(this._position > this._avaliableLength)
                this._avaliableLength = this._position;
        }

        public byte ReadByte() {
            if(this._position < this._avaliableLength) {
                byte value = this._buffer[this._position];
                this.Position++;
                return value;
            }
            //读到文件尾
            throw new Exception("读到尾部");
        }

        //写入字节数组
        public void WriteBytes(byte[] bytes, int offset = 0, int length = 0) {
            if(length == 0)
                length = bytes.Length - offset;
            //从offset索引开始读取length个字节
            this.write(bytes, offset, length, this.reverse);
        }

        //TODO
        public byte[] ReadBytes(int length = 0) {
            if(length == 0)
                length = this.bytesAvaliable;
            byte[] bytes = new byte[length];
            //从Position索引开始读取length个字节，写入到offset位置
            this.read(bytes, 0, length, this.reverse);
            return bytes;
        }

        public byte[] ReadBytes(byte[] bytes, int offset = 0, int length = 0) {
            if(length == 0) {
                length = this.bytesAvaliable;
                if(offset + length > bytes.Length)
                    length = bytes.Length - offset;
            }
            //
            this.read(bytes, offset, length, this.reverse);
            return bytes;
        }

        //-------------------------
        //1 bytes
        public void WriteBoolean(bool b) {
            this.WriteByte(b ? (byte)1 : (byte)0);
        }

        public bool ReadBoolean() {
            return this.ReadByte() == 0 ? false : true;
        }

        //-------------------------
        //2 bytes
        public void WriteShort(short value) {
            byte[] bytes = BitConverter.GetBytes(value);
            this.write(bytes, 0, bytes.Length, this.reverse);
        }

        public short ReadShort() {
            this.read(_temp, 0, 2, this.reverse);
            return BitConverter.ToInt16(_temp, 0);
        }

        public void WriteUnsignedShort(ushort value) {
            byte[] bytes = BitConverter.GetBytes(value);
            this.write(bytes, 0, bytes.Length, this.reverse);
        }

        public ushort ReadUnsignedShort() {
            this.read(_temp, 0, 2, this.reverse);
            return BitConverter.ToUInt16(_temp, 0);
        }

        //-------------------------
        //4 bytes
        public void WriteInt(int value) {
            byte[] bytes = BitConverter.GetBytes(value);
            this.write(bytes, 0, bytes.Length, this.reverse);
        }

        public int ReadInt() {
            this.read(_temp, 0, 4, this.reverse);
            return BitConverter.ToInt32(_temp, 0);
        }

        public void WriteUnsignedInt(uint value) {
            byte[] bytes = BitConverter.GetBytes(value);
            this.write(bytes, 0, bytes.Length, this.reverse);
        }

        public uint ReadUnsignedInt() {
            this.read(_temp, 0, 4, this.reverse);
            return BitConverter.ToUInt32(_temp, 0);
        }

        //-------------------------
        //8 bytes
        public void WriteLong(long value) {
            byte[] bytes = BitConverter.GetBytes(value);
            this.write(bytes, 0, bytes.Length, this.reverse);
        }

        public long ReadLong() {
            this.read(_temp, 0, 8, this.reverse);
            return BitConverter.ToInt64(_temp, 0);
        }

        public void WriteUnsignedLong(ulong value) {
            byte[] bytes = BitConverter.GetBytes(value);
            this.write(bytes, 0, bytes.Length, this.reverse);
        }

        public ulong ReadUnsignedLong() {
            this.read(_temp, 0, 4, this.reverse);
            return BitConverter.ToUInt64(_temp, 0);
        }

        //-------------------------
        //4 bytes
        public void WriteFloat(float value) {
            byte[] bytes = BitConverter.GetBytes(value);
            this.write(bytes, 0, bytes.Length, this.reverse);
        }

        public float ReadFloat() {
            this.read(_temp, 0, 4, this.reverse);
            return BitConverter.ToSingle(_temp, 0);
        }

        //-------------------------
        //8 bytes
        public void WriteDouble(double value) {
            byte[] bytes = BitConverter.GetBytes(value);
            this.write(bytes, 0, bytes.Length, this.reverse);
        }

        public double ReadDouble() {
            this.read(_temp, 0, 8, this.reverse);
            return BitConverter.ToDouble(_temp, 0);
        }

        //---------------------------------
        public void WriteUTF(string value) {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(value);
            this.WriteInt(bytes.Length);
            this.write(bytes, 0, bytes.Length, false);
        }

        public void WriteUTFBytes(string value) {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(value);
            this.write(bytes, 0, bytes.Length, false);
        }

        public void WriteUTFBytes(string value, short length) {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(value);
            //
            if(bytes.Length < length) {
                this.write(bytes, 0, bytes.Length, false);
                //补0
                byte[] zBytes = new byte[length - bytes.Length];
                this.write(zBytes, 0, zBytes.Length, false);
            } else {
                this.write(bytes, 0, length, false);
            }
        }

        public string ReadUTF() {
            int length = this.ReadInt();
            byte[] bytes = new byte[length];
            this.read(bytes, 0, length, false);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        public string ReadUTFBytes(int length) {
            byte[] bytes = new byte[length];
            this.read(bytes, 0, length, false);
            //
            // string s = "";
            // for (int i = 0; i < bytes.Length; i++)
            //     s += Convert.ToString(bytes[i]);
            // Debug.Log(s);
            //
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        //----------------------------------------------------------------------
        //----------------------------------------------------------------------
        //----------------------------------------------------------------------
        private bool reverse {
            get {
                return (this._endian == EndianEnum.Little && !BitConverter.IsLittleEndian) || (this._endian == EndianEnum.Big && BitConverter.IsLittleEndian);
            }
        }

        //从offset索引开始读取length个字节
        private void write(byte[] bytes, int offset, int length, bool reverse) {
            int toPosition = this._position + length;
            if(toPosition > this._buffer.Length) {
                // int len = this._buffer.Length;
                // while (len < toPosition)
                //     len += this._bufferIncrement;
                // Array.Resize<byte>(ref this._buffer, len);
                //
                Array.Resize<byte>(ref this._buffer,
                                   this._bufferIncrement * (int)Math.Ceiling((double)toPosition / (double)this._bufferIncrement));
            }
            //
            if(reverse) {
                //倒序写入
                for(int i = offset + length - 1; i >= offset; i--) {
                    this._buffer[this._position] = bytes[i];
                    this._position++;
                }
            } else {
                //Array.Copy(bytes, offset, this.buffer, this.position, length);
                Buffer.BlockCopy(bytes, offset, this._buffer, this._position, length);
            }
            //
            this._position = toPosition;
            if(this._position > this._avaliableLength)
                this._avaliableLength = this._position;
        }


        //从Position索引开始读取length个字节，写入到offset位置
        private void read(byte[] bytes, int offset, int length, bool reverse) {
            //检测有没有足够的位置可读取
            int toPosition = this._position + length;
            if(toPosition <= this._avaliableLength) {
                //如果需要转换字节序
                if(reverse) {
                    //倒序读取
                    this.Position = toPosition - 1;
                    for(int i = offset; i < offset + length; i++) {
                        bytes[i] = this._buffer[this._position];
                        this.Position--;
                    }
                } else {
                    //Array.Copy(this.buffer, this.position, bytes, offset, length);
                    Buffer.BlockCopy(this._buffer, this._position, bytes, offset, length);
                }
                //
                this._position = toPosition;
            } else {
                throw new Exception("读到尾部");
            }
        }
    }
}