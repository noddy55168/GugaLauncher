namespace GugaLauncher;
/// <summary>只操作假宿主，任何案例都不列舉或建立真實遊戲／XXMI。</summary>
internal static class OfflineTests
{
    private static int assertions;
    private static void Assert(bool ok,string text){assertions++;if(!ok)throw new Exception("TEST FAILED: "+text);}
    private sealed class Fake : ICoordinatorHost
    {
        public string Fail="";public int GameChecks,Dispatches,Creates,Prepared,Released;public List<string> Events=[];
        public void RejectExistingGame(){Events.Add("game-check");GameChecks++;if(Fail=="game"||(Fail=="race-before-xxmi"&&GameChecks==2)||(Fail=="race-before-game"&&GameChecks==3))throw new Exception("existing game");}
        public void RejectExistingXxmi(){Events.Add("xxmi-check");if(Fail=="xxmi")throw new Exception("existing xxmi");}
        public void Validate(){Events.Add("validate");if(Fail=="config")throw new Exception("bad config");}
        public IDisposable PrepareOfficial(){Events.Add("prepare");if(Fail=="permissions")throw new Exception("denied");Prepared++;return new Lease(()=>Released++);}
        public void DispatchXxmi(){Events.Add("dispatch");Dispatches++;}
        public void ReceiveCallback(){Events.Add("callback");if(Fail is "timeout" or "bad-nonce" or "xxmi-exit")throw new Exception(Fail);}
        public uint CreateGame(){Events.Add("create");Creates++;return 42;}
    }
    private sealed class Lease(Action close):IDisposable{public void Dispose()=>close();}
    public static void Run()
    {
        foreach(string failure in new[]{"game","xxmi","config","permissions","race-before-xxmi","timeout","bad-nonce","xxmi-exit","race-before-game"})
        {
            var fake=new Fake{Fail=failure};bool rejected=false;
            try{Coordinator.Run(fake);}catch{rejected=true;}
            Assert(rejected,failure+" rejected");Assert(fake.Creates==0,failure+" never creates game");
            if(failure is "game" or "xxmi" or "config" or "permissions" or "race-before-xxmi")Assert(fake.Dispatches==0,failure+" never dispatches XXMI");
            Assert(fake.Prepared==fake.Released,failure+" lease released without termination");
        }
        var good=new Fake();Assert(Coordinator.Run(good)==42,"success result");
        Assert(good.Dispatches==1&&good.Creates==1&&good.Released==1,"single dispatch and creation");
        Assert(good.Events.SequenceEqual(new[]{"game-check","xxmi-check","validate","prepare","game-check","xxmi-check","dispatch","callback","game-check","create"}),"early checks order");
        Assert(GameCreation.Quote("a b")=="\"a b\"","space quoting");
        Assert(GameCreation.Quote("x\\")=="\"x\\\\\"","trailing slash quoting");
        Assert(Settings.BridgeCommand("C:\\A B\\GugaLauncher.exe")=="\"C:\\A B\\GugaLauncher.exe\" bridge","custom command quoting");
        foreach(string text in new[]{"C:\\%TEMP%\\GugaLauncher.exe","C:\\bad&path\\GugaLauncher.exe"})
        {bool bad=false;try{Settings.BridgeCommand(text);}catch(InvalidDataException){bad=true;}Assert(bad,"shell expansions rejected");}
        string? pipe=Environment.GetEnvironmentVariable("GUGA_COORD_PIPE"),nonce=Environment.GetEnvironmentVariable("GUGA_COORD_NONCE");
        try{Environment.SetEnvironmentVariable("GUGA_COORD_PIPE",null);Environment.SetEnvironmentVariable("GUGA_COORD_NONCE",null);bool bad=false;try{WindowsHost.Bridge();}catch(InvalidOperationException){bad=true;}Assert(bad,"standalone bridge rejected before native calls");}
        finally{Environment.SetEnvironmentVariable("GUGA_COORD_PIPE",pipe);Environment.SetEnvironmentVariable("GUGA_COORD_NONCE",nonce);}
        int attempts=0;var output=new StringWriter();
        Program.ReviewSetup(new StringReader("\n\n"),output,()=>{if(++attempts==1)throw new InvalidDataException("fixture invalid config");});
        Assert(attempts==2,"setup retries validation");
        Assert(output.ToString().Contains("尚未完成")&&output.ToString().Contains("OK，"),"setup shows failure then success");
        bool cancelled=false;
        try{Program.ReviewSetup(new StringReader(""),new StringWriter(),()=>throw new Exception("must not validate"));}
        catch(OperationCanceledException){cancelled=true;}
        Assert(cancelled,"setup EOF cancels without validation");
        Console.WriteLine($"PASS: {assertions} offline assertions; no game/XXMI launch, injection, or termination.");
    }
}
