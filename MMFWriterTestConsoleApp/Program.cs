using MMFMessagesManagerClass;

namespace MMFWriterTestConsoleApp 
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var mmfMessagesManager = new MMFMessagesManager("this_app#2");

            var writeTask = Task.Run(() =>
            {
                while (true)
                {
                    Console.Write($"> ");
                    var message = Console.ReadLine();
                    //protocol.SendToProgram("MMFWriterTextConsoleApp.exe","OtherProgram.exe", message);
                    
                    // можно направлять сообщения конкретному приложению
                    //mmfMessagesManager.SendToProgram(Guid.NewGuid(), DateTime.Now, message, "this_app#1");
                    // или всем сразу
                    mmfMessagesManager.SendToProgram(Guid.NewGuid(), DateTime.Now, message, "this_app#1");
                    Console.WriteLine($"Message sent");

                }
            });

            var readTask = Task.Run(() =>
            {
                var oldMessage = new ProgramMessage();
                while (true)
                {
                    var messages = mmfMessagesManager.GetUnreadFromProgram();
                    
                    if (messages != null)
                    {
                        if(messages.Count != 0)
                        {
                            foreach(var message in messages)
                            {
                                Console.WriteLine(message);
                            }
                            //var message = messages[messages.Count - 1];
                            //if (message.Guid != oldMessage.Guid)
                            //{
                            //    Console.WriteLine(message);
                            //    oldMessage = message;
                            //}
                        }
                        
                    }
                    Thread.Sleep(1000);

                }

            });

            await Task.WhenAll(writeTask, readTask);
        }

        
    }
}



