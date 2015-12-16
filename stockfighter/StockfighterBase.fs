module StockfigherCommon
  open System
  open System.Text
  open System.Web.Script.Serialization
  open HttpClient
  open FSharp.Data
  open FSharp.Data.JsonExtensions

  type Direction =
    | Buy
    | Sell

  type OrderType =
    | Limit
    | Market
    | FillOrKill
    | ImmidiateOrCancel

  type Fills = {
      price: int;
      qty: int;
      ts: String
  }



  type GetQuoteResponse(response : String) =
    let info = JsonValue.Parse(response)
    let askValue =
      let askvalue = info.TryGetProperty("ask")
      match askvalue with
        | None -> 0
        | _ -> info?ask.AsInteger()

    member this.ok: bool = info?ok.AsBoolean()
    member this.symbol: String = info?symbol.AsString()
    member this.venue: String = info?venue.AsString()
    member this.bid: int = info?bid.AsInteger()
    member this.ask: int = askValue
    member this.bidSize: int = info?bidSize.AsInteger()
    member this.askSize: int = info?askSize.AsInteger()
    member this.bidDepth: int = info?bidDepth.AsInteger()
    member this.askDepth: int = info?askDepth.AsInteger()
    member this.last: int = info?last.AsInteger()
    member this.lastSize: int = info?lastSize.AsInteger()
    member this.lastTrade: String = info?lastSize.AsString()
    member this.quoteTime: String = info?quoteTime.AsString()

  type MakeOrderResponse(response : String) =
    let info = JsonValue.Parse(response)
    member this.ok: bool = info?ok.AsBoolean()
    member this.symbol: String = info?symbol.AsString()
    member this.venue: String = info?venue.AsString()
    member this.direction: String = info?direction.AsString()
    member this.originalQty: int = info?originalQty.AsInteger()
    member this.qty: int = info?qty.AsInteger()
    member this.price: int = info?price.AsInteger()
    member this.orderType: String = info?orderType.AsString()
    member this.id: int = info?id.AsInteger()
    member this.account: String = info?account.AsString()
    member this.ts: String = info?ts.AsString()
    //member this.fills: List<Fills> = info?fills
    member this.totalFilled: int = info?totalFilled.AsInteger()
    member this.isOpen: bool = info?opens.AsBoolean()


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

  let baseUrl = "https://api.stockfighter.io/ob/api"
  let deserializer = (System.Web.Script.Serialization.JavaScriptSerializer())

  let serialise obj =
    (System.Web.Script.Serialization.JavaScriptSerializer()).Serialize(obj)

  let post url bodyObject apiKey =
      let body = serialise bodyObject
      createRequest Post url
      |> withHeader (Custom {name="X-Starfighter-Authorization"; value=apiKey})
      |> withBody body

  let get url apiKey =
      createRequest Get url
      |> withHeader (Custom {name="X-Starfighter-Authorization"; value=apiKey})

  let delete url apiKey =
    createRequest Delete url
    |> withHeader (Custom {name="X-Starfighter-Authorization"; value=apiKey})
