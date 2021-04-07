using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AiTest
{
    public static class ClientWebsocketHelper
    {
        private static ArrayPool<byte> arrayPool = ArrayPool<byte>.Shared;
        public static async Task SendTextAsync(this ClientWebSocket client, string msg, CancellationToken token)
        {
            var bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg));
            await client.SendAsync(bytesToSend, WebSocketMessageType.Text, true, token);

        }

        public static async Task<string> ReceiveMessageAsync(this ClientWebSocket client, CancellationToken token)
        {
            if (client.State != WebSocketState.Open)
            {
                return null;
            }

            using (var ms = new MemoryStream())
            {
                WebSocketReceiveResult received;
                do
                {
                    byte[] buffer = arrayPool.Rent(1024);
                    try
                    {
                        var receiveBuffer = new ArraySegment<byte>(buffer);
                        received = await client.ReceiveAsync(receiveBuffer, token).ConfigureAwait(false);
                        if (received.MessageType == WebSocketMessageType.Close)
                        {
                            throw new WebSocketException("WebSocket already closed!");
                            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                            return null;
                        }

                        ms.Write(receiveBuffer.Array, receiveBuffer.Offset, received.Count);
                    }
                    finally
                    {
                        arrayPool.Return(buffer);
                    }
       
  
                } while (!received.EndOfMessage && client.State == WebSocketState.Open);

                
                string msg = Encoding.UTF8.GetString(ms.ToArray());

                return msg;
            }
        }

        public static async Task<string> ReceiveTextAsync(this ClientWebSocket client, CancellationToken token)
        {

            string msg = await client.ReceiveMessageAsync(token);

            return msg;


            //try
            //{


            //WebSocketReceiveResult result;
            //do
            //{
            //    byte[] buffer = arrayPool.Rent(1024);
            //    var bytesReceived = new ArraySegment<byte>(buffer); //todo: use object pool

            //    result = await client.ReceiveAsync(bytesReceived, token);
            //} 
            //while (!result.EndOfMessage);

            // var result = await client.ReceiveAsync(bytesReceived, token);

            //string msg = await client.ReceiveMessageAsync(token);
            //token.ThrowIfCancellationRequested();

            //if (result.MessageType == WebSocketMessageType.Close)
            //{
            //    await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            //    return null;
            //}

            //if (result.MessageType != WebSocketMessageType.Text)
            //{
            //    throw new NotImplementedException();
            //}


            // string msg = Encoding.UTF8.GetString(bytesReceived.Array, 0, result.Count);

            //return (msg, result.EndOfMessage);
            //}
            //finally
            //{
            //    arrayPool.Return(buffer);
            //}
        }
    }
}