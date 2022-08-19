using System;
using System.IO;

namespace AGSUnpacker.Shared
{
  internal class ReadOnlySubStream : Stream
  {
    private Stream _superStream;
    private long _superStreamOffsetStart;
    private long _superStreamOffsetEnd;
    private long _superStreamPosition;

    public ReadOnlySubStream(Stream superStream, long offset, long length)
    {
      if (superStream == null)
        throw new ArgumentNullException(nameof(superStream));

      _superStream = superStream;
      _superStreamOffsetStart = offset;
      _superStreamOffsetEnd = offset + length;
      _superStreamPosition = offset;
    }

    public override bool CanRead => _superStream.CanRead;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => _superStreamOffsetEnd - _superStreamOffsetStart;

    public override long Position
    {
      get => _superStreamPosition - _superStreamOffsetStart;
      set => throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
      if (_superStream.Position != _superStreamPosition)
        _superStream.Seek(_superStreamPosition, SeekOrigin.Begin);

      if (_superStreamPosition + count > _superStreamOffsetEnd)
        count = (int)(_superStreamOffsetEnd - _superStreamPosition);

      int bytesRead = _superStream.Read(buffer, offset, count);

      _superStreamPosition += bytesRead;

      return bytesRead;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
      throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
      throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
      throw new NotSupportedException();
    }

    public override void Flush()
    {
      throw new NotSupportedException();
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
    }
  }
}
