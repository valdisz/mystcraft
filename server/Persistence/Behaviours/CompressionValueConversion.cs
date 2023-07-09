namespace advisor.Persistence;

using System.IO;
using System.IO.Compression;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public static class CompressionValueConversion {
    public static PropertyBuilder<byte[]> HasCompression(this PropertyBuilder<byte[]> propertyBuilder) {
        propertyBuilder.HasConversion
        (
            v => CompressDecompress(v, CompressionMode.Compress),
            v => CompressDecompress(v, CompressionMode.Decompress)
        );

        return propertyBuilder;
    }

    private static byte[] CompressDecompress(byte[] bytes, CompressionMode mode) {
        using var msi = new MemoryStream(bytes);
        using var mso = new MemoryStream();
        using var gs = mode == CompressionMode.Compress
            ? new BrotliStream(mso, CompressionLevel.Optimal)
            : new BrotliStream(mso, mode);

        msi.CopyTo(gs, 0x10000);

        return mso.ToArray();
    }
}
