namespace deamon;

public class LocalClient
{
    private static Client? instance;
    
    public static Client GetInstance()
    {
        if (instance == null)
        {
            instance = new Client("ws://localhost:6969");
        }

        return instance;
    }
}