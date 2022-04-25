``` csharp
public event EventHandler<Message> MessageReceived;

public Initialize()
{
    // 제대로 동작함
    Module.MessageReceived += OnMessageReceived;

    // 제대로 동작안함
    Module.MessageReceived += MessageReceived;
}

public void OnMessageReceived(object sender, Message message)
{
    MessageReceived?.Invoke(sender, message);
}


protected override void OnReceived(byte[] buffer, long offset, long size)
{
    string aaa = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
    _bufferedStream.Write(buffer, (int)offset, (int)size);
    _bufferedStream.Position = 0;
    _bufferedStream.Flush();

    int readOffset = 0;
    byte[] length = new byte[4];
    byte[] bufferToRead = new byte[65536];
    while (_bufferedStream.CanRead)
    {
        if (size < readOffset + 4)
        {
            return;
        }
        int nReadLengthBytes = _bufferedStream.Read(length, 0, 4);
        if (nReadLengthBytes < 4)
        {
            _bufferedStream.Seek(-nReadLengthBytes, SeekOrigin.Current);
            return;
        }
        readOffset = readOffset + 4;

        int nExpectBytes = BitConverter.ToInt32(length, 0);
        if (size < readOffset + nExpectBytes)
        {
            _bufferedStream.Seek(-nReadLengthBytes, SeekOrigin.Current);
            return;
        }
        int nReadBytes = _bufferedStream.Read(bufferToRead, 4, nExpectBytes);
        if (nReadBytes < nExpectBytes)
        {
            _bufferedStream.Seek(-(nReadLengthBytes + nReadBytes), SeekOrigin.Current);
            return;
        }
        readOffset = readOffset + nReadBytes;
        _bufferedStream.Flush();

        string data = Encoding.UTF8.GetString(bufferToRead, 4, nReadBytes);
        Message message = JsonConvert.DeserializeObject<Message>(data);
        MessageReceived?.Invoke(this, message);
    }
}

```