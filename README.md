# stockfighter
Fsharp library for https://www.stockfighter.io API.

Build (on osx)
```fsharp
sh build.sh
```

Start dashboard server (on osx)
```fsharp
sh startDashboard.sh
```

Stop dashboard server (on osx)
```fsharp
sh killDashboard.sh
```

Initalise api

```fsharp
let apiKey = "xxxxxxx"
let venueId = "yyyy"
let stockId = "zzzz"
let account = "mmmmmmmm"

let api = StockFighter(apiKey, account)
let venue = StockFighterVenue(api, venueId)
let stock = StockFighterStock(venue, stockId)

```

Buy 100 shares for 40.00$
```fsharp
stock.makeOrder 4000 100 Direction.Buy
```

Open web socket connection
```fsharp
stock.openSocketConnection()
```

Close web socket connection
```fsharp
stock.closeSocketConnection()
```



Other methods:
```fsharp
venue.isVenueUp()
venue.listStocks()
venue.getAllOrders()

stock.getQuote ()
stock.makeOrder 100 100 Direction.Buy
stock.getOrderStatus "100"
stock.getOrderBook()
stock.cancelOrder "100"
stock.getAllOrdersForStock()
```
