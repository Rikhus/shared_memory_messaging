using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMFMessagesManagerClass
{    
    public class ProgramMessage
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
        public DateTime DateTime { get; set; } = DateTime.Now;
        public string FromProgram { get; set; }
        public string Message { get; set; }
        public List<ReadState> Targets { get; set; } = new List<ReadState>();
        public bool IsForAll { get; set; } = false;

        public ProgramMessage(Guid guid, DateTime date, string fromProgram, string message, List<string> readerIds)
        {
            Guid = guid;
            DateTime= date;
            FromProgram = fromProgram;
            Message = message;
            foreach(string readerId in readerIds)
            {
                Targets.Add(new ReadState(readerId));
            }
        }

        public ProgramMessage(Guid guid, DateTime date, string fromProgram, string message, string readerId)
        {
            Guid = guid;
            DateTime = date;
            FromProgram = fromProgram;
            Message = message;
            Targets.Add(new ReadState(readerId));
        }

        public ProgramMessage(Guid guid, DateTime date, string fromProgram, string message)
        {
            Guid = guid;
            DateTime = date;
            FromProgram = fromProgram;
            Message = message;
            IsForAll = true;
        }

        public ProgramMessage() { }

        public override string ToString()
        {
            var targets = String.Join(", ", Targets.Select((x) => x.AppId));
            return $"[{DateTime}] New message from {FromProgram} to {(IsForAll ? "all" : targets)}: {Message}";
        }
    }

    public class ReadState
    {
        public string AppId { get; private set; }
        public bool IsRead { get; set; }

        public ReadState(string appId)
        {
            AppId = appId;
        }
    }
}
