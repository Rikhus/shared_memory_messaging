using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MMFMessagesManagerClass
{
    public class MMFManager
    {
        /*        
        символы, которые обозначают начало и конец записи в памяти - 
        <<  и  >>
        */

        const int megabyte = 1048576;
        public const int size = megabyte * 2;
        //public const int size = 4092 * 2;
        public const int oneMessageSize = 2048;  //2 kilobytes
        public const int reservedBytesBeforeData = 6;

        MemoryMappedFile sharedMemory;


        public MMFManager()
        {
            sharedMemory = MemoryMappedFile.CreateOrOpen("MemoryFile", size);            
        }

        public void Write(string message)
        {
            //Clear();
            var newMessageAddress = GetAddressForNewMessage();
            using (MemoryMappedViewAccessor writer = sharedMemory.CreateViewAccessor(0, size))
            {
                writer.Write(newMessageAddress, '<');
                writer.Write(newMessageAddress + 1, '<');
                writer.Write(newMessageAddress + 2, message.Length * 2);
                //запись сообщения с четвертого байта в разделяемой памяти
                writer.WriteArray<char>(newMessageAddress + 6, message.ToCharArray(), 0, message.Length);
            }
        }

        public void Rewrite(int position, string message)
        {
            using (MemoryMappedViewAccessor writer = sharedMemory.CreateViewAccessor(0, size))
            {
                writer.Write(position, '<');
                writer.Write(position + 1, '<');
                writer.Write(position + 2, message.Length * 2);
                //запись сообщения с четвертого байта в разделяемой памяти
                writer.WriteArray<char>(position + 6, message.ToCharArray(), 0, message.Length);
            }
        }

        public List<string> Read()
        {
            List<string> messages = new List<string>();

            var msgChars = new char[size];
            using (MemoryMappedViewAccessor reader = sharedMemory.CreateViewAccessor(0, size, MemoryMappedFileAccess.Read))
            {
                //Массив символов сообщения
                var offset = 0;                

                while (true)
                {
                    var startSymbol1 = (char)reader.ReadByte(offset);
                    var startSymbol2 = (char)reader.ReadByte(offset+1);
                    if (startSymbol1 != '<' || startSymbol2 != '<') break;
                    var messageSize = reader.ReadInt32(offset + 2);
                    if (messageSize == 0) break;
                    reader.ReadArray<char>(offset + 6, msgChars, 0, messageSize / 2);
                    var message = new string(msgChars).Substring(0, messageSize / 2);
                    messages.Add(message);
                    msgChars = new char[size];
                    offset += oneMessageSize;
                }                
            }

            return messages;
        }

        private int GetAddressForNewMessage()
        {
            int newMessageAddress = 0;

            using (MemoryMappedViewAccessor reader = sharedMemory.CreateViewAccessor(0, size, MemoryMappedFileAccess.Read))
            {
                //Массив символов сообщения
                var offset = 0;

                while (true)
                {
                    var startSymbol1 = (char)reader.ReadByte(offset);
                    var startSymbol2 = (char)reader.ReadByte(offset + 1);
                    if (startSymbol1 != '<' || startSymbol2 != '<') break;
                    
                    offset += oneMessageSize;
                }
                newMessageAddress = offset;

            }                    
            return newMessageAddress;
        }

        public void Clear()
        {
            //var prevMessageSize = GetMessageSize();
            //if (prevMessageSize == 0) return;
            //using (MemoryMappedViewAccessor writer = sharedMemory.CreateViewAccessor(0, size + 4))
            //{
            //    char[] message = new char[prevMessageSize];
            //    //запись сообщения с четвертого байта в разделяемой памяти
            //    writer.WriteArray<char>(4, message, 0, message.Length);                
            //}
        }
    }
}
