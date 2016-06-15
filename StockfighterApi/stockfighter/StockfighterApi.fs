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
  open SocketWrapper

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
    let ms = sprintf "%i %i %i" orderStatus.bid orderStatus.ask orderStatus.last
    printfn "%s" ms
    stream.WriteLine(ms)
    stream.Flush()
    stream.Close()


  let isApiUp() =
    let url = baseUrl + "/heartbeat"
    let request = get url ""
    let response = request |> getResponseBody
    printfn "%s" (response)
    response

  type StockFighter(apiKey: String, account: String) =
    member this.makeOrder (order: Order) = async {
      let url = baseUrl + "/venues/" + order.venue + "/stocks/" + order.symbol + "/orders"
      let request = post url order apiKey
      let! response = request |> getResponseBodyAsync
      printfn "new response is this %s" (response)
      let responseObject = OrderStatus(response)
      return responseObject
    }

    member this.getQuote (venue: String) (stock: String) = async {
      //https://api.stockfighter.io/ob/api/venues/:venue/stocks/:stock/quote
      let url = baseUrl + "/venues/" + venue + "/stocks/" + stock + "/quote"
      printf "%s\n" url
      let request = get url apiKey
      let! response = request |> getResponseBodyAsync
      let responseObject = GetQuoteResponse(response)
      return responseObject
    }

    member this.getOrderStatus (venue: String) (stock: String) (id: String) = async {
      let url = baseUrl + "/venues/" + venue + "/stocks/" + stock + "/orders/" + id
      let request = get url apiKey
      let! response = request |> getResponseBodyAsync
      printfn "%s" (response)
      let responseObject = OrderStatus(response)
      return responseObject
    }

    member this.isVenueUp(venue: String) = async {
      let url = baseUrl + "/venues/" + venue + "/heartbeat"
      let request = get url apiKey
      let! response = request |> getResponseBodyAsync
      printfn "%s" (response)
      return response
    }

    member this.listStocks(venue: String) = async {
      let url = baseUrl + "/venues/" + venue + "/stocks"
      let request = get url apiKey
      let! response = request |> getResponseBodyAsync
      printfn "%s" (response)
      return response
    }


    member this.getOrderBook(venue: String) (stock: String) = async {
      let url = baseUrl + "/venues/" + venue + "/stocks/" + stock
      let request = get url apiKey
      let! response = request |> getResponseBodyAsync
      printfn "%s" (response)
      let responseObject = OrderBook(response)
      logOrderBook responseObject
      return responseObject
    }

    member this.cancelOrder(venue: String) (stock: String) (orderId: String) = async {
      //https://api.stockfighter.io/ob/api/venues/:venue/stocks/:stock/orders/:order
      let url = baseUrl + "/venues/" + venue + "/stocks/" + stock + "/orders/" + orderId
      printfn "%s\n" url
      let request = delete url apiKey
      let! response = request |> getResponseBodyAsync
      printfn "%s" (response)
      return response
    }


    member this.getAllOrders(venue: String) = async {
      let url = baseUrl + "/venues/" + venue + "/accounts/" + account + "/orders"
      let request = get url apiKey
      let! response = request |> getResponseBodyAsync
      printfn "%s" (response)
      return response
    }


    member this.getAllOrdersForStock(venue: String) (stock: String) = async {
      //https://api.stockfighter.io/ob/api/venues/:venue/accounts/:account/stocks/:stock/orders
      let url = baseUrl + "/venues/" + venue + "/accounts/" + account + "/stocks/" + stock + "/orders"
      let request = get url apiKey
      let! response = request |> getResponseBodyAsync
      printfn "%s" (response)
      return response
    }


    member this.apiKey = apiKey
    member this.account = account


  type StockFighterVenue (api: StockFighter, venue: String) =
    member this.getAllOrders () =
      this.getAllOrdersAsync () |> Async.RunSynchronously

    member this.getAllOrdersAsync () =
      api.getAllOrders venue

    member this.isVenueUp () =
      this.isVenueUpAsync () |> Async.RunSynchronously

    member this.isVenueUpAsync () =
      api.isVenueUp venue

    member this.listStocks () =
      this.listStocksAsync () |> Async.RunSynchronously

    member this.listStocksAsync () =
      api.listStocks venue

    member this.venue = venue
    member this.api = api

  type StockFighterStock (venue: StockFighterVenue, stock: String) =

    member this.makeOrder (price: int) (quantity: int) (direction: Direction) =
      this.makeOrderAsync price quantity direction |> Async.RunSynchronously

    member this.makeOrderAsync (price: int) (quantity: int) (direction: Direction) =
      let order = Order(venue.api.account, venue.venue, stock, price, quantity, direction, Limit)
      venue.api.makeOrder order

    member this.getQuote () =
      this.getQuoteAsync () |> Async.RunSynchronously

    member this.getQuoteAsync () =
      venue.api.getQuote venue.venue stock

    member this.getOrderStatus (id: String) =
      this.getOrderStatusAsync id |> Async.RunSynchronously

    member this.getOrderStatusAsync (id: String) =
      venue.api.getOrderStatus venue.venue stock id

    member this.getOrderBook () =
      this.getOrderBookAsync () |> Async.RunSynchronously

    member this.getOrderBookAsync () =
      venue.api.getOrderBook venue.venue stock

    member this.cancelOrder (orderId: int) =
      this.cancelOrderAsync orderId |> Async.RunSynchronously

    member this.cancelOrderAsync (orderId: int) =
      venue.api.cancelOrder venue.venue stock (sprintf "%i" orderId)

    member this.getAllOrdersForStock() =
      this.getAllOrdersForStockAsync () |> Async.RunSynchronously

    member this.getAllOrdersForStockAsync() =
      venue.api.getAllOrdersForStock venue.venue stock

    member this.getSockertUrl() =
      Uri("wss://api.stockfighter.io/ob/api/ws/" + venue.api.account + "/venues/" + venue.venue + "/tickertape/stocks/" + stock)

    member this.openSocketConnection() =
      let wsUri = this.getSockertUrl()
      this.token <- new CancellationTokenSource()
      Async.Start ((startSocket wsUri logQuote GetQuoteResponse), this.token.Token)
      ()

    member this.closeSocketConnection() =
      this.token.Cancel()

    member val token : CancellationTokenSource = new CancellationTokenSource() with get,set
    member this.venue = venue
    member this.api = venue.api
    member this.stock = stock
