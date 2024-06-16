using PtySharp;

try
{
    Terminal.Run("cmd.exe");
}
catch (InvalidOperationException e)
{
    Console.Error.WriteLine(e.Message);
    throw;
}