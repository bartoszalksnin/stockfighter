# stockfighter
Fsharp library for https://www.stockfighter.io API.

Example usage:
```fsharp
let apiKey = "xxxxxxx"
let venueId = "yyyy"
let stockId = "zzzz"
let account = "mmmmmmmm"

let api = StockFighter(apiKey, account)
let venue = StockFighterVenue(api, venueId)
let stock = StockFighterStock(venue, stockId)

venue.isVenueUp()
venue.listStocks()
venue.getAllOrders()

stock.getQuote ()
stock.getOrderStatus("100")
stock.getOrderBook()
stock.cancelOrder("100")
stock.getAllOrdersForStock()
```
