using MainGameServer;

try
{
    Consumer.StartChat();    
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}


Console.ReadKey();
