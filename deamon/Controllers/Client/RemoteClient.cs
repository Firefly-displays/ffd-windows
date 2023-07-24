namespace deamon;

public static class RemoteClient
{
    private static Client? instance;
    
    public static Client GetInstance()
    {
        if (instance == null)
        {
            instance = new Client("ws://oneren.space:6969");
        }

        return instance;
    }
}