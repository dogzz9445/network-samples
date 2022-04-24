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

```