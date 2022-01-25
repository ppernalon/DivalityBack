using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DivalityBack.Services
{
    [ExcludeFromCodeCoverage]
    public class DuelWebsocketService
    {
        private readonly UsersService _usersService;

        public DuelWebsocketService(UsersService usersService)
        {
            _usersService = usersService;
        }
        public async Task HandleMessages(WebSocket websocket)
        {
            try
            {
                using (var ms = new MemoryStream()) {
                    while (websocket.State == WebSocketState.Open) {
                        WebSocketReceiveResult result;
                        do {
                            var messageBuffer = WebSocket.CreateClientBuffer(1024, 16);
                            result = await websocket.ReceiveAsync(messageBuffer, CancellationToken.None);
                            ms.Write(messageBuffer.Array, messageBuffer.Offset, result.Count);
                        }
                        while (!result.EndOfMessage);

                        if (result.MessageType == WebSocketMessageType.Text) {
                            JsonDocument msgJson = JsonDocument.Parse(ms.ToArray());
                        }
                        
                        /*
                         handle messages here
                         */
                        
                        ms.SetLength(0);
                        ms.Seek(0, SeekOrigin.Begin);
                        ms.Position = 0;
                    }
                }

            }
            catch(InvalidOperationException e) 
            {
                Console.Write("ERREUR WS: " + e.Message);
                
            }
        }
    }
}