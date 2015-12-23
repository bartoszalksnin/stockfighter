module StockfighterApi
  open System
  open System.Text
  open System.Web.Script.Serialization
  open HttpClient
  open FSharp.Data
  open StockfigherCommon

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

    member this.getQuote (venue: String, stock: String) =
        //https://api.stockfighter.io/ob/api/venues/:venue/stocks/:stock/quote
        let url = baseUrl + "/venues/" + venue + "/stocks/" + stock + "/quote"
        printf "%s\n" url
        let request = get url apiKey
        let response = request |> getResponseBody
        let responseObject = GetQuoteResponse(response)
        responseObject

    member this.getOrderStatus (venue: String, stock: String, id: String) =
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

    member this.getOrderBook(venue: String, stock: String) =
      let url = baseUrl + "/venues/" + venue + "/stocks/" + stock
      let request = get url apiKey
      let response = request |> getResponseBody
      printfn "%s" (response)
      let responseObject = OrderBook(response)
      responseObject

    member this.cancelOrder(venue: String, stock: String, orderId: String) =
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

    member this.getAllOrdersForStock(venue: String, stock: String) =
      //https://api.stockfighter.io/ob/api/venues/:venue/accounts/:account/stocks/:stock/orders
      let url = baseUrl + "/venues/" + venue + "/accounts/" + account + "/stocks/" + stock + "/orders"
      let request = get url apiKey
      let response = request |> getResponseBody
      printfn "%s" (response)
      response

    member this.apiKey = apiKey
    member this.account = account


  type StockFighterVenue (api: StockFighter, venue: String) =
    member this.getAllOrders() =
      api.getAllOrders(venue)

    member this.isVenueUp () =
      api.isVenueUp (venue)

    member this.listStocks () =
      api.listStocks (venue)

    member this.venue = venue
    member this.api = api

  type StockFighterStock (venue: StockFighterVenue, stock: String) =

    member this.makeOrder (price: int, quantity: int, direction: Direction) =
      let order = Order(venue.api.account, venue.venue, stock, price, quantity, direction, Limit)
      venue.api.makeOrder order

    member this.getQuote() =
      venue.api.getQuote (venue.venue, stock)

    member this.getOrderStatus (id: String) =
      venue.api.getOrderStatus (venue.venue, stock, id)

    member this.getOrderBook () =
      venue.api.getOrderBook (venue.venue, stock)

    member this.cancelOrder (orderId: int) =
      venue.api.cancelOrder (venue.venue, stock, sprintf "%i" orderId)

    member this.getAllOrdersForStock() =
      venue.api.getAllOrdersForStock(venue.venue, stock)


    member this.venue = venue
    member this.api = venue.api
    member this.stock = stock
