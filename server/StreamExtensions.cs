namespace advisor;

using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public static class StreamExtensions {
    public static async Task<byte[]> ReadAllBytesAsync(this Stream stream, CancellationToken cancellationToken = default) {
        await using var ms = new MemoryStream();
        using var lease = MemoryPool<byte>.Shared.Rent(0x10000);

        Memory<byte> buffer = lease.Memory;

        int bytesRed;
        do {
            bytesRed = await stream.ReadAsync(buffer, cancellationToken);
            await ms.WriteAsync(buffer[..bytesRed], cancellationToken);
        }
        while (bytesRed > 0);

        await ms.FlushAsync(cancellationToken);

        return ms.ToArray();
    }
    public static byte[] ReadAllBytes(this Stream stream) {
        using var ms = new MemoryStream();
        var buffer = ArrayPool<byte>.Shared.Rent(0x10000);

        try {
            int bytesRed;
            do {
                bytesRed = stream.Read(buffer, 0, buffer.Length);
                ms.Write(buffer, 0, bytesRed);
            }
            while (bytesRed > 0);

            ms.Flush();

            return ms.ToArray();
        }
        finally {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public static Task<byte[]> ReadAllBytesAsync(this FileInfo fileInfo, CancellationToken cancellationToken = default) {
        using var stream = fileInfo.OpenRead();
        return stream.ReadAllBytesAsync(cancellationToken);
    }

    public static byte[] ReadAllBytes(this FileInfo fileInfo) {
        using var stream = fileInfo.OpenRead();
        return stream.ReadAllBytes();
    }
}
