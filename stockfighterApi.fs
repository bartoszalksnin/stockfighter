module stockfighterApi
  open System
  open System.Text
  open System.Web.Script.Serialization
  open HttpClient
  open FSharp.Data

  type Direction =
    | Buy
    | Sell

  type OrderType =
    | Limit
    | Market
    | FillOrKill
    | ImmidiateOrCancel

  let baseUrl = "https://api.stockfighter.io/ob/api"

  let serialise obj =
    (System.Web.Script.Serialization.JavaScriptSerializer()).Serialize(obj)

  let post url bodyObject apiKey =
      let body = serialise bodyObject
      printfn "%s" body
      createRequest Post url
      |> withHeader (Custom {name="X-Starfighter-Authorization"; value=apiKey})
      |> withBody body

  let get url apiKey =
      createRequest Get url
      |> withHeader (Custom {name="X-Starfighter-Authorization"; value=apiKey})

  let delete url apiKey =
    createRequest Delete url
    |> withHeader (Custom {name="X-Starfighter-Authorization"; value=apiKey})

  let isApiUp() =
    let url = baseUrl + "/heartbeat"
    let heartbeatRequest = get url ""
    let response = heartbeatRequest |> getResponseBody
    printfn "%s" (response)
    response

  type Order (account: String, venue: String, symbol: String, price : int, qty: int, direction: Direction, orderType: OrderType) =
      member this.account = account
      member this.venue = venue
      member this.symbol = symbol
      member this.price = price
      member this.qty = qty
      member this.direction =
        match direction with
          | Buy -> "Buy"
          | Sell -> "Sell"
      member this.orderType =
        match orderType with
          | Limit -> "limit"
          | Market -> "market"
          | FillOrKill -> "fill-or-kill"
          | ImmidiateOrCancel -> "immediate-or-cancel"


  type StockFighter(apiKey: String, account: String) =


    member this.makeOrder (order: Order) =
        let makeOrderUrl = baseUrl + "/venues/" + order.venue + "/stocks/" + order.symbol + "/orders"
        let makeOrderRequest = post makeOrderUrl order apiKey
        let response = makeOrderRequest |> getResponseBody
        printfn "%s" (response)
        response

    member this.getQuote (venue: String, stock: String) =
        let getQuoteUrl = baseUrl + "/venues/" + venue + "/stocks/" + stock + "/orders"
        let getQuoteRequest = get getQuoteUrl apiKey
        let response = getQuoteRequest |> getResponseBody
        printfn "%s" (response)
        response

    member this.getOrderStatus (venue: String, stock: String, id: String) =
        let getOrderStatusUrl = baseUrl + "/venues/" + venue + "/stocks/" + stock + "/orders/" + id
        let getOrderStatusRequest = get getOrderStatusUrl apiKey
        let response = getOrderStatusRequest |> getResponseBody
        printfn "%s" (response)
        response

    member this.isVenueUp(venue: String) =
      let venueHeartBeatUrl = baseUrl + "/venues/" + venue + "/heartbeat"
      let venueHeartBeatRequest = get venueHeartBeatUrl apiKey
      let response = venueHeartBeatRequest |> getResponseBody
      printfn "%s" (response)
      response

    member this.listStocks(venue: String) =
      let listStocksUrl = baseUrl + "/venues/" + venue + "/stocks"
      let listStocksRequest = get listStocksUrl apiKey
      let response = listStocksRequest |> getResponseBody
      printfn "%s" (response)
      response

    member this.getOrderBook(venue: String, stock: String) =
      let getOrderBookUrl = baseUrl + "/venues/" + venue + "/stocks/" + stock
      let getOrderBookRequest = get getOrderBookUrl apiKey
      let response = getOrderBookRequest |> getResponseBody
      printfn "%s" (response)
      response

    member this.cancelOrder(venue: String, stock: String, orderId: String) =
      let cancelOrderUrl = baseUrl + "/venues/" + venue + "/stocks/" + stock
      let cancelOrderRequest = delete cancelOrderUrl apiKey
      let response = cancelOrderRequest |> getResponseBody
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
    member this.makeBuyOrder (price: int, quantity: int) =
      let order = Order(venue.api.account, venue.venue, stock, price, quantity, Buy, Limit)
      venue.api.makeOrder order

    member this.getQuote() =
      venue.api.getQuote (venue.venue, stock)

    member this.getOrderStatus (id: String) =
      venue.api.getOrderStatus (venue.venue, stock, id)

    member this.getOrderBook () =
      venue.api.getOrderBook (venue.venue, stock)

    member this.cancelOrder (orderId: String) =
      venue.api.cancelOrder (venue.venue, stock, orderId)

    member this.getAllOrdersForStock() =
      venue.api.getAllOrdersForStock(venue.venue, stock)


    member this.venue = venue
    member this.api = venue.api
    member this.stock = stock
