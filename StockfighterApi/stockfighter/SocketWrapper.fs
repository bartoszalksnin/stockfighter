module SocketWrapper
  open System
  open System.IO
  open System.Text
  open System.Threading
  open System.Net.WebSockets
  open System.Web.Script.Serialization
  open HttpClient
  open FSharp.Data
  open FSharp.Data.JsonExtensions

  let openSocket wsUri (socket: ClientWebSocket) =
      do socket.ConnectAsync(wsUri, CancellationToken.None) |> ignore

  let encoder = new UTF8Encoding();

  let recieveSocketMessages wsUri (socket: ClientWebSocket) log parse =
    let buffer = Array.create<byte> 4084 0uy
    let task = Async.AwaitTask (socket.ReceiveAsync(ArraySegment<byte>(buffer),CancellationToken.None))
    Async.RunSynchronously task |> ignore

    let bufferString = encoder.GetString(buffer |> Array.filter (fun x -> x <> 0uy))
    if String.length bufferString > 0 then
      try
        let info = JsonValue.Parse(bufferString)
        let ok = info.TryGetProperty("ok")
        match ok with
        | None -> None
        | x -> log (parse(info?quote.ToString())); Some(parse(info?quote.ToString()))
      with
        | e -> printfn "failed with %s" (e.ToString()); None
    else
       openSocket wsUri socket
       printfn "empty message"
       None

  let GetSocketMessage wsUri (socket: ClientWebSocket) log parse =
    printfn "while end wait %s " (socket.State.ToString())
    match socket.State with
    | WebSocketState.Open -> recieveSocketMessages wsUri socket (log) (parse)
    | WebSocketState.Closed -> openSocket wsUri socket; None
    | _ -> None

  let startSocket wsUri log parse =
      let socket = new ClientWebSocket()
      openSocket wsUri socket
      async {
        while true do
          async {
            do! Async.Sleep(16)
          } |> Async.RunSynchronously
          GetSocketMessage wsUri socket |> ignore
      }
