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

        int bytesRead;
        do {
            bytesRead = await stream.ReadAsync(buffer, cancellationToken);
            await ms.WriteAsync(buffer.Slice(0, bytesRead), cancellationToken);
        }
        while (bytesRead > 0);

        await ms.FlushAsync();

        return ms.ToArray();
    }

    public static Task<byte[]> ReadAllBytesAsync(this FileInfo fileInfo, CancellationToken cancellationToken = default) {
        using var stream = fileInfo.OpenRead();
        return stream.ReadAllBytesAsync(cancellationToken);
    }
}
