namespace advisor.Persistence;

using System.IO;
using System.IO.Compression;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public static class CompressionValueConversion {
    public static PropertyBuilder<byte[]> HasCompression(this PropertyBuilder<byte[]> propertyBuilder) {
        propertyBuilder.HasConversion
        (
            v => Compress(v),
            v => Decompress(v)
        );

        return propertyBuilder;
    }

    public static byte[] Compress(byte[] bytes) {
        if (bytes.Length == 0) {
            return bytes;
        }

        using var msi = new MemoryStream(bytes);
        using var mso = new MemoryStream();

        using (var gs = new BrotliStream(mso, CompressionLevel.Optimal)) {
            msi.CopyTo(gs, 0x10000);
        }

        return mso.ToArray();
    }

    public static byte[] Decompress(byte[] bytes) {
        if (bytes.Length == 0) {
            return bytes;
        }

        using var msi = new MemoryStream(bytes);
        using var mso = new MemoryStream();

        using (var gs = new BrotliStream(msi, CompressionMode.Decompress)) {
            gs.CopyTo(mso, 0x10000);
        }

        return mso.ToArray();
    }
}
