namespace Virgil.DropBox.Client.Encryption
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public static class StreamHelpers
    {
        public static Task WriteAsync(this Stream srteam, byte[] bytes)
        {
            return srteam.WriteAsync(bytes, 0, bytes.Length);
        }

        public static async Task<byte[]> TryReadExactly(this Stream stream, int count, byte[] buffer)
        {
            int offset = 0;
            while (offset < count)
            {
                int read = await stream.ReadAsync(buffer, offset, count - offset);
                offset += read;
                if (read == 0)
                {
                    var result = new byte[offset];
                    Array.Copy(buffer, result, offset);
                    return result;
                }
            }
            
            return buffer;
        }
    }
}