namespace GugaLauncher;
/// <summary>副作用邊界；離線驗證以假宿主取代，不接觸程序或原安裝。</summary>
internal interface ICoordinatorHost
{
    void RejectExistingGame();
    void RejectExistingXxmi();
    void Validate();
    IDisposable PrepareOfficial();
    void DispatchXxmi();
    void ReceiveCallback();
    uint CreateGame();
}
/// <summary>最前段及派送前重查既有遊戲；回呼不會再次執行 XXMI。</summary>
internal static class Coordinator
{
    public static uint Run(ICoordinatorHost host)
    {
        host.RejectExistingGame();host.RejectExistingXxmi();host.Validate();
        using var parent=host.PrepareOfficial();
        host.RejectExistingGame();host.RejectExistingXxmi();
        host.DispatchXxmi();
        host.ReceiveCallback();
        host.RejectExistingGame();
        return host.CreateGame();
    }
}
