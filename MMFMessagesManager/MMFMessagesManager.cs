using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Runtime.Remoting.Messaging;

namespace MMFMessagesManagerClass
{
    public class MMFMessagesManager
    {
        MMFManager mmfManager;
        private string AppId = "";
        public MMFMessagesManager(string appId)
        {
            AppId = appId;
            mmfManager = new MMFManager();
        }

        public void SendToProgram(Guid guid, DateTime date, string message, string readerId = "")
        {
            var progMessage = new ProgramMessage();
            if (String.IsNullOrEmpty(readerId))
            {
                progMessage = new ProgramMessage(guid, date, AppId, message);
            }
            else
            {
                progMessage = new ProgramMessage(guid, date, AppId, message, readerId);
            }
            string jsonString = JsonSerializer.Serialize(progMessage);

            if(jsonString.Length * 2 + 6 > MMFManager.oneMessageSize)
            {
                throw new System.ArgumentException("Размер сообщения превышает максимально возможный (примерно 950 символов)");
            }

            var messages = GetAllFromProgramUnsorted();
            if(messages != null)
            {
                // если файл переполнен - перезаписываем самое старое сообщение
                if (MMFManager.size - messages.Count * MMFManager.oneMessageSize < MMFManager.oneMessageSize)
                {
                    var minDateTime = messages.Min(m => m.DateTime);
                    var oldestMessage = messages.FirstOrDefault(m => m.DateTime == minDateTime);

                    int position = 0;

                    if (oldestMessage != null) position = messages.IndexOf(oldestMessage);

                    mmfManager.Rewrite(position * MMFManager.oneMessageSize, jsonString);

                    
                    return;
                }
            }    
            mmfManager.Write(jsonString);

            //mmfManager.Write($"{Guid.NewGuid()};{DateTime.Now};{from};{to};{message}");
        }

        private List<ProgramMessage> GetAllFromProgramUnsorted()
        {
            List<ProgramMessage> messages = new List<ProgramMessage>();
            
            var rawMessages = mmfManager.Read();
            if (rawMessages.Count == 0) return null;
            foreach(string rawMessage in rawMessages)
            {
                try
                {
                    ProgramMessage progMessage = JsonSerializer.Deserialize<ProgramMessage>(rawMessage);
                    messages.Add(progMessage);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            
            return messages;
        }

        public List<ProgramMessage> GetAllFromProgram()
        {
            var messages = GetAllFromProgramUnsorted();
            var messagesSorted = messages.OrderBy(m => m.DateTime).ToList();
            return messagesSorted;
        }

        public List<ProgramMessage> GetUnreadFromProgram()
        {
            List<ProgramMessage> messages = new List<ProgramMessage>();

            var rawMessages = mmfManager.Read();
            if (rawMessages.Count == 0) return null;
            foreach (string rawMessage in rawMessages)
            {
                ProgramMessage progMessage = JsonSerializer.Deserialize<ProgramMessage>(rawMessage);
                if(progMessage.Targets.Where(t=> t.IsRead == false).Select(t=> t.AppId).Contains(AppId)
                    || (progMessage.IsForAll == true && progMessage.Targets.FirstOrDefault(t=> t.AppId == AppId) == null))
                {
                    messages.Add(progMessage);
                    SetReadStatus(AppId, progMessage.Guid, true);
                }
            }
            var messagesSorted = messages.OrderBy(m => m.DateTime).ToList();
            return messagesSorted;
        }

        public void SetReadStatus(string appId, Guid guid, bool isRead)
        {
            var messages = GetAllFromProgramUnsorted();
            var message = messages.FirstOrDefault(m => m.Guid == guid);
            if (message == null) return;
            var index = messages.IndexOf(message);
            if (index == -1) return;

            var target = message.Targets.FirstOrDefault(t => t.AppId.Equals(appId));
            if(target == null)
            {
                var readTarget = new ReadState(appId);
                readTarget.IsRead = isRead;
                message.Targets.Add(readTarget);
            }
            else
            {
                target.IsRead = isRead;
            }
            string jsonString = JsonSerializer.Serialize(message);
            var position = index * MMFManager.oneMessageSize;

            mmfManager.Rewrite(position, jsonString);
        }
    }

}
