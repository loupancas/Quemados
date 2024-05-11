using Fusion;

public class LocalPlayerReference : NetworkBehaviour
{
    public static LocalPlayerReference Instance { get; private set; }
    
    public override void Spawned()
    {
        if (!HasStateAuthority) return;

        Instance = this;
    }
}
