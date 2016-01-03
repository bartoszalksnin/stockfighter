module StockfighterApi
  open System
  open System.IO
  open System.Text
  open System.Threading
  open System.Net.WebSockets
  open System.Web.Script.Serialization
  open HttpClient
  open FSharp.Data
  open FSharp.Data.JsonExtensions
  open StockfigherCommon

  let logOrderBook (orderBook: OrderBook) =
    let bid =
      match orderBook.bids with
      | [||] -> 0
      | x ->
        x
        |> Array.map (fun x -> x.price)
        |> Array.max

    let ask =
      match orderBook.asks with
      | [||] -> 0
      | x ->
        x
        |> Array.map (fun x -> x.price)
        |> Array.min
    let stream = new StreamWriter("orderBookData.txt", true)
    stream.WriteLine(sprintf "%i %i" bid ask )
    stream.Flush()
    stream.Close()

  let logQuote (orderStatus: GetQuoteResponse) =
    let stream = new StreamWriter("quoteData.txt", true)
    stream.WriteLine(sprintf "%i %i %i" orderStatus.bid orderStatus.ask orderStatus.last )
    stream.Flush()
    stream.Close()


  let socket = new ClientWebSocket()
  let openSocket wsUri =
      do socket.ConnectAsync(wsUri, CancellationToken.None) |> ignore

  let encoder = new UTF8Encoding();

  let sendPong() =
    let bytes = "pong"B
    let task = Async.AwaitTask (socket.SendAsync(ArraySegment<byte>(bytes),WebSocketMessageType.Text, false, CancellationToken.None))
    let result =
      try
        Some(Async.RunSynchronously task)
      with
        | _ -> None
    printfn "send result %A " (result)
    ()


  let recieveSocketMessages wsUri =
    let buffer = Array.create<byte> 4084 0uy
    let task = Async.AwaitTask (socket.ReceiveAsync(ArraySegment<byte>(buffer),CancellationToken.None))
    let result =
      try
        Some(Async.RunSynchronously task)
      with
        | _ -> None

    let bufferString = encoder.GetString(buffer |> Array.filter (fun x -> x <> 0uy))
    printfn "message %s " bufferString
    if String.length bufferString > 0 then
      try
        let info = JsonValue.Parse(bufferString)
        let ok = info.TryGetProperty("ok")
        match ok with
        | None -> ()
        | _ -> logQuote (GetQuoteResponse(info?quote.ToString()));
      with
        | e -> printfn "failed with %s" (e.ToString())
    else
       openSocket wsUri
       printfn "empty message"

    ()

  let startSocket wsUri  =
      openSocket wsUri
      printfn "%s" (socket.State.ToString())
      async {
        while true do
          async {
            do! Async.Sleep(16)
          } |> Async.RunSynchronously
          printfn "while end wait %s " (socket.State.ToString())
          match socket.State with
          | WebSocketState.Open -> recieveSocketMessages wsUri
          | WebSocketState.Closed -> openSocket wsUri
          | _ -> ()
        ()
      }

  let isApiUp() =
    let url = baseUrl + "/heartbeat"
    let request = get url ""
    let response = request |> getResponseBody
    printfn "%s" (response)
    response

  type StockFighter(apiKey: String, account: String) =
    member this.makeOrder (order: Order) =
        let url = baseUrl + "/venues/" + order.venue + "/stocks/" + order.symbol + "/orders"
        let request = post url order apiKey
        let response = request |> getResponseBody
        printfn "new response is this %s" (response)
        let responseObject = OrderStatus(response)
        responseObject

    member this.getQuote (venue: String) (stock: String) =
        //https://api.stockfighter.io/ob/api/venues/:venue/stocks/:stock/quote
        let url = baseUrl + "/venues/" + venue + "/stocks/" + stock + "/quote"
        printf "%s\n" url
        let request = get url apiKey
        let response = request |> getResponseBody
        let responseObject = GetQuoteResponse(response)
        responseObject

    member this.getOrderStatus (venue: String) (stock: String) (id: String) =
        let url = baseUrl + "/venues/" + venue + "/stocks/" + stock + "/orders/" + id
        let request = get url apiKey
        let response = request |> getResponseBody
        printfn "%s" (response)
        let responseObject = OrderStatus(response)
        responseObject

    member this.isVenueUp(venue: String) =
      let url = baseUrl + "/venues/" + venue + "/heartbeat"
      let request = get url apiKey
      let response = request |> getResponseBody
      printfn "%s" (response)
      response

    member this.listStocks(venue: String) =
      let url = baseUrl + "/venues/" + venue + "/stocks"
      let request = get url apiKey
      let response = request |> getResponseBody
      printfn "%s" (response)
      response

    member this.getOrderBook(venue: String) (stock: String) =
      let url = baseUrl + "/venues/" + venue + "/stocks/" + stock
      let request = get url apiKey
      let response = request |> getResponseBody
      printfn "%s" (response)
      let responseObject = OrderBook(response)
      logOrderBook responseObject
      responseObject

    member this.cancelOrder(venue: String) (stock: String) (orderId: String) =
      //https://api.stockfighter.io/ob/api/venues/:venue/stocks/:stock/orders/:order
      let url = baseUrl + "/venues/" + venue + "/stocks/" + stock + "/orders/" + orderId
      printfn "%s\n" url
      let request = delete url apiKey
      let response = request |> getResponseBody
      printfn "%s" (response)
      response

    member this.getAllOrders(venue: String) =
      let url = baseUrl + "/venues/" + venue + "/accounts/" + account + "/orders"
      let request = get url apiKey
      let response = request |> getResponseBody
      printfn "%s" (response)
      response

    member this.getAllOrdersForStock(venue: String) (stock: String) =
      //https://api.stockfighter.io/ob/api/venues/:venue/accounts/:account/stocks/:stock/orders
      let url = baseUrl + "/venues/" + venue + "/accounts/" + account + "/stocks/" + stock + "/orders"
      let request = get url apiKey
      let response = request |> getResponseBody
      printfn "%s" (response)
      response

    member this.apiKey = apiKey
    member this.account = account


  type StockFighterVenue (api: StockFighter, venue: String) =
    member this.getAllOrders () =
      api.getAllOrders venue

    member this.isVenueUp () =
      api.isVenueUp venue

    member this.listStocks () =
      api.listStocks venue

    member this.venue = venue
    member this.api = api

  type StockFighterStock (venue: StockFighterVenue, stock: String) =

    member this.makeOrder (price: int) (quantity: int) (direction: Direction) =
      let order = Order(venue.api.account, venue.venue, stock, price, quantity, direction, Limit)
      venue.api.makeOrder order

    member this.getQuote () =
      venue.api.getQuote venue.venue stock

    member this.getOrderStatus (id: String) =
      venue.api.getOrderStatus venue.venue stock id

    member this.getOrderBook () =
      venue.api.getOrderBook venue.venue stock

    member this.cancelOrder (orderId: int) =
      venue.api.cancelOrder venue.venue stock (sprintf "%i" orderId)

    member this.getAllOrdersForStock() =
      venue.api.getAllOrdersForStock venue.venue stock

    member this.openSocketConnection() =
      let wsUri = Uri("wss://api.stockfighter.io/ob/api/ws/" + venue.api.account + "/venues/" + venue.venue + "/tickertape/stocks/" + stock)
      this.token <- new CancellationTokenSource()
      Async.Start ((startSocket wsUri), this.token.Token)
      ()

    member this.closeSocketConnection() =
      this.token.Cancel()

    member val token : CancellationTokenSource = new CancellationTokenSource() with get,set
    member this.venue = venue
    member this.api = venue.api
    member this.stock = stock