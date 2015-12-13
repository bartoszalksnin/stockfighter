# stockfighter
Fsharp library fot stockfighter API.

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

stock.makeBuyOrder (10,10)
stock.getQuote ()
stock.getOrderStatus("100")
stock.getOrderBook()
stock.cancelOrder("100")
stock.getAllOrdersForStock()
```
