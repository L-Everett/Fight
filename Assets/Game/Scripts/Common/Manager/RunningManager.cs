
public class RunningManager
{
    private static readonly object _lock = new object();
    private static RunningManager instance;
    public static RunningManager Instance
    {
        get
        {
            if (instance == null)
            {
                lock (_lock)
                {
                    instance = new RunningManager();
                }
            }
            return instance;
        }
        private set { }
    }

    public string mCurrentCharacter = "C001";
}