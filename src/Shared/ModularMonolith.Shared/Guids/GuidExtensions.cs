using System.Diagnostics.CodeAnalysis;

namespace ModularMonolith.Shared.Guids;

public static class GuidExtensions
{
  private const long UnixEpochMilliseconds = 62_135_596_800_000;
  private const long TicksPerMillisecond = 10_000;

  private static bool IsV7Internal(Span<byte> bytes)
  {
    return (bytes[6] & 0xF0) == 0x70;
  }

  public static bool IsV7(this Guid guid)
  {
    Span<byte> bytes = stackalloc byte[16];
    guid.TryWriteBytes(bytes, bigEndian: true, out var _);
    return IsV7Internal(bytes);
  }

  public static DateTime GetDateTime(this Guid guid)
  {
    if (guid.TryGetDateTime(out var dateTime))
    {
      return dateTime.Value;
    }
    throw new InvalidOperationException("Not compatible with Uuid version 7.");
  }

  public static bool TryGetDateTime(this Guid guid, [NotNullWhen(true)] out DateTime? dateTime)
  {
    dateTime = null;
    Span<byte> bytes = stackalloc byte[16];
    guid.TryWriteBytes(bytes, bigEndian: true, out var _);
    var isV7 = IsV7Internal(bytes);
    if (!isV7) { return false; }
    var unixMs = (long)bytes[0] << 40 | (long)bytes[1] << 32 | (long)bytes[2] << 24 | (long)bytes[3] << 16 | (long)bytes[4] << 8 | bytes[5];
    var ticks = (UnixEpochMilliseconds + unixMs) * TicksPerMillisecond;
    dateTime = new DateTime(ticks, DateTimeKind.Utc);
    return true;
  }

  public static DateTimeOffset GetDateTimeOffset(this Guid guid)
  {
    if (guid.TryGetDateTimeOffset(out var dateTimeOffset))
    {
      return dateTimeOffset.Value;
    }
    throw new InvalidOperationException("Not compatible with Uuid version 7.");
  }

  public static bool TryGetDateTimeOffset(this Guid guid, [NotNullWhen(true)] out DateTimeOffset? dateTimeOffset)
  {
    dateTimeOffset = null;
    Span<byte> bytes = stackalloc byte[16];
    guid.TryWriteBytes(bytes, bigEndian: true, out var _);
    var isV7 = IsV7Internal(bytes);
    if (!isV7) { return false; }
    var unixMs = (long)bytes[0] << 40 | (long)bytes[1] << 32 | (long)bytes[2] << 24 | (long)bytes[3] << 16 | (long)bytes[4] << 8 | bytes[5];
    var ticks = (UnixEpochMilliseconds + unixMs) * TicksPerMillisecond;
    dateTimeOffset = new DateTimeOffset(ticks, TimeSpan.Zero);
    return true;
  }
}
